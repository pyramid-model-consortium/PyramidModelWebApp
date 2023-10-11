<%@ Page Title="Master Cadre Dashboard" Language="C#" MasterPageFile="~/MasterPages/Dashboard.master" AutoEventWireup="true" CodeBehind="MasterCadreDashboard.aspx.cs" Inherits="Pyramid.Pages.MasterCadreDashboard" %>

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
            $('[ID$="lnkMCDashboard"]').addClass('active');

            //Show/hide the view only fields
            setViewOnlyVisibility();

            //Allow date format for DataTables sorting
            $.fn.dataTable.moment('MM/DD/YYYY');


            if (!$.fn.dataTable.isDataTable('#tblTrainingTrackerItemDates')) {
                $('#tblTrainingTrackerItemDates').DataTable({
                    responsive: {
                        details: {
                            type: 'column'
                        }
                    },
                    columnDefs: [
                        { orderable: false, targets: [4] },
                        { className: 'control', orderable: false, targets: 0 }
                    ],
                    order: [[1, 'desc']],
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

        function showHideTrackerIDFields() {
            //Determine whether to show course or event ID
            var showEvent = $('[ID$="hfActivityPKsRequiringEventID"]').val();
            var showCourse = $('[ID$="hfActivityPKsRequiringCourseID"]').val();

            //Arrays of activity PKs that require course/event IDs
            const eventPKs = showEvent.split(",");
            const coursePKs = showCourse.split(",");

            //Break out of the function if GetValue returns null
            if (ddTrackerActivity.GetValue() == null) {
                return;
            }

            //Retrieve the selected activity PK from the dropdown
            var activityPK = ddTrackerActivity.GetValue().toString();

            if (eventPKs.includes(activityPK)) {
                $('.type-tracker-event-div').removeClass('hidden');

            }
            else {
                $('.type-tracker-event-div').addClass('hidden');

            }


            if (coursePKs.includes(activityPK)) {
                $('.type-tracker-course-div').removeClass('hidden');

            }
            else {
                $('.type-tracker-course-div').addClass('hidden');

            }
        }

        function showHideDebriefIDFields() {
            //Determine whether to show course or event ID
            var showEvent = $('[ID$="hfActivityPKsRequiringEventID"]').val();
            var showCourse = $('[ID$="hfActivityPKsRequiringCourseID"]').val();

            //Arrays of activity PKs that require course/event IDs
            const eventPKs = showEvent.split(",");
            const coursePKs = showCourse.split(",");

            //Break out of the function if GetValue returns null
            if (ddDebriefActivity.GetValue() == null) {
                return;
            }

            //Retrieve the selected activity PK from the dropdown
            var activityPK = ddDebriefActivity.GetValue().toString();

            if (eventPKs.includes(activityPK)) {
                $('.type-debrief-event-div').removeClass('hidden');

            }
            else {
                $('.type-debrief-event-div').addClass('hidden');

            }


            if (coursePKs.includes(activityPK)) {
                $('.type-debrief-course-div').removeClass('hidden');

            }
            else {
                $('.type-debrief-course-div').addClass('hidden');

            }

        }

        function setViewOnlyVisibility() {
            //Hide controls if this is a view
            var isTrainingTrackerView = $('[ID$="hfTrainingTrackerViewOnly"]').val();
            if (isTrainingTrackerView === 'True') {
                $('.tracker-hide-on-view').addClass('hidden');
            }
            else {
                $('.tracker-hide-on-view').removeClass('hidden');
            }

            var isTrainingDebriefView = $('[ID$="hfTrainingDebriefViewOnly"]').val();
            if (isTrainingDebriefView === 'True') {
                $('.debrief-hide-on-view').addClass('hidden');
            }
            else {
                $('.debrief-hide-on-view').removeClass('hidden');
            }
        }
    </script>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <asp:HiddenField ID="hfActivityPKsRequiringEventID" runat="server" Value="False" />
    <asp:HiddenField ID="hfActivityPKsRequiringCourseID" runat="server" Value="False" />
    <asp:HiddenField ID="hfTrainingTrackerViewOnly" runat="server" Value="False" />
    <asp:HiddenField ID="hfTrainingDebriefViewOnly" runat="server" Value="False" />
    <asp:UpdatePanel ID="upDashboardMessaging" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <uc:Messaging ID="msgSys" runat="server" />
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="lbDeleteTrainingTracker" />
            <asp:AsyncPostBackTrigger ControlID="lbAddTrainingTracker" />
            <asp:AsyncPostBackTrigger ControlID="bsGRTrainingTrackers" />
            <asp:AsyncPostBackTrigger ControlID="submitTrainingTracker" />
            <asp:AsyncPostBackTrigger ControlID="lbDeleteTrainingDebrief" />
            <asp:AsyncPostBackTrigger ControlID="lbAddTrainingDebrief" />
            <asp:AsyncPostBackTrigger ControlID="bsGRTrainingDebriefs" />
            <asp:AsyncPostBackTrigger ControlID="submitTrainingDebrief" />
            <asp:AsyncPostBackTrigger ControlID="lbDeleteTrainingTrackerItemDate" />
            <asp:AsyncPostBackTrigger ControlID="lbAddTrainingTrackerItemDate" />
            <asp:AsyncPostBackTrigger ControlID="submitTrainingTrackerItemDate" />
            <asp:AsyncPostBackTrigger ControlID="repeatTrainingTrackerItemDates" />
        </Triggers>
    </asp:UpdatePanel>
    <div class="alert alert-warning">
        <i class="fas fa-exclamation-circle"></i>&nbsp;NOTE: The Master Cadre Member (username) columns are only searchable and sortable by the username.
    </div>
    <!-- Start of the activity tracker section -->
    <div id="divTrainingTrackers" runat="server" class="row">
        <div class="col-xl-12">
            <asp:HiddenField ID="hfDeleteTrainingTrackerPK" runat="server" Value="0" />
            <asp:UpdatePanel ID="upTrainingTracker" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                <ContentTemplate>
                    <div class="card bg-light">
                        <div class="card-header">
                            <span class="font-weight-bold">Pyramid Model Activity Tracker Forms</span>
                            <asp:LinkButton ID="lbAddTrainingTracker" runat="server" CssClass="btn btn-loader btn-primary float-right tracker-hide-on-view hidden" OnClick="lbAddTrainingTracker_Click"><i class="fas fa-plus"></i>&nbsp;Add New Activity Tracker</asp:LinkButton>
                        </div>
                        <div class="card-body">
                            <div class="row">
                                <div class="col-lg-12">
                                    <dx:BootstrapGridView ID="bsGRTrainingTrackers" runat="server" EnableCallBacks="false" EnableRowsCache="true"
                                        KeyFieldName="MasterCadreTrainingTrackerItemPK" OnHtmlRowCreated="bsGRTrainingTrackers_HtmlRowCreated"
                                        AutoGenerateColumns="false" DataSourceID="efTrainingTrackerDataSource">
                                        <SettingsPager PageSize="15" />
                                        <SettingsBootstrap Striped="true" />
                                        <SettingsBehavior EnableRowHotTrack="true" />
                                        <Settings ShowGroupPanel="false" />
                                        <SettingsSearchPanel Visible="true" ShowApplyButton="true" />
                                        <CssClasses IconHeaderSortUp="fas fa-long-arrow-alt-up" IconHeaderSortDown="fas fa-long-arrow-alt-down" IconShowAdaptiveDetailButton="fas fa-plus-circle" />
                                        <SettingsAdaptivity AdaptivityMode="HideDataCells" AllowOnlyOneAdaptiveDetailExpanded="true" AdaptiveColumnPosition="Left"></SettingsAdaptivity>
                                        <Columns>
                                            <dx:BootstrapGridViewDataColumn Name="MasterCadreMemberColumn" FieldName="MasterCadreMemberUsername" Caption="Master Cadre Member (username)" AdaptivePriority="2">
                                                <DataItemTemplate>
                                                    <asp:Label ID="lblMasterCadreMemberUsername" runat="server"></asp:Label>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDateColumn FieldName="StartDateTime" Caption="Start Date Time" PropertiesDateEdit-DisplayFormatString="MM/dd/yyyy hh:mm tt" SortIndex="0" SortOrder="Descending" AdaptivePriority="0"></dx:BootstrapGridViewDateColumn>
                                            <dx:BootstrapGridViewDateColumn FieldName="EndDateTime" Caption="End Date Time" PropertiesDateEdit-DisplayFormatString="MM/dd/yyyy hh:mm tt" AdaptivePriority="5"></dx:BootstrapGridViewDateColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="Activity" Caption="Pyramid Model Activity" AdaptivePriority="4"></dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewTextColumn FieldName="ParticipantFee" Caption="Participant Fee" PropertiesTextEdit-DisplayFormatString="c2" AdaptivePriority="6"></dx:BootstrapGridViewTextColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="FundingSource" Caption="Funding Source" AdaptivePriority="7"></dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="IsOpenToPublic" Caption="Open to the Public?" AdaptivePriority="8">
                                                <DataItemTemplate>
                                                    <i class='<%# (Convert.ToBoolean(Eval("IsOpenToPublic")) ? "fas fa-check green-text" : "fas fa-times red-text") %>'></i>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="TargetAudience" Caption="Target Audience" AdaptivePriority="9"></dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="MeetingFormat" Caption="Meeting Type" AdaptivePriority="10"></dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="MeetingLocation" Caption="Meeting Location (address, city, state and ZIP if applicable)" AdaptivePriority="11"></dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn Name="AspireEventNumColumn" FieldName="AspireEventNum" Caption="Event ID #" AdaptivePriority="12"></dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn Name="CourseNumColumn" FieldName="CourseIDNum" Caption="Course ID #" AdaptivePriority="12"></dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="NumHours" Caption="# of Hours Planned for Session" AdaptivePriority="13"></dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="DidEventOccur" Caption="Did Event Occur?" AdaptivePriority="14">
                                                <DataItemTemplate>
                                                    <i class='<%# (Convert.ToBoolean(Eval("DidEventOccur")) ? "fas fa-check green-text" : "fas fa-times red-text") %>'></i>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn Name="StateNameColumn" FieldName="State" Caption="State" AdaptivePriority="15"></dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewButtonEditColumn Settings-AllowDragDrop="False" AdaptivePriority="1" CssClasses-DataCell="text-center">
                                                <DataItemTemplate>
                                                    <div class="btn-group">
                                                        <button type="button" class="btn btn-secondary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                            Actions
                                                        </button>
                                                        <div class="dropdown-menu dropdown-menu-right">
                                                            <asp:LinkButton ID="lbViewTrainingTracker" runat="server" CssClass="dropdown-item" OnClick="lbViewTrainingTracker_Click"><i class="fas fa-list"></i>&nbsp;View Details</asp:LinkButton>
                                                            <asp:LinkButton ID="lbEditTrainingTracker" runat="server" CssClass="dropdown-item tracker-hide-on-view" OnClick="lbEditTrainingTracker_Click"><i class="fas fa-edit"></i> Edit</asp:LinkButton>
                                                            <button class="dropdown-item delete-gridview tracker-hide-on-view" data-pk='<%# Eval("MasterCadreTrainingTrackerItemPK") %>' data-hf="hfDeleteTrainingTrackerPK" data-target="#divDeleteTrainingTrackerModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                        </div>
                                                    </div>
                                                    <asp:Label ID="lblTrainingTrackerPK" runat="server" Visible="false" Text='<%# Eval("MasterCadreTrainingTrackerItemPK") %>'></asp:Label>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewButtonEditColumn>
                                        </Columns>
                                    </dx:BootstrapGridView>
                                    <dx:EntityServerModeDataSource ID="efTrainingTrackerDataSource" runat="server" OnSelecting="efTrainingTrackerDataSource_Selecting" />
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-12">
                                    <div id="divAddEditTrainingTracker" runat="server" class="card mt-2" visible="false">
                                        <div class="card-header">
                                            <asp:Label ID="lblAddEditTrainingTracker" runat="server" Text=""></asp:Label>
                                        </div>
                                        <div class="card-body">
                                            <div id="divAddTrainingTrackerAlert" runat="server" class="alert alert-primary">
                                                <i class="fas fa-info-circle"></i>&nbsp;After you save the activity once, you will be able to record additional dates and times for this activity.
                                            </div>
                                            <div class="row">
                                                <div class="col-lg-12">
                                                    <asp:Label runat="server" AssociatedControlID="lblTrackerMCUsername" CssClass="d-block" Text="Master Cadre Member (username)"></asp:Label>
                                                    <asp:Label ID="lblTrackerMCUsername" runat="server" CssClass="d-block" Text=""></asp:Label>
                                                </div>
                                            </div>
                                            <div class="row">
                                                <div class="col-lg-3">
                                                    <dx:BootstrapComboBox ID="ddTrackerActivity" runat="server" Caption="Pyramid Model Activity" NullText="--Select--"
                                                        TextField="Description" ValueField="CodeMasterCadreActivityPK" ValueType="System.Int32" AllowNull="true"
                                                        IncrementalFilteringMode="Contains" AllowMouseWheel="false" ClientInstanceName="ddTrackerActivity">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ClientSideEvents Init="showHideTrackerIDFields" SelectedIndexChanged="showHideTrackerIDFields" />
                                                        <ValidationSettings ValidationGroup="vgTrainingTracker" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                            <RequiredField IsRequired="true" ErrorText="Pyramid Model Activity is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapComboBox>
                                                </div>
                                                <div id="divTrainingTrackerItemInitialDate" runat="server" class="col-lg-9">
                                                    <div class="row">
                                                        <div class="col-lg-4">
                                                            <dx:BootstrapDateEdit ID="deTrackerItemInitialDate" runat="server" Caption="Initial Date" EditFormat="Date"
                                                                EditFormatString="MM/dd/yyyy" UseMaskBehavior="true" NullText="--Select--"
                                                                AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900">
                                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                                <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                                                <ValidationSettings ValidationGroup="vgTrainingTracker" ErrorDisplayMode="ImageWithText">
                                                                    <RequiredField IsRequired="true" ErrorText="Initial Date is required!" />
                                                                </ValidationSettings>
                                                            </dx:BootstrapDateEdit>
                                                        </div>
                                                        <div class="col-lg-4">
                                                            <dx:BootstrapTimeEdit ID="teTrackerItemInitialStartTime" runat="server" Caption="Initial Start Time" EditFormat="Time"
                                                                EditFormatString="hh:mm tt" NullText=""
                                                                SpinButtons-ClientVisible="false">
                                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                                <ValidationSettings ValidationGroup="vgTrainingTracker" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                                    <RequiredField IsRequired="true" ErrorText="Initial Start Time is required!" />
                                                                </ValidationSettings>
                                                            </dx:BootstrapTimeEdit>
                                                        </div>
                                                        <div class="col-lg-4">
                                                            <dx:BootstrapTimeEdit ID="teTrackerItemInitialEndTime" runat="server" Caption="Initial End Time" EditFormat="Time"
                                                                EditFormatString="hh:mm tt" NullText=""
                                                                SpinButtons-ClientVisible="false" OnValidation="teTrackerItemInitialEndTime_Validation">
                                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                                <ValidationSettings ValidationGroup="vgTrainingTracker" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                                    <RequiredField IsRequired="true" ErrorText="Initial End Time is required!" />
                                                                </ValidationSettings>
                                                            </dx:BootstrapTimeEdit>
                                                        </div>
                                                    </div>

                                                </div>
                                            </div>
                                            <div class="row">
                                                <div class="col-lg-3">
                                                    <dx:BootstrapButtonEdit ID="beTrackerParticipantFee" runat="server" Caption="Fee for Participants" MaxLength="18"
                                                        OnValidation="beTrackerParticipantFee_Validation">
                                                        <Buttons>
                                                            <dx:BootstrapEditButton IconCssClass="fas fa-dollar-sign" Position="Left" />
                                                        </Buttons>
                                                        <ClientSideEvents ButtonClick="function(s, e) { return false; }" />
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgTrainingTracker" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                            <RequiredField IsRequired="true" ErrorText="Fee for Participants is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapButtonEdit>
                                                </div>
                                                <div class="col-lg-3">
                                                    <dx:BootstrapComboBox ID="ddTrackerFundingSource" runat="server" Caption="Funding Source" NullText="--Select--"
                                                        TextField="Description" ValueField="CodeMasterCadreFundingSourcePK" ValueType="System.Int32" AllowNull="true"
                                                        IncrementalFilteringMode="Contains" AllowMouseWheel="false">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgTrainingTracker" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                            <RequiredField IsRequired="true" ErrorText="Funding Source is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapComboBox>
                                                </div>
                                                <div class="col-lg-3">
                                                    <dx:BootstrapComboBox ID="ddTrackerIsOpenToPublic" runat="server" NullText="--Select--"
                                                        Caption="Open to the Public?" ValueType="System.Boolean" IncrementalFilteringMode="StartsWith">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgTrainingTracker" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="true" ErrorText="Open to the Public? is required!" />
                                                        </ValidationSettings>
                                                        <Items>
                                                            <dx:BootstrapListEditItem Text="Yes" Value="True"></dx:BootstrapListEditItem>
                                                            <dx:BootstrapListEditItem Text="No" Value="False"></dx:BootstrapListEditItem>
                                                        </Items>
                                                    </dx:BootstrapComboBox>
                                                </div>
                                                <div class="col-lg-3">
                                                    <dx:BootstrapComboBox ID="ddTrackerMeetingType" runat="server" Caption="Meeting Type" NullText="--Select--"
                                                        TextField="Description" ValueField="CodeMeetingFormatPK" ValueType="System.Int32" AllowNull="true"
                                                        IncrementalFilteringMode="Contains" AllowMouseWheel="false">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgTrainingTracker" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                            <RequiredField IsRequired="true" ErrorText="Meeting Type is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapComboBox>
                                                </div>
                                            </div>
                                            <div class="row">
                                                <div class="col-lg-4">
                                                    <dx:BootstrapMemo ID="txtTrackerMeetingLocation" runat="server" Caption="Meeting Location (address, city, state and ZIP if applicable)" MaxLength="3000" Rows="2">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgTrainingTracker" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="true" ErrorText="Meeting Location (address, city, state and ZIP if applicable) is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapMemo>
                                                </div>
                                                <div class="col-lg-3">
                                                    <dx:BootstrapComboBox ID="ddTrackerDidEventOccur" runat="server" NullText="--Select--"
                                                        Caption="Did the Event Occur?" ValueType="System.Boolean" IncrementalFilteringMode="StartsWith">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgTrainingTracker" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="true" ErrorText="Did the Event Occur? is required!" />
                                                        </ValidationSettings>
                                                        <Items>
                                                            <dx:BootstrapListEditItem Text="Yes" Value="True"></dx:BootstrapListEditItem>
                                                            <dx:BootstrapListEditItem Text="No" Value="False"></dx:BootstrapListEditItem>
                                                        </Items>
                                                    </dx:BootstrapComboBox>
                                                </div>
                                                <div class="col-lg-4">
                                                    <dx:BootstrapMemo ID="txtTrackerTargetAudience" runat="server" Caption="Target Audience" MaxLength="1000" Rows="2">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgTrainingTracker" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="true" ErrorText="Target Audience is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapMemo>
                                                </div>
                                            </div>
                                            <div class="row">
                                                <div class="col-lg-3">
                                                    <dx:BootstrapTextBox ID="txtTrackerNumHours" runat="server" Caption="# of Hours Planned for Session" MaxLength="10"
                                                        OnValidation="txtTrackerNumHours_Validation">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgTrainingTracker" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                            <RequiredField IsRequired="true" ErrorText="# of Hours Planned for Session is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapTextBox>
                                                </div>
                                                <div class="col-lg-3 type-tracker-event-div hidden">
                                                    <dx:BootstrapTextBox ID="txtTrackerEventNum" runat="server" Caption="Event ID #" MaxLength="100" OnValidation="txtTrackerEventNum_Validation">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgTrainingTracker" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                            <RequiredField IsRequired="false" ErrorText="Event ID # is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapTextBox>
                                                </div>
                                                <div class="col-lg-3 type-tracker-course-div hidden">
                                                    <dx:BootstrapTextBox ID="txtTrackerCourseNum" runat="server" Caption="Course ID #" MaxLength="100" OnValidation="txtTrackerCourseNum_Validation">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgTrainingTracker" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                            <RequiredField IsRequired="false" ErrorText="Course ID # is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapTextBox>
                                                </div>
                                            </div>
                                            <div id="divTrainingTrackerItemDateHistory" runat="server" class="row">
                                                <div class="col-xl-12">
                                                    <asp:HiddenField ID="hfDeleteMasterCadreTrainingTrackerItemDatePK" runat="server" Value="0" />
                                                    <div class="card bg-light">
                                                        <div class="card-header">
                                                            Tracker Item Dates
                                                            <asp:LinkButton ID="lbAddTrainingTrackerItemDate" runat="server" CssClass="btn btn-loader btn-primary float-right tracker-hide-on-view hidden" OnClick="lbAddTrainingTrackerItemDate_Click"><i class="fas fa-plus"></i>&nbsp;Add New Tracker Item Date</asp:LinkButton>
                                                        </div>
                                                        <div class="card-body">
                                                            <div class="row">
                                                                <div class="col-lg-12">
                                                                    <label>All date records for this Tracker Item</label>
                                                                    <asp:Repeater ID="repeatTrainingTrackerItemDates" runat="server" ItemType="Pyramid.Models.MasterCadreTrainingTrackerItemDate">
                                                                        <HeaderTemplate>
                                                                            <table id="tblTrainingTrackerItemDates" class="table table-striped table-bordered table-hover">
                                                                                <thead>
                                                                                    <tr>
                                                                                        <th data-priority="1"></th>
                                                                                        <th data-priority="3">Date</th>
                                                                                        <th data-priority="4">Start Time</th>
                                                                                        <th data-priority="5">End Time</th>
                                                                                        <th data-priority="2"></th>
                                                                                    </tr>
                                                                                </thead>
                                                                                <tbody>
                                                                        </HeaderTemplate>
                                                                        <ItemTemplate>
                                                                            <tr>
                                                                                <td></td>
                                                                                <td><%# Item.StartDateTime.ToString("MM/dd/yyyy") %></td>
                                                                                <td><%# Item.StartDateTime.ToString("h:mm tt") %></td>
                                                                                <td><%# Item.EndDateTime.ToString("h:mm tt") %></td>
                                                                                <td class="text-center tracker-hide-on-view">
                                                                                    <div class="btn-group">
                                                                                        <button type="button" class="btn btn-secondary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                                                            Actions
                                                                                        </button>
                                                                                        <div class="dropdown-menu dropdown-menu-right">
                                                                                            <asp:LinkButton ID="lbEditTrainingTrackerItemDate" runat="server" CssClass="dropdown-item tracker-hide-on-view" OnClick="lbEditTrainingTrackerItemDate_Click"><i class="fas fa-edit"></i>&nbsp;Edit</asp:LinkButton>
                                                                                            <button class="dropdown-item delete-gridview tracker-hide-on-view" data-pk='<%# Item.MasterCadreTrainingTrackerItemDatePK%>' data-hf="hfDeleteMasterCadreTrainingTrackerItemDatePK" data-target="#divDeleteTrainingTrackerItemDateModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                                                        </div>
                                                                                    </div>
                                                                                    <asp:Label ID="lblMasterCadreTrainingTrackerItemDatePK" runat="server" Visible="false" Text='<%# Item.MasterCadreTrainingTrackerItemDatePK %>'></asp:Label>
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
                                                                    <div id="divAddEditTrainingTrackerItemDate" runat="server" class="card mt-2" visible="false">
                                                                        <div class="card-header">
                                                                            <asp:Label ID="lblAddEditTrainingTrackerItemDate" runat="server" Text=""></asp:Label>
                                                                        </div>
                                                                        <div class="card-body">
                                                                            <div class="row">
                                                                                <div class="col-lg-4">
                                                                                    <dx:BootstrapDateEdit ID="deTrainingTrackerItemDate" runat="server" Caption="Date" EditFormat="Date"
                                                                                        EditFormatString="MM/dd/yyyy" UseMaskBehavior="true" NullText="--Select--"
                                                                                        AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900">
                                                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                                                        <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                                                                        <ValidationSettings ValidationGroup="vgTrackerDate" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                                                            <RequiredField IsRequired="true" ErrorText="Date is required!" />
                                                                                        </ValidationSettings>
                                                                                    </dx:BootstrapDateEdit>
                                                                                </div>
                                                                                <div class="col-lg-4">
                                                                                    <dx:BootstrapTimeEdit ID="teTrainingTrackerItemStartTime" runat="server" Caption="Start Time" EditFormat="Time"
                                                                                        EditFormatString="hh:mm tt" NullText=""
                                                                                        SpinButtons-ClientVisible="false">
                                                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                                                        <ValidationSettings ValidationGroup="vgTrackerDate" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                                                            <RequiredField IsRequired="true" ErrorText="Start Time is required!" />
                                                                                        </ValidationSettings>
                                                                                    </dx:BootstrapTimeEdit>
                                                                                </div>
                                                                                <div class="col-lg-4">
                                                                                    <dx:BootstrapTimeEdit ID="teTrainingTrackerItemEndTime" runat="server" Caption="End Time" EditFormat="Time"
                                                                                        EditFormatString="hh:mm tt" NullText=""
                                                                                        SpinButtons-ClientVisible="false" OnValidation="teTrainingTrackerItemEndTime_Validation">
                                                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                                                        <ValidationSettings ValidationGroup="vgTrackerDate" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                                                            <RequiredField IsRequired="true" ErrorText="End Time is required!" />
                                                                                        </ValidationSettings>
                                                                                    </dx:BootstrapTimeEdit>
                                                                                </div>
                                                                            </div>
                                                                        </div>
                                                                        <div class="card-footer">
                                                                            <div class="center-content">
                                                                                <asp:HiddenField ID="hfAddEditTrainingTrackerItemDatePK" runat="server" Value="0" />
                                                                                <uc:Submit ID="submitTrainingTrackerItemDate" runat="server"
                                                                                    ValidationGroup="vgTrackerDate"
                                                                                    ControlCssClass="center-content"
                                                                                    OnSubmitClick="submitTrainingTrackerItemDate_Click"
                                                                                    OnCancelClick="submitTrainingTrackerItemDate_CancelClick"
                                                                                    OnValidationFailed="submitTrainingTrackerItemDate_ValidationFailed" />
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
                                                <asp:HiddenField ID="hfAddEditTrainingTrackerPK" runat="server" Value="0" />
                                                <uc:Submit ID="submitTrainingTracker" runat="server"
                                                    ValidationGroup="vgTrainingTracker"
                                                    ControlCssClass="center-content"
                                                    OnSubmitClick="submitTrainingTracker_Click"
                                                    OnCancelClick="submitTrainingTracker_CancelClick"
                                                    OnValidationFailed="submitTrainingTracker_ValidationFailed" />
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="lbDeleteTrainingTracker" EventName="Click" />
                    <asp:AsyncPostBackTrigger ControlID="lbDeleteTrainingTrackerItemDate" EventName="Click" />
                </Triggers>
            </asp:UpdatePanel>
        </div>
    </div>
    <!-- End of the activity tracker section -->
    <!-- Start of the training debrief section -->
    <div id="divTrainingDebriefs" runat="server" class="row">
        <div class="col-xl-12">
            <asp:HiddenField ID="hfDeleteTrainingDebriefPK" runat="server" Value="0" />
            <asp:UpdatePanel ID="upTrainingDebrief" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                <ContentTemplate>
                    <div class="card bg-light">
                        <div class="card-header">
                            <span class="font-weight-bold">Pyramid Model Activity Debrief Forms</span>
                            <asp:LinkButton ID="lbAddTrainingDebrief" runat="server" CssClass="btn btn-loader btn-primary float-right debrief-hide-on-view hidden" OnClick="lbAddTrainingDebrief_Click"><i class="fas fa-plus"></i>&nbsp;Add New Activity Debrief</asp:LinkButton>
                        </div>
                        <div class="card-body">
                            <div class="row">
                                <div class="col-lg-12">
                                    <dx:BootstrapGridView ID="bsGRTrainingDebriefs" runat="server" EnableCallBacks="false" EnableRowsCache="true"
                                        KeyFieldName="MasterCadreTrainingDebriefPK" OnHtmlRowCreated="bsGRTrainingDebriefs_HtmlRowCreated"
                                        AutoGenerateColumns="false" DataSourceID="efTrainingDebriefDataSource">
                                        <SettingsPager PageSize="15" />
                                        <SettingsBootstrap Striped="true" />
                                        <SettingsBehavior EnableRowHotTrack="true" />
                                        <Settings ShowGroupPanel="false" />
                                        <SettingsSearchPanel Visible="true" ShowApplyButton="true" />
                                        <CssClasses IconHeaderSortUp="fas fa-long-arrow-alt-up" IconHeaderSortDown="fas fa-long-arrow-alt-down" IconShowAdaptiveDetailButton="fas fa-plus-circle" />
                                        <SettingsAdaptivity AdaptivityMode="HideDataCells" AllowOnlyOneAdaptiveDetailExpanded="true" AdaptiveColumnPosition="Left"></SettingsAdaptivity>
                                        <Columns>
                                            <dx:BootstrapGridViewDataColumn Name="MasterCadreMemberColumn" FieldName="MasterCadreMemberUsername" Caption="Master Cadre Member (username)" AdaptivePriority="4">
                                                <DataItemTemplate>
                                                    <asp:Label ID="lblMasterCadreMemberUsername" runat="server"></asp:Label>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="CodeMasterCadreActivity.Description" Caption="Pyramid Model Activity" AdaptivePriority="5"></dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDateColumn FieldName="DateCompleted" Caption="Date Completed" PropertiesDateEdit-DisplayFormatString="MM/dd/yyyy" SortIndex="0" SortOrder="Descending" AdaptivePriority="5"></dx:BootstrapGridViewDateColumn>
                                            <dx:BootstrapGridViewDataColumn Name="AspireEventNumColumn" FieldName="AspireEventNum" Caption="Event ID #"></dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn Name="CourseNumColumn" FieldName="CourseIDNum" Caption="Course ID #" ></dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="CodeMeetingFormat.Description" Caption="Training Format" AdaptivePriority="5"></dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="MeetingLocation" Caption="Meeting Location (address, city, state and ZIP if applicable)" AdaptivePriority="5"></dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="NumAttendees" Caption="# of Attendees" AdaptivePriority="5"></dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="NumEvalsReceived" Caption="# of Evals Received" AdaptivePriority="5"></dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn Name="AspireUploadColumn" FieldName="WasUploadedToAspire" Caption="Attended & Evals Uploaded to ASPIRE?" AdaptivePriority="6">
                                                <DataItemTemplate>
                                                    <i class='<%# (Eval("WasUploadedToAspire") == null ? "" : Convert.ToBoolean(Eval("WasUploadedToAspire")) ? "fas fa-check green-text" : "fas fa-times red-text") %>'></i>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn Name="StateNameColumn" FieldName="State.Name" Caption="State" AdaptivePriority="18"></dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewButtonEditColumn Settings-AllowDragDrop="False" AdaptivePriority="1" CssClasses-DataCell="text-center">
                                                <DataItemTemplate>
                                                    <div class="btn-group">
                                                        <button type="button" class="btn btn-secondary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                            Actions
                                                        </button>
                                                        <div class="dropdown-menu dropdown-menu-right">
                                                            <asp:LinkButton ID="lbViewTrainingDebrief" runat="server" CssClass="dropdown-item" OnClick="lbViewTrainingDebrief_Click"><i class="fas fa-list"></i>&nbsp;View Details</asp:LinkButton>
                                                            <asp:LinkButton ID="lbEditTrainingDebrief" runat="server" CssClass="dropdown-item debrief-hide-on-view" OnClick="lbEditTrainingDebrief_Click"><i class="fas fa-edit"></i> Edit</asp:LinkButton>
                                                            <button class="dropdown-item delete-gridview debrief-hide-on-view" data-pk='<%# Eval("MasterCadreTrainingDebriefPK") %>' data-hf="hfDeleteTrainingDebriefPK" data-target="#divDeleteTrainingDebriefModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                        </div>
                                                    </div>
                                                    <asp:Label ID="lblTrainingDebriefPK" runat="server" Visible="false" Text='<%# Eval("MasterCadreTrainingDebriefPK") %>'></asp:Label>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewButtonEditColumn>
                                        </Columns>
                                    </dx:BootstrapGridView>
                                    <dx:EntityServerModeDataSource ID="efTrainingDebriefDataSource" runat="server" OnSelecting="efTrainingDebriefDataSource_Selecting" />
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-12">
                                    <div id="divAddEditTrainingDebrief" runat="server" class="card mt-2" visible="false">
                                        <div class="card-header">
                                            <asp:Label ID="lblAddEditTrainingDebrief" runat="server" Text=""></asp:Label>
                                        </div>
                                        <div class="card-body">
                                            <div class="row">
                                                <div class="col-lg-12">
                                                    <asp:Label runat="server" AssociatedControlID="lblDebriefMCUsername" CssClass="d-block" Text="Master Cadre Member (username)"></asp:Label>
                                                    <asp:Label ID="lblDebriefMCUsername" runat="server" CssClass="d-block" Text=""></asp:Label>
                                                </div>
                                            </div>
                                            <div class="row">
                                                <div class="col-lg-3">
                                                    <dx:BootstrapComboBox ID="ddDebriefActivity" runat="server" Caption="Pyramid Model Activity" NullText="--Select--"
                                                        TextField="Description" ValueField="CodeMasterCadreActivityPK" ValueType="System.Int32" AllowNull="true"
                                                        IncrementalFilteringMode="Contains" AllowMouseWheel="false" ClientInstanceName="ddDebriefActivity">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ClientSideEvents Init="showHideDebriefIDFields" SelectedIndexChanged="showHideDebriefIDFields" />
                                                        <ValidationSettings ValidationGroup="vgTrainingDebrief" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                            <RequiredField IsRequired="true" ErrorText="Pyramid Model Activity is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapComboBox>
                                                </div>
                                                <div class="col-lg-3">
                                                    <dx:BootstrapDateEdit ID="deDebriefDateCompleted" runat="server" Caption="Date Completed" EditFormat="Date"
                                                        EditFormatString="MM/dd/yyyy" UseMaskBehavior="true" NullText="--Select--"
                                                        AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900"
                                                        OnValidation="deDebriefDateCompleted_Validation">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                                        <ValidationSettings ValidationGroup="vgTrainingDebrief" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="true" ErrorText="Date Completed is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapDateEdit>
                                                </div>
                                                <div class="col-lg-3">
                                                    <dx:BootstrapComboBox ID="ddDebriefTrainingFormat" runat="server" Caption="Training Format" NullText="--Select--"
                                                        TextField="Description" ValueField="CodeMeetingFormatPK" ValueType="System.Int32" AllowNull="true"
                                                        IncrementalFilteringMode="Contains" AllowMouseWheel="false">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgTrainingDebrief" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                            <RequiredField IsRequired="true" ErrorText="Training Format is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapComboBox>
                                                </div>
                                                <div class="col-lg-3">
                                                    <dx:BootstrapMemo ID="txtDebriefMeetingLocation" runat="server" Caption="Meeting Location (address, city, state and ZIP if applicable)" MaxLength="3000" Rows="2">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgTrainingDebrief" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="true" ErrorText="Meeting Location (address, city, state and ZIP if applicable) is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapMemo>
                                                </div>
                                            </div>
                                            <div class="row">
                                                <div class="col-lg-3">
                                                    <dx:BootstrapTextBox ID="txtDebriefNumAttendees" runat="server" Caption="# of Attendees" MaxLength="3"
                                                        OnValidation="txtDebriefNumAttendees_Validation">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgTrainingDebrief" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                            <RequiredField IsRequired="true" ErrorText="# of Attendees is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapTextBox>
                                                </div>
                                                <div class="col-lg-3">
                                                    <dx:BootstrapTextBox ID="txtDebriefNumEvalsReceived" runat="server" Caption="# of Evaluations Received" MaxLength="3"
                                                        OnValidation="txtDebriefNumEvalsReceived_Validation">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgTrainingDebrief" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                            <RequiredField IsRequired="true" ErrorText="# of Evaluations Received is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapTextBox>
                                                </div>
                                                <div class="col-lg-3 type-debrief-event-div hidden">
                                                    <dx:BootstrapTextBox ID="txtDebriefEventNum" runat="server" Caption="Event ID #" MaxLength="100" OnValidation="txtDebriefEventNum_Validation">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgTrainingDebrief" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                            <RequiredField IsRequired="false" ErrorText="Event ID # is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapTextBox>
                                                </div>
                                                <div class="col-lg-3 type-debrief-course-div hidden">
                                                    <dx:BootstrapTextBox ID="txtDebriefCourseNum" runat="server" Caption="Course ID #" MaxLength="100" OnValidation="txtDebriefCourseNum_Validation">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgTrainingDebrief" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                            <RequiredField IsRequired="false" ErrorText="Course ID # is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapTextBox>
                                                </div>
                                                <div class="col-lg-3">
                                                    <dx:BootstrapComboBox ID="ddDebriefWasUploadedToAspire" runat="server" NullText="--Select--" AllowNull="true"
                                                        Caption="Attended & Evaluations Uploaded to ASPIRE?" ValueType="System.Boolean" IncrementalFilteringMode="StartsWith">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgTrainingDebrief" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="false" ErrorText="Attended & Evaluations Uploaded to ASPIRE? is required!" />
                                                        </ValidationSettings>
                                                        <Items>
                                                            <dx:BootstrapListEditItem Text="Yes" Value="True"></dx:BootstrapListEditItem>
                                                            <dx:BootstrapListEditItem Text="No" Value="False"></dx:BootstrapListEditItem>
                                                        </Items>
                                                    </dx:BootstrapComboBox>
                                                </div>
                                            </div>
                                            <div class="row">
                                                <div class="col-lg-6">
                                                    <dx:BootstrapMemo ID="txtDebriefCoachingInterest" runat="server" Caption="Was there interest in coaching or additional coaching circles/community of practice? What are the plans for follow-up?" MaxLength="8000" Rows="2">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgTrainingDebrief" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="true" ErrorText="Was there interest in coaching... is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapMemo>
                                                </div>
                                                <div class="col-lg-6">
                                                    <dx:BootstrapMemo ID="txtDebriefReflection" runat="server" Caption="Reflection - How did the session go overall?" MaxLength="8000" Rows="2">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgTrainingDebrief" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="true" ErrorText="Reflection - How did the session go overall? is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapMemo>
                                                </div>
                                            </div>
                                            <div class="row">
                                                <div class="col-lg-6">
                                                    <dx:BootstrapMemo ID="txtDebriefAssistanceNeeded" runat="server" Caption="Assistance I Need - Who do I Plan to Contact" MaxLength="8000" Rows="2">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgTrainingDebrief" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="true" ErrorText="Assistance I Need - Who do I Plan to Contact is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapMemo>
                                                </div>
                                            </div>


                                        </div>
                                        <div class="card-footer">
                                            <div class="center-content">
                                                <asp:HiddenField ID="hfAddEditTrainingDebriefPK" runat="server" Value="0" />
                                                <uc:Submit ID="submitTrainingDebrief" runat="server"
                                                    ValidationGroup="vgTrainingDebrief"
                                                    ControlCssClass="center-content"
                                                    OnSubmitClick="submitTrainingDebrief_Click"
                                                    OnCancelClick="submitTrainingDebrief_CancelClick"
                                                    OnValidationFailed="submitTrainingDebrief_ValidationFailed" />
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="lbDeleteTrainingDebrief" EventName="Click" />
                </Triggers>
            </asp:UpdatePanel>
        </div>
    </div>
    <!-- End of the training debrief section -->
    <div class="modal" id="divDeleteTrainingTrackerModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete Activity Tracker Item</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this Activity Tracker Item?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteTrainingTracker" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteTrainingTrackerModal" OnClick="lbDeleteTrainingTracker_Click"><i class="fas fa-check"></i>&nbsp;Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
    <div class="modal" id="divDeleteTrainingDebriefModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete Training Debrief Form</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this Training Debrief Form?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteTrainingDebrief" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteTrainingDebriefModal" OnClick="lbDeleteTrainingDebrief_Click"><i class="fas fa-check"></i>&nbsp;Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
    <div class="modal" id="divDeleteTrainingTrackerItemDateModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete Tracker Date</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this Tracker Date?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteTrainingTrackerItemDate" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteTrainingTrackerItemDateModal" OnClick="lbDeleteTrainingTrackerItemDate_Click"><i class="fas fa-check"></i>&nbsp;Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>

</asp:Content>
