<%@ Page Title="Employee Dashboard" Language="C#" MasterPageFile="~/MasterPages/Dashboard.master" AutoEventWireup="true" CodeBehind="ProgramEmployeeDashboard.aspx.cs" Inherits="Pyramid.Pages.ProgramEmployeeDashboard" %>

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
            $('#lnkEmployeeDashboard').addClass('active');
            
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
            <asp:AsyncPostBackTrigger ControlID="lbDeleteEmployee" EventName="Click" />
        </Triggers>
    </asp:UpdatePanel>
    <asp:HiddenField ID="hfDeleteEmployeePK" runat="server" Value="0" />
    <div class="row">
        <div class="col-lg-12 col-xl-12">
            <div class="card bg-light">
                <div class="card-header">
                    Employees
                    <a href="/Pages/ProgramEmployee.aspx?EmployeePK=0&Action=Add" class="btn btn-loader btn-primary float-right hide-on-view hidden"><i class="fas fa-plus"></i>&nbsp;Add New Employee</a>
                </div>
                <div class="card-body">
                    <asp:UpdatePanel ID="upAllEmployees" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>
                            <label>All Employees</label>
                            <div class="alert alert-primary">
                                This table contains all employees, regardless of their employment status.
                            </div>
                            <dx:BootstrapGridView ID="bsGREmployees" runat="server" EnableCallBacks="false" EnableRowsCache="true" 
                                KeyFieldName="ProgramEmployeePK" AutoGenerateColumns="false" DataSourceID="efEmployeeDataSource">
                                <SettingsPager PageSize="15" />
                                <SettingsBootstrap Striped="true" />
                                <SettingsBehavior EnableRowHotTrack="true" />
                                <SettingsSearchPanel Visible="true" ShowApplyButton="true" />
                                <Settings ShowGroupPanel="false" />
                                <CssClasses IconHeaderSortUp="fas fa-long-arrow-alt-up" IconHeaderSortDown="fas fa-long-arrow-alt-down" IconShowAdaptiveDetailButton="fas fa-plus-circle" />
                                <SettingsAdaptivity AdaptivityMode="HideDataCells" AllowOnlyOneAdaptiveDetailExpanded="true" AdaptiveColumnPosition="Left"></SettingsAdaptivity>
                                <Columns>
                                    <dx:BootstrapGridViewDataColumn FieldName="Name" Caption="Name" SortIndex="0" SortOrder="Ascending" AdaptivePriority="0" />
                                    <dx:BootstrapGridViewDataColumn FieldName="JobFunctions" Caption="Job Function(s)" AdaptivePriority="3" Settings-AllowFilterBySearchPanel="False" Settings-AllowSort="False">
                                        <DataItemTemplate>
                                            <%# string.Join(", ", (IEnumerable<string>)Eval("JobFunctions")) %>
                                        </DataItemTemplate>
                                    </dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewDateColumn FieldName="HireDate" Caption="Hire Date" PropertiesDateEdit-DisplayFormatString="MM/dd/yyyy" AdaptivePriority="2" />
                                    <dx:BootstrapGridViewDateColumn FieldName="TermDate" Caption="Termination Date" PropertiesDateEdit-DisplayFormatString="MM/dd/yyyy" AdaptivePriority="4" />
                                        <dx:BootstrapGridViewDataColumn FieldName="EmailAddress" Caption="Email" AdaptivePriority="5"
                                            CssClasses-DataCell="hidden" CssClasses-FilterCell="hidden" CssClasses-FooterCell="hidden" 
                                            CssClasses-EditCell="hidden" CssClasses-HeaderCell="hidden" CssClasses-GroupFooterCell="hidden" />
                                    <dx:BootstrapGridViewDataColumn FieldName="ProgramName" Caption="Program" AdaptivePriority="4" />
                                    <dx:BootstrapGridViewButtonEditColumn Settings-AllowDragDrop="False" AdaptivePriority="1" CssClasses-DataCell="text-center">
                                        <DataItemTemplate>
                                            <div class="btn-group">
                                                <button id="btnActions" type="button" class="btn btn-secondary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                    Actions
                                                </button>
                                                <div class="dropdown-menu dropdown-menu-right" aria-labelledby="btnActions">
                                                    <a class="dropdown-item" href="/Pages/ProgramEmployee?ProgramEmployeePK=<%# Eval("ProgramEmployeePK") %>&Action=View"><i class="fas fa-list"></i>&nbsp;View Details</a>
                                                    <a class="dropdown-item hide-on-view" href="/Pages/ProgramEmployee?ProgramEmployeePK=<%# Eval("ProgramEmployeePK") %>&Action=Edit"><i class="fas fa-edit"></i>&nbsp;Edit</a>
                                                    <button class="dropdown-item delete-gridview hide-on-view" data-pk='<%# Eval("ProgramEmployeePK") %>' data-hf="hfDeleteEmployeePK" data-target="#divDeleteEmployeeModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                </div>
                                            </div>
                                        </DataItemTemplate>
                                    </dx:BootstrapGridViewButtonEditColumn>
                                </Columns>
                            </dx:BootstrapGridView>
                            <dx:EntityServerModeDataSource ID="efEmployeeDataSource" runat="server"
                                OnSelecting="efEmployeeDataSource_Selecting" />
                        </ContentTemplate>
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="lbDeleteEmployee" EventName="Click" />
                        </Triggers>
                    </asp:UpdatePanel>
                </div>
            </div>
        </div>
    </div>
    <div class="modal" id="divDeleteEmployeeModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete Employee</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this employee, their training history, their job function history, and their classroom assignment history?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteEmployee" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteEmployeeModal" OnClick="lbDeleteEmployee_Click"><i class="fas fa-check"></i>&nbsp;Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
