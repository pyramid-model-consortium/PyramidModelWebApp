<%@ Page Title="Uploaded Files" Language="C#" MasterPageFile="~/MasterPages/Dashboard.master" AutoEventWireup="true" CodeBehind="UploadedFiles.aspx.cs" Inherits="Pyramid.Pages.UserFileUploads" %>

<%@ Register Assembly="DevExpress.Web.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Data.Linq" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.Bootstrap.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>
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
            //Highlight the correct link in the master page
            $('#lnkUploadedFileDashboard').addClass('active');

            //Show/hide the view only fields
            setViewOnlyVisibility();

            //Set up the click events for the help buttons
            $('#btnFileTypeHelp').popover({
                trigger: 'hover focus',
                title: 'Help',
                html: true,
                content: 'The options for this are "Program-Wide", "Hub-Wide", and "State-Wide", but you may only be able to use a subset of those options depending on your role. ' +
                    'The definitions for the options are below:' +
                    '<ul>' +
                    '<li>Program-Wide: Files that have this option selected are accessible to all users in the program, but not accessible to hub or state users. ' +
                    'NOTE: Users with the Classroom Coach Data Collector, Leadership Coach, or Program Implementation Coach role will only be able to see files that they have uploaded.  They cannot see files that other users have uploaded.</li>' +
                    '<li>Hub-Wide: Files that have this option selected are accessible to all hub-level users for the selected hub and all the state-level users for the selected hub\'s state.</li>' +
                    '<li>State-Wide: Files that have this option selected are accessible to all state-level users for the selected state. ' +
                    'NOTE: Users with the Master Cadre Member role will only be able to see files that they have uploaded.  They cannot see files that other users have uploaded.</li>' +
                    '</ul>'
            });
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

        //Show the proper file type option
        function showFileTypeOption(s, e) {
            //Get the file type
            var fileTypeText = ddFileType.GetText().toLowerCase();

            if (fileTypeText) {
                //Show the proper option
                if (fileTypeText.includes('program')) {
                    $('.file-type-option').addClass('hidden');
                    $('#divPrograms').removeClass('hidden');
                }
                else if (fileTypeText.includes('hub')) {
                    $('.file-type-option').addClass('hidden');
                    $('#divHubs').removeClass('hidden');
                }
                else if (fileTypeText.includes('state')) {
                    $('.file-type-option').addClass('hidden');
                    $('#divStates').removeClass('hidden');
                }
                else if (fileTypeText.includes('cohort')) {
                    $('.file-type-option').addClass('hidden');
                    $('#divCohorts').removeClass('hidden');
                }
            }
        }

        //This function validates the file type options
        function validateFileTypeOption(s, e) {
            //Get the entry type
            var fileTypeText = ddFileType.GetText().toLowerCase();
            var selectedOption;

            //Perform validation
            if (fileTypeText) {
                if (fileTypeText.includes('program')) {
                    selectedOption = ddProgram.GetValue();

                    if (!selectedOption) {
                        e.isValid = false;
                        e.errorText = 'Required!';
                    }
                }
                else if (fileTypeText.includes('hub')) {
                    selectedOption = ddHub.GetValue();

                    if (!selectedOption) {
                        e.isValid = false;
                        e.errorText = 'Required!';
                    }
                }
                else if (fileTypeText.includes('state')) {
                    selectedOption = ddState.GetValue();

                    if (!selectedOption) {
                        e.isValid = false;
                        e.errorText = 'Required!';
                    }
                }
                else if (fileTypeText.includes('cohort')) {
                    selectedOption = ddCohort.GetValue();

                    if (!selectedOption) {
                        e.isValid = false;
                        e.errorText = 'Required!';
                    }
                }
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
            <asp:AsyncPostBackTrigger ControlID="lbDeleteUserFileUpload" EventName="Click" />
        </Triggers>
    </asp:UpdatePanel>
    <asp:HiddenField ID="hfDeleteUserFileUploadPK" runat="server" Value="0" />
    <asp:UpdatePanel ID="upAllUserFileUploads" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <div class="row">
                <div class="col-lg-12">
                    <div class="card bg-light">
                        <div class="card-header">
                            All Files
                            <asp:LinkButton ID="lbNewFile" runat="server" CssClass="btn btn-loader btn-primary float-right hide-on-view hidden" OnClick="lbNewFile_Click"><i class="fas fa-file-upload"></i>&nbsp;Upload New File</asp:LinkButton>
                        </div>
                        <div class="card-body">
                            <div class="row">
                                <div class="col-lg-12">
                                    <div class="alert alert-primary">
                                        This table contains all uploaded files regardless of when they were uploaded.
                                    </div>
                                    <dx:BootstrapGridView ID="bsGRUserFileUploads" runat="server" EnableCallBacks="false"
                                        EnableRowsCache="true" KeyFieldName="UserFileUploadPK" AutoGenerateColumns="false"
                                        OnHtmlRowCreated="bsGRUserFileUploads_HtmlRowCreated" DataSourceID="sqlUserFileUploadDataSource">
                                        <SettingsPager PageSize="15" />
                                        <SettingsBootstrap Striped="true" />
                                        <SettingsBehavior EnableRowHotTrack="true" />
                                        <SettingsSearchPanel Visible="true" ShowApplyButton="true" />
                                        <Settings ShowGroupPanel="false" />
                                        <CssClasses IconHeaderSortUp="fas fa-long-arrow-alt-up" IconHeaderSortDown="fas fa-long-arrow-alt-down" IconShowAdaptiveDetailButton="fas fa-plus-circle" />
                                        <SettingsAdaptivity AdaptivityMode="HideDataCells" AllowOnlyOneAdaptiveDetailExpanded="true" AdaptiveColumnPosition="Left"></SettingsAdaptivity>
                                        <Columns>
                                            <dx:BootstrapGridViewDataColumn FieldName="FileType" Caption="Type" Width="10%" AdaptivePriority="5">
                                                <DataItemTemplate>
                                                    <div class="text-center">
                                                        <i class='<%# "d-block mb-0 h2 fas fa-file-" + Eval("FileType") %>'></i>
                                                        <label class="d-block mb-0">
                                                            <%# (Eval("FileType").ToString() == "alt" ? "other" : Eval("FileType")) %>
                                                        </label>
                                                    </div>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewDataColumn>
                                            <dx:BootstrapGridViewDataColumn FieldName="DisplayFileName" Caption="File Name" AdaptivePriority="0" />
                                            <dx:BootstrapGridViewDataColumn FieldName="FileDescription" Caption="Description" AdaptivePriority="2" />
                                            <dx:BootstrapGridViewDateColumn FieldName="FileCreateDate" Caption="Upload Date and Time" SortIndex="0" SortOrder="Descending" PropertiesDateEdit-DisplayFormatString="MM/dd/yyyy hh:mm:ss tt" AdaptivePriority="3" />
                                            <dx:BootstrapGridViewDataColumn FieldName="FileUploadedBy" Caption="Uploaded By" AdaptivePriority="3" />
                                            <dx:BootstrapGridViewDataColumn FieldName="TypeDescription" Caption="Source" AdaptivePriority="4" />
                                            <dx:BootstrapGridViewButtonEditColumn Name="ActionColumn" Settings-AllowDragDrop="False" AdaptivePriority="1" CssClasses-DataCell="text-center">
                                                <DataItemTemplate>
                                                    <div class="btn-group">
                                                        <button type="button" class="btn btn-secondary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                            Actions
                                                        </button>
                                                        <div class="dropdown-menu dropdown-menu-right">
                                                            <a class="dropdown-item" target="_blank" href="/Pages/ViewFile?UserFileUploadPK=<%# Eval("UserFileUploadPK") %>"><i class="fas fa-file-download"></i>&nbsp;View/Download</a>
                                                            <asp:LinkButton ID="lbDeleteFile" runat="server" CssClass="dropdown-item delete-gridview hide-on-view" data-pk='<%# Eval("UserFileUploadPK") %>' data-hf="hfDeleteUserFileUploadPK" data-target="#divDeleteUserFileUploadModal"><i class="fas fa-trash"></i>&nbsp;Delete</asp:LinkButton>
                                                        </div>
                                                    </div>
                                                </DataItemTemplate>
                                            </dx:BootstrapGridViewButtonEditColumn>
                                        </Columns>
                                    </dx:BootstrapGridView>
                                    <asp:SqlDataSource ID="sqlUserFileUploadDataSource" runat="server" CancelSelectOnNullParameter="false"
                                        SelectCommandType="StoredProcedure" SelectCommand="spGetAllFileUploads">
                                        <SelectParameters>
                                            <asp:Parameter Name="ProgramFKs" Type="String" />
                                            <asp:Parameter Name="HubFKs" Type="String" />
                                            <asp:Parameter Name="StateFKs" Type="String" />
                                            <asp:Parameter Name="CohortFKs" Type="String" />
                                            <asp:Parameter Name="RoleFK" Type="Int32" />
                                            <asp:Parameter Name="Username" Type="String" />
                                        </SelectParameters>
                                    </asp:SqlDataSource>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-12">
                                    <div id="divUploadFile" runat="server" class="card mt-2" visible="false">
                                        <div class="card-header">
                                            <asp:Label ID="lblFileUpload" runat="server" Text="New File Upload"></asp:Label>
                                        </div>
                                        <div class="card-body">
                                            <div class="alert alert-warning">
                                                <i class="fas fa-exclamation-circle"></i>&nbsp;
                                                Do NOT upload files that contain Personal Health Information covered by HIPAA.  If in doubt about whether a file is allowed to be uploaded, please contact your program administrator or PIDS state administrator to request clarification.
                                            </div>
                                            <div class="row">
                                                <div class="col-lg-4">
                                                    <dx:BootstrapComboBox ID="ddFileType" runat="server" Caption="File Accessible to:" NullText="--Select--" ValueType="System.Int32"
                                                        TextField="Description" ValueField="CodeFileUploadTypePK"
                                                        IncrementalFilteringMode="StartsWith" AllowMouseWheel="false"
                                                        ClientInstanceName="ddFileType">
                                                        <ClientSideEvents Init="showFileTypeOption" ValueChanged="showFileTypeOption" />
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgFileUpload" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="true" ErrorText="Type is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapComboBox>
                                                    <button id="btnFileTypeHelp" type="button" class="btn btn-link p-0"><i class="fas fa-question-circle"></i>&nbsp;Help</button>
                                                </div>
                                                <div class="col-lg-4">
                                                    <label id="lblUploadControl">File to upload</label>
                                                    <dx:BootstrapUploadControl ID="bucUploadFile" runat="server" ShowUploadButton="false" aria-labelledby="lblUploadControl">
                                                        <ValidationSettings MaxFileSize="20000000" AllowedFileExtensions=".pdf,.doc,.docx,.ppt,.pptx,.xls,.xlsx,.xlsm,.jpeg,.jpg,.png" />
                                                    </dx:BootstrapUploadControl>
                                                    <small>Allowed file extensions: .pdf, .doc, .docx, .ppt, .pptx, .xls, .xlsx, .xlsm, .jpeg, .jpg, .png</small>
                                                    <br />
                                                    <small>Maximum file size: 20 MB.</small>
                                                </div>
                                                <div class="col-lg-4">
                                                    <dx:BootstrapMemo ID="txtFileDescription" runat="server" Caption="Description">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgFileUpload" ErrorDisplayMode="ImageWithText">
                                                            <RequiredField IsRequired="true" ErrorText="Description is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapMemo>
                                                </div>
                                            </div>
                                            <div class="row">                                                
                                                <div id="divPrograms" class="col-lg-4 hidden file-type-option">
                                                    <dx:BootstrapComboBox ID="ddProgram" runat="server" Caption="Program" NullText="--Select--" AllowNull="true"
                                                        TextField="ProgramName" ValueField="ProgramPK" ValueType="System.Int32"
                                                        IncrementalFilteringMode="Contains" AllowMouseWheel="false"
                                                        ClientInstanceName="ddProgram" OnValidation="FileTypeOption_Validation">
                                                        <ClientSideEvents Validation="validateFileTypeOption" />
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgFileUpload" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                            <RequiredField IsRequired="false" ErrorText="Program is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapComboBox>
                                                </div>
                                                <div id="divHubs" class="col-lg-4 hidden file-type-option">
                                                    <dx:BootstrapComboBox ID="ddHub" runat="server" Caption="Hub" NullText="--Select--" AllowNull="true"
                                                        TextField="Name" ValueField="HubPK" ValueType="System.Int32"
                                                        IncrementalFilteringMode="Contains" AllowMouseWheel="false"
                                                        ClientInstanceName="ddHub" OnValidation="FileTypeOption_Validation">
                                                        <ClientSideEvents Validation="validateFileTypeOption" />
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgFileUpload" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                            <RequiredField IsRequired="false" ErrorText="Hub is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapComboBox>
                                                </div>
                                                <div id="divStates" class="col-lg-4 hidden file-type-option">
                                                    <dx:BootstrapComboBox ID="ddState" runat="server" Caption="State" NullText="--Select--" AllowNull="true"
                                                        TextField="Name" ValueField="StatePK" ValueType="System.Int32"
                                                        IncrementalFilteringMode="Contains" AllowMouseWheel="false"
                                                        ClientInstanceName="ddState" OnValidation="FileTypeOption_Validation">
                                                        <ClientSideEvents Validation="validateFileTypeOption" />
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgFileUpload" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                            <RequiredField IsRequired="false" ErrorText="State is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapComboBox>
                                                </div>
                                                <div id="divCohorts" class="col-lg-4 hidden file-type-option">
                                                    <dx:BootstrapComboBox ID="ddCohort" runat="server" Caption="Cohort" NullText="--Select--" AllowNull="true"
                                                        TextField="CohortName" ValueField="CohortPK" ValueType="System.Int32"
                                                        IncrementalFilteringMode="Contains" AllowMouseWheel="false"
                                                        ClientInstanceName="ddCohort" OnValidation="FileTypeOption_Validation">
                                                        <ClientSideEvents Validation="validateFileTypeOption" />
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <ValidationSettings ValidationGroup="vgFileUpload" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                            <RequiredField IsRequired="false" ErrorText="Cohort is required!" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapComboBox>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="card-footer">
                                            <div class="center-content">
                                                <uc:Submit ID="submitFileUpload" runat="server"
                                                    ControlCssClass="center-content"
                                                    ValidationGroup="vgFileUpload" OnSubmitClick="submitFileUpload_Click" 
                                                    OnCancelClick="submitFileUpload_CancelClick" 
                                                    OnValidationFailed="submitFileUpload_ValidationFailed" />
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
            <asp:PostBackTrigger ControlID="submitFileUpload" />
            <asp:AsyncPostBackTrigger ControlID="lbDeleteUserFileUpload" EventName="Click" />
        </Triggers>
    </asp:UpdatePanel>
    <div class="modal" id="divDeleteUserFileUploadModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete Uploaded File</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this file?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteUserFileUpload" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteUserFileUploadModal" OnClick="lbDeleteUserFileUpload_Click"><i class="fas fa-check"></i>&nbsp;Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
