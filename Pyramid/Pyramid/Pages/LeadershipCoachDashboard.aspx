<%@ Page Title="Leadership Coach Dashboard" Language="C#" MasterPageFile="~/MasterPages/Dashboard.master" AutoEventWireup="true" CodeBehind="LeadershipCoachDashboard.aspx.cs" Inherits="Pyramid.Pages.LeadershipCoachDashboard" %>
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
            $('[ID$="lnkLCDashboard"]').addClass('active');

            //Show/hide the view only fields
            setViewOnlyVisibility();
        }

        function setViewOnlyVisibility() {
            //Hide controls if this is a view
            var isLCLView = $('[ID$="hfLeadershipCoachLogViewOnly"]').val();
            if (isLCLView === 'True') {
                $('.lcl-hide-on-view').addClass('hidden');
            }
            else {
                $('.lcl-hide-on-view').removeClass('hidden');
            }

            var isMeetingScheduleView = $('[ID$="hfMeetingScheduleViewOnly"]').val();
            if (isMeetingScheduleView === 'True') {
                $('.schedule-hide-on-view').addClass('hidden');
            }
            else {
                $('.schedule-hide-on-view').removeClass('hidden');
            }

            var isMeetingScheduleView = $('[ID$="hfMeetingDebriefViewOnly"]').val();
            if (isMeetingScheduleView === 'True') {
                $('.debrief-hide-on-view').addClass('hidden');
            }
            else {
                $('.debrief-hide-on-view').removeClass('hidden');
            }

            var isMeetingScheduleView = $('[ID$="hfCCMeetingScheduleViewOnly"]').val();
            if (isMeetingScheduleView === 'True') {
                $('.cc-schedule-hide-on-view').addClass('hidden');
            }
            else {
                $('.cc-schedule-hide-on-view').removeClass('hidden');
            }

            var isMeetingScheduleView = $('[ID$="hfCCMeetingDebriefViewOnly"]').val();
            if (isMeetingScheduleView === 'True') {
                $('.cc-debrief-hide-on-view').addClass('hidden');
            }
            else {
                $('.cc-debrief-hide-on-view').removeClass('hidden');
            }
        }

        function checkMeetingScheduleType() {
            //Handle the meeting schedule team type dropdown
            var scheduleType = ddMeetingScheduleType.GetValue();

            //Check to see if this is a Program or Hub schedule
            if (scheduleType === 'Program') {
                //Show the program dropdown and hide the hub dropdown
                $('#divMeetingScheduleProgram').removeClass('hidden');
                $('#divMeetingScheduleHub').addClass('hidden');
            }
            else if (scheduleType === 'Hub') {
                //Show the hub dropdown and hide the program dropdown
                $('#divMeetingScheduleHub').removeClass('hidden');
                $('#divMeetingScheduleProgram').addClass('hidden');
            }
        }
    </script>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <asp:HiddenField ID="hfLeadershipCoachLogViewOnly" runat="server" Value="False" />
    <asp:HiddenField ID="hfMeetingScheduleViewOnly" runat="server" Value="False" />
    <asp:HiddenField ID="hfMeetingDebriefViewOnly" runat="server" Value="False" />
    <asp:HiddenField ID="hfCCMeetingScheduleViewOnly" runat="server" Value="False" />
    <asp:HiddenField ID="hfCCMeetingDebriefViewOnly" runat="server" Value="False" />
    <asp:UpdatePanel ID="upDashboardMessaging" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <uc:Messaging ID="msgSys" runat="server" />
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="lbDeleteLeadershipCoachLog" />
            <asp:AsyncPostBackTrigger ControlID="bsGRLeadershipCoachLogs" />
            <asp:AsyncPostBackTrigger ControlID="lbDeleteMeetingSchedule" />
            <asp:AsyncPostBackTrigger ControlID="lbAddMeetingSchedule" />
            <asp:AsyncPostBackTrigger ControlID="bsGRMeetingSchedules" />
            <asp:AsyncPostBackTrigger ControlID="submitMeetingSchedule" />
            <asp:AsyncPostBackTrigger ControlID="lbDeleteMeetingDebrief" />
            <asp:AsyncPostBackTrigger ControlID="bsGRMeetingDebriefs" />
            <asp:AsyncPostBackTrigger ControlID="lbDeleteCCMeetingSchedule" />
            <asp:AsyncPostBackTrigger ControlID="lbAddCCMeetingSchedule" />
            <asp:AsyncPostBackTrigger ControlID="bsGRCCMeetingSchedules" />
            <asp:AsyncPostBackTrigger ControlID="submitCCMeetingSchedule" />
            <asp:AsyncPostBackTrigger ControlID="lbDeleteCCMeetingDebrief" />
            <asp:AsyncPostBackTrigger ControlID="bsGRCCMeetingDebriefs" />
        </Triggers>
    </asp:UpdatePanel>
    <div class="alert alert-warning">
        <i class="fas fa-exclamation-circle"></i>&nbsp;NOTE: The Leadership Coach (username) columns are only searchable and sortable by the username.
    </div>
    <!-- Start of the Leadership Coach Log section -->
    <div id="divLeadershipCoachLogs" runat="server" class="row">
        <div class="col-xl-12">
            <asp:HiddenField ID="hfDeleteLeadershipCoachLogPK" runat="server" Value="0" />
            <asp:UpdatePanel ID="upLeadershipCoachLog" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                <ContentTemplate>
                    <div class="card bg-light">
                        <div class="card-header">
                            <span class="font-weight-bold">Leadership Coach Logs</span>
                            <a href="/Pages/LeadershipCoachLog.aspx?LeadershipCoachLogPK=0&action=Add" class="btn btn-loader btn-primary float-right lcl-hide-on-view hidden"><i class="fas fa-plus"></i>&nbsp;Add New Leadership Coach Log</a>
                        </div>
                        <div class="card-body">
                            <div class="row">
                                <div class="col-lg-12">
                                    <dx:BootstrapGridView ID="bsGRLeadershipCoachLogs" runat="server" EnableCallBacks="false" EnableRowsCache="true" 
                                        KeyFieldName="LeadershipCoachLogPK" OnHtmlRowCreated="bsGRLeadershipCoachLogs_HtmlRowCreated"
                                        AutoGenerateColumns="false" DataSourceID="efLeadershipCoachLogDataSource">
                                        <SettingsPager PageSize="15" />
                                        <SettingsBootstrap Striped="true" />
                                        <SettingsBehavior EnableRowHotTrack="true" />
                                        <Settings ShowGroupPanel="false" />
                                        <SettingsSearchPanel Visible="true" ShowApplyButton="true" />
                                        <CssClasses IconHeaderSortUp="fas fa-long-arrow-alt-up" IconHeaderSortDown="fas fa-long-arrow-alt-down" IconShowAdaptiveDetailButton="fas fa-plus-circle" />
                                        <SettingsAdaptivity AdaptivityMode="HideDataCells" AllowOnlyOneAdaptiveDetailExpanded="true" AdaptiveColumnPosition="Left"></SettingsAdaptivity>
                                        <Columns>
                                            <dx:BootstrapGridViewDataColumn FieldName="Program.ProgramName" Caption="Program" AdaptivePriority="2"></dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="IsComplete" Caption="Is the form complete?"  AdaptivePriority="6">
                                                <DataItemTemplate>
                                                    <i class='<%# (Convert.ToBoolean(Eval("IsComplete")) ? "fas fa-check green-text" : "fas fa-times red-text") %>'></i>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn Name="LeadershipCoachColumn" FieldName="LeadershipCoachUsername" Caption="Leadership Coach (username)" AdaptivePriority="3">
                                                <DataItemTemplate>
                                                    <asp:Label ID="lblLeadershipCoachUsername" runat="server" ></asp:Label>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="IsMonthly" Caption="Type of Log" HorizontalAlign="Left" AdaptivePriority="3">
                                                <DataItemTemplate>
                                                    <%# (Eval("IsMonthly") == null ? "" : (Convert.ToBoolean(Eval("IsMonthly")) ? "Cumulative record of engagements over one month period" : "Single engagement/encounter")) %>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDateColumn FieldName="DateCompleted" Caption="Date or Month of Log" HorizontalAlign="Left" SortIndex="0" SortOrder="Descending" AdaptivePriority="4">
                                                <DataItemTemplate>
                                                    <%# (Eval("IsMonthly") == null || Eval("DateCompleted") == null ? "" : (Convert.ToBoolean(Eval("IsMonthly")) ? string.Format("{0:MM/yyyy}", Convert.ToDateTime(Eval("DateCompleted"))) : string.Format("{0:MM/dd/yyyy}", Convert.ToDateTime(Eval("DateCompleted"))))) %>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDateColumn>
                                            <dx:BootstrapGridViewDataColumn Name="StateNameColumn" FieldName="Program.State.Name" Caption="State" AdaptivePriority="4"></dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewButtonEditColumn Settings-AllowDragDrop="False" AdaptivePriority="1" CssClasses-DataCell="text-center">
                                                <DataItemTemplate>
                                                    <div class="btn-group">
                                                        <button type="button" class="btn btn-secondary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                            Actions
                                                        </button>
                                                        <div class="dropdown-menu dropdown-menu-right">
                                                            <a class="dropdown-item" href='<%# string.Format("/Pages/LeadershipCoachLog.aspx?LeadershipCoachLogPK={0}&action={1}", Eval("LeadershipCoachLogPK").ToString(), "View") %>'><i class="fas fa-list"></i>&nbsp;View Details</a>
                                                            <a class="dropdown-item lcl-hide-on-view" href='<%#string.Format("/Pages/LeadershipCoachLog.aspx?LeadershipCoachLogPK={0}&action={1}", Eval("LeadershipCoachLogPK").ToString(), "Edit") %>'><i class="fas fa-edit"></i>&nbsp;Edit</a>
                                                            <button class="dropdown-item delete-gridview lcl-hide-on-view" data-pk='<%# Eval("LeadershipCoachLogPK") %>' data-hf="hfDeleteLeadershipCoachLogPK" data-target="#divDeleteLeadershipCoachLogModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                        </div>
                                                    </div>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewButtonEditColumn>
                                        </Columns>
                                    </dx:BootstrapGridView>
                                    <dx:EntityServerModeDataSource ID="efLeadershipCoachLogDataSource" runat="server" OnSelecting="efLeadershipCoachLogDataSource_Selecting" />
                                </div>
                            </div>
                        </div>
                    </div>
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="lbDeleteLeadershipCoachLog" EventName="Click" />
                </Triggers>
            </asp:UpdatePanel>
        </div>
    </div>
    <!-- End of the Leadership Coach Log section -->
    <!-- Start of the meeting schedule section -->
    <div id="divMeetingSchedules" runat="server" class="row">
        <div class="col-xl-12">
            <asp:HiddenField ID="hfDeleteMeetingSchedulePK" runat="server" Value="0" />
            <asp:HiddenField ID="hfDeleteMeetingScheduleType" runat="server" Value="" />
            <asp:UpdatePanel ID="upMeetingSchedule" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                <ContentTemplate>
                    <div class="card bg-light">
                        <div class="card-header">
                            <span class="font-weight-bold">Leadership Team Meeting Schedules</span>
                            <asp:LinkButton ID="lbAddMeetingSchedule" runat="server" CssClass="btn btn-loader btn-primary float-right schedule-hide-on-view hidden" OnClick="lbAddMeetingSchedule_Click"><i class="fas fa-plus"></i>&nbsp;Add New Schedule</asp:LinkButton>
                        </div>
                        <div class="card-body">
                            <div class="row">
                                <div class="col-lg-12">
                                    <dx:BootstrapGridView ID="bsGRMeetingSchedules" runat="server" EnableCallBacks="false" EnableRowsCache="true" 
                                        KeyFieldName="CustomViewPK" OnHtmlRowCreated="bsGRMeetingSchedules_HtmlRowCreated"
                                        AutoGenerateColumns="false" DataSourceID="efMeetingScheduleDataSource">
                                        <SettingsPager PageSize="15" />
                                        <SettingsBootstrap Striped="true" />
                                        <SettingsBehavior EnableRowHotTrack="true" />
                                        <Settings ShowGroupPanel="false" />
                                        <SettingsSearchPanel Visible="true" ShowApplyButton="true" />
                                        <CssClasses IconHeaderSortUp="fas fa-long-arrow-alt-up" IconHeaderSortDown="fas fa-long-arrow-alt-down" IconShowAdaptiveDetailButton="fas fa-plus-circle" />
                                        <SettingsAdaptivity AdaptivityMode="HideDataCells" AllowOnlyOneAdaptiveDetailExpanded="true" AdaptiveColumnPosition="Left"></SettingsAdaptivity>
                                        <Columns>
                                            <dx:BootstrapGridViewDateColumn FieldName="TeamDescription" Caption="Team (Type)" SortIndex="1" SortOrder="Ascending" AdaptivePriority="0"></dx:BootstrapGridViewDateColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="CohortName" Caption="Cohort" AdaptivePriority="3"></dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="MeetingYear" HorizontalAlign="Left" Caption="Year" SortIndex="0" SortOrder="Descending" AdaptivePriority="2"></dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn Name="LeadershipCoachColumn" FieldName="LeadershipCoachUsername" Caption="Leadership Coach (username)" AdaptivePriority="4">
                                                <DataItemTemplate>
                                                    <asp:Label ID="lblLeadershipCoachUsername" runat="server" ></asp:Label>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="TotalMeetings" HorizontalAlign="Left" Caption="Total Meetings" AdaptivePriority="5"></dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="MeetingInJan" Caption="Jan"  AdaptivePriority="6">
                                                <DataItemTemplate>
                                                    <i class='<%# (Convert.ToBoolean(Eval("MeetingInJan")) ? "fas fa-check green-text" : "fas fa-times red-text") %>'></i>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="MeetingInFeb" Caption="Feb" AdaptivePriority="7">
                                                <DataItemTemplate>
                                                    <i class='<%# (Convert.ToBoolean(Eval("MeetingInFeb")) ? "fas fa-check green-text" : "fas fa-times red-text") %>'></i>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="MeetingInMar" Caption="Mar" AdaptivePriority="8">
                                                <DataItemTemplate>
                                                    <i class='<%# (Convert.ToBoolean(Eval("MeetingInMar")) ? "fas fa-check green-text" : "fas fa-times red-text") %>'></i>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="MeetingInApr" Caption="Apr" AdaptivePriority="9">
                                                <DataItemTemplate>
                                                    <i class='<%# (Convert.ToBoolean(Eval("MeetingInApr")) ? "fas fa-check green-text" : "fas fa-times red-text") %>'></i>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="MeetingInMay" Caption="May" AdaptivePriority="10">
                                                <DataItemTemplate>
                                                    <i class='<%# (Convert.ToBoolean(Eval("MeetingInMay")) ? "fas fa-check green-text" : "fas fa-times red-text") %>'></i>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="MeetingInJun" Caption="Jun" AdaptivePriority="11">
                                                <DataItemTemplate>
                                                    <i class='<%# (Convert.ToBoolean(Eval("MeetingInJun")) ? "fas fa-check green-text" : "fas fa-times red-text") %>'></i>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="MeetingInJul" Caption="Jul" AdaptivePriority="12">
                                                <DataItemTemplate>
                                                    <i class='<%# (Convert.ToBoolean(Eval("MeetingInJul")) ? "fas fa-check green-text" : "fas fa-times red-text") %>'></i>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="MeetingInAug" Caption="Aug" AdaptivePriority="13">
                                                <DataItemTemplate>
                                                    <i class='<%# (Convert.ToBoolean(Eval("MeetingInAug")) ? "fas fa-check green-text" : "fas fa-times red-text") %>'></i>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="MeetingInSep" Caption="Sep" AdaptivePriority="14">
                                                <DataItemTemplate>
                                                    <i class='<%# (Convert.ToBoolean(Eval("MeetingInSep")) ? "fas fa-check green-text" : "fas fa-times red-text") %>'></i>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="MeetingInOct" Caption="Oct" AdaptivePriority="15">
                                                <DataItemTemplate>
                                                    <i class='<%# (Convert.ToBoolean(Eval("MeetingInOct")) ? "fas fa-check green-text" : "fas fa-times red-text") %>'></i>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="MeetingInNov" Caption="Nov" AdaptivePriority="16">
                                                <DataItemTemplate>
                                                    <i class='<%# (Convert.ToBoolean(Eval("MeetingInNov")) ? "fas fa-check green-text" : "fas fa-times red-text") %>'></i>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="MeetingInDec" Caption="Dec" AdaptivePriority="17">
                                                <DataItemTemplate>
                                                    <i class='<%# (Convert.ToBoolean(Eval("MeetingInDec")) ? "fas fa-check green-text" : "fas fa-times red-text") %>'></i>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn Name="StateNameColumn" FieldName="StateName" Caption="State" AdaptivePriority="18"></dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewButtonEditColumn Name="ActionColumn" Settings-AllowDragDrop="False" AdaptivePriority="1" CssClasses-DataCell="text-center">
                                                <DataItemTemplate>
                                                    <div class="btn-group">
                                                        <button type="button" class="btn btn-secondary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                            Actions
                                                        </button>
                                                        <div class="dropdown-menu dropdown-menu-right">
                                                            <asp:LinkButton ID="lbViewMeetingSchedule" runat="server" CssClass="dropdown-item" OnClick="lbViewMeetingSchedule_Click"><i class="fas fa-list"></i>&nbsp;View Details</asp:LinkButton>
                                                            <asp:LinkButton ID="lbEditMeetingSchedule" runat="server" CssClass="dropdown-item schedule-hide-on-view" OnClick="lbEditMeetingSchedule_Click"><i class="fas fa-edit"></i> Edit</asp:LinkButton>
                                                            <button id="btnDeleteMeetingSchedule" runat="server" class="dropdown-item delete-gridview schedule-hide-on-view" data-pk='<%# Eval("MeetingSchedulePK") %>' data-hf="hfDeleteMeetingSchedulePK" data-additional='<%# Eval("MeetingScheduleType") %>' data-additional-hf="hfDeleteMeetingScheduleType" data-target="#divDeleteMeetingScheduleModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                        </div>
                                                    </div>
                                                    <asp:Label ID="lblMeetingSchedulePK" runat="server" Visible="false" Text='<%# Eval("MeetingSchedulePK") %>'></asp:Label>
                                                    <asp:Label ID="lblMeetingScheduleType" runat="server" Visible="false" Text='<%# Eval("MeetingScheduleType") %>'></asp:Label>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewButtonEditColumn>
                                        </Columns>
                                    </dx:BootstrapGridView>
                                    <dx:EntityServerModeDataSource ID="efMeetingScheduleDataSource" runat="server" OnSelecting="efMeetingScheduleDataSource_Selecting" />
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-12">
                                    <div id="divAddEditMeetingSchedule" runat="server" class="card mt-2" visible="false">
                                        <div class="card-header">
                                            <asp:Label ID="lblAddEditMeetingSchedule" runat="server" Text=""></asp:Label>
                                        </div>
                                        <div class="card-body">
                                            <div class="row">
                                                <div class="col-lg-12">
                                                    <asp:Label runat="server" AssociatedControlID="lblMeetingScheduleLCUsername" CssClass="d-block" Text="Leadership Coach (username)"></asp:Label>
                                                    <asp:Label ID="lblMeetingScheduleLCUsername" runat="server" CssClass="d-block" Text=""></asp:Label>
                                                </div>
                                            </div>
                                            <div class="row">
                                                <div class="col-lg-3">
                                                    <dx:BootstrapComboBox ID="ddMeetingScheduleType" runat="server" Caption="Team Type" 
                                                        NullText="--Select--" ClientInstanceName="ddMeetingScheduleType"
                                                        IncrementalFilteringMode="Contains" AllowMouseWheel="false">
                                                        <ClientSideEvents ValueChanged="checkMeetingScheduleType" Init="checkMeetingScheduleType" />
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <Items>
                                                            <dx:BootstrapListEditItem Value="Program" Text="Program Team"></dx:BootstrapListEditItem>
                                                            <dx:BootstrapListEditItem Value="Hub" Text="Hub Team"></dx:BootstrapListEditItem>
                                                        </Items>
                                                        <ValidationSettings ValidationGroup="vgMeetingSchedule" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="true" ErrorText="Team Type is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapComboBox>
                                                </div>
                                                <div class="col-lg-3">
                                                    <div id="divMeetingScheduleProgram" class="hidden">
                                                        <dx:BootstrapComboBox ID="ddMeetingScheduleProgram" runat="server" Caption="Program" NullText="--Select--"
                                                            TextField="ProgramName" ValueField="ProgramPK" ValueType="System.Int32" AllowNull="true"
                                                            IncrementalFilteringMode="Contains" AllowMouseWheel="false"
                                                            OnValidation="ddMeetingScheduleProgram_Validation">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <ValidationSettings ValidationGroup="vgMeetingSchedule" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="false" ErrorText="Program is required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapComboBox>
                                                    </div>
                                                    <div id="divMeetingScheduleHub" class="hidden">
                                                        <dx:BootstrapComboBox ID="ddMeetingScheduleHub" runat="server" Caption="Hub" NullText="--Select--"
                                                            TextField="Name" ValueField="HubPK" ValueType="System.Int32" AllowNull="true"
                                                            IncrementalFilteringMode="Contains" AllowMouseWheel="false"
                                                            OnValidation="ddMeetingScheduleHub_Validation">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <ValidationSettings ValidationGroup="vgMeetingSchedule" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="false" ErrorText="Hub is required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapComboBox>
                                                    </div>
                                                </div>
                                                <div class="col-lg-3">
                                                    <dx:BootstrapDateEdit ID="deMeetingScheduleYear" runat="server" Caption="Year" EditFormat="Date"
                                                        EditFormatString="yyyy" UseMaskBehavior="true" NullText="--Select--" PickerType="Years"
                                                        AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                                        <ValidationSettings ValidationGroup="vgMeetingSchedule" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="true" ErrorText="Year is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapDateEdit>
                                                </div>
                                                <div class="col-lg-3">
                                                    <dx:BootstrapTextBox ID="txtMeetingScheduleTotalMeetings" runat="server" Caption="Total Meetings" MaxLength="3"
                                                        OnValidation="txtMeetingScheduleTotalMeetings_Validation">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgMeetingSchedule" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                            <RequiredField IsRequired="false" ErrorText="Total Meetings is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapTextBox>
                                                </div>
                                            </div>
                                            <div class="row mt-2">
                                                <div class="col-lg-12">
                                                    <label>Meetings Scheduled for:</label>
                                                    <br />
                                                    <dx:BootstrapCheckBox ID="chkMeetingScheduleMeetingInJan" runat="server" Text="Jan">
                                                        <SettingsBootstrap InlineMode="true" />
                                                        <ValidationSettings ValidationGroup="vgMeetingSchedule" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="false" ErrorText="Jan is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapCheckBox>
                                                    <dx:BootstrapCheckBox ID="chkMeetingScheduleMeetingInFeb" runat="server" Text="Feb">
                                                        <SettingsBootstrap InlineMode="true" />
                                                        <ValidationSettings ValidationGroup="vgMeetingSchedule" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="false" ErrorText="Feb is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapCheckBox>
                                                    <dx:BootstrapCheckBox ID="chkMeetingScheduleMeetingInMar" runat="server" Text="Mar">
                                                        <SettingsBootstrap InlineMode="true" />
                                                        <ValidationSettings ValidationGroup="vgMeetingSchedule" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="false" ErrorText="Mar is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapCheckBox>
                                                    <dx:BootstrapCheckBox ID="chkMeetingScheduleMeetingInApr" runat="server" Text="Apr">
                                                        <SettingsBootstrap InlineMode="true" />
                                                        <ValidationSettings ValidationGroup="vgMeetingSchedule" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="false" ErrorText="Apr is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapCheckBox>
                                                    <dx:BootstrapCheckBox ID="chkMeetingScheduleMeetingInMay" runat="server" Text="May">
                                                        <SettingsBootstrap InlineMode="true" />
                                                        <ValidationSettings ValidationGroup="vgMeetingSchedule" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="false" ErrorText="May is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapCheckBox>
                                                    <dx:BootstrapCheckBox ID="chkMeetingScheduleMeetingInJun" runat="server" Text="Jun">
                                                        <SettingsBootstrap InlineMode="true" />
                                                        <ValidationSettings ValidationGroup="vgMeetingSchedule" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="false" ErrorText="Jun is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapCheckBox>
                                                    <dx:BootstrapCheckBox ID="chkMeetingScheduleMeetingInJul" runat="server" Text="Jul">
                                                        <SettingsBootstrap InlineMode="true" />
                                                        <ValidationSettings ValidationGroup="vgMeetingSchedule" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="false" ErrorText="Jul is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapCheckBox>
                                                    <dx:BootstrapCheckBox ID="chkMeetingScheduleMeetingInAug" runat="server" Text="Aug">
                                                        <SettingsBootstrap InlineMode="true" />
                                                        <ValidationSettings ValidationGroup="vgMeetingSchedule" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="false" ErrorText="Aug is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapCheckBox>
                                                    <dx:BootstrapCheckBox ID="chkMeetingScheduleMeetingInSep" runat="server" Text="Sep">
                                                        <SettingsBootstrap InlineMode="true" />
                                                        <ValidationSettings ValidationGroup="vgMeetingSchedule" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="false" ErrorText="Sep is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapCheckBox>
                                                    <dx:BootstrapCheckBox ID="chkMeetingScheduleMeetingInOct" runat="server" Text="Oct">
                                                        <SettingsBootstrap InlineMode="true" />
                                                        <ValidationSettings ValidationGroup="vgMeetingSchedule" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="false" ErrorText="Oct is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapCheckBox>
                                                    <dx:BootstrapCheckBox ID="chkMeetingScheduleMeetingInNov" runat="server" Text="Nov">
                                                        <SettingsBootstrap InlineMode="true" />
                                                        <ValidationSettings ValidationGroup="vgMeetingSchedule" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="false" ErrorText="Nov is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapCheckBox>
                                                    <dx:BootstrapCheckBox ID="chkMeetingScheduleMeetingInDec" runat="server" Text="Dec">
                                                        <SettingsBootstrap InlineMode="true" />
                                                        <ValidationSettings ValidationGroup="vgMeetingSchedule" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="false" ErrorText="Dec is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapCheckBox>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="card-footer">
                                            <div class="center-content">
                                                <asp:HiddenField ID="hfAddEditMeetingSchedulePK" runat="server" Value="0" />
                                                <asp:HiddenField ID="hfAddEditMeetingScheduleType" runat="server" Value="" />
                                                <asp:HiddenField ID="hfAddEditMeetingScheduleLeadershipCoachUsername" runat="server" Value="" />
                                                <uc:Submit ID="submitMeetingSchedule" runat="server" 
                                                    ValidationGroup="vgMeetingSchedule"
                                                    ControlCssClass="center-content"
                                                    OnSubmitClick="submitMeetingSchedule_Click" 
                                                    OnCancelClick="submitMeetingSchedule_CancelClick" 
                                                    OnValidationFailed="submitMeetingSchedule_ValidationFailed" />
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="lbDeleteMeetingSchedule" EventName="Click" />
                </Triggers>
            </asp:UpdatePanel>
        </div>
    </div>
    <!-- End of the meeting schedule section -->
    <!-- Start of the Meeting Debrief section -->
    <div id="divMeetingDebriefs" runat="server" class="row">
        <div class="col-xl-12">
            <asp:HiddenField ID="hfDeleteMeetingDebriefPK" runat="server" Value="0" />
            <asp:HiddenField ID="hfDeleteMeetingDebriefType" runat="server" Value="" />
            <asp:UpdatePanel ID="upMeetingDebrief" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                <ContentTemplate>
                    <div class="card bg-light">
                        <div class="card-header">
                            <span class="font-weight-bold">Leadership Team Meeting Debriefs</span>
                            <div class="btn-group float-right">
                                <button type="button" class="btn btn-primary dropdown-toggle debrief-hide-on-view hidden" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                    <i class="fas fa-plus"></i>&nbsp;Add Meeting Debrief
                                </button>
                                <div class="dropdown-menu dropdown-menu-right">
                                    <a id="lnkAddProgramDebrief" runat="server" class="dropdown-item" href="/Pages/ProgramLCMeetingDebrief.aspx?LCMeetingDebriefPK=0&action=Add"><i class="fas fa-plus"></i>&nbsp;Add Program Meeting Debrief</a>
                                    <a id="lnkAddHubDebrief" runat="server" class="dropdown-item" href="/Pages/HubLCMeetingDebrief.aspx?LCMeetingDebriefPK=0&action=Add"><i class="fas fa-plus"></i>&nbsp;Add Hub Meeting Debrief</a>
                                </div>
                            </div>
                        </div>
                        <div class="card-body">
                            <div class="row">
                                <div class="col-lg-12">
                                    <dx:BootstrapGridView ID="bsGRMeetingDebriefs" runat="server" EnableCallBacks="false" EnableRowsCache="true" 
                                        KeyFieldName="CustomViewPK" OnHtmlRowCreated="bsGRMeetingDebriefs_HtmlRowCreated"
                                        AutoGenerateColumns="false" DataSourceID="efMeetingDebriefDataSource">
                                        <SettingsPager PageSize="15" />
                                        <SettingsBootstrap Striped="true" />
                                        <SettingsBehavior EnableRowHotTrack="true" />
                                        <Settings ShowGroupPanel="false" />
                                        <SettingsSearchPanel Visible="true" ShowApplyButton="true" />
                                        <CssClasses IconHeaderSortUp="fas fa-long-arrow-alt-up" IconHeaderSortDown="fas fa-long-arrow-alt-down" IconShowAdaptiveDetailButton="fas fa-plus-circle" />
                                        <SettingsAdaptivity AdaptivityMode="HideDataCells" AllowOnlyOneAdaptiveDetailExpanded="true" AdaptiveColumnPosition="Left"></SettingsAdaptivity>
                                        <Columns>
                                            <dx:BootstrapGridViewDateColumn FieldName="TeamDescription" Caption="Team (Type)" SortIndex="1" SortOrder="Ascending" AdaptivePriority="0"></dx:BootstrapGridViewDateColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="CohortName" Caption="Cohort" AdaptivePriority="3"></dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="DebriefYear" HorizontalAlign="Left" Caption="Year" SortIndex="0" SortOrder="Descending" AdaptivePriority="2"></dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn Name="LeadershipCoachColumn" FieldName="LeadershipCoachUsername" Caption="Leadership Coach (username)" AdaptivePriority="4">
                                                <DataItemTemplate>
                                                    <asp:Label ID="lblLeadershipCoachUsername" runat="server" ></asp:Label>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn Name="StateNameColumn" FieldName="StateName" Caption="State" AdaptivePriority="18"></dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewButtonEditColumn Name="ActionColumn" Settings-AllowDragDrop="False" AdaptivePriority="1" CssClasses-DataCell="text-center">
                                                <DataItemTemplate>
                                                    <div class="btn-group">
                                                        <button type="button" class="btn btn-secondary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                            Actions
                                                        </button>
                                                        <div class="dropdown-menu dropdown-menu-right">
                                                            <a id="lnkViewMeetingDebrief" runat="server" class="dropdown-item" href='<%# string.Format("/Pages/{0}.aspx?LCMeetingDebriefPK={1}&action={2}", (Eval("MeetingDebriefType").ToString() == "Program" ? "ProgramLCMeetingDebrief" : "HubLCMeetingDebrief"), Eval("MeetingDebriefPK").ToString(), "View") %>'><i class="fas fa-list"></i>&nbsp;View Details</a>
                                                            <a id="lnkEditMeetingDebrief" runat="server" class="dropdown-item debrief-hide-on-view" href='<%#string.Format("/Pages/{0}.aspx?LCMeetingDebriefPK={1}&action={2}", (Eval("MeetingDebriefType").ToString() == "Program" ? "ProgramLCMeetingDebrief" : "HubLCMeetingDebrief"), Eval("MeetingDebriefPK").ToString(), "Edit") %>'><i class="fas fa-edit"></i>&nbsp;Edit</a>
                                                            <button id="btnDeleteMeetingDebrief" runat="server" class="dropdown-item delete-gridview debrief-hide-on-view" data-pk='<%# Eval("MeetingDebriefPK") %>' data-hf="hfDeleteMeetingDebriefPK" data-additional='<%# Eval("MeetingDebriefType") %>' data-additional-hf="hfDeleteMeetingDebriefType" data-target="#divDeleteMeetingDebriefModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                        </div>
                                                    </div>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewButtonEditColumn>
                                        </Columns>
                                    </dx:BootstrapGridView>
                                    <dx:EntityServerModeDataSource ID="efMeetingDebriefDataSource" runat="server" OnSelecting="efMeetingDebriefDataSource_Selecting" />
                                </div>
                            </div>
                        </div>
                    </div>
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="lbDeleteMeetingDebrief" EventName="Click" />
                </Triggers>
            </asp:UpdatePanel>
        </div>
    </div>
    <!-- End of the Meeting Debrief section -->
    <!-- Start of the Coaching Circle/Community of Practice schedule section -->
    <div id="divCCSchedules" runat="server" class="row">
        <div class="col-xl-12">
            <asp:HiddenField ID="hfDeleteCCMeetingSchedulePK" runat="server" Value="0" />
            <asp:UpdatePanel ID="upCCMeetingSchedule" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                <ContentTemplate>
                    <div class="card bg-light">
                        <div class="card-header">
                            <span class="font-weight-bold">Coaching Circle/Community of Practice Meeting Schedules</span>
                            <asp:LinkButton ID="lbAddCCMeetingSchedule" runat="server" CssClass="btn btn-loader btn-primary float-right cc-schedule-hide-on-view hidden" OnClick="lbAddCCMeetingSchedule_Click"><i class="fas fa-plus"></i>&nbsp;Add New Schedule</asp:LinkButton>
                        </div>
                        <div class="card-body">
                            <div class="row">
                                <div class="col-lg-12">
                                    <dx:BootstrapGridView ID="bsGRCCMeetingSchedules" runat="server" EnableCallBacks="false" EnableRowsCache="true" 
                                        KeyFieldName="CoachingCircleLCMeetingSchedulePK" OnHtmlRowCreated="bsGRCCMeetingSchedules_HtmlRowCreated"
                                        AutoGenerateColumns="false" DataSourceID="efCCMeetingScheduleDataSource">
                                        <SettingsPager PageSize="15" />
                                        <SettingsBootstrap Striped="true" />
                                        <SettingsBehavior EnableRowHotTrack="true" />
                                        <Settings ShowGroupPanel="false" />
                                        <SettingsSearchPanel Visible="true" ShowApplyButton="true" />
                                        <CssClasses IconHeaderSortUp="fas fa-long-arrow-alt-up" IconHeaderSortDown="fas fa-long-arrow-alt-down" IconShowAdaptiveDetailButton="fas fa-plus-circle" />
                                        <SettingsAdaptivity AdaptivityMode="HideDataCells" AllowOnlyOneAdaptiveDetailExpanded="true" AdaptiveColumnPosition="Left"></SettingsAdaptivity>
                                        <Columns>
                                            <dx:BootstrapGridViewDateColumn FieldName="CoachingCircleName" Caption="Coaching Circle/Community of Practice" SortIndex="1" SortOrder="Ascending" AdaptivePriority="0"></dx:BootstrapGridViewDateColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="TargetAudience" Caption="Target Audience" AdaptivePriority="3"></dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="MeetingYear" HorizontalAlign="Left" Caption="Year" SortIndex="0" SortOrder="Descending" AdaptivePriority="2"></dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn Name="LeadershipCoachColumn" FieldName="LeadershipCoachUsername" Caption="Leadership Coach (username)" AdaptivePriority="4">
                                                <DataItemTemplate>
                                                    <asp:Label ID="lblLeadershipCoachUsername" runat="server" ></asp:Label>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="TotalMeetings" HorizontalAlign="Left" Caption="Total Meetings" AdaptivePriority="5"></dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="MeetingInJan" Caption="Jan"  AdaptivePriority="6">
                                                <DataItemTemplate>
                                                    <i class='<%# (Convert.ToBoolean(Eval("MeetingInJan")) ? "fas fa-check green-text" : "fas fa-times red-text") %>'></i>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="MeetingInFeb" Caption="Feb" AdaptivePriority="7">
                                                <DataItemTemplate>
                                                    <i class='<%# (Convert.ToBoolean(Eval("MeetingInFeb")) ? "fas fa-check green-text" : "fas fa-times red-text") %>'></i>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="MeetingInMar" Caption="Mar" AdaptivePriority="8">
                                                <DataItemTemplate>
                                                    <i class='<%# (Convert.ToBoolean(Eval("MeetingInMar")) ? "fas fa-check green-text" : "fas fa-times red-text") %>'></i>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="MeetingInApr" Caption="Apr" AdaptivePriority="9">
                                                <DataItemTemplate>
                                                    <i class='<%# (Convert.ToBoolean(Eval("MeetingInApr")) ? "fas fa-check green-text" : "fas fa-times red-text") %>'></i>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="MeetingInMay" Caption="May" AdaptivePriority="10">
                                                <DataItemTemplate>
                                                    <i class='<%# (Convert.ToBoolean(Eval("MeetingInMay")) ? "fas fa-check green-text" : "fas fa-times red-text") %>'></i>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="MeetingInJun" Caption="Jun" AdaptivePriority="11">
                                                <DataItemTemplate>
                                                    <i class='<%# (Convert.ToBoolean(Eval("MeetingInJun")) ? "fas fa-check green-text" : "fas fa-times red-text") %>'></i>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="MeetingInJul" Caption="Jul" AdaptivePriority="12">
                                                <DataItemTemplate>
                                                    <i class='<%# (Convert.ToBoolean(Eval("MeetingInJul")) ? "fas fa-check green-text" : "fas fa-times red-text") %>'></i>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="MeetingInAug" Caption="Aug" AdaptivePriority="13">
                                                <DataItemTemplate>
                                                    <i class='<%# (Convert.ToBoolean(Eval("MeetingInAug")) ? "fas fa-check green-text" : "fas fa-times red-text") %>'></i>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="MeetingInSep" Caption="Sep" AdaptivePriority="14">
                                                <DataItemTemplate>
                                                    <i class='<%# (Convert.ToBoolean(Eval("MeetingInSep")) ? "fas fa-check green-text" : "fas fa-times red-text") %>'></i>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="MeetingInOct" Caption="Oct" AdaptivePriority="15">
                                                <DataItemTemplate>
                                                    <i class='<%# (Convert.ToBoolean(Eval("MeetingInOct")) ? "fas fa-check green-text" : "fas fa-times red-text") %>'></i>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="MeetingInNov" Caption="Nov" AdaptivePriority="16">
                                                <DataItemTemplate>
                                                    <i class='<%# (Convert.ToBoolean(Eval("MeetingInNov")) ? "fas fa-check green-text" : "fas fa-times red-text") %>'></i>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="MeetingInDec" Caption="Dec" AdaptivePriority="17">
                                                <DataItemTemplate>
                                                    <i class='<%# (Convert.ToBoolean(Eval("MeetingInDec")) ? "fas fa-check green-text" : "fas fa-times red-text") %>'></i>
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
                                                            <asp:LinkButton ID="lbViewCCMeetingSchedule" runat="server" CssClass="dropdown-item" OnClick="lbViewCCMeetingSchedule_Click"><i class="fas fa-list"></i>&nbsp;View Details</asp:LinkButton>
                                                            <asp:LinkButton ID="lbEditCCMeetingSchedule" runat="server" CssClass="dropdown-item cc-schedule-hide-on-view" OnClick="lbEditCCMeetingSchedule_Click"><i class="fas fa-edit"></i>&nbsp;Edit</asp:LinkButton>
                                                            <button class="dropdown-item delete-gridview cc-schedule-hide-on-view" data-pk='<%# Eval("CoachingCircleLCMeetingSchedulePK") %>' data-hf="hfDeleteCCMeetingSchedulePK" data-target="#divDeleteCCMeetingScheduleModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                        </div>
                                                    </div>
                                                    <asp:Label ID="lblCCMeetingSchedulePK" runat="server" Visible="false" Text='<%# Eval("CoachingCircleLCMeetingSchedulePK") %>'></asp:Label>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewButtonEditColumn>
                                        </Columns>
                                    </dx:BootstrapGridView>
                                    <dx:EntityServerModeDataSource ID="efCCMeetingScheduleDataSource" runat="server" OnSelecting="efCCMeetingScheduleDataSource_Selecting" />
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-12">
                                    <div id="divAddEditCCMeetingSchedule" runat="server" class="card mt-2" visible="false">
                                        <div class="card-header">
                                            <asp:Label ID="lblAddEditCCMeetingSchedule" runat="server" Text=""></asp:Label>
                                        </div>
                                        <div class="card-body">
                                            <div class="row">
                                                <div class="col-lg-12">
                                                    <asp:Label runat="server" AssociatedControlID="lblCCMeetingScheduleLCUsername" CssClass="d-block" Text="Leadership Coach (username)"></asp:Label>
                                                    <asp:Label ID="lblCCMeetingScheduleLCUsername" runat="server" CssClass="d-block" Text=""></asp:Label>
                                                </div>
                                            </div>
                                            <div class="row">
                                                <div class="col-lg-3">
                                                    <dx:BootstrapTextBox ID="txtCCMeetingScheduleCCName" runat="server" Caption="Coaching Circle/Community of Practice" MaxLength="500">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgCCMeetingSchedule" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="true" ErrorText="Coaching Circle/Community of Practice is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapTextBox>
                                                </div>
                                                <div class="col-lg-3">
                                                    <dx:BootstrapMemo ID="txtCCMeetingScheduleTargetAudience" runat="server" Caption="Target Audience" MaxLength="1000" Rows="2">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgCCMeetingSchedule" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="true" ErrorText="Target Audience is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapMemo>
                                                </div>
                                                <div class="col-lg-3">
                                                    <dx:BootstrapDateEdit ID="deCCMeetingScheduleYear" runat="server" Caption="Year" EditFormat="Date"
                                                        EditFormatString="yyyy" UseMaskBehavior="true" NullText="--Select--" PickerType="Years"
                                                        AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                                        <ValidationSettings ValidationGroup="vgCCMeetingSchedule" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="true" ErrorText="Year is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapDateEdit>
                                                </div>
                                                <div class="col-lg-3">
                                                    <dx:BootstrapTextBox ID="txtCCMeetingScheduleTotalMeetings" runat="server" Caption="Total Meetings" MaxLength="3"
                                                        OnValidation="txtCCMeetingScheduleTotalMeetings_Validation">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgCCMeetingSchedule" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                            <RequiredField IsRequired="false" ErrorText="Total Meetings is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapTextBox>
                                                </div>
                                            </div>
                                            <div class="row mt-2">
                                                <div class="col-lg-12">
                                                    <label>Meetings Scheduled for:</label>
                                                    <br />
                                                    <dx:BootstrapCheckBox ID="chkCCMeetingScheduleMeetingInJan" runat="server" Text="Jan">
                                                        <SettingsBootstrap InlineMode="true" />
                                                        <ValidationSettings ValidationGroup="vgCCMeetingSchedule" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="false" ErrorText="Jan is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapCheckBox>
                                                    <dx:BootstrapCheckBox ID="chkCCMeetingScheduleMeetingInFeb" runat="server" Text="Feb">
                                                        <SettingsBootstrap InlineMode="true" />
                                                        <ValidationSettings ValidationGroup="vgCCMeetingSchedule" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="false" ErrorText="Feb is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapCheckBox>
                                                    <dx:BootstrapCheckBox ID="chkCCMeetingScheduleMeetingInMar" runat="server" Text="Mar">
                                                        <SettingsBootstrap InlineMode="true" />
                                                        <ValidationSettings ValidationGroup="vgCCMeetingSchedule" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="false" ErrorText="Mar is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapCheckBox>
                                                    <dx:BootstrapCheckBox ID="chkCCMeetingScheduleMeetingInApr" runat="server" Text="Apr">
                                                        <SettingsBootstrap InlineMode="true" />
                                                        <ValidationSettings ValidationGroup="vgCCMeetingSchedule" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="false" ErrorText="Apr is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapCheckBox>
                                                    <dx:BootstrapCheckBox ID="chkCCMeetingScheduleMeetingInMay" runat="server" Text="May">
                                                        <SettingsBootstrap InlineMode="true" />
                                                        <ValidationSettings ValidationGroup="vgCCMeetingSchedule" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="false" ErrorText="May is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapCheckBox>
                                                    <dx:BootstrapCheckBox ID="chkCCMeetingScheduleMeetingInJun" runat="server" Text="Jun">
                                                        <SettingsBootstrap InlineMode="true" />
                                                        <ValidationSettings ValidationGroup="vgCCMeetingSchedule" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="false" ErrorText="Jun is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapCheckBox>
                                                    <dx:BootstrapCheckBox ID="chkCCMeetingScheduleMeetingInJul" runat="server" Text="Jul">
                                                        <SettingsBootstrap InlineMode="true" />
                                                        <ValidationSettings ValidationGroup="vgCCMeetingSchedule" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="false" ErrorText="Jul is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapCheckBox>
                                                    <dx:BootstrapCheckBox ID="chkCCMeetingScheduleMeetingInAug" runat="server" Text="Aug">
                                                        <SettingsBootstrap InlineMode="true" />
                                                        <ValidationSettings ValidationGroup="vgCCMeetingSchedule" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="false" ErrorText="Aug is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapCheckBox>
                                                    <dx:BootstrapCheckBox ID="chkCCMeetingScheduleMeetingInSep" runat="server" Text="Sep">
                                                        <SettingsBootstrap InlineMode="true" />
                                                        <ValidationSettings ValidationGroup="vgCCMeetingSchedule" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="false" ErrorText="Sep is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapCheckBox>
                                                    <dx:BootstrapCheckBox ID="chkCCMeetingScheduleMeetingInOct" runat="server" Text="Oct">
                                                        <SettingsBootstrap InlineMode="true" />
                                                        <ValidationSettings ValidationGroup="vgCCMeetingSchedule" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="false" ErrorText="Oct is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapCheckBox>
                                                    <dx:BootstrapCheckBox ID="chkCCMeetingScheduleMeetingInNov" runat="server" Text="Nov">
                                                        <SettingsBootstrap InlineMode="true" />
                                                        <ValidationSettings ValidationGroup="vgCCMeetingSchedule" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="false" ErrorText="Nov is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapCheckBox>
                                                    <dx:BootstrapCheckBox ID="chkCCMeetingScheduleMeetingInDec" runat="server" Text="Dec">
                                                        <SettingsBootstrap InlineMode="true" />
                                                        <ValidationSettings ValidationGroup="vgCCMeetingSchedule" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="false" ErrorText="Dec is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapCheckBox>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="card-footer">
                                            <div class="center-content">
                                                <asp:HiddenField ID="hfAddEditCCMeetingSchedulePK" runat="server" Value="0" />
                                                <asp:HiddenField ID="hfAddEditCCMeetingScheduleLeadershipCoachUsername" runat="server" Value="" />
                                                <uc:Submit ID="submitCCMeetingSchedule" runat="server" 
                                                    ValidationGroup="vgCCMeetingSchedule"
                                                    ControlCssClass="center-content"
                                                    OnSubmitClick="submitCCMeetingSchedule_Click" 
                                                    OnCancelClick="submitCCMeetingSchedule_CancelClick" 
                                                    OnValidationFailed="submitCCMeetingSchedule_ValidationFailed" />
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="lbDeleteCCMeetingSchedule" EventName="Click" />
                </Triggers>
            </asp:UpdatePanel>
        </div>
    </div>
    <!-- End of the Coaching Circle/Community of Practice schedule section -->
    <!-- Start of the Coaching Circle/Community of Practice debrief section -->
    <div id="divCCDebriefs" runat="server" class="row">
        <div class="col-xl-12">
            <asp:HiddenField ID="hfDeleteCCMeetingDebriefPK" runat="server" Value="0" />
            <asp:UpdatePanel ID="upCCMeetingDebrief" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                <ContentTemplate>
                    <div class="card bg-light">
                        <div class="card-header">
                            <span class="font-weight-bold">Coaching Circle/Community of Practice Meeting Debriefs</span>
                            <a href="/Pages/CoachingCircleLCMeetingDebrief.aspx?CCMeetingDebriefPK=0&action=Add" class="btn btn-loader btn-primary float-right cc-debrief-hide-on-view hidden"><i class="fas fa-plus"></i>&nbsp;Add New Debrief</a>
                        </div>
                        <div class="card-body">
                            <div class="row">
                                <div class="col-lg-12">
                                    <dx:BootstrapGridView ID="bsGRCCMeetingDebriefs" runat="server" EnableCallBacks="false" EnableRowsCache="true" 
                                        KeyFieldName="CoachingCircleLCMeetingDebriefPK" OnHtmlRowCreated="bsGRCCMeetingDebriefs_HtmlRowCreated"
                                        AutoGenerateColumns="false" DataSourceID="efCCMeetingDebriefDataSource">
                                        <SettingsPager PageSize="15" />
                                        <SettingsBootstrap Striped="true" />
                                        <SettingsBehavior EnableRowHotTrack="true" />
                                        <Settings ShowGroupPanel="false" />
                                        <SettingsSearchPanel Visible="true" ShowApplyButton="true" />
                                        <CssClasses IconHeaderSortUp="fas fa-long-arrow-alt-up" IconHeaderSortDown="fas fa-long-arrow-alt-down" IconShowAdaptiveDetailButton="fas fa-plus-circle" />
                                        <SettingsAdaptivity AdaptivityMode="HideDataCells" AllowOnlyOneAdaptiveDetailExpanded="true" AdaptiveColumnPosition="Left"></SettingsAdaptivity>
                                        <Columns>
                                            <dx:BootstrapGridViewDateColumn FieldName="CoachingCircleName" Caption="Coaching Circle/Community of Practice" SortIndex="1" SortOrder="Ascending" AdaptivePriority="0"></dx:BootstrapGridViewDateColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="TargetAudience" Caption="Target Audience" AdaptivePriority="3"></dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="DebriefYear" HorizontalAlign="Left" Caption="Year" SortIndex="0" SortOrder="Descending" AdaptivePriority="2"></dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn Name="LeadershipCoachColumn" FieldName="LeadershipCoachUsername" Caption="Leadership Coach (username)" AdaptivePriority="4">
                                                <DataItemTemplate>
                                                    <asp:Label ID="lblLeadershipCoachUsername" runat="server" ></asp:Label>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn Name="StateNameColumn" FieldName="State.Name" Caption="State" AdaptivePriority="4"></dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewButtonEditColumn Settings-AllowDragDrop="False" AdaptivePriority="1" CssClasses-DataCell="text-center">
                                                <DataItemTemplate>
                                                    <div class="btn-group">
                                                        <button type="button" class="btn btn-secondary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                            Actions
                                                        </button>
                                                        <div class="dropdown-menu dropdown-menu-right">
                                                            <a class="dropdown-item" href='<%# string.Format("/Pages/CoachingCircleLCMeetingDebrief.aspx?CCMeetingDebriefPK={0}&action={1}", Eval("CoachingCircleLCMeetingDebriefPK").ToString(), "View") %>'><i class="fas fa-list"></i>&nbsp;View Details</a>
                                                            <a class="dropdown-item cc-debrief-hide-on-view" href='<%#string.Format("/Pages/CoachingCircleLCMeetingDebrief.aspx?CCMeetingDebriefPK={0}&action={1}", Eval("CoachingCircleLCMeetingDebriefPK").ToString(), "Edit") %>'><i class="fas fa-edit"></i>&nbsp;Edit</a>
                                                            <button class="dropdown-item delete-gridview cc-debrief-hide-on-view" data-pk='<%# Eval("CoachingCircleLCMeetingDebriefPK") %>' data-hf="hfDeleteCCMeetingDebriefPK" data-target="#divDeleteCCMeetingDebriefModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                        </div>
                                                    </div>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewButtonEditColumn>
                                        </Columns>
                                    </dx:BootstrapGridView>
                                    <dx:EntityServerModeDataSource ID="efCCMeetingDebriefDataSource" runat="server" OnSelecting="efCCMeetingDebriefDataSource_Selecting" />
                                </div>
                            </div>
                        </div>
                    </div>
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="lbDeleteCCMeetingDebrief" EventName="Click" />
                </Triggers>
            </asp:UpdatePanel>
        </div>
    </div>
    <!-- End of the Coaching Circle/Community of Practice debrief section -->
    <div class="modal" id="divDeleteLeadershipCoachLogModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete Leadership Coach Log</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this Leadership Coach Log?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteLeadershipCoachLog" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteLeadershipCoachLogModal" OnClick="lbDeleteLeadershipCoachLog_Click"><i class="fas fa-check"></i>&nbsp;Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
    <div class="modal" id="divDeleteMeetingScheduleModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete Leadership Team Meeting Schedule</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this Leadership Team Meeting Schedule?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteMeetingSchedule" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteMeetingScheduleModal" OnClick="lbDeleteMeetingSchedule_Click"><i class="fas fa-check"></i>&nbsp;Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
    <div class="modal" id="divDeleteMeetingDebriefModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete Leadership Team Meeting Debrief</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this Leadership Team Meeting Debrief?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteMeetingDebrief" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteMeetingDebriefModal" OnClick="lbDeleteMeetingDebrief_Click"><i class="fas fa-check"></i>&nbsp;Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
    <div class="modal" id="divDeleteCCMeetingScheduleModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete Coaching Circle/Community of Practice Meeting Schedule</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this Coaching Circle/Community of Practice Meeting Schedule?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteCCMeetingSchedule" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteCCMeetingScheduleModal" OnClick="lbDeleteCCMeetingSchedule_Click"><i class="fas fa-check"></i>&nbsp;Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
    <div class="modal" id="divDeleteCCMeetingDebriefModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete Coaching Circle/Community of Practice Meeting Debrief</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this Coaching Circle/Community of Practice Meeting Debrief form?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteCCMeetingDebrief" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteCCMeetingDebriefModal" OnClick="lbDeleteCCMeetingDebrief_Click"><i class="fas fa-check"></i>&nbsp;Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
</asp:Content>