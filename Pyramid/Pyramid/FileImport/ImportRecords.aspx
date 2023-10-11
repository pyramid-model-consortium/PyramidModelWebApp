<%@ Page Title="Import Rosters" Language="C#" MasterPageFile="~/MasterPages/LoggedIn.master" AutoEventWireup="true" CodeBehind="ImportRecords.aspx.cs" Inherits="Pyramid.FileImport.ImportRecords" %>

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
            //Allow date format for DataTables sorting
            $.fn.dataTable.moment('MM/DD/YYYY');

            //Set up the show event for the role permission collapse
            $('#collapseFileDescription').on('shown.bs.collapse', function () {
                //Resize the datatable
                $('#tblFileDetails').DataTable().columns.adjust().responsive.recalc();
            });

            //Initialize the datatables
            if (!$.fn.dataTable.isDataTable('#tblFileDetails')) {
                $('#tblFileDetails').DataTable({
                    responsive: {
                        details: {
                            type: 'column'
                        }
                    },
                    columnDefs: [
                        { className: 'control', orderable: false, targets: 0 }
                    ],
                    stateSave: true,
                    stateDuration: 60,
                    ordering: false,
                    paging: false,
                    pageLength: 10,
                    dom: 'frtp',
                    language: {
                        searchPlaceholder: "Enter text to search...",
                        search: "",
                    }
                });
            }
        }
    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Label ID="lblPageTitle" runat="server" Text="Import Records" CssClass="h2"></asp:Label>
    <hr />
    <uc:Messaging ID="msgSys" runat="server" />
    <div class="card bg-light mb-2 mt-2">
        <div class="collapse-module">
            <div class="card-header">
                <a class="collapse-module-button" data-toggle="collapse" href="#collapseImportInstructions" aria-expanded="true" aria-controls="collapseImportInstructions">
                    <i class="collapse-module-icon"></i>&nbsp;Template and Instructions
                </a>
            </div>
            <div class="card-body pb-0 pt-0">
                <div id="collapseImportInstructions" aria-expanded="true" class="collapse show">
                    <div class="row mt-2 mb-2">
                        <div class="col-lg-6">
                            <h5>Template CSV File</h5>
                            <div class="mb-2">
                                This is a template CSV file that contains all the fields that are available for the upload. This is the file you should download, fill out, and upload for import.  Required fields are denoted by an asterisk (*) after the field name.
                            </div>
                            <asp:HyperLink ID="lnkImportFileTemplate" runat="server" Target="_blank" CssClass="btn btn-primary"><i class="fas fa-file-download"></i>&nbsp;Download Template</asp:HyperLink>
                        </div>
                        <div class="col-lg-6">
                            <h5>Example CSV File</h5>
                            <div class="mb-2">
                                This is a filled out version of the template file that exemplifies how to enter information.
                            </div>
                            <asp:HyperLink ID="lnkImportFileExample" runat="server" Target="_blank" CssClass="btn btn-primary"><i class="fas fa-file-download"></i>&nbsp;Download Example</asp:HyperLink>
                        </div>
                    </div>
                    <hr />
                    <div class="row mt-2 mb-2">
                        <div class="col-lg-12">
                            <h5>Instructions</h5>
                            <div class="mb-2">
                                <asp:Literal ID="litImportInstructions" runat="server"></asp:Literal>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <div class="card bg-light mb-2 mt-2">
        <div class="collapse-module">
            <div class="card-header">
                <a class="collapsed collapse-module-button" data-toggle="collapse" href="#collapseFileDescription" aria-expanded="false" aria-controls="collapseFileDescription">
                    <i class="collapse-module-icon"></i>&nbsp;CSV File Information
                </a>
            </div>
            <div class="card-body pb-0 pt-0">
                <div id="collapseFileDescription" aria-expanded="false" class="collapse">
                    <div class="row mt-2 mb-2">
                        <div class="col-lg-12">
                            <h5>CSV File Information</h5>
                            <div class="alert alert-primary">
                                <p>
                                    <i class="fas fa-info-circle mr-2"></i>All dates must be in a valid format if they are entered.  Valid formats include, but are not limited to: mm/dd/yyyy, m/d/yyyy, m/d/yy, and yyyy-mm-dd.  The recommended format is mm/dd/yyyy, and an example for May 12th, 2002 is: 05/12/2002.
                                </p>
                                <p>
                                    Case-sensitive means that the system cares about uppercase vs lowercase letters.  If a field is case-sensitive, it will count 'example' as different from 'Example' or 'EXAMPLE'.  If a field is NOT case-sensitive, it will count 'example' as the same as 'Example' or 'EXAMPLE'.
                                </p>
                            </div>
                            <asp:Repeater ID="repeatFileDetails" runat="server" ItemType="Pyramid.FileImport.CodeFiles.ImportFileFieldDetail">
                                <HeaderTemplate>
                                    <table id="tblFileDetails" class="table table-striped table-bordered table-hover">
                                        <thead>
                                            <tr>
                                                <th data-priority="1"></th>
                                                <th data-priority="2">Field Name</th>
                                                <th data-priority="6">Field Type</th>
                                                <th data-priority="4">Description</th>
                                                <th data-priority="5">Acceptable Input</th>
                                                <th data-priority="3">Required?</th>
                                            </tr>
                                        </thead>
                                        <tbody>
                                </HeaderTemplate>
                                <ItemTemplate>
                                            <tr>
                                                <td></td>
                                                <td><%# Item.FieldName %></td>
                                                <td><%# Item.FieldType %></td>
                                                <td><%# Item.FieldDescription %></td>
                                                <td><%# Item.FieldAcceptableInput %></td>
                                                <td><i class="fas fa-lg <%# (Item.IsRequired ? "fa-check green-text" : "fa-times red-text") %>"></i></td>
                                            </tr>
                                </ItemTemplate>
                                <FooterTemplate>
                                        </tbody>
                                    </table>
                                </FooterTemplate>
                            </asp:Repeater>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <div class="card bg-light mb-2 mt-2">
        <div class="card-header">
            Import CSV File
        </div>
        <div class="card-body">
            <div class="row">
                <div class="col-md-4">
                    <dx:BootstrapComboBox ID="ddProgram" runat="server" Caption="Program" NullText="--Select--"
                        TextField="ProgramName" ValueField="ProgramPK" ValueType="System.Int32"
                        IncrementalFilteringMode="Contains" AllowMouseWheel="false"
                        ClientInstanceName="ddProgram" AllowNull="false">
                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                        <ValidationSettings ValidationGroup="vgFileImport" ErrorDisplayMode="ImageWithText">
                            <RequiredField IsRequired="true" ErrorText="Program is required!" />
                        </ValidationSettings>
                    </dx:BootstrapComboBox>
                </div>
                <div class="col-md-4">
                    <asp:Label ID="lblUploadControl" runat="server" AssociatedControlID="bucUploadFile" CssClass="dxbs-edit-caption col-form-label" Text="Select File to Import"></asp:Label>
                    <dx:BootstrapUploadControl ID="bucUploadFile" runat="server" ShowUploadButton="false" aria-labelledby="lblUploadControl">
                        <ValidationSettings MaxFileSize="1000000" AllowedFileExtensions=".csv" />
                    </dx:BootstrapUploadControl>
                    <small>Allowed file extensions: .csv</small>
                    <br />
                    <small>Maximum file size: 1 MB.</small>
                </div>
            </div>
        </div>
        <div class="card-footer">
            <div class="center-content">
                <uc:Submit ID="submitFileImport" runat="server"
                    SubmitButtonText="Import and Preview Results"
                    SubmittingButtonText="Importing..." SubmitButtonIcon="fas fa-file-upload" SubmitButtonBootstrapType="primary"
                    ValidationGroup="vgFileImport" OnSubmitClick="submitFileImport_SubmitClick" 
                    OnCancelClick="submitFileImport_CancelClick" 
                    OnValidationFailed="submitFileImport_ValidationFailed" />
            </div>
        </div>
    </div>
</asp:Content>
