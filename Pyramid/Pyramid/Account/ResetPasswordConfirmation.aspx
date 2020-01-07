<%@ Page Title="Password Updated" Language="C#" MasterPageFile="~/MasterPages/NotLoggedIn.Master" AutoEventWireup="true" CodeBehind="ResetPasswordConfirmation.aspx.cs" Inherits="Pyramid.Account.ResetPasswordConfirmation" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <div class="row">
        <div class="col-md-6">
            <p>
                Your password has been successfully updated!
            </p>
        </div>
    </div>
    <div class="row">
        <div class="col-md-6">
            <a href="/Account/login.aspx" class="btn btn-loader btn-primary w-100">Log In</a>
        </div>
    </div>
</asp:Content>
