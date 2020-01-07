<%@ Page Title="Behavior Incident Report Dashboard" Language="C#" MasterPageFile="~/MasterPages/Dashboard.master" AutoEventWireup="true" CodeBehind="BehaviorIncidentDashboard.aspx.cs" Inherits="Pyramid.Pages.BehaviorIncidentDashboard" %>

<%@ Register Assembly="DevExpress.Web.v19.1, Version=19.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Data.Linq" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.Bootstrap.v19.1, Version=19.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.v19.1, Version=19.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>
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
            //Highlight the correct link in the master page
            $('#lnkBehaviorIncidentDashboard').addClass('active');

            //Show/hide the view only fields
            setViewOnlyVisibility();
        }
    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <asp:HiddenField ID="hfViewOnly" runat="server" Value="False" />
    <asp:UpdatePanel ID="upDashboardMessaging" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <uc:Messaging ID="msgSys" runat="server" />
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="lbDeleteBehaviorIncident" EventName="Click" />
        </Triggers>
    </asp:UpdatePanel>
    <asp:HiddenField ID="hfDeleteBehaviorIncidentPK" runat="server" Value="0" />
    <div class="row">
        <div class="col-lg-7 col-xl-8">
            <div class="card bg-light">
                <div class="card-header">
                    Behavior Incident Reports
                    <a href="/Pages/BehaviorIncident.aspx?BehaviorIncidentPK=0&Action=Add" class="btn btn-loader btn-primary float-right hide-on-view hidden"><i class="fas fa-plus"></i>&nbsp;Add New Incident Report</a>
                </div>
                <div class="card-body">
                    <label>All Behavior Incident Reports</label>
                    <div class="alert alert-primary">
                        This table contains all Behavior Incident Reports regardless of when they occurred.
                    </div>
                    <asp:UpdatePanel ID="upAllBehaviorIncidents" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>
                            <dx:BootstrapGridView ID="bsGRBehaviorIncidents" runat="server" EnableCallBacks="false" EnableRowsCache="true" 
                                KeyFieldName="BehaviorIncidentProgramPK" AutoGenerateColumns="false" 
                                DataSourceID="efBehaviorIncidentDataSource">
                                <SettingsPager PageSize="15" />
                                <SettingsBootstrap Striped="true" />
                                <SettingsBehavior EnableRowHotTrack="true" />
                                <SettingsSearchPanel Visible="true" ShowApplyButton="true" />
                                <Settings ShowGroupPanel="true" />
                                <CssClasses IconHeaderSortUp="fas fa-long-arrow-alt-up" IconHeaderSortDown="fas fa-long-arrow-alt-down" IconShowAdaptiveDetailButton="fas fa-plus-circle" />
                                <SettingsAdaptivity AdaptivityMode="HideDataCells" AllowOnlyOneAdaptiveDetailExpanded="true" AdaptiveColumnPosition="Left"></SettingsAdaptivity>
                                <Columns>
                                    <dx:BootstrapGridViewDateColumn FieldName="IncidentDatetime" SortIndex="0" SortOrder="Descending" PropertiesDateEdit-DisplayFormatString="MM/dd/yyyy hh:mm tt" AdaptivePriority="0" />
                                    <dx:BootstrapGridViewDataColumn FieldName="ChildName" Caption="Child" AdaptivePriority="2" />
                                    <dx:BootstrapGridViewDataColumn FieldName="ProblemBehavior" Caption="Problem Behavior" AdaptivePriority="3" />
                                    <dx:BootstrapGridViewDataColumn FieldName="ClassroomName" Caption="Classroom" AdaptivePriority="4" />
                                    <dx:BootstrapGridViewDataColumn FieldName="ProgramName" Caption="Program" AdaptivePriority="5" />
                                    <dx:BootstrapGridViewButtonEditColumn Settings-AllowDragDrop="False" AdaptivePriority="1" CssClasses-DataCell="text-center">
                                        <DataItemTemplate>
                                            <div class="btn-group">
                                                <button id="btnActions" type="button" class="btn btn-secondary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                    Actions
                                                </button>
                                                <div class="dropdown-menu dropdown-menu-right" aria-labelledby="btnActions">
                                                    <a class="dropdown-item" href="/Pages/BehaviorIncident?BehaviorIncidentPK=<%# Eval("BehaviorIncidentPK") %>&Action=View"><i class="fas fa-list"></i>&nbsp;View Details</a>
                                                    <a class="dropdown-item hide-on-view" href="/Pages/BehaviorIncident?BehaviorIncidentPK=<%# Eval("BehaviorIncidentPK") %>&Action=Edit"><i class="fas fa-edit"></i>&nbsp;Edit</a>
                                                    <button class="dropdown-item delete-gridview hide-on-view" data-pk='<%# Eval("BehaviorIncidentPK") %>' data-hf="hfDeleteBehaviorIncidentPK" data-target="#divDeleteBehaviorIncidentModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                </div>
                                            </div>
                                        </DataItemTemplate>
                                    </dx:BootstrapGridViewButtonEditColumn>
                                </Columns>
                            </dx:BootstrapGridView>
                            <dx:EntityServerModeDataSource ID="efBehaviorIncidentDataSource" runat="server"
                                OnSelecting="efBehaviorIncidentDataSource_Selecting" />
                        </ContentTemplate>
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="lbDeleteBehaviorIncident" EventName="Click" />
                        </Triggers>
                    </asp:UpdatePanel>
                </div>
            </div>
        </div>
        <div class="col-lg-5 col-xl-4">
            <div class="card bg-light">
                <div class="card-header">
                    Behavior Incident Reports by Problem Behavior
                </div>
                <div class="card-body">
                    <div class="alert alert-primary">
                        This chart only includes Behavior Incident Reports that have occurred in the past year.
                    </div>
                    <asp:UpdatePanel ID="upProblemBehaviorChart" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>
                            <dx:BootstrapPieChart ID="chartIncidentsByProblemBehavior" runat="server" Palette="SoftPastel">
                                <SettingsLegend Visible="false" OnClientCustomizeItems="createCustomLegend" />
                                <SettingsToolTip Enabled="true" OnClientCustomizeTooltip="customizeTooltip" />
                                <SeriesCollection>
                                    <dx:BootstrapPieChartSeries ArgumentField="ProblemBehavior" ValueField="NumIncidents">
                                        <Label Visible="false">
                                        </Label>
                                    </dx:BootstrapPieChartSeries>
                                </SeriesCollection>
                            </dx:BootstrapPieChart>
                            <div id="divLegend" class="row mt-4">
                            </div>
                        </ContentTemplate>
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="lbDeleteBehaviorIncident" EventName="Click" />
                        </Triggers>
                    </asp:UpdatePanel>
                </div>
            </div>
        </div>
    </div>
    <asp:UpdatePanel ID="upExcelDownload" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <div class="row">
                <div class="col-lg-6 col-xl-6">
                    <div class="card bg-light">
                        <div class="card-header">
                            NCPMI Behavior Incident Excel Report
                        </div>
                        <div class="card-body">
                            <div class="row">
                                <div class="col-lg-6">
                                    <dx:BootstrapListBox ID="lstBxProgram" runat="server" Caption="Program(s)"
                                        SelectionMode="CheckColumn" EnableSelectAll="true" 
                                        ValueField="ProgramPK" ValueType="System.Int32" TextField="ProgramName">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <FilteringSettings ShowSearchUI="true" EditorNullTextDisplayMode="Unfocused" />
                                        <ValidationSettings ValidationGroup="vgBIRExcel" ErrorDisplayMode="ImageWithText" CausesValidation="true">
                                            <RequiredField IsRequired="true" ErrorText="At least one Program must be selected!" />
                                        </ValidationSettings>
                                    </dx:BootstrapListBox>
                                </div>
                                <div class="col-lg-6">
                                    <dx:BootstrapComboBox ID="ddSchoolYear" runat="server" Caption="School Year" NullText="--Select--"
                                        ValueType="System.Int32"
                                        IncrementalFilteringMode="Contains" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgBIRExcel" ErrorDisplayMode="ImageWithText" CausesValidation="true">
                                            <RequiredField IsRequired="true" ErrorText="A school year must be selected!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </div>
                            </div>
                        </div>
                        <div class="card-footer text-center">
                            <div class="row">
                                <div class="col-lg-12">
                                    <asp:HyperLink ID="lnkDownloadExcel" runat="server" Visible="false" Target="_blank" CssClass="btn btn-success"><i class="fas fa-download"></i>&nbsp;Download Report</asp:HyperLink>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-12">
                                    <uc:Submit ID="submitGenerateDownloadLink" runat="server" ValidationGroup="vgBIRExcel" 
                                        SubmitButtonIcon="fas fa-cogs" SubmitButtonText="Generate Download Link" SubmitButtonBootstrapType="Primary"
                                        SubmittingButtonText="Generating..." 
                                        OnSubmitClick="submitGenerateDownloadLink_Click" ShowCancelButton="false" 
                                        OnValidationFailed="submitGenerateDownloadLink_ValidationFailed" />
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
    <div class="modal" id="divDeleteBehaviorIncidentModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete Behavior Incident Report</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this behavior incident report?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteBehaviorIncident" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteBehaviorIncidentModal" OnClick="lbDeleteBehaviorIncident_Click"><i class="fas fa-check"></i>&nbsp;Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
