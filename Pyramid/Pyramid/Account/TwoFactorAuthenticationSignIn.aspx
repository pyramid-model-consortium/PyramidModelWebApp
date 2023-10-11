<%@ Page Title="Two-Factor Authentication" Language="C#" MasterPageFile="~/MasterPages/NotLoggedIn.Master" AutoEventWireup="true" CodeBehind="TwoFactorAuthenticationSignIn.aspx.cs" Inherits="Pyramid.Account.TwoFactorAuthenticationSignIn" %>

<%@ Register TagPrefix="uc" TagName="Messaging" Src="~/User_Controls/MessagingSystem.ascx" %>
<%@ Register Assembly="DevExpress.Web.Bootstrap.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>

<asp:Content runat="server" ID="ScriptContent" ContentPlaceHolderID="ScriptContent">
    <script>
        $(document).ready(function () {
            //Initialize the page on page load
            initializePage();

            //Initialize the page on AJAX postbacks
            var requestManager = Sys.WebForms.PageRequestManager.getInstance();
            requestManager.add_endRequest(initializePage);
        });

        //This function initializes the page
        function initializePage(s, e) {
            //Set up the button click events
            setSendCodeButtonClick();
            setVerifyCodeButtonClick();
        }

        //This function shows a sending button for the Send Code button if the 
        //validation succeeds
        function setSendCodeButtonClick() {
            //Get the send code button
            var btnSendCode = $('[ID$="btnSendCode"]');

            //Hide the sending button
            $('.btn-sending').hide();

            //Show the send code button
            btnSendCode.show();

            //Create the click event
            btnSendCode.off('click').on('click', function () {
                //Get the validation group
                var validationGroup = 'vgSendCode';

                if (ASPxClientEdit.ValidateGroup(validationGroup)) {
                    //Validation succeeded
                    //Get the sending button
                    var sendingButton = btnSendCode.clone();

                    //Set the sending button attributes and content
                    sendingButton.attr('id', this.id + '_send');
                    sendingButton.addClass('btn-sending');
                    sendingButton.html('<span class="spinner-border spinner-border-sm"></span>&nbsp;Sending...');
                    sendingButton.off('click').on('click', function (e) {
                        e.preventDefault();
                        e.stopPropagation();
                    });

                    //Prevent clicks on the Send Code button
                    btnSendCode.off('click').on('click', function (e) {
                        e.preventDefault();
                        e.stopPropagation();
                    });

                    //Append the sending button
                    btnSendCode.after(sendingButton);

                    //Hide the Send Code button
                    btnSendCode.hide();

                    //Show the sending button
                    sendingButton.show();
                }
                else {
                    //Call the client validation failed method
                    clientValidationFailed();
                }
            });
        }

        //This function shows a verifying button for the Verify Code button if the 
        //validation succeeds
        function setVerifyCodeButtonClick() {
            //Get the verify code button
            var btnVerifyCode = $('[ID$="btnVerifyCode"]');

            //Hide the verifying button
            $('.btn-verifying').hide();

            //Show the verify code button
            btnVerifyCode.show();

            //Create the click event
            btnVerifyCode.off('click').on('click', function () {
                //Get the validation group
                var validationGroup = 'vgVerifyCode';

                if (ASPxClientEdit.ValidateGroup(validationGroup)) {
                    //Validation succeeded
                    //Get the verifying button
                    var verifyingButton = btnVerifyCode.clone();

                    //Set the verifying button attributes and content
                    verifyingButton.attr('id', this.id + '_verify');
                    verifyingButton.addClass('btn-verifying');
                    verifyingButton.html('<span class="spinner-border spinner-border-sm"></span>&nbsp;Verifying...');
                    verifyingButton.off('click').on('click', function (e) {
                        e.preventDefault();
                        e.stopPropagation();
                    });

                    //Prevent clicks on the Verify Code button
                    btnVerifyCode.off('click').on('click', function (e) {
                        e.preventDefault();
                        e.stopPropagation();
                    });

                    //Append the verifying button
                    btnVerifyCode.after(verifyingButton);

                    //Hide the Verify Code button
                    btnVerifyCode.hide();

                    //Show the verifying button
                    verifyingButton.show();
                }
                else {
                    //Call the client validation failed method
                    clientValidationFailed();
                }
            });
        }
    </script>
</asp:Content>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <asp:UpdatePanel ID="upEditUser" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <uc:Messaging ID="msgSys" runat="server" />
            <div id="divChooseProvider" runat="server">
                <h4>Send verification code</h4>
                <p>Choose a delivery method from the list below and then click the 'Send Code' button to receive your security code.</p>
                <hr />
                <div class="row">
                    <div class="col-md-12">
                        <dx:BootstrapComboBox ID="ddTwoFactorProviders" runat="server" Caption="Code Delivery Method" NullText="--Select--"
                            TextField="ProviderName" ValueField="ProviderName" ValueType="System.String"
                            IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                            <ValidationSettings ValidationGroup="vgSendCode" ErrorDisplayMode="ImageWithText">
                                <RequiredField IsRequired="true" ErrorText="Required!" />
                            </ValidationSettings>
                        </dx:BootstrapComboBox>
                        <dx:BootstrapButton ID="btnSendCode" runat="server" Text="Send Code"
                            OnClick="btnSendCode_Click" AutoPostBack="true" ValidationGroup="vgSendCode"
                            SettingsBootstrap-RenderOption="primary">
                            <CssClasses Icon="fas fa-envelope" Control="mt-3" />
                        </dx:BootstrapButton>
                    </div>
                </div>
            </div>
            <div id="divEnterCode" runat="server" visible="false">
                <h4>Enter verification code</h4>
                <p>Enter the security code you received below and then click the 'Verify Code' button.</p>
                <hr />
                <asp:HiddenField ID="hfSelectedProvider" runat="server" />
                <dx:BootstrapTextBox ID="txtCode" runat="server" Caption="Code">
                    <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                    <ValidationSettings ValidationGroup="vgVerifyCode" ErrorDisplayMode="ImageWithText">
                        <RequiredField IsRequired="true" ErrorText="Code is required!" />
                    </ValidationSettings>
                </dx:BootstrapTextBox>
                <br />
                <asp:CheckBox Text="Remember browser?" ID="chkRememberBrowser" runat="server" TextAlign="Right" CssClass="mt-3" />
                <br />
                <dx:BootstrapButton ID="btnVerifyCode" runat="server" Text="Verify Code"
                    OnClick="btnVerifyCode_Click" AutoPostBack="true" ValidationGroup="vgVerifyCode"
                    SettingsBootstrap-RenderOption="primary" data-validation-group="vgVerifyCode">
                    <CssClasses Icon="fas fa-shield-alt" Control="btn-loader mt-3" />
                </dx:BootstrapButton>
                <dx:BootstrapButton ID="btnResendCode" runat="server" OnClick="btnResendCode_Click" Text="Resend Code" 
                    SettingsBootstrap-RenderOption="Secondary">
                    <CssClasses Icon="fas fa-redo" Control="btn-loader mt-3" />
                </dx:BootstrapButton>
                <dx:BootstrapButton ID="btnSelectOtherMethod" runat="server" OnClick="btnSelectOtherMethod_Click" Text="Select Other Delivery Method" 
                    SettingsBootstrap-RenderOption="Secondary">
                    <CssClasses Icon="fas fa-exchange-alt" Control="btn-loader mt-3" />
                </dx:BootstrapButton>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
