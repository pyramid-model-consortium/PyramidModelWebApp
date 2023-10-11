<%@ Page Title="Reports" Language="C#" MasterPageFile="~/MasterPages/LoggedIn.master" AutoEventWireup="true" CodeBehind="Reports.aspx.cs" Inherits="Pyramid.Reports.Reports" %>

<%@ Register Assembly="DevExpress.XtraReports.v22.2.Web.WebForms, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.XtraReports.Web" TagPrefix="dx" %>
<%@ Register TagPrefix="uc" TagName="Messaging" Src="~/User_Controls/MessagingSystem.ascx" %>
<%@ Register Assembly="DevExpress.Web.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Data.Linq" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.Bootstrap.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>

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
            
            //Hide loading button
            $('[ID$="btnReportLoading"]').addClass('hidden');

            //Set the button visibility
            setReportButtonVisibility($('[ID$="hfReportToRunOnlyExportAllowed"]').val());

            //Get the running report value
            var runningReport = $('[ID$="hfRunningReport"]').val();

            //Check to see if the user is running the report
            if (runningReport == 'true') {
                //Get the report criteria options
                var reportCriteriaOptions = $('[ID*="hfReportToRunCriteriaOptions"]').val();

                //Show/hide criteria options
                setCriteriaVisibility(reportCriteriaOptions);
            }

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
                var reportOnlyExportAllowed = thisButton.siblings('[ID*="hfOnlyExportAllowed"]').val();
                var reportCriteriaOptions = thisButton.siblings('[ID*="hfCriteriaOptions"]').val();
                var reportOptionalCriteriaOptions = thisButton.siblings('[ID*="hfOptionalCriteriaOptions"]').val();
                var reportCriteriaDefaults = thisButton.siblings('[ID*="hfCriteriaDefaults"]').val();

                //Put the report to run information into hidden fields for the back-end
                $('[ID$="hfReportToRunPK"]').val(reportCatalogPK);
                $('[ID$="hfReportToRunClass"]').val(reportClass);
                $('[ID$="hfReportToRunName"]').val(reportName);
                $('[ID$="hfReportToRunOnlyExportAllowed"]').val(reportOnlyExportAllowed);
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

                //Set the button visibility
                setReportButtonVisibility(reportOnlyExportAllowed);

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
                $('[ID$="hfReportToRunOnlyExportAllowed"]').val('');
                $('[ID$="hfReportToRunCriteriaOptions"]').val('');
                $('[ID$="hfReportToRunOptionalCriteriaOptions"]').val('');
                $('[ID$="hfReportToRunCriteriaDefaults"]').val('');
                $('[ID$="hfRunningReport"]').val('');

                //Hide the no criteria label
                $('#divNoCriteria').addClass('hidden');

                //Clear the criteria
                hideAndClearCriteria();

                //Set the report title
                $('#h5ReportName').html('No report selected...');
            });
        }

        //This function sets the visibility of the run report and export report buttons
        function setReportButtonVisibility(isExportOnly) {
            //Get the buttons
            var runReportButton = $('[ID$="btnRunReport"]');
            var exportReportButton = $('[ID$="btnExportReport"]');

            //Set the button visibility
            if (isExportOnly === 'true') {
                //This is an export-only report, show the export button and hide the run button
                exportReportButton.removeClass('hidden');
                runReportButton.addClass('hidden');
            }
            else {
                //This is a normal report, show the run button and hide the export button
                runReportButton.removeClass('hidden');
                exportReportButton.addClass('hidden');
            }
        }

        //The change event for the report focus dropdown
        function ddReportFocus_SelectedIndexChanged() {
            //Set the report focus warning display
            setReportFocusWarningDisplay();
        }

        //The initialization event for the report focus dropdown
        function ddReportFocus_Init() {
            //Set the report focus warning display
            setReportFocusWarningDisplay();
        }

        //This function shows/hides the report focus warning
        function setReportFocusWarningDisplay() {
            //Get the selected report focus
            var selectedValue = ddReportFocus.GetValue();

            //Check to see which focus was selected
            if (selectedValue === 'CHI') {
                //Show the warning
                $('#divReportFocusWarning').removeClass('hidden').html('<i class="fas fa-exclamation-circle"></i>&nbsp;Because you selected the Children report focus, the report may hide certain information or ignore certain criteria.  For details, please check the report documentation or the criteria section on the last page of the report.');
            }
            else if (selectedValue === 'CR') {
                //Show the warning
                $('#divReportFocusWarning').removeClass('hidden').html('<i class="fas fa-exclamation-circle"></i>&nbsp;Because you selected the Classrooms report focus, the report may hide certain information or ignore certain criteria.  For details, please check the report documentation or the criteria section on the last page of the report.');
            }
            else {
                //Hide the warning
                $('#divReportFocusWarning').addClass('hidden').html('');
            }
        }

        //This function fires when the report viewer initializes
        function reportViewerInit(s, e) {
            //Get the report div size
            var reportDivWidth = $('#divReports').width();

            //If the report div is over 1000px wide, set zoom to 100%
            if (reportDivWidth && reportDivWidth > 1000) {
                //Set zoom to 100%
                s.GetReportPreview().zoom(1);
            }
        }
    </script>
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
    <asp:HiddenField ID="hfCurrentHubFK" runat="server" Value="" />
    <asp:HiddenField ID="hfCurrentStateFK" runat="server" Value="" />
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
                        <asp:HiddenField ID="hfReportToRunOnlyExportAllowed" runat="server" Value="" />
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
                        <dx:BootstrapButton ID="btnExportReport" runat="server" Text="Export and Download Report"
                            OnClick="btnExportReport_Click" AutoPostBack="true" ValidationGroup="vgCriteria"
                            SettingsBootstrap-RenderOption="primary">
                            <CssClasses Icon="fas fa-file-download" Control="mb-2 hidden" />
                            <ClientSideEvents Click="btnExportReportClick" />
                        </dx:BootstrapButton>
                        <button id="btnReportLoading" class="mb-2 btn btn-primary dxbs-button hidden">
                            <span class="spinner-border spinner-border-sm"></span>&nbsp;Loading...
                        </button>
                    </div>
                    <div id="divNoCriteria" class="text-center hidden">
                        <label>No criteria for this report!</label>
                    </div>
                    <asp:UpdatePanel ID="upCriteria" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>
                            <asp:HiddenField ID="hfRunningReport" runat="server" />
                            <div id="divStartEndDates" class="criteria-div hidden">
                                <dx:BootstrapDateEdit ID="deStartDate" runat="server" Caption="Start Date" EditFormat="Date" AllowNull="true"
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
                                <dx:BootstrapDateEdit ID="deEndDate" runat="server" Caption="End Date" EditFormat="Date" AllowNull="true"
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
                                <dx:BootstrapDateEdit ID="dePointInTime" runat="server" Caption="Point In Time" EditFormat="Date" AllowNull="true"
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
                            <div id="divYears" class="criteria-div hidden">
                                <dx:BootstrapComboBox ID="ddYear" runat="server" Caption="Years" NullText="--Select--" AllowNull="true"
                                    ValueField="ItemValue" TextField="ItemText" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false"
                                    OnValidation="ddYear_Validation" ClientInstanceName="ddYear">
                                    <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                    <ClientSideEvents Validation="validateYear" />
                                    <ValidationSettings ValidationGroup="vgCriteria" ErrorDisplayMode="ImageWithText" CausesValidation="true">
                                        <RequiredField IsRequired="false" ErrorText="A year must be selected!" />
                                    </ValidationSettings>
                                </dx:BootstrapComboBox>
                            </div>
                            <div id="divBIRProfileGroup" class="criteria-div hidden">
                                <dx:BootstrapComboBox ID="ddBIRProfileGroup" runat="server" Caption="BIR Profile Group" NullText="--Select--" AllowNull="true"
                                    IncrementalFilteringMode="StartsWith" AllowMouseWheel="false"
                                    OnValidation="ddBIRProfileGroup_Validation" ClientInstanceName="ddBIRProfileGroup">
                                    <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                    <ClientSideEvents Validation="validateBIRProfileGroup" />
                                    <Items>
                                        <dx:BootstrapListEditItem Value="RACE" Text="Race"></dx:BootstrapListEditItem>
                                        <dx:BootstrapListEditItem Value="GENDER" Text="Gender"></dx:BootstrapListEditItem>
                                        <dx:BootstrapListEditItem Value="ETHNICITY" Text="Ethnicity"></dx:BootstrapListEditItem>
                                        <dx:BootstrapListEditItem Value="IEP" Text="IEP Status"></dx:BootstrapListEditItem>
                                        <dx:BootstrapListEditItem Value="DLL" Text="DLL Status"></dx:BootstrapListEditItem>
                                    </Items>
                                    <ValidationSettings ValidationGroup="vgCriteria" ErrorDisplayMode="ImageWithText" CausesValidation="true">
                                        <RequiredField IsRequired="false" ErrorText="A BIR Profile Group must be selected!" />
                                    </ValidationSettings>
                                </dx:BootstrapComboBox>
                            </div>
                            <div id="divBIRProfileItem" class="criteria-div hidden">
                                <dx:BootstrapComboBox ID="ddBIRProfileItem" runat="server" Caption="BIR Profile Item" NullText="--Select--" AllowNull="true"
                                    IncrementalFilteringMode="StartsWith" AllowMouseWheel="false"
                                    OnValidation="ddBIRProfileItem_Validation" ClientInstanceName="ddBIRProfileItem">
                                    <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                    <ClientSideEvents Validation="validateBIRProfileItem" />
                                    <Items>
                                        <dx:BootstrapListEditItem Value="BIR" Text="Behavior Incident Frequency"></dx:BootstrapListEditItem>
                                        <dx:BootstrapListEditItem Value="ISS" Text="In-School Suspensions"></dx:BootstrapListEditItem>
                                        <dx:BootstrapListEditItem Value="OSS" Text="Out-of-School Suspensions"></dx:BootstrapListEditItem>
                                        <dx:BootstrapListEditItem Value="DISMISSAL" Text="Dismissals"></dx:BootstrapListEditItem>
                                    </Items>
                                    <ValidationSettings ValidationGroup="vgCriteria" ErrorDisplayMode="ImageWithText" CausesValidation="true">
                                        <RequiredField IsRequired="false" ErrorText="An BIR Profile Item must be selected!" />
                                    </ValidationSettings>
                                </dx:BootstrapComboBox>
                            </div>
                            <div id="divReportFocus" class="criteria-div hidden">
                                <dx:BootstrapComboBox ID="ddReportFocus" runat="server" Caption="Report Focus" NullText="--Select--" AllowNull="true"
                                    IncrementalFilteringMode="StartsWith" AllowMouseWheel="false"
                                    OnValidation="ddReportFocus_Validation" ClientInstanceName="ddReportFocus">
                                    <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                    <ClientSideEvents Validation="validateReportFocus" SelectedIndexChanged="ddReportFocus_SelectedIndexChanged" Init="ddReportFocus_Init" />
                                    <Items>
                                        <dx:BootstrapListEditItem Value="ALL" Text="All"></dx:BootstrapListEditItem>
                                        <dx:BootstrapListEditItem Value="CHI" Text="Children"></dx:BootstrapListEditItem>
                                        <dx:BootstrapListEditItem Value="CR" Text="Classrooms"></dx:BootstrapListEditItem>
                                    </Items>
                                    <ValidationSettings ValidationGroup="vgCriteria" ErrorDisplayMode="ImageWithText" CausesValidation="true">
                                        <RequiredField IsRequired="false" ErrorText="A Report Focus must be selected!" />
                                    </ValidationSettings>
                                </dx:BootstrapComboBox>
                                <div id="divReportFocusWarning" class="alert alert-warning font-size-80 hidden">
                                </div>
                            </div>
                            <div id="divClassrooms" class="criteria-div hidden">
                                <dx:BootstrapListBox ID="lstBxClassroom" runat="server" Caption="Classroom(s)" AllowCustomValues="true"
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
                                <dx:BootstrapListBox ID="lstBxChild" runat="server" Caption="Child(ren)" AllowCustomValues="true"
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
                                <dx:BootstrapListBox ID="lstBxRace" runat="server" Caption="Race(s)" AllowCustomValues="true"
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
                                <dx:BootstrapListBox ID="lstBxEthnicity" runat="server" Caption="Ethnicity(ies)" AllowCustomValues="true"
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
                                <dx:BootstrapListBox ID="lstBxGender" runat="server" Caption="Gender(s)" AllowCustomValues="true"
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
                                <dx:BootstrapComboBox ID="ddIEP" runat="server" Caption="IEP Status" NullText="--Select--" AllowNull="true"
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
                                <dx:BootstrapComboBox ID="ddDLL" runat="server" Caption="DLL Status" NullText="--Select--" AllowNull="true"
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
                                <dx:BootstrapListBox ID="lstBxEmployee" runat="server" Caption="Professional(s)" AllowCustomValues="true"
                                    SelectionMode="CheckColumn" EnableSelectAll="true"
                                    ValueField="ProgramEmployeePK" ValueType="System.Int32" TextField="ProgramEmployeeName"
                                    OnValidation="lstBxEmployee_Validation" ClientInstanceName="lstBxEmployee">
                                    <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                    <ClientSideEvents Validation="validateEmployeeList" />
                                    <FilteringSettings ShowSearchUI="true" EditorNullTextDisplayMode="Unfocused" />
                                    <ValidationSettings ValidationGroup="vgCriteria" ErrorDisplayMode="ImageWithText" CausesValidation="true">
                                        <RequiredField IsRequired="false" ErrorText="At least one professional must be selected!" />
                                    </ValidationSettings>
                                </dx:BootstrapListBox>
                            </div>
                            <div id="divEmployeeRole" class="criteria-div hidden">
                                <dx:BootstrapComboBox ID="ddEmployeeRole" runat="server" Caption="Professional Role" NullText="--Select--" AllowNull="true"
                                    IncrementalFilteringMode="StartsWith" AllowMouseWheel="false"
                                    OnValidation="ddEmployeeRole_Validation" ClientInstanceName="ddEmployeeRole">
                                    <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                    <ClientSideEvents Validation="validateEmployeeRole" />
                                    <Items>
                                        <dx:BootstrapListEditItem Value="ANY" Text="Any Role"></dx:BootstrapListEditItem>
                                        <dx:BootstrapListEditItem Value="OBS" Text="Observer"></dx:BootstrapListEditItem>
                                        <dx:BootstrapListEditItem Value="LT" Text="Lead Teacher"></dx:BootstrapListEditItem>
                                        <dx:BootstrapListEditItem Value="TA" Text="Teaching Assistant"></dx:BootstrapListEditItem>
                                    </Items>
                                    <ValidationSettings ValidationGroup="vgCriteria" ErrorDisplayMode="ImageWithText" CausesValidation="true">
                                        <RequiredField IsRequired="false" ErrorText="A professional role must be selected!" />
                                    </ValidationSettings>
                                </dx:BootstrapComboBox>
                            </div>
                            <div id="divTeachers" class="criteria-div hidden">
                                <dx:BootstrapListBox ID="lstBxTeacher" runat="server" Caption="Teacher(s)" AllowCustomValues="true"
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
                                <dx:BootstrapListBox ID="lstBxCoach" runat="server" Caption="Coach(es)" AllowCustomValues="true"
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
                            <div id="divProblemBehaviors" class="criteria-div hidden">
                                <dx:BootstrapListBox ID="lstBxProblemBehavior" runat="server" Caption="Problem Behavior(s)" AllowCustomValues="true"
                                    SelectionMode="CheckColumn" EnableSelectAll="true"
                                    ValueField="CodeProblemBehaviorPK" ValueType="System.Int32" TextField="Description"
                                    OnValidation="lstBxProblemBehavior_Validation" ClientInstanceName="lstBxProblemBehavior">
                                    <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                    <ClientSideEvents Validation="validateProblemBehaviorList" />
                                    <FilteringSettings ShowSearchUI="true" EditorNullTextDisplayMode="Unfocused" />
                                    <ValidationSettings ValidationGroup="vgCriteria" ErrorDisplayMode="ImageWithText" CausesValidation="true">
                                        <RequiredField IsRequired="false" ErrorText="At least one Problem Behavior must be selected!" />
                                    </ValidationSettings>
                                </dx:BootstrapListBox>
                            </div>
                            <div id="divActivities" class="criteria-div hidden">
                                <dx:BootstrapListBox ID="lstBxActivity" runat="server" Caption="Activity(ies)" AllowCustomValues="true"
                                    SelectionMode="CheckColumn" EnableSelectAll="true"
                                    ValueField="CodeActivityPK" ValueType="System.Int32" TextField="Description"
                                    OnValidation="lstBxActivity_Validation" ClientInstanceName="lstBxActivity">
                                    <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                    <ClientSideEvents Validation="validateActivityList" />
                                    <FilteringSettings ShowSearchUI="true" EditorNullTextDisplayMode="Unfocused" />
                                    <ValidationSettings ValidationGroup="vgCriteria" ErrorDisplayMode="ImageWithText" CausesValidation="true">
                                        <RequiredField IsRequired="false" ErrorText="At least one Activity must be selected!" />
                                    </ValidationSettings>
                                </dx:BootstrapListBox>
                            </div>
                            <div id="divOthersInvolved" class="criteria-div hidden">
                                <dx:BootstrapListBox ID="lstBxOthersInvolved" runat="server" Caption="Others Involved" AllowCustomValues="true"
                                    SelectionMode="CheckColumn" EnableSelectAll="true"
                                    ValueField="CodeOthersInvolvedPK" ValueType="System.Int32" TextField="Description"
                                    OnValidation="lstBxOthersInvolved_Validation" ClientInstanceName="lstBxOthersInvolved">
                                    <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                    <ClientSideEvents Validation="validateOthersInvolvedList" />
                                    <FilteringSettings ShowSearchUI="true" EditorNullTextDisplayMode="Unfocused" />
                                    <ValidationSettings ValidationGroup="vgCriteria" ErrorDisplayMode="ImageWithText" CausesValidation="true">
                                        <RequiredField IsRequired="false" ErrorText="At least one Others Involved must be selected!" />
                                    </ValidationSettings>
                                </dx:BootstrapListBox>
                            </div>
                            <div id="divPossibleMotivations" class="criteria-div hidden">
                                <dx:BootstrapListBox ID="lstBxPossibleMotivation" runat="server" Caption="Possible Motivation(s)" AllowCustomValues="true"
                                    SelectionMode="CheckColumn" EnableSelectAll="true"
                                    ValueField="CodePossibleMotivationPK" ValueType="System.Int32" TextField="Description"
                                    OnValidation="lstBxPossibleMotivation_Validation" ClientInstanceName="lstBxPossibleMotivation">
                                    <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                    <ClientSideEvents Validation="validatePossibleMotivationList" />
                                    <FilteringSettings ShowSearchUI="true" EditorNullTextDisplayMode="Unfocused" />
                                    <ValidationSettings ValidationGroup="vgCriteria" ErrorDisplayMode="ImageWithText" CausesValidation="true">
                                        <RequiredField IsRequired="false" ErrorText="At least one Possible Motivation must be selected!" />
                                    </ValidationSettings>
                                </dx:BootstrapListBox>
                            </div>
                            <div id="divStrategyResponses" class="criteria-div hidden">
                                <dx:BootstrapListBox ID="lstBxStrategyResponse" runat="server" Caption="Strategy Response(s)" AllowCustomValues="true"
                                    SelectionMode="CheckColumn" EnableSelectAll="true"
                                    ValueField="CodeStrategyResponsePK" ValueType="System.Int32" TextField="Description"
                                    OnValidation="lstBxStrategyResponse_Validation" ClientInstanceName="lstBxStrategyResponse">
                                    <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                    <ClientSideEvents Validation="validateStrategyResponseList" />
                                    <FilteringSettings ShowSearchUI="true" EditorNullTextDisplayMode="Unfocused" />
                                    <ValidationSettings ValidationGroup="vgCriteria" ErrorDisplayMode="ImageWithText" CausesValidation="true">
                                        <RequiredField IsRequired="false" ErrorText="At least one Strategy Response must be selected!" />
                                    </ValidationSettings>
                                </dx:BootstrapListBox>
                            </div>
                            <div id="divAdminFollowUps" class="criteria-div hidden">
                                <dx:BootstrapListBox ID="lstBxAdminFollowUp" runat="server" Caption="Admin Follow-up(s)" AllowCustomValues="true"
                                    SelectionMode="CheckColumn" EnableSelectAll="true"
                                    ValueField="CodeAdminFollowUpPK" ValueType="System.Int32" TextField="Description"
                                    OnValidation="lstBxAdminFollowUp_Validation" ClientInstanceName="lstBxAdminFollowUp">
                                    <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                    <ClientSideEvents Validation="validateAdminFollowUpList" />
                                    <FilteringSettings ShowSearchUI="true" EditorNullTextDisplayMode="Unfocused" />
                                    <ValidationSettings ValidationGroup="vgCriteria" ErrorDisplayMode="ImageWithText" CausesValidation="true">
                                        <RequiredField IsRequired="false" ErrorText="At least one Admin Follow-up must be selected!" />
                                    </ValidationSettings>
                                </dx:BootstrapListBox>
                            </div>
                            <div id="divPrograms" class="criteria-div hidden">
                                <dx:BootstrapListBox ID="lstBxProgram" runat="server" Caption="Program(s)" AllowCustomValues="true"
                                    SelectionMode="CheckColumn" EnableSelectAll="true"
                                    ValueField="ProgramPK" ValueType="System.Int32" TextField="ProgramName"
                                    OnValidation="lstBxPHCS_Validation" ClientInstanceName="lstBxProgram">
                                    <ClientSideEvents Validation="validatePHCSList" />
                                    <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                    <FilteringSettings ShowSearchUI="true" EditorNullTextDisplayMode="Unfocused" />
                                    <ValidationSettings ValidationGroup="vgCriteria" ErrorDisplayMode="ImageWithText" CausesValidation="true">
                                        <RequiredField IsRequired="false" ErrorText="At least one Program must be selected!" />
                                    </ValidationSettings>
                                </dx:BootstrapListBox>
                            </div>
                            <div id="divHubs" class="criteria-div hidden">
                                <dx:BootstrapListBox ID="lstBxHub" runat="server" Caption="Hub(s)" AllowCustomValues="true"
                                    SelectionMode="CheckColumn" EnableSelectAll="true"
                                    ValueField="HubPK" ValueType="System.Int32" TextField="Name"
                                    OnValidation="lstBxPHCS_Validation" ClientInstanceName="lstBxHub">
                                    <ClientSideEvents Validation="validatePHCSList" />
                                    <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                    <FilteringSettings ShowSearchUI="true" EditorNullTextDisplayMode="Unfocused" />
                                    <ValidationSettings ValidationGroup="vgCriteria" ErrorDisplayMode="ImageWithText" CausesValidation="true">
                                        <RequiredField IsRequired="false" ErrorText="At least one hub must be selected!" />
                                    </ValidationSettings>
                                </dx:BootstrapListBox>
                            </div>
                            <div id="divCohorts" class="criteria-div hidden">
                                <dx:BootstrapListBox ID="lstBxCohort" runat="server" Caption="Cohort(s)" AllowCustomValues="true"
                                    SelectionMode="CheckColumn" EnableSelectAll="true"
                                    ValueField="CohortPK" ValueType="System.Int32" TextField="CohortName"
                                    OnValidation="lstBxPHCS_Validation" ClientInstanceName="lstBxCohort">
                                    <ClientSideEvents Validation="validatePHCSList" />
                                    <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                    <FilteringSettings ShowSearchUI="true" EditorNullTextDisplayMode="Unfocused" />
                                    <ValidationSettings ValidationGroup="vgCriteria" ErrorDisplayMode="ImageWithText" CausesValidation="true">
                                        <RequiredField IsRequired="false" ErrorText="At least one cohort must be selected!" />
                                    </ValidationSettings>
                                </dx:BootstrapListBox>
                            </div>
                            <div id="divStates" class="criteria-div hidden">
                                <dx:BootstrapListBox ID="lstBxState" runat="server" Caption="State(s)" AllowCustomValues="true"
                                    SelectionMode="CheckColumn" EnableSelectAll="true"
                                    ValueField="StatePK" ValueType="System.Int32" TextField="StateName"
                                    OnValidation="lstBxPHCS_Validation" ClientInstanceName="lstBxState">
                                    <ClientSideEvents Validation="validatePHCSList" />
                                    <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                    <FilteringSettings ShowSearchUI="true" EditorNullTextDisplayMode="Unfocused" />
                                    <ValidationSettings ValidationGroup="vgCriteria" ErrorDisplayMode="ImageWithText" CausesValidation="true">
                                        <RequiredField IsRequired="false" ErrorText="At least one State must be selected!" />
                                    </ValidationSettings>
                                </dx:BootstrapListBox>
                            </div>
                        </ContentTemplate>
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="btnRunReport" EventName="Click" />
                        </Triggers>
                    </asp:UpdatePanel>
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
                                    AutoGenerateColumns="false" DataSourceID="linqReportDataSource">
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
                                        <dx:BootstrapGridViewDateColumn FieldName="LastRun" Caption="Last Used On" PropertiesDateEdit-DisplayFormatString="MM/dd/yyyy hh:mm tt ET" AdaptivePriority="3" />
                                        <dx:BootstrapGridViewDataColumn FieldName="Keywords" Caption="Keywords" AdaptivePriority="4"
                                            CssClasses-DataCell="hidden" CssClasses-FilterCell="hidden" CssClasses-FooterCell="hidden" 
                                            CssClasses-EditCell="hidden" CssClasses-HeaderCell="hidden" CssClasses-GroupFooterCell="hidden" />
                                        <dx:BootstrapGridViewDataColumn Settings-AllowDragDrop="False" AdaptivePriority="0" MaxWidth="100" CssClasses-DataCell="text-center">
                                            <DataItemTemplate>
                                                <button class="btn btn-primary select-report-button"><i class="fas fa-mouse-pointer"></i>&nbsp;Select</button>
                                                <asp:HiddenField ID="hfReportCatalogPK" runat="server" Value='<%# Eval("ReportCatalogPK") %>' />
                                                <asp:HiddenField ID="hfReportClass" runat="server" Value='<%# Eval("ReportClass") %>' />
                                                <asp:HiddenField ID="hfReportName" runat="server" Value='<%# Eval("ReportName") %>' />
                                                <asp:HiddenField ID="hfOnlyExportAllowed" runat="server" Value='<%# Convert.ToBoolean(Eval("OnlyExportAllowed")) ? "true" : "false" %>' />
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
                                <asp:LinqDataSource ID="linqReportDataSource" runat="server" OnSelecting="linqReportDataSource_Selecting"></asp:LinqDataSource>
                            </ContentTemplate>
                            <Triggers>
                                <asp:AsyncPostBackTrigger ControlID="btnRunReport" EventName="Click" />
                            </Triggers>
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
                                <dx:ASPxWebDocumentViewer ID="reportViewer" runat="server">
                                    <ClientSideEvents Init="reportViewerInit" BeforeRender="resizeWindow" />
                                </dx:ASPxWebDocumentViewer>
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
