<%@ Page Title="BOQFCC Dashboard" Language="C#" MasterPageFile="~/MasterPages/Dashboard.master" AutoEventWireup="true" CodeBehind="BOQFCCDashboard.aspx.cs" Inherits="Pyramid.Pages.BOQFCCDashboard" %>

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
            $('[ID$="lnkBOQFCCDashboard"]').addClass('active');

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
            <asp:AsyncPostBackTrigger ControlID="lbDeleteBOQFCC" />
        </Triggers>
    </asp:UpdatePanel>
    <asp:HiddenField ID="hfDeleteBOQFCCPK" runat="server" Value="0" />
    <div class="row">
        <div class="col-lg-12 col-xl-12">
            <div class="card bg-light">
                <div class="card-header">
                    Benchmarks of Quality FCC Forms
                    <a href="/Pages/BOQFCC.aspx?BOQFCCPK=0&action=Add" class="btn btn-loader btn-primary float-right hide-on-view hidden"><i class="fas fa-plus"></i>&nbsp;Add New BOQ FCC</a>
                </div>
                <div class="card-body">
                    <asp:UpdatePanel runat="server" ID="upAllBOQFCC">
                        <ContentTemplate>
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
                                    <dx:BootstrapGridViewDataColumn FieldName="Program.ProgramName" Caption="Program Name" AdaptivePriority="2"></dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewDataColumn FieldName="Program.Location" Caption="Program Location" AdaptivePriority="3"></dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewDataColumn FieldName="TeamMembers" Caption="Team Members" AdaptivePriority="4"></dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewButtonEditColumn Settings-AllowDragDrop="False" AdaptivePriority="1" CssClasses-DataCell="text-center">
                                        <DataItemTemplate>
                                            <div class="btn-group">
                                                <button type="button" class="btn btn-secondary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                    Actions
                                                </button>
                                                <div class="dropdown-menu dropdown-menu-right">
                                                    <a class="dropdown-item" href='<%# string.Format("/Pages/BOQFCC.aspx?BOQFCCPK={0}&action={1}", Eval("BenchmarkOfQualityFCCPK").ToString(), "view") %>'><i class="fas fa-list"></i>&nbsp;View Details</a>
                                                    <a class="dropdown-item hide-on-view" href='<%#string.Format("/Pages/BOQFCC.aspx?BOQFCCPK={0}&action={1}", Eval("BenchmarkOfQualityFCCPK").ToString(), "edit") %>'><i class="fas fa-edit"></i>&nbsp;Edit</a>
                                                    <button class="dropdown-item delete-gridview hide-on-view" data-pk='<%# Eval("BenchmarkOfQualityFCCPK") %>' data-hf="hfDeleteBOQFCCPK" data-target="#divDeleteBOQFCCModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                </div>
                                            </div>
                                        </DataItemTemplate>
                                    </dx:BootstrapGridViewButtonEditColumn>
                                </Columns>
                            </dx:BootstrapGridView>
                            <dx:EntityServerModeDataSource ID="efBOQFCCDataSource" runat="server" OnSelecting="efBOQFCCDataSource_Selecting" />
                        </ContentTemplate>
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="lbDeleteBOQFCC" />
                        </Triggers>
                    </asp:UpdatePanel>
                </div>
            </div>
        </div>
    </div>
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
</asp:Content>
