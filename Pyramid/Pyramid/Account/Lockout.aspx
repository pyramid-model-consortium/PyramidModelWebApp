<%@ Page Title="Locked Out" Language="C#" MasterPageFile="~/MasterPages/NotLoggedIn.Master" AutoEventWireup="true" CodeBehind="Lockout.aspx.cs" Inherits="Pyramid.Account.Lockout" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h3 class="text-danger">This account has been locked out, please try again later.</h3>
    <br />
    <a href="/Account/Login.aspx" class="btn btn-loader btn-primary"><i class="fas fa-arrow-left"></i> Return to Login Page</a>
</asp:Content>
