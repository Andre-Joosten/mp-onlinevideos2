﻿<%@ Page Title="OnlineVideos Sites Overview" Language="C#" AutoEventWireup="true" MasterPageFile="~/OnlineVideos.Master" CodeBehind="SiteOverview.aspx.cs" Inherits="OnlineVideos.WebService.SiteOverview" %>
<asp:Content ID="Content1" ContentPlaceHolderID="body" runat="server">
    <div>        
        <asp:GridView ID="siteOverview" runat="server" AutoGenerateColumns="False" AllowSorting="True"
            CellPadding="3" 
            onrowdatabound="siteOverview_RowDataBound" BackColor="#EEEEEE" 
            BorderColor="#CCCCCC" BorderStyle="None" BorderWidth="1px" 
            onsorting="siteOverview_Sorting" EnableViewState="False" 
            EnableModelValidation="True" Font-Names="Calibri">
            <RowStyle ForeColor="#000066" />
        <Columns>
            <asp:TemplateField HeaderText="Nr." ItemStyle-HorizontalAlign="Right">
                <ItemTemplate>
                    <asp:Label runat="server" Text='<%# ((int)DataBinder.Eval(Container, "DataItemIndex")+1).ToString() + "." %>' />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:ImageField HeaderText="Logo" DataImageUrlField="Name" 
                DataImageUrlFormatString="./Icons/{0}.png" ControlStyle-Width="48" 
                ControlStyle-Height="48">
                <ControlStyle Height="48px" Width="48px"></ControlStyle>
            </asp:ImageField>
            <asp:TemplateField HeaderText="Site" SortExpression="Name" ItemStyle-HorizontalAlign="Center" ItemStyle-Font-Bold="true">
                <ItemTemplate>
                    <asp:HyperLink ID="HyperLink1" runat="server" Text='<%# Eval("Name") %>' Visible='<%# (uint)Eval("ReportCount") > 0 %>' NavigateUrl='<%# "Reports.aspx?site=" + HttpUtility.UrlEncode((string)Eval("Name")) %>' />
                    <asp:Label ID="Label1" runat="server" Text='<%# Eval("Name") %>' Visible='<%# (uint)Eval("ReportCount") == 0 %>' />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField HeaderText="Creator" DataField="Owner_FK" SortExpression="Owner_FK"
                ItemStyle-HorizontalAlign="Center" >
                <ItemStyle HorizontalAlign="Center"></ItemStyle>
            </asp:BoundField>
            <asp:BoundField HeaderText="Language" DataField="Language" SortExpression="Language"
                ItemStyle-HorizontalAlign="Center" >
                <ItemStyle HorizontalAlign="Center"></ItemStyle>
            </asp:BoundField>
            <asp:BoundField HeaderText="Update" DataField="LastUpdated" DataFormatString="{0:g}" SortExpression="LastUpdated" />
            <asp:BoundField HeaderText="Description" DataField="Description" />            
        </Columns>
            <FooterStyle BackColor="White" ForeColor="#000066" />
            <PagerStyle BackColor="White" ForeColor="#000066" HorizontalAlign="Left" />
            <SelectedRowStyle BackColor="#669999" Font-Bold="True" ForeColor="White" />
            <HeaderStyle BackColor="#006699" Font-Bold="True" ForeColor="White" />
            <AlternatingRowStyle BackColor="LightBlue" />
        </asp:GridView>
    </div>    
</asp:Content>