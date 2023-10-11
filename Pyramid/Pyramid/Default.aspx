<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/MasterPages/Dashboard.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Pyramid._Default" %>

<%@ Register TagPrefix="uc" TagName="Messaging" Src="~/User_Controls/MessagingSystem.ascx" %>
<%@ Register Assembly="DevExpress.Web.Bootstrap.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>

<asp:Content ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .news-div {
            position: fixed;
            bottom: 10px;
            left: 10px;
            z-index: 999;
            max-width: 25%;
            min-width: 250px;
        }

        .news-div-content {
            max-height: 450px;
            overflow: auto;
        }

        @media (max-height: 600px) {
            .news-div-content {
                max-height: 350px;
            }
        }
    </style>
</asp:Content>
<asp:Content ContentPlaceHolderID="ScriptContent" runat="server">
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
            $('#lnkHomeDashboard').addClass('active');

            //Allow date format for DataTables sorting
            $.fn.dataTable.moment('MM/DD/YYYY');

            //Initialize the datatables
            if (!$.fn.dataTable.isDataTable('#tblInvalidForms')) {
                $('#tblInvalidForms').DataTable({
                    responsive: {
                        details: {
                            type: 'column'
                        }
                    },
                    columnDefs: [
                        { orderable: false, targets: [5] },
                        { className: 'control', orderable: false, targets: 0 }
                    ],
                    order: [[2, 'desc']],
                    stateSave: true,
                    stateDuration: 60,
                    pageLength: 5,
                    dom: 'frtp',
                    language: {
                        searchPlaceholder: "Enter text to search...",
                        search: "",
                    }
                });
            }
            $('.dataTables_filter input').removeClass('form-control-sm');

            //Create event listener for the invalid forms collapse shown method
            $('#collapseInvalidForms').off('shown.bs.collapse').on('shown.bs.collapse', function (e) {
                //Readjust the datatable
                $('#tblInvalidForms').DataTable()
                    .columns.adjust()
                    .responsive.recalc();
            });

            //When the user hides the news, store the date it was hidden in a cookie
            $('#btnHideNews').off('click').on('click', function (e) {
                //Prevent postback
                e.preventDefault();
                e.stopPropagation();

                //Get date the news was hidden
                var date = new Date();
                var dateString = ((date.getMonth() + 1) + '/' + date.getDate() + '/' + date.getFullYear());

                //Set the news cookie and hide the news
                Cookies.set('newsHidden', dateString, { expires: 365, sameSite: 'lax' });
                $('#divNews').slideUp(400, "linear", function () {
                    $('#divNewsButton').fadeIn();
                });
            });

            //When the user shows the news, remove the hidden cookie and show the news
            $('#btnShowNews').off('click').on('click', function (e) {
                //Prevent postback
                e.preventDefault();
                e.stopPropagation();

                //Remove the news cookie
                if (Cookies.get('newsHidden') !== undefined) {
                    Cookies.remove('newsHidden');
                }

                //Show the news
                $("#divNews").slideDown();
                $("#divNewsButton").hide();
            });

            if (Cookies.get('newsHidden') !== undefined) {
                //Get the date of the most recent news
                var mostRecentNewsDateText = $("#divNews .news-entry-date").first().text();
                var mostRecentNewsDate = new Date(mostRecentNewsDateText);

                //Get the date the news was hidden
                var comparisonDate = new Date(Cookies.get('newsHidden'));

                //If the last news date doesn't exist or it is before, or on the hide date, don't show the news
                if (!mostRecentNewsDateText || mostRecentNewsDate <= comparisonDate) {
                    $("#divNewsButton").show();
                }
                else {
                    $("#divNewsButton").hide();
                    $("#divNews").slideDown();
                }
            }
            else {
                $("#divNews").slideDown();
                $("#divNewsButton").hide();
            }

            //Get the forms due information
            var monthsOverdue = $('[ID$="hfFormsDueMonthsOverdue"]').val();
            var monthsUpcoming = $('[ID$="hfFormsDueMonthsUpcoming"]').val();
            var formsDueDaysUntilWarning = $('[ID$="hfFormsDueDaysUntilWarning"]').val();

            //Set the form due widget help text
            $('#btnFormsDueHelp').popover({
                trigger: 'hover focus',
                title: 'Help',
                html: true,
                content: '<p>The forms listed here are either due in the next ' + monthsUpcoming +
                    ' month(s) or are up to ' + monthsOverdue + ' month(s) past their due date. ' +
                    'Each form must have a date within the specified date range in order to count as completed. ' +
                    'All forms in this list are color coded by their due date status, with the color meanings listed here:</p>' +
                    '<p>Blue = This form is due over ' + formsDueDaysUntilWarning + ' days from now.</p >' +
                    '<p>Yellow = This form is due between now and ' + formsDueDaysUntilWarning + ' days from now.</p>' +
                    '<p>Red = This form is overdue.  Please contact your program\'s data collector to submit the data for the indicated timeframe.</p>' +
                    '<p>Green = This form is completed.</p>'
            });

            //Get the dashboard loaded hidden field
            var hfDashboardLoaded = $('[ID$="hfDashboardLoaded"]');

            //If the dashboard is not loaded yet
            if (hfDashboardLoaded.val() == 'false') {
                //Click the refresh dashboard button
                $('[ID$="btnRefreshDashboard"]').click();

                //Note that the dashboard is loaded
                hfDashboardLoaded.val('true');
            }
            else {
                $('#divContent').removeClass('hidden');
                $('#divContentLoading').addClass('hidden');
            }
        }
    </script>
