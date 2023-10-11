<%@ Page Title="Program Leadership Team Member" Language="C#" MasterPageFile="~/MasterPages/Dashboard.master" AutoEventWireup="true" CodeBehind="PLTMember.aspx.cs" Inherits="Pyramid.Pages.PLTMember" %>

<%@ Register Assembly="DevExpress.Web.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Data.Linq" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.Bootstrap.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>
<%@ Register TagPrefix="uc" TagName="Submit" Src="~/User_Controls/Submit.ascx" %>
<%@ Register TagPrefix="uc" TagName="Messaging" Src="~/User_Controls/MessagingSystem.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ScriptContent" runat="server">
    <script type="text/javascript">
        $(document).ready(function () {
            //Run the initial page init
            initializePage();

            //Run the page init after every AJAX load
            var requestManager = Sys.WebForms.PageRequestManager.getInstance();
            requestManager.add_endRequest(initializePage);
        });

        //Initializes the page
        function initializePage() {
            //Highlight the correct dashboard link
            $('#lnkPLTDashboard').addClass('active');

            //Allow date format for DataTables sorting
            $.fn.dataTable.moment('MM/DD/YYYY');

            //Show/hide the view only fields
            setViewOnlyVisibility();

            //Set up the click events for the help buttons
            $('#btnIDNumberHelp').popover({
                trigger: 'hover focus',
                title: 'Help',
                html: true,
                content: 'This field is not required, but will be automatically populated in the format of SID-[number] if you do not enter an ID number. ' +
                    'You can either leave the system-generated ID number there or create your own ID number.'
            });
        }

        function setViewOnlyVisibility() {
            //Hide controls if this is a view
            var isView = $('[ID$="hfViewOnly"]').val();
            if (isView == 'True') {
                $('.hide-on-view').addClass('hidden');
            }
            else {
                $('.hide-on-view').removeClass('hidden');
            }
        }
    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Label ID="lblPageTitle" runat="server" Text="Program Leadership Team Member" CssClass="h2"></asp:Label>
    <hr />
    <asp:HiddenField ID="hfViewOnly" runat="server" Value="False" />
    <asp:UpdatePanel ID="upMessaging" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <uc:Messaging ID="msgSys" runat="server" />
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="btnPrintPreview" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="submitPLTMember" />
        </Triggers>
    </asp:UpdatePanel>
    <asp:UpdatePanel ID="upBasicInfo" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <ContentTemplate>
            <asp:HiddenField ID="hfPLTMemberPK" runat="server" Value="" />
            <div class="row">
                <div class="col-lg-12">
                    <div class="card bg-light">
                        <div class="card-header">
                            <div class="row">
                                <div class="col-lg-8">
                                    Basic Information
                                </div>
                                <div class="col-lg-4">
                                    <dx:BootstrapButton ID="btnPrintPreview" runat="server" Text="Save and Download/Print" OnClick="btnPrintPreview_Click"
                                        SettingsBootstrap-RenderOption="primary" ValidationGroup="vgPLTMember" data-validation-group="vgPLTMember">
                                        <CssClasses Icon="fas fa-print" Control="float-right btn-loader" />
                                    </dx:BootstrapButton>
                                </div>
                            </div>
                        </div>
                        <div class="card-body">
                            <div class="row">
                                <div class="col-lg-4">
                                    <dx:BootstrapComboBox ID="ddProgram" runat="server" Caption="Program" NullText="--Select--"
                                        TextField="ProgramName" ValueField="ProgramPK" ValueType="System.Int32" AllowNull="true"
                                        IncrementalFilteringMode="Contains" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgPLTMember" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                            <RequiredField IsRequired="true" ErrorText="Program is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-4">
                                    <div class="form-group">
                                        <dx:BootstrapTextBox ID="txtFirstName" runat="server" Caption="First Name" MaxLength="250">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ValidationSettings ValidationGroup="vgPLTMember" ErrorDisplayMode="ImageWithText">
                                                <RequiredField IsRequired="true" ErrorText="First Name is required!" />
                                            </ValidationSettings>
                                        </dx:BootstrapTextBox>
                                    </div>
                                </div>
                                <div class="col-lg-4">
                                    <div class="form-group">
                                        <dx:BootstrapTextBox ID="txtLastName" runat="server" Caption="Last Name" MaxLength="250">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ValidationSettings ValidationGroup="vgPLTMember" ErrorDisplayMode="ImageWithText">
                                                <RequiredField IsRequired="true" ErrorText="Last Name is required!" />
                                            </ValidationSettings>
                                        </dx:BootstrapTextBox>
                                    </div>
                                </div>
                                <div class="col-lg-4">
                                    <div class="form-group">
                                        <dx:BootstrapTextBox ID="txtIDNumber" runat="server" Caption="ID Number" MaxLength="100"
                                            OnValidation="txtIDNumber_Validation">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ValidationSettings ValidationGroup="vgPLTMember" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                <RequiredField IsRequired="false" ErrorText="ID Number is required!" />
                                            </ValidationSettings>
                                        </dx:BootstrapTextBox>
                                        <button id="btnIDNumberHelp" type="button" class="btn btn-link p-0"><i class="fas fa-question-circle"></i>&nbsp;Help</button>
                                    </div>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-4">
                                    <div class="form-group">
                                        <dx:BootstrapTextBox ID="txtEmailAddress" runat="server" Caption="Current Email" MaxLength="256"
                                            ClientInstanceName="txtEmailAddress" OnValidation="txtEmailAddress_Validation">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ValidationSettings ValidationGroup="vgPLTMember" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                <RequiredField IsRequired="true" ErrorText="Current Email is required!" />
                                            </ValidationSettings>
                                        </dx:BootstrapTextBox>
                                    </div>
                                </div>
                                <div class="col-lg-4">
                                    <dx:BootstrapTextBox ID="txtPhoneNumber" runat="server" Caption="Phone Number" MaxLength="40" OnValidation="txtPhoneNumber_Validation">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <MaskSettings Mask="+1 (999) 999-9999 \e\x\t\. 999999" IncludeLiterals="None" ErrorText="Must be a valid phone number!" />
                                        <ValidationSettings ValidationGroup="vgPLTMember" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="false" ErrorText="Phone Number is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapTextBox>
                                </div>
                                <div class="col-lg-4">
                                    <dx:BootstrapTagBox ID="tbRoles" runat="server" Caption="Roles"
                                        AllowCustomTags="false" TextField="Description" ValueField="CodeTeamPositionPK">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgPLTMember" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="false" ErrorText="At least one role must be selected!" />
                                        </ValidationSettings>
                                    </dx:BootstrapTagBox>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-4">
                                    <div class="form-group">
                                        <dx:BootstrapDateEdit ID="deStartDate" runat="server" Caption="Start Date" EditFormat="Date" EditFormatString="MM/dd/yyyy"
                                            UseMaskBehavior="true" AllowMouseWheel="false" ClientInstanceName="deStartDate"
                                            OnValidation="deStartDate_Validation" PickerDisplayMode="Calendar" MinDate="01/01/1900">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                            <CalendarProperties ShowClearButton="true"></CalendarProperties>
                                            <ValidationSettings ValidationGroup="vgPLTMember" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                <RequiredField IsRequired="true" ErrorText="Start Date is required!" />
                                            </ValidationSettings>
                                        </dx:BootstrapDateEdit>
                                    </div>
                                </div>
                                <div class="col-lg-4">
                                    <div class="form-group">
                                        <dx:BootstrapDateEdit ID="deLeaveDate" runat="server" Caption="Leave Date" AllowNull="true"
                                            EditFormat="Date" EditFormatString="MM/dd/yyyy" UseMaskBehavior="true"
                                            ClientInstanceName="deLeaveDate" 
                                            OnValidation="deLeaveDate_Validation" AllowMouseWheel="false" 
                                            PickerDisplayMode="Calendar" MinDate="01/01/1900">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                            <CalendarProperties ShowClearButton="true"></CalendarProperties>
                                            <ValidationSettings ValidationGroup="vgPLTMember" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                <RequiredField IsRequired="false" ErrorText="Leave Date is required!" />
                                            </ValidationSettings>
                                        </dx:BootstrapDateEdit>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="submitPLTMember" />
        </Triggers>
    </asp:UpdatePanel>
    <div class="page-footer">
        <uc:Submit ID="submitPLTMember" runat="server" ValidationGroup="vgPLTMember"
            ControlCssClass="center-content"
            OnSubmitClick="submitPLTMember_Click" OnCancelClick="submitPLTMember_CancelClick" 
            OnValidationFailed="submitPLTMember_ValidationFailed" />
    </div>
</asp:Content>
