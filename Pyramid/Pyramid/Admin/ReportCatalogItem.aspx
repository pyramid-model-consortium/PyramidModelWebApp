<%@ Page Title="Report Catalog Item" Language="C#" MasterPageFile="~/MasterPages/LoggedIn.master" AutoEventWireup="true" CodeBehind="ReportCatalogItem.aspx.cs" Inherits="Pyramid.Admin.ReportCatalogItem" %>

<%@ Register TagPrefix="uc" TagName="Messaging" Src="~/User_Controls/MessagingSystem.ascx" %>
<%@ Register TagPrefix="uc" TagName="Submit" Src="~/User_Controls/Submit.ascx" %>
<%@ Register Assembly="DevExpress.Web.Bootstrap.v19.1, Version=19.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>

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
            //Set the click events for the help buttons
            $('#btnCriteriaDescriptions, #btnOptionalCriteriaDescriptions').off('click').on('click', function (e) {
                //Prevent postback
                e.preventDefault();

                //Get the criteria description list
                var criteriaDescriptionList = $('[ID$="hfCriteriaDescriptions"]').val();

                //Split the list into an array
                var criteriaDescriptionArray = criteriaDescriptionList.split('*sep*');

                //To hold the criteria descriptions
                var criteriaDescriptions = '';

                //Loop through each criteria description and add it to the string
                $.each(criteriaDescriptionArray, function (index, value) {
                    criteriaDescriptions += '<div class="mt-2">';
                    criteriaDescriptions += criteriaDescriptionArray[index];
                    criteriaDescriptions += '</div>';
                });

                //Show the descriptions
                showNotification('primary', 'Criteria Descriptions', criteriaDescriptions, 30000);
            });

            $('#btnDefaultDescriptions').off('click').on('click', function (e) {
                //Prevent postback
                e.preventDefault();

                //Get the criteria default description list
                var criteriaDefaultList = $('[ID$="hfCriteriaDefaultDescriptions"]').val();

                //Split the list into an array
                var criteriaDefaultArray = criteriaDefaultList.split('*sep*');

                //To hold the criteria defaults
                var criteriaDefaults = '';

                //Add the custom criteria rules
                criteriaDefaults += '<h4>Custom defaults:</h4>'
                criteriaDefaults += '<div class="mt-2">';
                criteriaDefaults += 'RSD-M[Modifiers] - Relative Start Date.  Modifiers: The + or - symbol and number of months to add or subtract from now.  Ex. RSD-M-12 will default the Start Date to 12 months in the past.';
                criteriaDefaults += '</div>';
                criteriaDefaults += '<div class="mt-2">';
                criteriaDefaults += 'RSD-Y[Modifiers] - Relative Start Date.  Modifiers: The + or - symbol and number of years to add or subtract from now.  Ex. RSD-Y-1 will default the Start Date to 1 year in the past.';
                criteriaDefaults += '</div>';
                criteriaDefaults += '<div class="mt-2">';
                criteriaDefaults += 'ASD[Modifiers] - Absolute Start Date.  Modifiers: The date you want the Start Date to default to. Ex. RSD-02/01/2019 will default the Start Date to 02/01/2019.';
                criteriaDefaults += '</div>';
                criteriaDefaults += '<div class="mt-2">';
                criteriaDefaults += 'RED-M[Modifiers] - Relative End Date.  Modifiers: The + or - symbol and number of months to add or subtract from now.  Ex. RSD-M-12 will default the End Date to 12 months in the past.';
                criteriaDefaults += '</div>';
                criteriaDefaults += '<div class="mt-2">';
                criteriaDefaults += 'RED-Y[Modifiers] - Relative End Date.  Modifiers: The + or - symbol and number of years to add or subtract from now.  Ex. RSD-Y-1 will default the End Date to 1 year in the past.';
                criteriaDefaults += '</div>';
                criteriaDefaults += '<div class="mt-2">';
                criteriaDefaults += 'AED[Modifiers] - Absolute End Date.  Modifiers: The date you want the End Date to default to. Ex. RSD-02/01/2019 will default the End Date to 02/01/2019.';
                criteriaDefaults += '</div>';
                criteriaDefaults += '<div class="mt-2">';
                criteriaDefaults += 'RPIT-M[Modifiers] - Relative Point in Time.  Modifiers: The + or - symbol and number of months to add or subtract from now.  Ex. RSD-M-12 will default the Point in Time to 12 months in the past.';
                criteriaDefaults += '</div>';
                criteriaDefaults += '<div class="mt-2">';
                criteriaDefaults += 'RPIT-Y[Modifiers] - Relative Point in Time.  Modifiers: The + or - symbol and number of years to add or subtract from now.  Ex. RSD-Y-1 will default the Point in Time to 1 year in the past.';
                criteriaDefaults += '</div>';
                criteriaDefaults += '<div class="mt-2">';
                criteriaDefaults += 'APIT[Modifiers] - Absolute Point in Time.  Modifiers: The date you want the Point in Time to default to. Ex. RSD-02/01/2019 will default the Point in Time to 02/01/2019.';
                criteriaDefaults += '</div><br/>';
                criteriaDefaults += '<h4>Preset defaults:</h4>'

                //Loop through each criteria description and add it to the string
                $.each(criteriaDefaultArray, function (index, value) {
                    criteriaDefaults += '<div class="mt-2">';
                    criteriaDefaults += criteriaDefaultArray[index];
                    criteriaDefaults += '</div>';
                });

                //Show the descriptions
                showNotification('primary', 'Criteria Default Descriptions', criteriaDefaults, 30000);
            });
        }

        //This function controls whether or not the specify field for the report category is shown
        //based on the value in the ddReportCategory Combo Box
        function showHideReportCategorySpecify() {
            //Get the report category
            var reportCategory = ddReportCategory.GetText();

            //If the admin follow-up is other, show the specify div
            if (reportCategory.toLowerCase().includes('other')) {
                $('#divReportCategorySpecify').slideDown();
            }
            else {
                //The admin follow-up is not other, clear the specify text box and hide the specify div
                txtReportCategorySpecify.SetValue('');
                $('#divReportCategorySpecify').slideUp();
            }
        }

        //Validate the report category specify field
        function validateReportCategorySpecify(s, e) {
            var reportCategorySpecify = e.value;
            var reportCategory = ddReportCategory.GetText();

            if ((reportCategorySpecify == null || reportCategorySpecify == ' ') && reportCategory.toLowerCase().includes('other')) {
                e.isValid = false;
                e.errorText = "Specify Report Category is required when the 'Other' report category is selected!";
            }
            else {
                e.isValid = true;
            }
        }
    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <asp:UpdatePanel ID="upReportCatalogItem" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <asp:Label ID="lblPageTitle" Text="" CssClass="h2" runat="server"></asp:Label>
            <hr />
            <asp:HiddenField ID="hfViewOnly" runat="server" Value="False" />
            <asp:HiddenField ID="hfCriteriaDescriptions" runat="server" Value="" />
            <asp:HiddenField ID="hfCriteriaDefaultDescriptions" runat="server" Value="" />
            <uc:Messaging ID="msgSys" runat="server" />
            <div class="row">
                <div class="col-lg-12">
                    <div class="card bg-light">
                        <div class="card-header">
                            Basic Information
                        </div>
                        <div class="card-body">
                            <div class="row">
                                <div class="col-lg-4">
                                    <dx:BootstrapTextBox ID="txtReportName" runat="server" Caption="Report Name" MaxLength="100">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgReportCatalogItem">
                                            <RequiredField IsRequired="true" ErrorText="Required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapTextBox>
                                </div>
                                <div class="col-lg-4">
                                    <dx:BootstrapTextBox ID="txtReportClass" runat="server" Caption="Report Class" MaxLength="100">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgReportCatalogItem">
                                            <RequiredField IsRequired="true" ErrorText="Required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapTextBox>
                                </div>
                                <div class="col-lg-4">
                                    <dx:BootstrapComboBox ID="ddReportCategory" runat="server" Caption="Report Category" NullText="--Select--"
                                        TextField="ReportCategory" ValueField="ReportCategory" ValueType="System.String"
                                        IncrementalFilteringMode="Contains" ClientInstanceName="ddReportCategory" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ClientSideEvents Init="showHideReportCategorySpecify" ValueChanged="showHideReportCategorySpecify" />
                                        <ValidationSettings ValidationGroup="vgReportCatalogItem" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                    <div id="divReportCategorySpecify" style="display: none">
                                        <dx:BootstrapTextBox ID="txtReportCategorySpecify" runat="server" Caption="Specify Report Category" MaxLength="100"
                                            OnValidation="txtReportCategorySpecify_Validation" ClientInstanceName="txtReportCategorySpecify">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ClientSideEvents Validation="validateReportCategorySpecify" />
                                            <ValidationSettings ValidationGroup="vgReportCatalogItem" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                            </ValidationSettings>
                                        </dx:BootstrapTextBox>
                                    </div>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-4">
                                    <dx:BootstrapMemo ID="txtReportDescription" runat="server" Caption="Report Description" Rows="5" MaxLength="1000">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgReportCatalogItem" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapMemo>
                                </div>
                                <div class="col-lg-4">
                                    <dx:BootstrapTagBox ID="tbKeywords" runat="server" Caption="Keywords"
                                        AllowCustomTags="true" MaxLength="100">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgReportCatalogItem" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapTagBox>
                                </div>
                                <div class="col-lg-4">
                                    <dx:BootstrapTagBox ID="tbRolesAuthorizedToRun" runat="server" Caption="Roles Authorized to View"
                                        AllowCustomTags="false" TextField="RoleName" ValueField="CodeProgramRolePK">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgReportCatalogItem" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapTagBox>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-4">
                                    <dx:BootstrapTagBox ID="tbCriteriaOptions" runat="server" Caption="Criteria"
                                        AllowCustomTags="false" MaxLength="100" TextField="Abbreviation" ValueField="Abbreviation">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgReportCatalogItem" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapTagBox>
                                    <button id="btnCriteriaDescriptions" class="btn btn-link"><i class="fas fa-question-circle"></i>&nbsp;Help</button>
                                </div>
                                <div class="col-lg-4">
                                    <dx:BootstrapTagBox ID="tbOptionalCriteriaOptions" runat="server" Caption="Optional Criteria"
                                        AllowCustomTags="false" MaxLength="100" TextField="Abbreviation" ValueField="Abbreviation">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgReportCatalogItem" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapTagBox>
                                    <button id="btnOptionalCriteriaDescriptions" class="btn btn-link"><i class="fas fa-question-circle"></i>&nbsp;Help</button>
                                </div>
                                <div class="col-lg-4">
                                    <dx:BootstrapTagBox ID="tbCriteriaDefaults" runat="server" Caption="Criteria Defaults"
                                        AllowCustomTags="true" MaxLength="100" TextField="Abbreviation" ValueField="Abbreviation">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgReportCatalogItem" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapTagBox>
                                    <button id="btnDefaultDescriptions" class="btn btn-link"><i class="fas fa-question-circle"></i>&nbsp;Help</button>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div class="row">
                <div class="col-lg-12">
                    <div class="card bg-light">
                        <div class="card-header">
                            Documentation
                        </div>
                        <div class="card-body">
                            <div class="row">
                                <div class="col-md-6 col-lg-4">
                                    <dx:BootstrapTextBox ID="txtDocumentationFileName" runat="server" Caption="Documentation File Name"
                                        OnValidation="txtDocumentationFileName_Validation">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <CssClasses Control="mw-100" Input="mw-100" />
                                        <ValidationSettings ValidationGroup="vgReportCatalogItem" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                        </ValidationSettings>
                                    </dx:BootstrapTextBox>
                                </div>
                            </div>
                            <asp:HyperLink ID="lnkExistingDocumentation" runat="server" Visible="false" CssClass="btn btn-primary mt-3" Target="_blank"><i class="fas fa-file-download"></i>&nbsp;Existing Documentation</asp:HyperLink>
                        </div>
                    </div>
                </div>
            </div>
            <div class="page-footer">
                <uc:Submit ID="submitReportCatalogItem" runat="server" ValidationGroup="vgReportCatalogItem" OnSubmitClick="submitReportCatalogItem_Click" OnCancelClick="submitReportCatalogItem_CancelClick" OnValidationFailed="submitReportCatalogItem_ValidationFailed" />
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