</asp:Content>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <asp:HiddenField ID="hfDashboardLoaded" runat="server" Value="false" />
    <asp:UpdatePanel ID="upMessaging" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <uc:Messaging ID="msgSys" runat="server" />
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="repeatInvalidForms" />
            <asp:AsyncPostBackTrigger ControlID="btnRefreshInvalidForms" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="btnRefreshDashboard" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="btnRefreshFormsDue" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="bsGRFormsDue" />
        </Triggers>
    </asp:UpdatePanel>
    <div id="divContentLoading" class="text-center w-100 mt-5 font-size-1-3">
        <span class="spinner-border text-primary" role="status"></span>&nbsp;
        <span>Loading...</span>
    </div>
    <div id="divContent" class="hidden">
        <!-- Welcome Section -->
        <asp:UpdatePanel ID="upWelcomeSection" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div id="divAccountUpdateAlert" runat="server" class="alert alert-warning">
                    <div>
                        <i class="fas fa-exclamation-circle"></i>&nbsp;
                        You haven't updated your account information in over 6 months, please click the button below to review and/or update your account information. 
                    </div>
                    <div class="mt-2">
                        <a class="btn btn-secondary" href="/Account/Manage.aspx"><i class="fas fa-clipboard-check"></i>&nbsp;Review/Update Account Information</a>
                    </div>
                </div>
                <div id="divWelcomeSection" runat="server">
                    <div id="divFireworks" runat="server">
                        <div class="before"></div>
                        <div class="after"></div>
                    </div>
                    <div class="row">
                        <div class="col-sm-12 col-md-12">
                            <div class="text-center mt-2">
                                <h3>Welcome to the Pyramid Model Implementation Data System!</h3>
                                <dx:BootstrapImage ID="bsImgLargeThumbnailLogo" runat="server" AlternateText="Large Thumbnail Logo" CssClasses-Control="mt-3 mr-2 large-logo" ImageUrl="/Content/images/CustomPIDSLogoSquare.png" Width="35%"></dx:BootstrapImage>
                                <dx:BootstrapImage ID="bsImgLargeLogo" runat="server" AlternateText="Large Site Logo" CssClasses-Control="mt-3 ml-2 large-logo" ImageUrl="/Content/images/GenericLogoSquare.png" Width="35%"></dx:BootstrapImage>
                                <div class="mt-4">
                                    <asp:LinkButton ID="lbEnableFireworks" runat="server" CssClass="btn btn-loader btn-primary" OnClick="lbEnableFireworks_Click"><i class="fas fa-toggle-off"></i> Enable Fireworks</asp:LinkButton>
                                    <asp:LinkButton ID="lbDisableFireworks" runat="server" CssClass="btn btn-loader btn-primary" OnClick="lbDisableFireworks_Click"><i class="fas fa-toggle-on"></i> Disable Fireworks</asp:LinkButton>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="btnRefreshDashboard" EventName="Click" />
            </Triggers>
        </asp:UpdatePanel>
        <!-- Dashboard sections -->
        <asp:UpdatePanel ID="upDashboard" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
            <ContentTemplate>
                <asp:Button ID="btnRefreshDashboard" runat="server" Text="Refresh Dashboard" OnClick="btnRefreshDashboard_Click" CssClass="hidden" />
                <div id="divInvalidForms" runat="server" class="row">
                    <div class="col-sm-12 col-md-10">
                        <div class="card bg-transparent border-warning">
                            <div class="collapse-module">
                                <div class="card-header bg-warning">
                                    <div class="row">
                                        <div class="col-md-8">
                                            <a id="lnkCollapseInvalidForms" class="collapsed collapse-module-button" data-toggle="collapse" href="#collapseInvalidForms" aria-expanded="false" aria-controls="collapseInvalidForms">
                                                <i class="collapse-module-icon"></i>&nbsp;Invalid Forms
                                            </a>
                                        </div>
                                        <div id="divInvalidFormsRefreshButton" style="display: none;" class="col-md-4 collapse-module-toggle-section">
                                            <dx:BootstrapButton ID="btnRefreshInvalidForms" runat="server" Text="Refresh" OnClick="btnRefreshInvalidForms_Click"
                                                SettingsBootstrap-RenderOption="secondary">
                                                <CssClasses Icon="fas fa-sync-alt" Control="float-right btn-loader" />
                                            </dx:BootstrapButton>
                                        </div>
                                    </div>
                                </div>
                                <div id="collapseInvalidForms" class="collapse" aria-labelledby="lnkCollapseInvalidForms">
                                    <div class="card-body">
                                        <div class="row">
                                            <div class="col-md-12">
                                                <div class="alert alert-warning">
                                                    <i class="fas fa-exclamation-triangle"></i>&nbsp;
                                                    <asp:Label ID="lblInvalidFormsWarning" runat="server"></asp:Label>
                                                </div>
                                            </div>
                                        </div>
                                        <asp:UpdatePanel ID="upInvalidForms" runat="server" UpdateMode="Conditional">
                                            <ContentTemplate>
                                                <asp:Repeater ID="repeatInvalidForms" runat="server" ItemType="Pyramid.Models.rspInvalidForms_Result"
                                                    OnItemDataBound="repeatInvalidForms_ItemDataBound">
                                                    <HeaderTemplate>
                                                        <table id="tblInvalidForms" class="table table-striped table-bordered table-hover">
                                                            <thead>
                                                                <tr>
                                                                    <th data-priority="1"></th>
                                                                    <th data-priority="2">Form Name</th>
                                                                    <th data-priority="4">Form Date</th>
                                                                    <th data-priority="5">Validation Failure</th>
                                                                    <th>Failure Explanation</th>
                                                                    <th data-priority="3"></th>
                                                                </tr>
                                                            </thead>
                                                            <tbody>
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <tr>
                                                            <td></td>
                                                            <td><%# Item.ObjectName %></td>
                                                            <td><%# (Item.ObjectDate.HasValue ? Item.ObjectDate.Value.ToString("MM/dd/yyyy") : "") %></td>
                                                            <td><%# Item.InvalidReason %></td>
                                                            <td><%# Item.InvalidExplanation %></td>
                                                            <td class="text-center">
                                                                <div id="divActionDropdown" runat="server" class="btn-group">
                                                                    <button type="button" class="btn btn-secondary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                                        Actions
                                                                    </button>
                                                                    <div class="dropdown-menu dropdown-menu-right">
                                                                        <asp:HyperLink ID="lnkViewInvalidForm" runat="server" CssClass="dropdown-item" Target="_blank"><i class="fas fa-list"></i>&nbsp;View Form</asp:HyperLink>
                                                                        <asp:HyperLink ID="lnkEditInvalidForm" runat="server" CssClass="dropdown-item" Target="_blank"><i class="fas fa-edit"></i>&nbsp;Edit Form</asp:HyperLink>
                                                                        <div id="divDropdownDivider" runat="server" class="dropdown-divider"></div>
                                                                        <asp:HyperLink ID="lnkViewInvalidEmployee" runat="server" CssClass="dropdown-item" Target="_blank"><i class="fas fa-list"></i>&nbsp;View Pyramid Model Professional</asp:HyperLink>
                                                                        <asp:HyperLink ID="lnkEditInvalidEmployee" runat="server" CssClass="dropdown-item" Target="_blank"><i class="fas fa-edit"></i>&nbsp;Edit Pyramid Model Professional</asp:HyperLink>
                                                                    </div>
                                                                </div>
                                                            </td>
                                                        </tr>
                                                    </ItemTemplate>
                                                    <FooterTemplate>
                                                        </tbody>
                                                        </table>
                                                    </FooterTemplate>
                                                </asp:Repeater>
                                            </ContentTemplate>
                                            <Triggers>
                                                <asp:AsyncPostBackTrigger ControlID="btnRefreshInvalidForms" EventName="Click" />
                                            </Triggers>
                                        </asp:UpdatePanel>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <div id="divFormsDue" runat="server" class="row">
                    <asp:HiddenField ID="hfFormsDueMonthsOverdue" runat="server" Value="0" />
                    <asp:HiddenField ID="hfFormsDueMonthsUpcoming" runat="server" Value="0" />
                    <asp:HiddenField ID="hfFormsDueDaysUntilWarning" runat="server" Value="0" />
                    <div class="col-md-10">
                        <div class="card bg-light">
                            <div class="collapse-module">
                                <div class="card-header">
                                    <div class="row">
                                        <div class="col-md-8">
                                            <a id="lnkCollapseFormsDue" class="collapse-module-button" data-toggle="collapse" href="#collapseFormsDue" aria-expanded="true" aria-controls="collapseFormsDue">
                                                <i class="collapse-module-icon"></i>&nbsp;Forms Due
                                            </a>
                                        </div>
                                        <div class="col-md-4 collapse-module-toggle-section">
                                            <div class="float-right">
                                                <button id="btnFormsDueHelp" type="button" class="btn btn-outline-primary mr-2"><i class="fas fa-question-circle"></i>&nbsp;Help</button>
                                                <dx:BootstrapButton ID="btnRefreshFormsDue" runat="server" Text="Refresh" OnClick="btnRefreshFormsDue_Click"
                                                    SettingsBootstrap-RenderOption="secondary">
                                                    <CssClasses Icon="fas fa-sync-alt" Control="btn-loader" />
                                                </dx:BootstrapButton>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                <div id="collapseFormsDue" class="collapse show" aria-labelledby="lnkCollapseFormsDue">
                                    <div class="card-body">
                                        <asp:UpdatePanel ID="upFormsDue" runat="server" UpdateMode="Conditional">
                                            <ContentTemplate>
                                                <dx:BootstrapGridView ID="bsGRFormsDue" runat="server" EnableCallBacks="false" EnableRowsCache="true"
                                                    AutoGenerateColumns="false" DataSourceID="sqlFormsDueDataSource" OnHtmlRowCreated="bsGRFormsDue_HtmlRowCreated"
                                                    SettingsText-EmptyDataRow="No forms due...">
                                                    <SettingsPager PageSize="15" />
                                                    <SettingsBootstrap Striped="false" />
                                                    <SettingsBehavior EnableRowHotTrack="true" AutoExpandAllGroups="true" />
                                                    <SettingsSearchPanel Visible="true" ShowApplyButton="true" />
                                                    <Settings ShowGroupPanel="true" />
                                                    <CssClasses IconHeaderSortUp="fas fa-long-arrow-alt-up" IconHeaderSortDown="fas fa-long-arrow-alt-down" IconShowAdaptiveDetailButton="fas fa-plus-circle" />
                                                    <SettingsAdaptivity AdaptivityMode="HideDataCells" AllowOnlyOneAdaptiveDetailExpanded="true" AdaptiveColumnPosition="Left"></SettingsAdaptivity>
                                                    <Columns>
                                                        <dx:BootstrapGridViewDataColumn FieldName="FormAbbreviation" Caption="Form" AdaptivePriority="0" />
                                                        <dx:BootstrapGridViewDataColumn FieldName="DueDateDescription" Caption="Requirements" AdaptivePriority="3" />
                                                        <dx:BootstrapGridViewDataColumn FieldName="DueStartDate" Caption="Date Range" AdaptivePriority="4">
                                                            <DataItemTemplate>
                                                                <%# string.Format("{0:MM/dd/yyyy} - {1:MM/dd/yyyy}", Eval("DueStartDate"), Eval("DueEndDate")) %>
                                                            </DataItemTemplate>
                                                        </dx:BootstrapGridViewDataColumn>
                                                        <dx:BootstrapGridViewDateColumn FieldName="DueRecommendedDate" Caption="Due Date" SortIndex="0" SortOrder="Ascending" PropertiesDateEdit-DisplayFormatString="MM/dd/yyyy" AdaptivePriority="1" />
                                                        <dx:BootstrapGridViewDataColumn Name="CompletedFormColumn" FieldName="FormDate" Caption="Completed Form" Settings-AllowSort="False" Settings-AllowGroup="False" AdaptivePriority="5">
                                                            <DataItemTemplate>
                                                                <asp:Label ID="lblFormInfo" runat="server"></asp:Label>
                                                                <dx:BootstrapHyperLink ID="lnkCompletedForm" runat="server" Target="_blank" CssClasses-Control="btn btn-secondary" CssClasses-Icon="fas fa-list" Text="View"></dx:BootstrapHyperLink>
                                                            </DataItemTemplate>
                                                        </dx:BootstrapGridViewDataColumn>
                                                        <dx:BootstrapGridViewDataColumn Name="HelpColumn" Settings-AllowSort="False" Settings-AllowGroup="False" AdaptivePriority="1">
                                                            <DataItemTemplate>
                                                                <a tabindex="0" class="btn btn-outline-primary" role="button" data-toggle="popover" data-trigger="hover" data-content='<%# Eval("HelpText") %>'><i class="fas fa-question-circle"></i></a>
                                                            </DataItemTemplate>
                                                        </dx:BootstrapGridViewDataColumn>
                                                        <dx:BootstrapGridViewTextColumn FieldName="IsCompleteText" Caption="Completed" GroupIndex="0" AdaptivePriority="2" />
                                                    </Columns>
                                                </dx:BootstrapGridView>
                                                <asp:SqlDataSource ID="sqlFormsDueDataSource" runat="server" CancelSelectOnNullParameter="true"
                                                    SelectCommandType="StoredProcedure" SelectCommand="rspFormsDue">
                                                    <SelectParameters>
                                                        <asp:Parameter Name="ProgramFKs" Type="String" />
                                                        <asp:Parameter Name="StartDate" Type="DateTime" />
                                                        <asp:Parameter Name="EndDate" Type="DateTime" />
                                                    </SelectParameters>
                                                </asp:SqlDataSource>
                                            </ContentTemplate>
                                            <Triggers>
                                                <asp:AsyncPostBackTrigger ControlID="btnRefreshFormsDue" EventName="Click" />
                                                <asp:AsyncPostBackTrigger ControlID="btnRefreshDashboard" EventName="Click" />
                                            </Triggers>
                                        </asp:UpdatePanel>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="btnRefreshDashboard" EventName="Click" />
            </Triggers>
        </asp:UpdatePanel>
        <!-- News Section -->
        <div id="divNewsButton" class="row" style="display: none;">
            <div class="col-md-12">
                <button id="btnShowNews" class="btn btn-primary"><span class="fas fa-plus"></span>&nbsp;Show News</button>
            </div>
        </div>
    </div>
    <div id="divNews" class="news-div" style="display: none;">
        <div class="card">
            <div class="card-header">
                <i class="fas fa-newspaper"></i>&nbsp;News
                <a href="/Pages/News.aspx" class="btn btn-primary float-right"><i class="fas fa-newspaper"></i>&nbsp;All News</a>
            </div>
            <div class="card-body">
                <div class="news-div-content">
                    <asp:Literal ID="ltlNews" runat="server"></asp:Literal>
                </div>
                <hr />
                <div class="mt-2">
                    <button id="btnHideNews" class="btn btn-primary"><span class="fas fa-minus"></span>&nbsp;Hide News</button>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
