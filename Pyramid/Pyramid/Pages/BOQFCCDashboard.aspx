﻿<%@ Page Title="BOQFCC Dashboard" Language="C#" MasterPageFile="~/MasterPages/Dashboard.master" AutoEventWireup="true" CodeBehind="BOQFCCDashboard.aspx.cs" Inherits="Pyramid.Pages.BOQFCCDashboard" %>

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
            $('[ID$="lnkBOQFCCDashboard"]').addClass('active');

            //Show/hide the view only fields
            setViewOnlyVisibility();
        }

        function setViewOnlyVisibility() {
            //Hide controls if this is a view
            var isBOQView = $('[ID$="hfBOQViewOnly"]').val();
            if (isBOQView == 'True') {
                $('.boq-hide-on-view').addClass('hidden');
            }
            else {
                $('.boq-hide-on-view').removeClass('hidden');
            }
            var isScheduleView = $('[ID$="hfScheduleViewOnly"]').val();
            if (isScheduleView == 'True') {
                $('.schedule-hide-on-view').addClass('hidden');
            }
            else {
                $('.schedule-hide-on-view').removeClass('hidden');
            }
        }
    </script>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <asp:HiddenField ID="hfBOQViewOnly" runat="server" Value="False" />
    <asp:HiddenField ID="hfScheduleViewOnly" runat="server" Value="False" />
    <asp:UpdatePanel ID="upDashboardMessaging" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <uc:Messaging ID="msgSys" runat="server" />
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="lbDeleteBOQFCC" />
            <asp:AsyncPostBackTrigger ControlID="bsGRBOQFCCs" />
            <asp:AsyncPostBackTrigger ControlID="lbDeleteFormSchedule" />
            <asp:AsyncPostBackTrigger ControlID="lbAddFormSchedule" />
            <asp:AsyncPostBackTrigger ControlID="bsGRFormSchedules" />
            <asp:AsyncPostBackTrigger ControlID="submitFormSchedule" />
        </Triggers>
    </asp:UpdatePanel>
    <div id="divBOQs" runat="server" class="row">
        <div class="col-lg-12 col-xl-12">
            <asp:HiddenField ID="hfDeleteBOQFCCPK" runat="server" Value="0" />
            <asp:UpdatePanel runat="server" ID="upAllBOQFCC">
                <ContentTemplate>
                    <div class="card bg-light">
                        <div class="card-header">
                            Benchmarks of Quality FCC Forms
                            <a href="/Pages/BOQFCCV2.aspx?BOQFCCPK=0&action=Add" class="btn btn-loader btn-primary float-right boq-hide-on-view hidden"><i class="fas fa-plus"></i>&nbsp;Add New BOQ FCC</a>
                        </div>
                        <div class="card-body">
                            <label>All Benchmarks of Quality FCC Forms</label>
                            <div class="alert alert-primary">
                                This table contains all Benchmarks of Quality FCC forms regardless of when they were performed.
                            </div>
                            <dx:BootstrapGridView ID="bsGRBOQFCCs" runat="server" EnableCallBacks="false" EnableRowsCache="true" 
                                KeyFieldName="BenchmarkOfQualityFCCPK"
                                AutoGenerateColumns="false" DataSourceID="efBOQFCCDataSource">
                                <SettingsPager PageSize="15" />
                                <SettingsBootstrap Striped="true" />
                                <SettingsBehavior EnableRowHotTrack="true" />
                                <Settings ShowGroupPanel="false" />
                                <SettingsSearchPanel Visible="true" ShowApplyButton="true" />
                                <CssClasses IconHeaderSortUp="fas fa-long-arrow-alt-up" IconHeaderSortDown="fas fa-long-arrow-alt-down" IconShowAdaptiveDetailButton="fas fa-plus-circle" />
                                <SettingsAdaptivity AdaptivityMode="HideDataCells" AllowOnlyOneAdaptiveDetailExpanded="true" AdaptiveColumnPosition="Left"></SettingsAdaptivity>
                                <Columns>
                                    <dx:BootstrapGridViewDateColumn FieldName="FormDate" SortIndex="0" SortOrder="Descending" PropertiesDateEdit-DisplayFormatString="MM/dd/yyyy" AdaptivePriority="0"></dx:BootstrapGridViewDateColumn>
                                    <dx:BootstrapGridViewDataColumn FieldName="VersionNumber" Caption="Version" AdaptivePriority="2"></dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewDataColumn FieldName="IsComplete" Caption="Is the form complete?"  AdaptivePriority="3">
                                        <DataItemTemplate>
                                            <i class='<%# (Convert.ToBoolean(Eval("IsComplete")) ? "fas fa-check green-text" : "fas fa-times red-text") %>'></i>
                                        </DataItemTemplate>
                                    </dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewDataColumn FieldName="Program.ProgramName" Caption="Program Name" AdaptivePriority="3"></dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewDataColumn FieldName="Program.Location" Caption="Program Location" AdaptivePriority="4"></dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewDataColumn FieldName="TeamMembers" Caption="Team Members" AdaptivePriority="4"></dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewDataColumn Name="StateNameColumn" FieldName="Program.State.Name" Caption="State" AdaptivePriority="5"></dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewButtonEditColumn Settings-AllowDragDrop="False" AdaptivePriority="1" CssClasses-DataCell="text-center">
                                        <DataItemTemplate>
                                            <div class="btn-group">
                                                <button type="button" class="btn btn-secondary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                    Actions
                                                </button>
                                                <div class="dropdown-menu dropdown-menu-right">
                                                    <a class="dropdown-item" href='<%# string.Format("/Pages/{0}.aspx?BOQFCCPK={1}&action={2}", (Eval("VersionNumber").ToString() == "1" ? "BOQFCC" : "BOQFCCV2"), Eval("BenchmarkOfQualityFCCPK").ToString(), "view") %>'><i class="fas fa-list"></i>&nbsp;View Details</a>
                                                    <a class="dropdown-item boq-hide-on-view" href='<%#string.Format("/Pages/{0}.aspx?BOQFCCPK={1}&action={2}", (Eval("VersionNumber").ToString() == "1" ? "BOQFCC" : "BOQFCCV2"), Eval("BenchmarkOfQualityFCCPK").ToString(), "edit") %>'><i class="fas fa-edit"></i>&nbsp;Edit</a>
                                                    <button class="dropdown-item delete-gridview boq-hide-on-view" data-pk='<%# Eval("BenchmarkOfQualityFCCPK") %>' data-hf="hfDeleteBOQFCCPK" data-target="#divDeleteBOQFCCModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                </div>
                                            </div>
                                        </DataItemTemplate>
                                    </dx:BootstrapGridViewButtonEditColumn>
                                </Columns>
                            </dx:BootstrapGridView>
                            <dx:EntityServerModeDataSource ID="efBOQFCCDataSource" runat="server" OnSelecting="efBOQFCCDataSource_Selecting" />
                        </div>
                    </div>
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="lbDeleteBOQFCC" />
                </Triggers>
            </asp:UpdatePanel>
        </div>
    </div>
    <!-- Start of the schedule section -->
    <div id="divFormSchedules" runat="server" class="row">
        <div class="col-xl-12">
            <asp:HiddenField ID="hfDeleteFormSchedulePK" runat="server" Value="0" />
            <asp:UpdatePanel ID="upFormSchedule" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                <ContentTemplate>
                    <div class="card bg-light">
                        <div class="card-header">
                            <span class="font-weight-bold">Benchmark of Quality FCC Schedule</span>
                            <asp:LinkButton ID="lbAddFormSchedule" runat="server" CssClass="btn btn-loader btn-primary float-right schedule-hide-on-view hidden" OnClick="lbAddFormSchedule_Click"><i class="fas fa-plus"></i>&nbsp;Add New Schedule</asp:LinkButton>
                        </div>
                        <div class="card-body">
                            <div class="row">
                                <div class="col-lg-12">
                                    <dx:BootstrapGridView ID="bsGRFormSchedules" runat="server" EnableCallBacks="false" EnableRowsCache="true" 
                                        KeyFieldName="FormSchedulePK" AutoGenerateColumns="false" DataSourceID="efFormScheduleDataSource">
                                        <SettingsPager PageSize="15" />
                                        <SettingsBootstrap Striped="true" />
                                        <SettingsBehavior EnableRowHotTrack="true" />
                                        <Settings ShowGroupPanel="false" />
                                        <SettingsSearchPanel Visible="true" ShowApplyButton="true" />
                                        <CssClasses IconHeaderSortUp="fas fa-long-arrow-alt-up" IconHeaderSortDown="fas fa-long-arrow-alt-down" IconShowAdaptiveDetailButton="fas fa-plus-circle" />
                                        <SettingsAdaptivity AdaptivityMode="HideDataCells" AllowOnlyOneAdaptiveDetailExpanded="true" AdaptiveColumnPosition="Left"></SettingsAdaptivity>
                                        <Columns>
                                            <dx:BootstrapGridViewDateColumn FieldName="Program.ProgramName" Caption="Program" SortIndex="1" SortOrder="Ascending" AdaptivePriority="0"></dx:BootstrapGridViewDateColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="ScheduleYear" HorizontalAlign="Left" Caption="Year" SortIndex="0" SortOrder="Descending" AdaptivePriority="2"></dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="ScheduledForJan" Caption="Jan"  AdaptivePriority="6">
                                                <DataItemTemplate>
                                                    <i class='<%# (Convert.ToBoolean(Eval("ScheduledForJan")) ? "fas fa-check green-text" : "fas fa-times red-text") %>'></i>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="ScheduledForFeb" Caption="Feb" AdaptivePriority="7">
                                                <DataItemTemplate>
                                                    <i class='<%# (Convert.ToBoolean(Eval("ScheduledForFeb")) ? "fas fa-check green-text" : "fas fa-times red-text") %>'></i>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="ScheduledForMar" Caption="Mar" AdaptivePriority="8">
                                                <DataItemTemplate>
                                                    <i class='<%# (Convert.ToBoolean(Eval("ScheduledForMar")) ? "fas fa-check green-text" : "fas fa-times red-text") %>'></i>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="ScheduledForApr" Caption="Apr" AdaptivePriority="9">
                                                <DataItemTemplate>
                                                    <i class='<%# (Convert.ToBoolean(Eval("ScheduledForApr")) ? "fas fa-check green-text" : "fas fa-times red-text") %>'></i>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="ScheduledForMay" Caption="May" AdaptivePriority="10">
                                                <DataItemTemplate>
                                                    <i class='<%# (Convert.ToBoolean(Eval("ScheduledForMay")) ? "fas fa-check green-text" : "fas fa-times red-text") %>'></i>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="ScheduledForJun" Caption="Jun" AdaptivePriority="11">
                                                <DataItemTemplate>
                                                    <i class='<%# (Convert.ToBoolean(Eval("ScheduledForJun")) ? "fas fa-check green-text" : "fas fa-times red-text") %>'></i>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="ScheduledForJul" Caption="Jul" AdaptivePriority="12">
                                                <DataItemTemplate>
                                                    <i class='<%# (Convert.ToBoolean(Eval("ScheduledForJul")) ? "fas fa-check green-text" : "fas fa-times red-text") %>'></i>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="ScheduledForAug" Caption="Aug" AdaptivePriority="13">
                                                <DataItemTemplate>
                                                    <i class='<%# (Convert.ToBoolean(Eval("ScheduledForAug")) ? "fas fa-check green-text" : "fas fa-times red-text") %>'></i>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="ScheduledForSep" Caption="Sep" AdaptivePriority="14">
                                                <DataItemTemplate>
                                                    <i class='<%# (Convert.ToBoolean(Eval("ScheduledForSep")) ? "fas fa-check green-text" : "fas fa-times red-text") %>'></i>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="ScheduledForOct" Caption="Oct" AdaptivePriority="15">
                                                <DataItemTemplate>
                                                    <i class='<%# (Convert.ToBoolean(Eval("ScheduledForOct")) ? "fas fa-check green-text" : "fas fa-times red-text") %>'></i>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="ScheduledForNov" Caption="Nov" AdaptivePriority="16">
                                                <DataItemTemplate>
                                                    <i class='<%# (Convert.ToBoolean(Eval("ScheduledForNov")) ? "fas fa-check green-text" : "fas fa-times red-text") %>'></i>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="ScheduledForDec" Caption="Dec" AdaptivePriority="17">
                                                <DataItemTemplate>
                                                    <i class='<%# (Convert.ToBoolean(Eval("ScheduledForDec")) ? "fas fa-check green-text" : "fas fa-times red-text") %>'></i>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn Name="StateNameColumn" FieldName="Program.State.Name" Caption="State" AdaptivePriority="18"></dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewButtonEditColumn Settings-AllowDragDrop="False" AdaptivePriority="1" CssClasses-DataCell="text-center">
                                                <DataItemTemplate>
                                                    <div class="btn-group">
                                                        <button type="button" class="btn btn-secondary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                            Actions
                                                        </button>
                                                        <div class="dropdown-menu dropdown-menu-right">
                                                            <asp:LinkButton ID="lbViewFormSchedule" runat="server" CssClass="dropdown-item" OnClick="lbViewFormSchedule_Click"><i class="fas fa-list"></i>&nbsp;View Details</asp:LinkButton>
                                                            <asp:LinkButton ID="lbEditFormSchedule" runat="server" CssClass="dropdown-item schedule-hide-on-view" OnClick="lbEditFormSchedule_Click"><i class="fas fa-edit"></i> Edit</asp:LinkButton>
                                                            <button class="dropdown-item delete-gridview schedule-hide-on-view" data-pk='<%# Eval("FormSchedulePK") %>' data-hf="hfDeleteFormSchedulePK" data-target="#divDeleteFormScheduleModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                        </div>
                                                    </div>
                                                    <asp:Label ID="lblFormSchedulePK" runat="server" Visible="false" Text='<%# Eval("FormSchedulePK") %>'></asp:Label>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewButtonEditColumn>
                                        </Columns>
                                    </dx:BootstrapGridView>
                                    <dx:EntityServerModeDataSource ID="efFormScheduleDataSource" runat="server" OnSelecting="efFormScheduleDataSource_Selecting" />
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-12">
                                    <div id="divAddEditFormSchedule" runat="server" class="card mt-2" visible="false">
                                        <div class="card-header">
                                            <asp:Label ID="lblAddEditFormSchedule" runat="server" Text=""></asp:Label>
                                        </div>
                                        <div class="card-body">
                                            <div class="row">
                                                <div class="col-lg-4">
                                                    <dx:BootstrapComboBox ID="ddFSProgram" runat="server" Caption="Program" NullText="--Select--"
                                                        TextField="ProgramName" ValueField="ProgramPK" ValueType="System.Int32" AllowNull="true"
                                                        IncrementalFilteringMode="Contains" AllowMouseWheel="false"
                                                        OnValidation="ddFSProgram_Validation">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgFormSchedule" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                            <RequiredField IsRequired="true" ErrorText="Program is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapComboBox>
                                                </div>
                                                <div class="col-lg-4">
                                                    <dx:BootstrapDateEdit ID="deFSYear" runat="server" Caption="Year" EditFormat="Date"
                                                        EditFormatString="yyyy" UseMaskBehavior="true" NullText="--Select--" PickerType="Years"
                                                        AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                                        <ValidationSettings ValidationGroup="vgFormSchedule" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="true" ErrorText="Year is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapDateEdit>
                                                </div>
                                            </div>
                                            <div class="row mt-2">
                                                <div class="col-lg-12">
                                                    <label>Scheduled for:</label>
                                                    <br />
                                                    <dx:BootstrapCheckBox ID="chkFSScheduledForJan" runat="server" Text="Jan">
                                                        <SettingsBootstrap InlineMode="true" />
                                                        <ValidationSettings ValidationGroup="vgFormSchedule" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="false" ErrorText="Jan is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapCheckBox>
                                                    <dx:BootstrapCheckBox ID="chkFSScheduledForFeb" runat="server" Text="Feb">
                                                        <SettingsBootstrap InlineMode="true" />
                                                        <ValidationSettings ValidationGroup="vgFormSchedule" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="false" ErrorText="Feb is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapCheckBox>
                                                    <dx:BootstrapCheckBox ID="chkFSScheduledForMar" runat="server" Text="Mar">
                                                        <SettingsBootstrap InlineMode="true" />
                                                        <ValidationSettings ValidationGroup="vgFormSchedule" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="false" ErrorText="Mar is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapCheckBox>
                                                    <dx:BootstrapCheckBox ID="chkFSScheduledForApr" runat="server" Text="Apr">
                                                        <SettingsBootstrap InlineMode="true" />
                                                        <ValidationSettings ValidationGroup="vgFormSchedule" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="false" ErrorText="Apr is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapCheckBox>
                                                    <dx:BootstrapCheckBox ID="chkFSScheduledForMay" runat="server" Text="May">
                                                        <SettingsBootstrap InlineMode="true" />
                                                        <ValidationSettings ValidationGroup="vgFormSchedule" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="false" ErrorText="May is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapCheckBox>
                                                    <dx:BootstrapCheckBox ID="chkFSScheduledForJun" runat="server" Text="Jun">
                                                        <SettingsBootstrap InlineMode="true" />
                                                        <ValidationSettings ValidationGroup="vgFormSchedule" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="false" ErrorText="Jun is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapCheckBox>
                                                    <dx:BootstrapCheckBox ID="chkFSScheduledForJul" runat="server" Text="Jul">
                                                        <SettingsBootstrap InlineMode="true" />
                                                        <ValidationSettings ValidationGroup="vgFormSchedule" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="false" ErrorText="Jul is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapCheckBox>
                                                    <dx:BootstrapCheckBox ID="chkFSScheduledForAug" runat="server" Text="Aug">
                                                        <SettingsBootstrap InlineMode="true" />
                                                        <ValidationSettings ValidationGroup="vgFormSchedule" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="false" ErrorText="Aug is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapCheckBox>
                                                    <dx:BootstrapCheckBox ID="chkFSScheduledForSep" runat="server" Text="Sep">
                                                        <SettingsBootstrap InlineMode="true" />
                                                        <ValidationSettings ValidationGroup="vgFormSchedule" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="false" ErrorText="Sep is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapCheckBox>
                                                    <dx:BootstrapCheckBox ID="chkFSScheduledForOct" runat="server" Text="Oct">
                                                        <SettingsBootstrap InlineMode="true" />
                                                        <ValidationSettings ValidationGroup="vgFormSchedule" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="false" ErrorText="Oct is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapCheckBox>
                                                    <dx:BootstrapCheckBox ID="chkFSScheduledForNov" runat="server" Text="Nov">
                                                        <SettingsBootstrap InlineMode="true" />
                                                        <ValidationSettings ValidationGroup="vgFormSchedule" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="false" ErrorText="Nov is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapCheckBox>
                                                    <dx:BootstrapCheckBox ID="chkFSScheduledForDec" runat="server" Text="Dec">
                                                        <SettingsBootstrap InlineMode="true" />
                                                        <ValidationSettings ValidationGroup="vgFormSchedule" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="false" ErrorText="Dec is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapCheckBox>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="card-footer">
                                            <div class="center-content">
                                                <asp:HiddenField ID="hfAddEditFormSchedulePK" runat="server" Value="0" />
                                                <uc:Submit ID="submitFormSchedule" runat="server" 
                                                    ValidationGroup="vgFormSchedule"
                                                    ControlCssClass="center-content"
                                                    OnSubmitClick="submitFormSchedule_Click" 
                                                    OnCancelClick="submitFormSchedule_CancelClick" 
                                                    OnValidationFailed="submitFormSchedule_ValidationFailed" />
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </ContentTemplate>
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="lbDeleteFormSchedule" EventName="Click" />
                </Triggers>
            </asp:UpdatePanel>
        </div>
    </div>
    <!-- End of the schedule section -->
    <div class="modal" id="divDeleteBOQFCCModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete Benchmarks of Quality FCC Form</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this Benchmarks of Quality FCC form?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteBOQFCC" runat="server" CssClass="btn btn-loader btn-loader btn-danger modal-delete" data-target="#divDeleteBOQFCCModal" OnClick="lbDeleteBOQFCC_Click"><i class="fas fa-check"></i>&nbsp;Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
    <div class="modal" id="divDeleteFormScheduleModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete Schedule</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this Benchmarks of Quality FCC Schedule?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteFormSchedule" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteFormScheduleModal" OnClick="lbDeleteFormSchedule_Click"><i class="fas fa-check"></i>&nbsp;Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
