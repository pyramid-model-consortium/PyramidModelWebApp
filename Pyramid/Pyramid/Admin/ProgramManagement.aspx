<%@ Page Title="Program Management" Language="C#" MasterPageFile="~/MasterPages/LoggedIn.master" AutoEventWireup="true" CodeBehind="ProgramManagement.aspx.cs" Inherits="Pyramid.Admin.ProgramManagement" %>

<%@ Register Assembly="DevExpress.Web.v19.1, Version=19.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Data.Linq" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.Bootstrap.v19.1, Version=19.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>
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

        }

        //Validate the cohort end date field
        function validateCohortEndDate(s, e) {
            //Get the start and end dates
            var endDate = e.value;
            var startDate = deCohortStartDate.GetDate();

            //Validate the end date
            if (endDate == null) {
                e.isValid = false;
                e.errorText = "Cohort End Date is required!";
            }
            else if(endDate != null && startDate != null && startDate >= endDate) {
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
            if(endDate != null && startDate != null && startDate >= endDate) {
                e.isValid = false;
                e.errorText = "Program End Date must be after the Program Start Date!";
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
                        <div class="card-header">States</div>
                        <div class="card-body">
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
                                    <dx:BootstrapGridViewTextColumn FieldName="Name" SortIndex="0" SortOrder="Ascending" AdaptivePriority="0">
                                        <PropertiesTextEdit>
                                            <ValidationSettings RequiredField-IsRequired="true" RequiredField-ErrorText="Name is required!"></ValidationSettings>
                                        </PropertiesTextEdit>
                                    </dx:BootstrapGridViewTextColumn>
                                    <dx:BootstrapGridViewImageColumn FieldName="LogoFilename" Caption="Logo" AdaptivePriority="2">
                                        <PropertiesImage ImageUrlFormatString="~/Content/images/{0}" ImageHeight="30px" ImageWidth="30px" />
                                    </dx:BootstrapGridViewImageColumn>
                                </Columns>
                            </dx:BootstrapGridView>
                            <dx:EntityServerModeDataSource ID="efStateDataSource" runat="server"
                                ContextTypeName="Pyramid.Models.PyramidContext" TableName="State"
                                OnSelecting="efStateDataSource_Selecting" />
                        </div>
                    </div>
                </div>
            </div>
            <div class="row">
                <div class="col-lg-6 mb-4">
                    <div class="card bg-light">
                        <div class="card-header">
                            Hubs
                            <asp:LinkButton ID="lbAddHub" runat="server" CssClass="btn btn-loader btn-primary float-right" OnClick="lbAddHub_Click"><i class="fas fa-plus"></i> Add New Hub</asp:LinkButton>
                        </div>
                        <div class="card-body">
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
                                                    <div class="form-group">
                                                        <dx:BootstrapTextBox ID="txtHubName" runat="server" Caption="Hub Name">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <ValidationSettings ValidationGroup="vgHub" ErrorDisplayMode="ImageWithText">
                                                                <RequiredField IsRequired="true" ErrorText="Hub Name is required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapTextBox>
                                                    </div>
                                                </div>
                                                <div class="col-lg-4">
                                                    <div class="form-group">
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
                                        </div>
                                        <div class="card-footer">
                                            <div class="d-flex align-items-center justify-content-center">
                                                <asp:HiddenField ID="hfAddEditHubPK" runat="server" Value="0" />
                                                <uc:Submit ID="submitHub" runat="server" ValidationGroup="vgHub" OnSubmitClick="submitHub_Click" OnCancelClick="submitHub_CancelClick" OnValidationFailed="submitHub_ValidationFailed" />
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
                            <asp:LinkButton ID="lbAddCohort" runat="server" CssClass="btn btn-loader btn-primary float-right" OnClick="lbAddCohort_Click"><i class="fas fa-plus"></i> Add New Cohort</asp:LinkButton>
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
                                                    <div class="form-group">
                                                        <dx:BootstrapTextBox ID="txtCohortName" runat="server" Caption="Cohort Name">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <ValidationSettings ValidationGroup="vgCohort" ErrorDisplayMode="ImageWithText">
                                                                <RequiredField IsRequired="true" ErrorText="Cohort Name is required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapTextBox>
                                                    </div>
                                                </div>
                                                <div class="col-xl-3 col-lg-6">
                                                    <div class="form-group">
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
                                                </div>
                                                <div class="col-xl-3 col-lg-6">
                                                    <div class="form-group">
                                                        <dx:BootstrapDateEdit ID="deCohortEndDate" runat="server" Caption="Cohort End Date" 
                                                            EditFormatString="MM/dd/yyyy" UseMaskBehavior="true" ClientInstanceName="deCohortEndDate"
                                                            OnValidation="deCohortEndDate_Validation"
                                                            AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                                            <ClientSideEvents Validation="validateCohortEndDate" />
                                                            <ValidationSettings ValidationGroup="vgCohort" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="true" ErrorText="Cohort End Date is required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapDateEdit>
                                                    </div>
                                                </div>
                                                <div class="col-xl-3 col-lg-6">
                                                    <div class="form-group">
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

                                        </div>
                                        <div class="card-footer">
                                            <div class="col-lg-12 d-flex align-items-center justify-content-center">
                                                <asp:HiddenField ID="hfAddEditCohortPK" runat="server" Value="0" />
                                                <uc:Submit ID="submitCohort" runat="server" ValidationGroup="vgCohort" OnSubmitClick="submitCohort_Click" OnCancelClick="submitCohort_CancelClick" OnValidationFailed="submitCohort_ValidationFailed" />
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
                    <asp:HiddenField ID="hfDeleteProgramPK" runat="server" Value="0" />
                    <asp:HiddenField ID="hfDeleteHubPK" runat="server" Value="0" />
                    <asp:HiddenField ID="hfDeleteCohortPK" runat="server" Value="0" />
                    <div class="card bg-light">
                        <div class="card-header">
                            Programs
                            <asp:LinkButton ID="lbAddProgram" runat="server" CssClass="btn btn-loader btn-primary float-right" OnClick="lbAddProgram_Click"><i class="fas fa-plus"></i> Add New Program</asp:LinkButton>
                        </div>
                        <div class="card-body">
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
                                            <dx:BootstrapGridViewDateColumn FieldName="Location" AdaptivePriority="2" />
                                            <dx:BootstrapGridViewDataColumn FieldName="CohortName" Caption="Cohort" AdaptivePriority="2" />
                                            <dx:BootstrapGridViewDataColumn FieldName="HubName" Caption="Hub" AdaptivePriority="3" />
                                            <dx:BootstrapGridViewDataColumn FieldName="StateName" Caption="State" AdaptivePriority="1" />
                                            <dx:BootstrapGridViewDataColumn FieldName="ProgramTypes" Caption="Type(s)" AdaptivePriority="4" Settings-AllowFilterBySearchPanel="False" Settings-AllowSort="False">
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
                                                    <div class="form-group">
                                                        <dx:BootstrapTextBox ID="txtProgramName" runat="server" Caption="Program Name">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <ValidationSettings ValidationGroup="vgProgram" ErrorDisplayMode="ImageWithText">
                                                                <RequiredField IsRequired="true" ErrorText="Program Name is required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapTextBox>
                                                    </div>
                                                </div>
                                                <div class="col-lg-4">
                                                    <div class="form-group">
                                                        <dx:BootstrapTextBox ID="txtProgramLocation" runat="server" Caption="Program Location">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <ValidationSettings ValidationGroup="vgProgram" ErrorDisplayMode="ImageWithText">
                                                                <RequiredField IsRequired="true" ErrorText="Program Location is required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapTextBox>
                                                    </div>
                                                </div>
                                                <div class="col-lg-4">
                                                    <div class="form-group">
                                                        <dx:BootstrapComboBox ID="ddProgramCohort" runat="server" Caption="Cohort" NullText="--Select--"
                                                            TextField="CohortName" ValueField="CohortPK" ValueType="System.Int32" 
                                                            IncrementalFilteringMode="Contains" AllowMouseWheel="false">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <ValidationSettings ValidationGroup="vgProgram" ErrorDisplayMode="ImageWithText">
                                                                <RequiredField IsRequired="true" ErrorText="Cohort is required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapComboBox>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="row">
                                                <div class="col-lg-4">
                                                    <div class="form-group">
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
                                                </div>
                                                <div class="col-lg-4">
                                                    <div class="form-group">
                                                        <dx:BootstrapDateEdit ID="deProgramEndDate" runat="server" Caption="Program End Date" 
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
                                            </div>
                                            <div class="row">
                                                <div class="col-lg-4">
                                                    <div class="form-group">
                                                        <dx:BootstrapComboBox ID="ddProgramHub" runat="server" Caption="Hub" NullText="--Select--"
                                                            TextField="Name" ValueField="HubPK" ValueType="System.Int32" 
                                                            IncrementalFilteringMode="Contains" AllowMouseWheel="false">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <ValidationSettings ValidationGroup="vgProgram" ErrorDisplayMode="ImageWithText">
                                                                <RequiredField IsRequired="true" ErrorText="Hub is required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapComboBox>
                                                    </div>
                                                </div>
                                                <div class="col-lg-4">
                                                    <div class="form-group">
                                                        <dx:BootstrapComboBox ID="ddProgramState" runat="server" Caption="State" NullText="--Select--"
                                                            TextField="Name" ValueField="StatePK" ValueType="System.Int32" 
                                                            IncrementalFilteringMode="Contains" AllowMouseWheel="false">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <ValidationSettings ValidationGroup="vgProgram" ErrorDisplayMode="ImageWithText">
                                                                <RequiredField IsRequired="true" ErrorText="State is required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapComboBox>
                                                    </div>
                                                </div>
                                                <div class="col-lg-4">
                                                    <div class="form-group">
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
                                            </div>
                                        </div>
                                        <div class="card-footer">
                                            <div class="d-flex align-items-center justify-content-center">
                                                <asp:HiddenField ID="hfAddEditProgramPK" runat="server" Value="0" />
                                                <uc:Submit ID="submitProgram" runat="server" ValidationGroup="vgProgram" OnSubmitClick="submitProgram_Click" OnCancelClick="submitProgram_CancelClick" OnValidationFailed="submitProgram_ValidationFailed" />
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
            <asp:AsyncPostBackTrigger ControlID="lbDeleteHub" />
            <asp:AsyncPostBackTrigger ControlID="lbDeleteProgram" />
            <asp:AsyncPostBackTrigger ControlID="lbDeleteCohort" />
        </Triggers>
    </asp:UpdatePanel>
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
                    <asp:LinkButton ID="lbDeleteHub" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteHubModal" OnClick="lbDeleteHub_Click"><i class="fas fa-check"></i> Yes</asp:LinkButton>
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
                    <asp:LinkButton ID="lbDeleteCohort" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteCohort" OnClick="lbDeleteCohort_Click"><i class="fas fa-check"></i> Yes</asp:LinkButton>
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
                    <asp:LinkButton ID="lbDeleteProgram" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteProgramModal" OnClick="lbDeleteProgram_Click"><i class="fas fa-check"></i> Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
