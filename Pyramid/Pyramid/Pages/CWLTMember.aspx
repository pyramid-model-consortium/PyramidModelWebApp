<%@ Page Title="CWLT Member" Language="C#" MasterPageFile="~/MasterPages/Dashboard.master" AutoEventWireup="true" CodeBehind="CWLTMember.aspx.cs" Inherits="Pyramid.Pages.CWLTMember" %>

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
            $('#lnkCWLTDashboard').addClass('active');

            //Allow date format for DataTables sorting
            $.fn.dataTable.moment('MM/DD/YYYY');

            //Show/hide the view only fields
            setViewOnlyVisibility();

            //Initialize the datatable
            if (!$.fn.dataTable.isDataTable('#tblAgencyAssignments')) {
                $('#tblAgencyAssignments').DataTable({
                    responsive: {
                        details: {
                            type: 'column'
                        }
                    },
                    columnDefs: [
                        { orderable: false, targets: [4] },
                        { className: 'control', orderable: false, targets: 0 }
                    ],
                    order: [[2, 'desc']],
                    stateSave: true,
                    stateDuration: 60,
                    pageLength: 10,
                    dom: 'frtp',
                    language: {
                        searchPlaceholder: "Enter text to search...",
                        search: "",
                    }
                });
            }
            $('.dataTables_filter input').removeClass('form-control-sm');

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

        //This function controls whether or not the specify field for the gender is shown
        //based on the value in the ddGender Combo Box
        function showHideGenderSpecify() {
            //Get the gender
            var gender = ddGender.GetText();

            //If the gender is prefer to self-describe, show the specify div
            if (gender.toLowerCase() == 'prefer to self-describe') {
                $('#divGenderSpecify').slideDown();
            }
            else {
                //The leave reason is not prefer to self-describe, clear the specify text box and hide the specify div
                txtGenderSpecify.SetValue('');
                $('#divGenderSpecify').slideUp();
            }
        }
    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Label ID="lblPageTitle" runat="server" Text="Community Leadership Team Member" CssClass="h2"></asp:Label>
    <hr />
    <asp:HiddenField ID="hfViewOnly" runat="server" Value="False" />
    <asp:UpdatePanel ID="upMessaging" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <uc:Messaging ID="msgSys" runat="server" />
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="lbAddAgencyAssignment" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="repeatAgencyAssignments" />
            <asp:AsyncPostBackTrigger ControlID="lbDeleteAgencyAssignment" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="submitAgencyAssignment" />
            <asp:AsyncPostBackTrigger ControlID="btnPrintPreview" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="submitCWLTMember" />
        </Triggers>
    </asp:UpdatePanel>
    <asp:UpdatePanel ID="upBasicInfo" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <ContentTemplate>
            <asp:HiddenField ID="hfCWLTMemberPK" runat="server" Value="" />
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
                                        SettingsBootstrap-RenderOption="primary" ValidationGroup="vgCWLTMember" data-validation-group="vgCWLTMember">
                                        <CssClasses Icon="fas fa-print" Control="float-right btn-loader" />
                                    </dx:BootstrapButton>
                                </div>
                            </div>
                        </div>
                        <div class="card-body">
                            <div class="row">
                                <div class="col-lg-6">
                                    <label>Hub: </label>
                                    <asp:Label ID="lblHub" runat="server" Text=""></asp:Label>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-4">
                                    <div class="form-group">
                                        <dx:BootstrapTextBox ID="txtFirstName" runat="server" Caption="First Name" MaxLength="250">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ValidationSettings ValidationGroup="vgCWLTMember" ErrorDisplayMode="ImageWithText">
                                                <RequiredField IsRequired="true" ErrorText="First Name is required!" />
                                            </ValidationSettings>
                                        </dx:BootstrapTextBox>
                                    </div>
                                </div>
                                <div class="col-lg-4">
                                    <div class="form-group">
                                        <dx:BootstrapTextBox ID="txtLastName" runat="server" Caption="Last Name" MaxLength="250">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ValidationSettings ValidationGroup="vgCWLTMember" ErrorDisplayMode="ImageWithText">
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
                                            <ValidationSettings ValidationGroup="vgCWLTMember" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
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
                                            <ValidationSettings ValidationGroup="vgCWLTMember" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                <RequiredField IsRequired="true" ErrorText="Current Email is required!" />
                                            </ValidationSettings>
                                        </dx:BootstrapTextBox>
                                    </div>
                                </div>
                                <div class="col-lg-4">
                                    <dx:BootstrapTextBox ID="txtPhoneNumber" runat="server" Caption="Phone Number" MaxLength="40" OnValidation="txtPhoneNumber_Validation">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <MaskSettings Mask="+1 (999) 999-9999 \e\x\t\. 999999" IncludeLiterals="None" ErrorText="Must be a valid phone number!" />
                                        <ValidationSettings ValidationGroup="vgCWLTMember" ErrorDisplayMode="ImageWithText">
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
                                            <ValidationSettings ValidationGroup="vgCWLTMember" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
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
                                            <ValidationSettings ValidationGroup="vgCWLTMember" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                <RequiredField IsRequired="false" ErrorText="Leave Date is required!" />
                                            </ValidationSettings>
                                        </dx:BootstrapDateEdit>
                                    </div>
                                </div>
                            </div>
                            <div id="divDemographicInfo" runat="server" visible="false">
                                <div class="row">
                                    <div class="col-lg-3">
                                        <div class="form-group">
                                            <dx:BootstrapComboBox ID="ddGender" runat="server" Caption="Gender" NullText="--Select--"
                                                TextField="Description" ValueField="CodeGenderPK" ValueType="System.Int32" AllowNull="true"
                                                IncrementalFilteringMode="StartsWith" AllowMouseWheel="false"
                                                ClearButton-DisplayMode="Always" ClientInstanceName="ddGender">
                                                <ClientSideEvents Init="showHideGenderSpecify" SelectedIndexChanged="showHideGenderSpecify" />
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <ValidationSettings ValidationGroup="vgCWLTMember" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="false" ErrorText="Gender is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                            <div id="divGenderSpecify" style="display: none">
                                                <dx:BootstrapTextBox ID="txtGenderSpecify" runat="server" Caption="Specify Gender" MaxLength="100"
                                                    OnValidation="txtGenderSpecify_Validation" ClientInstanceName="txtGenderSpecify">
                                                    <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                    <ValidationSettings ValidationGroup="vgCWLTMember" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                    </ValidationSettings>
                                                </dx:BootstrapTextBox>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="col-lg-3">
                                        <div class="form-group">
                                            <dx:BootstrapComboBox ID="ddEthnicity" runat="server" Caption="Ethnicity" NullText="--Select--"
                                                TextField="Description" ValueField="CodeEthnicityPK" ValueType="System.Int32" AllowNull="true"
                                                IncrementalFilteringMode="StartsWith" AllowMouseWheel="false" ClearButton-DisplayMode="Always">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <ValidationSettings ValidationGroup="vgCWLTMember" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="false" ErrorText="Ethnicity is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </div>
                                    </div>
                                    <div class="col-lg-3">
                                        <div class="form-group">
                                            <dx:BootstrapComboBox ID="ddRace" runat="server" Caption="Race" NullText="--Select--"
                                                TextField="Description" ValueField="CodeRacePK" ValueType="System.Int32" AllowNull="true" 
                                                IncrementalFilteringMode="StartsWith" AllowMouseWheel="false" ClearButton-DisplayMode="Always">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <ValidationSettings ValidationGroup="vgCWLTMember" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="false" ErrorText="Race is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </div>
                                    </div>
                                    <div class="col-lg-3">
                                        <div class="form-group">
                                            <dx:BootstrapComboBox ID="ddHouseholdIncome" runat="server" Caption="Household Income" NullText="--Select--"
                                                TextField="Description" ValueField="CodeHouseholdIncomePK" ValueType="System.Int32" AllowNull="true"
                                                IncrementalFilteringMode="StartsWith" AllowMouseWheel="false" ClearButton-DisplayMode="Always">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <ValidationSettings ValidationGroup="vgCWLTMember" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="false" ErrorText="Household Income is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="submitCWLTMember" />
        </Triggers>
    </asp:UpdatePanel>
    <div id="divEditOnly" runat="server" visible="false">
        <div class="row">
            <div class="col-xl-12">
                <asp:HiddenField ID="hfDeleteAgencyAssignmentPK" runat="server" Value="0" />
                <asp:UpdatePanel ID="upAgencyAssignment" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                    <ContentTemplate>
                        <div class="card bg-light">
                            <div class="card-header">
                                Agency Assignments
                                <asp:LinkButton ID="lbAddAgencyAssignment" runat="server" CssClass="btn btn-loader btn-primary float-right hide-on-view hidden" OnClick="lbAddAgencyAssignment_Click"><i class="fas fa-plus"></i> Add New Assignment</asp:LinkButton>
                            </div>
                            <div class="card-body">
                                <div class="row">
                                    <div class="col-lg-12">
                                        <label>All Agency Assignments for this CWLT Member</label>
                                        <asp:Repeater ID="repeatAgencyAssignments" runat="server" ItemType="Pyramid.Models.CWLTMemberAgencyAssignment">
                                            <HeaderTemplate>
                                                <table id="tblAgencyAssignments" class="table table-striped table-bordered table-hover">
                                                    <thead>
                                                        <tr>
                                                            <th data-priority="1"></th>
                                                            <th data-priority="3">Agency</th>
                                                            <th data-priority="4">Start Date</th>
                                                            <th data-priority="5">End Date</th>
                                                            <th data-priority="2"></th>
                                                        </tr>
                                                    </thead>
                                                    <tbody>
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <tr>
                                                    <td></td>
                                                    <td><%# Item.CWLTAgency.Name %></td>
                                                    <td><%# Item.StartDate.ToString("MM/dd/yyyy") %></td>
                                                    <td class="leave-date"><%# (Item.EndDate.HasValue ? Item.EndDate.Value.ToString("MM/dd/yyyy") : "") %></td>
                                                    <td class="text-center">
                                                        <div class="btn-group">
                                                            <button type="button" class="btn btn-secondary dropdown-toggle hide-on-view hidden" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                                Actions
                                                            </button>
                                                            <div class="dropdown-menu dropdown-menu-right">
                                                                <asp:LinkButton ID="lbEditAgencyAssignment" runat="server" CssClass="dropdown-item" OnClick="lbEditAgencyAssignment_Click"><i class="fas fa-edit"></i>&nbsp;Edit</asp:LinkButton>
                                                                <button class="dropdown-item delete-gridview" data-pk='<%# Item.CWLTMemberAgencyAssignmentPK %>' data-hf="hfDeleteAgencyAssignmentPK" data-target="#divDeleteAgencyAssignmentModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                            </div>
                                                        </div>
                                                        <!-- Need to use labels so that values are maintained after postback (inputs get cleared because of an interaction with DataTables and the repeater) -->
                                                        <asp:Label ID="lblAgencyFK" runat="server" Visible="false" Text='<%# Item.CWLTAgencyFK %>'></asp:Label>
                                                        <asp:Label ID="lblAgencyAssignmentPK" runat="server" Visible="false" Text='<%# Item.CWLTMemberAgencyAssignmentPK %>'></asp:Label>
                                                    </td>
                                                </tr>
                                            </ItemTemplate>
                                            <FooterTemplate>
                                                </tbody>
                                                </table>
                                            </FooterTemplate>
                                        </asp:Repeater>
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-lg-12">
                                        <div id="divAddEditAgencyAssignment" runat="server" class="card mt-2" visible="false">
                                            <div class="card-header">
                                                <asp:Label ID="lblAddEditAgencyAssignment" runat="server" Text=""></asp:Label>
                                            </div>
                                            <div class="card-body">
                                                <div class="row">
                                                    <div class="col-lg-4">
                                                        <div class="form-group">
                                                            <dx:BootstrapComboBox ID="ddAssignmentAgency" runat="server" Caption="Agency" NullText="--Select--" ValueType="System.Int32"
                                                                TextField="Name" ValueField="CWLTAgencyPK" 
                                                                IncrementalFilteringMode="Contains" AllowMouseWheel="false"
                                                                ClientInstanceName="ddAssignmentAgency">
                                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                                <ValidationSettings ValidationGroup="vgAgencyAssignment" ErrorDisplayMode="ImageWithText">
                                                                    <RequiredField IsRequired="true" ErrorText="Agency is required!" />
                                                                </ValidationSettings>
                                                            </dx:BootstrapComboBox>
                                                        </div>
                                                    </div>
                                                    <div class="col-lg-4">
                                                        <div class="form-group">
                                                            <dx:BootstrapDateEdit ID="deAssignmentStartDate" runat="server" Caption="Start Date"
                                                                EditFormat="Date" EditFormatString="MM/dd/yyyy"
                                                                UseMaskBehavior="true" ClientInstanceName="deAssignmentStartDate" 
                                                                OnValidation="deAssignmentStartDate_Validation" 
                                                                AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900">
                                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                                <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                                                <CalendarProperties ShowClearButton="true"></CalendarProperties>
                                                                <ValidationSettings ValidationGroup="vgAgencyAssignment" ErrorDisplayMode="ImageWithText">
                                                                    <RequiredField IsRequired="true" ErrorText="Start Date is required!" />
                                                                </ValidationSettings>
                                                            </dx:BootstrapDateEdit>
                                                        </div>
                                                    </div>
                                                    <div class="col-lg-4">
                                                        <div class="form-group">
                                                            <dx:BootstrapDateEdit ID="deAssignmentEndDate" runat="server" Caption="End Date" EditFormat="Date" AllowNull="true"
                                                                EditFormatString="MM/dd/yyyy" UseMaskBehavior="true"
                                                                ClientInstanceName="deEndDate" 
                                                                OnValidation="deAssignmentEndDate_Validation" 
                                                                AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900">
                                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                                <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                                                <CalendarProperties ShowClearButton="true"></CalendarProperties>
                                                                <ValidationSettings ValidationGroup="vgAgencyAssignment" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                                    <RequiredField IsRequired="false" ErrorText="End Date is required!" />
                                                                </ValidationSettings>
                                                            </dx:BootstrapDateEdit>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="card-footer">
                                                <div class="center-content">
                                                    <asp:HiddenField ID="hfAddEditAgencyAssignmentPK" runat="server" Value="0" />
                                                    <uc:Submit ID="submitAgencyAssignment" runat="server" 
                                                        ValidationGroup="vgAgencyAssignment"
                                                        ControlCssClass="center-content"
                                                        OnSubmitClick="submitAgencyAssignment_Click" 
                                                        OnCancelClick="submitAgencyAssignment_CancelClick" 
                                                        OnValidationFailed="submitAgencyAssignment_ValidationFailed" />
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="lbDeleteAgencyAssignment" EventName="Click" />
                    </Triggers>
                </asp:UpdatePanel>
            </div>
        </div>
    </div>
    <div class="page-footer">
        <uc:Submit ID="submitCWLTMember" runat="server" ValidationGroup="vgCWLTMember"
            ControlCssClass="center-content"
            OnSubmitClick="submitCWLTMember_Click" OnCancelClick="submitCWLTMember_CancelClick" 
            OnValidationFailed="submitCWLTMember_ValidationFailed" />
    </div>
    <div class="modal" id="divDeleteAgencyAssignmentModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete Agency Assignment</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this agency assignment?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteAgencyAssignment" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteAgencyAssignmentModal" OnClick="lbDeleteAgencyAssignment_Click"><i class="fas fa-check"></i> Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
</asp:Content>