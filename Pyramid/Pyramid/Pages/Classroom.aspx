<%@ Page Title="Classroom" Language="C#" MasterPageFile="~/MasterPages/Dashboard.master" AutoEventWireup="true" CodeBehind="Classroom.aspx.cs" Inherits="Pyramid.Pages.Classroom" %>

<%@ Register Assembly="DevExpress.Web.v19.1, Version=19.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Data.Linq" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.Bootstrap.v19.1, Version=19.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>
<%@ Register TagPrefix="uc" TagName="Classroom" Src="~/User_Controls/Classroom.ascx" %>
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
            $('#lnkClassroomDashboard').addClass('active');

            //Allow date format for DataTables sorting
            $.fn.dataTable.moment( 'MM/DD/YYYY' );

            //Initialize the datatables
            if (!$.fn.dataTable.isDataTable('#tblChildClassroomAssignments')) {
                $('#tblChildClassroomAssignments').DataTable({
                    responsive: true,
                    columnDefs: [
                        { orderable: false, targets: [4] }
                    ],
                    order: [[ 0, 'asc' ], [ 1, 'asc' ]],
                    pageLength: 10,
                    dom: 'frtp',
                    language: {
                        searchPlaceholder: "Enter text to search...",
                        search: "",
                    }
                });
            }
            
            if (!$.fn.dataTable.isDataTable('#tblEmployeeClassroomAssignments')) {
                $('#tblEmployeeClassroomAssignments').DataTable({
                    responsive: true,
                    columnDefs: [
                        { orderable: false, targets: [5] }
                    ],
                    order: [[ 0, 'asc' ], [ 2, 'asc' ]],
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

        //This function controls whether or not the specify field for the leave reason is shown
        //based on the value in the ddLeaveReason Combo Box
        function showHideChildLeaveReasonSpecify() {
            //Get the leave reason
            var leaveReason = ddChildLeaveReason.GetText();

            //If the leave reason is other, show the specify div
            if (leaveReason.toLowerCase().includes('other')) {
                $('#divChildLeaveReasonSpecify').slideDown();
            }
            else {
                //The leave reason is not other, clear the specify text box and hide the specify div
                txtChildLeaveReasonSpecify.SetValue('');
                $('#divChildLeaveReasonSpecify').slideUp();
            }
        }

        //Validate the leave reason field
        function validateChildLeaveReason(s, e) {
            var leaveReason = e.value;
            var leaveDate = deChildLeaveDate.GetDate();

            if (leaveDate != null && leaveReason == null) {
                e.isValid = false;
                e.errorText = "Leave Reason is required if you have a Leave Date!"
            }
            else {
                e.isValid = true;
            }
        }

        //Validate the leave reason specify field
        function validateChildLeaveReasonSpecify(s, e) {
            var reasonSpecify = e.value;
            var leaveReason = ddChildLeaveReason.GetText();

            if ((reasonSpecify == null || reasonSpecify == ' ') && leaveReason.toLowerCase().includes('other')) {
                e.isValid = false;
                e.errorText = "Specify Leave Reason is required when the 'Other' leave reason is selected!";
            }
            else {
                e.isValid = true;
            }
        }

        //This function controls whether or not the specify field for the leave reason is shown
        //based on the value in the ddLeaveReason Combo Box
        function showHideEmployeeLeaveReasonSpecify() {
            //Get the leave reason
            var leaveReason = ddEmployeeLeaveReason.GetText();

            //If the leave reason is other, show the specify div
            if (leaveReason.toLowerCase().includes('other')) {
                $('#divEmployeeLeaveReasonSpecify').slideDown();
            }
            else {
                //The leave reason is not other, clear the specify text box and hide the specify div
                txtEmployeeLeaveReasonSpecify.SetValue('');
                $('#divEmployeeLeaveReasonSpecify').slideUp();
            }
        }

        //Validate the leave reason field
        function validateEmployeeLeaveReason(s, e) {
            var leaveReason = e.value;
            var leaveDate = deEmployeeLeaveDate.GetDate();

            if (leaveDate != null && leaveReason == null) {
                e.isValid = false;
                e.errorText = "Leave Reason is required if you have a Leave Date!"
            }
            else {
                e.isValid = true;
            }
        }

        //Validate the leave reason specify field
        function validateEmployeeLeaveReasonSpecify(s, e) {
            var reasonSpecify = e.value;
            var leaveReason = ddEmployeeLeaveReason.GetText();

            if ((reasonSpecify == null || reasonSpecify == ' ') && leaveReason.toLowerCase().includes('other')) {
                e.isValid = false;
                e.errorText = "Specify Leave Reason is required when the 'Other' leave reason is selected!";
            }
            else {
                e.isValid = true;
            }
        }
    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Label ID="lblPageTitle" runat="server" Text="Child" CssClass="h2"></asp:Label>
    <hr />
    <asp:HiddenField ID="hfViewOnly" runat="server" Value="False" />
    <asp:UpdatePanel ID="upMessaging" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <uc:Messaging ID="msgSys" runat="server" />
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="repeatChildClassroomAssignments" />
            <asp:AsyncPostBackTrigger ControlID="lbDeleteChildClassroomAssignment" />
            <asp:AsyncPostBackTrigger ControlID="submitChildClassroomAssignment" />
            <asp:AsyncPostBackTrigger ControlID="repeatEmployeeClassroomAssignments" />
            <asp:AsyncPostBackTrigger ControlID="lbDeleteEmployeeClassroomAssignment" />
            <asp:AsyncPostBackTrigger ControlID="submitEmployeeClassroomAssignment" />
            <asp:AsyncPostBackTrigger ControlID="submitClassroom" />
        </Triggers>
    </asp:UpdatePanel>
    <div class="row">
        <div class="col-lg-12">
            <div class="card bg-light">
                <div class="card-header">Basic Information</div>
                <div class="card-body">
                    <asp:UpdatePanel ID="upBasicInfo" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                        <ContentTemplate>
                            <div class="row">
                                <div class="col-md-6">
                                    <label>Program: </label>
                                    <asp:Label ID="lblProgram" runat="server" Text=""></asp:Label>
                                </div>
                            </div>
                            <uc:Classroom ID="classroomControl" runat="server"></uc:Classroom>
                        </ContentTemplate>
                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="submitClassroom" />
                        </Triggers>
                    </asp:UpdatePanel>
                </div>
            </div>
        </div>
    </div>
    <div id="divEditOnly" runat="server" visible="false">
        <div class="row">
            <div class="col-xl-12">
                <asp:HiddenField ID="hfDeleteChildClassroomAssignmentPK" runat="server" Value="0" />
                <asp:UpdatePanel ID="upChildClassroomAssignment" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                    <ContentTemplate>
                        <div class="card bg-light">
                            <div class="card-header">
                                Child Assignment History
                            </div>
                            <div class="card-body">
                                <div class="row">
                                    <div class="col-lg-12">
                                        <label>Children Assigned to this Classroom</label>
                                        <asp:Repeater ID="repeatChildClassroomAssignments" runat="server">
                                            <HeaderTemplate>
                                                <table id="tblChildClassroomAssignments" class="table table-striped table-bordered table-hover">
                                                    <thead>
                                                        <tr>
                                                            <th data-priority="1">Child</th>
                                                            <th data-priority="3">Assign Date</th>
                                                            <th class="min-tablet-l">Leave Date</th>
                                                            <th class="min-tablet-l">Leave Reason</th>
                                                            <th data-priority="2"></th>
                                                        </tr>
                                                    </thead>
                                                    <tbody>
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                        <tr>
                                                            <td><%# Eval("ChildIdAndName") %></td>
                                                            <td><%# Eval("AssignDate", "{0:MM/dd/yyyy}") %></td>
                                                            <td class="leave-date"><%# Eval("LeaveDate", "{0:MM/dd/yyyy}") %></td>
                                                            <td><%# Eval("LeaveReason") %></td>
                                                            <td class="text-center">
                                                                <div class="btn-group">
                                                                    <button id="btnActions" type="button" class="btn btn-secondary dropdown-toggle hide-on-view hidden" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                                        Actions
                                                                    </button>
                                                                    <div class="dropdown-menu dropdown-menu-right" aria-labelledby="btnActions">
                                                                        <asp:LinkButton ID="lbEditChildClassroomAssignment" runat="server" CssClass="dropdown-item" OnClick="lbEditChildClassroomAssignment_Click"><i class="fas fa-edit"></i> Edit</asp:LinkButton>
                                                                        <button class="dropdown-item delete-gridview" data-pk='<%# Eval("ChildClassroomPK") %>' data-hf="hfDeleteChildClassroomAssignmentPK" data-target="#divDeleteChildClassroomAssignmentModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                                    </div>
                                                                </div>
                                                                <asp:HiddenField ID="hfClassroomAssignmentPK" runat="server" Value='<%# Eval("ChildClassroomPK") %>' />
                                                                <asp:HiddenField ID="hfChildFK" runat="server" Value='<%# Eval("ChildFK") %>' />
                                                                <asp:HiddenField ID="hfEnrollmentDate" runat="server" Value='<%# Eval("EnrollmentDate") %>' />
                                                                <asp:HiddenField ID="hfDischargeDate" runat="server" Value='<%# Eval("DischargeDate") %>' />
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
                                        <div id="divAddEditChildClassroomAssignment" runat="server" class="card mt-2" visible="false">
                                            <div class="card-header">
                                                <asp:Label ID="lblAddEditChildClassroomAssignment" runat="server" Text=""></asp:Label>
                                            </div>
                                            <div class="card-body">
                                                <div class="row">
                                                    <div class="col-lg-6">
                                                        <div class="form-group">
                                                            <dx:BootstrapComboBox ID="ddChild" runat="server" Caption="Child" NullText="--Select--"
                                                                TextField="IdAndName" ValueField="ChildPK" ValueType="System.Int32" 
                                                                IncrementalFilteringMode="Contains" AllowMouseWheel="false">
                                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                                <ValidationSettings ValidationGroup="vgChildClassroomAssignment" ErrorDisplayMode="ImageWithText">
                                                                    <RequiredField IsRequired="true" ErrorText="Child is required!" />
                                                                </ValidationSettings>
                                                            </dx:BootstrapComboBox>
                                                        </div>
                                                    </div>
                                                    <div class="col-lg-6">
                                                        <div class="form-group">
                                                            <dx:BootstrapDateEdit ID="deChildAssignDate" runat="server" Caption="Assign Date" 
                                                                EditFormat="Date" EditFormatString="MM/dd/yyyy"
                                                                UseMaskBehavior="true" ClientInstanceName="deChildAssignDate" 
                                                                OnValidation="deChildAssignDate_Validation" AllowMouseWheel="false" 
                                                                PickerDisplayMode="Calendar" MinDate="01/01/1900">
                                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                                <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                                                <CalendarProperties ShowClearButton="true"></CalendarProperties>
                                                                <ValidationSettings ValidationGroup="vgChildClassroomAssignment" ErrorDisplayMode="ImageWithText">
                                                                    <RequiredField IsRequired="true" ErrorText="Assign Date is required!" />
                                                                </ValidationSettings>
                                                            </dx:BootstrapDateEdit>
                                                        </div>
                                                    </div>
                                                </div>
                                                <div class="row">
                                                    <div class="col-lg-6">
                                                        <div class="form-group">
                                                            <dx:BootstrapDateEdit ID="deChildLeaveDate" runat="server" Caption="Leave Date" EditFormat="Date"
                                                                EditFormatString="MM/dd/yyyy" UseMaskBehavior="true"
                                                                ClientInstanceName="deChildLeaveDate" OnValidation="deChildLeaveDate_Validation" 
                                                                AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900">
                                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                                <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                                                <CalendarProperties ShowClearButton="true"></CalendarProperties>
                                                                <ValidationSettings ValidationGroup="vgChildClassroomAssignment" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                                    <RequiredField IsRequired="false" ErrorText="Leave Date is required!" />
                                                                </ValidationSettings>
                                                            </dx:BootstrapDateEdit>
                                                        </div>
                                                    </div>
                                                    <div class="col-lg-6">
                                                        <dx:BootstrapComboBox ID="ddChildLeaveReason" runat="server" Caption="Leave Reason" NullText="--Select--"
                                                            TextField="Description" ValueField="CodeChildLeaveReasonPK" ValueType="System.Int32"
                                                            IncrementalFilteringMode="Contains"  AllowMouseWheel="false"
                                                            OnValidation="ddChildLeaveReason_Validation" ClientInstanceName="ddChildLeaveReason" 
                                                            AllowNull="true" ClearButton-DisplayMode="Always">
                                                            <ClientSideEvents Validation="validateChildLeaveReason" Init="showHideChildLeaveReasonSpecify" SelectedIndexChanged="showHideChildLeaveReasonSpecify" />
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <ValidationSettings ValidationGroup="vgChildClassroomAssignment" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="false" ErrorText="Leave Reason is required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapComboBox>
                                                        <div id="divChildLeaveReasonSpecify" style="display: none">
                                                            <dx:BootstrapTextBox ID="txtChildLeaveReasonSpecify" runat="server" Caption="Specify Leave Reason" MaxLength="500"
                                                                OnValidation="txtChildLeaveReasonSpecify_Validation" ClientInstanceName="txtChildLeaveReasonSpecify">
                                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                                <ClientSideEvents Validation="validateChildLeaveReasonSpecify" />
                                                                <ValidationSettings ValidationGroup="vgChildClassroomAssignment" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                                </ValidationSettings>
                                                            </dx:BootstrapTextBox>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="card-footer">
                                                <div class="center-content">
                                                    <asp:HiddenField ID="hfAddEditChildClassroomAssignmentPK" runat="server" Value="0" />
                                                    <asp:HiddenField ID="hfAddEditChildClassroomChildPK" runat="server" Value="0" />
                                                    <asp:HiddenField ID="hfAddEditChildClassroomEnrollmentDate" runat="server" Value="" />
                                                    <asp:HiddenField ID="hfAddEditChildClassroomDischargeDate" runat="server" Value="" />
                                                    <uc:Submit ID="submitChildClassroomAssignment" runat="server" ValidationGroup="vgChildClassroomAssignment" OnSubmitClick="submitChildClassroomAssignment_Click" OnCancelClick="submitChildClassroomAssignment_CancelClick" OnValidationFailed="submitChildClassroomAssignment_ValidationFailed" />
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="lbDeleteChildClassroomAssignment" EventName="Click" />
                    </Triggers>
                </asp:UpdatePanel>
            </div>
        </div>
        <div class="row">
            <div class="col-xl-12">
                <asp:HiddenField ID="hfDeleteEmployeeClassroomAssignmentPK" runat="server" Value="0" />
                <asp:UpdatePanel ID="upEmployeeClassroomAssignment" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                    <ContentTemplate>
                        <div class="card bg-light">
                            <div class="card-header">
                                Employee Assignment History
                            </div>
                            <div class="card-body">
                                <div class="row">
                                    <div class="col-lg-12">
                                        <label>Employees Assigned to this Classroom</label>
                                        <asp:Repeater ID="repeatEmployeeClassroomAssignments" runat="server" ItemType="Pyramid.Models.EmployeeClassroom">
                                            <HeaderTemplate>
                                                <table id="tblEmployeeClassroomAssignments" class="table table-striped table-bordered table-hover">
                                                    <thead>
                                                        <tr>
                                                            <th data-priority="1">Employee</th>
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
                                                            <td><%# Item.ProgramEmployee.FirstName + " " + Item.ProgramEmployee.LastName %></td>
                                                            <td><%# Item.CodeJobType.Description %></td>
                                                            <td><%# Item.AssignDate.ToString("MM/dd/yyyy") %></td>
                                                            <td class="leave-date"><%# (Item.LeaveDate.HasValue ? Item.LeaveDate.Value.ToString("MM/dd/yyyy") : "") %></td>
                                                            <td><%# (Item.CodeEmployeeLeaveReason != null ? Item.CodeEmployeeLeaveReason.Description +  (!String.IsNullOrWhiteSpace(Item.LeaveReasonSpecify) ? " (" + Item.LeaveReasonSpecify + ")" :"") : "") %></td>
                                                            <td class="text-center">
                                                                <div class="btn-group">
                                                                    <button id="btnActions" type="button" class="btn btn-secondary dropdown-toggle hide-on-view hidden" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                                        Actions
                                                                    </button>
                                                                    <div class="dropdown-menu dropdown-menu-right" aria-labelledby="btnActions">
                                                                        <asp:LinkButton ID="lbEditEmployeeClassroomAssignment" runat="server" CssClass="dropdown-item" OnClick="lbEditEmployeeClassroomAssignment_Click"><i class="fas fa-edit"></i> Edit</asp:LinkButton>
                                                                        <button class="dropdown-item delete-gridview" data-pk='<%# Item.EmployeeClassroomPK %>' data-hf="hfDeleteEmployeeClassroomAssignmentPK" data-target="#divDeleteEmployeeClassroomAssignmentModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                                    </div>
                                                                </div>
                                                                <asp:HiddenField ID="hfClassroomAssignmentPK" runat="server" Value='<%# Item.EmployeeClassroomPK %>' />
                                                                <asp:HiddenField ID="hfEmployeeFK" runat="server" Value='<%# Item.EmployeeFK %>' />
                                                                <asp:HiddenField ID="hfHireDate" runat="server" Value='<%# Item.ProgramEmployee.HireDate %>' />
                                                                <asp:HiddenField ID="hfTermDate" runat="server" Value='<%# Item.ProgramEmployee.TermDate %>' />
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
                                        <div id="divAddEditEmployeeClassroomAssignment" runat="server" class="card mt-2" visible="false">
                                            <div class="card-header">
                                                <asp:Label ID="lblAddEditEmployeeClassroomAssignment" runat="server" Text=""></asp:Label>
                                            </div>
                                            <div class="card-body">
                                                <div class="row">
                                                    <div class="col-lg-4">
                                                        <div class="form-group">
                                                            <dx:BootstrapComboBox ID="ddEmployee" runat="server" Caption="Employee" NullText="--Select--"
                                                                TextField="Name" ValueField="ProgramEmployeePK" ValueType="System.Int32" 
                                                                IncrementalFilteringMode="Contains" AllowMouseWheel="false">
                                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                                <ValidationSettings ValidationGroup="vgEmployeeClassroomAssignment" ErrorDisplayMode="ImageWithText">
                                                                    <RequiredField IsRequired="true" ErrorText="Employee is required!" />
                                                                </ValidationSettings>
                                                            </dx:BootstrapComboBox>
                                                        </div>
                                                    </div>
                                                    <div class="col-lg-4">
                                                        <div class="form-group">
                                                            <dx:BootstrapDateEdit ID="deEmployeeAssignDate" runat="server" Caption="Assign Date" 
                                                                EditFormat="Date" EditFormatString="MM/dd/yyyy"
                                                                UseMaskBehavior="true" ClientInstanceName="deEmployeeAssignDate" OnValidation="deEmployeeAssignDate_Validation" 
                                                                AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900">
                                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                                <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                                                <CalendarProperties ShowClearButton="true"></CalendarProperties>
                                                                <ValidationSettings ValidationGroup="vgEmployeeClassroomAssignment" ErrorDisplayMode="ImageWithText">
                                                                    <RequiredField IsRequired="true" ErrorText="Assign Date is required!" />
                                                                </ValidationSettings>
                                                            </dx:BootstrapDateEdit>
                                                        </div>
                                                    </div>
                                                    <div class="col-lg-4">
                                                        <div class="form-group">
                                                            <dx:BootstrapComboBox ID="ddClassroomJobType" runat="server" Caption="Classroom Job" NullText="--Select--" ValueType="System.Int32"
                                                                TextField="Description" ValueField="CodeJobTypePK" 
                                                                IncrementalFilteringMode="Contains" AllowMouseWheel="false"
                                                                ClientInstanceName="ddClassroomJobType" OnValidation="ddClassroomJobType_Validation">
                                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                                <ValidationSettings ValidationGroup="vgEmployeeClassroomAssignment" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                                    <RequiredField IsRequired="true" ErrorText="Classroom Job is required!" />
                                                                </ValidationSettings>
                                                            </dx:BootstrapComboBox>
                                                        </div>
                                                    </div>
                                                </div>
                                                <div class="row">
                                                    <div class="col-lg-4">
                                                        <div class="form-group">
                                                            <dx:BootstrapDateEdit ID="deEmployeeLeaveDate" runat="server" Caption="Leave Date" EditFormat="Date"
                                                                EditFormatString="MM/dd/yyyy" UseMaskBehavior="true"
                                                                ClientInstanceName="deEmployeeLeaveDate" OnValidation="deEmployeeLeaveDate_Validation" 
                                                                AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900">
                                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                                <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                                                <CalendarProperties ShowClearButton="true"></CalendarProperties>
                                                                <ValidationSettings ValidationGroup="vgEmployeeClassroomAssignment" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                                    <RequiredField IsRequired="false" ErrorText="Leave Date is required!" />
                                                                </ValidationSettings>
                                                            </dx:BootstrapDateEdit>
                                                        </div>
                                                    </div>
                                                    <div class="col-lg-4">
                                                        <dx:BootstrapComboBox ID="ddEmployeeLeaveReason" runat="server" Caption="Leave Reason" NullText="--Select--"
                                                            TextField="Description" ValueField="CodeEmployeeLeaveReasonPK" ValueType="System.Int32"
                                                            IncrementalFilteringMode="Contains"  AllowMouseWheel="false"
                                                            OnValidation="ddEmployeeLeaveReason_Validation" ClientInstanceName="ddEmployeeLeaveReason" 
                                                            AllowNull="true" ClearButton-DisplayMode="Always">
                                                            <ClientSideEvents Validation="validateEmployeeLeaveReason" Init="showHideEmployeeLeaveReasonSpecify" SelectedIndexChanged="showHideEmployeeLeaveReasonSpecify" />
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <ValidationSettings ValidationGroup="vgEmployeeClassroomAssignment" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="false" ErrorText="Leave Reason is required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapComboBox>
                                                        <div id="divEmployeeLeaveReasonSpecify" style="display: none">
                                                            <dx:BootstrapTextBox ID="txtEmployeeLeaveReasonSpecify" runat="server" Caption="Specify Leave Reason" MaxLength="500"
                                                                OnValidation="txtEmployeeLeaveReasonSpecify_Validation" ClientInstanceName="txtEmployeeLeaveReasonSpecify">
                                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                                <ClientSideEvents Validation="validateEmployeeLeaveReasonSpecify" />
                                                                <ValidationSettings ValidationGroup="vgEmployeeClassroomAssignment" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                                </ValidationSettings>
                                                            </dx:BootstrapTextBox>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="card-footer">
                                                <div class="center-content">
                                                    <asp:HiddenField ID="hfAddEditEmployeeClassroomAssignmentPK" runat="server" Value="0" />
                                                    <asp:HiddenField ID="hfAddEditEmployeeClassroomEmployeePK" runat="server" Value="0" />
                                                    <asp:HiddenField ID="hfAddEditEmployeeClassroomHireDate" runat="server" Value="" />
                                                    <asp:HiddenField ID="hfAddEditEmployeeClassroomTermDate" runat="server" Value="" />
                                                    <uc:Submit ID="submitEmployeeClassroomAssignment" runat="server" ValidationGroup="vgEmployeeClassroomAssignment" OnSubmitClick="submitEmployeeClassroomAssignment_Click" OnCancelClick="submitEmployeeClassroomAssignment_CancelClick" OnValidationFailed="submitEmployeeClassroomAssignment_ValidationFailed" />
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="lbDeleteEmployeeClassroomAssignment" EventName="Click" />
                    </Triggers>
                </asp:UpdatePanel>
            </div>
        </div>
    </div>
    <div class="page-footer">
        <uc:Submit ID="submitClassroom" runat="server" ValidationGroup="vgClassroom" OnSubmitClick="submitClassroom_Click" OnCancelClick="submitClassroom_CancelClick" OnValidationFailed="submitClassroom_ValidationFailed"></uc:Submit>
    </div>
    <div class="modal" id="divDeleteChildClassroomAssignmentModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete Child Classroom Assignment</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this child's classroom assignment?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteChildClassroomAssignment" runat="server" CssClass="btn btn-loader btn-loader btn-danger modal-delete" data-target="#divDeleteChildClassroomAssignmentModal" OnClick="lbDeleteChildClassroomAssignment_Click"><i class="fas fa-check"></i>&nbsp;Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
    <div class="modal" id="divDeleteEmployeeClassroomAssignmentModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete Employee Classroom Assignment</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this employee's classroom assignment?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteEmployeeClassroomAssignment" runat="server" CssClass="btn btn-loader btn-loader btn-danger modal-delete" data-target="#divDeleteEmployeeClassroomAssignmentModal" OnClick="lbDeleteEmployeeClassroomAssignment_Click"><i class="fas fa-check"></i>&nbsp;Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
