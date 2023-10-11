<%@ Page Title="State Settings" Language="C#" MasterPageFile="~/MasterPages/LoggedIn.master" AutoEventWireup="true" CodeBehind="StateSettings.aspx.cs" Inherits="Pyramid.Pages.StateSettings" %>

<%@ Register Assembly="DevExpress.Web.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Data.Linq" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.Bootstrap.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>
<%@ Register TagPrefix="uc" TagName="Messaging" Src="~/User_Controls/MessagingSystem.ascx" %>
<%@ Register TagPrefix="uc" TagName="Submit" Src="~/User_Controls/Submit.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ScriptContent" runat="server">
    <script type="text/javascript">
        $(document).ready(function () {
            //Run the initial page init
            initializePage();

            //Run the page init after every AJAX load
            var requestManager = Sys.WebForms.PageRequestManager.getInstance();
            requestManager.add_endRequest(initializePage);
        });

        //Initializes the page
        function initializePage() {
            //Set up the click event for the help buttons
            $('#btnFormDueDateSettingsHelp').popover({
                trigger: 'hover focus',
                title: 'Help',
                html: true,
                content:'<p>These settings perform the following functions:</p>' +
                    '<p>Show Due Dates on Home Page? = Whether or not the form due date list will display on the home page if the current date is after the date that due dates begin.</p >' +
                    '<p>Date that Due Dates Begin = The date that the due dates below will start.  For example, if you set this to 01/01/2021, only forms that are due after that date will show up in the form due date list.</p>' +
                    '<p>Days Before the Due Date to Warn the User = The number of days before the due date that an upcoming form will turn yellow in order to warn the user that it will be due soon.</p>' +
                    '<p>Months to Display Overdue/Completed Forms = The number of months that overdue and completed forms will show up in the form due date list.</p>' +
                    '<p>Months to Display Upcoming Due Forms = The number of months in the future to display upcoming forms that are due.</p>'
            });
        }

        //This function validates the begin date
        function validateDueDatesBeginDate(s, e) {
            //Get the begin date and due dates enabled
            var beginDate = deDueDatesBeginDate.GetDate();
            var dueDatesEnabled = ddDueDatesEnabled.GetText();

            //Check to see if due dates are enabled
            if (dueDatesEnabled != null && dueDatesEnabled == 'Yes') {
                //Check to see if the begin date has value
                if (beginDate == null) {
                    //Show an error message
                    e.isValid = false;
                    e.errorText = 'Required when due dates are shown!';
                }
            }
        }

        //This function validates the months start
        function validateDueDatesMonthsStart(s, e) {
            //Get the months start and due dates enabled
            var dueDatesMonthsStart = txtDueDatesMonthsStart.GetText();
            var dueDatesEnabled = ddDueDatesEnabled.GetText();

            //Check to see if due dates are enabled
            if (dueDatesEnabled != null && dueDatesEnabled == 'Yes') {
                //Check to see if the months start has value
                if (dueDatesMonthsStart == null || dueDatesMonthsStart == '') {
                    //Show an error message
                    e.isValid = false;
                    e.errorText = 'Required when due dates are shown!';
                }
            }
        }

        //This function validates the months end
        function validateDueDatesMonthsEnd(s, e) {
            //Get the months end and due dates enabled
            var dueDatesMonthsEnd = txtDueDatesMonthsEnd.GetText();
            var dueDatesEnabled = ddDueDatesEnabled.GetText();

            //Check to see if due dates are enabled
            if (dueDatesEnabled != null && dueDatesEnabled == 'Yes') {
                //Check to see if the months end has value
                if (dueDatesMonthsEnd == null || dueDatesMonthsEnd == '') {
                    //Show an error message
                    e.isValid = false;
                    e.errorText = 'Required when due dates are shown!';
                }
            }
        }

        //This function validates the days until warning
        function validateDueDatesDaysUntilWarning(s, e) {
            //Get the months end and due dates enabled
            var dueDatesDaysUntilWarning = txtDueDatesDaysUntilWarning.GetText();
            var dueDatesEnabled = ddDueDatesEnabled.GetText();

            //Check to see if due dates are enabled
            if (dueDatesEnabled != null && dueDatesEnabled == 'Yes') {
                //Check to see if the days until warning has value
                if (dueDatesDaysUntilWarning == null || dueDatesDaysUntilWarning == '') {
                    //Show an error message
                    e.isValid = false;
                    e.errorText = 'Required when due dates are shown!';
                }
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
            <asp:AsyncPostBackTrigger ControlID="submitDueDateSettings" />
            <asp:AsyncPostBackTrigger ControlID="bsGRDueDates" />
            <asp:AsyncPostBackTrigger ControlID="lbAddDueDate" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="lbDeleteDueDate" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="submitDueDate" />
        </Triggers>
    </asp:UpdatePanel>
    <asp:UpdatePanel ID="upDueDates" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <div id="divDueDates">
                <div class="row">
                    <div class="col-lg-12 mb-4">
                        <div class="card bg-light">
                            <div class="card-header">
                                <div class="row">
                                    <div class="col-md-8">
                                        Form Due Date Settings
                                    </div>
                                    <div class="col-md-4">
                                        <div class="float-right">
                                            <button id="btnFormDueDateSettingsHelp" type="button" class="btn btn-outline-primary"><i class="fas fa-question-circle"></i>&nbsp;Help</button>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div class="card-body">
                                <div class="alert alert-primary">
                                    <i class="fas fa-info-circle"></i>&nbsp;These settings control the due dates section on the home page.
                                </div>
                                <div class="row">
                                    <div class="col-md-4">
                                        <dx:BootstrapComboBox ID="ddDueDatesEnabled" runat="server" Caption="Show Due Dates on Home Page?" NullText="--Select--"
                                            ValueType="System.Boolean" ClientInstanceName="ddDueDatesEnabled"
                                            IncrementalFilteringMode="Contains" AllowMouseWheel="false">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ValidationSettings ValidationGroup="vgDueDateSettings" ErrorDisplayMode="ImageWithText">
                                                <RequiredField IsRequired="true" ErrorText="Required!" />
                                            </ValidationSettings>
                                            <Items>
                                                <dx:BootstrapListEditItem Value="True" Text="Yes"></dx:BootstrapListEditItem>
                                                <dx:BootstrapListEditItem Value="False" Text="No"></dx:BootstrapListEditItem>
                                            </Items>
                                        </dx:BootstrapComboBox>
                                    </div>
                                    <div class="col-md-4">
                                        <dx:BootstrapDateEdit ID="deDueDatesBeginDate" runat="server" Caption="Date that Due Dates Go into Effect" EditFormat="Date"
                                            EditFormatString="MM/dd/yyyy" UseMaskBehavior="true" NullText="--Select--"
                                            AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900"
                                            ClientInstanceName="deDueDatesBeginDate"
                                            OnValidation="deDueDatesBeginDate_Validation">
                                            <ClientSideEvents Validation="validateDueDatesBeginDate" />
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                            <ValidationSettings ValidationGroup="vgDueDateSettings" ErrorDisplayMode="ImageWithText">
                                                <RequiredField IsRequired="false" ErrorText="Required!" />
                                            </ValidationSettings>
                                        </dx:BootstrapDateEdit>
                                    </div>
                                    <div class="col-md-4">
                                        <dx:BootstrapTextBox ID="txtDueDatesDaysUntilWarning" runat="server" Caption="Days Before the Due Date to Warn the User"
                                            ClientInstanceName="txtDueDatesDaysUntilWarning" OnValidation="txtDueDatesDaysUntilWarning_Validation">
                                            <ClientSideEvents Validation="validateDueDatesDaysUntilWarning" />
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ValidationSettings ValidationGroup="vgDueDateSettings" ErrorDisplayMode="ImageWithText">
                                                <RegularExpression ValidationExpression="\d{1,3}" ErrorText="Must be a positive number with a maximum of 3 digits!" />
                                                <RequiredField IsRequired="false" ErrorText="Required!" />
                                            </ValidationSettings>
                                        </dx:BootstrapTextBox>
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-md-4">
                                        <dx:BootstrapTextBox ID="txtDueDatesMonthsStart" runat="server" Caption="Months to Display Overdue/Completed Forms"
                                            ClientInstanceName="txtDueDatesMonthsStart" OnValidation="txtDueDatesMonthsStart_Validation">
                                            <ClientSideEvents Validation="validateDueDatesMonthsStart" />
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ValidationSettings ValidationGroup="vgDueDateSettings" ErrorDisplayMode="ImageWithText">
                                                <RegularExpression ValidationExpression="\d{1,5}(\.\d{0,2})?" ErrorText="Must be a positive number with a maximum of 5 digits and 2 decimal places!" />
                                                <RequiredField IsRequired="false" ErrorText="Required!" />
                                            </ValidationSettings>
                                        </dx:BootstrapTextBox>
                                    </div>
                                    <div class="col-md-4">
                                        <dx:BootstrapTextBox ID="txtDueDatesMonthsEnd" runat="server" Caption="Months to Display Upcoming Due Forms"
                                            ClientInstanceName="txtDueDatesMonthsEnd" OnValidation="txtDueDatesMonthsEnd_Validation">
                                            <ClientSideEvents Validation="validateDueDatesMonthsEnd" />
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ValidationSettings ValidationGroup="vgDueDateSettings" ErrorDisplayMode="ImageWithText">
                                                <RegularExpression ValidationExpression="\d{1,5}(\.\d{0,2})?" ErrorText="Must be a positive number with a maximum of 5 digits and 2 decimal places!" />
                                                <RequiredField IsRequired="false" ErrorText="Required!" />
                                            </ValidationSettings>
                                        </dx:BootstrapTextBox>
                                    </div>
                                </div>
                            </div>
                            <div class="card-footer">
                                <div class="d-flex align-items-center justify-content-center">
                                    <uc:Submit ID="submitDueDateSettings" runat="server" ValidationGroup="vgDueDateSettings"
                                        ControlCssClass="center-content" ShowCancelButton="false" SubmitButtonText="Save Settings" SubmittingButtonText="Saving..."
                                        OnSubmitClick="submitDueDateSettings_SubmitClick"
                                        OnValidationFailed="submitDueDateSettings_ValidationFailed" />
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="row">
                    <div class="col-lg-12 mb-4">
                        <div class="card bg-light">
                            <div class="card-header">
                                Form Due Dates
                                <asp:LinkButton ID="lbAddDueDate" runat="server" CssClass="btn btn-loader btn-primary float-right" OnClick="lbAddDueDate_Click"><i class="fas fa-plus"></i>&nbsp;Add New Due Date</asp:LinkButton>
                            </div>
                            <div class="card-body">
                                <div class="alert alert-primary">
                                    <i class="fas fa-info-circle"></i>&nbsp;All dates are in mm/dd format.  For example, May 12th is entered as 05/12.
                                </div>
                                <div class="row">
                                    <div class="col-md-12">
                                        <dx:BootstrapGridView ID="bsGRDueDates" runat="server" EnableCallBacks="false" KeyFieldName="FormDueDatePK"
                                            AutoGenerateColumns="false" DataSourceID="linqDueDateDataSource">
                                            <SettingsPager PageSize="15" />
                                            <SettingsBootstrap Striped="true" />
                                            <SettingsBehavior EnableRowHotTrack="true" />
                                            <SettingsSearchPanel Visible="true" ShowApplyButton="true" />
                                            <Settings ShowGroupPanel="true" />
                                            <CssClasses IconHeaderSortUp="fas fa-long-arrow-alt-up" IconHeaderSortDown="fas fa-long-arrow-alt-down" IconShowAdaptiveDetailButton="fas fa-plus-circle" />
                                            <SettingsAdaptivity AdaptivityMode="HideDataCells" AllowOnlyOneAdaptiveDetailExpanded="true" AdaptiveColumnPosition="Left"></SettingsAdaptivity>
                                            <Columns>
                                                <dx:BootstrapGridViewDataColumn FieldName="FormAbbreviation" Caption="Form" SortIndex="0" SortOrder="Ascending" AdaptivePriority="0" />
                                                <dx:BootstrapGridViewDateColumn FieldName="DueStartDate" Caption="Window Start Date" PropertiesDateEdit-DisplayFormatString="MM/dd" AdaptivePriority="3" />
                                                <dx:BootstrapGridViewDateColumn FieldName="DueRecommendedDate" Caption="Due Date" SortIndex="1" SortOrder="Ascending" PropertiesDateEdit-DisplayFormatString="MM/dd" AdaptivePriority="2" />
                                                <dx:BootstrapGridViewDateColumn FieldName="DueEndDate" Caption="Window End Date" PropertiesDateEdit-DisplayFormatString="MM/dd" AdaptivePriority="4" />
                                                <dx:BootstrapGridViewDataColumn FieldName="HelpText" Caption="Help Text/Tooltip" AdaptivePriority="5" />
                                                <dx:BootstrapGridViewButtonEditColumn Settings-AllowDragDrop="False" AdaptivePriority="1" CssClasses-DataCell="text-center">
                                                    <DataItemTemplate>
                                                        <div class="btn-group">
                                                            <button type="button" class="btn btn-secondary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                                Actions
                                                            </button>
                                                            <div class="dropdown-menu dropdown-menu-right">
                                                                <asp:LinkButton ID="lbEditDueDate" runat="server" CssClass="dropdown-item" OnClick="lbEditDueDate_Click"><i class="fas fa-edit"></i> Edit</asp:LinkButton>
                                                                <button class="dropdown-item delete-gridview" data-pk='<%# Eval("FormDueDatePK") %>' data-hf="hfDeleteDueDatePK" data-target="#divDeleteDueDateModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                            </div>
                                                        </div>
                                                        <asp:HiddenField ID="hfDueDatePK" runat="server" Value='<%# Eval("FormDueDatePK") %>' />
                                                    </DataItemTemplate>
                                                </dx:BootstrapGridViewButtonEditColumn>
                                            </Columns>
                                        </dx:BootstrapGridView>
                                        <asp:HiddenField ID="hfDeleteDueDatePK" runat="server" Value="0" />
                                        <asp:LinqDataSource ID="linqDueDateDataSource" runat="server" OnSelecting="linqDueDateDataSource_Selecting"></asp:LinqDataSource>
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-lg-12">
                                        <div id="divAddEditDueDate" runat="server" class="card mt-2" visible="false">
                                            <div class="card-header">
                                                <asp:Label ID="lblAddEditDueDate" runat="server" Text=""></asp:Label>
                                            </div>
                                            <div class="card-body">
                                                <div class="row">
                                                    <div class="col-md-3">
                                                        <dx:BootstrapComboBox ID="ddDueDateForm" runat="server" Caption="Form" NullText="--Select--"
                                                            TextField="FormAbbreviation" ValueField="CodeFormPK" ValueType="System.Int32" 
                                                            IncrementalFilteringMode="Contains" AllowMouseWheel="false">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <ValidationSettings ValidationGroup="vgDueDate" ErrorDisplayMode="ImageWithText">
                                                                <RequiredField IsRequired="true" ErrorText="Form is required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapComboBox>
                                                    </div>
                                                    <div class="col-md-3">
                                                        <dx:BootstrapTextBox ID="txtDueStartDate" runat="server" Caption="Window Start Date" DisplayFormatString="MM/dd"
                                                            ClientInstanceName="txtDueStartDate" OnValidation="txtDueStartDate_Validation">
                                                            <MaskSettings Mask="00/00" AllowMouseWheel="false" ErrorText="Must be a valid month/day format!" ShowHints="true" />
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <ValidationSettings ValidationGroup="vgDueDate" ErrorDisplayMode="ImageWithText">
                                                                <RequiredField IsRequired="true" ErrorText="Window Start Date is required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapTextBox>
                                                    </div>
                                                    <div class="col-md-3">
                                                        <dx:BootstrapTextBox ID="txtDueRecommendedDate" runat="server" Caption="Due Date" DisplayFormatString="MM/dd"
                                                            ClientInstanceName="txtDueRecommendedDate" OnValidation="txtDueRecommendedDate_Validation">
                                                            <MaskSettings Mask="00/00" AllowMouseWheel="false" ErrorText="Must be a valid month/day format!" ShowHints="true" />
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <ValidationSettings ValidationGroup="vgDueDate" ErrorDisplayMode="ImageWithText">
                                                                <RequiredField IsRequired="true" ErrorText="Due Date is required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapTextBox>
                                                    </div>
                                                    <div class="col-md-3">
                                                        <dx:BootstrapTextBox ID="txtDueEndDate" runat="server" Caption="Window End Date" DisplayFormatString="MM/dd"
                                                            ClientInstanceName="txtDueEndDate" OnValidation="txtDueEndDate_Validation">
                                                            <MaskSettings Mask="00/00" AllowMouseWheel="false" ErrorText="Must be a valid month/day format!" ShowHints="true" />
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <ValidationSettings ValidationGroup="vgDueDate" ErrorDisplayMode="ImageWithText">
                                                                <RequiredField IsRequired="true" ErrorText="Window End Date is required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapTextBox>
                                                    </div>
                                                </div>
                                                <div class="row">
                                                    <div class="col-md-6">
                                                        <dx:BootstrapMemo ID="txtDueDateHelpText" runat="server" Caption="Help Text/Tooltip" Rows="3" MaxLength="500">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <ValidationSettings ValidationGroup="vgDueDate" ErrorDisplayMode="ImageWithText">
                                                                <RequiredField IsRequired="false" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapMemo>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="card-footer">
                                                <div class="d-flex align-items-center justify-content-center">
                                                    <asp:HiddenField ID="hfAddEditDueDatePK" runat="server" Value="0" />
                                                    <uc:Submit ID="submitDueDate" runat="server" ValidationGroup="vgDueDate"
                                                        ControlCssClass="center-content"
                                                        OnSubmitClick="submitDueDate_SubmitClick" OnCancelClick="submitDueDate_CancelClick" 
                                                        OnValidationFailed="submitDueDate_ValidationFailed" />
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="lbDeleteDueDate" EventName="Click" />
        </Triggers>
    </asp:UpdatePanel>
    <div class="modal" id="divDeleteDueDateModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete Form Due Date</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this due date?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteDueDate" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteDueDateModal" OnClick="lbDeleteDueDate_Click"><i class="fas fa-check"></i>&nbsp;Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
