<%@ Page Title="Password Created" Language="C#" MasterPageFile="~/MasterPages/NotLoggedIn.master" AutoEventWireup="true" CodeBehind="CreatePasswordConfirmation.aspx.cs" Inherits="Pyramid.Account.CreatePasswordConfirmation" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <div class="row">
        <div class="col-md-6">
            <p>
                Your password has been successfully created!
            </p>
        </div>
    </div>
    <div class="row">
        <div class="col-md-6">
            <a href="/Account/login.aspx" class="btn btn-loader btn-primary w-100">Log In</a>
        </div>
    </div>
</asp:Content>
