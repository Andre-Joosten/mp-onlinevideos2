﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Dialogs;
using System.Threading;

namespace OnlineVideos.MediaPortal1
{
	public partial class GUIOnlineVideos
	{
		bool firstLoadDone = false;
		BackgroundWorker initializationBackgroundWorker;
		public LatestVideosManager LatestVideosManager { get; protected set; }

		void StartBackgroundInitialization()
		{
			initializationBackgroundWorker = new BackgroundWorker();
			initializationBackgroundWorker.WorkerReportsProgress = true;
			initializationBackgroundWorker.WorkerSupportsCancellation = false;
			initializationBackgroundWorker.DoWork += DoInitialization;
			initializationBackgroundWorker.RunWorkerAsync();
		}

		void DoInitialization(object sender, DoWorkEventArgs e)
		{
			try
			{
				if (string.IsNullOrEmpty(Thread.CurrentThread.Name)) Thread.CurrentThread.Name = "OVInit";

				// Get the BackgroundWorker that raised this event.
				BackgroundWorker worker = sender as BackgroundWorker;

				// clear cache files that might be left over from an application crash
				MPUrlSourceFilter.MPUrlSourceFilterDownloader.ClearDownloadCache();

				// Load localized strings and set them to Translation class and GuiProperties
				Translator.TranslateSkin();

				// The default connection limit is 2 in .Net on most platforms! This means downloading two files will block all other WebRequests.
				System.Net.ServicePointManager.DefaultConnectionLimit = 100;

				// The default .Net implementation for URI parsing removes trailing dots, which is not correct
				Utils.FixUriTrailingDots();

				// When AutoUpdate of sites on startup is enabled and the last update was done earlier than configured run AutoUpdate and deletion of old thumbs
				if (PluginConfiguration.Instance.updateOnStart == true &&
					PluginConfiguration.Instance.lastFirstRun.AddHours(PluginConfiguration.Instance.updatePeriod) > DateTime.Now)
				{
					OnlineVideos.Sites.Updater.UpdateSites();
					ImageDownloader.DeleteOldThumbs(PluginConfiguration.Instance.ThumbsAge, r => { return true; });
					PluginConfiguration.Instance.lastFirstRun = DateTime.Now;
				}

				// instantiates and initializes all siteutils
				OnlineVideoSettings.Instance.BuildSiteUtilsList();
                PluginConfiguration.Instance.BuildAutomaticSitesGroups();

				LatestVideosManager = new LatestVideosManager();
				LatestVideosManager.Start();
			}
			catch (Exception ex)
			{
				Log.Instance.Error("Error during initialization: {0}", ex.ToString());
			}
		}

		void DoFirstLoad()
		{
			// replace g_player's ShowFullScreenWindowVideo
			g_Player.ShowFullScreenWindowVideo = ShowFullScreenWindowHandler;
			g_Player.PlayBackEnded += new g_Player.EndedHandler(g_Player_PlayBackEnded);
			g_Player.PlayBackStopped += new g_Player.StoppedHandler(g_Player_PlayBackStopped);
			// attach to global action event, to handle next and previous for playlist playback
			GUIWindowManager.OnNewAction += new OnActionHandler(this.GUIWindowManager_OnNewAction);
			if (GroupsEnabled) CurrentState = State.groups;
			firstLoadDone = true;
			DoSubsequentLoad();
		}

