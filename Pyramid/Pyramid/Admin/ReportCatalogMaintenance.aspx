<%@ Page Title="Report Catalog Maintenance" Language="C#" MasterPageFile="~/MasterPages/LoggedIn.master" AutoEventWireup="true" CodeBehind="ReportCatalogMaintenance.aspx.cs" Inherits="Pyramid.Admin.ReportCatalogMaintenance" %>

<%@ Register TagPrefix="uc" TagName="Messaging" Src="~/User_Controls/MessagingSystem.ascx" %>
<%@ Register Assembly="DevExpress.Web.v19.1, Version=19.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Data.Linq" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.Bootstrap.v19.1, Version=19.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>

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
            //Show/hide the view only fields
            setViewOnlyVisibility();
        }
    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <asp:HiddenField ID="hfViewOnly" runat="server" Value="False" />
    <asp:UpdatePanel ID="upMessaging" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <uc:Messaging ID="msgSys" runat="server" />
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="lbDeleteReportCatalogItem" />
        </Triggers>
    </asp:UpdatePanel>
    <asp:HiddenField ID="hfDeleteReportCatalogItemPK" runat="server" Value="0" />
    <div class="row">
        <div class="col-lg-12">
            <div class="card bg-light">
                <div class="card-header">
                    All Report Catalog Items
                    <a href="/Admin/ReportCatalogItem?ReportCatalogPK=0&Action=Add" class="btn btn-loader btn-primary float-right hide-on-view hidden"><i class="fas fa-plus"></i>&nbsp;Add New Report Catalog Item</a>
                </div>
                <div class="card-body">
                    <asp:UpdatePanel runat="server" ID="upAllReportCatalogItems">
                        <ContentTemplate>
                    <dx:BootstrapGridView ID="bsGRReports" runat="server" EnableCallBacks="false" KeyFieldName="ReportCatalogPK" 
                        AutoGenerateColumns="false" DataSourceID="efReportDataSource">
                        <SettingsPager PageSize="15" />
                        <SettingsBootstrap Striped="true" />
                        <SettingsBehavior EnableRowHotTrack="true" />
                        <SettingsSearchPanel Visible="true" ShowApplyButton="true" />
                        <Settings ShowGroupPanel="true" />
                        <CssClasses IconHeaderSortUp="fas fa-long-arrow-alt-up" IconHeaderSortDown="fas fa-long-arrow-alt-down" IconShowAdaptiveDetailButton="fas fa-plus-circle" />
                        <SettingsAdaptivity AdaptivityMode="HideDataCells" AllowOnlyOneAdaptiveDetailExpanded="true" AdaptiveColumnPosition="Left"></SettingsAdaptivity>
                        <Columns>
                            <dx:BootstrapGridViewDataColumn FieldName="ReportName" Caption="Report Name" SortIndex="0" SortOrder="Ascending" AdaptivePriority="0" />
                            <dx:BootstrapGridViewDataColumn FieldName="ReportCategory" Caption="Category" AdaptivePriority="1" />
                            <dx:BootstrapGridViewDataColumn FieldName="ReportDescription" Caption="Description" AdaptivePriority="2" />
                            <dx:BootstrapGridViewDataColumn FieldName="CriteriaOptions" Caption="Criteria" AdaptivePriority="3" />
                            <dx:BootstrapGridViewDataColumn FieldName="OptionalCriteriaOptions" Caption="Optional Criteria" AdaptivePriority="4" />
                            <dx:BootstrapGridViewDataColumn FieldName="ReportClass" Caption="Class" AdaptivePriority="5" />
                            <dx:BootstrapGridViewButtonEditColumn Settings-AllowDragDrop="False" AdaptivePriority="0">
                                <DataItemTemplate>
                                    <div class="btn-group">
                                        <button id="btnActions" type="button" class="btn btn-secondary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                            Actions
                                        </button>
                                        <div class="dropdown-menu dropdown-menu-right" aria-labelledby="btnActions">
                                            <a class="dropdown-item" href="/Admin/ReportCatalogItem?ReportCatalogPK=<%# Eval("ReportCatalogPK") %>&Action=View"><i class="fas fa-list"></i>&nbsp;View Details</a>
                                            <a class="dropdown-item" target="_blank" href="/Pages/ViewFile.aspx?ReportCatalogPK=<%# Eval("ReportCatalogPK") %>"><i class="fas fa-file-download"></i>&nbsp;View/Download Documentation</a>
                                            <a class="dropdown-item hide-on-view" href="/Admin/ReportCatalogItem?ReportCatalogPK=<%# Eval("ReportCatalogPK") %>&Action=Edit"><i class="fas fa-edit"></i>&nbsp;Edit</a>
                                            <button class="dropdown-item delete-gridview hide-on-view" data-pk='<%# Eval("ReportCatalogPK") %>' data-hf="hfDeleteReportCatalogItemPK" data-target="#divDeleteReportCatalogItemModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                        </div>
                                    </div>
                                    
                                </DataItemTemplate>
                            </dx:BootstrapGridViewButtonEditColumn>
                        </Columns>
                    </dx:BootstrapGridView>
                    <dx:EntityServerModeDataSource ID="efReportDataSource" runat="server"
                        OnSelecting="efReportDataSource_Selecting" />
                            </ContentTemplate>
                        </asp:UpdatePanel>
                </div>
            </div>
        </div>
    </div>
    <div class="modal" id="divDeleteReportCatalogItemModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete Report Catalog Item</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this Report Catalog Item?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteReportCatalogItem" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteReportCatalogItemModal" OnClick="lbDeleteReportCatalogItem_Click"><i class="fas fa-check"></i>&nbsp;Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
