<%@ Page Title="Account Confirmed" Language="C#" MasterPageFile="~/MasterPages/NotLoggedIn.Master" AutoEventWireup="true" CodeBehind="Confirm.aspx.cs" Inherits="Pyramid.Account.Confirm" Async="true" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <div>
        <asp:PlaceHolder runat="server" ID="successPanel" ViewStateMode="Disabled" Visible="true">
            <div id="divEmailConfirm" runat="server">
                <div class="row">
                    <div class="col-md-6">
                        <p>
                            Your email address has been confirmed!
                        </p>
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-6">
                        <a href="/Account/login.aspx" class="btn btn-loader btn-primary w-100">Log In</a>
                    </div>
                </div>
            </div>
            <div id="divAccountConfirm" runat="server" >
                <div class="row">
                    <div class="col-md-6">
                        <p>
                            Your account has been confirmed!  Please create your password by using the button below.
                        </p>
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-6">
                        <a href="/Account/CreatePasswordLink.aspx" class="btn btn-loader btn-primary w-100">Create your password</a>
                    </div>
                </div>
            </div>
        </asp:PlaceHolder>
        <asp:PlaceHolder runat="server" ID="errorPanel" ViewStateMode="Disabled" Visible="false">
            <p class="text-danger">
                An error has occurred.
            </p>
        </asp:PlaceHolder>
    </div>
</asp:Content>
