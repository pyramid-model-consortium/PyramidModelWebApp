<%@ Page Title="TPITOS Dashboard" Language="C#" MasterPageFile="~/MasterPages/Dashboard.master" AutoEventWireup="true" CodeBehind="TPITOSDashboard.aspx.cs" Inherits="Pyramid.Pages.TPITOSDashboard" %>

<%@ Register Assembly="DevExpress.Web.v19.1, Version=19.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Data.Linq" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.Bootstrap.v19.1, Version=19.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.v19.1, Version=19.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>
<%@ Register TagPrefix="uc" TagName="Messaging" Src="~/User_Controls/MessagingSystem.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .invalid-tpitos {
            background-color: #f8d7da !important;
            border-color: #f5c6cb !important;
        }
    </style>
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
            $('#lnkTPITOSDashboard').addClass('active');

            //Get any invalid TPITOS rows
            var invalidRows = $('.invalid-tpitos');

            //Show the invalid alert if there are invalid rows
            if (invalidRows.length > 0) {
                $('#divInvalidAlert').removeClass('hidden');
            }

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
            <asp:AsyncPostBackTrigger ControlID="lbDeleteTPITOS" />
        </Triggers>
    </asp:UpdatePanel>
    <asp:HiddenField ID="hfDeleteTPITOSPK" runat="server" Value="0" />
    <div class="row">
        <div class="col-lg-12 col-xl-12">
            <div class="card bg-light">
                <div class="card-header">
                    TPITOS Observations
                    <a href="/Pages/TPITOS.aspx?TPITOSPK=0&action=Add" class="btn btn-loader btn-primary float-right hide-on-view hidden"><i class="fas fa-plus"></i>&nbsp;Add New TPITOS Observation</a>
                </div>
                <div class="card-body">
                    <div id="divInvalidAlert" class="alert alert-warning hidden">
                        <i class="fas fa-exclamation-triangle"></i>&nbsp;<span class="font-weight-bold">At least one observation is invalid!</span>
                        <div class="mt-2">
                            To fix this, follow these steps:
                        </div>
                        <ul>
                            <li>Find the invalid observation(s) by looking for rows highlighted in red or searching &quot;invalid&quot; in the search box below.</li>
                            <li>Edit the observation(s) and fix any validation errors that occur when you try to save the observation(s).</li>
                        </ul> 
                    </div>
                    <asp:UpdatePanel runat="server" ID="upAllTPITOS">
                        <ContentTemplate>
                            <label>All TPITOS Observations</label>
                            <div class="alert alert-primary">
                                This table contains all TPITOS Observations regardless of when they were performed.
                            </div>
                            <dx:BootstrapGridView ID="bsGRTPITOS" runat="server" EnableCallBacks="false" EnableRowsCache="true" KeyFieldName="TPITOSPK"
                                AutoGenerateColumns="false" DataSourceID="efTPITOSDataSource" OnHtmlRowPrepared="bsGRTPITOS_HtmlRowPrepared">
                                <SettingsPager PageSize="15" />
                                <SettingsBootstrap Striped="true" />
                                <SettingsBehavior EnableRowHotTrack="true" />
                                <Settings ShowGroupPanel="false" />
                                <SettingsSearchPanel Visible="true" ShowApplyButton="true" />
                                <CssClasses IconHeaderSortUp="fas fa-long-arrow-alt-up" IconHeaderSortDown="fas fa-long-arrow-alt-down" IconShowAdaptiveDetailButton="fas fa-plus-circle" />
                                <SettingsAdaptivity AdaptivityMode="HideDataCells" AllowOnlyOneAdaptiveDetailExpanded="true" AdaptiveColumnPosition="Left"></SettingsAdaptivity>
                                <Columns>
                                    <dx:BootstrapGridViewDateColumn FieldName="ObservationStartDateTime" Caption="Observation Date" SortIndex="0" SortOrder="Descending" PropertiesDateEdit-DisplayFormatString="MM/dd/yyyy" AdaptivePriority="0"></dx:BootstrapGridViewDateColumn>
                                    <dx:BootstrapGridViewDataColumn FieldName="ClassroomIdAndName" Caption="Classroom" AdaptivePriority="2"></dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewDateColumn FieldName="ObservationStartDateTime" Caption="Start Time" PropertiesDateEdit-DisplayFormatString="hh:mm tt" AdaptivePriority="3"></dx:BootstrapGridViewDateColumn>
                                    <dx:BootstrapGridViewDateColumn FieldName="ObservationEndDateTime" Caption="End Time" PropertiesDateEdit-DisplayFormatString="hh:mm tt" AdaptivePriority="4"></dx:BootstrapGridViewDateColumn>
                                    <dx:BootstrapGridViewDataColumn FieldName="IsValidText" Caption="Valid?" AdaptivePriority="5"></dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewDataColumn FieldName="ProgramName" Caption="Program" AdaptivePriority="6"></dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewButtonEditColumn Settings-AllowDragDrop="False" AdaptivePriority="1" CssClasses-DataCell="text-center">
                                        <DataItemTemplate>
                                            <div class="btn-group">
                                                <button type="button" class="btn btn-secondary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                    Actions
                                                </button>
                                                <div class="dropdown-menu dropdown-menu-right">
                                                    <a class="dropdown-item" href='<%# string.Format("/Pages/TPITOS.aspx?TPITOSPK={0}&action={1}",Eval("TPITOSPK").ToString(),"view") %>'><i class="fas fa-list"></i>&nbsp;View Details</a>
                                                    <a class="dropdown-item hide-on-view" href='<%# string.Format("/Pages/TPITOS.aspx?TPITOSPK={0}&action={1}",Eval("TPITOSPK").ToString(),"edit") %>'><i class="fas fa-edit"></i>&nbsp;Edit</a>
                                                    <button class="dropdown-item delete-gridview hide-on-view" data-pk='<%# Eval("TPITOSPK") %>' data-hf="hfDeleteTPITOSPK" data-target="#divDeleteTPITOSModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                </div>
                                            </div>
                                        </DataItemTemplate>
                                    </dx:BootstrapGridViewButtonEditColumn>
                                </Columns>
                            </dx:BootstrapGridView>
                            <dx:EntityServerModeDataSource ID="efTPITOSDataSource" runat="server" OnSelecting="efTPITOSDataSource_Selecting" />
                        </ContentTemplate>
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="lbDeleteTPITOS" EventName="Click" />
                        </Triggers>
                    </asp:UpdatePanel>
                </div>
            </div>
        </div>
    </div>
    <div class="modal" id="divDeleteTPITOSModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete TPITOS</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this TPITOS observation?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteTPITOS" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteTPITOSModal" OnClick="lbDeleteTPITOS_Click"><i class="fas fa-check"></i>&nbsp;Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
