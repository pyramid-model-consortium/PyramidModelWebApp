<%@ Page Title="Manage Account" Language="C#" MasterPageFile="~/MasterPages/LoggedIn.Master" AutoEventWireup="true" CodeBehind="Manage.aspx.cs" Inherits="Pyramid.Account.Manage" %>

<%@ Register TagPrefix="uc" TagName="Messaging" Src="~/User_Controls/MessagingSystem.ascx" %>
<%@ Register TagPrefix="uc" TagName="Submit" Src="~/User_Controls/Submit.ascx" %>
<%@ Register Assembly="DevExpress.Web.Bootstrap.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>

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
            $('#btnPhoneHelp').popover({
                trigger: 'hover focus',
                title: 'Help',
                html: true,
                content: 'Any phone number can be entered, but in order to use your phone for Two-Factor Authentication, ' +
                    'the number you enter must be able to accept text messages.<br/><br/>' +
                    'The Verify button will try to send a code via text message to the number that was entered and take you to a page where you can enter that code. ' +
                    'Once that code is entered into the system, you will be able to use that phone number for Two-Factor Authentication.'
            });

            //Set up the click event for the help button for the phone text box
            $('#btnWorkPhoneHelp').popover({
                trigger: 'hover focus',
                title: 'Help',
                html: true,
                content: 'This is your work phone number.  It cannot be used for two-factor authentication.'
            });

            //Set up the click event for the help button for the two factor authentication
            $('#btnTwoFactorHelp').popover({
                trigger: 'hover focus',
                title: 'Help',
                html: true,
                content: 'This setting controls whether or not the system will require an email or text message confirmation before logging in.<br/>' +
                    'NOTE: A mobile phone number must be entered and verified above before the text message confirmation will be enabled.'
            });

            //Set up the click event for the help button by the fireworks dropdown
            $('#btnFireworksHelp').popover({
                trigger: 'hover focus',
                title: 'Help',
                html: true,
                content: 'This setting controls whether or not fireworks will appear on the home page and on success notifications.'
            });

            //Set up the click event for the help button by the welcome message dropdown
            $('#btnWelcomeMessageHelp').popover({
                trigger: 'hover focus',
                title: 'Help',
                html: true,
                content: 'This setting controls whether or not the state logo and welcome message will display on the home page.<br/>' +
                    'NOTE: The welcome message will only be hidden if this setting is disabled and there is other content on the home page.'
            });

            //Set up the click event for the help button by the cancel confirmation dropdown
            $('#btnCancelConfirmationHelp').popover({
                trigger: 'hover focus',
                title: 'Help',
                html: true,
                content: 'This setting controls whether or not you will be prompted to confirm when you click the cancel button on data-entry forms.'
            });
        }
    </script>
