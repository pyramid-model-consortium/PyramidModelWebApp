<%@ Page Title="Confirm Import" Language="C#" MasterPageFile="~/MasterPages/LoggedIn.master" AutoEventWireup="true" CodeBehind="ConfirmImport.aspx.cs" Inherits="Pyramid.FileImport.ConfirmImport" %>

<%@ Register Assembly="DevExpress.Web.Bootstrap.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>
<%@ Register TagPrefix="uc" TagName="Messaging" Src="~/User_Controls/MessagingSystem.ascx" %>
<%@ Register TagPrefix="uc" TagName="Submit" Src="~/User_Controls/Submit.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style type="text/css">
        [ID$="bsGRUploadPreview"] td {
            white-space: pre-wrap;
        }

        [ID$="bsGRUploadPreview"] td .check-adaptive {
            display: none !important;
        }

        [ID$="bsGRUploadPreview"] td .check-icon {
            display: block !important;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ScriptContent" runat="server">
    <script type="text/javascript">
        //This function fires when the user clicks the export invalid records button
        function btnExportInvalidRecords_ClientClick() {
            //Show the user a message about the export
            showNotification('primary', 'Export In Progress', 'The invalid records are being exported.  The Excel file with the invalid records should appear in the default download location on your computer momentarily.', 15000);
        }
    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Label ID="lblPageTitle" runat="server" Text="Import Rows" CssClass="h2"></asp:Label>
    <hr />
    <div class="card bg-light mb-2 mt-2">
        <div class="collapse-module">
            <div class="card-header">
                <a class="collapse-module-button" data-toggle="collapse" href="#collapseConfirmationInstructions" aria-expanded="true" aria-controls="collapseConfirmationInstructions">
                    <i class="collapse-module-icon"></i>&nbsp;Instructions
                </a>
            </div>
            <div class="card-body pb-0 pt-0">
                <div id="collapseConfirmationInstructions" aria-expanded="true" class="collapse show">
                    <div class="row mt-2 mb-2">
                        <div class="col-lg-12">
                            <asp:Literal ID="litConfirmationInstructions" runat="server"></asp:Literal>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <asp:UpdatePanel ID="upConfirmImport" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <uc:Messaging ID="msgSys" runat="server" />
            <div class="card bg-light">
                <div class="card-header">
                    Import Result Preview
                </div>
                <div class="card-body">
                    <div class="row">
                        <div class="col-lg-12">
                            <div id="divProgram" runat="server" class="mb-3">
                                <strong>Program:</strong>&nbsp;<asp:Label ID="lblProgram" runat="server" Text=""></asp:Label>
                            </div>
                            <div class="alert alert-warning">
                                <i class="fas fa-exclamation-triangle"></i>&nbsp;Only the records that are valid will be imported into PIDS.
                            </div>
                            <dx:BootstrapButton ID="btnExportInvalidRecords" runat="server" SettingsBootstrap-RenderOption="Primary" Text="Export Invalid Records to Excel" OnClick="btnExportInvalidRecords_Click">
                                <ClientSideEvents Click="btnExportInvalidRecords_ClientClick" />
                                <CssClasses Icon="fas fa-file-excel" Control="mb-2" />
                            </dx:BootstrapButton>
                            <dx:BootstrapGridView ID="bsGRUploadPreview" runat="server" EnableCallBacks="false" AutoGenerateColumns="true">
                                <SettingsPager PageSize="15" />
                                <SettingsBootstrap Striped="true" />
                                <SettingsBehavior EnableRowHotTrack="true" />
                                <SettingsSearchPanel Visible="true" ShowApplyButton="true" />
                                <Settings ShowGroupPanel="false" />
                                <CssClasses IconHeaderSortUp="fas fa-long-arrow-alt-up" IconHeaderSortDown="fas fa-long-arrow-alt-down" IconShowAdaptiveDetailButton="fas fa-plus-circle" />
                                <CssClassesEditor IconCheckColumnChecked="fas fa-lg fa-check green-text" IconCheckColumnUnchecked="fas fa-lg fa-times red-text" />
                                <SettingsAdaptivity AdaptivityMode="HideDataCells" AllowOnlyOneAdaptiveDetailExpanded="true" AdaptiveColumnPosition="Left"></SettingsAdaptivity>
                            </dx:BootstrapGridView>
                        </div>
                    </div>
                </div>
                <div class="card-footer">
                    <div class="center-content">
                        <uc:Submit ID="submitConfirmImport" runat="server" SubmitButtonText="Save Valid Records"
                            ValidationGroup="vgConfirmImport" UseSubmitConfirm="true" SubmitConfirmButtonText="Yes, save records" 
                            SubmitConfirmModalText="Are you sure you want to save the valid records?  This action can only be undone by searching for and deleting each imported record."
                            OnSubmitClick="submitConfirmImport_SubmitClick" 
                            OnCancelClick="submitConfirmImport_CancelClick" 
                            OnValidationFailed="submitConfirmImport_ValidationFailed" />
                    </div>
                </div>
            </div>
        </ContentTemplate>
        <Triggers>
            <asp:PostBackTrigger ControlID="btnExportInvalidRecords" />
        </Triggers>
    </asp:UpdatePanel>
</asp:Content>
