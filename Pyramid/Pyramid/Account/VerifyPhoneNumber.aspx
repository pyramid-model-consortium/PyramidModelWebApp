<%@ Page Title="Verify Phone Number" Language="C#" MasterPageFile="~/MasterPages/LoggedIn.Master" AutoEventWireup="true" CodeBehind="VerifyPhoneNumber.aspx.cs" Inherits="Pyramid.Account.VerifyPhoneNumber" %>

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
            setVerifyCodeButtonClick();
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
                    verifyingButton.addClass('.btn-verifying');
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
            <div class="row">
                <div class="col-lg-12">
                    <div class="alert alert-primary">
                        <i class="fas fa-info-circle"></i>&nbsp;A code has been texted to <asp:Label ID="lblPhoneNum" runat="server"></asp:Label>, please enter it below after you receive it.
                    </div>
                    <div id="divWarning" runat="server" class="alert alert-warning" visible="false">
                        <i class="fas fa-exclamation-triangle"></i>&nbsp;Didn't receive a text message with a code?  Please make sure that the phone number above is correct and can receive text messages.
                    </div>
                    <asp:HiddenField ID="hfPhoneNumber" runat="server" />
                    <dx:BootstrapTextBox ID="txtCode" runat="server" Caption="Code">
                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                        <ValidationSettings ValidationGroup="vgVerifyCode" ErrorDisplayMode="ImageWithText">
                            <RequiredField IsRequired="true" ErrorText="Code is required!" />
                        </ValidationSettings>
                    </dx:BootstrapTextBox>
                    <dx:BootstrapButton ID="btnVerifyCode" runat="server" Text="Verify Code"
                        OnClick="btnVerifyCode_Click" AutoPostBack="true" ValidationGroup="vgVerifyCode"
                        SettingsBootstrap-RenderOption="Primary" data-validation-group="vgVerifyCode">
                        <CssClasses Icon="fas fa-shield-alt" Control="btn-loader mt-3" />
                    </dx:BootstrapButton>
                    <dx:BootstrapButton ID="btnResendCode" runat="server" OnClick="btnResendCode_Click" Text="Resend Code" 
                        SettingsBootstrap-RenderOption="Secondary">
                        <CssClasses Icon="fas fa-redo" Control="btn-loader mt-3" />
                    </dx:BootstrapButton>
                </div>
            </div>
            <div class="row">
                <div class="col-lg-12">
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
