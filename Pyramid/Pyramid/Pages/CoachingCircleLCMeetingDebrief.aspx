<%@ Page Title="Coaching Circle Debrief" Language="C#" MasterPageFile="~/MasterPages/Dashboard.master" AutoEventWireup="true" CodeBehind="CoachingCircleLCMeetingDebrief.aspx.cs" Inherits="Pyramid.Pages.CoachingCircleLCMeetingDebrief" %>

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
            $('#lnkLCDashboard').addClass('active');

            //Allow date format for DataTables sorting
            $.fn.dataTable.moment('MM/DD/YYYY');

            //Show/hide the view only fields
            setViewOnlyVisibility();

            //Initialize the datatables
            if (!$.fn.dataTable.isDataTable('#tblTeamMembers')) {
                $('#tblTeamMembers').DataTable({
                    responsive: {
                        details: {
                            type: 'column'
                        }
                    },
                    columnDefs: [
                        { orderable: false, targets: [6] },
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
            if (!$.fn.dataTable.isDataTable('#tblDebriefSessions')) {
                $('#tblDebriefSessions').DataTable({
                    responsive: {
                        details: {
                            type: 'column'
                        }
                    },
                    columnDefs: [
                        { orderable: false, targets: [5] },
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

        function showHideDebriefSessionReviewedOtherSpecify() {
            //Get the other checkbox value
            var wasOtherItemReviewed = chkDebriefSessionReviewedOther.GetChecked();

            //Show/hide the specify textbox
            if (wasOtherItemReviewed === true) {
                //Show the div
                $('#divDebriefSessionReviewedOtherSpecify').show();
            }
            else {
                //Hide the div and clear the textbox
                $('#divDebriefSessionReviewedOtherSpecify').hide();
                txtDebriefSessionReviewedOtherSpecify.SetText('');
            }
        }
    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Label ID="lblPageTitle" runat="server" Text="Coaching Circle/Community of Practice Meeting Debrief" CssClass="h2"></asp:Label>
    <hr />
    <asp:HiddenField ID="hfViewOnly" runat="server" Value="False" />
    <asp:UpdatePanel ID="upMessaging" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <uc:Messaging ID="msgSys" runat="server" />
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="lbAddTeamMember" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="repeatTeamMembers" />
            <asp:AsyncPostBackTrigger ControlID="lbDeleteTeamMember" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="submitTeamMember" />
            <asp:AsyncPostBackTrigger ControlID="lbAddDebriefSession" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="repeatDebriefSessions" />
            <asp:AsyncPostBackTrigger ControlID="lbDeleteDebriefSession" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="submitDebriefSession" />
            <asp:AsyncPostBackTrigger ControlID="btnPrintPreview" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="submitMeetingDebrief" />
        </Triggers>
    </asp:UpdatePanel>
    <asp:UpdatePanel ID="upBasicInfo" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <ContentTemplate>
            <asp:HiddenField ID="hfCoachingCircleLCMeetingDebriefPK" runat="server" Value="" />
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
                                        SettingsBootstrap-RenderOption="primary" ValidationGroup="vgCoachingCircleLCMeetingDebrief" data-validation-group="vgCoachingCircleLCMeetingDebrief">
                                        <CssClasses Icon="fas fa-print" Control="float-right btn-loader" />
                                    </dx:BootstrapButton>
                                </div>
                            </div>
                        </div>
                        <div class="card-body">
                            <div class="row">
                                <div class="col-lg-4">
                                    <asp:Label runat="server" AssociatedControlID="lblLeadershipCoach" CssClass="d-block" Text="Leadership Coach"></asp:Label>
                                    <asp:Label ID="lblLeadershipCoach" runat="server"></asp:Label>
                                </div>
                                <div class="col-lg-4">
                                    <asp:Label runat="server" AssociatedControlID="lblDebriefState" CssClass="d-block" Text="State"></asp:Label>
                                    <asp:Label ID="lblDebriefState" runat="server"></asp:Label>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-4">
                                    <dx:BootstrapMemo ID="txtDebriefCoachingCircle" runat="server" Caption="Coaching Circle/Community of Practice" MaxLength="500" Rows="1">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgMeetingDebrief" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Coaching Circle/Community of Practice is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapMemo>
                                </div>
                                <div class="col-lg-4">
                                    <dx:BootstrapDateEdit ID="deDebriefYear" runat="server" Caption="Year" EditFormat="Date"
                                        EditFormatString="yyyy" UseMaskBehavior="true" NullText="--Select--" PickerType="Years"
                                        AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900"
                                        OnValidation="deDebriefYear_Validation">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                        <ValidationSettings ValidationGroup="vgMeetingDebrief" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Year is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapDateEdit>
                                </div>
                                <div class="col-lg-4">
                                    <dx:BootstrapMemo ID="txtDebriefTargetAudience" runat="server" Caption="Target Audience" MaxLength="1000" Rows="2">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgMeetingDebrief" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Target Audience is required!" />
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
            <asp:AsyncPostBackTrigger ControlID="submitMeetingDebrief" />
        </Triggers>
    </asp:UpdatePanel>
    <div id="divEditOnly" runat="server" visible="false">
        <div class="row">
            <div class="col-xl-12">
                <asp:HiddenField ID="hfDeleteTeamMemberPK" runat="server" Value="0" />
                <asp:UpdatePanel ID="upTeamMember" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                    <ContentTemplate>
                        <div class="card bg-light">
                            <div class="card-header">
                                Group Members
                                <asp:LinkButton ID="lbAddTeamMember" runat="server" CssClass="btn btn-loader btn-primary float-right hide-on-view hidden" OnClick="lbAddTeamMember_Click"><i class="fas fa-plus"></i>&nbsp;Add New Group Member</asp:LinkButton>
                            </div>
                            <div class="card-body">
                                <div class="row">
                                    <div class="col-lg-12">
                                        <label>All group members for this year</label>
                                        <asp:Repeater ID="repeatTeamMembers" runat="server" ItemType="Pyramid.Models.CoachingCircleLCMeetingDebriefTeamMember">
                                            <HeaderTemplate>
                                                <table id="tblTeamMembers" class="table table-striped table-bordered table-hover">
                                                    <thead>
                                                        <tr>
                                                            <th data-priority="1"></th>
                                                            <th data-priority="3">Position</th>
                                                            <th data-priority="4">First Name</th>
                                                            <th data-priority="5">Last Name</th>
                                                            <th data-priority="6">Email</th>
                                                            <th data-priority="7">Phone</th>
                                                            <th data-priority="2"></th>
                                                        </tr>
                                                    </thead>
                                                    <tbody>
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <tr>
                                                    <td></td>
                                                    <td><%# Item.CodeTeamPosition.Description %></td>
                                                    <td><%# Item.FirstName %></td>
                                                    <td><%# Item.LastName %></td>
                                                    <td><%# Item.EmailAddress %></td>
                                                    <td><%# (string.IsNullOrWhiteSpace(Item.PhoneNumber) ? "" : Pyramid.Code.Utilities.FormatPhoneNumber(Item.PhoneNumber, "US")) %></td>
                                                    <td class="text-center">
                                                        <div class="btn-group">
                                                            <button type="button" class="btn btn-secondary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                                Actions
                                                            </button>
                                                            <div class="dropdown-menu dropdown-menu-right">
                                                                <asp:LinkButton ID="lbViewTeamMember" runat="server" CssClass="dropdown-item" OnClick="lbViewTeamMember_Click"><i class="fas fa-list"></i>&nbsp;View Details</asp:LinkButton>
                                                                <asp:LinkButton ID="lbEditTeamMember" runat="server" CssClass="dropdown-item hide-on-view" OnClick="lbEditTeamMember_Click"><i class="fas fa-edit"></i>&nbsp;Edit</asp:LinkButton>
                                                                <button class="dropdown-item delete-gridview hide-on-view" data-pk='<%# Item.CoachingCircleLCMeetingDebriefTeamMemberPK %>' data-hf="hfDeleteTeamMemberPK" data-target="#divDeleteTeamMemberModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                            </div>
                                                        </div>
                                                        <asp:Label ID="lblTeamPositionCodeFK" runat="server" Visible="false" Text='<%# Item.TeamPositionCodeFK %>'></asp:Label>
                                                        <asp:Label ID="lblTeamMemberPK" runat="server" Visible="false" Text='<%# Item.CoachingCircleLCMeetingDebriefTeamMemberPK %>'></asp:Label>
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
                                        <div id="divAddEditTeamMember" runat="server" class="card mt-2" visible="false">
                                            <div class="card-header">
                                                <asp:Label ID="lblAddEditTeamMember" runat="server" Text=""></asp:Label>
                                            </div>
                                            <div class="card-body">
                                                <div class="row">
                                                    <div class="col-lg-4">
                                                        <dx:BootstrapComboBox ID="ddTeamMemberPosition" runat="server" Caption="Position" NullText="--Select--" ValueType="System.Int32"
                                                            TextField="Description" ValueField="CodeTeamPositionPK" 
                                                            IncrementalFilteringMode="Contains" AllowMouseWheel="false"
                                                            ClientInstanceName="ddTeamMemberPosition">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <ValidationSettings ValidationGroup="vgTeamMember" ErrorDisplayMode="ImageWithText">
                                                                <RequiredField IsRequired="true" ErrorText="Position is required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapComboBox>
                                                    </div>
                                                    <div class="col-lg-4">
                                                        <dx:BootstrapTextBox ID="txtTeamMemberFirstName" runat="server" Caption="First Name">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <ValidationSettings ValidationGroup="vgTeamMember" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="true" ErrorText="First Name is required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapTextBox>
                                                    </div>
                                                    <div class="col-lg-4">
                                                        <dx:BootstrapTextBox ID="txtTeamMemberLastName" runat="server" Caption="Last Name">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <ValidationSettings ValidationGroup="vgTeamMember" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="true" ErrorText="Last Name is required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapTextBox>
                                                    </div>
                                                </div>
                                                <div class="row">
                                                    <div class="col-lg-4">
                                                        <dx:BootstrapTextBox ID="txtTeamMemberEmail" runat="server" Caption="Email" MaxLength="400"
                                                            ClientInstanceName="txtTeamMemberEmail">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <ValidationSettings ValidationGroup="vgTeamMember" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="false" ErrorText="Email is required!" />
                                                                <RegularExpression ErrorText="Invalid Email address!" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapTextBox>
                                                    </div>
                                                    <div class="col-lg-4">
                                                        <dx:BootstrapTextBox ID="txtTeamMemberPhone" runat="server" Caption="Phone" MaxLength="40" OnValidation="txtTeamMemberPhone_Validation">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <MaskSettings Mask="+1 (999) 999-9999 \e\x\t\. 999999" IncludeLiterals="None" ErrorText="Must be a valid phone number!" />
                                                            <ValidationSettings ValidationGroup="vgTeamMember" ErrorDisplayMode="ImageWithText">
                                                                <RequiredField IsRequired="false" ErrorText="Phone is required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapTextBox>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="card-footer">
                                                <div class="center-content">
                                                    <asp:HiddenField ID="hfAddEditTeamMemberPK" runat="server" Value="0" />
                                                    <uc:Submit ID="submitTeamMember" runat="server" 
                                                        ValidationGroup="vgTeamMember"
                                                        ControlCssClass="center-content"
                                                        OnSubmitClick="submitTeamMember_Click" 
                                                        OnCancelClick="submitTeamMember_CancelClick" 
                                                        OnValidationFailed="submitTeamMember_ValidationFailed" />
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="lbDeleteTeamMember" EventName="Click" />
                    </Triggers>
                </asp:UpdatePanel>
            </div>
        </div>
        <div class="row">
            <div class="col-xl-12">
                <asp:HiddenField ID="hfDeleteDebriefSessionPK" runat="server" Value="0" />
                <asp:UpdatePanel ID="upDebriefSession" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                    <ContentTemplate>
                        <div class="card bg-light">
                            <div class="card-header">
                                Session Debriefs
                                <asp:LinkButton ID="lbAddDebriefSession" runat="server" CssClass="btn btn-loader btn-primary float-right hide-on-view hidden" OnClick="lbAddDebriefSession_Click"><i class="fas fa-plus"></i>&nbsp;Add New Debrief Session</asp:LinkButton>
                            </div>
                            <div class="card-body">
                                <div class="row">
                                    <div class="col-lg-12">
                                        <label>All session debriefs</label>
                                        <asp:Repeater ID="repeatDebriefSessions" runat="server" ItemType="Pyramid.Models.CoachingCircleLCMeetingDebriefSession">
                                            <HeaderTemplate>
                                                <table id="tblDebriefSessions" class="table table-striped table-bordered table-hover">
                                                    <thead>
                                                        <tr>
                                                            <th data-priority="1"></th>
                                                            <th data-priority="3">Attendees</th>
                                                            <th data-priority="4">Session Date</th>
                                                            <th data-priority="5">Start Time</th>
                                                            <th data-priority="6">End Time</th>
                                                            <th data-priority="2"></th>
                                                        </tr>
                                                    </thead>
                                                    <tbody>
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <tr>
                                                    <td></td>
                                                    <td><%# string.Join(", ", Item.CoachingCircleLCMeetingDebriefSessionAttendee.Select(a => string.Format("{0} {1}", a.CoachingCircleLCMeetingDebriefTeamMember.FirstName, a.CoachingCircleLCMeetingDebriefTeamMember.LastName)).ToList()) %></td>
                                                    <td><%# Item.SessionStartDateTime.ToString("MM/dd/yyyy") %></td>
                                                    <td><%# Item.SessionStartDateTime.ToString("hh:mm tt") %></td>
                                                    <td><%# Item.SessionEndDateTime.ToString("hh:mm tt") %></td>
                                                    <td class="text-center">
                                                        <div class="btn-group">
                                                            <button type="button" class="btn btn-secondary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                                Actions
                                                            </button>
                                                            <div class="dropdown-menu dropdown-menu-right">
                                                                <asp:LinkButton ID="lbViewDebriefSession" runat="server" CssClass="dropdown-item" OnClick="lbViewDebriefSession_Click"><i class="fas fa-list"></i>&nbsp;View Details</asp:LinkButton>
                                                                <asp:LinkButton ID="lbEditDebriefSession" runat="server" CssClass="dropdown-item hide-on-view" OnClick="lbEditDebriefSession_Click"><i class="fas fa-edit"></i>&nbsp;Edit</asp:LinkButton>
                                                                <button class="dropdown-item delete-gridview hide-on-view" data-pk='<%# Item.CoachingCircleLCMeetingDebriefSessionPK %>' data-hf="hfDeleteDebriefSessionPK" data-target="#divDeleteDebriefSessionModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                            </div>
                                                        </div>
                                                        <asp:Label ID="lblDebriefSessionPK" runat="server" Visible="false" Text='<%# Item.CoachingCircleLCMeetingDebriefSessionPK %>'></asp:Label>
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
                                        <div id="divAddEditDebriefSession" runat="server" class="card mt-2" visible="false">
                                            <div class="card-header">
                                                <asp:Label ID="lblAddEditDebriefSession" runat="server" Text=""></asp:Label>
                                            </div>
                                            <div class="card-body">
                                                <div class="row">
                                                    <div class="col-lg-4">
                                                        <dx:BootstrapDateEdit ID="deDebriefSessionDate" runat="server" Caption="Session Date" EditFormat="Date"
                                                            EditFormatString="MM/dd/yyyy" UseMaskBehavior="true" NullText="--Select--"
                                                            AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900" OnValidation="deDebriefSessionDate_Validation">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                                            <ValidationSettings ValidationGroup="vgDebriefSession" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="true" ErrorText="Session Date is required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapDateEdit>
                                                    </div>
                                                    <div class="col-lg-4">
                                                        <dx:BootstrapTimeEdit ID="teDebriefSessionStartTime" runat="server" Caption="Session Start Time" EditFormat="Time"
                                                            EditFormatString="hh:mm tt" NullText="" ClientInstanceName="teDebriefSessionStartTime"
                                                            SpinButtons-ClientVisible="false">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <ValidationSettings ValidationGroup="vgDebriefSession" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="true" ErrorText="Session Start Time is required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapTimeEdit>
                                                    </div>
                                                    <div class="col-lg-4">
                                                        <dx:BootstrapTimeEdit ID="teDebriefSessionEndTime" runat="server" Caption="Session End Time" EditFormat="Time"
                                                            EditFormatString="hh:mm tt" NullText="" ClientInstanceName="teDebriefSessionEndTime"
                                                            SpinButtons-ClientVisible="false" OnValidation="teDebriefSessionEndTime_Validation">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <ValidationSettings ValidationGroup="vgDebriefSession" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="true" ErrorText="Session End Time is required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapTimeEdit>
                                                    </div>
                                                    <div class="col-lg-4">
                                                        <dx:BootstrapTagBox ID="tbDebriefSessionAttendees" runat="server" Caption="Attendees"
                                                            AllowCustomTags="false" TextField="FullName" ValueField="CoachingCircleLCMeetingDebriefTeamMemberPK">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <ValidationSettings ValidationGroup="vgDebriefSession" ErrorDisplayMode="ImageWithText">
                                                                <RequiredField IsRequired="true" ErrorText="At least one attendee must be selected!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapTagBox>
                                                        <button id="lnkDebriefSessionAttendeeHelp" type="button" class="btn btn-link p-0"
                                                            data-toggle="popover" data-trigger="focus hover" 
                                                            title="Help" 
                                                            data-content="The group members must be entered above before they can be selected here.">
                                                            <i class="fas fa-question-circle"></i>&nbsp;Help
                                                        </button>
                                                    </div>
                                                </div>
                                                <div class="row">
                                                    <div class="col-lg-12">
                                                        <dx:BootstrapMemo ID="txtDebriefSessionSummary" runat="server" Caption="Meeting Summary" MaxLength="5000" Rows="4">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <ValidationSettings ValidationGroup="vgDebriefSession" ErrorDisplayMode="ImageWithText">
                                                                <RequiredField IsRequired="true" ErrorText="Meeting Summary is required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapMemo>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="card-footer">
                                                <div class="center-content">
                                                    <asp:HiddenField ID="hfAddEditDebriefSessionPK" runat="server" Value="0" />
                                                    <uc:Submit ID="submitDebriefSession" runat="server" 
                                                        ValidationGroup="vgDebriefSession"
                                                        ControlCssClass="center-content"
                                                        OnSubmitClick="submitDebriefSession_Click" 
                                                        OnCancelClick="submitDebriefSession_CancelClick" 
                                                        OnValidationFailed="submitDebriefSession_ValidationFailed" />
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="lbDeleteDebriefSession" EventName="Click" />
                    </Triggers>
                </asp:UpdatePanel>
            </div>
        </div>
    </div>
    <div class="page-footer">
        <uc:Submit ID="submitMeetingDebrief" runat="server" ValidationGroup="vgMeetingDebrief"
            ControlCssClass="center-content"
            OnSubmitClick="submitMeetingDebrief_Click" 
            OnCancelClick="submitMeetingDebrief_CancelClick" 
            OnValidationFailed="submitMeetingDebrief_ValidationFailed" />
    </div>
    <div class="modal" id="divDeleteTeamMemberModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete Group Member</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this group member?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteTeamMember" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteTeamMemberModal" OnClick="lbDeleteTeamMember_Click"><i class="fas fa-check"></i>&nbsp;Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
    <div class="modal" id="divDeleteDebriefSessionModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete Session Debrief</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this session debrief?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteDebriefSession" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteDebriefSessionModal" OnClick="lbDeleteDebriefSession_Click"><i class="fas fa-check"></i>&nbsp;Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
</asp:Content>