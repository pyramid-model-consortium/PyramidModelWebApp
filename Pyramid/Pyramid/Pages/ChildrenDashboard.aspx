<%@ Page Title="Children Dashboard" Language="C#" MasterPageFile="~/MasterPages/Dashboard.master" AutoEventWireup="true" CodeBehind="ChildrenDashboard.aspx.cs" Inherits="Pyramid.Pages.ChildrenDashboard" %>

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
            //Highlight the correct link in the master page
            $('#lnkChildrenDashboard').addClass('active');
            
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
            <asp:AsyncPostBackTrigger ControlID="lbDeleteChild" EventName="Click" />
        </Triggers>
    </asp:UpdatePanel>
    <asp:HiddenField ID="hfDeleteChildProgramPK" runat="server" Value="0" />
    <div class="row">
        <div class="col-lg-7 col-xl-8">
            <div class="card bg-light">
                <div class="card-header">
                    Children
                    <a href="/Pages/Child.aspx?ChildProgramPK=0&Action=Add" class="btn btn-loader btn-loader btn-primary float-right hide-on-view hidden"><i class="fas fa-plus"></i>&nbsp;Add New Child</a>
                </div>
                <div class="card-body">
                    <asp:UpdatePanel ID="upAllChildren" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>
                            <label>All Children</label>
                            <div class="alert alert-primary">
                                This table contains all children, regardless of enrollment status.
                            </div>
                            <dx:BootstrapGridView ID="bsGRChildren" runat="server" EnableCallBacks="false" EnableRowsCache="true" 
                                KeyFieldName="ChildProgramPK" AutoGenerateColumns="false" DataSourceID="efChildDataSource">
                                <SettingsPager PageSize="15" />
                                <SettingsBootstrap Striped="true" />
                                <SettingsBehavior EnableRowHotTrack="true" />
                                <SettingsSearchPanel Visible="true" ShowApplyButton="true" />
                                <Settings ShowGroupPanel="false" />
                                <CssClasses IconHeaderSortUp="fas fa-long-arrow-alt-up" IconHeaderSortDown="fas fa-long-arrow-alt-down" IconShowAdaptiveDetailButton="fas fa-plus-circle" />
                                <SettingsAdaptivity AdaptivityMode="HideDataCells" AllowOnlyOneAdaptiveDetailExpanded="true" AdaptiveColumnPosition="Left"></SettingsAdaptivity>
                                <Columns>
                                    <dx:BootstrapGridViewDataColumn FieldName="ProgramSpecificID" Caption="ID" Width="10%" AdaptivePriority="0" />
                                    <dx:BootstrapGridViewDataColumn FieldName="Name" Caption="Name" SortIndex="0" SortOrder="Ascending" AdaptivePriority="0" />
                                    <dx:BootstrapGridViewDateColumn FieldName="BirthDate" PropertiesDateEdit-DisplayFormatString="MM/dd/yyyy" AdaptivePriority="2" />
                                    <dx:BootstrapGridViewDateColumn FieldName="EnrollmentDate" PropertiesDateEdit-DisplayFormatString="MM/dd/yyyy" AdaptivePriority="3" />
                                    <dx:BootstrapGridViewDateColumn FieldName="DischargeDate" PropertiesDateEdit-DisplayFormatString="MM/dd/yyyy" AdaptivePriority="4" />
                                    <dx:BootstrapGridViewDataColumn FieldName="DischargeReason" Caption="Discharge Reason" AdaptivePriority="8">
                                        <DataItemTemplate>
                                            <%# Eval("DischargeReason") + (Eval("DischargeReasonSpecify") == null ? "" : " (" + Eval("DischargeReasonSpecify") + ")") %>
                                        </DataItemTemplate>
                                    </dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewDataColumn FieldName="HasIEP" Caption="IEP?" AdaptivePriority="6">
                                        <DataItemTemplate>
                                            <%# (Convert.ToBoolean(Eval("HasIEP")) ? "Yes" : "No") %>
                                        </DataItemTemplate>
                                    </dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewDataColumn FieldName="IsDLL" Caption="DLL?" AdaptivePriority="7">
                                        <DataItemTemplate>
                                            <%# (Convert.ToBoolean(Eval("IsDLL")) ? "Yes" : "No") %>
                                        </DataItemTemplate>
                                    </dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewDataColumn FieldName="ProgramName" Caption="Program" AdaptivePriority="5" />
                                    <dx:BootstrapGridViewButtonEditColumn Settings-AllowDragDrop="False" AdaptivePriority="1" CssClasses-DataCell="text-center">
                                        <DataItemTemplate>
                                            <div class="btn-group">
                                                <button type="button" class="btn btn-secondary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                    Actions
                                                </button>
                                                <div class="dropdown-menu dropdown-menu-right">
                                                    <a class="dropdown-item" href="/Pages/Child?ChildProgramPK=<%# Eval("ChildProgramPK") %>&Action=View"><i class="fas fa-list"></i>&nbsp;View Details</a>
                                                    <a class="dropdown-item hide-on-view" href="/Pages/Child?ChildProgramPK=<%# Eval("ChildProgramPK") %>&Action=Edit"><i class="fas fa-edit"></i>&nbsp;Edit</a>
                                                    <button class="dropdown-item delete-gridview hide-on-view" data-pk='<%# Eval("ChildProgramPK") %>' data-hf="hfDeleteChildProgramPK" data-target="#divDeleteChildModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                </div>
                                            </div>
                                        </DataItemTemplate>
                                    </dx:BootstrapGridViewButtonEditColumn>
                                </Columns>
                            </dx:BootstrapGridView>
                            <dx:EntityServerModeDataSource ID="efChildDataSource" runat="server"
                                OnSelecting="efChildDataSource_Selecting" />
                        </ContentTemplate>
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="lbDeleteChild" EventName="Click" />
                        </Triggers>
                    </asp:UpdatePanel>
                </div>
            </div>
        </div>
        <div class="col-lg-5 col-xl-4">
            <div class="card bg-light">
                <div class="card-header">
                    Children by Race
                </div>
                <div class="card-body">
                    <div class="alert alert-primary">
                        This chart only includes children that are active as of today. (Active means that the child has not been discharged)
                    </div>
                    <asp:UpdatePanel ID="upRaceChart" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>
                            <dx:BootstrapPieChart ID="chartChildRace" runat="server" Palette="SoftPastel">
                                <SettingsLegend Visible="false" OnClientCustomizeItems="createCustomLegend" />
                                <SettingsToolTip Enabled="true" OnClientCustomizeTooltip="customizeTooltip" />
                                <SeriesCollection>
                                    <dx:BootstrapPieChartSeries ArgumentField="RaceName" ValueField="NumKids">
                                        <Label Visible="false">
                                        </Label>
                                    </dx:BootstrapPieChartSeries>
                                </SeriesCollection>
                            </dx:BootstrapPieChart>
                            <div id="divLegend" class="row mt-4">
                            </div>
                        </ContentTemplate>
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="lbDeleteChild" EventName="Click" />
                        </Triggers>
                    </asp:UpdatePanel>
                </div>
            </div>
        </div>
    </div>
    <div class="modal" id="divDeleteChildModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete Child</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this child, all their notes, their status history, and their classroom assignment history?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteChild" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteChildModal" OnClick="lbDeleteChild_Click"><i class="fas fa-check"></i>&nbsp;Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
