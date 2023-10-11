<%@ Page Title="CWLT Action Plan" Language="C#" MasterPageFile="~/MasterPages/Dashboard.master" AutoEventWireup="true" CodeBehind="CWLTActionPlan.aspx.cs" Inherits="Pyramid.Pages.CWLTActionPlan" %>

<%@ Register Assembly="DevExpress.Web.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Data.Linq" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.Bootstrap.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>
<%@ Register TagPrefix="uc" TagName="Messaging" Src="~/User_Controls/MessagingSystem.ascx" %>
<%@ Register TagPrefix="uc" TagName="Submit" Src="~/User_Controls/Submit.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ScriptContent" runat="server">
    <script>
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
            $('[ID$="lnkCWLTDashboard"]').addClass('active');

            //Show/hide the view only fields
            setViewOnlyVisibility();

            //Set up the click events for the help buttons
            $('#btnLeadershipCoachHelp').popover({
                trigger: 'hover focus',
                title: 'Help',
                html: true,
                content: 'In order for a leadership coach to appear in this list, they must have ' +
                    'an account in PIDS with a Hub Leadership Coach role associated with the selected hub.'
            });
            $('#btnHubCoordinatorHelp').popover({
                trigger: 'hover focus',
                title: 'Help',
                html: true,
                content: 'This list contains Community Leadership Team Members that are active during the timeframe of the action plan.'
            });

            //Allow date format for DataTables sorting
            $.fn.dataTable.moment('MM/DD/YYYY');

            //Initialize the datatables
            if (!$.fn.dataTable.isDataTable('#tblLeadershipTeamMembers')) {
                $('#tblLeadershipTeamMembers').DataTable({
                    responsive: {
                        details: {
                            type: 'column'
                        }
                    },
                    columnDefs: [
                        { className: 'control', orderable: false, targets: 0 }
                    ],
                    order: [[1, 'asc']],
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
            if (!$.fn.dataTable.isDataTable('#tblMeetings')) {
                $('#tblMeetings').DataTable({
                    responsive: {
                        details: {
                            type: 'column'
                        }
                    },
                    columnDefs: [
                        { orderable: false, targets: [4] },
                        { className: 'control', orderable: false, targets: 0 }
                    ],
                    order: [[1, 'asc']],
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
            if (!$.fn.dataTable.isDataTable('#tblLeadershipCoachSchedule')) {
                $('#tblLeadershipCoachSchedule').DataTable({
                    responsive: {
                        details: {
                            type: 'column'
                        }
                    },
                    columnDefs: [
                        { orderable: false, targets: [5,6,7,8,9,10,11,12,13,14,15,16] },
                        { className: 'control', orderable: false, targets: 0 }
                    ],
                    order: [[3, 'asc']],
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
            if (!$.fn.dataTable.isDataTable('#tblGroundRules')) {
                $('#tblGroundRules').DataTable({
                    responsive: {
                        details: {
                            type: 'column'
                        }
                    },
                    columnDefs: [
                        { orderable: false, targets: [3] },
                        { className: 'control', orderable: false, targets: 0 }
                    ],
                    order: [[1, 'asc']],
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
            if (!$.fn.dataTable.isDataTable('#tblBOQIndicators')) {
                $('#tblBOQIndicators').DataTable({
                    responsive: {
                        details: {
                            type: 'column'
                        }
                    },
                    columnDefs: [
                        { className: 'control', orderable: false, targets: 0 }
                    ],
                    order: [[2, 'asc']],
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
            if (!$.fn.dataTable.isDataTable('#tblActionSteps')) {
                $('#tblActionSteps').DataTable({
                    responsive: {
                        details: {
                            type: 'column'
                        }
                    },
                    columnDefs: [
                        { orderable: false, targets: [8] },
                        { className: 'control', orderable: false, targets: 0 }
                    ],
                    order: [[2, 'asc']],
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
            if (!$.fn.dataTable.isDataTable('#tblActionStepStatuses')) {
                $('#tblActionStepStatuses').DataTable({
                    responsive: {
                        details: {
                            type: 'column'
                        }
                    },
                    columnDefs: [
                        { orderable: false, targets: [3] },
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

        function showHideLeadershipCoachDivs() {
            //Determine if a leadership coach is involved
            var isLeadershipCoachInvolved = ddIsLeadershipCoachInvolved.GetValue();

            //Show/hide the leadership coach sections
            if (isLeadershipCoachInvolved == true) {
                $('.leadership-coach-section').removeClass('hidden');
            }
            else {
                $('.leadership-coach-section').addClass('hidden');
            }
        }

        function setReviewRequiredDisplay() {
            var displayReviewSections = $('[ID$="hfDisplayReviewSections"]').val();

            if (displayReviewSections === 'True') {
                //Show the pre-fill warning
                $('#divPreFillWarning').removeClass('hidden');

                $('.review-required-div').each(function () {
                    //Get the div and controls
                    var currentReviewDiv = $(this);
                    var reviewedCheckBox = currentReviewDiv.children('.review-required-check-box');
                    var iconControl = currentReviewDiv.children('.review-required-icon');
                    var textControl = currentReviewDiv.children('.review-required-text');

                    //Show the div
                    currentReviewDiv.removeClass('hidden');

                    //Get the DevEx checkbox
                    var chkReviewed = ASPxClientControl.Cast(reviewedCheckBox.attr('id'));

                    //Get the value in the check box
                    var isReviewed = chkReviewed.GetChecked();

                    if (isReviewed === true) {
                        iconControl.removeClass('fas fa-exclamation-circle').addClass('fas fa-check');
                        textControl.html('Successfully reviewed!');
                        currentReviewDiv.removeClass('alert alert-warning').addClass('alert alert-success');
                    }
                    else {
                        iconControl.removeClass('fas fa-check').addClass('fas fa-exclamation-circle');
                        textControl.html('Because this section was pre-filled from a previous action plan, you need to review it and confirm it is correct. ' +
                            'Once you have fully reviewed this section and made any necessary modifications, please check the box below.');
                        currentReviewDiv.removeClass('alert alert-success').addClass('alert alert-warning');
                    }
                });
            }
            else {
                //Hide the pre-fill warning and review divs
                $('#divPreFillWarning').addClass('hidden');
                $('.review-required-div').addClass('hidden');
            }
        }
    </script>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Label ID="lblPageTitle" runat="server" Text="" CssClass="h2"></asp:Label>
    <hr />
    <asp:HiddenField ID="hfViewOnly" runat="server" Value="False" />
    <asp:HiddenField ID="hfDisplayReviewSections" runat="server" Value="False" />
    <asp:UpdatePanel ID="upMessaging" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <uc:Messaging ID="msgSys" runat="server" />
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="lbAddMeeting" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="repeatMeetings" />
            <asp:AsyncPostBackTrigger ControlID="lbDeleteMeeting" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="submitMeeting" />
            <asp:AsyncPostBackTrigger ControlID="lbAddGroundRule" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="repeatGroundRules" />
            <asp:AsyncPostBackTrigger ControlID="lbDeleteGroundRule" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="submitGroundRule" />
            <asp:AsyncPostBackTrigger ControlID="lbAddActionStep" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="repeatActionSteps" />
            <asp:AsyncPostBackTrigger ControlID="lbDeleteActionStep" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="submitActionStep" />
            <asp:AsyncPostBackTrigger ControlID="lbAddActionStepStatus" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="repeatActionStepStatuses" />
            <asp:AsyncPostBackTrigger ControlID="lbDeleteActionStepStatus" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="submitActionStepStatus" />
            <asp:AsyncPostBackTrigger ControlID="btnPrintPreview" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="submitActionPlan" />
        </Triggers>
    </asp:UpdatePanel>
    <div id="divPreFillWarning" class="alert alert-warning hidden">
        <i class="fas fa-exclamation-circle"></i>&nbsp;Because this action plan was pre-filled from a previous action plan, several sections must be reviewed.  Once all sections are reviewed and the action plan is saved, this warning and the review indicators will disappear.
    </div>
    <asp:UpdatePanel ID="upBasicInfo" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <ContentTemplate>
            <asp:HiddenField ID="hfCWLTActionPlanPK" runat="server" Value="" />
            <div class="row">
                <div class="col-lg-12">
                    <div class="card bg-light bold-border">
                        <div class="card-header">
                            <div class="row">
                                <div class="col-lg-8">
                                    Basic Information (Section 1)
                                </div>
                                <div class="col-lg-4">
                                    <dx:BootstrapButton ID="btnPrintPreview" runat="server" Text="Save and Download/Print" OnClick="btnPrintPreview_Click"
                                        SettingsBootstrap-RenderOption="primary" ValidationGroup="vgActionPlan" data-validation-group="vgActionPlan">
                                        <CssClasses Icon="fas fa-print" Control="float-right btn-loader" />
                                    </dx:BootstrapButton>
                                </div>
                            </div>
                        </div>
                        <div class="card-body">
                            <div id="divActionPlanAlert" runat="server" class="alert alert-danger" visible="false">
                                <i class="fas fa-exclamation-triangle"></i>&nbsp;<asp:Label ID="lblActionPlanAlert" runat="server" Text=""></asp:Label>
                            </div>
                            <div class="review-required-div hidden">
                                <i class="review-required-icon"></i>&nbsp;
                                <p class="review-required-text d-inline"></p>
                                <dx:BootstrapCheckBox ID="chkReviewedBasicInfo" runat="server" Text="This section is complete and accurate.">
                                    <CssClasses Control="review-required-check-box" />
                                    <ClientSideEvents Init="setReviewRequiredDisplay" CheckedChanged="setReviewRequiredDisplay" />
                                </dx:BootstrapCheckBox>
                            </div>
                            <div class="row">
                                <div class="col-lg-4">
                                    <dx:BootstrapComboBox ID="ddHub" runat="server" Caption="Hub" NullText="--Select--"
                                        TextField="Name" ValueField="HubPK" ValueType="System.Int32" AllowNull="true"
                                        IncrementalFilteringMode="Contains" AllowMouseWheel="false" AutoPostBack="true"
                                        OnValidation="ddHub_Validation" OnValueChanged="ddHub_ValueChanged">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgActionPlan" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                            <RequiredField IsRequired="true" ErrorText="Hub is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </div>
                                <div class="col-lg-4">
                                    <dx:BootstrapDateEdit ID="deActionPlanStartDate" runat="server" Caption="Action Plan Start Date" EditFormat="Date"
                                        EditFormatString="MM/dd/yyyy" UseMaskBehavior="true" NullText="--Select--"
                                        AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900" AutoPostBack="true"
                                        OnValidation="deActionPlanStartDate_Validation" OnValueChanged="ActionPlanDate_ValueChanged">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                        <ValidationSettings ValidationGroup="vgActionPlan" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Action Plan Start Date is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapDateEdit>
                                </div>
                                <div class="col-lg-4">
                                    <dx:BootstrapDateEdit ID="deActionPlanEndDate" runat="server" Caption="Action Plan End Date" EditFormat="Date"
                                        EditFormatString="MM/dd/yyyy" UseMaskBehavior="true" NullText="--Select--"
                                        AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900" AutoPostBack="true"
                                        OnValidation="deActionPlanEndDate_Validation" OnValueChanged="ActionPlanDate_ValueChanged">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                        <ValidationSettings ValidationGroup="vgActionPlan" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Action Plan End Date is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapDateEdit>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-4">
                                    <dx:BootstrapComboBox ID="ddHubCoordinator" runat="server" Caption="Hub Coordinator" NullText="--Select--" ValueType="System.Int32"
                                        TextField="IDNumberAndName" ValueField="CWLTMemberPK" 
                                        IncrementalFilteringMode="Contains" AllowMouseWheel="false"
                                        ClientInstanceName="ddHubCoordinator" AutoPostBack="true"
                                        OnValueChanged="ddHubCoordinator_ValueChanged">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgActionPlan" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Hub Coordinator is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                    <button id="btnHubCoordinatorHelp" type="button" class="btn btn-link p-0"><i class="fas fa-question-circle"></i>&nbsp;Help</button>
                                </div>
                                <div class="col-lg-4">
                                    <asp:Label runat="server" AssociatedControlID="lblHubCoordinatorEmail" CssClass="d-block col-form-label" Text="Hub Coordinator Email"></asp:Label>
                                    <asp:Label ID="lblHubCoordinatorEmail" runat="server"></asp:Label>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-4">
                                    <dx:BootstrapComboBox ID="ddIsLeadershipCoachInvolved" runat="server" Caption="Is Leadership Coach Involved" 
                                        NullText="--Select--" ValueType="System.Boolean"
                                        IncrementalFilteringMode="Contains" AllowMouseWheel="false"
                                        ClientInstanceName="ddIsLeadershipCoachInvolved">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ClientSideEvents Init="showHideLeadershipCoachDivs" SelectedIndexChanged="showHideLeadershipCoachDivs" />
                                        <Items>
                                            <dx:BootstrapListEditItem Text="Yes" Value="True"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Text="No" Value="False"></dx:BootstrapListEditItem>
                                        </Items>
                                        <ValidationSettings ValidationGroup="vgActionPlan" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Is Leadership Coach Involved is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </div>
                                <div class="col-lg-4 leadership-coach-section hidden">
                                    <dx:BootstrapComboBox ID="ddLeadershipCoach" runat="server" Caption="Primary Leadership Coach" 
                                        NullText="--Select--" ValueType="System.String" AllowNull="true"
                                        TextField="FullName" ValueField="UserName" 
                                        IncrementalFilteringMode="Contains" AllowMouseWheel="false"
                                        ClientInstanceName="ddLeadershipCoach" AutoPostBack="true"
                                        OnValidation="ddLeadershipCoach_Validation" OnValueChanged="ddLeadershipCoach_ValueChanged">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgActionPlan" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="false" ErrorText="Primary Leadership Coach is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                    <button id="btnLeadershipCoachHelp" type="button" class="btn btn-link p-0"><i class="fas fa-question-circle"></i>&nbsp;Help</button>
                                </div>
                                <div class="col-lg-4 leadership-coach-section hidden">
                                    <asp:Label runat="server" AssociatedControlID="lblLeadershipCoachEmail" CssClass="d-block col-form-label" Text="Primary Leadership Coach Email"></asp:Label>
                                    <asp:Label ID="lblLeadershipCoachEmail" runat="server"></asp:Label>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-6">
                                    <dx:BootstrapMemo ID="txtMissionStatement" runat="server" Caption="Mission Statement" MaxLength="5000" Rows="4">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgActionPlan" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Mission Statement is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapMemo>
                                </div>
                                <div class="col-lg-6">
                                    <dx:BootstrapMemo ID="txtAdditionalNotes" runat="server" Caption="Additional Notes/Comments" MaxLength="5000" Rows="4">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgActionPlan" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="false" ErrorText="Additional Notes/Comments is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapMemo>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="submitActionPlan" />
        </Triggers>
    </asp:UpdatePanel>
    <div id="divEditOnly" runat="server" visible="false">
        <div class="row">
            <div class="col-xl-12">
                <asp:UpdatePanel ID="upLeadershipTeamMembers" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                    <ContentTemplate>
                        <div class="card bg-light bold-border">
                            <div class="card-header">
                                Active Leadership Team Members (Section 2)
                            </div>
                            <div class="card-body">
                                <div class="alert alert-primary">
                                    <i class="fas fa-info-circle"></i>&nbsp;This section is automatically filled based on the Hub and action plan timeframe in the Basic Information section.
                                </div>
                                <div class="row">
                                    <div class="col-lg-12">
                                        <label>All active Leadership Team members from the Community Leadership Team dashboard</label>
                                        <asp:Repeater ID="repeatLeadershipTeamMembers" runat="server" ItemType="Pyramid.Models.CWLTMember">
                                            <HeaderTemplate>
                                                <table id="tblLeadershipTeamMembers" class="table table-striped table-bordered table-hover">
                                                    <thead>
                                                        <tr>
                                                            <th data-priority="1"></th>
                                                            <th data-priority="2">Team Member</th>
                                                            <th data-priority="3">Start Date</th>
                                                            <th data-priority="4">Email Address</th>
                                                            <th data-priority="5">Leave Date</th>
                                                            <th data-priority="4">Hub</th>
                                                        </tr>
                                                    </thead>
                                                    <tbody>
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <tr>
                                                    <td></td>
                                                    <td><%# string.Format("({0}) {1} {2}", Item.IDNumber, Item.FirstName, Item.LastName) %></td>
                                                    <td><%# Item.StartDate.ToString("MM/dd/yyyy") %></td>
                                                    <td><%# Item.EmailAddress %></td>
                                                    <td><%# (Item.LeaveDate.HasValue ? Item.LeaveDate.Value.ToString("MM/dd/yyyy") : "") %></td>
                                                    <td><%# Item.Hub.Name %></td>
                                                </tr>
                                            </ItemTemplate>
                                            <FooterTemplate>
                                                </tbody>
                                                </table>
                                            </FooterTemplate>
                                        </asp:Repeater>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="ddHub" />
                        <asp:AsyncPostBackTrigger ControlID="deActionPlanStartDate" />
                        <asp:AsyncPostBackTrigger ControlID="deActionPlanEndDate" />
                    </Triggers>
                </asp:UpdatePanel>
            </div>
        </div>
        <div class="row">
            <div class="col-xl-12">
                <asp:HiddenField ID="hfDeleteMeetingPK" runat="server" Value="0" />
                <asp:UpdatePanel ID="upMeeting" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                    <ContentTemplate>
                        <div class="card bg-light bold-border">
                            <div class="card-header">
                                All Meeting Dates (Section 3)
                                <asp:LinkButton ID="lbAddMeeting" runat="server" CssClass="btn btn-loader btn-primary float-right hide-on-view hidden" OnClick="lbAddMeeting_Click"><i class="fas fa-plus"></i>&nbsp;Add New Meeting Date</asp:LinkButton>
                            </div>
                            <div class="card-body">
                                <div class="row">
                                    <div class="col-lg-12">
                                        <div class="alert alert-primary">
                                            <i class="fas fa-info-circle"></i>&nbsp;This section should be used to record all the Hub Leadership Team meeting dates.
                                        </div>
                                        <label>All meeting dates for the action plan timeframe</label>
                                        <asp:Repeater ID="repeatMeetings" runat="server" ItemType="Pyramid.Models.CWLTActionPlanMeeting">
                                            <HeaderTemplate>
                                                <table id="tblMeetings" class="table table-striped table-bordered table-hover">
                                                    <thead>
                                                        <tr>
                                                            <th data-priority="1"></th>
                                                            <th data-priority="4">Meeting Date</th>
                                                            <th data-priority="5">Leadership Coach Attendance?</th>
                                                            <th data-priority="3">Notes</th>
                                                            <th data-priority="2"></th>
                                                        </tr>
                                                    </thead>
                                                    <tbody>
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <tr>
                                                    <td></td>
                                                    <td><%# Item.MeetingDate.ToString("MM/dd/yyyy") %></td>
                                                    <td><%# (Item.LeadershipCoachAttendance ? "Yes" : "No") %></td>
                                                    <td><%# (Item.MeetingNotes != null && Item.MeetingNotes.Length > 200 ? string.Format("{0}...  [View meeting detials for full notes]", Item.MeetingNotes.Substring(0, 200)) : Item.MeetingNotes) %></td>
                                                    <td class="text-center">
                                                        <div class="btn-group">
                                                            <button type="button" class="btn btn-secondary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                                Actions
                                                            </button>
                                                            <div class="dropdown-menu dropdown-menu-right">
                                                                <asp:LinkButton ID="lbViewMeeting" runat="server" CssClass="dropdown-item" OnClick="lbViewMeeting_Click"><i class="fas fa-list"></i>&nbsp;View Details</asp:LinkButton>
                                                                <asp:LinkButton ID="lbEditMeeting" runat="server" CssClass="dropdown-item hide-on-view" OnClick="lbEditMeeting_Click"><i class="fas fa-edit"></i>&nbsp;Edit</asp:LinkButton>
                                                                <button class="dropdown-item delete-gridview hide-on-view" data-pk='<%# Item.CWLTActionPlanMeetingPK %>' data-hf="hfDeleteMeetingPK" data-target="#divDeleteMeetingModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                            </div>
                                                        </div>
                                                        <asp:Label ID="lblMeetingPK" runat="server" Visible="false" Text='<%# Item.CWLTActionPlanMeetingPK %>'></asp:Label>
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
                                        <div id="divAddEditMeeting" runat="server" class="card mt-2" visible="false">
                                            <div class="card-header">
                                                <asp:Label ID="lblAddEditMeeting" runat="server" Text=""></asp:Label>
                                            </div>
                                            <div class="card-body">
                                                <div class="row">
                                                    <div class="col-lg-4">
                                                        <dx:BootstrapDateEdit ID="deMeetingDate" runat="server" Caption="Meeting Date" EditFormat="Date"
                                                            EditFormatString="MM/dd/yyyy" UseMaskBehavior="true" NullText="--Select--"
                                                            AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900" OnValidation="deMeetingDate_Validation">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                                            <ValidationSettings ValidationGroup="vgMeeting" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="true" ErrorText="Meeting Date is required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapDateEdit>
                                                    </div>
                                                    <div class="col-lg-4">
                                                        <dx:BootstrapComboBox ID="ddMeetingLeadershipCoachAttendance" runat="server" NullText="--Select--" 
                                                            Caption="Leadership Coach Attendance?" ValueType="System.Boolean" IncrementalFilteringMode="StartsWith">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <ValidationSettings ValidationGroup="vgMeeting" ErrorDisplayMode="ImageWithText">
                                                                <RequiredField IsRequired="true" ErrorText="Leadership Coach Attendance? must be answered!" />
                                                            </ValidationSettings>
                                                            <Items>
                                                                <dx:BootstrapListEditItem Text="Yes" Value="True"></dx:BootstrapListEditItem>
                                                                <dx:BootstrapListEditItem Text="No" Value="False"></dx:BootstrapListEditItem>
                                                            </Items>
                                                        </dx:BootstrapComboBox>
                                                    </div>
                                                    <div class="col-lg-4">
                                                        <dx:BootstrapMemo ID="txtMeetingNotes" runat="server" Caption="Notes" MaxLength="3000" Rows="4">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <ValidationSettings ValidationGroup="vgMeeting" ErrorDisplayMode="ImageWithText">
                                                                <RequiredField IsRequired="false" ErrorText="Notes are required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapMemo>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="card-footer">
                                                <div class="center-content">
                                                    <asp:HiddenField ID="hfAddEditMeetingPK" runat="server" Value="0" />
                                                    <uc:Submit ID="submitMeeting" runat="server" 
                                                        ValidationGroup="vgMeeting"
                                                        ControlCssClass="center-content"
                                                        OnSubmitClick="submitMeeting_Click" 
                                                        OnCancelClick="submitMeeting_CancelClick" 
                                                        OnValidationFailed="submitMeeting_ValidationFailed" />
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="lbDeleteMeeting" EventName="Click" />
                    </Triggers>
                </asp:UpdatePanel>
            </div>
        </div>
        <div class="row">
            <div class="col-xl-12">
                <asp:UpdatePanel ID="upLeadershipCoachSchedule" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                    <ContentTemplate>
                        <div class="card bg-light bold-border">
                            <div class="card-header">
                                Meeting Dates Proposed by Leadership Coaches (Section 4)
                            </div>
                            <div class="card-body">
                                <div class="alert alert-primary">
                                    <i class="fas fa-info-circle"></i>&nbsp;This meeting date section is automatically filled based on the Hub and action plan timeframe in the Basic Information section.  The only Leadership Coach schedules that will appear below will be for the selected Hub and action plan timeframe.
                                </div>
                                <div class="row">
                                    <div class="col-lg-12">
                                        <label>All meeting dates from the Leadership Coach dashboard</label>
                                        <asp:Repeater ID="repeatLeadershipCoachSchedule" runat="server" ItemType="Pyramid.Models.HubLCMeetingSchedule">
                                            <HeaderTemplate>
                                                <table id="tblLeadershipCoachSchedule" class="table table-striped table-bordered table-hover">
                                                    <thead>
                                                        <tr>
                                                            <th data-priority="1"></th>
                                                            <th data-priority="4">Hub</th>
                                                            <th data-priority="5">Year</th>
                                                            <th data-priority="2">Leadership Coach</th>
                                                            <th data-priority="3">Total Meetings</th>
                                                            <th data-priority="6">Jan</th>
                                                            <th data-priority="7">Feb</th>
                                                            <th data-priority="8">Mar</th>
                                                            <th data-priority="9">Apr</th>
                                                            <th data-priority="10">May</th>
                                                            <th data-priority="11">Jun</th>
                                                            <th data-priority="12">Jul</th>
                                                            <th data-priority="13">Aug</th>
                                                            <th data-priority="14">Sep</th>
                                                            <th data-priority="15">Oct</th>
                                                            <th data-priority="16">Nov</th>
                                                            <th data-priority="17">Dec</th>
                                                        </tr>
                                                    </thead>
                                                    <tbody>
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <tr>
                                                    <td></td>
                                                    <td><%# Item.Hub.Name %></td>
                                                    <td><%# Item.MeetingYear %></td>
                                                    <td><%# Item.LeadershipCoachUsername %></td>
                                                    <td><%# Item.TotalMeetings %></td>
                                                    <td><i class='<%# (Item.MeetingInJan ? "fas fa-check green-text" : "fas fa-times red-text") %>'></i></td>
                                                    <td><i class='<%# (Item.MeetingInFeb ? "fas fa-check green-text" : "fas fa-times red-text") %>'></td>
                                                    <td><i class='<%# (Item.MeetingInMar ? "fas fa-check green-text" : "fas fa-times red-text") %>'></td>
                                                    <td><i class='<%# (Item.MeetingInApr ? "fas fa-check green-text" : "fas fa-times red-text") %>'></td>
                                                    <td><i class='<%# (Item.MeetingInMay ? "fas fa-check green-text" : "fas fa-times red-text") %>'></td>
                                                    <td><i class='<%# (Item.MeetingInJun ? "fas fa-check green-text" : "fas fa-times red-text") %>'></td>
                                                    <td><i class='<%# (Item.MeetingInJul ? "fas fa-check green-text" : "fas fa-times red-text") %>'></td>
                                                    <td><i class='<%# (Item.MeetingInAug ? "fas fa-check green-text" : "fas fa-times red-text") %>'></td>
                                                    <td><i class='<%# (Item.MeetingInSep ? "fas fa-check green-text" : "fas fa-times red-text") %>'></td>
                                                    <td><i class='<%# (Item.MeetingInOct ? "fas fa-check green-text" : "fas fa-times red-text") %>'></td>
                                                    <td><i class='<%# (Item.MeetingInNov ? "fas fa-check green-text" : "fas fa-times red-text") %>'></td>
                                                    <td><i class='<%# (Item.MeetingInDec ? "fas fa-check green-text" : "fas fa-times red-text") %>'></td>
                                                </tr>
                                            </ItemTemplate>
                                            <FooterTemplate>
                                                </tbody>
                                                </table>
                                            </FooterTemplate>
                                        </asp:Repeater>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="ddHub" />
                        <asp:AsyncPostBackTrigger ControlID="deActionPlanStartDate" />
                        <asp:AsyncPostBackTrigger ControlID="deActionPlanEndDate" />
                    </Triggers>
                </asp:UpdatePanel>
            </div>
        </div>
        <div class="row">
            <div class="col-xl-12">
                <asp:HiddenField ID="hfDeleteGroundRulePK" runat="server" Value="0" />
                <asp:UpdatePanel ID="upGroundRule" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                    <ContentTemplate>
                        <div class="card bg-light bold-border">
                            <div class="card-header">
                                Meeting Ground Rules (Max: 6) (Section 5)
                                <asp:LinkButton ID="lbAddGroundRule" runat="server" CssClass="btn btn-loader btn-primary float-right hide-on-view hidden" OnClick="lbAddGroundRule_Click"><i class="fas fa-plus"></i>&nbsp;Add New Ground Rule</asp:LinkButton>
                            </div>
                            <div class="card-body">
                                <div class="review-required-div hidden">
                                    <i class="review-required-icon"></i>&nbsp;
                                    <p class="review-required-text d-inline"></p>
                                    <dx:BootstrapCheckBox ID="chkReviewedGroundRules" runat="server" Text="This section is complete and accurate.">
                                        <CssClasses Control="review-required-check-box" />
                                        <ClientSideEvents Init="setReviewRequiredDisplay" CheckedChanged="setReviewRequiredDisplay" />
                                    </dx:BootstrapCheckBox>
                                </div>
                                <div class="row">
                                    <div class="col-lg-12">
                                        <div class="alert alert-primary">
                                            <i class="fas fa-info-circle"></i>&nbsp;Each ground rule should be entered individually with a unique Rule Number.
                                        </div>
                                        <label>All meeting ground rules for the action plan timeframe</label>
                                        <asp:Repeater ID="repeatGroundRules" runat="server" ItemType="Pyramid.Models.CWLTActionPlanGroundRule">
                                            <HeaderTemplate>
                                                <table id="tblGroundRules" class="table table-striped table-bordered table-hover">
                                                    <thead>
                                                        <tr>
                                                            <th data-priority="1"></th>
                                                            <th data-priority="3">Rule Number</th>
                                                            <th data-priority="4">Rule Description</th>
                                                            <th data-priority="2"></th>
                                                        </tr>
                                                    </thead>
                                                    <tbody>
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <tr>
                                                    <td></td>
                                                    <td><%# Item.GroundRuleNumber %></td>
                                                    <td><%# Regex.Replace(Item.GroundRuleDescription, @"\r\n?|\n", "<br/>") %></td>
                                                    <td class="text-center">
                                                        <div class="btn-group">
                                                            <button type="button" class="btn btn-secondary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                                Actions
                                                            </button>
                                                            <div class="dropdown-menu dropdown-menu-right">
                                                                <asp:LinkButton ID="lbViewGroundRule" runat="server" CssClass="dropdown-item" OnClick="lbViewGroundRule_Click"><i class="fas fa-list"></i>&nbsp;View Details</asp:LinkButton>
                                                                <asp:LinkButton ID="lbEditGroundRule" runat="server" CssClass="dropdown-item hide-on-view" OnClick="lbEditGroundRule_Click"><i class="fas fa-edit"></i>&nbsp;Edit</asp:LinkButton>
                                                                <button class="dropdown-item delete-gridview hide-on-view" data-pk='<%# Item.CWLTActionPlanGroundRulePK %>' data-hf="hfDeleteGroundRulePK" data-target="#divDeleteGroundRuleModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                            </div>
                                                        </div>
                                                        <asp:Label ID="lblGroundRulePK" runat="server" Visible="false" Text='<%# Item.CWLTActionPlanGroundRulePK %>'></asp:Label>
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
                                        <div id="divAddEditGroundRule" runat="server" class="card mt-2" visible="false">
                                            <div class="card-header">
                                                <asp:Label ID="lblAddEditGroundRule" runat="server" Text=""></asp:Label>
                                            </div>
                                            <div class="card-body">
                                                <div class="row">
                                                    <div class="col-lg-4">
                                                        <dx:BootstrapTextBox ID="txtGroundRuleNumber" runat="server" Caption="Rule Number" MaxLength="3"
                                                            OnValidation="txtGroundRuleNumber_Validation">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <ValidationSettings ValidationGroup="vgGroundRule" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="true" ErrorText="Rule Number is required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapTextBox>
                                                    </div>
                                                    <div class="col-lg-8">
                                                        <dx:BootstrapMemo ID="txtGroundRuleDescription" runat="server" Caption="Rule Description" 
                                                            MaxLength="3000" Rows="4" OnValidation="txtGroundRuleDescription_Validation">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <ValidationSettings ValidationGroup="vgGroundRule" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="true" ErrorText="Rule Description is required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapMemo>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="card-footer">
                                                <div class="center-content">
                                                    <asp:HiddenField ID="hfAddEditGroundRulePK" runat="server" Value="0" />
                                                    <uc:Submit ID="submitGroundRule" runat="server" 
                                                        ValidationGroup="vgGroundRule"
                                                        ControlCssClass="center-content"
                                                        OnSubmitClick="submitGroundRule_Click" 
                                                        OnCancelClick="submitGroundRule_CancelClick" 
                                                        OnValidationFailed="submitGroundRule_ValidationFailed" />
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="lbDeleteGroundRule" EventName="Click" />
                    </Triggers>
                </asp:UpdatePanel>
            </div>
        </div>
        <asp:UpdatePanel ID="upHubAndBOQInfo" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
            <ContentTemplate>
                <div class="row">
                    <div class="col-xl-12">
                        <div class="card bg-light bold-border">
                            <div class="card-header">
                                Hub Information (Section 6)
                            </div>
                            <div class="card-body">
                                <div class="alert alert-primary">
                                    <i class="fas fa-info-circle"></i>&nbsp;This section is automatically filled based on the Hub and action plan timeframe in the Basic Information section.
                                </div>
                                <div class="row">
                                    <div class="col-lg-12">
                                        <asp:Label runat="server" AssociatedControlID="lblTotalActivePrograms" CssClass="d-block col-form-label" Text="Total Number of Active Programs"></asp:Label>
                                        <asp:Label ID="lblTotalActivePrograms" runat="server"></asp:Label>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="row">
                    <div class="col-xl-12">
                        <div class="card bg-light bold-border">
                            <div class="card-header">
                                Benchmark of Quality Information (Section 7)
                            </div>
                            <div class="card-body">
                                <div class="alert alert-primary">
                                    <i class="fas fa-info-circle"></i>&nbsp;This section is automatically filled based on the hub and timeframe in the Basic Information section.  
                                    It pulls the most recent Benchmark of Quality form for the hub that is on or before the action plan end date and on or after the action plan start date minus six months.
                                    The table below displays all the indicators that are either Not In Place or Needs Improvement.
                                </div>
                                <div class="row">
                                    <div class="col-lg-12">
                                        <asp:Label runat="server" AssociatedControlID="repeatBOQCriticalElements" CssClass="d-block col-form-label" Text="Critical Elements"></asp:Label>
                                        <div class="h5">
                                            <asp:Repeater ID="repeatBOQCriticalElements" runat="server" ItemType="Pyramid.Models.CodeBOQCriticalElement">
                                                <ItemTemplate>
                                                    <span class="badge badge-info text-wrap mb-2"><%# string.Format("{0} = {1}", Item.Abbreviation, Item.Description) %></span>
                                                </ItemTemplate>
                                            </asp:Repeater>
                                        </div>
                                    </div>
                                </div>
                                <div class="row mb-2">
                                    <div class="col-lg-12">
                                        <asp:Label runat="server" AssociatedControlID="lblMostRecentBOQDate" CssClass="d-block col-form-label" Text="Most Recent BOQ Date"></asp:Label>
                                        <asp:Label ID="lblMostRecentBOQDate" runat="server"></asp:Label>
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-lg-12">
                                        <label>Indicators from the Most Recent BOQ that can be Improved</label>
                                        <asp:Repeater ID="repeatBOQIndicatorsToBeImproved" runat="server" ItemType="Pyramid.Models.spGetBOQCWLTIndicatorValues_Result">
                                            <HeaderTemplate>
                                                <table id="tblBOQIndicators" class="table table-bordered table-hover">
                                                    <thead>
                                                        <tr>
                                                            <th data-priority="1"></th>
                                                            <th data-priority="4">Critical Element</th>
                                                            <th data-priority="2">Indicator Number</th>
                                                            <th data-priority="5">Benchmark of Quality</th>
                                                            <th data-priority="3">Indicator Status</th>
                                                        </tr>
                                                    </thead>
                                                    <tbody>
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <tr class='<%# (Item.IndicatorValue == (int)Pyramid.Models.CodeBOQIndicatorValue.BOQCWLTIndicatorValues.NEEDS_IMPROVEMENT ? "alert-warning"
                                                                : Item.IndicatorValue == (int)Pyramid.Models.CodeBOQIndicatorValue.BOQCWLTIndicatorValues.NOT_IN_PLACE ? "alert-danger"
                                                                : "") %>'>
                                                    <td></td>
                                                    <td><%# Item.CriticalElementAbbreviation %></td>
                                                    <td><%# Item.IndicatorNumber %></td>
                                                    <td><%# Item.IndicatorDescription %></td>
                                                    <td><%# Item.IndicatorValueDescription %></td>
                                                </tr>
                                            </ItemTemplate>
                                            <FooterTemplate>
                                                </tbody>
                                                </table>
                                            </FooterTemplate>
                                        </asp:Repeater>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="ddHub" />
                <asp:AsyncPostBackTrigger ControlID="deActionPlanStartDate" />
                <asp:AsyncPostBackTrigger ControlID="deActionPlanEndDate" />
            </Triggers>
        </asp:UpdatePanel>
        <div class="row">
            <div class="col-xl-12">
                <asp:HiddenField ID="hfDeleteActionStepPK" runat="server" Value="0" />
                <asp:UpdatePanel ID="upActionStep" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                    <ContentTemplate>
                        <div class="card bg-light bold-border">
                            <div class="card-header">
                                Benchmark of Quality Action Steps (Section 8)
                                <asp:LinkButton ID="lbAddActionStep" runat="server" CssClass="btn btn-loader btn-primary float-right hide-on-view hidden" OnClick="lbAddActionStep_Click"><i class="fas fa-plus"></i>&nbsp;Add New Action Step</asp:LinkButton>
                            </div>
                            <div class="card-body">
                                <div class="review-required-div hidden">
                                    <i class="review-required-icon"></i>&nbsp;
                                    <p class="review-required-text d-inline"></p>
                                    <dx:BootstrapCheckBox ID="chkReviewedActionSteps" runat="server" Text="This section is complete and accurate.">
                                        <CssClasses Control="review-required-check-box" />
                                        <ClientSideEvents Init="setReviewRequiredDisplay" CheckedChanged="setReviewRequiredDisplay" />
                                    </dx:BootstrapCheckBox>
                                </div>
                                <div class="row">
                                    <div class="col-lg-12">
                                        <div class="alert alert-primary">
                                            <p><i class="fas fa-info-circle"></i>&nbsp;Each time the status of an Action Step changes, edit the Action Step and add a new status record in the Action Step Status section.</p>
                                            <p class="mb-0">After pre-fill, make sure that the target dates are between the start and end dates of the Action Plan.</p>
                                        </div>
                                        <label>All action steps for the action plan timeframe</label>
                                        <asp:Repeater ID="repeatActionSteps" runat="server" ItemType="Pyramid.Models.CWLTActionPlanActionStep">
                                            <HeaderTemplate>
                                                <table id="tblActionSteps" class="table table-striped table-bordered table-hover">
                                                    <thead>
                                                        <tr>
                                                            <th data-priority="1"></th>
                                                            <th data-priority="4">Critical Element</th>
                                                            <th data-priority="3">Indicator</th>
                                                            <th data-priority="5">What is the Problem/Issue/Task to be Addressed?</th>
                                                            <th data-priority="6">Action Step/Activity</th>
                                                            <th data-priority="7">Persons Responsible</th>
                                                            <th data-priority="8">Target Date</th>
                                                            <th data-priority="9">Current Action Step Status</th>
                                                            <th data-priority="2"></th>
                                                        </tr>
                                                    </thead>
                                                    <tbody>
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <tr>
                                                    <td></td>
                                                    <td><%# Item.CodeBOQIndicator.CodeBOQCriticalElement.Abbreviation %></td>
                                                    <td><%# Item.CodeBOQIndicator.IndicatorNumber %></td>
                                                    <td><%# Item.ProblemIssueTask %></td>
                                                    <td><%# Item.ActionStepActivity %></td>
                                                    <td><%# Item.PersonsResponsible %></td>
                                                    <td><%# Item.TargetDate.ToString("MM/dd/yyyy") %></td>
                                                    <td><%# (Item.CWLTActionPlanActionStepStatus.OrderByDescending(s => s.StatusDate).FirstOrDefault() == null ? "N/A" : Item.CWLTActionPlanActionStepStatus.OrderByDescending(s => s.StatusDate).FirstOrDefault().CodeActionPlanActionStepStatus.Description) %></td>
                                                    <td class="text-center">
                                                        <div class="btn-group">
                                                            <button type="button" class="btn btn-secondary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                                Actions
                                                            </button>
                                                            <div class="dropdown-menu dropdown-menu-right">
                                                                <asp:LinkButton ID="lbViewActionStep" runat="server" CssClass="dropdown-item" OnClick="lbViewActionStep_Click"><i class="fas fa-list"></i>&nbsp;View Details</asp:LinkButton>
                                                                <asp:LinkButton ID="lbEditActionStep" runat="server" CssClass="dropdown-item hide-on-view" OnClick="lbEditActionStep_Click"><i class="fas fa-edit"></i>&nbsp;Edit</asp:LinkButton>
                                                                <button class="dropdown-item delete-gridview hide-on-view" data-pk='<%# Item.CWLTActionPlanActionStepPK %>' data-hf="hfDeleteActionStepPK" data-target="#divDeleteActionStepModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                            </div>
                                                        </div>
                                                        <asp:Label ID="lblActionStepPK" runat="server" Visible="false" Text='<%# Item.CWLTActionPlanActionStepPK %>'></asp:Label>
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
                                        <div id="divAddEditActionStep" runat="server" class="card mt-2" visible="false">
                                            <div class="card-header">
                                                <asp:Label ID="lblAddEditActionStep" runat="server" Text=""></asp:Label>
                                            </div>
                                            <div class="card-body">
                                                <div class="row">
                                                    <div class="col-lg-4">
                                                        <dx:BootstrapComboBox ID="ddActionStepIndicator" runat="server" Caption="Indicator" NullText="--Select--" ValueType="System.Int32"
                                                            TextField="IndicatorNumAndElement" ValueField="CodeBOQIndicatorPK" 
                                                            IncrementalFilteringMode="Contains" AllowMouseWheel="false" AutoPostBack="true"
                                                            OnValueChanged="ddActionStepIndicator_ValueChanged">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <ValidationSettings ValidationGroup="vgActionStep" ErrorDisplayMode="ImageWithText">
                                                                <RequiredField IsRequired="true" ErrorText="Indicator is required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapComboBox>
                                                    </div>
                                                    <div class="col-lg-4">
                                                        <asp:Label runat="server" AssociatedControlID="lblActionStepCriticalElement" CssClass="d-block col-form-label" Text="Critical Element"></asp:Label>
                                                        <asp:Label ID="lblActionStepCriticalElement" runat="server"></asp:Label>
                                                    </div>
                                                    <div class="col-lg-4">
                                                        <asp:Label runat="server" AssociatedControlID="lblActionStepIndicatorDescription" CssClass="d-block col-form-label" Text="Indicator Description"></asp:Label>
                                                        <asp:Label ID="lblActionStepIndicatorDescription" runat="server"></asp:Label>
                                                    </div>
                                                </div>
                                                <div class="row">
                                                    <div class="col-lg-6">
                                                        <dx:BootstrapMemo ID="txtActionStepProblemIssueTask" runat="server" Caption="What is the Problem/Issue/Task to be Addressed?" 
                                                            MaxLength="3000" Rows="4">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <ValidationSettings ValidationGroup="vgActionStep" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="true" ErrorText="What is the Problem/Issue/Task to be Addressed? is required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapMemo>
                                                    </div>
                                                    <div class="col-lg-6">
                                                        <dx:BootstrapMemo ID="txtActionStepActivity" runat="server" Caption="Action Step/Activity" 
                                                            MaxLength="3000" Rows="4">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <ValidationSettings ValidationGroup="vgActionStep" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="true" ErrorText="Action Step/Activity is required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapMemo>
                                                    </div>
                                                </div>
                                                <div class="row">
                                                    <div class="col-lg-6">
                                                        <dx:BootstrapMemo ID="txtActionStepPersonsResponsible" runat="server" Caption="Persons Responsible" 
                                                            MaxLength="1500" Rows="4">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <ValidationSettings ValidationGroup="vgActionStep" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="true" ErrorText="Persons Responsible is required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapMemo>
                                                    </div>
                                                    <div class="col-lg-6">
                                                        <dx:BootstrapDateEdit ID="deActionStepTargetDate" runat="server" Caption="Target Date" EditFormat="Date"
                                                            EditFormatString="MM/dd/yyyy" UseMaskBehavior="true" NullText="--Select--"
                                                            AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900" OnValidation="deActionStepTargetDate_Validation">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                                            <ValidationSettings ValidationGroup="vgActionStep" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="true" ErrorText="Target Date is required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapDateEdit>
                                                    </div>
                                                </div>
                                                <div id="divActionStepInitialStatus" runat="server" class="row">
                                                    <div class="col-lg-6">
                                                        <dx:BootstrapComboBox ID="ddActionStepInitialStatus" runat="server" Caption="Current Status" NullText="--Select--" ValueType="System.Int32"
                                                            TextField="Description" ValueField="CodeActionPlanActionStepStatusPK" 
                                                            IncrementalFilteringMode="Contains" AllowMouseWheel="false" OnValidation="ddActionStepInitialStatus_Validation">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <ValidationSettings ValidationGroup="vgActionStep" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="false" ErrorText="Current Status is required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapComboBox>
                                                    </div>
                                                    <div class="col-lg-6">
                                                        <dx:BootstrapDateEdit ID="deActionStepInitialStatusDate" runat="server" Caption="Current Status Date" EditFormat="Date"
                                                            EditFormatString="MM/dd/yyyy" UseMaskBehavior="true" NullText="--Select--"
                                                            AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900" OnValidation="deActionStepInitialStatusDate_Validation">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                                            <ValidationSettings ValidationGroup="vgActionStep" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="false" ErrorText="Current Status Date is required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapDateEdit>
                                                    </div>
                                                </div>
                                                <div id="divActionStepStatusHistory" runat="server" class="row">
                                                    <div class="col-xl-12">
                                                        <asp:HiddenField ID="hfDeleteActionStepStatusPK" runat="server" Value="0" />
                                                        <div class="card bg-light">
                                                            <div class="card-header">
                                                                Action Step Status
                                                                <asp:LinkButton ID="lbAddActionStepStatus" runat="server" CssClass="btn btn-loader btn-primary float-right hide-on-view hidden" OnClick="lbAddActionStepStatus_Click"><i class="fas fa-plus"></i>&nbsp;Add New Action Step Status</asp:LinkButton>
                                                            </div>
                                                            <div class="card-body">
                                                                <div class="row">
                                                                    <div class="col-lg-12">
                                                                        <label>All status records for this action step</label>
                                                                        <asp:Repeater ID="repeatActionStepStatuses" runat="server" ItemType="Pyramid.Models.CWLTActionPlanActionStepStatus">
                                                                            <HeaderTemplate>
                                                                                <table id="tblActionStepStatuses" class="table table-striped table-bordered table-hover">
                                                                                    <thead>
                                                                                        <tr>
                                                                                            <th data-priority="1"></th>
                                                                                            <th data-priority="3">Status</th>
                                                                                            <th data-priority="4">Status Date</th>
                                                                                            <th data-priority="2"></th>
                                                                                        </tr>
                                                                                    </thead>
                                                                                    <tbody>
                                                                            </HeaderTemplate>
                                                                            <ItemTemplate>
                                                                                <tr>
                                                                                    <td></td>
                                                                                    <td><%# Item.CodeActionPlanActionStepStatus.Description %></td>
                                                                                    <td><%# Item.StatusDate.ToString("MM/dd/yyyy") %></td>
                                                                                    <td class="text-center">
                                                                                        <div class="btn-group">
                                                                                            <button type="button" class="btn btn-secondary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                                                                Actions
                                                                                            </button>
                                                                                            <div class="dropdown-menu dropdown-menu-right">
                                                                                                <asp:LinkButton ID="lbViewActionStepStatus" runat="server" CssClass="dropdown-item" OnClick="lbViewActionStepStatus_Click"><i class="fas fa-list"></i>&nbsp;View Details</asp:LinkButton>
                                                                                                <asp:LinkButton ID="lbEditActionStepStatus" runat="server" CssClass="dropdown-item hide-on-view" OnClick="lbEditActionStepStatus_Click"><i class="fas fa-edit"></i>&nbsp;Edit</asp:LinkButton>
                                                                                                <button class="dropdown-item delete-gridview hide-on-view" data-pk='<%# Item.CWLTActionPlanActionStepStatusPK %>' data-hf="hfDeleteActionStepStatusPK" data-target="#divDeleteActionStepStatusModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                                                            </div>
                                                                                        </div>
                                                                                        <asp:Label ID="lblActionStepStatusPK" runat="server" Visible="false" Text='<%# Item.CWLTActionPlanActionStepStatusPK %>'></asp:Label>
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
                                                                        <div id="divAddEditActionStepStatus" runat="server" class="card mt-2" visible="false">
                                                                            <div class="card-header">
                                                                                <asp:Label ID="lblAddEditActionStepStatus" runat="server" Text=""></asp:Label>
                                                                            </div>
                                                                            <div class="card-body">
                                                                                <div class="row">
                                                                                    <div class="col-lg-6">
                                                                                        <dx:BootstrapComboBox ID="ddActionStepStatus" runat="server" Caption="Status" NullText="--Select--" ValueType="System.Int32"
                                                                                            TextField="Description" ValueField="CodeActionPlanActionStepStatusPK" 
                                                                                            IncrementalFilteringMode="Contains" AllowMouseWheel="false">
                                                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                                                            <ValidationSettings ValidationGroup="vgActionStepStatus" ErrorDisplayMode="ImageWithText">
                                                                                                <RequiredField IsRequired="true" ErrorText="Status is required!" />
                                                                                            </ValidationSettings>
                                                                                        </dx:BootstrapComboBox>
                                                                                    </div>
                                                                                    <div class="col-lg-6">
                                                                                        <dx:BootstrapDateEdit ID="deActionStepStatusDate" runat="server" Caption="Status Date" EditFormat="Date"
                                                                                            EditFormatString="MM/dd/yyyy" UseMaskBehavior="true" NullText="--Select--"
                                                                                            AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900" OnValidation="deActionStepStatusDate_Validation">
                                                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                                                            <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                                                                            <ValidationSettings ValidationGroup="vgActionStepStatus" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                                                                <RequiredField IsRequired="true" ErrorText="Status Date is required!" />
                                                                                            </ValidationSettings>
                                                                                        </dx:BootstrapDateEdit>
                                                                                    </div>
                                                                                </div>
                                                                            </div>
                                                                            <div class="card-footer">
                                                                                <div class="center-content">
                                                                                    <asp:HiddenField ID="hfAddEditActionStepStatusPK" runat="server" Value="0" />
                                                                                    <uc:Submit ID="submitActionStepStatus" runat="server" 
                                                                                        ValidationGroup="vgActionStepStatus"
                                                                                        ControlCssClass="center-content"
                                                                                        OnSubmitClick="submitActionStepStatus_Click" 
                                                                                        OnCancelClick="submitActionStepStatus_CancelClick" 
                                                                                        OnValidationFailed="submitActionStepStatus_ValidationFailed" />
                                                                                </div>
                                                                            </div>
                                                                        </div>
                                                                    </div>
                                                                </div>
                                                            </div>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="card-footer">
                                                <div class="center-content">
                                                    <asp:HiddenField ID="hfAddEditActionStepPK" runat="server" Value="0" />
                                                    <uc:Submit ID="submitActionStep" runat="server" 
                                                        ValidationGroup="vgActionStep"
                                                        ControlCssClass="center-content"
                                                        OnSubmitClick="submitActionStep_Click" 
                                                        OnCancelClick="submitActionStep_CancelClick" 
                                                        OnValidationFailed="submitActionStep_ValidationFailed" />
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="lbDeleteActionStep" EventName="Click" />
                        <asp:AsyncPostBackTrigger ControlID="lbDeleteActionStepStatus" EventName="Click" />
                    </Triggers>
                </asp:UpdatePanel>
            </div>
        </div>
    </div>
    <div class="page-footer">
        <uc:Submit ID="submitActionPlan" runat="server" ValidationGroup="vgActionPlan"
            ControlCssClass="center-content"
            OnSubmitClick="submitActionPlan_Click" 
            OnCancelClick="submitActionPlan_CancelClick" 
            OnValidationFailed="submitActionPlan_ValidationFailed" />
    </div>
    <div class="modal" id="divDeleteMeetingModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete Meeting Date</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this meeting date?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteMeeting" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteMeetingModal" OnClick="lbDeleteMeeting_Click"><i class="fas fa-check"></i>&nbsp;Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
    <div class="modal" id="divDeleteGroundRuleModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete Ground Rule</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this ground rule?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteGroundRule" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteGroundRuleModal" OnClick="lbDeleteGroundRule_Click"><i class="fas fa-check"></i>&nbsp;Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
    <div class="modal" id="divDeleteActionStepModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete Action Step</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this action step?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteActionStep" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteActionStepModal" OnClick="lbDeleteActionStep_Click"><i class="fas fa-check"></i>&nbsp;Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
    <div class="modal" id="divDeleteActionStepStatusModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete Action Step Status</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this action step status?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteActionStepStatus" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteActionStepStatusModal" OnClick="lbDeleteActionStepStatus_Click"><i class="fas fa-check"></i>&nbsp;Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
</asp:Content>