<%@ Page Title="Log in" Language="C#" MasterPageFile="~/MasterPages/NotLoggedIn.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Pyramid.Account.Login" %>

<%@ Register TagPrefix="uc" TagName="Messaging" Src="~/User_Controls/MessagingSystem.ascx" %>
<%@ Register Assembly="DevExpress.Web.Bootstrap.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>

<asp:Content runat="server" ID="ScriptContent" ContentPlaceHolderID="ScriptContent">
    <script>
        $(document).ready(function () {
            //Initialize the master page on page load
            initializePage();

            //Initialize the master page and check for server errors on AJAX postbacks
            var requestManager = Sys.WebForms.PageRequestManager.getInstance();
            requestManager.add_endRequest(initializePage);
        });

        //This function shows a logging in button for the Log In button if the 
        //validation succeeds
        function initializePage(s, e) {
            //Get the login button
            var btnLogin = $('[ID$="btnLogin"]');

            //Hide the loading button
            $('.btn-loading').hide();

            //Show the login button
            btnLogin.show();

            //Set the click event
            btnLogin.off('click').on('click', function () {
                //Get the validation group
                var validationGroup = 'vgLogin';

                if (ASPxClientEdit.ValidateGroup(validationGroup)) {
                    //Validation succeeded
                    //Get the Log In button and loggingIn button
                    var loggingInButton = btnLogin.clone();
                    
                    //Set the loading button's attributes, content, and click event
                    loggingInButton.attr('id', this.id + '_loading');
                    loggingInButton.addClass('btn-loading');
                    loggingInButton.html('<span class="spinner-border spinner-border-sm"></span>&nbsp;Logging In...');
                    loggingInButton.off('click').on('click', function (e) {
                        e.preventDefault();
                        e.stopPropagation();
                    });

                    //Append the logging in button
                    btnLogin.after(loggingInButton);

                    //Hide the Log In button
                    btnLogin.hide();

                    //Show the logging in button
                    loggingInButton.show();

                }
                else {
                    //Call the client validation failed method
                    clientValidationFailed();
                }
            });
        }
    </script>
</asp:Content>
<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <asp:UpdatePanel ID="upEditUser" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <uc:Messaging ID="msgSys" runat="server" />
            <div class="row d-flex justify-content-center">
                <div class="col-sm-6">
                    <asp:Panel ID="pnlLoginForm" runat="server" DefaultButton="btnLogin">
                        <section id="loginForm">
                            <div class="form-horizontal">
                                <div class="form-group mt-2">
                                    <h2>Log In</h2>
                                    <hr />
                                </div>
                                <div class="form-group">
                                    <dx:BootstrapTextBox ID="txtUsername" runat="server" Caption="Username">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <CssClasses Input="mw-100" Control="mw-100" />
                                        <ValidationSettings ValidationGroup="vgLogin" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Username is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapTextBox>
                                </div>
                                <div class="form-group">
                                    <dx:BootstrapTextBox ID="txtPassword" ClientInstanceName="txtPassword" Password="true" runat="server" Caption="Password">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <CssClasses Input="mw-100" Control="mw-100" />
                                        <ValidationSettings ValidationGroup="vgLogin" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Password is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapTextBox>
                                </div>
                                <div class="form-group">
                                    <dx:BootstrapButton ID="btnLogin" runat="server" Text="Log In"
                                        OnClick="btnLogin_Click" AutoPostBack="true" ValidationGroup="vgLogin"
                                        SettingsBootstrap-RenderOption="success">
                                        <CssClasses Icon="fas fa-key" Control="w-100" />
                                    </dx:BootstrapButton>
                                </div>
                                <div class="form-group">
                                    <a href="/Account/Forgot.aspx" class="btn btn-loader btn-secondary w-100">Forgot your password?</a>
                                </div>
                                <div class="form-group">
                                    <label>Don't have a password?</label>
                                    <a href="/Account/CreatePasswordLink.aspx" class="btn btn-loader btn-primary w-100">Create your password</a>
                                </div>
                            </div>
                        </section>
                    </asp:Panel>
                    <hr />
                    <div class="row">
                        <div class="col text-center">
                            <p class="disclaimer"></p>
                            <p>
                                <a href="/SupportTicket.aspx" class="btn btn-loader btn-link w-100">
                                    <i class="fas fa-headset"></i>
                                    Submit a Support Ticket
                                </a>
                            </p>
                        </div>
                    </div>
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
