﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.239
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OnlineVideos.Sites.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("net.pipe://127.0.0.1/MPExtended/MediaAccessService")]
        public string OnlineVideos_Sites_diebagger_MpExtendedService_MediaAccessService {
            get {
                return ((string)(this["OnlineVideos_Sites_diebagger_MpExtendedService_MediaAccessService"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("net.pipe://127.0.0.1/MPExtended/StreamingService/soap")]
        public string OnlineVideos_Sites_diebagger_MpExtendedStreamingService_StreamingService {
            get {
                return ((string)(this["OnlineVideos_Sites_diebagger_MpExtendedStreamingService_StreamingService"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("net.pipe://127.0.0.1/MPExtended/TVAccessService")]
        public string OnlineVideos_Sites_diebagger_MpExtendedTvService_TVAccessService {
            get {
                return ((string)(this["OnlineVideos_Sites_diebagger_MpExtendedTvService_TVAccessService"]));
            }
        }
    }
}
