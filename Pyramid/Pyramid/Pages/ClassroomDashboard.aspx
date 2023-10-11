<%@ Page Title="Classroom Dashboard" Language="C#" MasterPageFile="~/MasterPages/Dashboard.master" AutoEventWireup="true" CodeBehind="ClassroomDashboard.aspx.cs" Inherits="Pyramid.Pages.ClassroomDashboard" %>

<%@ Register Assembly="DevExpress.Web.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Data.Linq" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.Bootstrap.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>
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
            $('#lnkClassroomDashboard').addClass('active');
            
            //Show/hide the view only fields
            setViewOnlyVisibility();
        }

        function setViewOnlyVisibility() {
            //Hide controls if this is a view
            var isView = $('[ID$="hfViewOnly"]').val();
            if (isView == 'True') {
                $('.hide-on-view').addClass('hidden');
            }
            else {
                $('.hide-on-view').removeClass('hidden');
            }
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
            <asp:AsyncPostBackTrigger ControlID="lbDeleteClassroom" EventName="Click" />
        </Triggers>
    </asp:UpdatePanel>
    <asp:HiddenField ID="hfDeleteClassroomPK" runat="server" Value="0" />
    <div class="row">
        <div class="col-lg-7 col-xl-8">
            <div class="card bg-light">
                <div class="card-header">
                    Classrooms
                    <a href="/Pages/Classroom.aspx?ClassroomPK=0&Action=Add" class="btn btn-loader btn-primary float-right hide-on-view hidden"><i class="fas fa-plus"></i>&nbsp;Add New Classroom</a>
                </div>
                <div class="card-body">
                    <asp:UpdatePanel ID="upAllClassroom" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>
                            <label>All Classrooms</label>
                            <div class="alert alert-primary">
                                This table contains all classrooms.
                            </div>
                            <dx:BootstrapGridView ID="bsGRClassrooms" runat="server" EnableCallBacks="false" EnableRowsCache="true" 
                                KeyFieldName="ClassroomPK" AutoGenerateColumns="false" DataSourceID="efClassroomDataSource">
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
                                    <dx:BootstrapGridViewDataColumn FieldName="Location" Caption="Location" AdaptivePriority="2" />
                                    <dx:BootstrapGridViewDataColumn FieldName="IsInfantToddler" Caption="Infant/Toddler?" AdaptivePriority="3">
                                        <DataItemTemplate>
                                            <%# (Convert.ToBoolean(Eval("IsInfantToddler")) ? "Yes" : "No") %>
                                        </DataItemTemplate>
                                    </dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewDataColumn FieldName="IsPreschool" Caption="Preschool?" AdaptivePriority="3">
                                        <DataItemTemplate>
                                            <%# (Convert.ToBoolean(Eval("IsPreschool")) ? "Yes" : "No") %>
                                        </DataItemTemplate>
                                    </dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewDataColumn FieldName="BeingServedSubstitute" Caption="Substitute?" AdaptivePriority="3">
                                        <DataItemTemplate>
                                            <%# (Convert.ToBoolean(Eval("BeingServedSubstitute")) ? "Yes" : "No") %>
                                        </DataItemTemplate>
                                    </dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewDataColumn FieldName="Program.ProgramName" Caption="Program" AdaptivePriority="4" />
                                    <dx:BootstrapGridViewDataColumn Name="StateNameColumn" FieldName="Program.State.Name" Caption="State" AdaptivePriority="6" />
                                    <dx:BootstrapGridViewButtonEditColumn Settings-AllowDragDrop="False" AdaptivePriority="1" CssClasses-DataCell="text-center">
                                        <DataItemTemplate>
                                            <div class="btn-group">
                                                <button type="button" class="btn btn-secondary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                    Actions
                                                </button>
                                                <div class="dropdown-menu dropdown-menu-right">
                                                    <a class="dropdown-item" href="/Pages/Classroom?ClassroomPK=<%# Eval("ClassroomPK") %>&Action=View"><i class="fas fa-list"></i>&nbsp;View Details</a>
                                                    <a class="dropdown-item hide-on-view" href="/Pages/Classroom?ClassroomPK=<%# Eval("ClassroomPK") %>&Action=Edit"><i class="fas fa-edit"></i>&nbsp;Edit</a>
                                                    <button class="dropdown-item delete-gridview hide-on-view" data-pk='<%# Eval("ClassroomPK") %>' data-hf="hfDeleteClassroomPK" data-target="#divDeleteClassroomModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                </div>
                                            </div>
                                        </DataItemTemplate>
                                    </dx:BootstrapGridViewButtonEditColumn>
                                </Columns>
                            </dx:BootstrapGridView>
                            <dx:EntityServerModeDataSource ID="efClassroomDataSource" runat="server"
                                OnSelecting="efClassroomDataSource_Selecting" />
                        </ContentTemplate>
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="lbDeleteClassroom" EventName="Click" />
                        </Triggers>
                    </asp:UpdatePanel>
                </div>
            </div>
        </div>
        <div class="col-lg-5 col-xl-4">
            <div class="card bg-light">
                <div class="card-header">
                    Classrooms by Current Teacher Type
                </div>
                <div class="card-body">
                    <div class="alert alert-primary">
                        This chart includes all classrooms.
                    </div>
                    <asp:UpdatePanel ID="upClassroomTypeChart" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>
                            <dx:BootstrapPieChart ID="chartClassroomType" runat="server" Palette="SoftPastel">
                                <SettingsLegend Visible="false" OnClientCustomizeItems="createCustomLegend" />
                                <SettingsToolTip Enabled="true" OnClientCustomizeTooltip="customizeTooltip" />
                                <SeriesCollection>
                                    <dx:BootstrapPieChartSeries ArgumentField="SubstituteStatus" ValueField="NumClassrooms">
                                        <Label Visible="false">
                                        </Label>
                                    </dx:BootstrapPieChartSeries>
                                </SeriesCollection>
                            </dx:BootstrapPieChart>
                            <div id="divLegend" class="row mt-4">
                            </div>
                        </ContentTemplate>
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="lbDeleteClassroom" EventName="Click" />
                        </Triggers>
                    </asp:UpdatePanel>
                </div>
            </div>
        </div>
    </div>
    <div class="modal" id="divDeleteClassroomModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete Classroom</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this classroom?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteClassroom" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteClassroomModal" OnClick="lbDeleteClassroom_Click"><i class="fas fa-check"></i>&nbsp;Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
