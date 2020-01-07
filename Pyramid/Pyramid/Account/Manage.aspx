<%@ Page Title="Manage Account" Language="C#" MasterPageFile="~/MasterPages/LoggedIn.Master" AutoEventWireup="true" CodeBehind="Manage.aspx.cs" Inherits="Pyramid.Account.Manage" %>

<%@ Register TagPrefix="uc" TagName="Messaging" Src="~/User_Controls/MessagingSystem.ascx" %>
<%@ Register TagPrefix="uc" TagName="Submit" Src="~/User_Controls/Submit.ascx" %>
<%@ Register Assembly="DevExpress.Web.Bootstrap.v19.1, Version=19.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>

<asp:Content ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content3" runat="server" ContentPlaceHolderID="ScriptContent">
    <script>
        $(document).ready(function () {
            //Run the initial page init
            initializePage();

            //Run the page init after every AJAX load
            var requestManager = Sys.WebForms.PageRequestManager.getInstance();
            requestManager.add_endRequest(initializePage);
        });

        //This function initializes the page
        function initializePage() {
            //Open the confirm modal when editing an email
            $('[ID$="btnEditEmail"]').off('click').on('click', function (e) {
                //Prevent a postback
                e.preventDefault();

                //Validate the email
                if (ASPxClientEdit.ValidateGroup('vgEmail')) {
                    //Only show the modal if the validation is good
                    $('#divEmailChangeModal').modal('show');
                }
                else {
                    //Call the client validation failed method
                    clientValidationFailed();
                }
            });

            //Hide the modal after the user confirms the change
            $('[ID$="lbConfirmEmailChange"]').off('click').on('click', function (e) {
                $('#divEmailChangeModal').modal('hide');
            });
            
            //Set up the click event for the help button for the phone text box
            $('#btnPhoneHelp').on('click', function () {
                //Show the user a help message
                showNotification('primary', 'Phone Number Help', 'Any phone number can be entered, but in order to use your phone for Two-Factor Authentication, the number you enter must be able to accept text messages.<br/><br/>The Verify button will try to send a code via text message to the number that was entered and take you to a page where you can enter that code.  Once that code is entered into the system, you will be able to use that phone number for Two-Factor Authentication.', 20000);
            });

            //Set up the click event for the help button for the two factor authentication
            $('#btnTwoFactorHelp').on('click', function () {
                //Show the user a help message
                showNotification('primary', 'Two-Factor Authentication Help', 'This setting controls whether or not the system will require an email or text message confirmation before logging in.<br/>NOTE: A mobile phone number must be entered and verified above before the text message confirmation will be enabled.', 20000);
            });

            //Set up the click event for the help button by the fireworks dropdown
            $('#btnFireworksHelp').on('click', function () {
                //Show the user a help message
                showNotification('primary', 'Fireworks Help', 'This setting controls whether or not fireworks will appear on the home page and on success notifications.', 20000);
            });
        };
    </script>
