<%@ Page Title="Program Leadership Team Dashboard" Language="C#" MasterPageFile="~/MasterPages/Dashboard.master" AutoEventWireup="true" CodeBehind="PLTDashboard.aspx.cs" Inherits="Pyramid.Pages.PLTDashboard" %>

<%@ Register Assembly="DevExpress.Web.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Data.Linq" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.Bootstrap.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>
<%@ Register TagPrefix="uc" TagName="Messaging" Src="~/User_Controls/MessagingSystem.ascx" %>
<%@ Register TagPrefix="uc" TagName="Submit" Src="~/User_Controls/Submit.ascx" %>

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
            $('[ID$="lnkPLTDashboard"]').addClass('active');

            //Show/hide the view only fields
            setViewOnlyVisibility();

            //Set up the events for the help buttons
            $('#btnPAPHelp').popover({
                trigger: 'hover focus',
                title: 'Help',
                html: true,
                content: 'The Add New Blank Action Plan button will allow you to create a new action plan without any information being pre-filled.<br/><br/>' +
                    'The Prefill New Action Plan button will create a new action plan and fill it with all the information ' +
                    'from the most recent action plan for your program. You will then be able to review and modify the new action plan.'                    
            });
            $('#btnPAPFCCHelp').popover({
                trigger: 'hover focus',
                title: 'Help',
                html: true,
                content: 'The Add New Blank Action Plan button will allow you to create a new action plan without any information being pre-filled.<br/><br/>' +
                    'The Prefill New Action Plan button will create a new action plan and fill it with all the information ' +
                    'from the most recent action plan for your program. You will then be able to review and modify the new action plan.'
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
            var isPAPView = $('[ID$="hfPAPViewOnly"]').val();
            if (isPAPView == 'True') {
                $('.pap-hide-on-view').addClass('hidden');
            }
            else {
                $('.pap-hide-on-view').removeClass('hidden');
            }

            var isPAPFCCView = $('[ID$="hfPAPFCCViewOnly"]').val();
            if (isPAPFCCView == 'True') {
                $('.papfcc-hide-on-view').addClass('hidden');
            }
            else {
                $('.papfcc-hide-on-view').removeClass('hidden');
            }

            var isMemberView = $('[ID$="hfMemberViewOnly"]').val();
            if (isMemberView == 'True') {
                $('.member-hide-on-view').addClass('hidden');
            }
            else {
                $('.member-hide-on-view').removeClass('hidden');
            }

            var isProgramAddressView = $('[ID$="hfProgramAddressViewOnly"]').val();
            if (isProgramAddressView == 'True') {
                $('.program-address-hide-on-view').addClass('hidden');
            }
            else {
                $('.program-address-hide-on-view').removeClass('hidden');
            }
        }
    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <asp:HiddenField ID="hfPAPViewOnly" runat="server" Value="False" />
    <asp:HiddenField ID="hfPAPFCCViewOnly" runat="server" Value="False" />
    <asp:HiddenField ID="hfMemberViewOnly" runat="server" Value="False" />
    <asp:HiddenField ID="hfProgramAddressViewOnly" runat="server" Value="False" />
    <asp:UpdatePanel ID="upDashboardMessaging" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <uc:Messaging ID="msgSys" runat="server" />
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="lbDeleteProgramActionPlan" />
            <asp:AsyncPostBackTrigger ControlID="lbDeleteProgramActionPlanFCC" />
            <asp:AsyncPostBackTrigger ControlID="lbDeleteMember" />
            <asp:AsyncPostBackTrigger ControlID="lbDeleteProgramAddress" />
        </Triggers>
    </asp:UpdatePanel>
    <asp:HiddenField ID="hfDeleteProgramActionPlanPK" runat="server" Value="0" />
    <asp:HiddenField ID="hfDeleteProgramActionPlanFCCPK" runat="server" Value="0" />
    <asp:HiddenField ID="hfDeleteMemberPK" runat="server" Value="0" />
    <asp:HiddenField ID="hfDeleteProgramAddressPK" runat="server" Value="0" />
    <!-- Regular Action Plans -->
    <div id="divActionPlan" runat="server" class="row">
        <div class="col-lg-12 col-xl-12">
            <div class="card bg-light">
                <div class="card-header">
                    Action Plans
                    <div class="btn-group float-right">
                        <button type="button" class="btn btn-primary dropdown-toggle pap-hide-on-view hidden" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                            <i class="fas fa-plus"></i>&nbsp;Add Action Plan
                        </button>
                        <div class="dropdown-menu dropdown-menu-right">
                            <a href="/Pages/ProgramActionPlan.aspx?ProgramActionPlanPK=0&action=Add" class="dropdown-item"><i class="fas fa-plus"></i>&nbsp;Add New Blank Action Plan</a>
                            <asp:LinkButton ID="lbPrefillPAP" runat="server" CssClass="dropdown-item" OnClick="lbPrefillPAP_Click"><i class="fas fa-plus"></i>&nbsp;Prefill New Action Plan</asp:LinkButton>
                        </div>
                    </div>
                    <button id="btnPAPHelp" type="button" class="btn btn-link pap-hide-on-view p-0 mr-2 float-right"><i class="fas fa-question-circle"></i>&nbsp;Help</button>
                </div>
                <div class="card-body">
                    <asp:UpdatePanel runat="server" ID="upAllProgramActionPlans">
                        <ContentTemplate>
                            <label>All Action Plans</label>
                            <div class="alert alert-primary">
                                <p>This table contains all Action Plans regardless of their start date.</p>
                                <p class="mb-0">Any action plans that were pre-filled and are not fully reviewed will be highlighted in yellow.</p>
                            </div>
                            <dx:BootstrapGridView ID="bsGRProgramActionPlans" runat="server" EnableCallBacks="false" 
                                KeyFieldName="ProgramActionPlanPK" OnHtmlRowCreated="bsGRProgramActionPlan_HtmlRowCreated"
                                AutoGenerateColumns="false" DataSourceID="efProgramActionPlanDataSource">
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
                                    <dx:BootstrapGridViewDataColumn FieldName="Program.ProgramName" Caption="Program Name" AdaptivePriority="3"></dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewDateColumn FieldName="Program.ProgramStartDate" Caption="Program Start Date" PropertiesDateEdit-DisplayFormatString="MM/dd/yyyy" AdaptivePriority="4"></dx:BootstrapGridViewDateColumn>
                                    <dx:BootstrapGridViewDataColumn FieldName="Program.Cohort.CohortName" Caption="Cohort" AdaptivePriority="5"></dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewDataColumn Name="LeadershipCoachColumn" FieldName="LeadershipCoachUsername" Caption="Leadership Coach (username)" AdaptivePriority="6">
                                        <DataItemTemplate>
                                            <asp:Label ID="lblLeadershipCoachUsername" runat="server" ></asp:Label>
                                        </DataItemTemplate>
                                    </dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewDataColumn Name="StateNameColumn" FieldName="Program.State.Name" Caption="State" AdaptivePriority="7"></dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewButtonEditColumn Settings-AllowDragDrop="False" AdaptivePriority="1" CssClasses-DataCell="text-center">
                                        <DataItemTemplate>
                                            <div class="btn-group">
                                                <button type="button" class="btn btn-secondary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                    Actions
                                                </button>
                                                <div class="dropdown-menu dropdown-menu-right">
                                                    <a class="dropdown-item" href='<%# string.Format("/Pages/ProgramActionPlan.aspx?ProgramActionPlanPK={0}&action={1}", Eval("ProgramActionPlanPK").ToString(), "View") %>'><i class="fas fa-list"></i>&nbsp;View Details</a>
                                                    <a class="dropdown-item pap-hide-on-view" href='<%# string.Format("/Pages/ProgramActionPlan.aspx?ProgramActionPlanPK={0}&action={1}", Eval("ProgramActionPlanPK").ToString(), "Edit") %>'><i class="fas fa-edit"></i>&nbsp;Edit</a>
                                                    <button class="dropdown-item delete-gridview pap-hide-on-view" data-pk='<%# Eval("ProgramActionPlanPK") %>' data-hf="hfDeleteProgramActionPlanPK" data-target="#divDeleteProgramActionPlanModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                </div>
                                            </div>
                                        </DataItemTemplate>
                                    </dx:BootstrapGridViewButtonEditColumn>
                                </Columns>
                            </dx:BootstrapGridView>
                            <dx:EntityServerModeDataSource ID="efProgramActionPlanDataSource" runat="server" OnSelecting="efProgramActionPlanDataSource_Selecting" />
                        </ContentTemplate>
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="lbDeleteProgramActionPlan" />
                        </Triggers>
                    </asp:UpdatePanel>
                </div>
            </div>
        </div>
    </div>
    <!-- FCC Action Plans -->
    <div id="divActionPlanFCC" runat="server" class="row">
        <div class="col-lg-12 col-xl-12">
            <div class="card bg-light">
                <div class="card-header">
                    Family Child Care Action Plans
                    <div class="btn-group float-right">
                        <button type="button" class="btn btn-primary dropdown-toggle papfcc-hide-on-view hidden" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                            <i class="fas fa-plus"></i>&nbsp;Add Action Plan
                        </button>
                        <div class="dropdown-menu dropdown-menu-right">
                            <a href="/Pages/ProgramActionPlanFCC.aspx?ProgramActionPlanFCCPK=0&action=Add" class="dropdown-item"><i class="fas fa-plus"></i>&nbsp;Add New Blank Action Plan</a>
                            <asp:LinkButton ID="lbPrefillPAPFCC" runat="server" CssClass="dropdown-item" OnClick="lbPrefillPAPFCC_Click"><i class="fas fa-plus"></i>&nbsp;Prefill New Action Plan</asp:LinkButton>
                        </div>
                    </div>
                    <button id="btnPAPFCCHelp" type="button" class="btn btn-link papfcc-hide-on-view p-0 mr-2 float-right"><i class="fas fa-question-circle"></i>&nbsp;Help</button>
                </div>
                <div class="card-body">
                    <asp:UpdatePanel runat="server" ID="upAllProgramActionPlanFCCs">
                        <ContentTemplate>
                            <label>All Family Child Care Action Plans</label>
                            <div class="alert alert-primary">
                                <p>This table contains all Family Child Care Action Plans regardless of their start date.</p>
                                <p class="mb-0">Any action plans that were pre-filled and are not fully reviewed will be highlighted in yellow.</p>
                            </div>
                            <dx:BootstrapGridView ID="bsGRProgramActionPlanFCCs" runat="server" EnableCallBacks="false" 
                                KeyFieldName="ProgramActionPlanFCCPK" OnHtmlRowCreated="bsGRProgramActionPlanFCC_HtmlRowCreated"
                                AutoGenerateColumns="false" DataSourceID="efProgramActionPlanFCCDataSource">
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
                                    <dx:BootstrapGridViewDataColumn FieldName="Program.ProgramName" Caption="Program Name" AdaptivePriority="3"></dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewDateColumn FieldName="Program.ProgramStartDate" Caption="Program Start Date" PropertiesDateEdit-DisplayFormatString="MM/dd/yyyy" AdaptivePriority="4"></dx:BootstrapGridViewDateColumn>
                                    <dx:BootstrapGridViewDataColumn FieldName="Program.Cohort.CohortName" Caption="Cohort" AdaptivePriority="5"></dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewDataColumn Name="LeadershipCoachColumn" FieldName="LeadershipCoachUsername" Caption="Leadership Coach (username)" AdaptivePriority="6">
                                        <DataItemTemplate>
                                            <asp:Label ID="lblLeadershipCoachUsername" runat="server" ></asp:Label>
                                        </DataItemTemplate>
                                    </dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewDataColumn Name="StateNameColumn" FieldName="Program.State.Name" Caption="State" AdaptivePriority="7"></dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewButtonEditColumn Settings-AllowDragDrop="False" AdaptivePriority="1" CssClasses-DataCell="text-center">
                                        <DataItemTemplate>
                                            <div class="btn-group">
                                                <button type="button" class="btn btn-secondary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                    Actions
                                                </button>
                                                <div class="dropdown-menu dropdown-menu-right">
                                                    <a class="dropdown-item" href='<%# string.Format("/Pages/ProgramActionPlanFCC.aspx?ProgramActionPlanFCCPK={0}&action={1}", Eval("ProgramActionPlanFCCPK").ToString(), "View") %>'><i class="fas fa-list"></i>&nbsp;View Details</a>
                                                    <a class="dropdown-item papfcc-hide-on-view" href='<%#string.Format("/Pages/ProgramActionPlanFCC.aspx?ProgramActionPlanFCCPK={0}&action={1}", Eval("ProgramActionPlanFCCPK").ToString(), "Edit") %>'><i class="fas fa-edit"></i>&nbsp;Edit</a>
                                                    <button class="dropdown-item delete-gridview papfcc-hide-on-view" data-pk='<%# Eval("ProgramActionPlanFCCPK") %>' data-hf="hfDeleteProgramActionPlanFCCPK" data-target="#divDeleteProgramActionPlanFCCModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                </div>
                                            </div>
                                        </DataItemTemplate>
                                    </dx:BootstrapGridViewButtonEditColumn>
                                </Columns>
                            </dx:BootstrapGridView>
                            <dx:EntityServerModeDataSource ID="efProgramActionPlanFCCDataSource" runat="server" OnSelecting="efProgramActionPlanFCCDataSource_Selecting" />
                        </ContentTemplate>
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="lbDeleteProgramActionPlanFCC" />
                        </Triggers>
                    </asp:UpdatePanel>
                </div>
            </div>
        </div>
    </div>
    <!-- PLT Members -->
    <div id="divMembers" runat="server" class="row">
        <div class="col-lg-12 col-xl-12">
            <div class="card bg-light">
                <div class="card-header">
                    Program Leadership Team Members
                    <a href="/Pages/PLTMember.aspx?PLTMemberPK=0&action=Add" class="btn btn-loader btn-primary float-right member-hide-on-view hidden"><i class="fas fa-plus"></i>&nbsp;Add New Member</a>
                </div>
                <div class="card-body">
                    <asp:UpdatePanel ID="upAllMembers" runat="server">
                        <ContentTemplate>
                            <label>All Program Leadership Team Members</label>
                            <dx:BootstrapGridView ID="bsGRMembers" runat="server" EnableCallBacks="false" EnableRowsCache="true" 
                                KeyFieldName="PLTMemberPK"
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
                                    <dx:BootstrapGridViewDataColumn FieldName="Program.ProgramName" Caption="Program" AdaptivePriority="7"></dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewDataColumn Name="StateNameColumn" FieldName="Program.State.Name" Caption="State" AdaptivePriority="8"></dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewButtonEditColumn Settings-AllowDragDrop="False" AdaptivePriority="1" CssClasses-DataCell="text-center">
                                        <DataItemTemplate>
                                            <div class="btn-group">
                                                <button type="button" class="btn btn-secondary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                    Actions
                                                </button>
                                                <div class="dropdown-menu dropdown-menu-right">
                                                    <a class="dropdown-item" href='<%# string.Format("/Pages/PLTMember.aspx?PLTMemberPK={0}&action={1}", Eval("PLTMemberPK").ToString(), "View") %>'><i class="fas fa-list"></i>&nbsp;View Details</a>
                                                    <a class="dropdown-item member-hide-on-view" href='<%#string.Format("/Pages/PLTMember.aspx?PLTMemberPK={0}&action={1}", Eval("PLTMemberPK").ToString(), "Edit") %>'><i class="fas fa-edit"></i>&nbsp;Edit</a>
                                                    <button class="dropdown-item delete-gridview member-hide-on-view" data-pk='<%# Eval("PLTMemberPK") %>' data-hf="hfDeleteMemberPK" data-target="#divDeleteMemberModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
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
    <!-- Program Addresses -->
    <div id="divProgramAddress" runat="server" class="row">
        <div class="col-lg-12 col-xl-12">
            <div class="card bg-light">
                <div class="card-header">
                    Program Addresses
                    <a href="/Pages/ProgramAddress.aspx?ProgramAddressPK=0&action=Add" class="btn btn-loader btn-primary float-right program-address-hide-on-view hidden"><i class="fas fa-plus"></i>&nbsp;Add New Address</a>
                </div>
                <div class="card-body">
                    <asp:UpdatePanel ID="upAllAddresses" runat="server">
                        <ContentTemplate>
                            <label>All Program Addresses</label>
                            <dx:BootstrapGridView ID="bsGRProgramAddresses" runat="server" EnableCallBacks="false" EnableRowsCache="true" 
                                KeyFieldName="ProgramAddressPK"
                                AutoGenerateColumns="false" DataSourceID="efProgramAddressDataSource">
                                <SettingsPager PageSize="15" />
                                <SettingsBootstrap Striped="true" />
                                <SettingsBehavior EnableRowHotTrack="true" />
                                <Settings ShowGroupPanel="false" />
                                <SettingsSearchPanel Visible="true" ShowApplyButton="true" />
                                <CssClasses IconHeaderSortUp="fas fa-long-arrow-alt-up" IconHeaderSortDown="fas fa-long-arrow-alt-down" IconShowAdaptiveDetailButton="fas fa-plus-circle" />
                                <SettingsAdaptivity AdaptivityMode="HideDataCells" AllowOnlyOneAdaptiveDetailExpanded="true" AdaptiveColumnPosition="left"></SettingsAdaptivity>
                                <Columns>
                                    <dx:BootstrapGridViewDataColumn FieldName="Street" Caption="Street" AdaptivePriority="0"></dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewDateColumn FieldName="City" Caption="City" SortIndex="0" SortOrder="Ascending" AdaptivePriority="2"></dx:BootstrapGridViewDateColumn>
                                    <dx:BootstrapGridViewDataColumn FieldName="State" Caption="State" AdaptivePriority="2"></dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewDataColumn FieldName="ZIPCode" Caption="ZIP Code" AdaptivePriority="2"></dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewDataColumn FieldName="LicenseNumber" Caption="License Number" AdaptivePriority="2"></dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewDataColumn FieldName="IsMailingAddress" Caption="Is this the Program's Mailing Address?" Width="50px" AdaptivePriority="3">
                                        <DataItemTemplate>
                                            <%# (Convert.ToBoolean(Eval("IsMailingAddress")) ? "Yes" : "No") %>
                                        </DataItemTemplate>
                                    </dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewDateColumn FieldName="Notes" Caption="Notes" AdaptivePriority="4"></dx:BootstrapGridViewDateColumn>
                                    <dx:BootstrapGridViewDataColumn FieldName="Program.ProgramName" Caption="Program" AdaptivePriority="3"></dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewButtonEditColumn Settings-AllowDragDrop="False" AdaptivePriority="1" CssClasses-DataCell="text-center">
                                        <DataItemTemplate>
                                            <div class="btn-group">
                                                <button type="button" class="btn btn-secondary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                    Actions
                                                </button>
                                                <div class="dropdown-menu dropdown-menu-right">
                                                    <a class="dropdown-item" href='<%# string.Format("/Pages/ProgramAddress.aspx?ProgramAddressPK={0}&action={1}", Eval("ProgramAddressPK").ToString(), "View") %>'><i class="fas fa-list"></i>&nbsp;View Details</a>
                                                    <a class="dropdown-item program-address-hide-on-view" href='<%#string.Format("/Pages/ProgramAddress.aspx?ProgramAddressPK={0}&action={1}", Eval("ProgramAddressPK").ToString(), "Edit") %>'><i class="fas fa-edit"></i>&nbsp;Edit</a>
                                                    <button class="dropdown-item delete-gridview program-address-hide-on-view" data-pk='<%# Eval("ProgramAddressPK") %>' data-hf="hfDeleteProgramAddressPK" data-target="#divDeleteProgramAddressModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                </div>
                                            </div>
                                        </DataItemTemplate>
                                    </dx:BootstrapGridViewButtonEditColumn>
                                </Columns>
                            </dx:BootstrapGridView>
                            <dx:EntityServerModeDataSource ID="efProgramAddressDataSource" runat="server" OnSelecting="efProgramAddressDataSource_Selecting" />
                        </ContentTemplate>
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="lbDeleteProgramAddress" />
                        </Triggers>
                    </asp:UpdatePanel>
                </div>
            </div>
        </div>
    </div>
    <!-- Modals -->
    <div class="modal" id="divDeleteProgramActionPlanModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete Action Plan</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this Action Plan?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteProgramActionPlan" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteProgramActionPlanModal" OnClick="lbDeleteProgramActionPlan_Click"><i class="fas fa-check"></i>&nbsp;Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
    <div class="modal" id="divDeleteProgramActionPlanFCCModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete Family Child Care Action Plan</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this Family Child Care Action Plan?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteProgramActionPlanFCC" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteProgramActionPlanFCCModal" OnClick="lbDeleteProgramActionPlanFCC_Click"><i class="fas fa-check"></i>&nbsp;Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
    <div class="modal" id="divDeleteMemberModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete Leadership Team Member</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this Program Leadership Team Member?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteMember" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteMemberModal" OnClick="lbDeleteMember_Click"><i class="fas fa-check"></i>&nbsp;Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
    <div class="modal" id="divDeleteProgramAddressModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete Program Address</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this Program Address?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteProgramAddress" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteProgramAddressModal" OnClick="lbDeleteProgramAddress_Click"><i class="fas fa-check"></i>&nbsp;Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
</asp:Content>