<%@ Page Title="Download File" Language="C#" MasterPageFile="~/MasterPages/LoggedIn.master" AutoEventWireup="true" CodeBehind="DownloadFile.aspx.cs" Inherits="Pyramid.Pages.DownloadFile" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ScriptContent" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <div class="alert alert-danger">
        Download failed!  Please try again, and if it continues to fail, please <a href="../SupportTicket.aspx">submit a support ticket</a>.
    </div>
</asp:Content>