</asp:Content>
<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <asp:UpdatePanel ID="upManage" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <uc:Messaging ID="msgSys" runat="server" />
            <div class="row">
                <div class="col-lg-12">
                    <div class="alert alert-primary">
                        Use this page to manage your personal information and settings.
                    </div>
                </div>
            </div>
            <div class="card bg-light">
                <div class="card-header">
                    Contact Info
                </div>
                <div class="card-body">
                    <div class="row">
                        <div class="col-md-6 col-lg-4">
                            <dx:BootstrapTextBox ID="txtEmail" runat="server" Caption="Email">
                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                <ValidationSettings ValidationGroup="vgEmail" ErrorDisplayMode="ImageWithText">
                                    <RequiredField IsRequired="true" ErrorText="Email is required!" />
                                    <RegularExpression ErrorText="Invalid Email!" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" />
                                </ValidationSettings>
                            </dx:BootstrapTextBox>
                            <dx:BootstrapButton ID="btnEditEmail" runat="server" Text="Update" ValidationGroup="vgEmail" SettingsBootstrap-RenderOption="Secondary">
                                <CssClasses Icon="fas fa-edit" Control="btn-loader mt-2" />
                            </dx:BootstrapButton>
                        </div>
                        <div class="col-md-6 col-lg-4">
                            <dx:BootstrapTextBox ID="txtPhoneNumber" runat="server" Caption="Phone Number">
                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                <MaskSettings Mask="+1 (999) 999-9999" IncludeLiterals="None" ErrorText="Must be a valid phone number!" />
                                <ValidationSettings ValidationGroup="vgPhone" ErrorDisplayMode="ImageWithText">
                                    <RequiredField IsRequired="true" ErrorText="Phone Number is required!" />
                                </ValidationSettings>
                            </dx:BootstrapTextBox>
                            <div class="mt-2">
                                <dx:BootstrapButton ID="btnAddPhone" runat="server" OnClick="EditPhone_Click" Text="Add" ValidationGroup="vgPhone" SettingsBootstrap-RenderOption="Secondary">
                                    <CssClasses Icon="fas fa-plus" Control="btn-loader" />
                                </dx:BootstrapButton>
                                <dx:BootstrapButton ID="btnEditPhone" runat="server" OnClick="EditPhone_Click" Text="Edit" ValidationGroup="vgPhone" SettingsBootstrap-RenderOption="Secondary">
                                    <CssClasses Icon="fas fa-edit" Control="btn-loader" />
                                </dx:BootstrapButton>
                                <dx:BootstrapButton ID="btnRemovePhone" runat="server" OnClick="btnRemovePhone_Click" Text="Remove" SettingsBootstrap-RenderOption="Warning">
                                    <CssClasses Icon="fas fa-minus" Control="btn-loader" />
                                </dx:BootstrapButton>
                                <dx:BootstrapButton ID="btnVerifyPhone" runat="server" OnClick="btnVerifyPhone_Click" Text="Verify" ValidationGroup="vgPhone" SettingsBootstrap-RenderOption="Primary">
                                    <CssClasses Icon="fas fa-check" Control="btn-loader" />
                                </dx:BootstrapButton>
                            </div>
                            <button id="btnPhoneHelp" class="btn btn-link"><i class="fas fa-question-circle"></i>&nbsp;Help</button>
                        </div>
                    </div>
                </div>
            </div>
            <div class="card bg-light mt-4">
                <div class="card-header">
                    Security Settings
                </div>
                <div class="card-body">
                    <div class="row">
                        <div class="col-md-6 col-lg-4">
                            <label>Password</label>
                            <div>
                                <a id="lnkChangePassword" href="/Account/ManagePassword" class="btn btn-loader btn-secondary"><i class="fas fa-edit"></i>&nbsp;Change Password</a>
                            </div>
                        </div>
                        <div class="col-md-6 col-lg-4">
                            <label>Two-Factor Authentication</label>
                            <div>
                                <asp:LinkButton ID="lbEnableTwoFactor" runat="server" CssClass="btn btn-loader btn-primary" OnClick="TwoFactorEnable_Click"><i class="fas fa-toggle-off"></i>&nbsp;Enable</asp:LinkButton>
                                <asp:LinkButton ID="lbDisableTwoFactor" runat="server" CssClass="btn btn-loader btn-primary" OnClick="TwoFactorDisable_Click"><i class="fas fa-toggle-on"></i>&nbsp;Disable</asp:LinkButton>
                            </div>
                            <button id="btnTwoFactorHelp" class="btn btn-link"><i class="fas fa-question-circle"></i>&nbsp;Help</button>
                        </div>
                    </div>
                </div>
            </div>
            <div class="card bg-light mt-4">
                <div class="card-header">
                    Customization Options
                </div>
                <div class="card-body">
                    <div class="row">
                        <div class="col-md-6 col-lg-4">
                            <dx:BootstrapComboBox ID="ddFireworks" runat="server" Caption="Fireworks" NullText="--Select--"
                                TextField="Description" ValueField="CodeCustomizationOptionValuePK" ValueType="System.Int32" 
                                IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                <ValidationSettings ValidationGroup="vgCustomizationOptions" ErrorDisplayMode="ImageWithText">
                                    <RequiredField IsRequired="true" ErrorText="Fireworks selection is required!" />
                                </ValidationSettings>
                            </dx:BootstrapComboBox>
                            <button id="btnFireworksHelp" class="btn btn-link"><i class="fas fa-question-circle"></i>&nbsp;Help</button>
                        </div>
                        <div class="col-md-6 col-lg-4">

                        </div>
                    </div>
                </div>
                <div class="card-footer">
                    <div class="center-content">
                        <uc:Submit ID="submitCustomizationOptions" runat="server" ShowCancelButton="false" SubmitButtonText="Save Customization Options" ValidationGroup="vgCustomizationOptions" OnSubmitClick="submitCustomizationOptions_Click" OnValidationFailed="submitCustomizationOptions_ValidationFailed" />
                    </div>
                </div>
            </div>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="lbConfirmEmailChange" />
        </Triggers>
    </asp:UpdatePanel>
    <div id="divEmailChangeModal" class="modal fade">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Warning!</h4>
                </div>
                <div class="modal-body">
                    <div class="alert alert-warning">
                        <strong>If you update your email, you will not be able to log back in until you verify it through a confirmation email.</strong>
                        <br />
                        Are you sure you want to change your email?
                    </div>
                </div>
                <div class="modal-footer">
                    <button id="btnCancelEmailChange" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbConfirmEmailChange" runat="server" CssClass="btn btn-loader btn-warning" OnClick="lbConfirmEmailChange_Click"><i class="fas fa-check"></i>&nbsp;Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
