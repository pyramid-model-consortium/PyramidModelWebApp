<%@ Page Title="Employee" Language="C#" MasterPageFile="~/MasterPages/Dashboard.master" AutoEventWireup="true" CodeBehind="ProgramEmployee.aspx.cs" Inherits="Pyramid.Pages.ProgramEmployee" %>

<%@ Register Assembly="DevExpress.Web.v19.1, Version=19.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Data.Linq" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.Bootstrap.v19.1, Version=19.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>
<%@ Register TagPrefix="uc" TagName="Submit" Src="~/User_Controls/Submit.ascx" %>
<%@ Register TagPrefix="uc" TagName="Messaging" Src="~/User_Controls/MessagingSystem.ascx" %>

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
            //Highlight the correct dashboard link
            $('#lnkEmployeeDashboard').addClass('active');
            
            //Allow date format for DataTables sorting
            $.fn.dataTable.moment('MM/DD/YYYY');

            //Initialize the datatables
            if (!$.fn.dataTable.isDataTable('#tblTrainings')) {
                $('#tblTrainings').DataTable({
                    responsive: true,
                    columnDefs: [
                        { orderable: false, targets: [2] }
                    ],
                    order: [[ 0, 'asc' ]],
                    pageLength: 10,
                    dom: 'frtp',
                    language: {
                        searchPlaceholder: "Enter text to search...",
                        search: "",
                    }
                });
            }
            
            if (!$.fn.dataTable.isDataTable('#tblJobFunctions')) {
                $('#tblJobFunctions').DataTable({
                    responsive: true,
                    columnDefs: [
                        { orderable: false, targets: [3] }
                    ],
                    order: [[ 1, 'asc' ]],
                    pageLength: 10,
                    dom: 'frtp',
                    language: {
                        searchPlaceholder: "Enter text to search...",
                        search: "",
                    }
                });
            }
            
            if (!$.fn.dataTable.isDataTable('#tblClassroomAssignments')) {
                $('#tblClassroomAssignments').DataTable({
                    responsive: true,
                    columnDefs: [
                        { orderable: false, targets: [5] }
                    ],
                    order: [[ 2, 'asc' ]],
                    pageLength: 10,
                    dom: 'frtp',
                    language: {
                        searchPlaceholder: "Enter text to search...",
                        search: "",
                    }
                });
            }
            $('.dataTables_filter input').removeClass('form-control-sm');

            //Show/hide the view only fields
            setViewOnlyVisibility();
        }

        //This function controls whether or not the specify field for the termination reason is shown
        //based on the value in the ddTerminationReason Combo Box
        function showHideTerminationReasonSpecify() {
            //Get the termination reason
            var terminationReason = ddTerminationReason.GetText();

            //If the termination reason is other, show the specify div
            if (terminationReason.toLowerCase().includes('other')) {
                $('#divTerminationReasonSpecify').slideDown();
            }
            else {
                //The termination reason is not other, clear the specify text box and hide the specify div
                txtTerminationReasonSpecify.SetValue('');
                $('#divTerminationReasonSpecify').slideUp();
            }
        }

        //Validate the email field
        function validateEmail(s, e) {
            var email = e.value;
            var usedEmails = $('[ID$="hfUsedEmails"]').val();
            var usedEmailArray = usedEmails.split(',');

            if (email == null) {
                e.isValid = false;
                e.errorText = "Email is required!";
            }
            else if (usedEmailArray.includes(email.toLowerCase())) {
                e.isValid = false;
                e.errorText = "That email is already taken!";
            }
            else {
                e.isValid = true;
            }
        }

        //Validate the hire date field
        function validateHireDate(s, e) {
            var hireDate = e.value;
            var termDate = deTerminationDate.GetDate();

            if (hireDate == null) {
                e.isValid = false;
                e.errorText = "Hire Date is required!";
            }
            else if (termDate != null && hireDate >= termDate) {
                e.isValid = false;
                e.errorText = "Hire Date must be before the termination date!";
            }
            else if (hireDate != null && hireDate > new Date()) {
                e.isValid = false;
                e.errorText = "Hire Date cannot be in the future!";
            }
        }

        //Validate the termination date field
        function validateTerminationDate(s, e) {
            var terminationDate = e.value;
            var hireDate = deHireDate.GetDate();
            var terminationReason = ddTerminationReason.GetValue();

            if (terminationDate == null && terminationReason != null) {
                e.isValid = false;
                e.errorText = "Termination Date is required if you have a termination reason!";
            }
            else if (hireDate == null && terminationDate != null) {
                e.isValid = false;
                e.errorText = "Hire Date must be entered before the Termination Date!";
            }
            else if (terminationDate != null && terminationDate < hireDate) {
                e.isValid = false;
                e.errorText = "Termination Date must be after the hire date!";
            }
            else if (terminationDate != null && terminationDate > new Date()) {
                e.isValid = false;
                e.errorText = "Termination Date cannot be in the future!";
            }
        }

        //Validate the termination reason field
        function validateTerminationReason(s, e) {
            var terminationReason = e.value;
            var terminationDate = deTerminationDate.GetDate();

            if (terminationDate != null && terminationReason == null) {
                e.isValid = false;
                e.errorText = "Termination Reason is required if you have a Termination Date!"
            }
            else {
                e.isValid = true;
            }
        }

        //Validate the termination reason specify field
        function validateTerminationReasonSpecify(s, e) {
            var reasonSpecify = e.value;
            var terminationReason = ddTerminationReason.GetText();

            if ((reasonSpecify == null || reasonSpecify == ' ') && terminationReason.toLowerCase().includes('other')) {
                e.isValid = false;
                e.errorText = "Specify Termination Reason is required when the 'Other' termination reason is selected!";
            }
            else {
                e.isValid = true;
            }
        }

        //This function controls whether or not the specify field for the leave reason is shown
        //based on the value in the ddLeaveReason Combo Box
        function showHideLeaveReasonSpecify() {
            //Get the leave reason
            var leaveReason = ddLeaveReason.GetText();

            //If the leave reason is other, show the specify div
            if (leaveReason.toLowerCase().includes('other')) {
                $('#divLeaveReasonSpecify').slideDown();
            }
            else {
                //The leave reason is not other, clear the specify text box and hide the specify div
                txtLeaveReasonSpecify.SetValue('');
                $('#divLeaveReasonSpecify').slideUp();
            }
        }

        //Validate the leave reason field
        function validateLeaveReason(s, e) {
            var leaveReason = e.value;
            var leaveDate = deLeaveDate.GetDate();

            if (leaveDate != null && leaveReason == null) {
                e.isValid = false;
                e.errorText = "Leave Reason is required if you have a Leave Date!"
            }
            else {
                e.isValid = true;
            }
        }

        //Validate the leave reason specify field
        function validateLeaveReasonSpecify(s, e) {
            var reasonSpecify = e.value;
            var leaveReason = ddLeaveReason.GetText();

            if ((reasonSpecify == null || reasonSpecify == ' ') && leaveReason.toLowerCase().includes('other')) {
                e.isValid = false;
                e.errorText = "Specify Leave Reason is required when the 'Other' leave reason is selected!";
            }
            else {
                e.isValid = true;
            }
        }

        //This function allows multiple date edits to check against term and hire date
        function checkBetweenTermAndHireAndRequired(s, e) {
            //Get the date to check
            var dateToCheck = e.value;

            //Get the termination date and hire date
            var termDate = deTerminationDate.GetDate();
            var hireDate = deHireDate.GetDate();

            //Perform validation
            if (dateToCheck == null) {
                e.isValid = false;
                e.errorText = "This date is required!";
            }
            else if (termDate == null && (dateToCheck > new Date() || dateToCheck < hireDate)) {
                e.isValid = false;
                e.errorText = "This date must between the hire date and now!";
            }
            else if (termDate != null && (dateToCheck > termDate || dateToCheck < hireDate)) {
                e.isValid = false;
                e.errorText = "This date must between the hire date and termination date!";
            }
        }
    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Label ID="lblPageTitle" runat="server" Text="Employee" CssClass="h2"></asp:Label>
    <hr />
    <asp:HiddenField ID="hfViewOnly" runat="server" Value="False" />
    <asp:UpdatePanel ID="upMessaging" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <uc:Messaging ID="msgSys" runat="server" />
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="lbAddTraining" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="repeatTrainings" />
            <asp:AsyncPostBackTrigger ControlID="lbDeleteTraining" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="submitTraining" />
            <asp:AsyncPostBackTrigger ControlID="lbAddJobFunction" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="repeatJobFunctions" />
            <asp:AsyncPostBackTrigger ControlID="lbDeleteJobFunction" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="submitJobFunction" />
            <asp:AsyncPostBackTrigger ControlID="lbAddClassroomAssignment" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="repeatClassroomAssignments" />
            <asp:AsyncPostBackTrigger ControlID="lbDeleteClassroomAssignment" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="submitClassroomAssignment" />
            <asp:AsyncPostBackTrigger ControlID="submitEmployee" />
        </Triggers>
    </asp:UpdatePanel>
    <div class="row">
        <div class="col-lg-12">
            <div class="card bg-light">
                <div class="card-header">Basic Information</div>
                <div class="card-body">
                    <asp:UpdatePanel ID="upBasicInfo" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                        <ContentTemplate>
                            <asp:HiddenField ID="hfUsedEmails" runat="server" Value="" />
                            <div class="row">
                                <div class="col-md-6">
                                    <label>Program: </label>
                                    <asp:Label ID="lblProgram" runat="server" Text=""></asp:Label>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-4">
                                    <div class="form-group">
                                        <dx:BootstrapTextBox ID="txtFirstName" runat="server" Caption="First Name">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ValidationSettings ValidationGroup="vgEmployee" ErrorDisplayMode="ImageWithText">
                                                <RequiredField IsRequired="true" ErrorText="First Name is required!" />
                                            </ValidationSettings>
                                        </dx:BootstrapTextBox>
                                    </div>
                                </div>
                                <div class="col-lg-4">
                                    <div class="form-group">
                                        <dx:BootstrapTextBox ID="txtLastName" runat="server" Caption="Last Name">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ValidationSettings ValidationGroup="vgEmployee" ErrorDisplayMode="ImageWithText">
                                                <RequiredField IsRequired="true" ErrorText="Last Name is required!" />
                                            </ValidationSettings>
                                        </dx:BootstrapTextBox>
                                    </div>
                                </div>
                                <div class="col-lg-4">
                                    <div class="form-group">
                                        <dx:BootstrapTextBox ID="txtEmail" runat="server" Caption="Current Email" 
                                            ClientInstanceName="txtEmail" OnValidation="txtEmail_Validation">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ClientSideEvents Validation="validateEmail" />
                                            <ValidationSettings ValidationGroup="vgEmployee" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                <RequiredField IsRequired="true" ErrorText="Current Email is required!" />
                                                <RegularExpression ErrorText="Invalid Email address!" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" />
                                            </ValidationSettings>
                                        </dx:BootstrapTextBox>
                                    </div>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-4">
                                    <div class="form-group">
                                        <dx:BootstrapDateEdit ID="deHireDate" runat="server" Caption="Hire Date" EditFormat="Date" EditFormatString="MM/dd/yyyy"
                                            UseMaskBehavior="true" AllowMouseWheel="false" ClientInstanceName="deHireDate"
                                            OnValidation="deHireDate_Validation" PickerDisplayMode="Calendar" MinDate="01/01/1900">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                            <ClientSideEvents Validation="validateHireDate" />
                                            <CalendarProperties ShowClearButton="true"></CalendarProperties>
                                            <ValidationSettings ValidationGroup="vgEmployee" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                <RequiredField IsRequired="true" ErrorText="Date of Birth is required!" />
                                            </ValidationSettings>
                                        </dx:BootstrapDateEdit>
                                    </div>
                                </div>
                                <div class="col-lg-4">
                                    <div class="form-group">
                                        <dx:BootstrapDateEdit ID="deTerminationDate" runat="server" Caption="Termination Date" 
                                            EditFormat="Date" EditFormatString="MM/dd/yyyy" UseMaskBehavior="true"
                                            ClientInstanceName="deTerminationDate" 
                                            OnValidation="deTerminationDate_Validation" AllowMouseWheel="false" 
                                            PickerDisplayMode="Calendar" MinDate="01/01/1900">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                            <ClientSideEvents Validation="validateTerminationDate" />
                                            <CalendarProperties ShowClearButton="true"></CalendarProperties>
                                            <ValidationSettings ValidationGroup="vgEmployee" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                <RequiredField IsRequired="false" ErrorText="Termination Date is required!" />
                                            </ValidationSettings>
                                        </dx:BootstrapDateEdit>
                                    </div>
                                </div>
                                <div class="col-lg-4">
                                    <div class="form-group">
                                        <dx:BootstrapComboBox ID="ddTerminationReason" runat="server" Caption="Termination Reason" NullText="--Select--"
                                            TextField="Description" ValueField="CodeTermReasonPK" ValueType="System.Int32"
                                            IncrementalFilteringMode="Contains" AllowMouseWheel="false" 
                                            OnValidation="ddTerminationReason_Validation" ClientInstanceName="ddTerminationReason"
                                            AllowNull="true" ClearButton-DisplayMode="Always">
                                            <ClientSideEvents Validation="validateTerminationReason" Init="showHideTerminationReasonSpecify" SelectedIndexChanged="showHideTerminationReasonSpecify" />
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ValidationSettings ValidationGroup="vgEmployee" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                <RequiredField IsRequired="false" ErrorText="Termination Reason is required!" />
                                            </ValidationSettings>
                                        </dx:BootstrapComboBox>
                                        <div id="divTerminationReasonSpecify" style="display: none">
                                            <dx:BootstrapTextBox ID="txtTerminationReasonSpecify" runat="server" Caption="Specify Termination Reason" MaxLength="500"
                                                OnValidation="txtTerminationReasonSpecify_Validation" ClientInstanceName="txtTerminationReasonSpecify">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <ClientSideEvents Validation="validateTerminationReasonSpecify" />
                                                <ValidationSettings ValidationGroup="vgEmployee" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                </ValidationSettings>
                                            </dx:BootstrapTextBox>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </ContentTemplate>
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="submitEmployee" />
                        </Triggers>
                    </asp:UpdatePanel>
                </div>
            </div>
        </div>
    </div>
    <div id="divEditOnly" runat="server" visible="false">
        <div class="row">
            <div class="col-xl-6">
                <asp:HiddenField ID="hfDeleteTrainingPK" runat="server" Value="0" />
                <asp:UpdatePanel ID="upTrainings" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                    <ContentTemplate>
                        <div class="card bg-light">
                            <div class="card-header">
                                Trainings
                                <asp:LinkButton ID="lbAddTraining" runat="server" CssClass="btn btn-loader btn-primary float-right hide-on-view hidden" OnClick="lbAddTraining_Click"><i class="fas fa-plus"></i> Add New Training</asp:LinkButton>
                            </div>
                            <div class="card-body">
                                <div class="row">
                                    <div class="col-lg-12">
                                        <label>All Trainings</label>
                                        <asp:Repeater ID="repeatTrainings" runat="server" ItemType="Pyramid.Models.Training">
                                            <HeaderTemplate>
                                                <table id="tblTrainings" class="table table-striped table-bordered table-hover">
                                                    <thead>
                                                        <tr>
                                                            <th data-priority="1">Date</th>
                                                            <th>Type</th>
                                                            <th data-priority="2"></th>
                                                        </tr>
                                                    </thead>
                                                    <tbody>
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                        <tr>
                                                            <td><%# Item.TrainingDate.ToString("MM/dd/yyyy") %></td>
                                                            <td><%# Item.CodeTraining.Description %></td>
                                                            <td class="text-center">
                                                                <div class="btn-group">
                                                                    <button type="button" class="btn btn-secondary dropdown-toggle hide-on-view hidden" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                                        Actions
                                                                    </button>
                                                                    <div class="dropdown-menu dropdown-menu-right">
                                                                        <asp:LinkButton ID="lbEditTraining" runat="server" CssClass="dropdown-item" OnClick="lbEditTraining_Click"><i class="fas fa-edit"></i> Edit</asp:LinkButton>
                                                                        <button class="dropdown-item delete-gridview" data-pk='<%# Item.TrainingPK %>' data-hf="hfDeleteTrainingPK" data-target="#divDeleteTrainingModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                                    </div>
                                                                </div>
                                                                <asp:HiddenField ID="hfTrainingPK" runat="server" Value='<%# Item.TrainingPK %>' />
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
                                        <div id="divAddEditTraining" runat="server" class="card mt-2" visible="false">
                                            <div class="card-header">
                                                <asp:Label ID="lblAddEditTraining" runat="server" Text=""></asp:Label>
                                            </div>
                                            <div class="card-body">
                                                <div class="row">
                                                    <div class="col-xl-12">
                                                        <div class="form-group">
                                                            <dx:BootstrapDateEdit ID="deTrainingDate" runat="server" Caption="Date" EditFormat="Date" EditFormatString="MM/dd/yyyy"
                                                                UseMaskBehavior="true" AllowMouseWheel="false" 
                                                                PickerDisplayMode="Calendar" MinDate="01/01/1900">
                                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                                <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                                                <CalendarProperties ShowClearButton="true"></CalendarProperties>
                                                                <ValidationSettings ValidationGroup="vgTraining" ErrorDisplayMode="ImageWithText">
                                                                    <RequiredField IsRequired="true" ErrorText="Date is required!" />
                                                                </ValidationSettings>
                                                            </dx:BootstrapDateEdit>
                                                        </div>
                                                    </div>
                                                    <div class="col-xl-12">
                                                        <div class="form-group">
                                                            <dx:BootstrapComboBox ID="ddTraining" runat="server" Caption="Type" NullText="--Select--" ValueType="System.Int32"
                                                                TextField="Description" ValueField="CodeTrainingPK" 
                                                                IncrementalFilteringMode="Contains" AllowMouseWheel="false">
                                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                                <ValidationSettings ValidationGroup="vgTraining" ErrorDisplayMode="ImageWithText">
                                                                    <RequiredField IsRequired="true" ErrorText="Type is required!" />
                                                                </ValidationSettings>
                                                            </dx:BootstrapComboBox>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="card-footer">
                                                <div class="center-content">
                                                    <asp:HiddenField ID="hfAddEditTrainingPK" runat="server" Value="0" />
                                                    <uc:Submit ID="submitTraining" runat="server" ValidationGroup="vgTraining" OnSubmitClick="submitTraining_Click" OnCancelClick="submitTraining_CancelClick" OnValidationFailed="submitTraining_ValidationFailed" />
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="lbDeleteTraining" EventName="Click" />
                    </Triggers>
                </asp:UpdatePanel>
            </div>
            <div class="col-xl-6">
                <asp:HiddenField ID="hfDeleteJobFunctionPK" runat="server" Value="0" />
                <asp:UpdatePanel ID="upJobFunction" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                    <ContentTemplate>
                        <div class="card bg-light">
                            <div class="card-header">
                                Job Functions
                                <asp:LinkButton ID="lbAddJobFunction" runat="server" CssClass="btn btn-loader btn-primary float-right hide-on-view hidden" OnClick="lbAddJobFunction_Click"><i class="fas fa-plus"></i> Add New Job Function</asp:LinkButton>
                            </div>
                            <div class="card-body">
                                <div class="row">
                                    <div class="col-lg-12">
                                        <label>All Job Functions</label>
                                        <asp:Repeater ID="repeatJobFunctions" runat="server" ItemType="Pyramid.Models.JobFunction">
                                            <HeaderTemplate>
                                                <table id="tblJobFunctions" class="table table-striped table-bordered table-hover">
                                                    <thead>
                                                        <tr>
                                                            <th data-priority="1">Type</th>
                                                            <th data-priority="3">Start Date</th>
                                                            <th class="min-tablet-l">End Date</th>
                                                            <th data-priority="2"></th>
                                                        </tr>
                                                    </thead>
                                                    <tbody>
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                        <tr>
                                                            <td><%# Item.CodeJobType.Description %></td>
                                                            <td class="job-start-date"><%# Item.StartDate.ToString("MM/dd/yyyy") %></td>
                                                            <td class="job-end-date"><%# (Item.EndDate.HasValue ? Item.EndDate.Value.ToString("MM/dd/yyyy") : "") %></td>
                                                            <td class="text-center">
                                                                <div class="btn-group">
                                                                    <button type="button" class="btn btn-secondary dropdown-toggle hide-on-view hidden" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                                        Actions
                                                                    </button>
                                                                    <div class="dropdown-menu dropdown-menu-right">
                                                                        <asp:LinkButton ID="lbEditJobFunction" runat="server" CssClass="dropdown-item" OnClick="lbEditJobFunction_Click"><i class="fas fa-edit"></i> Edit</asp:LinkButton>
                                                                        <button class="dropdown-item delete-gridview" data-pk='<%# Item.JobFunctionPK %>' data-hf="hfDeleteJobFunctionPK" data-target="#divDeleteJobFunctionModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                                    </div>
                                                                </div>
                                                                <asp:HiddenField ID="hfJobFunctionPK" runat="server" Value='<%# Item.JobFunctionPK %>' />
                                                                <asp:HiddenField ID="hfJobTypeCodeFK" runat="server" Value='<%# Item.JobTypeCodeFK %>' />
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
                                        <div id="divAddEditJobFunction" runat="server" class="card mt-2" visible="false">
                                            <div class="card-header">
                                                <asp:Label ID="lblAddEditJobFunction" runat="server" Text=""></asp:Label>
                                            </div>
                                            <div class="card-body">
                                                <div class="row">
                                                    <div class="col-xl-12">
                                                        <div class="form-group">
                                                            <dx:BootstrapComboBox ID="ddJobType" runat="server" Caption="Job Function" NullText="--Select--" ValueType="System.Int32"
                                                                TextField="Description" ValueField="CodeJobTypePK" 
                                                                IncrementalFilteringMode="Contains" AllowMouseWheel="false"
                                                                ClientInstanceName="ddJobType">
                                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                                <ValidationSettings ValidationGroup="vgJobFunction" ErrorDisplayMode="ImageWithText">
                                                                    <RequiredField IsRequired="true" ErrorText="Job Function is required!" />
                                                                </ValidationSettings>
                                                            </dx:BootstrapComboBox>
                                                        </div>
                                                    </div>
                                                    <div class="col-xl-12">
                                                        <div class="form-group">
                                                            <dx:BootstrapDateEdit ID="deJobStartDate" runat="server" Caption="Start Date" 
                                                                EditFormat="Date" EditFormatString="MM/dd/yyyy"
                                                                UseMaskBehavior="true" ClientInstanceName="deJobStartDate" 
                                                                OnValidation="deJobStartDate_Validation" 
                                                                AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900">
                                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                                <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                                                <CalendarProperties ShowClearButton="true"></CalendarProperties>
                                                                <ValidationSettings ValidationGroup="vgJobFunction" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                                    <RequiredField IsRequired="true" ErrorText="Start Date is required!" />
                                                                </ValidationSettings>
                                                            </dx:BootstrapDateEdit>
                                                        </div>
                                                    </div>
                                                    <div class="col-xl-12">
                                                        <div class="form-group">
                                                            <dx:BootstrapDateEdit ID="deJobEndDate" runat="server" Caption="End Date" EditFormat="Date" EditFormatString="MM/dd/yyyy"
                                                                UseMaskBehavior="true" ClientInstanceName="deJobEndDate" 
                                                                OnValidation="deJobEndDate_Validation" 
                                                                AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900">
                                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                                <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                                                <CalendarProperties ShowClearButton="true"></CalendarProperties>
                                                                <ValidationSettings ValidationGroup="vgJobFunction" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                                    <RequiredField IsRequired="false" ErrorText="End Date is required!" />
                                                                </ValidationSettings>
                                                            </dx:BootstrapDateEdit>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="card-footer">
                                                <div class="center-content">
                                                    <asp:HiddenField ID="hfAddEditJobFunctionPK" runat="server" Value="0" />
                                                    <uc:Submit ID="submitJobFunction" runat="server" ValidationGroup="vgJobFunction" OnSubmitClick="submitJobFunction_Click" OnCancelClick="submitJobFunction_CancelClick" OnValidationFailed="submitJobFunction_ValidationFailed" />
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="lbDeleteJobFunction" EventName="Click" />
                    </Triggers>
                </asp:UpdatePanel>
            </div>
        </div>
        <div class="row">
            <div class="col-xl-12">
                <asp:HiddenField ID="hfDeleteClassroomAssignmentPK" runat="server" Value="0" />
                <asp:UpdatePanel ID="upClassroomAssignment" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                    <ContentTemplate>
                        <div class="card bg-light">
                            <div class="card-header">
                                Classroom Assignment History
                                <asp:LinkButton ID="lbAddClassroomAssignment" runat="server" CssClass="btn btn-loader btn-primary float-right hide-on-view hidden" OnClick="lbAddClassroomAssignment_Click"><i class="fas fa-plus"></i> Add New Assignment</asp:LinkButton>
                            </div>
                            <div class="card-body">
                                <div class="row">
                                    <div class="col-lg-12">
                                        <label>All Classroom Assignments for this Employee</label>
                                        <asp:Repeater ID="repeatClassroomAssignments" runat="server" ItemType="Pyramid.Models.EmployeeClassroom">
                                            <HeaderTemplate>
                                                <table id="tblClassroomAssignments" class="table table-striped table-bordered table-hover">
                                                    <thead>
                                                        <tr>
                                                            <th data-priority="1">Classroom</th>
                                                            <th data-priority="3">Classroom Job</th>
                                                            <th data-priority="4">Assign Date</th>
                                                            <th class="min-tablet-l">Leave Date</th>
                                                            <th class="min-tablet-l">Leave Reason</th>
                                                            <th data-priority="2"></th>
                                                        </tr>
                                                    </thead>
                                                    <tbody>
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <tr>
                                                    <td><%# "(" + Item.Classroom.ProgramSpecificID + ") " + Item.Classroom.Name %></td>
                                                    <td><%# Item.CodeJobType.Description %></td>
                                                    <td><%# Item.AssignDate.ToString("MM/dd/yyyy") %></td>
                                                    <td class="leave-date"><%# (Item.LeaveDate.HasValue ? Item.LeaveDate.Value.ToString("MM/dd/yyyy") : "") %></td>
                                                    <td><%# (Item.CodeEmployeeLeaveReason != null ? Item.CodeEmployeeLeaveReason.Description +  (!String.IsNullOrWhiteSpace(Item.LeaveReasonSpecify) ? " (" + Item.LeaveReasonSpecify + ")" :"") : "") %></td>
                                                    <td class="text-center">
                                                        <div class="btn-group">
                                                            <button type="button" class="btn btn-secondary dropdown-toggle hide-on-view hidden" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                                Actions
                                                            </button>
                                                            <div class="dropdown-menu dropdown-menu-right">
                                                                <asp:LinkButton ID="lbEditClassroomAssignment" runat="server" CssClass="dropdown-item" OnClick="lbEditClassroomAssignment_Click"><i class="fas fa-edit"></i> Edit</asp:LinkButton>
                                                                <button class="dropdown-item delete-gridview" data-pk='<%# Item.EmployeeClassroomPK %>' data-hf="hfDeleteClassroomAssignmentPK" data-target="#divDeleteClassroomAssignmentModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                            </div>
                                                        </div>
                                                        <asp:HiddenField ID="hfClassroomFK" runat="server" Value='<%# Item.ClassroomFK %>' />   
                                                        <asp:HiddenField ID="hfClassroomAssignmentPK" runat="server" Value='<%# Item.EmployeeClassroomPK %>' />
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
                                        <div id="divAddEditClassroomAssignment" runat="server" class="card mt-2" visible="false">
                                            <div class="card-header">
                                                <asp:Label ID="lblAddEditClassroomAssignment" runat="server" Text=""></asp:Label>
                                            </div>
                                            <div class="card-body">
                                                <div class="row">
                                                    <div class="col-lg-4">
                                                        <div class="form-group">
                                                            <dx:BootstrapDateEdit ID="deAssignDate" runat="server" Caption="Assign Date"
                                                                EditFormat="Date" EditFormatString="MM/dd/yyyy"
                                                                UseMaskBehavior="true" ClientInstanceName="deAssignDate" 
                                                                OnValidation="deAssignDate_Validation" 
                                                                AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900">
                                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                                <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                                                <CalendarProperties ShowClearButton="true"></CalendarProperties>
                                                                <ValidationSettings ValidationGroup="vgClassroomAssignment" ErrorDisplayMode="ImageWithText">
                                                                    <RequiredField IsRequired="true" ErrorText="Assign Date is required!" />
                                                                </ValidationSettings>
                                                            </dx:BootstrapDateEdit>
                                                        </div>
                                                    </div>
                                                    <div class="col-lg-4">
                                                        <div class="form-group">
                                                            <dx:BootstrapComboBox ID="ddClassroom" runat="server" Caption="Classroom" NullText="--Select--" ValueType="System.Int32"
                                                                TextField="IdAndName" ValueField="ClassroomPK" 
                                                                IncrementalFilteringMode="Contains" AllowMouseWheel="false"
                                                                ClientInstanceName="ddClassroom">
                                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                                <ValidationSettings ValidationGroup="vgClassroomAssignment" ErrorDisplayMode="ImageWithText">
                                                                    <RequiredField IsRequired="true" ErrorText="Classroom is required!" />
                                                                </ValidationSettings>
                                                            </dx:BootstrapComboBox>
                                                        </div>
                                                    </div>
                                                    <div class="col-lg-4">
                                                        <div class="form-group">
                                                            <dx:BootstrapComboBox ID="ddClassroomJobType" runat="server" Caption="Classroom Job" NullText="--Select--" ValueType="System.Int32"
                                                                TextField="Description" ValueField="CodeJobTypePK" 
                                                                IncrementalFilteringMode="Contains" AllowMouseWheel="false"
                                                                ClientInstanceName="ddClassroomJobType"
                                                                OnValidation="ddClassroomJobType_Validation">
                                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                                <ValidationSettings ValidationGroup="vgClassroomAssignment" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                                    <RequiredField IsRequired="true" ErrorText="Classroom Job is required!" />
                                                                </ValidationSettings>
                                                            </dx:BootstrapComboBox>
                                                        </div>
                                                    </div>
                                                </div>
                                                <div class="row">
                                                    <div class="col-lg-4">
                                                        <div class="form-group">
                                                            <dx:BootstrapDateEdit ID="deLeaveDate" runat="server" Caption="Leave Date" EditFormat="Date"
                                                                EditFormatString="MM/dd/yyyy" UseMaskBehavior="true"
                                                                ClientInstanceName="deLeaveDate" 
                                                                OnValidation="deLeaveDate_Validation" 
                                                                AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900">
                                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                                <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                                                <CalendarProperties ShowClearButton="true"></CalendarProperties>
                                                                <ValidationSettings ValidationGroup="vgClassroomAssignment" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                                    <RequiredField IsRequired="false" ErrorText="Leave Date is required!" />
                                                                </ValidationSettings>
                                                            </dx:BootstrapDateEdit>
                                                        </div>
                                                    </div>
                                                    <div class="col-lg-4">
                                                        <dx:BootstrapComboBox ID="ddLeaveReason" runat="server" Caption="Leave Reason" NullText="--Select--"
                                                            TextField="Description" ValueField="CodeEmployeeLeaveReasonPK" ValueType="System.Int32"
                                                            IncrementalFilteringMode="Contains" AllowMouseWheel="false"
                                                            OnValidation="ddLeaveReason_Validation" ClientInstanceName="ddLeaveReason"
                                                            AllowNull="true" ClearButton-DisplayMode="Always">
                                                            <ClientSideEvents Validation="validateLeaveReason" Init="showHideLeaveReasonSpecify" SelectedIndexChanged="showHideLeaveReasonSpecify" />
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <ValidationSettings ValidationGroup="vgClassroomAssignment" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="false" ErrorText="Leave Reason is required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapComboBox>
                                                        <div id="divLeaveReasonSpecify" style="display: none">
                                                            <dx:BootstrapTextBox ID="txtLeaveReasonSpecify" runat="server" Caption="Specify Leave Reason" MaxLength="500"
                                                                OnValidation="txtLeaveReasonSpecify_Validation" ClientInstanceName="txtLeaveReasonSpecify">
                                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                                <ClientSideEvents Validation="validateLeaveReasonSpecify" />
                                                                <ValidationSettings ValidationGroup="vgClassroomAssignment" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                                </ValidationSettings>
                                                            </dx:BootstrapTextBox>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="card-footer">
                                                <div class="center-content">
                                                    <asp:HiddenField ID="hfAddEditClassroomAssignmentPK" runat="server" Value="0" />
                                                    <uc:Submit ID="submitClassroomAssignment" runat="server" ValidationGroup="vgClassroomAssignment" OnSubmitClick="submitClassroomAssignment_Click" OnCancelClick="submitClassroomAssignment_CancelClick" OnValidationFailed="submitClassroomAssignment_ValidationFailed" />
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="lbDeleteClassroomAssignment" EventName="Click" />
                    </Triggers>
                </asp:UpdatePanel>
            </div>
        </div>
    </div>
    <div class="page-footer">
        <uc:Submit ID="submitEmployee" runat="server" ValidationGroup="vgEmployee" OnSubmitClick="submitEmployee_Click" OnCancelClick="submitEmployee_CancelClick" OnValidationFailed="submitEmployee_ValidationFailed" />
    </div>
    <div class="modal" id="divDeleteClassroomAssignmentModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete Classroom Assignment</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this classroom assignment?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteClassroomAssignment" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteClassroomAssignmentModal" OnClick="lbDeleteClassroomAssignment_Click"><i class="fas fa-check"></i> Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
    <div class="modal" id="divDeleteTrainingModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete Training</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this training?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteTraining" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteTrainingModal" OnClick="lbDeleteTraining_Click"><i class="fas fa-check"></i> Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
    <div class="modal" id="divDeleteJobFunctionModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete Job Function</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this job function?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteJobFunction" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteJobFunctionModal" OnClick="lbDeleteJobFunction_Click"><i class="fas fa-check"></i> Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