		void DoSubsequentLoad()
		{
			if (PreviousWindowId != 4758 &&
				PluginConfiguration.Instance.updateOnStart != false &&
				PluginConfiguration.Instance.lastFirstRun.AddHours(PluginConfiguration.Instance.updatePeriod) < DateTime.Now)
			{
				bool? doUpdate = PluginConfiguration.Instance.updateOnStart;
				if (!PluginConfiguration.Instance.updateOnStart.HasValue && !preventDialogOnLoad)
				{
					GUIDialogYesNo dlg = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
					if (dlg != null)
					{
						dlg.Reset();
						dlg.SetHeading(PluginConfiguration.Instance.BasicHomeScreenName);
						dlg.SetLine(1, Translation.Instance.PerformAutomaticUpdate);
						dlg.SetLine(2, Translation.Instance.UpdateAllYourSites);
						dlg.DoModal(GUIWindowManager.ActiveWindow);
						doUpdate = dlg.IsConfirmed;
					}
				}
				PluginConfiguration.Instance.lastFirstRun = DateTime.Now;
				if (doUpdate == true || PluginConfiguration.Instance.ThumbsAge >= 0)
				{
					GUIDialogProgress dlgPrgrs = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);
					if (dlgPrgrs != null)
					{
						dlgPrgrs.Reset();
						dlgPrgrs.DisplayProgressBar = true;
						dlgPrgrs.ShowWaitCursor = false;
						dlgPrgrs.DisableCancel(true);
						dlgPrgrs.SetHeading(PluginConfiguration.Instance.BasicHomeScreenName);
						dlgPrgrs.StartModal(GUIWindowManager.ActiveWindow);
					}
					else
					{
						GUIWaitCursor.Init(); GUIWaitCursor.Show();
					}
					GUISiteUpdater guiUpdater = (GUISiteUpdater)GUIWindowManager.GetWindow(GUISiteUpdater.WindowId);
					new System.Threading.Thread(delegate()
					{
						if (doUpdate == true && guiUpdater != null)
						{
							if (dlgPrgrs != null) dlgPrgrs.SetHeading(string.Format("{0} - {1}", PluginConfiguration.Instance.BasicHomeScreenName, Translation.Instance.AutomaticUpdate));
							bool? updateResult = OnlineVideos.Sites.Updater.UpdateSites((m, p) =>
								{
									if (dlgPrgrs != null)
									{
										if (!string.IsNullOrEmpty(m)) dlgPrgrs.SetLine(1, m);
										if (p != null) dlgPrgrs.SetPercentage(p.Value);
										return dlgPrgrs.ShouldRenderLayer();
									}
									else return true;
								}
								);
							if (updateResult == true && OnlineVideoSettings.Instance.SiteUtilsList.Count > 0) guiUpdater.ReloadDownloadedDlls();
							else if (updateResult == null || OnlineVideoSettings.Instance.SiteUtilsList.Count > 0) OnlineVideoSettings.Instance.BuildSiteUtilsList();
							if (updateResult != false)
							{
								PluginConfiguration.Instance.BuildAutomaticSitesGroups();
								GUIOnlineVideos.cachedImageForSite.Clear();
							}
						}
						if (PluginConfiguration.Instance.ThumbsAge >= 0)
						{
							if (dlgPrgrs != null)
							{
								dlgPrgrs.SetHeading(PluginConfiguration.Instance.BasicHomeScreenName);
								dlgPrgrs.SetLine(1, Translation.Instance.DeletingOldThumbs);
								dlgPrgrs.Percentage = 0;
							}
							ImageDownloader.DeleteOldThumbs(PluginConfiguration.Instance.ThumbsAge, r =>
							{
								if (dlgPrgrs != null) dlgPrgrs.Percentage = r;
								return dlgPrgrs != null ? dlgPrgrs.ShouldRenderLayer() : true;
							});
						}
						if (dlgPrgrs != null) { dlgPrgrs.Percentage = 100; dlgPrgrs.SetLine(1, Translation.Instance.Done); dlgPrgrs.Close(); }
						else GUIWaitCursor.Hide();
						GUIWindowManager.SendThreadCallbackAndWait((p1, p2, data) =>
						{
							DoPageLoad();
							return 0;
						}, 0, 0, null);
					}) { Name = "OVLoad", IsBackground = true }.Start();
					return;
				}
			}
			DoPageLoad();
		}

		void DoPageLoad()
		{
			// called everytime the plugin is shown, after some other window was shown (also after fullscreen playback)
			if (PreviousWindowId != 4758)
			{
				// reload settings that can be modified with the MPEI plugin
				PluginConfiguration.Instance.ReLoadRuntimeSettings();

				// if groups are now enabled/disabled we need to set the states accordingly
				if (GroupsEnabled)
				{
					// showing sites, but groups are enabled and no group is selected -> show groups
					if (CurrentState == State.sites && selectedSitesGroup == null) CurrentState = State.groups;
				}
				else
				{
					// showing groups, but groups are disabled -> show sites
					if (CurrentState == State.groups) CurrentState = State.sites;
					selectedSitesGroup = null;
				}

				// reset the LoadParameterInfo
				loadParamInfo = null;

				string loadParam = null;
				// check if running version of mediaportal supports loading with parameter and handle _loadParameter
				System.Reflection.FieldInfo fi = typeof(GUIWindow).GetField("_loadParameter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				if (fi != null)
				{
					loadParam = (string)fi.GetValue(this);
				}
				// check for LoadParameters by GUIproperties if nothing was set by the _loadParameter
				if (string.IsNullOrEmpty(loadParam)) loadParam = LoadParameterInfo.FromGuiProperties();

				if (!string.IsNullOrEmpty(loadParam))
				{
					Log.Instance.Info("Called with LoadParameter: '{0}'", loadParam);
					loadParamInfo = new LoadParameterInfo(loadParam);

                    // set all state variables to reflect the state we were called with
                    currentNavigationContextSwitch = null;
					if (!string.IsNullOrEmpty(loadParamInfo.Group))
					{
						SitesGroup group = PluginConfiguration.Instance.SitesGroups.FirstOrDefault(sg => sg.Name == loadParamInfo.Group);
						if (group == null) group = PluginConfiguration.Instance.CachedAutomaticSitesGroups.FirstOrDefault(sg => sg.Name == loadParamInfo.Group);
						if (group != null)
						{
							selectedSitesGroup = new OnlineVideosGuiListItem(group);
							CurrentState = State.sites;
						}
					}
					if (!string.IsNullOrEmpty(loadParamInfo.Site) && OnlineVideoSettings.Instance.SiteUtilsList.ContainsKey(loadParamInfo.Site))
					{
						SelectedSite = OnlineVideoSettings.Instance.SiteUtilsList[loadParamInfo.Site];
						CurrentState = State.categories;
						selectedCategory = null;
					}
					if (SelectedSite != null && SelectedSite.CanSearch && !string.IsNullOrEmpty(loadParamInfo.Search))
					{
						lastSearchQuery = loadParamInfo.Search;
						Display_SearchResults(loadParamInfo.Search);
						return;
					}
				}
			}

			Log.Instance.Info("DoPageLoad with CurrentState '{0}', PreviousWindowId '{1}'", CurrentState, PreviousWindowId);
			switch (CurrentState)
			{
				case State.groups: DisplayGroups(); break;
				case State.sites: DisplaySites(); break;
				case State.categories: DisplayCategories(selectedCategory); break;
				case State.videos: SetVideosToFacade(currentVideoList, currentVideosDisplayMode); break;
				default: SetVideosToInfoList(currentTrailerList); break;
			}
		}

	}
}
