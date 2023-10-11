<%@ Page Title="Program Management" Language="C#" MasterPageFile="~/MasterPages/LoggedIn.master" AutoEventWireup="true" CodeBehind="ProgramManagement.aspx.cs" Inherits="Pyramid.Admin.ProgramManagement" %>

<%@ Register Assembly="DevExpress.Web.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Data.Linq" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.Bootstrap.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>
<%@ Register TagPrefix="uc" TagName="Messaging" Src="~/User_Controls/MessagingSystem.ascx" %>
<%@ Register TagPrefix="uc" TagName="Submit" Src="~/User_Controls/Submit.ascx" %>

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
            //Get the program count and max count
            var programCount = parseFloat($('[ID$="hfStateProgramCount"]').val());
            var maxProgramCount = parseFloat($('[ID$="hfMaxStateProgramCount"]').val());

            //Get the count elements
            var divProgramCountAlert = $('#divProgramCountAlert');
            var programCountAlertIcon = $('#iconProgramCountAlert');
            var spanProgramCountAlertText = $('#spanProgramCountAlertText');

            if (!$.fn.dataTable.isDataTable('#tblProgramStatuses')) {
                $('#tblProgramStatuses').DataTable({
                    responsive: {
                        details: {
                            type: 'column'
                        }
                    },
                    columnDefs: [
                        { orderable: false, targets: [-1] },
                        { className: 'control', orderable: false, targets: 0 }
                    ],
                    order: [[2, 'desc']],
                    stateSave: true,
                    stateDuration: 60,
                    pageLength: 10,
                    dom: 'frtp',
                    language: {
                        searchPlaceholder: "Enter text to search...",
                        search: "",
                    }
                });
            }
            //Make sure that the state has a limited number of programs
            if (maxProgramCount > -1) {
                //Get the percentage of maximum
                var percentOfMax = (programCount / maxProgramCount) * 100.00;

                //The label text
                var labelText = 'Your state is at <b>' + programCount.toString() + '</b> out of a maximum of <b>' + maxProgramCount.toString() + '</b> active programs. ' +
                    '(Active means that the program does not have an end date or the end date is after today)';

                

                if (percentOfMax <= 75) {
                    //Set the alert to primary
                    divProgramCountAlert.addClass('alert-primary');
                    programCountAlertIcon.addClass('fa-info-circle');
                    spanProgramCountAlertText.html(labelText);
                }
                else if (percentOfMax <= 85) {
                    //Set the alert to warning
                    divProgramCountAlert.addClass('alert-warning');
                    programCountAlertIcon.addClass('fa-info-circle');
                    spanProgramCountAlertText.html(labelText);
                }
                else if (programCount >= maxProgramCount) {
                    //Set the alert to danger
                    divProgramCountAlert.addClass('alert-danger');
                    programCountAlertIcon.addClass('fa-exclamation-triangle');
                    spanProgramCountAlertText.html('You have reached the maximum of ' + maxProgramCount.toString() + ' active programs. ' +
                        'Please contact support to have your active program limit raised. ' +
                        '(Active means that the program does not have an end date or the end date is after today.)');
                }
                else {
                    //Set the alert to danger
                    divProgramCountAlert.addClass('alert-danger');
                    programCountAlertIcon.addClass('fa-exclamation-triangle');
                    spanProgramCountAlertText.html(labelText);
                }
            }
            else {
                //Hide the program count div
                divProgramCountAlert.addClass('hidden');
            }
        }

        //Validate the cohort end date field
        function validateCohortEndDate(s, e) {
            //Get the start and end dates
            var endDate = e.value;
            var startDate = deCohortStartDate.GetDate();

            //Validate the end date
            if (endDate != null && startDate != null && startDate >= endDate) {
                e.isValid = false;
                e.errorText = "Cohort End Date must be after the Cohort Start Date!";
            }
        }

        //Validate the program end date field
        function validateProgramEndDate(s, e) {
            //Get the start and end dates
            var endDate = e.value;
            var startDate = deProgramStartDate.GetDate();

            //Validate the end date
            if (endDate != null && startDate != null && startDate >= endDate) {
                e.isValid = false;
                e.errorText = "Program End Date must be after the Program Start Date!";
            }
        }

        //This function determines the visibility of the confidentiality divs based on
        //the values in the confidentiality drop-downs
        function showHideConfidentialityDivs() {
            //Determine if confidentiality is enabled
            var confidentialityEnabled = ddStateConfidentialityEnabled.GetValue();

            //If confidentiality is enabled, show the update document div
            if (confidentialityEnabled == true) {
                $('#divUpdateConfidentiality').removeClass('hidden');
            }
            else {
                //Hide the update document div
                $('#divUpdateConfidentiality').addClass('hidden');

                //Set the update document value
                ddUpdateConfidentialityDocument.SetValue('');

                //Hide the upload fields
                $('#divConfidentialityChangeDate').addClass('hidden');
                $('#divUploadConfidentiality').addClass('hidden');
            }

            //Determine if the confidentiality document is being updated
            var updateDocument = ddUpdateConfidentialityDocument.GetValue();

            //If the confidentiality document is being updated, show the upload div
            if (updateDocument == true) {
                $('#divConfidentialityChangeDate').removeClass('hidden');
                $('#divUploadConfidentiality').removeClass('hidden');
            }
            else {
                $('#divConfidentialityChangeDate').addClass('hidden');
                $('#divUploadConfidentiality').addClass('hidden');
            }
        }

        //Validate the confidentiality change date field
        function validateConfidentialityChangeDate(s, e) {
            //Get the change date
            var changeDate = e.value;

            //Determine if the user is updating the document
            var updateDocument = ddUpdateConfidentialityDocument.GetValue();

            //Validate the date
            if (updateDocument == true && changeDate == null) {
                e.isValid = false;
                e.errorText = "Document Change Date is required!";
            }
        }
    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <asp:UpdatePanel ID="upProgramManagement" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <ContentTemplate>
            <uc:Messaging ID="msgSys" runat="server" />
            <div id="divStates" runat="server" class="row">
                <div class="col-lg-12 mb-4">
                    <div class="card bg-light">
                        <div class="card-header">
                            States
                            <asp:LinkButton ID="lbAddState" runat="server" CssClass="btn btn-loader btn-primary float-right" OnClick="lbAddState_Click"><i class="fas fa-plus"></i>&nbsp;Add New State</asp:LinkButton>
                        </div>
                        <div class="card-body">
                            <div class="row">
                                <div class="col-lg-12">
                                    <label>All States</label>
                                    <dx:BootstrapGridView ID="bsGRStates" runat="server" EnableCallBacks="false" KeyFieldName="StatePK"
                                        AutoGenerateColumns="false" DataSourceID="efStateDataSource">
                                        <SettingsPager PageSize="15" />
                                        <SettingsBootstrap Striped="true" />
                                        <SettingsBehavior EnableRowHotTrack="true" />
                                        <SettingsSearchPanel Visible="true" ShowApplyButton="true" />
                                        <CssClasses IconHeaderSortUp="fas fa-long-arrow-alt-up" IconHeaderSortDown="fas fa-long-arrow-alt-down" IconShowAdaptiveDetailButton="fas fa-plus-circle" />
                                        <SettingsAdaptivity AdaptivityMode="HideDataCells" AllowOnlyOneAdaptiveDetailExpanded="true" AdaptiveColumnPosition="Left"></SettingsAdaptivity>
                                        <Columns>
                                            <dx:BootstrapGridViewTextColumn FieldName="Abbreviation" AdaptivePriority="3">
                                                <PropertiesTextEdit>
                                                    <ValidationSettings RequiredField-IsRequired="true" RequiredField-ErrorText="Abbreviation is required!"></ValidationSettings>
                                                </PropertiesTextEdit>
                                            </dx:BootstrapGridViewTextColumn>
                                            <dx:BootstrapGridViewTextColumn FieldName="Name" SortIndex="0" SortOrder="Ascending" AdaptivePriority="1">
                                                <PropertiesTextEdit>
                                                    <ValidationSettings RequiredField-IsRequired="true" RequiredField-ErrorText="Name is required!"></ValidationSettings>
                                                </PropertiesTextEdit>
                                            </dx:BootstrapGridViewTextColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="ConfidentialityEnabled" Caption="User Agreement Enabled?" AdaptivePriority="3">
                                                <DataItemTemplate>
                                                    <%# Convert.ToBoolean(Eval("ConfidentialityEnabled")) ? "Yes" : "No" %>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="ShareDataNationally" Caption="Share Data Nationally" AdaptivePriority="4">
                                                <DataItemTemplate>
                                                    <%# Convert.ToBoolean(Eval("ShareDataNationally")) ? "Yes" : "No" %>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="UtilizingPIDS" Caption="Utilizing PIDS" AdaptivePriority="7">
                                                <DataItemTemplate>
                                                    <%# Convert.ToBoolean(Eval("UtilizingPIDS")) ? "Yes" : "No" %>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="LockEndedPrograms" Caption="Lock Ended Programs" AdaptivePriority="6">
                                                <DataItemTemplate>
                                                    <%# Convert.ToBoolean(Eval("LockEndedPrograms")) ? "Yes" : "No" %>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="MaxNumberOfPrograms" Caption="Max Number of Programs" AdaptivePriority="5">
                                                <DataItemTemplate>
                                                    <%# Convert.ToInt32(Eval("MaxNumberOfPrograms")) == -1 ? "Unlimited" : Convert.ToInt32(Eval("MaxNumberOfPrograms")).ToString() %>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewImageColumn FieldName="LogoFilename" Caption="State Logo" AdaptivePriority="2">
                                                <PropertiesImage ImageUrlFormatString="~/Content/images/{0}" ImageHeight="30px" ImageWidth="30px" />
                                            </dx:BootstrapGridViewImageColumn>
                                            <dx:BootstrapGridViewImageColumn FieldName="ThumbnailLogoFilename" Caption="Thumbnail Logo" AdaptivePriority="3">
                                                <PropertiesImage ImageUrlFormatString="~/Content/images/{0}" ImageHeight="30px" ImageWidth="30px" />
                                            </dx:BootstrapGridViewImageColumn>
                                            <dx:BootstrapGridViewButtonEditColumn Settings-AllowDragDrop="False" AdaptivePriority="0" CssClasses-DataCell="text-center">
                                                <DataItemTemplate>
                                                    <div class="btn-group">
                                                        <button type="button" class="btn btn-secondary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                            Actions
                                                        </button>
                                                        <div class="dropdown-menu dropdown-menu-right">
                                                            <asp:HyperLink ID="lnkViewConfidentiality" runat="server" Visible='<%# Convert.ToBoolean(Eval("ConfidentialityEnabled")) %>' CssClass="dropdown-item" Target="_blank" NavigateUrl='<%# string.Format("/Pages/ViewFile.aspx?StatePK={0}", Eval("StatePK")) %>'><i class="fas fa-file-download"></i>&nbsp;View User Agreement</asp:HyperLink>
                                                            <asp:LinkButton ID="lbEditState" runat="server" CssClass="dropdown-item" OnClick="lbEditState_Click"><i class="fas fa-edit"></i> Edit</asp:LinkButton>
                                                            <button class="dropdown-item delete-gridview" data-pk='<%# Eval("StatePK") %>' data-hf="hfDeleteStatePK" data-target="#divDeleteStateModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                        </div>
                                                    </div>
                                                    <asp:HiddenField ID="hfStatePK" runat="server" Value='<%# Eval("StatePK") %>' />
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewButtonEditColumn>
                                        </Columns>
                                    </dx:BootstrapGridView>
                                    <dx:EntityServerModeDataSource ID="efStateDataSource" runat="server"
                                        OnSelecting="efStateDataSource_Selecting" />
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-12">
                                    <div id="divAddEditState" runat="server" class="card mt-2" visible="false">
                                        <div class="card-header">
                                            <asp:Label ID="lblAddEditState" runat="server" Text=""></asp:Label>
                                        </div>
                                        <div class="card-body">
                                            <!--
                                            <div class="row">
                                                <div class="col-lg-12">
                                                    <div class="alert alert-warning">
                                                        <p>
                                                            <i class="fas fa-exclamation-circle"></i> &nbsp;All states should have confidentiality enabled!
                                                        </p>
                                                        <p>
                                                            Please ensure that you upload the correct confidentiality document!
                                                        </p>
                                                    </div>
                                                </div>
                                            </div>
                                            -->
                                            <div class="row">
                                                <div class="col-lg-4">
                                                    <dx:BootstrapTextBox ID="txtStateAbbreviation" runat="server" Caption="Abbreviation" MaxLength="2">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <MaskSettings Mask="AA" PromptChar=" " ErrorText="Abbreviation must be two letters!" />
                                                        <ValidationSettings ValidationGroup="vgState" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="true" ErrorText="Abbreviation is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapTextBox>
                                                </div>
                                                <div class="col-lg-6">
                                                    <dx:BootstrapTextBox ID="txtStateName" runat="server" Caption="Name">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgState" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="true" ErrorText="Name is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapTextBox>
                                                </div>
                                            </div>
                                            <div class="row">
                                                <div class="col-lg-4">
                                                    <dx:BootstrapTextBox ID="txtStateLogoFilePath" runat="server" Caption="State Logo File Path">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgState" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="true" ErrorText="State Logo File Path is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapTextBox>
                                                </div>
                                                <div class="col-lg-4">
                                                    <dx:BootstrapTextBox ID="txtStateThumbnailLogoFilePath" runat="server" Caption="Thumbnail Logo File Path">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgState" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="true" ErrorText="Thumbnail Logo File Path is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapTextBox>
                                                </div>
                                                <div class="col-lg-4">
                                                    <dx:BootstrapComboBox ID="ddStateHomePageLogoOption" runat="server" NullText="--Select--" Caption="Home Page Logo Options"
                                                        ValueType="System.Int32" IncrementalFilteringMode="StartsWith">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgState">
                                                            <RequiredField IsRequired="true" ErrorText="Home Page Logo Options is required!" />
                                                        </ValidationSettings>
                                                        <Items>
                                                            <dx:BootstrapListEditItem Text="Display both logos" Value="1"></dx:BootstrapListEditItem>
                                                            <dx:BootstrapListEditItem Text="Display only state logo" Value="2"></dx:BootstrapListEditItem>
                                                            <dx:BootstrapListEditItem Text="Display only thumbnail logo" Value="3"></dx:BootstrapListEditItem>
                                                        </Items>
                                                    </dx:BootstrapComboBox>
                                                </div>
                                            </div>
                                            <div class="row">
                                                <div class="col-lg-3">
                                                    <dx:BootstrapComboBox ID="ddStateConfidentialityEnabled" runat="server" NullText="--Select--" Caption="User Agreement Enabled"
                                                        ValueType="System.Boolean" IncrementalFilteringMode="StartsWith" ClientInstanceName="ddStateConfidentialityEnabled">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ClientSideEvents SelectedIndexChanged="showHideConfidentialityDivs" />
                                                        <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgState">
                                                            <RequiredField IsRequired="true" ErrorText="User Agreement Enabled is required!" />
                                                        </ValidationSettings>
                                                        <Items>
                                                            <dx:BootstrapListEditItem Text="Yes" Value="True"></dx:BootstrapListEditItem>
                                                            <dx:BootstrapListEditItem Text="No" Value="False"></dx:BootstrapListEditItem>
                                                        </Items>
                                                    </dx:BootstrapComboBox>
                                                </div>
                                                <div class="col-lg-3">
                                                    <dx:BootstrapComboBox ID="ddStateShareDataNationally" runat="server" NullText="--Select--" Caption="Share Data Nationally"
                                                        ValueType="System.Boolean" IncrementalFilteringMode="StartsWith">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgState">
                                                            <RequiredField IsRequired="true" ErrorText="Share Data Nationally is required!" />
                                                        </ValidationSettings>
                                                        <Items>
                                                            <dx:BootstrapListEditItem Text="Yes" Value="True"></dx:BootstrapListEditItem>
                                                            <dx:BootstrapListEditItem Text="No" Value="False"></dx:BootstrapListEditItem>
                                                        </Items>
                                                    </dx:BootstrapComboBox>
                                                </div>
                                                <div class="col-lg-3">
                                                    <dx:BootstrapComboBox ID="ddStateUtilizingPIDS" runat="server" NullText="--Select--" Caption="Utilizing PIDS"
                                                        ValueType="System.Boolean" IncrementalFilteringMode="StartsWith">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgState">
                                                            <RequiredField IsRequired="true" ErrorText="Utilizing PIDS is required!" />
                                                        </ValidationSettings>
                                                        <Items>
                                                            <dx:BootstrapListEditItem Text="Yes" Value="True"></dx:BootstrapListEditItem>
                                                            <dx:BootstrapListEditItem Text="No" Value="False"></dx:BootstrapListEditItem>
                                                        </Items>
                                                    </dx:BootstrapComboBox>
                                                </div>
                                                <div class="col-lg-3">
                                                    <dx:BootstrapComboBox ID="ddStateLockEndedPrograms" runat="server" NullText="--Select--" Caption="Lock Ended Programs"
                                                        ValueType="System.Boolean" IncrementalFilteringMode="StartsWith">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgState">
                                                            <RequiredField IsRequired="true" ErrorText="Lock Ended Programs is required!" />
                                                        </ValidationSettings>
                                                        <Items>
                                                            <dx:BootstrapListEditItem Text="Yes" Value="True"></dx:BootstrapListEditItem>
                                                            <dx:BootstrapListEditItem Text="No" Value="False"></dx:BootstrapListEditItem>
                                                        </Items>
                                                    </dx:BootstrapComboBox>
                                                </div>
                                            </div>
                                            <div class="row">
                                                <div id="divUpdateConfidentiality" class="col-lg-4 hidden">
                                                    <dx:BootstrapComboBox ID="ddUpdateConfidentialityDocument" runat="server" NullText="--Select--" Caption="Update User Agreement Document?"
                                                        ValueType="System.Boolean" IncrementalFilteringMode="StartsWith" ClientInstanceName="ddUpdateConfidentialityDocument">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ClientSideEvents SelectedIndexChanged="showHideConfidentialityDivs" Init="showHideConfidentialityDivs" />
                                                        <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgState">
                                                            <RequiredField IsRequired="true" ErrorText="Update User Agreement Document is required!" />
                                                        </ValidationSettings>
                                                        <Items>
                                                            <dx:BootstrapListEditItem Text="Yes" Value="True"></dx:BootstrapListEditItem>
                                                            <dx:BootstrapListEditItem Text="No" Value="False"></dx:BootstrapListEditItem>
                                                        </Items>
                                                    </dx:BootstrapComboBox>
                                                    <asp:HyperLink ID="bsLnkCurrentDocument" runat="server" Visible="false" NavigateUrl="/Pages/ViewFile.aspx" CssClass="btn btn-primary mt-3" Target="_blank"><i class="fas fa-file-download"></i>&nbsp;Current Document</asp:HyperLink>
                                                </div>
                                                <div id="divConfidentialityChangeDate" class="col-lg-4 hidden">
                                                    <dx:BootstrapDateEdit ID="deConfidentialityChangeDate" runat="server" Caption="Document Change Date" AllowNull="true"
                                                        EditFormatString="MM/dd/yyyy" UseMaskBehavior="true" ClientInstanceName="deConfidentialityChangeDate"
                                                        AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900"
                                                        OnValidation="deConfidentialityChangeDate_Validation">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ClientSideEvents Validation="validateConfidentialityChangeDate" />
                                                        <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                                        <ValidationSettings ValidationGroup="vgState" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                            <RequiredField IsRequired="false" ErrorText="Document Change Date is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapDateEdit>
                                                </div>
                                                <div id="divUploadConfidentiality" class="col-lg-4 hidden">
                                                    <label id="lblUploadControl">User Agreement Document</label>
                                                    <dx:BootstrapUploadControl ID="bucConfidentialityDocument" runat="server" ShowUploadButton="false" aria-labelledby="lblUploadControl">
                                                        <ValidationSettings MaxFileSize="20000000" AllowedFileExtensions=".pdf" />
                                                    </dx:BootstrapUploadControl>
                                                    <small>Allowed file extensions: .pdf</small>
                                                    <br />
                                                    <small>Maximum file size: 20 MB.</small>
                                                </div>
                                            </div>
                                            <div class="row">
                                                <div class="col-lg-4">
                                                    <dx:BootstrapTextBox ID="txtStateMaxNumberOfPrograms" runat="server" Caption="Maximum Number of Programs"
                                                        OnValidation="txtStateMaxNumberOfPrograms_Validation" HelpText="-1 = Unlimited">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgState" ErrorDisplayMode="ImageWithText">
                                                            <RegularExpression ValidationExpression="(-[1])|\d+" ErrorText="Must be a positive integer or -1 if you want the state to have unlimited programs!" />
                                                            <RequiredField IsRequired="true" ErrorText="Required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapTextBox>
                                                </div>
                                                <div class="col-lg-4">
                                                    <dx:BootstrapMemo ID="txtStateCatchphrase" runat="server" Caption="Catchphrase" Rows="2">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgState" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="false" ErrorText="Catchphrase is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapMemo>
                                                </div>
                                                <div class="col-lg-4">
                                                    <dx:BootstrapMemo ID="txtStateDisclaimer" runat="server" Caption="Disclaimer" Rows="5">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgState" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="false" ErrorText="Disclaimer is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapMemo>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="card-footer">
                                            <div class="d-flex align-items-center justify-content-center">
                                                <asp:HiddenField ID="hfAddEditStatePK" runat="server" Value="0" />
                                                <uc:Submit ID="submitState" runat="server" ValidationGroup="vgState"
                                                    ControlCssClass="center-content"
                                                    OnSubmitClick="submitState_Click" OnCancelClick="submitState_CancelClick"
                                                    OnValidationFailed="submitState_ValidationFailed" />
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>

                        </div>
                    </div>
                </div>
            </div>
            <div class="row">
                <div class="col-lg-6 mb-4">
                    <div class="card bg-light">
                        <div class="card-header">
                            Hubs
                            <asp:LinkButton ID="lbAddHub" runat="server" CssClass="btn btn-loader btn-primary float-right" OnClick="lbAddHub_Click"><i class="fas fa-plus"></i>&nbsp;Add New Hub</asp:LinkButton>
                        </div>
                        <div class="card-body">
                            <div class="row">
                                <div class="col-lg-12">
                                    <div class="alert alert-primary">
                                        <i class="fas fa-info-circle"></i>&nbsp;Please note that community leadership teams and Community-Wide Benchmarks of Quality forms are linked directly to specific hubs. Hub users are able to manage both the teams and BOQ forms for their hub.
                                    </div>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-12">
                                    <label>All Hubs</label>
                                    <dx:BootstrapGridView ID="bsGRHubs" runat="server" EnableCallBacks="false" KeyFieldName="HubPK"
                                        AutoGenerateColumns="false" DataSourceID="efHubDataSource">
                                        <SettingsPager PageSize="15" />
                                        <SettingsBootstrap Striped="true" />
                                        <SettingsBehavior EnableRowHotTrack="true" />
                                        <SettingsSearchPanel Visible="true" ShowApplyButton="true" />
                                        <Settings ShowGroupPanel="true" />
                                        <CssClasses IconHeaderSortUp="fas fa-long-arrow-alt-up" IconHeaderSortDown="fas fa-long-arrow-alt-down" IconShowAdaptiveDetailButton="fas fa-plus-circle" />
                                        <SettingsAdaptivity AdaptivityMode="HideDataCells" AllowOnlyOneAdaptiveDetailExpanded="true" AdaptiveColumnPosition="Left"></SettingsAdaptivity>
                                        <Columns>
                                            <dx:BootstrapGridViewDataColumn FieldName="Name" SortIndex="0" SortOrder="Ascending" AdaptivePriority="0" />
                                            <dx:BootstrapGridViewDataColumn FieldName="State.Name" Caption="State" AdaptivePriority="1" />
                                            <dx:BootstrapGridViewButtonEditColumn Settings-AllowDragDrop="False" AdaptivePriority="0" CssClasses-DataCell="text-center">
                                                <DataItemTemplate>
                                                    <div class="btn-group">
                                                        <button type="button" class="btn btn-secondary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                            Actions
                                                        </button>
                                                        <div class="dropdown-menu dropdown-menu-right">
                                                            <asp:LinkButton ID="lbEditHub" runat="server" CssClass="dropdown-item" OnClick="lbEditHub_Click"><i class="fas fa-edit"></i> Edit</asp:LinkButton>
                                                            <button class="dropdown-item delete-gridview" data-pk='<%# Eval("HubPK") %>' data-hf="hfDeleteHubPK" data-target="#divDeleteHubModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                        </div>
                                                    </div>
                                                    <asp:HiddenField ID="hfHubPK" runat="server" Value='<%# Eval("HubPK") %>' />
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewButtonEditColumn>
                                        </Columns>
                                    </dx:BootstrapGridView>
                                    <dx:EntityServerModeDataSource ID="efHubDataSource" runat="server"
                                        OnSelecting="efHubDataSource_Selecting" />
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-12">
                                    <div id="divAddEditHub" runat="server" class="card mt-2" visible="false">
                                        <div class="card-header">
                                            <asp:Label ID="lblAddEditHub" runat="server" Text=""></asp:Label>
                                        </div>
                                        <div class="card-body">
                                            <div class="row">
                                                <div class="col-lg-4">
                                                    <dx:BootstrapTextBox ID="txtHubName" runat="server" Caption="Hub Name">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgHub" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="true" ErrorText="Hub Name is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapTextBox>
                                                </div>
                                                <div class="col-lg-4">
                                                    <dx:BootstrapComboBox ID="ddHubState" runat="server" Caption="State" NullText="--Select--"
                                                        TextField="Name" ValueField="StatePK" ValueType="System.Int32"
                                                        IncrementalFilteringMode="Contains" AllowMouseWheel="false">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgHub" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="true" ErrorText="State is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapComboBox>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="card-footer">
                                            <div class="d-flex align-items-center justify-content-center">
                                                <asp:HiddenField ID="hfAddEditHubPK" runat="server" Value="0" />
                                                <uc:Submit ID="submitHub" runat="server" ValidationGroup="vgHub"
                                                    ControlCssClass="center-content"
                                                    OnSubmitClick="submitHub_Click" OnCancelClick="submitHub_CancelClick"
                                                    OnValidationFailed="submitHub_ValidationFailed" />
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="col-lg-6 mb-4">
                    <div class="card bg-light">
                        <div class="card-header">
                            Cohorts
                            <asp:LinkButton ID="lbAddCohort" runat="server" CssClass="btn btn-loader btn-primary float-right" OnClick="lbAddCohort_Click"><i class="fas fa-plus"></i>&nbsp;Add New Cohort</asp:LinkButton>
                        </div>
                        <div class="card-body">
                            <div class="row">
                                <div class="col-lg-12">
                                    <label>All Cohorts</label>
                                    <dx:BootstrapGridView ID="bsGRCohort" runat="server" EnableCallBacks="false" KeyFieldName="CohortPK"
                                        AutoGenerateColumns="false" DataSourceID="efCohortDataSource">
                                        <SettingsPager PageSize="15" />
                                        <SettingsBootstrap Striped="true" />
                                        <SettingsBehavior EnableRowHotTrack="true" />
                                        <SettingsSearchPanel Visible="true" ShowApplyButton="true" />
                                        <Settings ShowGroupPanel="true" />
                                        <CssClasses IconHeaderSortUp="fas fa-long-arrow-alt-up" IconHeaderSortDown="fas fa-long-arrow-alt-down" IconShowAdaptiveDetailButton="fas fa-plus-circle" />
                                        <SettingsAdaptivity AdaptivityMode="HideDataCells" AllowOnlyOneAdaptiveDetailExpanded="true" AdaptiveColumnPosition="Left"></SettingsAdaptivity>
                                        <Columns>
                                            <dx:BootstrapGridViewDataColumn FieldName="CohortName" SortIndex="0" SortOrder="Ascending" AdaptivePriority="0" />
                                            <dx:BootstrapGridViewDateColumn FieldName="StartDate" PropertiesDateEdit-DisplayFormatString="MM/dd/yyyy" AdaptivePriority="1" />
                                            <dx:BootstrapGridViewDateColumn FieldName="EndDate" PropertiesDateEdit-DisplayFormatString="MM/dd/yyyy" AdaptivePriority="2" />
                                            <dx:BootstrapGridViewDataColumn FieldName="State.Name" Caption="State" AdaptivePriority="3" />
                                            <dx:BootstrapGridViewButtonEditColumn Settings-AllowDragDrop="False" AdaptivePriority="0" CssClasses-DataCell="text-center">
                                                <DataItemTemplate>
                                                    <div class="btn-group">
                                                        <button type="button" class="btn btn-secondary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                            Actions
                                                        </button>
                                                        <div class="dropdown-menu dropdown-menu-right">
                                                            <asp:LinkButton ID="lbEditCohort" runat="server" CssClass="dropdown-item" OnClick="lbEditCohort_Click"><i class="fas fa-edit"></i> Edit</asp:LinkButton>
                                                            <button class="dropdown-item delete-gridview" data-pk='<%# Eval("CohortPK") %>' data-hf="hfDeleteCohortPK" data-target="#divDeleteCohortModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                        </div>
                                                    </div>
                                                    <asp:HiddenField ID="hfCohortPK" runat="server" Value='<%# Eval("CohortPK") %>' />
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewButtonEditColumn>
                                        </Columns>
                                    </dx:BootstrapGridView>
                                    <dx:EntityServerModeDataSource ID="efCohortDataSource" runat="server"
                                        OnSelecting="efCohortDataSource_Selecting" />
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-12">
                                    <div id="divAddEditCohort" runat="server" class="card mt-2" visible="false">
                                        <div class="card-header">
                                            <asp:Label ID="lblAddEditCohort" runat="server" Text=""></asp:Label>
                                        </div>
                                        <div class="card-body">
                                            <div class="row">
                                                <div class="col-xl-3 col-lg-6">
                                                    <dx:BootstrapTextBox ID="txtCohortName" runat="server" Caption="Cohort Name">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgCohort" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="true" ErrorText="Cohort Name is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapTextBox>
                                                </div>
                                                <div class="col-xl-3 col-lg-6">
                                                    <dx:BootstrapDateEdit ID="deCohortStartDate" runat="server" Caption="Cohort Start Date"
                                                        EditFormatString="MM/dd/yyyy" UseMaskBehavior="true" ClientInstanceName="deCohortStartDate"
                                                        AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                                        <ValidationSettings ValidationGroup="vgCohort" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="true" ErrorText="Cohort Start Date is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapDateEdit>
                                                </div>
                                                <div class="col-xl-3 col-lg-6">
                                                    <dx:BootstrapDateEdit ID="deCohortEndDate" runat="server" Caption="Cohort End Date" AllowNull="true"
                                                        EditFormatString="MM/dd/yyyy" UseMaskBehavior="true" ClientInstanceName="deCohortEndDate"
                                                        OnValidation="deCohortEndDate_Validation"
                                                        AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                                        <ClientSideEvents Validation="validateCohortEndDate" />
                                                        <ValidationSettings ValidationGroup="vgCohort" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                            <RequiredField IsRequired="false" ErrorText="Cohort End Date is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapDateEdit>
                                                </div>
                                                <div class="col-xl-3 col-lg-6">
                                                    <dx:BootstrapComboBox ID="ddCohortState" runat="server" Caption="State" NullText="--Select--"
                                                        TextField="Name" ValueField="StatePK" ValueType="System.Int32"
                                                        IncrementalFilteringMode="Contains" AllowMouseWheel="false">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgCohort" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="true" ErrorText="State is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapComboBox>
                                                </div>
                                            </div>

                                        </div>
                                        <div class="card-footer">
                                            <div class="col-lg-12 d-flex align-items-center justify-content-center">
                                                <asp:HiddenField ID="hfAddEditCohortPK" runat="server" Value="0" />
                                                <uc:Submit ID="submitCohort" runat="server" ValidationGroup="vgCohort"
                                                    ControlCssClass="center-content"
                                                    OnSubmitClick="submitCohort_Click" OnCancelClick="submitCohort_CancelClick"
                                                    OnValidationFailed="submitCohort_ValidationFailed" />
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div class="row">
                <div class="col-lg-12 mb-4">
                    <asp:HiddenField ID="hfDeleteStatePK" runat="server" Value="0" />
                    <asp:HiddenField ID="hfDeleteHubPK" runat="server" Value="0" />
                    <asp:HiddenField ID="hfDeleteCohortPK" runat="server" Value="0" />
                    <asp:HiddenField ID="hfDeleteProgramPK" runat="server" Value="0" />
                    <asp:HiddenField ID="hfStateProgramCount" runat="server" Value="0" />
                    <asp:HiddenField ID="hfMaxStateProgramCount" runat="server" Value="0" />
                    <div class="card bg-light">
                        <div class="card-header">
                            Programs
                            <asp:LinkButton ID="lbAddProgram" runat="server" CssClass="btn btn-loader btn-primary float-right" OnClick="lbAddProgram_Click"><i class="fas fa-plus"></i>&nbsp;Add New Program</asp:LinkButton>
                        </div>
                        <div class="card-body">
                            <div id="divProgramCountAlert" class="alert">
                                <i id="iconProgramCountAlert" class="fas"></i>&nbsp;<span id="spanProgramCountAlertText"></span>
                            </div>
                            <div class="row">
                                <div class="col-lg-12">
                                    <label>All Programs</label>
                                    <dx:BootstrapGridView ID="bsGRPrograms" runat="server" EnableCallBacks="false" KeyFieldName="ProgramPK"
                                        AutoGenerateColumns="false" DataSourceID="efProgramDataSource">
                                        <SettingsPager PageSize="15" />
                                        <SettingsBootstrap Striped="true" />
                                        <SettingsBehavior EnableRowHotTrack="true" />
                                        <SettingsSearchPanel Visible="true" ShowApplyButton="true" />
                                        <Settings ShowGroupPanel="true" />
                                        <CssClasses IconHeaderSortUp="fas fa-long-arrow-alt-up" IconHeaderSortDown="fas fa-long-arrow-alt-down" IconShowAdaptiveDetailButton="fas fa-plus-circle" />
                                        <SettingsAdaptivity AdaptivityMode="HideDataCells" AllowOnlyOneAdaptiveDetailExpanded="true" AdaptiveColumnPosition="Left"></SettingsAdaptivity>
                                        <Columns>
                                            <dx:BootstrapGridViewDataColumn FieldName="ProgramName" Caption="Name" SortIndex="0" SortOrder="Ascending" AdaptivePriority="0" />
                                            <dx:BootstrapGridViewDateColumn FieldName="IDNumber" Caption="ID Number" AdaptivePriority="1" />
                                            <dx:BootstrapGridViewDateColumn FieldName="Location" Caption="Location" AdaptivePriority="3" />
                                            <dx:BootstrapGridViewDataColumn FieldName="CohortName" Caption="Cohort" AdaptivePriority="4" />
                                            <dx:BootstrapGridViewDataColumn FieldName="HubName" Caption="Hub" AdaptivePriority="5" />
                                            <dx:BootstrapGridViewDataColumn FieldName="StateName" Caption="State" AdaptivePriority="6" />
                                            <dx:BootstrapGridViewDataColumn FieldName="ProgramStatus" Caption="Status" AdaptivePriority="9" />
                                            <dx:BootstrapGridViewDateColumn FieldName="ProgramStartDate" Caption="Start Date" PropertiesDateEdit-DisplayFormatString="MM/dd/yyyy" AdaptivePriority="8" />
                                            <dx:BootstrapGridViewDateColumn FieldName="ProgramEndDate" Caption="End Date" PropertiesDateEdit-DisplayFormatString="MM/dd/yyyy" AdaptivePriority="2" />
                                            <dx:BootstrapGridViewDataColumn FieldName="ProgramTypes" Width="30%" Caption="Type(s)" AdaptivePriority="7" Settings-AllowFilterBySearchPanel="False" Settings-AllowSort="False">
                                                <DataItemTemplate>
                                                    <%# string.Join(", ", (IEnumerable<string>)Eval("ProgramTypes")) %>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewButtonEditColumn Settings-AllowDragDrop="False" AdaptivePriority="0" CssClasses-DataCell="text-center">
                                                <DataItemTemplate>
                                                    <div class="btn-group">
                                                        <button type="button" class="btn btn-secondary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                            Actions
                                                        </button>
                                                        <div class="dropdown-menu dropdown-menu-right">
                                                            <asp:LinkButton ID="lbEditProgram" runat="server" CssClass="dropdown-item" OnClick="lbEditProgram_Click"><i class="fas fa-edit"></i> Edit</asp:LinkButton>
                                                            <button class="dropdown-item delete-gridview" data-pk='<%# Eval("ProgramPK") %>' data-hf="hfDeleteProgramPK" data-target="#divDeleteProgramModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                        </div>
                                                    </div>
                                                    <asp:HiddenField ID="hfProgramPK" runat="server" Value='<%# Eval("ProgramPK") %>' />
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewButtonEditColumn>
                                        </Columns>
                                    </dx:BootstrapGridView>
                                    <dx:EntityServerModeDataSource ID="efProgramDataSource" runat="server"
                                        OnSelecting="efProgramDataSource_Selecting" />
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-12">
                                    <div id="divAddEditProgram" runat="server" class="card mt-2" visible="false">
                                        <div class="card-header">
                                            <asp:Label ID="lblAddEditProgram" runat="server" Text=""></asp:Label>
                                        </div>
                                        <div class="card-body">
                                            <div class="row">
                                                <div class="col-lg-4">
                                                    <dx:BootstrapTextBox ID="txtProgramName" runat="server" Caption="Program Name">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgProgram" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="true" ErrorText="Program Name is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapTextBox>
                                                </div>
                                                <div class="col-lg-4">
                                                    <dx:BootstrapTextBox ID="txtProgramLocation" runat="server" Caption="Program Location">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgProgram" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="true" ErrorText="Program Location is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapTextBox>
                                                </div>
                                                <div class="col-lg-4">
                                                    <dx:BootstrapTextBox ID="txtProgramIDNumber" runat="server" Caption="Program ID Number" OnValidation="txtProgramIDNumber_Validation">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgProgram" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                            <RequiredField IsRequired="false" ErrorText="Program ID Number is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapTextBox>
                                                </div>
                                            </div>
                                            <div class="row">
                                                <div class="col-lg-4">
                                                    <dx:BootstrapDateEdit ID="deProgramStartDate" runat="server" Caption="Program Start Date"
                                                        EditFormatString="MM/dd/yyyy" UseMaskBehavior="true" ClientInstanceName="deProgramStartDate"
                                                        AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                                        <ValidationSettings ValidationGroup="vgProgram" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="true" ErrorText="Program Start Date is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapDateEdit>
                                                </div>
                                                <div class="col-lg-4">
                                                    <dx:BootstrapDateEdit ID="deProgramEndDate" runat="server" Caption="Program End Date" AllowNull="true"
                                                        EditFormatString="MM/dd/yyyy" UseMaskBehavior="true" ClientInstanceName="deProgramEndDate"
                                                        OnValidation="deProgramEndDate_Validation"
                                                        AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                                        <ClientSideEvents Validation="validateProgramEndDate" />
                                                        <ValidationSettings ValidationGroup="vgProgram" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                            <RequiredField IsRequired="false" ErrorText="Program End Date is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapDateEdit>
                                                </div>
                                            </div>
                                            <div class="row">
                                                <div class="col-lg-4">
                                                    <dx:BootstrapComboBox ID="ddProgramCohort" runat="server" Caption="Cohort" NullText="--Select--"
                                                        TextField="CohortName" ValueField="CohortPK" ValueType="System.Int32"
                                                        IncrementalFilteringMode="Contains" AllowMouseWheel="false" OnValidation="ddProgramCohort_Validation">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgProgram" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                            <RequiredField IsRequired="true" ErrorText="Cohort is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapComboBox>
                                                </div>
                                                <div class="col-lg-4">
                                                    <dx:BootstrapComboBox ID="ddProgramHub" runat="server" Caption="Hub" NullText="--Select--"
                                                        TextField="Name" ValueField="HubPK" ValueType="System.Int32"
                                                        IncrementalFilteringMode="Contains" AllowMouseWheel="false" OnValidation="ddProgramHub_Validation">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgProgram" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                            <RequiredField IsRequired="true" ErrorText="Hub is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapComboBox>
                                                </div>
                                                <div class="col-lg-4">
                                                    <dx:BootstrapComboBox ID="ddProgramState" runat="server" Caption="State" NullText="--Select--"
                                                        TextField="Name" ValueField="StatePK" ValueType="System.Int32"
                                                        IncrementalFilteringMode="Contains" AllowMouseWheel="false" OnValidation="ddProgramState_Validation">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgProgram" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="true" ErrorText="State is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapComboBox>
                                                </div>
                                                <div class="col-lg-4">
                                                    <dx:BootstrapListBox ID="lstBxProgramType" runat="server" Caption="Program Type"
                                                        SelectionMode="CheckColumn" EnableSelectAll="false"
                                                        ValueField="CodeProgramTypePK" ValueType="System.Int32" TextField="Description">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <FilteringSettings ShowSearchUI="true" EditorNullTextDisplayMode="Unfocused" />
                                                        <ValidationSettings ValidationGroup="vgProgram" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="true" ErrorText="At least one program type must be selected!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapListBox>
                                                </div>
                                            </div>
                                            <div id="divInitialProgramStatus" runat="server" class="row">
                                                <div class="col-lg-4">
                                                    <dx:BootstrapComboBox ID="ddProgramInitialStatus" runat="server" Caption="Program Status" NullText="--Select--"
                                                        TextField="Description" ValueField="CodeProgramStatusPK" ValueType="System.Int32"
                                                        IncrementalFilteringMode="Contains" AllowMouseWheel="false">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgProgram" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="true" ErrorText="Status is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapComboBox>
                                                </div>
                                                <div class="col-lg-4">
                                                    <dx:BootstrapDateEdit ID="deProgramInitialStatusDate" runat="server" Caption="Program Status Date" AllowNull="true"
                                                        EditFormatString="MM/dd/yyyy" UseMaskBehavior="true" ClientInstanceName="deProgramInitialStatusDate"
                                                        AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900" OnValidation="deProgramInitialStatusDate_Validation">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                                        <ValidationSettings ValidationGroup="vgProgram" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="true" ErrorText="Program Status Date is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapDateEdit>
                                                </div>
                                                <div class="col-lg-4">
                                                    <dx:BootstrapMemo ID="txtProgramInitialStatusDetails" runat="server" Caption="Program Status Details (optional)" MaxLength="1000" Rows="3">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgProgram" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="false" ErrorText="Program Status Details are required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapMemo>
                                                </div>
                                            </div>
                                            <div id="divProgramStatusHistory" runat="server" class="row">
                                                <div class="col-xl-12">
                                                    <asp:HiddenField ID="hfDeleteStatusDatePK" runat="server" Value="0" />
                                                    <div class="card bg-light">
                                                        <div class="card-header">
                                                            Program Status
                                                            <asp:LinkButton ID="lbAddProgramStatus" runat="server" CssClass="btn btn-loader btn-primary float-right " OnClick="lbAddProgramStatus_Click"><i class="fas fa-plus"></i>&nbsp;Add New Program Status</asp:LinkButton>
                                                        </div>
                                                        <div class="card-body">
                                                            <div class="row">
                                                                <div class="col-lg-12">
                                                                    <label>All status records for this program</label>
                                                                    <asp:Repeater ID="repeatProgramStatuses" runat="server" ItemType="Pyramid.Models.ProgramStatus">
                                                                        <HeaderTemplate>
                                                                            <table id="tblProgramStatuses" class="table table-striped table-bordered table-hover">
                                                                                <thead>
                                                                                    <tr>
                                                                                        <th data-priority="1"></th>
                                                                                        <th data-priority="3">Status</th>
                                                                                        <th data-priority="4">Status Date</th>
                                                                                        <th data-priority="5">Status Details</th>
                                                                                        <th data-priority="2"></th>
                                                                                    </tr>
                                                                                </thead>
                                                                                <tbody>
                                                                        </HeaderTemplate>
                                                                        <ItemTemplate>
                                                                            <tr>
                                                                                <td></td>
                                                                                <td><%# Item.CodeProgramStatus.Description %></td>
                                                                                <td><%# Item.StatusDate.ToString("MM/dd/yyyy") %></td>
                                                                                <td><%# Item.StatusDetails %></td>
                                                                                <td class="text-center">
                                                                                    <div class="btn-group">
                                                                                        <button type="button" class="btn btn-secondary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                                                            Actions
                                                                                        </button>
                                                                                        <div class="dropdown-menu dropdown-menu-right">
                                                                                            <asp:LinkButton ID="lbViewProgramStatus" runat="server" CssClass="dropdown-item" OnClick="lbViewProgramStatus_Click"><i class="fas fa-list"></i>&nbsp;View Details</asp:LinkButton>
                                                                                            <asp:LinkButton ID="lbEditProgramStatus" runat="server" CssClass="dropdown-item" OnClick="lbEditProgramStatus_Click"><i class="fas fa-edit"></i>&nbsp;Edit</asp:LinkButton>
                                                                                            <button class="dropdown-item delete-gridview" data-pk='<%# Item.ProgramStatusPK %>' data-hf="hfDeleteStatusDatePK" data-target="#divDeleteStatusModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                                                        </div>
                                                                                    </div>
                                                                                    <asp:Label ID="lblProgramStatusPK" runat="server" Visible="false" Text='<%# Item.ProgramStatusPK %>'></asp:Label>
                                                                                </td>
                                                                            </tr>
                                                                        </ItemTemplate>
                                                                        <FooterTemplate>
                                                                            </tbody>
                                                                                </table>
                                                                        </FooterTemplate>
                                                                    </asp:Repeater>
                                                                </div>
                                                            </div>
                                                            <div class="row">
                                                                <div class="col-lg-12">
                                                                    <div id="divAddEditProgramStatus" runat="server" class="card mt-2" visible="false">
                                                                        <div class="card-header">
                                                                            <asp:Label ID="lblAddEditProgramStatus" runat="server" Text=""></asp:Label>
                                                                        </div>
                                                                        <div class="card-body">
                                                                            <div class="row">
                                                                                <div class="col-lg-4">
                                                                                    <dx:BootstrapComboBox ID="ddProgramStatus" runat="server" Caption="Status" NullText="--Select--" ValueType="System.Int32"
                                                                                        TextField="Description" ValueField="CodeProgramStatusPK"
                                                                                        IncrementalFilteringMode="Contains" AllowMouseWheel="false">
                                                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                                                        <ValidationSettings ValidationGroup="vgStatus" ErrorDisplayMode="ImageWithText">
                                                                                            <RequiredField IsRequired="true" ErrorText="Status is required!" />
                                                                                        </ValidationSettings>
                                                                                    </dx:BootstrapComboBox>
                                                                                </div>
                                                                                <div class="col-lg-4">
                                                                                    <dx:BootstrapDateEdit ID="deProgramStatusDate" runat="server" Caption="Status Date" EditFormat="Date"
                                                                                        EditFormatString="MM/dd/yyyy" UseMaskBehavior="true" NullText="--Select--"
                                                                                        AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900" OnValidation="deProgramStatusDate_Validation">
                                                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                                                        <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                                                                        <ValidationSettings ValidationGroup="vgStatus" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                                                            <RequiredField IsRequired="true" ErrorText="Status Date is required!" />
                                                                                        </ValidationSettings>
                                                                                    </dx:BootstrapDateEdit>
                                                                                </div>
                                                                                <div class="col-lg-4">
                                                                                    <dx:BootstrapMemo ID="txtProgramStatusDetails" runat="server" Caption="Status Details (optional)" MaxLength="1000" Rows="3">
                                                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                                                        <ValidationSettings ValidationGroup="vgStatus" ErrorDisplayMode="ImageWithText">
                                                                                            <RequiredField IsRequired="false" ErrorText="Status Details are required!" />
                                                                                        </ValidationSettings>
                                                                                    </dx:BootstrapMemo>
                                                                                </div>
                                                                            </div>
                                                                        </div>
                                                                        <div class="card-footer">
                                                                            <div class="center-content">
                                                                                <asp:HiddenField ID="hfAddEditStatusPK" runat="server" Value="0" />
                                                                                <uc:Submit ID="submitStatus" runat="server"
                                                                                    ValidationGroup="vgStatus"
                                                                                    ControlCssClass="center-content"
                                                                                    OnSubmitClick="submitStatus_Click"
                                                                                    OnCancelClick="submitStatus_CancelClick"
                                                                                    OnValidationFailed="submitStatus_ValidationFailed" />
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
                                        <div class="card-footer">
                                            <div class="d-flex align-items-center justify-content-center">
                                                <asp:HiddenField ID="hfAddEditProgramPK" runat="server" Value="0" />
                                                <uc:Submit ID="submitProgram" runat="server" ValidationGroup="vgProgram"
                                                    ControlCssClass="center-content"
                                                    OnSubmitClick="submitProgram_Click" OnCancelClick="submitProgram_CancelClick"
                                                    OnValidationFailed="submitProgram_ValidationFailed" />
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
            <asp:PostBackTrigger ControlID="submitState" />
            <asp:AsyncPostBackTrigger ControlID="lbDeleteState" />
            <asp:AsyncPostBackTrigger ControlID="lbDeleteHub" />
            <asp:AsyncPostBackTrigger ControlID="lbDeleteProgram" />
            <asp:AsyncPostBackTrigger ControlID="lbDeleteCohort" />
            <asp:AsyncPostBackTrigger ControlID="lbDeleteProgramStatus" EventName="Click" />
        </Triggers>
    </asp:UpdatePanel>
    <div class="modal" id="divDeleteStateModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete State</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this state?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteState" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteStateModal" OnClick="lbDeleteState_Click"><i class="fas fa-check"></i>&nbsp;Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
    <div class="modal" id="divDeleteHubModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete Hub</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this hub?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteHub" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteHubModal" OnClick="lbDeleteHub_Click"><i class="fas fa-check"></i>&nbsp;Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
    <div class="modal" id="divDeleteCohortModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete Cohort</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this cohort?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteCohort" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteCohortModal" OnClick="lbDeleteCohort_Click"><i class="fas fa-check"></i>&nbsp;Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
    <div class="modal" id="divDeleteProgramModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete Program</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this program?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteProgram" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteProgramModal" OnClick="lbDeleteProgram_Click"><i class="fas fa-check"></i>&nbsp;Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
    <div class="modal" id="divDeleteStatusModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete Program Status</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this program status?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteProgramStatus" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteStatusModal" OnClick="lbDeleteProgramStatus_Click"><i class="fas fa-check"></i>&nbsp;Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