</asp:Content>
<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <asp:UpdatePanel ID="upManage" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <uc:Messaging ID="msgSys" runat="server" />
            <div class="row">
                <div class="col-lg-12">
                    <div class="alert alert-primary">
                        <i class="fas fa-info-circle"></i>&nbsp;
                        Use this page to manage your personal information and settings.
                    </div>
                </div>
            </div>
            <div id="divAccountUpdateAlert" runat="server">
                <div class="alert alert-warning">
                    <div>
                        <i class="fas fa-exclamation-circle"></i>&nbsp;
                        Please review the account information below and update it if necessary.  If all the information is correct, just click the button below.
                    </div>
                    <div>
                        <dx:BootstrapButton ID="btnConfirmAccountInfo" runat="server" Text="My Information is Correct" OnClick="btnConfirmAccountInfo_Click"
                            SettingsBootstrap-RenderOption="Success">
                            <CssClasses Icon="fas fa-check" Control="btn-loader mt-2" />
                        </dx:BootstrapButton>
                    </div>
                </div>
            </div>
            <div class="card bg-light">
                <div class="card-header">
                    General Info
                </div>
                <div class="card-body">
                    <div class="row">
                        <div class="col-md-4 col-lg-4">
                            <dx:BootstrapTextBox ID="txtFirstName" runat="server" Caption="First Name">
                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                <ValidationSettings ValidationGroup="vgGeneralInfo" ErrorDisplayMode="ImageWithText">
                                    <RequiredField IsRequired="true" ErrorText="First Name is required!" />
                                </ValidationSettings>
                            </dx:BootstrapTextBox>
                        </div>
                        <div class="col-md-4 col-lg-4">
                            <dx:BootstrapTextBox ID="txtLastName" runat="server" Caption="Last Name">
                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                <ValidationSettings ValidationGroup="vgGeneralInfo" ErrorDisplayMode="ImageWithText">
                                    <RequiredField IsRequired="true" ErrorText="Last Name is required!" />
                                </ValidationSettings>
                            </dx:BootstrapTextBox>
                        </div>
                        <div class="col-md-4 col-lg-4">
                            <label>Last Login</label>
                            <div>
                                <asp:Label ID="lblLastLoginDate" runat="server"></asp:Label>
                            </div>
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-6 col-lg-4">
                            <dx:BootstrapTextBox ID="txtStreet" runat="server" Caption="Street Address">
                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                <ValidationSettings ValidationGroup="vgGeneralInfo" ErrorDisplayMode="ImageWithText">
                                    <RequiredField IsRequired="false" ErrorText="Street is required!" />
                                </ValidationSettings>
                            </dx:BootstrapTextBox>
                        </div>
                        <div class="col-md-6 col-lg-4">
                            <dx:BootstrapTextBox ID="txtCity" runat="server" Caption="City">
                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                <ValidationSettings ValidationGroup="vgGeneralInfo" ErrorDisplayMode="ImageWithText">
                                    <RequiredField IsRequired="false" ErrorText="City is required!" />
                                </ValidationSettings>
                            </dx:BootstrapTextBox>
                        </div>
                        <div class="col-md-6 col-lg-4">
                            <dx:BootstrapComboBox ID="ddState" runat="server" Caption="State" NullText="--Select--" AllowNull="true"
                                TextField="Name" ValueField="Name" ValueType="System.String" ClearButton-DisplayMode="Always">
                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                <ValidationSettings ValidationGroup="vgGeneralInfo" ErrorDisplayMode="ImageWithText">
                                    <RequiredField IsRequired="false" ErrorText="State is required!" />
                                </ValidationSettings>
                            </dx:BootstrapComboBox>
                        </div>
                    </div>
                <div class="row">
                    <div class="col-md-6 col-lg-4">
                        <dx:BootstrapTextBox ID="txtZIPCode" runat="server" Caption="ZIP Code">
                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                            <ValidationSettings ValidationGroup="vgGeneralInfo" ErrorDisplayMode="ImageWithText">
                                <RequiredField IsRequired="false" ErrorText="ZIP Code is required!" />
                            </ValidationSettings>
                        </dx:BootstrapTextBox>
                    </div>
                    <div class="col-md-6 col-lg-4">
                        <dx:BootstrapTextBox ID="txtRegionLocation" runat="server" Caption="What region are you located in?">
                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                            <ValidationSettings ValidationGroup="vgGeneralInfo" ErrorDisplayMode="ImageWithText">
                                <RequiredField IsRequired="false" ErrorText="Region Location is required!" />
                            </ValidationSettings>
                        </dx:BootstrapTextBox>
                    </div>
                </div>

                </div>
                <div class="card-footer">
                    <div class="center-content">
                        <uc:Submit ID="submitGeneralInfo" runat="server" ShowCancelButton="false"
                            SubmitButtonText="Save General Info" ControlCssClass="center-content"
                            ValidationGroup="vgGeneralInfo" OnSubmitClick="submitGeneralInfo_Click"
                            OnValidationFailed="submitGeneralInfo_ValidationFailed" />
                    </div>
                </div>
            </div>
            <div class="card bg-light mt-4">
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
                                <dx:BootstrapTextBox ID="txtPhoneNumber" runat="server" Caption="Personal Phone Number" OnValidation="txtPhoneNumber_Validation">
                                    <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                    <MaskSettings Mask="+1 (999) 999-9999" IncludeLiterals="None" ErrorText="Must be a valid phone number!" />
                                    <ValidationSettings ValidationGroup="vgPhone" ErrorDisplayMode="ImageWithText">
                                        <RequiredField IsRequired="true" ErrorText="Must be a valid phone number!" />
                                    </ValidationSettings>
                                </dx:BootstrapTextBox>
                                <div class="mt-2">
                                    <dx:BootstrapButton ID="btnAddPhone" runat="server" OnClick="EditPhone_Click" Text="Add" ValidationGroup="vgPhone" data-validation-group="vgPhone"
                                        SettingsBootstrap-RenderOption="Secondary">
                                        <CssClasses Icon="fas fa-plus" Control="btn-loader" />
                                    </dx:BootstrapButton>
                                    <dx:BootstrapButton ID="btnEditPhone" runat="server" OnClick="EditPhone_Click" Text="Update" ValidationGroup="vgPhone" data-validation-group="vgPhone"
                                        SettingsBootstrap-RenderOption="Secondary">
                                        <CssClasses Icon="fas fa-edit" Control="btn-loader" />
                                    </dx:BootstrapButton>
                                    <dx:BootstrapButton ID="btnRemovePhone" runat="server" OnClick="btnRemovePhone_Click" Text="Remove" SettingsBootstrap-RenderOption="Warning">
                                        <CssClasses Icon="fas fa-minus" Control="btn-loader" />
                                    </dx:BootstrapButton>
                                    <dx:BootstrapButton ID="btnVerifyPhone" runat="server" OnClick="btnVerifyPhone_Click" Text="Verify" ValidationGroup="vgPhone" data-validation-group="vgPhone"
                                        SettingsBootstrap-RenderOption="Primary">
                                        <CssClasses Icon="fas fa-check" Control="btn-loader" />
                                    </dx:BootstrapButton>
                                </div>
                                <button id="btnPhoneHelp" type="button" class="btn btn-link"><i class="fas fa-question-circle"></i>&nbsp;Help</button>
                            </div>
                            <div class="col-md-6 col-lg-4">
                                <dx:BootstrapTextBox ID="txtWorkPhoneNumber" runat="server" Caption="Work Phone Number" OnValidation="txtWorkPhoneNumber_Validation">
                                    <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                    <MaskSettings Mask="+1 (999) 999-9999 \e\x\t\. 999999" IncludeLiterals="None" ErrorText="Must be a valid phone number!" />
                                    <ValidationSettings ValidationGroup="vgWorkPhone" ErrorDisplayMode="ImageWithText">
                                        <RequiredField IsRequired="true" ErrorText="Must be a valid phone number!" />
                                    </ValidationSettings>
                                </dx:BootstrapTextBox>
                                <div class="mt-2">
                                    <dx:BootstrapButton ID="btnAddWorkPhone" runat="server" OnClick="EditWorkPhone_Click" Text="Add" ValidationGroup="vgWorkPhone" data-validation-group="vgWorkPhone"
                                        SettingsBootstrap-RenderOption="Secondary">
                                        <CssClasses Icon="fas fa-plus" Control="btn-loader" />
                                    </dx:BootstrapButton>
                                    <dx:BootstrapButton ID="btnEditWorkPhone" runat="server" OnClick="EditWorkPhone_Click" Text="Update" ValidationGroup="vgWorkPhone" data-validation-group="vgWorkPhone"
                                        SettingsBootstrap-RenderOption="Secondary">
                                        <CssClasses Icon="fas fa-edit" Control="btn-loader" />
                                    </dx:BootstrapButton>
                                    <dx:BootstrapButton ID="btnRemoveWorkPhone" runat="server" OnClick="btnRemoveWorkPhone_Click" Text="Remove" SettingsBootstrap-RenderOption="Warning">
                                        <CssClasses Icon="fas fa-minus" Control="btn-loader" />
                                    </dx:BootstrapButton>
                                </div>
                                <button id="btnWorkPhoneHelp" type="button" class="btn btn-link"><i class="fas fa-question-circle"></i>&nbsp;Help</button>
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
                                <button id="btnTwoFactorHelp" type="button" class="btn btn-link"><i class="fas fa-question-circle"></i>&nbsp;Help</button>
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
                            <div class="col-lg-4">
                                <dx:BootstrapComboBox ID="ddCancelConfirmation" runat="server" Caption="Cancel Confirmations" NullText="--Select--"
                                    TextField="Description" ValueField="CodeCustomizationOptionValuePK" ValueType="System.Int32"
                                    IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                    <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                    <ValidationSettings ValidationGroup="vgCustomizationOptions" ErrorDisplayMode="ImageWithText">
                                        <RequiredField IsRequired="true" ErrorText="Cancel Confirmations selection is required!" />
                                    </ValidationSettings>
                                </dx:BootstrapComboBox>
                                <button id="btnCancelConfirmationHelp" type="button" class="btn btn-link"><i class="fas fa-question-circle"></i>&nbsp;Help</button>
                            </div>
                            <div class="col-lg-4">
                                <dx:BootstrapComboBox ID="ddFireworks" runat="server" Caption="Fireworks" NullText="--Select--"
                                    TextField="Description" ValueField="CodeCustomizationOptionValuePK" ValueType="System.Int32"
                                    IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                    <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                    <ValidationSettings ValidationGroup="vgCustomizationOptions" ErrorDisplayMode="ImageWithText">
                                        <RequiredField IsRequired="true" ErrorText="Fireworks selection is required!" />
                                    </ValidationSettings>
                                </dx:BootstrapComboBox>
                                <button id="btnFireworksHelp" type="button" class="btn btn-link"><i class="fas fa-question-circle"></i>&nbsp;Help</button>
                            </div>
                            <div class="col-lg-4">
                                <dx:BootstrapComboBox ID="ddWelcomeMessage" runat="server" Caption="Welcome Message" NullText="--Select--"
                                    TextField="Description" ValueField="CodeCustomizationOptionValuePK" ValueType="System.Int32"
                                    IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                    <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                    <ValidationSettings ValidationGroup="vgCustomizationOptions" ErrorDisplayMode="ImageWithText">
                                        <RequiredField IsRequired="true" ErrorText="Welcome Message selection is required!" />
                                    </ValidationSettings>
                                </dx:BootstrapComboBox>
                                <button id="btnWelcomeMessageHelp" type="button" class="btn btn-link"><i class="fas fa-question-circle"></i>&nbsp;Help</button>
                            </div>
                        </div>
                    </div>
                    <div class="card-footer">
                        <div class="center-content">
                            <uc:Submit ID="submitCustomizationOptions" runat="server" ShowCancelButton="false"
                                SubmitButtonText="Save Customization Options" ControlCssClass="center-content"
                                ValidationGroup="vgCustomizationOptions" OnSubmitClick="submitCustomizationOptions_Click"
                                OnValidationFailed="submitCustomizationOptions_ValidationFailed" />
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
