<%@ Page Title="Reports" Language="C#" MasterPageFile="~/MasterPages/LoggedIn.master" AutoEventWireup="true" CodeBehind="Reports.aspx.cs" Inherits="Pyramid.Reports.Reports" %>

<%@ Register Assembly="DevExpress.XtraReports.v19.1.Web.WebForms, Version=19.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.XtraReports.Web" TagPrefix="dx" %>
<%@ Register TagPrefix="uc" TagName="Messaging" Src="~/User_Controls/MessagingSystem.ascx" %>
<%@ Register Assembly="DevExpress.Web.v19.1, Version=19.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Data.Linq" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.Bootstrap.v19.1, Version=19.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ScriptContent" runat="server">
    <script>
        //For the animations below
        var enterAnimation = 'animated zoomIn';
        var exitAnimation = 'animated zoomOut';
        var afterAnimationEvents = 'webkitAnimationEnd mozAnimationEnd MSAnimationEnd oanimationend animationend';

        $(document).ready(function () {
            //Run the initial page init
            initializePage();

            //Run the page init after every AJAX load
            var requestManager = Sys.WebForms.PageRequestManager.getInstance();
            requestManager.add_endRequest(initializePage);
        });

        //Initializes the page
        function initializePage() {
            //Get the width of the report details
            var reportDivWidth = $('#divReports').width();

            //Set the width hidden field
            $('[ID$="hfReportDivWidth"]').val(reportDivWidth);
            
            //Hide loading button and show the run report button
            $('[ID$="btnRunReport"]').show();
            $('[ID$="btnReportLoading"]').hide();

            //Set the click event for the select report button
            $('.select-report-button').off('click').on('click', function (e) {
                //Prevent postback
                e.preventDefault();
                
                //Clear the criteria
                hideAndClearCriteria();

                //Get this button
                var thisButton = $(this);

                //Get the report catalog pk
                var reportCatalogPK = thisButton.siblings('[ID*="hfReportCatalogPK"]').val();
                var reportName = thisButton.siblings('[ID*="hfReportName"]').val();
                var reportClass = thisButton.siblings('[ID*="hfReportClass"]').val();
                var reportCriteriaOptions = thisButton.siblings('[ID*="hfCriteriaOptions"]').val();
                var reportOptionalCriteriaOptions = thisButton.siblings('[ID*="hfOptionalCriteriaOptions"]').val();
                var reportCriteriaDefaults = thisButton.siblings('[ID*="hfCriteriaDefaults"]').val();

                //Put the report to run information into hidden fields for the back-end
                $('[ID$="hfReportToRunPK"]').val(reportCatalogPK);
                $('[ID$="hfReportToRunClass"]').val(reportClass);
                $('[ID$="hfReportToRunName"]').val(reportName);
                $('[ID$="hfReportToRunCriteriaOptions"]').val(reportCriteriaOptions);
                $('[ID$="hfReportToRunOptionalCriteriaOptions"]').val(reportOptionalCriteriaOptions);
                $('[ID$="hfReportToRunCriteriaDefaults"]').val(reportCriteriaDefaults);

                //Check the criteria options
                if (reportCriteriaOptions.includes('None')) {
                    //Show the no criteria label
                    $('#divNoCriteria').removeClass('hidden');
                }
                else {
                    //Hide the no criteria label
                    $('#divNoCriteria').addClass('hidden');

                    //Show/hide criteria options
                    setCriteriaVisibility(reportCriteriaOptions);

                    //Set the criteria defaults
                    setCriteriaDefaults(reportCriteriaDefaults);
                }

                //Set the report title
                $('#h5ReportName').html(reportName);
            });

            //Set the click event for the return to list button
            $('#btnReturnToList').off('click').on('click', function (e) {
                //Prevent a postback
                e.preventDefault();

                //Animate the report list
                $('#divReportViewer').removeClass(enterAnimation).addClass(exitAnimation);

                //Wait 500 ms to hide the viewer and show the list
                setTimeout(function () {
                    $('#divReportViewer').hide();
                    $('#divReportGridview').show().removeClass(exitAnimation).addClass(enterAnimation);
                }, 500);

                //Animate the return to list button
                $('#btnReturnToList').removeClass(enterAnimation).addClass(exitAnimation);

                //Wait 500 ms to hide the return to list button
                setTimeout(function () {
                    $('#btnReturnToList').hide();
                }, 500);

                //Clear the hidden fields
                $('[ID$="hfReportToRunPK"]').val('');
                $('[ID$="hfReportToRunClass"]').val('');
                $('[ID$="hfReportToRunName"]').val('');
                $('[ID$="hfReportToRunCriteriaOptions"]').val('');
                $('[ID$="hfReportToRunOptionalCriteriaOptions"]').val('');
                $('[ID$="hfReportToRunCriteriaDefaults"]').val('');

                //Hide the no criteria label
                $('#divNoCriteria').addClass('hidden');

                //Clear the criteria
                hideAndClearCriteria();

                //Set the report title
                $('#h5ReportName').html('No report selected...');
            });
        }
    </script>
    <script type="text/javascript" src="/Scripts/reports-page.js"></script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <asp:UpdatePanel ID="upMessaging" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <uc:Messaging ID="msgSys" runat="server" />
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="btnRunReport" EventName="Click" />
        </Triggers>
    </asp:UpdatePanel>
    <asp:HiddenField ID="hfReportDivWidth" runat="server" Value="" />
    <asp:HiddenField ID="hfCurrentProgramFK" runat="server" Value="" />
    <div id="divReports" class="row">
        <div class="col-lg-3">
            <div class="card bg-light">
                <div class="card-header">
                    Report Details
                </div>
                <div class="card-body">
                    <div id="divReportTitle" class="text-center">
                        <h5 id="h5ReportName">No Report Selected...</h5>
                        <hr />
                    </div>
                    <div class="text-center">
                        <asp:HiddenField ID="hfReportToRunPK" runat="server" Value="" />
                        <asp:HiddenField ID="hfReportToRunClass" runat="server" Value="" />
                        <asp:HiddenField ID="hfReportToRunName" runat="server" Value="" />
                        <asp:HiddenField ID="hfReportToRunCriteriaOptions" runat="server" Value="" />
                        <asp:HiddenField ID="hfReportToRunOptionalCriteriaOptions" runat="server" Value="" />
                        <asp:HiddenField ID="hfReportToRunCriteriaDefaults" runat="server" Value="" />
                        <button id="btnReturnToList" class="btn btn-primary mr-2 mb-2" style="display: none;"><i class="fas fa-arrow-left"></i>&nbsp;Return to Report List</button>
                        <dx:BootstrapButton ID="btnRunReport" runat="server" Text="Run Report"
                            OnClick="btnRunReport_Click" AutoPostBack="true" ValidationGroup="vgCriteria"
                            SettingsBootstrap-RenderOption="primary">
                            <CssClasses Icon="fas fa-play" Control="mb-2" />
                            <ClientSideEvents Click="btnRunReportClick" />
                        </dx:BootstrapButton>
                        <button id="btnReportLoading" class="mb-2 btn btn-primary dxbs-button" style="display: none;">
                            <span class="spinner-border spinner-border-sm"></span>&nbsp;Loading...
                        </button>
                    </div>
                    <div id="divNoCriteria" class="text-center hidden">
                        <label>No criteria for this report!</label>
                    </div>
                    <div id="divStartEndDates" class="criteria-div hidden">
                        <dx:BootstrapDateEdit ID="deStartDate" runat="server" Caption="Start Date" EditFormat="Date"
                            EditFormatString="MM/dd/yyyy" UseMaskBehavior="true" NullText="--Select--"
                            ClientInstanceName="deStartDate" OnValidation="deStartDate_Validation"
                            AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900">
                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                            <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                            <ClientSideEvents Validation="validateStartDate" />
                            <ValidationSettings ValidationGroup="vgCriteria" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                <RequiredField IsRequired="false" ErrorText="Start Date is required!" />
                            </ValidationSettings>
                        </dx:BootstrapDateEdit>
                        <dx:BootstrapDateEdit ID="deEndDate" runat="server" Caption="End Date" EditFormat="Date"
                            EditFormatString="MM/dd/yyyy" UseMaskBehavior="true" NullText="--Select--"
                            ClientInstanceName="deEndDate" OnValidation="deEndDate_Validation"
                            AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900">
                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                            <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                            <ClientSideEvents Validation="validateEndDate" />
                            <ValidationSettings ValidationGroup="vgCriteria" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                <RequiredField IsRequired="false" ErrorText="End Date is required!" />
                            </ValidationSettings>
                        </dx:BootstrapDateEdit>
                    </div>
                    <div id="divPointInTimeDate" class="criteria-div hidden">
                        <dx:BootstrapDateEdit ID="dePointInTime" runat="server" Caption="Point In Time" EditFormat="Date"
                            EditFormatString="MM/dd/yyyy" UseMaskBehavior="true" NullText="--Select--"
                            ClientInstanceName="dePointInTime" OnValidation="dePointInTime_Validation"
                            AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900">
                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                            <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                            <ClientSideEvents Validation="validatePointInTime" />
                            <ValidationSettings ValidationGroup="vgCriteria" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                <RequiredField IsRequired="false" ErrorText="Point In Time is required!" />
                            </ValidationSettings>
                        </dx:BootstrapDateEdit>
                    </div>
                    <div id="divClassrooms" class="criteria-div hidden">
                        <dx:BootstrapListBox ID="lstBxClassroom" runat="server" Caption="Classroom(s)"
                            SelectionMode="CheckColumn" EnableSelectAll="true"
                            ValueField="ClassroomPK" ValueType="System.Int32" TextField="ClassroomName"
                            OnValidation="lstBxClassroom_Validation" ClientInstanceName="lstBxClassroom">
                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                            <ClientSideEvents Validation="validateClassroomList" />
                            <FilteringSettings ShowSearchUI="true" EditorNullTextDisplayMode="Unfocused" />
                            <ValidationSettings ValidationGroup="vgCriteria" ErrorDisplayMode="ImageWithText" CausesValidation="true">
                                <RequiredField IsRequired="false" ErrorText="At least one classroom must be selected!" />
                            </ValidationSettings>
                        </dx:BootstrapListBox>
                    </div>
                    <div id="divChildren" class="criteria-div hidden">
                        <dx:BootstrapListBox ID="lstBxChild" runat="server" Caption="Child(ren)"
                            SelectionMode="CheckColumn" EnableSelectAll="true"
                            ValueField="ChildPK" ValueType="System.Int32" TextField="ChildName"
                            OnValidation="lstBxChild_Validation" ClientInstanceName="lstBxChild">
                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                            <ClientSideEvents Validation="validateChildList" />
                            <FilteringSettings ShowSearchUI="true" EditorNullTextDisplayMode="Unfocused" />
                            <ValidationSettings ValidationGroup="vgCriteria" ErrorDisplayMode="ImageWithText" CausesValidation="true">
                                <RequiredField IsRequired="false" ErrorText="At least one child must be selected!" />
                            </ValidationSettings>
                        </dx:BootstrapListBox>
                    </div>
                    <div id="divChildDemographics" class="criteria-div hidden">
                        <dx:BootstrapListBox ID="lstBxRace" runat="server" Caption="Race(s)"
                            SelectionMode="CheckColumn" EnableSelectAll="true"
                            ValueField="CodeRacePK" ValueType="System.Int32" TextField="Description"
                            OnValidation="lstBxCD_Validation" ClientInstanceName="lstBxRace">
                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                            <ClientSideEvents Validation="validateCDLists" />
                            <FilteringSettings ShowSearchUI="true" EditorNullTextDisplayMode="Unfocused" />
                            <ValidationSettings ValidationGroup="vgCriteria" ErrorDisplayMode="ImageWithText" CausesValidation="true">
                                <RequiredField IsRequired="false" ErrorText="At least one race must be selected!" />
                            </ValidationSettings>
                        </dx:BootstrapListBox>
                        <dx:BootstrapListBox ID="lstBxEthnicity" runat="server" Caption="Ethnicity(ies)"
                            SelectionMode="CheckColumn" EnableSelectAll="true"
                            ValueField="CodeEthnicityPK" ValueType="System.Int32" TextField="Description"
                            OnValidation="lstBxCD_Validation" ClientInstanceName="lstBxEthnicity">
                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                            <ClientSideEvents Validation="validateCDLists" />
                            <FilteringSettings ShowSearchUI="true" EditorNullTextDisplayMode="Unfocused" />
                            <ValidationSettings ValidationGroup="vgCriteria" ErrorDisplayMode="ImageWithText" CausesValidation="true">
                                <RequiredField IsRequired="false" ErrorText="At least one ethnicity must be selected!" />
                            </ValidationSettings>
                        </dx:BootstrapListBox>
                        <dx:BootstrapListBox ID="lstBxGender" runat="server" Caption="Gender(s)"
                            SelectionMode="CheckColumn" EnableSelectAll="true"
                            ValueField="CodeGenderPK" ValueType="System.Int32" TextField="Description"
                            OnValidation="lstBxCD_Validation" ClientInstanceName="lstBxGender">
                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                            <ClientSideEvents Validation="validateCDLists" />
                            <FilteringSettings ShowSearchUI="true" EditorNullTextDisplayMode="Unfocused" />
                            <ValidationSettings ValidationGroup="vgCriteria" ErrorDisplayMode="ImageWithText" CausesValidation="true">
                                <RequiredField IsRequired="false" ErrorText="At least one gender must be selected!" />
                            </ValidationSettings>
                        </dx:BootstrapListBox>
                        <dx:BootstrapComboBox ID="ddIEP" runat="server" Caption="IEP Status" NullText="--Select--"
                            ValueType="System.Int32"
                            IncrementalFilteringMode="StartsWith" AllowMouseWheel="false"
                            OnValidation="lstBxCD_Validation" ClientInstanceName="ddIEP">
                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                            <ClientSideEvents Validation="validateCDLists" />
                            <ValidationSettings ValidationGroup="vgCriteria" ErrorDisplayMode="ImageWithText" CausesValidation="true">
                                <RequiredField IsRequired="false" ErrorText="IEP is required!" />
                            </ValidationSettings>
                            <Items>
                                <dx:BootstrapListEditItem Value="2" Text="All"></dx:BootstrapListEditItem>
                                <dx:BootstrapListEditItem Value="1" Text="Has IEP"></dx:BootstrapListEditItem>
                                <dx:BootstrapListEditItem Value="0" Text="No IEP"></dx:BootstrapListEditItem>
                            </Items>
                        </dx:BootstrapComboBox>
                        <dx:BootstrapComboBox ID="ddDLL" runat="server" Caption="DLL Status" NullText="--Select--"
                            ValueType="System.Int32"
                            IncrementalFilteringMode="StartsWith" AllowMouseWheel="false"
                            OnValidation="lstBxCD_Validation" ClientInstanceName="ddDLL">
                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                            <ClientSideEvents Validation="validateCDLists" />
                            <ValidationSettings ValidationGroup="vgCriteria" ErrorDisplayMode="ImageWithText" CausesValidation="true">
                                <RequiredField IsRequired="false" ErrorText="DLL is required!" />
                            </ValidationSettings>
                            <Items>
                                <dx:BootstrapListEditItem Value="2" Text="All"></dx:BootstrapListEditItem>
                                <dx:BootstrapListEditItem Value="1" Text="Is DLL"></dx:BootstrapListEditItem>
                                <dx:BootstrapListEditItem Value="0" Text="Not DLL"></dx:BootstrapListEditItem>
                            </Items>
                        </dx:BootstrapComboBox>
                    </div>
                    <div id="divEmployees" class="criteria-div hidden">
                        <dx:BootstrapListBox ID="lstBxEmployee" runat="server" Caption="Employee(s)"
                            SelectionMode="CheckColumn" EnableSelectAll="true"
                            ValueField="ProgramEmployeePK" ValueType="System.Int32" TextField="ProgramEmployeeName"
                            OnValidation="lstBxEmployee_Validation" ClientInstanceName="lstBxEmployee">
                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                            <ClientSideEvents Validation="validateEmployeeList" />
                            <FilteringSettings ShowSearchUI="true" EditorNullTextDisplayMode="Unfocused" />
                            <ValidationSettings ValidationGroup="vgCriteria" ErrorDisplayMode="ImageWithText" CausesValidation="true">
                                <RequiredField IsRequired="false" ErrorText="At least one employee must be selected!" />
                            </ValidationSettings>
                        </dx:BootstrapListBox>
                    </div>
                    <div id="divTeachers" class="criteria-div hidden">
                        <dx:BootstrapListBox ID="lstBxTeacher" runat="server" Caption="Teacher(s)"
                            SelectionMode="CheckColumn" EnableSelectAll="true"
                            ValueField="ProgramEmployeePK" ValueType="System.Int32" TextField="ProgramEmployeeName"
                            OnValidation="lstBxTeacher_Validation" ClientInstanceName="lstBxTeacher">
                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                            <ClientSideEvents Validation="validateTeacherList" />
                            <FilteringSettings ShowSearchUI="true" EditorNullTextDisplayMode="Unfocused" />
                            <ValidationSettings ValidationGroup="vgCriteria" ErrorDisplayMode="ImageWithText" CausesValidation="true">
                                <RequiredField IsRequired="false" ErrorText="At least one teacher must be selected!" />
                            </ValidationSettings>
                        </dx:BootstrapListBox>
                    </div>
                    <div id="divCoaches" class="criteria-div hidden">
                        <dx:BootstrapListBox ID="lstBxCoach" runat="server" Caption="Coach(es)"
                            SelectionMode="CheckColumn" EnableSelectAll="true"
                            ValueField="ProgramEmployeePK" ValueType="System.Int32" TextField="ProgramEmployeeName"
                            OnValidation="lstBxCoach_Validation" ClientInstanceName="lstBxCoach">
                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                            <ClientSideEvents Validation="validateCoachList" />
                            <FilteringSettings ShowSearchUI="true" EditorNullTextDisplayMode="Unfocused" />
                            <ValidationSettings ValidationGroup="vgCriteria" ErrorDisplayMode="ImageWithText" CausesValidation="true">
                                <RequiredField IsRequired="false" ErrorText="At least one coach must be selected!" />
                            </ValidationSettings>
                        </dx:BootstrapListBox>
                    </div>
                    <div id="divPHC" class="criteria-div hidden">
                        <dx:BootstrapListBox ID="lstBxProgram" runat="server" Caption="Program(s)"
                            SelectionMode="CheckColumn" EnableSelectAll="true"
                            ValueField="ProgramPK" ValueType="System.Int32" TextField="ProgramName"
                            OnValidation="lstBxPHC_Validation" ClientInstanceName="lstBxProgram">
                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                            <ClientSideEvents Validation="validatePHCLists" />
                            <FilteringSettings ShowSearchUI="true" EditorNullTextDisplayMode="Unfocused" />
                            <ValidationSettings ValidationGroup="vgCriteria" ErrorDisplayMode="ImageWithText" CausesValidation="true">
                                <RequiredField IsRequired="false" ErrorText="At least one Program must be selected!" />
                            </ValidationSettings>
                        </dx:BootstrapListBox>
                        <dx:BootstrapListBox ID="lstBxHub" runat="server" Caption="Hub(s)"
                            SelectionMode="CheckColumn" EnableSelectAll="true"
                            ValueField="HubPK" ValueType="System.Int32" TextField="Name"
                            OnValidation="lstBxPHC_Validation" ClientInstanceName="lstBxHub">
                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                            <ClientSideEvents Validation="validatePHCLists" />
                            <FilteringSettings ShowSearchUI="true" EditorNullTextDisplayMode="Unfocused" />
                            <ValidationSettings ValidationGroup="vgCriteria" ErrorDisplayMode="ImageWithText" CausesValidation="true">
                                <RequiredField IsRequired="false" ErrorText="At least one hub must be selected!" />
                            </ValidationSettings>
                        </dx:BootstrapListBox>
                        <dx:BootstrapListBox ID="lstBxCohort" runat="server" Caption="Cohort(s)"
                            SelectionMode="CheckColumn" EnableSelectAll="true"
                            ValueField="CohortPK" ValueType="System.Int32" TextField="CohortName"
                            OnValidation="lstBxPHC_Validation" ClientInstanceName="lstBxCohort">
                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                            <ClientSideEvents Validation="validatePHCLists" />
                            <FilteringSettings ShowSearchUI="true" EditorNullTextDisplayMode="Unfocused" />
                            <ValidationSettings ValidationGroup="vgCriteria" ErrorDisplayMode="ImageWithText" CausesValidation="true">
                                <RequiredField IsRequired="false" ErrorText="At least one cohort must be selected!" />
                            </ValidationSettings>
                        </dx:BootstrapListBox>
                    </div>
                </div>
            </div>
        </div>
        <div class="col-lg-9">
            <div id="divReportGridview">
                <div class="card bg-light">
                    <div class="card-header">
                        All Reports
                    </div>
                    <div class="card-body">
                        <asp:UpdatePanel ID="upReportsGrid" runat="server" UpdateMode="Conditional">
                            <ContentTemplate>
                                <dx:BootstrapGridView ID="bsGVReports" runat="server" EnableCallBacks="false" KeyFieldName="ReportCatalogPK"
                                    AutoGenerateColumns="false" DataSourceID="efReportDataSource">
                                    <SettingsPager PageSize="15" />
                                    <SettingsBootstrap Striped="true" />
                                    <SettingsBehavior EnableRowHotTrack="true" />
                                    <SettingsSearchPanel Visible="true" ShowApplyButton="true" />
                                    <Settings ShowGroupPanel="false" />
                                    <CssClasses IconHeaderSortUp="fas fa-long-arrow-alt-up" IconHeaderSortDown="fas fa-long-arrow-alt-down" IconShowAdaptiveDetailButton="fas fa-plus-circle" />
                                    <SettingsAdaptivity AdaptivityMode="HideDataCells" AllowOnlyOneAdaptiveDetailExpanded="true" AdaptiveColumnPosition="Left"></SettingsAdaptivity>
                                    <Columns>
                                        <dx:BootstrapGridViewDataColumn FieldName="ReportName" Caption="Report Name" SortIndex="0" SortOrder="Ascending" AdaptivePriority="0" />
                                        <dx:BootstrapGridViewDataColumn FieldName="ReportCategory" Caption="Category" AdaptivePriority="1" />
                                        <dx:BootstrapGridViewDataColumn FieldName="ReportDescription" Caption="Description" AdaptivePriority="2" Width="40%" />
                                        <dx:BootstrapGridViewDataColumn FieldName="Keywords" Caption="Keywords" AdaptivePriority="4"
                                            CssClasses-DataCell="hidden" CssClasses-FilterCell="hidden" CssClasses-FooterCell="hidden" 
                                            CssClasses-EditCell="hidden" CssClasses-HeaderCell="hidden" CssClasses-GroupFooterCell="hidden" />
                                        <dx:BootstrapGridViewDataColumn Settings-AllowDragDrop="False" AdaptivePriority="0" MaxWidth="100" CssClasses-DataCell="text-center">
                                            <DataItemTemplate>
                                                <button class="btn btn-primary select-report-button"><i class="fas fa-mouse-pointer"></i>&nbsp;Select</button>
                                                <asp:HiddenField ID="hfReportCatalogPK" runat="server" Value='<%# Eval("ReportCatalogPK") %>' />
                                                <asp:HiddenField ID="hfReportClass" runat="server" Value='<%# Eval("ReportClass") %>' />
                                                <asp:HiddenField ID="hfReportName" runat="server" Value='<%# Eval("ReportName") %>' />
                                                <asp:HiddenField ID="hfCriteriaOptions" runat="server" Value='<%# Eval("CriteriaOptions") %>' />
                                                <asp:HiddenField ID="hfOptionalCriteriaOptions" runat="server" Value='<%# Eval("OptionalCriteriaOptions") %>' />
                                                <asp:HiddenField ID="hfCriteriaDefaults" runat="server" Value='<%# Eval("CriteriaDefaults") %>' />
                                            </DataItemTemplate>
                                        </dx:BootstrapGridViewDataColumn>
                                        <dx:BootstrapGridViewDataColumn Settings-AllowDragDrop="False" AdaptivePriority="2" MaxWidth="160" CssClasses-DataCell="text-center">
                                            <DataItemTemplate>
                                                <a class="btn btn-primary" target="_blank" href="/Pages/ViewFile.aspx?ReportCatalogPK=<%# Eval("ReportCatalogPK") %>"><i class="fas fa-file-download"></i>&nbsp;Documentation</a>
                                            </DataItemTemplate>
                                        </dx:BootstrapGridViewDataColumn>
                                    </Columns>
                                </dx:BootstrapGridView>
                                <dx:EntityServerModeDataSource ID="efReportDataSource" runat="server"
                                    OnSelecting="efReportDataSource_Selecting" />
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>
                </div>
            </div>
            <div id="divReportViewer" style="display: none;">
                <asp:UpdatePanel ID="upReportListAndViewer" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>
                        <div class="card bg-light">
                            <div class="card-header">
                                Report Results
                            </div>
                            <div id="divReportContent" class="card-body">
                                <dx:ASPxWebDocumentViewer ID="reportViewer" runat="server"></dx:ASPxWebDocumentViewer>
                            </div>
                        </div>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="btnRunReport" EventName="Click" />
                    </Triggers>
                </asp:UpdatePanel>
            </div>
        </div>
    </div>
</asp:Content>
