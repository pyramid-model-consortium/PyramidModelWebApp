<%@ Page Title="State Leadership Team Dashboard" Language="C#" MasterPageFile="~/MasterPages/Dashboard.master" AutoEventWireup="true" CodeBehind="SLTDashboard.aspx.cs" Inherits="Pyramid.Pages.SLTDashboard" %>

<%@ Register Assembly="DevExpress.Web.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Data.Linq" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.Bootstrap.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>
<%@ Register TagPrefix="uc" TagName="Messaging" Src="~/User_Controls/MessagingSystem.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style type="text/css">
        .not-fully-reviewed-row {
            color: rgb(133, 100, 4) !important;
            background-color: rgb(255, 243, 205) !important;
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
            $('[ID$="lnkSLTDashboard"]').addClass('active');

            //Show/hide the view only fields
            setViewOnlyVisibility();

            //Set up the events for the help buttons
            $('#btnSLTAPHelp').popover({
                trigger: 'hover focus',
                title: 'Help',
                html: true,
                content: 'The Add New Blank Action Plan button will allow you to create a new action plan without any information being pre-filled.<br/><br/>' +
                    'The Prefill New Action Plan button will create a new action plan and fill it with all the information ' +
                    'from the most recent action plan for your state. You will then be able to review and modify the new action plan.'
            });

            //Highlight the unreviewed action plans
            highlightReviewRows();
        }

        function highlightReviewRows() {
            $('.ap-fully-reviewed').each(function () {
                //Get the controls
                var currentGridCell = $(this);
                var cellValue = currentGridCell.children('input[type="hidden"]').val();
                var parentRow = currentGridCell.parent('tr');

                //Highlight rows that aren't fully reviewed
                if (cellValue === 'False') {
                    parentRow.addClass('not-fully-reviewed-row');
                }
                else {
                    parentRow.removeClass('not-fully-reviewed-row');
                }
            });
        }

        function setViewOnlyVisibility() {
            //Hide controls if this is a view
            var isSLTAPView = $('[ID$="hfSLTAPViewOnly"]').val();
            if (isSLTAPView == 'True') {
                $('.sltap-hide-on-view').addClass('hidden');
            }
            else {
                $('.sltap-hide-on-view').removeClass('hidden');
            }

            var isBOQView = $('[ID$="hfBOQViewOnly"]').val();
            if (isBOQView == 'True') {
                $('.boq-hide-on-view').addClass('hidden');
            }
            else {
                $('.boq-hide-on-view').removeClass('hidden');
            }

            var isMemberView = $('[ID$="hfMemberViewOnly"]').val();
            if (isMemberView == 'True') {
                $('.member-hide-on-view').addClass('hidden');
            }
            else {
                $('.member-hide-on-view').removeClass('hidden');
            }

            var isAgencyView = $('[ID$="hfAgencyViewOnly"]').val();
            if (isAgencyView == 'True') {
                $('.agency-hide-on-view').addClass('hidden');
            }
            else {
                $('.agency-hide-on-view').removeClass('hidden');
            }

            var isWorkGroupView = $('[ID$="hfWorkGroupViewOnly"]').val();
            if (isWorkGroupView == 'True') {
                $('.work-group-hide-on-view').addClass('hidden');
            }
            else {
                $('.work-group-hide-on-view').removeClass('hidden');
            }
        }
    </script>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <asp:HiddenField ID="hfSLTAPViewOnly" runat="server" Value="False" />
    <asp:HiddenField ID="hfBOQViewOnly" runat="server" Value="False" />
    <asp:HiddenField ID="hfMemberViewOnly" runat="server" Value="False" />
    <asp:HiddenField ID="hfAgencyViewOnly" runat="server" Value="False" />
    <asp:HiddenField ID="hfWorkGroupViewOnly" runat="server" Value="False" />
    <asp:UpdatePanel ID="upDashboardMessaging" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <uc:Messaging ID="msgSys" runat="server" />
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="lbDeleteSLTActionPlan" />
            <asp:AsyncPostBackTrigger ControlID="lbDeleteBOQ" />
            <asp:AsyncPostBackTrigger ControlID="lbDeleteMember" />
            <asp:AsyncPostBackTrigger ControlID="lbDeleteAgency" />
            <asp:AsyncPostBackTrigger ControlID="lbDeleteWorkGroup" />
        </Triggers>
    </asp:UpdatePanel>
    <asp:HiddenField ID="hfDeleteSLTActionPlanPK" runat="server" Value="0" />
    <asp:HiddenField ID="hfDeleteBOQPK" runat="server" Value="0" />
    <asp:HiddenField ID="hfDeleteMemberPK" runat="server" Value="0" />
    <asp:HiddenField ID="hfDeleteAgencyPK" runat="server" Value="0" />
    <asp:HiddenField ID="hfDeleteWorkGroupPK" runat="server" Value="0" />
    <!-- SLT Action Plans -->
    <div id="divSLTActionPlan" runat="server" class="row">
        <div class="col-lg-12 col-xl-12">
            <div class="card bg-light">
                <div class="card-header">
                    State Leadership Team Action Plans
                    <div class="btn-group float-right">
                        <button type="button" class="btn btn-primary dropdown-toggle sltap-hide-on-view hidden" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                            <i class="fas fa-plus"></i>&nbsp;Add Action Plan
                        </button>
                        <div class="dropdown-menu dropdown-menu-right">
                            <a href="/Pages/SLTActionPlan.aspx?SLTActionPlanPK=0&action=Add" class="dropdown-item"><i class="fas fa-plus"></i>&nbsp;Add New Blank Action Plan</a>
                            <asp:LinkButton ID="lbPrefillSLTAP" runat="server" CssClass="dropdown-item" OnClick="lbPrefillSLTAP_Click"><i class="fas fa-plus"></i>&nbsp;Prefill New Action Plan</asp:LinkButton>
                        </div>
                    </div>
                    <button id="btnSLTAPHelp" type="button" class="btn btn-link sltap-hide-on-view p-0 mr-2 float-right"><i class="fas fa-question-circle"></i>&nbsp;Help</button>
                </div>
                <div class="card-body">
                    <asp:UpdatePanel runat="server" ID="upAllSLTActionPlans">
                        <ContentTemplate>
                            <label>All Action Plans</label>
                            <div class="alert alert-primary">
                                <p>This table contains all State Leadership Team Action Plans regardless of their start date.</p>
                                <p class="mb-0">Any action plans that were pre-filled and are not fully reviewed will be highlighted in yellow.</p>
                            </div>
                            <dx:BootstrapGridView ID="bsGRSLTActionPlans" runat="server" EnableCallBacks="false" EnableRowsCache="false"
                                KeyFieldName="SLTActionPlanPK" AutoGenerateColumns="false" DataSourceID="efSLTActionPlanDataSource">
                                <SettingsPager PageSize="15" />
                                <SettingsBootstrap Striped="true" />
                                <SettingsBehavior EnableRowHotTrack="true" />
                                <Settings ShowGroupPanel="false" />
                                <SettingsSearchPanel Visible="true" ShowApplyButton="true" />
                                <CssClasses IconHeaderSortUp="fas fa-long-arrow-alt-up" IconHeaderSortDown="fas fa-long-arrow-alt-down" IconShowAdaptiveDetailButton="fas fa-plus-circle" />
                                <SettingsAdaptivity AdaptivityMode="HideDataCells" AllowOnlyOneAdaptiveDetailExpanded="true" AdaptiveColumnPosition="Left"></SettingsAdaptivity>
                                <Columns>
                                    <dx:BootstrapGridViewDateColumn FieldName="ActionPlanStartDate" Caption="Start Date" HorizontalAlign="Left" SortIndex="0" SortOrder="Descending" PropertiesDateEdit-DisplayFormatString="MM/dd/yyyy" AdaptivePriority="0"></dx:BootstrapGridViewDateColumn>
                                    <dx:BootstrapGridViewDateColumn FieldName="ActionPlanEndDate" Caption="End Date" HorizontalAlign="Left" PropertiesDateEdit-DisplayFormatString="MM/dd/yyyy" AdaptivePriority="2"></dx:BootstrapGridViewDateColumn>
                                    <dx:BootstrapGridViewDataColumn FieldName="IsFullyReviewed" Caption="Fully Reviewed" AdaptivePriority="3">
                                        <DataItemTemplate>
                                            <i class='<%# (Convert.ToBoolean(Eval("IsFullyReviewed")) ? "fas fa-check green-text" : "fas fa-times red-text") %>'></i>
                                            <asp:HiddenField runat="server" Value='<%# (Convert.ToBoolean(Eval("IsFullyReviewed")) ? "True" : "False") %>' />
                                        </DataItemTemplate>
                                        <CssClasses DataCell="ap-fully-reviewed" />
                                    </dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewDataColumn FieldName="SLTMember.IDNumber" Caption="Work Group Lead" AdaptivePriority="3">
                                        <DataItemTemplate>
                                            <%# string.Format("({0}) {1} {2}", Eval("SLTMember.IDNumber"), Eval("SLTMember.FirstName"), Eval("SLTMember.LastName")) %>
                                        </DataItemTemplate>
                                    </dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewDataColumn FieldName="SLTWorkGroup.WorkGroupName" Caption="Work Group" AdaptivePriority="4"></dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewDataColumn Name="StateNameColumn" FieldName="State.Name" Caption="State" AdaptivePriority="5"></dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewButtonEditColumn Settings-AllowDragDrop="False" AdaptivePriority="1" CssClasses-DataCell="text-center">
                                        <DataItemTemplate>
                                            <div class="btn-group">
                                                <button type="button" class="btn btn-secondary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                    Actions
                                                </button>
                                                <div class="dropdown-menu dropdown-menu-right">
                                                    <a class="dropdown-item" href='<%# string.Format("/Pages/SLTActionPlan.aspx?SLTActionPlanPK={0}&action={1}", Eval("SLTActionPlanPK").ToString(), "View") %>'><i class="fas fa-list"></i>&nbsp;View Details</a>
                                                    <a class="dropdown-item sltap-hide-on-view" href='<%#string.Format("/Pages/SLTActionPlan.aspx?SLTActionPlanPK={0}&action={1}", Eval("SLTActionPlanPK").ToString(), "Edit") %>'><i class="fas fa-edit"></i>&nbsp;Edit</a>
                                                    <button class="dropdown-item delete-gridview sltap-hide-on-view" data-pk='<%# Eval("SLTActionPlanPK") %>' data-hf="hfDeleteSLTActionPlanPK" data-target="#divDeleteSLTActionPlanModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                </div>
                                            </div>
                                        </DataItemTemplate>
                                    </dx:BootstrapGridViewButtonEditColumn>
                                </Columns>
                            </dx:BootstrapGridView>
                            <dx:EntityServerModeDataSource ID="efSLTActionPlanDataSource" runat="server" OnSelecting="efSLTActionPlanDataSource_Selecting" />
                        </ContentTemplate>
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="lbDeleteSLTActionPlan" />
                        </Triggers>
                    </asp:UpdatePanel>
                </div>
            </div>
        </div>
    </div>
    <!-- BOQ SLT Forms -->
    <div id="divBOQ" runat="server" class="row">
        <div class="col-lg-12 col-xl-12">
            <div class="card bg-light">
                <div class="card-header">
                    State Leadership Team Benchmarks of Quality Forms
                    <a href="/Pages/BOQSLT.aspx?BOQSLTPK=0&action=Add" class="btn btn-loader btn-primary float-right boq-hide-on-view hidden"><i class="fas fa-plus"></i>&nbsp;Add New BOQ</a>
                </div>
                <div class="card-body">
                    <asp:UpdatePanel ID="upAllBOQs" runat="server">
                        <ContentTemplate>
                            <label>All State Leadership Team Benchmarks of Quality Forms</label>
                            <div class="alert alert-primary">
                                This table contains all the Benchmarks of Quality forms regardless of when they were performed.
                            </div>
                            <dx:BootstrapGridView ID="bsGRBOQs" runat="server" EnableCallBacks="false" EnableRowsCache="true" 
                                KeyFieldName="BenchmarkOfQualitySLTPK"
                                AutoGenerateColumns="false" DataSourceID="efBOQDataSource">
                                <SettingsPager PageSize="15" />
                                <SettingsBootstrap Striped="true" />
                                <SettingsBehavior EnableRowHotTrack="true" />
                                <Settings ShowGroupPanel="false" />
                                <SettingsSearchPanel Visible="true" ShowApplyButton="true" />
                                <CssClasses IconHeaderSortUp="fas fa-long-arrow-alt-up" IconHeaderSortDown="fas fa-long-arrow-alt-down" IconShowAdaptiveDetailButton="fas fa-plus-circle" />
                                <SettingsAdaptivity AdaptivityMode="HideDataCells" AllowOnlyOneAdaptiveDetailExpanded="true" AdaptiveColumnPosition="Left"></SettingsAdaptivity>
                                <Columns>
                                    <dx:BootstrapGridViewDateColumn FieldName="FormDate" SortIndex="0" SortOrder="Descending" PropertiesDateEdit-DisplayFormatString="MM/dd/yyyy" AdaptivePriority="0"></dx:BootstrapGridViewDateColumn>
                                    <dx:BootstrapGridViewDataColumn FieldName="TeamMembers" Caption="Team Members" AdaptivePriority="4"></dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewDataColumn Name="StateNameColumn" FieldName="StateName" Caption="State" AdaptivePriority="5"></dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewButtonEditColumn Settings-AllowDragDrop="False" AdaptivePriority="1" CssClasses-DataCell="text-center">
                                        <DataItemTemplate>
                                            <div class="btn-group">
                                                <button type="button" class="btn btn-secondary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                    Actions 
                                                </button>
                                                <div class="dropdown-menu dropdown-menu-right">
                                                    <a class="dropdown-item" href='<%# string.Format("/Pages/BOQSLT.aspx?BOQSLTPK={0}&action={1}", Eval("BenchmarkOfQualitySLTPK").ToString(), "View") %>'><i class="fas fa-list"></i>&nbsp;View Details</a>
                                                    <a class="dropdown-item boq-hide-on-view" href='<%#string.Format("/Pages/BOQSLT.aspx?BOQSLTPK={0}&action={1}", Eval("BenchmarkOfQualitySLTPK").ToString(), "Edit") %>'><i class="fas fa-edit"></i>&nbsp;Edit</a>
                                                    <button class="dropdown-item delete-gridview boq-hide-on-view" data-pk='<%# Eval("BenchmarkOfQualitySLTPK") %>' data-hf="hfDeleteBOQPK" data-target="#divDeleteBOQModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                </div>
                                            </div>
                                        </DataItemTemplate>
                                    </dx:BootstrapGridViewButtonEditColumn>
                                </Columns>
                            </dx:BootstrapGridView>
                            <dx:EntityServerModeDataSource ID="efBOQDataSource" runat="server" OnSelecting="efBOQDataSource_Selecting" />
                        </ContentTemplate>
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="lbDeleteBOQ" />
                        </Triggers>
                    </asp:UpdatePanel>
                </div>
            </div>
        </div>
    </div>
    <!-- SLT Members -->
    <div id="divMembers" runat="server" class="row">
        <div class="col-lg-12 col-xl-12">
            <div class="card bg-light">
                <div class="card-header">
                    State Leadership Team Members
                    <a href="/Pages/SLTMember.aspx?SLTMemberPK=0&action=Add" class="btn btn-loader btn-primary float-right member-hide-on-view hidden"><i class="fas fa-plus"></i>&nbsp;Add New Member</a>
                </div>
                <div class="card-body">
                    <asp:UpdatePanel ID="upAllMembers" runat="server">
                        <ContentTemplate>
                            <label>All State Leadership Team Members</label>
                            <dx:BootstrapGridView ID="bsGRMembers" runat="server" EnableCallBacks="false" EnableRowsCache="true" 
                                KeyFieldName="SLTMemberPK"
                                AutoGenerateColumns="false" DataSourceID="efMemberDataSource">
                                <SettingsPager PageSize="15" />
                                <SettingsBootstrap Striped="true" />
                                <SettingsBehavior EnableRowHotTrack="true" />
                                <Settings ShowGroupPanel="false" />
                                <SettingsSearchPanel Visible="true" ShowApplyButton="true" />
                                <CssClasses IconHeaderSortUp="fas fa-long-arrow-alt-up" IconHeaderSortDown="fas fa-long-arrow-alt-down" IconShowAdaptiveDetailButton="fas fa-plus-circle" />
                                <SettingsAdaptivity AdaptivityMode="HideDataCells" AllowOnlyOneAdaptiveDetailExpanded="true" AdaptiveColumnPosition="Left"></SettingsAdaptivity>
                                <Columns>
                                    <dx:BootstrapGridViewDateColumn FieldName="IDNumber" Caption="ID" SortIndex="0" SortOrder="Ascending" AdaptivePriority="0"></dx:BootstrapGridViewDateColumn>
                                    <dx:BootstrapGridViewDataColumn FieldName="FirstName" Caption="First Name" AdaptivePriority="2"></dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewDataColumn FieldName="LastName" Caption="Last Name" AdaptivePriority="3"></dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewDateColumn FieldName="StartDate" Caption="Start Date" PropertiesDateEdit-DisplayFormatString="MM/dd/yyyy" AdaptivePriority="4"></dx:BootstrapGridViewDateColumn>
                                    <dx:BootstrapGridViewDataColumn FieldName="EmailAddress" Caption="Email Address" AdaptivePriority="5"></dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewDateColumn FieldName="LeaveDate" Caption="Leave Date" PropertiesDateEdit-DisplayFormatString="MM/dd/yyyy" AdaptivePriority="6"></dx:BootstrapGridViewDateColumn>
                                    <dx:BootstrapGridViewDataColumn FieldName="State.Name" Caption="State" AdaptivePriority="7"></dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewButtonEditColumn Settings-AllowDragDrop="False" AdaptivePriority="1" CssClasses-DataCell="text-center">
                                        <DataItemTemplate>
                                            <div class="btn-group">
                                                <button type="button" class="btn btn-secondary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                    Actions
                                                </button>
                                                <div class="dropdown-menu dropdown-menu-right">
                                                    <a class="dropdown-item" href='<%# string.Format("/Pages/SLTMember.aspx?SLTMemberPK={0}&action={1}", Eval("SLTMemberPK").ToString(), "View") %>'><i class="fas fa-list"></i>&nbsp;View Details</a>
                                                    <a class="dropdown-item member-hide-on-view" href='<%#string.Format("/Pages/SLTMember.aspx?SLTMemberPK={0}&action={1}", Eval("SLTMemberPK").ToString(), "Edit") %>'><i class="fas fa-edit"></i>&nbsp;Edit</a>
                                                    <button class="dropdown-item delete-gridview member-hide-on-view" data-pk='<%# Eval("SLTMemberPK") %>' data-hf="hfDeleteMemberPK" data-target="#divDeleteMemberModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                </div>
                                            </div>
                                        </DataItemTemplate>
                                    </dx:BootstrapGridViewButtonEditColumn>
                                </Columns>
                            </dx:BootstrapGridView>
                            <dx:EntityServerModeDataSource ID="efMemberDataSource" runat="server" OnSelecting="efMemberDataSource_Selecting" />
                        </ContentTemplate>
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="lbDeleteMember" />
                        </Triggers>
                    </asp:UpdatePanel>
                </div>
            </div>
        </div>
    </div>
    <!-- SLT Agencies -->
    <div id="divAgencies" runat="server" class="row">
        <div class="col-lg-12 col-xl-12">
            <div class="card bg-light">
                <div class="card-header">
                    State Leadership Team Agencies
                    <a href="/Pages/SLTAgency.aspx?SLTAgencyPK=0&action=Add" class="btn btn-loader btn-primary float-right agency-hide-on-view hidden"><i class="fas fa-plus"></i>&nbsp;Add New Agency</a>
                </div>
                <div class="card-body">
                    <asp:UpdatePanel ID="upAllAgencies" runat="server">
                        <ContentTemplate>
                            <label>All State Leadership Team Agencies</label>
                            <dx:BootstrapGridView ID="bsGRAgencies" runat="server" EnableCallBacks="false" EnableRowsCache="true" 
                                KeyFieldName="SLTAgencyPK"
                                AutoGenerateColumns="false" DataSourceID="efAgencyDataSource">
                                <SettingsPager PageSize="15" />
                                <SettingsBootstrap Striped="true" />
                                <SettingsBehavior EnableRowHotTrack="true" />
                                <Settings ShowGroupPanel="false" />
                                <SettingsSearchPanel Visible="true" ShowApplyButton="true" />
                                <CssClasses IconHeaderSortUp="fas fa-long-arrow-alt-up" IconHeaderSortDown="fas fa-long-arrow-alt-down" IconShowAdaptiveDetailButton="fas fa-plus-circle" />
                                <SettingsAdaptivity AdaptivityMode="HideDataCells" AllowOnlyOneAdaptiveDetailExpanded="true" AdaptiveColumnPosition="Left"></SettingsAdaptivity>
                                <Columns>
                                    <dx:BootstrapGridViewDateColumn FieldName="Name" Caption="Name" SortIndex="0" SortOrder="Ascending" AdaptivePriority="0"></dx:BootstrapGridViewDateColumn>
                                    <dx:BootstrapGridViewDataColumn FieldName="Website" Caption="Website" AdaptivePriority="3"></dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewDataColumn FieldName="State.Name" Caption="State" AdaptivePriority="5"></dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewButtonEditColumn Settings-AllowDragDrop="False" AdaptivePriority="1" CssClasses-DataCell="text-center">
                                        <DataItemTemplate>
                                            <div class="btn-group">
                                                <button type="button" class="btn btn-secondary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                    Actions
                                                </button>
                                                <div class="dropdown-menu dropdown-menu-right">
                                                    <a class="dropdown-item" href='<%# string.Format("/Pages/SLTAgency.aspx?SLTAgencyPK={0}&action={1}", Eval("SLTAgencyPK").ToString(), "View") %>'><i class="fas fa-list"></i>&nbsp;View Details</a>
                                                    <a class="dropdown-item agency-hide-on-view" href='<%#string.Format("/Pages/SLTAgency.aspx?SLTAgencyPK={0}&action={1}", Eval("SLTAgencyPK").ToString(), "Edit") %>'><i class="fas fa-edit"></i>&nbsp;Edit</a>
                                                    <button class="dropdown-item delete-gridview agency-hide-on-view" data-pk='<%# Eval("SLTAgencyPK") %>' data-hf="hfDeleteAgencyPK" data-target="#divDeleteAgencyModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                </div>
                                            </div>
                                        </DataItemTemplate>
                                    </dx:BootstrapGridViewButtonEditColumn>
                                </Columns>
                            </dx:BootstrapGridView>
                            <dx:EntityServerModeDataSource ID="efAgencyDataSource" runat="server" OnSelecting="efAgencyDataSource_Selecting" />
                        </ContentTemplate>
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="lbDeleteAgency" />
                        </Triggers>
                    </asp:UpdatePanel>
                </div>
            </div>
        </div>
    </div>
    <!-- SLT Work Groups -->
    <div id="divWorkGroups" runat="server" class="row">
        <div class="col-lg-12 col-xl-12">
            <div class="card bg-light">
                <div class="card-header">
                    State Leadership Team Work Groups
                    <a href="/Pages/SLTWorkGroup.aspx?SLTWorkGroupPK=0&action=Add" class="btn btn-loader btn-primary float-right work-group-hide-on-view hidden"><i class="fas fa-plus"></i>&nbsp;Add New Work Group</a>
                </div>
                <div class="card-body">
                    <asp:UpdatePanel ID="upAllWorkGroups" runat="server">
                        <ContentTemplate>
                            <label>All State Leadership Team Work Groups</label>
                            <dx:BootstrapGridView ID="bsGRWorkGroups" runat="server" EnableCallBacks="false" EnableRowsCache="true" 
                                KeyFieldName="SLTWorkGroupPK"
                                AutoGenerateColumns="false" DataSourceID="efWorkGroupDataSource">
                                <SettingsPager PageSize="15" />
                                <SettingsBootstrap Striped="true" />
                                <SettingsBehavior EnableRowHotTrack="true" />
                                <Settings ShowGroupPanel="false" />
                                <SettingsSearchPanel Visible="true" ShowApplyButton="true" />
                                <CssClasses IconHeaderSortUp="fas fa-long-arrow-alt-up" IconHeaderSortDown="fas fa-long-arrow-alt-down" IconShowAdaptiveDetailButton="fas fa-plus-circle" />
                                <SettingsAdaptivity AdaptivityMode="HideDataCells" AllowOnlyOneAdaptiveDetailExpanded="true" AdaptiveColumnPosition="Left"></SettingsAdaptivity>
                                <Columns>
                                    <dx:BootstrapGridViewDateColumn FieldName="WorkGroupName" Caption="Name" SortIndex="0" SortOrder="Ascending" AdaptivePriority="0"></dx:BootstrapGridViewDateColumn>
                                    <dx:BootstrapGridViewDateColumn FieldName="StartDate" Caption="Start Date" PropertiesDateEdit-DisplayFormatString="MM/dd/yyyy" AdaptivePriority="3"></dx:BootstrapGridViewDateColumn>
                                    <dx:BootstrapGridViewDateColumn FieldName="EndDate" Caption="End Date" PropertiesDateEdit-DisplayFormatString="MM/dd/yyyy" AdaptivePriority="4"></dx:BootstrapGridViewDateColumn>
                                    <dx:BootstrapGridViewDataColumn FieldName="State.Name" Caption="State" AdaptivePriority="2"></dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewButtonEditColumn Settings-AllowDragDrop="False" AdaptivePriority="1" CssClasses-DataCell="text-center">
                                        <DataItemTemplate>
                                            <div class="btn-group">
                                                <button type="button" class="btn btn-secondary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                    Actions
                                                </button>
                                                <div class="dropdown-menu dropdown-menu-right">
                                                    <a class="dropdown-item" href='<%# string.Format("/Pages/SLTWorkGroup.aspx?SLTWorkGroupPK={0}&action={1}", Eval("SLTWorkGroupPK").ToString(), "View") %>'><i class="fas fa-list"></i>&nbsp;View Details</a>
                                                    <a class="dropdown-item work-group-hide-on-view" href='<%#string.Format("/Pages/SLTWorkGroup.aspx?SLTWorkGroupPK={0}&action={1}", Eval("SLTWorkGroupPK").ToString(), "Edit") %>'><i class="fas fa-edit"></i>&nbsp;Edit</a>
                                                    <button class="dropdown-item delete-gridview work-group-hide-on-view" data-pk='<%# Eval("SLTWorkGroupPK") %>' data-hf="hfDeleteWorkGroupPK" data-target="#divDeleteWorkGroupModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                </div>
                                            </div>
                                        </DataItemTemplate>
                                    </dx:BootstrapGridViewButtonEditColumn>
                                </Columns>
                            </dx:BootstrapGridView>
                            <dx:EntityServerModeDataSource ID="efWorkGroupDataSource" runat="server" OnSelecting="efWorkGroupDataSource_Selecting" />
                        </ContentTemplate>
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="lbDeleteWorkGroup" />
                        </Triggers>
                    </asp:UpdatePanel>
                </div>
            </div>
        </div>
    </div>
    <!-- Modals -->
    <div class="modal" id="divDeleteSLTActionPlanModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete State Leadership Team Action Plan</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this State Leadership Team Action Plan?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteSLTActionPlan" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteSLTActionPlanModal" OnClick="lbDeleteSLTActionPlan_Click"><i class="fas fa-check"></i>&nbsp;Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
    <div class="modal" id="divDeleteBOQModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete Benchmarks of Quality Form</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this State Leadership Team Benchmarks of Quality form?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteBOQ" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteBOQModal" OnClick="lbDeleteBOQ_Click"><i class="fas fa-check"></i>&nbsp;Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
    <div class="modal" id="divDeleteMemberModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete SLT Member</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this State Leadership Team Member?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteMember" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteMemberModal" OnClick="lbDeleteMember_Click"><i class="fas fa-check"></i>&nbsp;Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
    <div class="modal" id="divDeleteAgencyModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete SLT Agency</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this State Leadership Team Agency?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteAgency" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteAgencyModal" OnClick="lbDeleteAgency_Click"><i class="fas fa-check"></i>&nbsp;Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
    <div class="modal" id="divDeleteWorkGroupModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete SLT Work Group</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this State Leadership Team Work Group?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteWorkGroup" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteWorkGroupModal" OnClick="lbDeleteWorkGroup_Click"><i class="fas fa-check"></i>&nbsp;Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
