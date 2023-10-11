<%@ Page Title="Locked Out" Language="C#" MasterPageFile="~/MasterPages/NotLoggedIn.Master" AutoEventWireup="true" CodeBehind="Lockout.aspx.cs" Inherits="Pyramid.Account.Lockout" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row">
        <div class="col-md-12">
            <div class="alert alert-warning" style="font-size: large;">
                <p>Your account has been locked out after too many failed login attempts.</p>
                <p>Please try logging in again after 5-10 minutes.</p>
                <hr />
                <div class="d-flex justify-content-center">
                    <a href="/Account/Login.aspx" class="btn btn-loader btn-secondary">
                        <i class="fas fa-arrow-left"></i>
                        Return to Log In Page
                    </a>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
