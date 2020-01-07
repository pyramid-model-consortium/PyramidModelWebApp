<%@ Page Title="Other Social Emotional Screening Dashboard" Language="C#" MasterPageFile="~/MasterPages/Dashboard.master" AutoEventWireup="true" CodeBehind="OtherSEScreenDashboard.aspx.cs" Inherits="Pyramid.Pages.OtherSEScreenDashboard" %>

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
            $('#lnkOtherSEScreenDashboard').addClass('active');
            
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
            <asp:AsyncPostBackTrigger ControlID="lbDeleteOtherSEScreen" />
        </Triggers>
    </asp:UpdatePanel>
    <asp:HiddenField ID="hfDeleteOtherSEScreenPK" runat="server" Value="0" />
    <div class="row">
        <div class="col-lg-7 col-xl-8">
            <div class="card bg-light">
                <div class="card-header">
                    Other Social Emotional Screenings
                    <a href="/Pages/OtherSEScreen.aspx?OtherSEScreenPK=0&action=Add" class="btn btn-loader btn-primary float-right hide-on-view hidden"><i class="fas fa-plus"></i>&nbsp;Add New Social-Emotional Screen</a>
                </div>
                <div class="card-body">
                    <asp:UpdatePanel runat="server" ID="upAllOtherSEScreen">
                        <ContentTemplate>
                            <label>All Other Social Emotional Screenings</label>
                            <div class="alert alert-primary">
                                This table contains all other social emotional screenings regardless of when they were performed.
                            </div>
                            <dx:BootstrapGridView ID="bsGROtherSEScreen" runat="server" EnableCallBacks="false" EnableRowsCache="true" KeyFieldName="OtherSEScreenPK"
                                AutoGenerateColumns="false" DataSourceID="efOtherSEScreenDataSource">
                                <SettingsPager PageSize="15" />
                                <SettingsBootstrap Striped="true" />
                                <SettingsBehavior EnableRowHotTrack="true" />
                                <Settings ShowGroupPanel="false" />
                                <SettingsSearchPanel Visible="true" ShowApplyButton="true" />
                                <CssClasses IconHeaderSortUp="fas fa-long-arrow-alt-up" IconHeaderSortDown="fas fa-long-arrow-alt-down" IconShowAdaptiveDetailButton="fas fa-plus-circle" />
                                <SettingsAdaptivity AdaptivityMode="HideDataCells" AllowOnlyOneAdaptiveDetailExpanded="true" AdaptiveColumnPosition="Left"></SettingsAdaptivity>
                                <Columns>
                                    <dx:BootstrapGridViewDateColumn FieldName="ScreenDate" SortIndex="0" SortOrder="Descending" PropertiesDateEdit-DisplayFormatString="MM/dd/yyyy" AdaptivePriority="0"></dx:BootstrapGridViewDateColumn>
                                    <dx:BootstrapGridViewDataColumn FieldName="ScreenType" Caption="Screen Type" AdaptivePriority="2"></dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewDataColumn FieldName="ChildIdAndName" Caption="Child" AdaptivePriority="3"></dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewDataColumn FieldName="Score" Caption="Total Score" AdaptivePriority="4"></dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewDataColumn FieldName="ScoreType" Caption="Score Type" AdaptivePriority="5"></dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewDataColumn FieldName="ProgramName" Caption="Program" AdaptivePriority="6"></dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewButtonEditColumn Settings-AllowDragDrop="False" AdaptivePriority="1" CssClasses-DataCell="text-center">
                                        <DataItemTemplate>
                                            <div class="btn-group">
                                                <button id="btnActions" type="button" class="btn btn-secondary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                    Actions
                                               
                                                </button>
                                                <div class="dropdown-menu dropdown-menu-right" aria-labelledby="btnActions">
                                                    <a class="dropdown-item" href='<%# string.Format("/Pages/OtherSEScreen.aspx?OtherSEScreenPK={0}&action={1}",Eval("OtherSEScreenPK").ToString(),"view") %>'><i class="fas fa-list"></i>&nbsp;View Details</a>
                                                    <a class="dropdown-item hide-on-view" href='<%# string.Format("/Pages/OtherSEScreen.aspx?OtherSEScreenPK={0}&action={1}",Eval("OtherSEScreenPK").ToString(),"edit") %>'><i class="fas fa-edit"></i>&nbsp;Edit</a>
                                                    <button class="dropdown-item delete-gridview hide-on-view" data-pk='<%# Eval("OtherSEScreenPK") %>' data-hf="hfDeleteOtherSEScreenPK" data-target="#divDeleteOtherSEScreenModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                </div>
                                            </div>
                                        </DataItemTemplate>
                                    </dx:BootstrapGridViewButtonEditColumn>
                                </Columns>
                            </dx:BootstrapGridView>
                            <dx:EntityServerModeDataSource ID="efOtherSEScreenDataSource" runat="server" OnSelecting="efOtherSEScreenDataSource_Selecting" />
                        </ContentTemplate>
                        <Triggers>
                        </Triggers>
                    </asp:UpdatePanel>
                </div>
            </div>
        </div>
        <div class="col-lg-5 col-xl-4">
            <div class="card bg-light">
                <div class="card-header">
                    Other Social Emotional Screenings by Score Type
                </div>
                <div class="card-body">
                    <div class="alert alert-primary">
                        This chart only includes other social emotional screenings performed in the past year.
                    </div>
                    <asp:UpdatePanel ID="upScoreTypeChart" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>
                            <dx:BootstrapPieChart ID="chartScoreType" runat="server" Palette="SoftPastel">
                                <SettingsLegend Visible="false" OnClientCustomizeItems="createCustomLegend" />
                                <SettingsToolTip Enabled="true" OnClientCustomizeTooltip="customizeTooltip" />
                                <SeriesCollection>
                                    <dx:BootstrapPieChartSeries ArgumentField="ScoreType" ValueField="NumOtherSEScreens">
                                        <Label Visible="false">
                                        </Label>
                                    </dx:BootstrapPieChartSeries>
                                </SeriesCollection>
                            </dx:BootstrapPieChart>
                            <div id="divLegend" class="row mt-4">
                            </div>
                        </ContentTemplate>
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="lbDeleteOtherSEScreen" EventName="Click" />
                        </Triggers>
                    </asp:UpdatePanel>
                </div>
            </div>
        </div>
    </div>
    <div class="modal" id="divDeleteOtherSEScreenModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete Social Emotional Screening</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this social emotional screening?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteOtherSEScreen" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteOtherSEScreenModal" OnClick="lbDeleteOtherSEScreen_Click"><i class="fas fa-check"></i>&nbsp;Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
