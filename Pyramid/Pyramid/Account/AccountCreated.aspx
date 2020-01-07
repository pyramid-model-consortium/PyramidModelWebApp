<%@ Page Title="Account Created" Language="C#" MasterPageFile="~/MasterPages/NotLoggedIn.Master" AutoEventWireup="true" CodeBehind="AccountCreated.aspx.cs" Inherits="Pyramid.Account.AccountCreated" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="center">
        <div class="card" style="width: 18rem;">
            <img src="../Content/images/tick.png" class="card-img-top" alt="...">
            <div class="card-body">
                <p class="card-text">Thank You for creating your account. Please check for email for a link to confirm your account </p>
            </div>
        </div>
    </div>
</asp:Content>
