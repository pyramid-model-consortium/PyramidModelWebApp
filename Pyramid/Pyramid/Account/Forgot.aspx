<%@ Page Title="Forgot password" Language="C#" MasterPageFile="~/MasterPages/NotLoggedIn.Master" AutoEventWireup="true" CodeBehind="Forgot.aspx.cs" Inherits="Pyramid.Account.ForgotPassword" Async="true" %>

<%@ Register TagPrefix="uc" TagName="Messaging" Src="~/User_Controls/MessagingSystem.ascx" %>
<%@ Register Assembly="DevExpress.Web.Bootstrap.v19.1, Version=19.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>

<asp:Content runat="server" ID="ScriptContent" ContentPlaceHolderID="ScriptContent">
    <script>
        $(document).ready(function () {
            //Initialize the page on page load
            initializePage();

            //Initialize the page on AJAX postbacks
            var requestManager = Sys.WebForms.PageRequestManager.getInstance();
            requestManager.add_endRequest(initializePage);
        });

        function initializePage(s, e) {
            setUpSendForgotLinkButtonClick();
        }
        
        //This function shows a sending button for the Send Email button if the 
        //validation succeeds
        function setUpSendForgotLinkButtonClick() {
            //Get the send forgot link button
            var btnSendForgotLink = $('[ID$="btnSendForgotLink"]');

            //Hide the sending button
            $('.btn-sending').hide();

            //Show the send forgot link button
            btnSendForgotLink.show();

            //Create the click event
            btnSendForgotLink.off('click').on('click', function () {
                //Get the validation group
                var validationGroup = 'vgForgot';

                if (ASPxClientEdit.ValidateGroup(validationGroup)) {
                    //Validation succeeded
                    //Get the Send Email button and sending button
                    var sendingButton = btnSendForgotLink.clone();

                    //Set the sending button attributes and content
                    sendingButton.attr('id', this.id + '_sending');
                    sendingButton.addClass('.btn-sending');
                    sendingButton.html('<span class="spinner-border spinner-border-sm"></span>&nbsp;Sending...');
                    sendingButton.off('click').on('click', function (e) {
                        e.preventDefault();
                        e.stopPropagation();
                    });

                    //Prevent clicks on the Send Email button
                    btnSendForgotLink.off('click').on('click', function (e) {
                        e.preventDefault();
                        e.stopPropagation();
                    });

                    //Append the sending button
                    btnSendForgotLink.after(sendingButton);

                    //Hide the Send Email button
                    btnSendForgotLink.hide();

                    //Show the sending button
                    sendingButton.show();
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
            <div class="row">
                <div class="col-md-8">
                    <p>
                        Please enter your username and then click the 'Email Link' button below to start the password reset process.
                    </p>
                    <div id="divForgot" runat="server">
                        <dx:BootstrapTextBox ID="txtUsername" runat="server" Caption="Username">
                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                            <ValidationSettings ValidationGroup="vgForgot" ErrorDisplayMode="ImageWithText">
                                <RequiredField IsRequired="true" ErrorText="Username is required!" />
                            </ValidationSettings>
                        </dx:BootstrapTextBox>
                        <dx:BootstrapButton ID="btnSendForgotLink" runat="server" Text="Email Link"
                            OnClick="btnSendForgotLink_Click" AutoPostBack="true" ValidationGroup="vgForgot"
                            SettingsBootstrap-RenderOption="primary">
                            <CssClasses Icon="fas fa-envelope" Control="mt-3" />
                        </dx:BootstrapButton>
                    </div>
                    <div id="divEmailSent" runat="server" visible="false">
                        <div class="alert alert-primary">
                            Email sent!
                            <br />
                            Please check your email and follow the instructions to reset your password.
                        </div>
                    </div>
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
