<%@ Page Title="Classroom Coaching Log Dashboard" Language="C#" MasterPageFile="~/MasterPages/Dashboard.master" AutoEventWireup="true" CodeBehind="CoachingLogDashboard.aspx.cs" Inherits="Pyramid.Pages.CoachingLogDashboard" %>

<%@ Register Assembly="DevExpress.Web.v19.1, Version=19.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Data.Linq" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.Bootstrap.v19.1, Version=19.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.v19.1, Version=19.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>
<%@ Register TagPrefix="uc" TagName="Messaging" Src="~/User_Controls/MessagingSystem.ascx" %>

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
            $('#lnkCoachingLogDashboard').addClass('active');

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
            <asp:AsyncPostBackTrigger ControlID="lbDeleteCoachingLog" />
        </Triggers>
    </asp:UpdatePanel>
    <asp:HiddenField ID="hfDeleteCoachingLogPK" runat="server" Value="0" />
    <div class="row">
        <div class="col-lg-12 col-xl-12">
            <div class="card bg-light">
                <div class="card-header">
                    Classroom Coaching Logs
                    <a href="/Pages/CoachingLog.aspx?CoachingLogPK=0&action=Add" class="btn btn-loader btn-primary float-right hide-on-view hidden"><i class="fas fa-plus"></i>&nbsp;Add New Coaching Log</a>
                </div>
                <div class="card-body">
                    <asp:UpdatePanel runat="server" ID="upAllCoachingLog">
                        <ContentTemplate>
                            <label>All Classroom Coaching Logs</label>
                            <div class="alert alert-primary">
                                This table contains all Classroom Coaching Logs regardless of when they were performed.
                            </div>
                            <dx:BootstrapGridView ID="bsGRCoachingLog" runat="server" EnableCallBacks="false" EnableRowsCache="true" KeyFieldName="CoachingLogPK"
                                AutoGenerateColumns="false" DataSourceID="efCoachingLogDataSource">
                                <SettingsPager PageSize="15" />
                                <SettingsBootstrap Striped="true" />
                                <SettingsBehavior EnableRowHotTrack="true" />
                                <Settings ShowGroupPanel="false" />
                                <SettingsSearchPanel Visible="true" ShowApplyButton="true" />
                                <CssClasses IconHeaderSortUp="fas fa-long-arrow-alt-up" IconHeaderSortDown="fas fa-long-arrow-alt-down" IconShowAdaptiveDetailButton="fas fa-plus-circle" />
                                <SettingsAdaptivity AdaptivityMode="HideDataCells" AllowOnlyOneAdaptiveDetailExpanded="true" AdaptiveColumnPosition="Left"></SettingsAdaptivity>
                                <Columns>
                                    <dx:BootstrapGridViewDateColumn FieldName="LogDate" SortIndex="0" SortOrder="Descending" PropertiesDateEdit-DisplayFormatString="MM/dd/yyyy" AdaptivePriority="0"></dx:BootstrapGridViewDateColumn>
                                    <dx:BootstrapGridViewDataColumn FieldName="DurationMinutes" Caption="Duration (minutes)" AdaptivePriority="3" HorizontalAlign="Left"></dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewDataColumn FieldName="CoachName" Caption="Coach" AdaptivePriority="3"></dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewDataColumn FieldName="TeacherName" Caption="Teacher" AdaptivePriority="4"></dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewDataColumn FieldName="ProgramName" Caption="Program" AdaptivePriority="6"></dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewButtonEditColumn Settings-AllowDragDrop="False" AdaptivePriority="1" CssClasses-DataCell="text-center">
                                        <DataItemTemplate>
                                            <div class="btn-group">
                                                <button id="btnActions" type="button" class="btn btn-secondary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                    Actions
                                                </button>
                                                <div class="dropdown-menu dropdown-menu-right" aria-labelledby="btnActions">
                                                    <a class="dropdown-item" href='<%# string.Format("/Pages/CoachingLog.aspx?CoachingLogPK={0}&action={1}",Eval("CoachingLogPK").ToString(),"view") %>'><i class="fas fa-list"></i>&nbsp;View Details</a>
                                                    <a class="dropdown-item hide-on-view" href='<%# string.Format("/Pages/CoachingLog.aspx?CoachingLogPK={0}&action={1}",Eval("CoachingLogPK").ToString(),"edit") %>'><i class="fas fa-edit"></i>&nbsp;Edit</a>
                                                    <button class="dropdown-item delete-gridview hide-on-view" data-pk='<%# Eval("CoachingLogPK") %>' data-hf="hfDeleteCoachingLogPK" data-target="#divDeleteCoachingLogModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                </div>
                                            </div>
                                        </DataItemTemplate>
                                    </dx:BootstrapGridViewButtonEditColumn>
                                </Columns>
                            </dx:BootstrapGridView>
                            <dx:EntityServerModeDataSource ID="efCoachingLogDataSource" runat="server" OnSelecting="efCoachingLogDataSource_Selecting" />
                        </ContentTemplate>
                        <Triggers>
                        </Triggers>
                    </asp:UpdatePanel>
                </div>
            </div>
        </div>
    </div>
    <div class="modal" id="divDeleteCoachingLogModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete Classroom Coaching Log</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this Classroom Coaching Log?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteCoachingLog" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteCoachingLogModal" OnClick="lbDeleteCoachingLog_Click"><i class="fas fa-check"></i>&nbsp;Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
