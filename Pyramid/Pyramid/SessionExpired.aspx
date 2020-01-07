<%@ Page Title="Session Expired" Language="C#" MasterPageFile="~/MasterPages/NotLoggedIn.master" AutoEventWireup="true" CodeBehind="SessionExpired.aspx.cs" Inherits="Pyramid.SessionExpired" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ScriptContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row">
        <div class="col-md-12">
            <div class="alert alert-danger text-center">
                <p>Your session has expired after a period of inactivity.</p>
                <a href="/Account/Login.aspx" class="btn btn-loader btn-secondary">Login</a>
            </div>
        </div>
    </div>
</asp:Content>
