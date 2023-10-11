﻿<%@ Page Title="Child" Language="C#" MasterPageFile="~/MasterPages/Dashboard.master" AutoEventWireup="true" CodeBehind="Child.aspx.cs" Inherits="Pyramid.Pages.Child" %>

<%@ Register Assembly="DevExpress.Web.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Data.Linq" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.Bootstrap.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>
<%@ Register TagPrefix="uc" TagName="Child" Src="~/User_Controls/Child.ascx" %>
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
            $('#lnkChildrenDashboard').addClass('active');

            //Allow date format for DataTables sorting
            $.fn.dataTable.moment('MM/DD/YYYY');

            //Show/hide the view only fields
            setViewOnlyVisibility();

            //Initialize the datatables
            if (!$.fn.dataTable.isDataTable('#tblChildNotes')) {
                $('#tblChildNotes').DataTable({
                    responsive: {
                        details: {
                            type: 'column'
                        }
                    },
                    columnDefs: [
                        { orderable: false, targets: [3] },
                        { className: 'control', orderable: false, targets: 0 }
                    ],
                    order: [[1, 'desc']],
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

            if (!$.fn.dataTable.isDataTable('#tblChildStatuses')) {
                $('#tblChildStatuses').DataTable({
                    responsive: {
                        details: {
                            type: 'column'
                        }
                    },
                    columnDefs: [
                        { orderable: false, targets: [3] },
                        { className: 'control', orderable: false, targets: 0 }
                    ],
                    order: [[1, 'desc']],
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

            if (!$.fn.dataTable.isDataTable('#tblClassroomAssignments')) {
                $('#tblClassroomAssignments').DataTable({
                    responsive: {
                        details: {
                            type: 'column'
                        }
                    },
                    columnDefs: [
                        { orderable: false, targets: [5] },
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
            $('.dataTables_filter input').removeClass('form-control-sm');
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

        //This function controls whether or not the specify field for the leave reason is shown
        //based on the value in the ddLeaveReason Combo Box
        function showHideLeaveReasonSpecify() {
            //Get the leave reason
            var leaveReason = ddLeaveReason.GetText();

            //If the leave reason is other, show the specify div
            if (leaveReason.toLowerCase() == 'other') {
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

            if ((reasonSpecify == null || reasonSpecify == ' ')
                    && leaveReason.toLowerCase() == 'other') {
                e.isValid = false;
                e.errorText = "Specify Leave Reason is required when the 'Other' leave reason is selected!";
            }
            else {
                e.isValid = true;
            }
        }

        //This function allows multiple date edits to check against discharge and enrollment date
        function checkBetweenEnrollmentAndDischargeAndRequired(s, e) {
            //Get the date to check
            var dateToCheck = e.value;

            //Get the dischargeination date and enrollment date
            var dischargeDate = deDischargeDate.GetDate();
            var enrollmentDate = deEnrollmentDate.GetDate();

            //Perform validation
            if (dateToCheck == null) {
                e.isValid = false;
                e.errorText = "This date is required!";
            }
            else if (dischargeDate == null && (dateToCheck > new Date() || dateToCheck < enrollmentDate)) {
                e.isValid = false;
                e.errorText = "This date must between the enrollment date and now!";
            }
            else if (dischargeDate != null && (dateToCheck > dischargeDate || dateToCheck < enrollmentDate)) {
                e.isValid = false;
                e.errorText = "This date must between the enrollment date and discharge date!";
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
            <asp:AsyncPostBackTrigger ControlID="lbAddChildNote" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="repeatChildNotes" />
            <asp:AsyncPostBackTrigger ControlID="lbDeleteChildNote" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="submitNote" />
            <asp:AsyncPostBackTrigger ControlID="lbAddChildStatus" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="repeatChildStatus" />
            <asp:AsyncPostBackTrigger ControlID="lbDeleteChildStatus" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="submitStatus" />
            <asp:AsyncPostBackTrigger ControlID="lbAddClassroomAssignment" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="repeatClassroomAssignments" />
            <asp:AsyncPostBackTrigger ControlID="lbDeleteClassroomAssignment" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="submitClassroomAssignment" />
            <asp:AsyncPostBackTrigger ControlID="btnPrintPreview" EventName="Click" />
            <asp:PostBackTrigger ControlID="submitChild" />
        </Triggers>
    </asp:UpdatePanel>
    <asp:UpdatePanel ID="upBasicInfo" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <ContentTemplate>
            <asp:HiddenField ID="hfChildProgramPK" runat="server" Value="" />
            <div class="row">
                <div class="col-lg-12">
                    <div class="card bg-light">
                        <div class="card-header">
                            <div class="row">
                                <div class="col-md-8">
                                    Basic Information
                                </div>
                                <div class="col-md-4">
                                    <dx:BootstrapButton ID="btnPrintPreview" runat="server" Text="Save and Download/Print" OnClick="btnPrintPreview_Click"
                                        SettingsBootstrap-RenderOption="primary" ValidationGroup="vgChild" data-validation-group="vgChild">
                                        <CssClasses Icon="fas fa-print" Control="float-right btn-loader" />
                                    </dx:BootstrapButton>
                                </div>
                            </div>
                        </div>
                        <div class="card-body">
                            <div class="row">
                                <div class="col-md-6">
                                    <label>Program: </label>
                                    <asp:Label ID="lblProgram" runat="server" Text=""></asp:Label>
                                </div>
                            </div>
                            <uc:Child ID="childControl" runat="server"></uc:Child>
                        </div>
                    </div>
                </div>
            </div>
        </ContentTemplate>
        <Triggers>
            <asp:PostBackTrigger ControlID="submitChild" />
        </Triggers>
    </asp:UpdatePanel>
    <div id="divEditOnly" runat="server" visible="false">
        <div class="row">
            <div class="col-xl-6">
                <asp:HiddenField ID="hfDeleteChildNotePK" runat="server" Value="0" />
                <asp:UpdatePanel ID="upNotes" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                    <ContentTemplate>
                        <div class="card bg-light">
                            <div class="card-header">
                                Notes
                                <asp:LinkButton ID="lbAddChildNote" runat="server" CssClass="btn btn-loader btn-primary float-right hide-on-view hidden" OnClick="lbAddChildNote_Click"><i class="fas fa-plus"></i> Add New Note</asp:LinkButton>
                            </div>
                            <div class="card-body">
                                <div class="row">
                                    <div class="col-lg-12">
                                        <label>All Notes for this Child</label>
                                        <asp:Repeater ID="repeatChildNotes" runat="server" ItemType="Pyramid.Models.ChildNote">
                                            <HeaderTemplate>
                                                <table id="tblChildNotes" class="table table-striped table-bordered table-hover">
                                                    <thead>
                                                        <tr>
                                                            <th data-priority="1"></th>
                                                            <th data-priority="2">Date</th>
                                                            <th>Contents</th>
                                                            <th data-priority="3"></th>
                                                        </tr>
                                                    </thead>
                                                    <tbody>
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                        <tr>
                                                            <td></td>
                                                            <td><%# Item.NoteDate.ToString("MM/dd/yyyy") %></td>
                                                            <td><%# Item.Contents %></td>
                                                            <td class="text-center">
                                                                <div class="btn-group">
                                                                    <button type="button" class="btn btn-secondary dropdown-toggle hide-on-view hidden" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                                        Actions
                                                                    </button>
                                                                    <div class="dropdown-menu dropdown-menu-right">
                                                                        <asp:LinkButton ID="lbEditChildNote" runat="server" CssClass="dropdown-item" OnClick="lbEditChildNote_Click"><i class="fas fa-edit"></i> Edit</asp:LinkButton>
                                                                        <button class="dropdown-item delete-gridview" data-pk='<%# Item.ChildNotePK %>' data-hf="hfDeleteChildNotePK" data-target="#divDeleteChildNoteModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                                    </div>
                                                                </div>
                                                                <!-- Need to use labels so that values are maintained after postback (inputs get cleared because of an interaction with DataTables and the repeater) -->
                                                                <asp:Label ID="lblChildNotePK" runat="server" Visible="false" Text='<%# Item.ChildNotePK %>'></asp:Label>
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
                                        <div id="divAddEditNote" runat="server" class="card mt-2" visible="false">
                                            <div class="card-header">
                                                <asp:Label ID="lblAddEditNote" runat="server" Text=""></asp:Label>
                                            </div>
                                            <div class="card-body">
                                                <div class="row">
                                                    <div class="col-xl-12">
                                                        <div class="form-group">
                                                            <dx:BootstrapDateEdit ID="deNoteDate" runat="server" Caption="Date" EditFormat="Date" 
                                                                EditFormatString="MM/dd/yyyy" UseMaskBehavior="true" AllowMouseWheel="false" 
                                                                OnValidation="CheckBetweenEnrollmentAndDischargeAndRequired_Validation" 
                                                                PickerDisplayMode="Calendar" MinDate="01/01/1900">
                                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                                <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                                                <ClientSideEvents Validation="checkBetweenEnrollmentAndDischargeAndRequired" />
                                                                <CalendarProperties ShowClearButton="true"></CalendarProperties>
                                                                <ValidationSettings ValidationGroup="vgNote" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                                    <RequiredField IsRequired="true" ErrorText="Date is required!" />
                                                                </ValidationSettings>
                                                            </dx:BootstrapDateEdit>
                                                        </div>
                                                    </div>
                                                    <div class="col-xl-12">
                                                        <div class="form-group">
                                                            <dx:BootstrapMemo ID="txtNoteContents" runat="server" Caption="Contents" Rows="5">
                                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                                <ValidationSettings ValidationGroup="vgNote" ErrorDisplayMode="ImageWithText">
                                                                    <RequiredField IsRequired="true" ErrorText="Contents are required!" />
                                                                </ValidationSettings>
                                                            </dx:BootstrapMemo>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="card-footer">
                                                <div class="center-content">
                                                    <asp:HiddenField ID="hfAddEditNotePK" runat="server" Value="0" />
                                                    <uc:Submit ID="submitNote" runat="server" ValidationGroup="vgNote"
                                                        ControlCssClass="center-content"
                                                        OnSubmitClick="submitNote_Click" OnCancelClick="submitNote_CancelClick" 
                                                        OnValidationFailed="submitNote_ValidationFailed" />
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="lbDeleteChildNote" EventName="Click" />
                    </Triggers>
                </asp:UpdatePanel>
            </div>
            <div class="col-xl-6">
                <asp:HiddenField ID="hfDeleteChildStatusPK" runat="server" Value="0" />
                <asp:UpdatePanel ID="upChildStatus" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                    <ContentTemplate>
                        <div class="card bg-light">
                            <div class="card-header">
                                Status History
                                <asp:LinkButton ID="lbAddChildStatus" runat="server" CssClass="btn btn-loader btn-primary float-right hide-on-view hidden" OnClick="lbAddChildStatus_Click"><i class="fas fa-plus"></i> Add New Status Change</asp:LinkButton>
                            </div>
                            <div class="card-body">
                                <div class="row">
                                    <div class="col-lg-12">
                                        <label>All Statuses for this Child</label>
                                        <asp:Repeater ID="repeatChildStatus" runat="server" ItemType="Pyramid.Models.spGetChildStatusHistory_Result">
                                            <HeaderTemplate>
                                                <table id="tblChildStatuses" class="table table-striped table-bordered table-hover">
                                                    <thead>
                                                        <tr>
                                                            <th data-priority="1"></th>
                                                            <th data-priority="2">Date</th>
                                                            <th>Status</th>
                                                            <th data-priority="3"></th>
                                                        </tr>
                                                    </thead>
                                                    <tbody>
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                        <tr>
                                                            <td></td>
                                                            <td><%# Item.StatusDate.ToString("MM/dd/yyyy") %></td>
                                                            <td><%# Item.StatusDescription %></td>
                                                            <td class="text-center">
                                                                <div class="btn-group">
                                                                    <button runat="server" visible='<%# Item.ChildStatusPK != null %>' type="button" class="btn btn-secondary dropdown-toggle hide-on-view hidden" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                                        Actions
                                                                    </button>
                                                                    <div class="dropdown-menu dropdown-menu-right">
                                                                        <asp:LinkButton ID="lbEditChildStatus" runat="server" CssClass="dropdown-item" OnClick="lbEditChildStatus_Click"><i class="fas fa-edit"></i> Edit</asp:LinkButton>
                                                                        <button class="dropdown-item delete-gridview" data-pk='<%# Item.ChildStatusPK %>' data-hf="hfDeleteChildStatusPK" data-target="#divDeleteChildStatusModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                                    </div>
                                                                </div>
                                                                <!-- Need to use labels so that values are maintained after postback (inputs get cleared because of an interaction with DataTables and the repeater) -->
                                                                <asp:Label ID="lblChildStatusPK" runat="server" Visible="false" Text='<%# Item.ChildStatusPK %>'></asp:Label>
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
                                        <div id="divAddEditStatus" runat="server" class="card mt-2" visible="false">
                                            <div class="card-header">
                                                <asp:Label ID="lblAddEditStatus" runat="server" Text=""></asp:Label>
                                            </div>
                                            <div class="card-body">
                                                <div class="row">
                                                    <div class="col-xl-12">
                                                        <div class="form-group">
                                                            <dx:BootstrapDateEdit ID="deStatusDate" runat="server" Caption="Status Date" EditFormat="Date" 
                                                                EditFormatString="MM/dd/yyyy" UseMaskBehavior="true" AllowMouseWheel="false"
                                                                OnValidation="CheckBetweenEnrollmentAndDischargeAndRequired_Validation" 
                                                                PickerDisplayMode="Calendar" MinDate="01/01/1900">
                                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                                <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                                                <ClientSideEvents Validation="checkBetweenEnrollmentAndDischargeAndRequired" />
                                                                <CalendarProperties ShowClearButton="true"></CalendarProperties>
                                                                <ValidationSettings ValidationGroup="vgStatus" ErrorDisplayMode="ImageWithText">
                                                                    <RequiredField IsRequired="true" ErrorText="Status Date is required!" />
                                                                </ValidationSettings>
                                                            </dx:BootstrapDateEdit>
                                                        </div>
                                                    </div>
                                                    <div class="col-xl-12">
                                                        <div class="form-group">
                                                            <dx:BootstrapComboBox ID="ddStatus" runat="server" Caption="Status" NullText="--Select--"
                                                                TextField="Description" ValueField="CodeChildStatusPK" ValueType="System.Int32" 
                                                                IncrementalFilteringMode="Contains" AllowMouseWheel="false">
                                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                                <ValidationSettings ValidationGroup="vgStatus" ErrorDisplayMode="ImageWithText">
                                                                    <RequiredField IsRequired="true" ErrorText="Status is required!" />
                                                                </ValidationSettings>
                                                            </dx:BootstrapComboBox>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="card-footer">
                                                <div class="center-content">
                                                    <asp:HiddenField ID="hfAddEditStatusPK" runat="server" Value="0" />
                                                    <uc:Submit ID="submitStatus" runat="server" ValidationGroup="vgStatus"
                                                        ControlCssClass="center-content"
                                                        OnSubmitClick="submitStatus_Click" OnCancelClick="submitStatus_CancelClick" 
                                                        OnValidationFailed="submitStatus_ValidationFailed" />
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="lbDeleteChildStatus" EventName="Click" />
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
                                        <label>All Classroom Assignments for this Child</label>
                                        <asp:Repeater ID="repeatClassroomAssignments" runat="server" ItemType="Pyramid.Models.ChildClassroom">
                                            <HeaderTemplate>
                                                <table id="tblClassroomAssignments" class="table table-striped table-bordered table-hover">
                                                    <thead>
                                                        <tr>
                                                            <th data-priority="1"></th>
                                                            <th data-priority="2">Classroom</th>
                                                            <th data-priority="4">Assign Date</th>
                                                            <th class="min-tablet-l">Leave Date</th>
                                                            <th class="min-tablet-l">Leave Reason</th>
                                                            <th data-priority="3"></th>
                                                        </tr>
                                                    </thead>
                                                    <tbody>
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                        <tr>
                                                            <td></td>
                                                            <td><%# "(" + Item.Classroom.ProgramSpecificID + ") " + Item.Classroom.Name %></td>
                                                            <td><%# Item.AssignDate.ToString("MM/dd/yyyy") %></td>
                                                            <td class="leave-date"><%# (Item.LeaveDate.HasValue ? Item.LeaveDate.Value.ToString("MM/dd/yyyy") : "") %></td>
                                                            <td><%# (Item.CodeChildLeaveReason != null ? Item.CodeChildLeaveReason.Description +  (!String.IsNullOrWhiteSpace(Item.LeaveReasonSpecify) ? " (" + Item.LeaveReasonSpecify + ")" :"") : "") %></td>
                                                            <td class="text-center">
                                                                <div class="btn-group">
                                                                    <button type="button" class="btn btn-secondary dropdown-toggle hide-on-view hidden" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                                        Actions
                                                                    </button>
                                                                    <div class="dropdown-menu dropdown-menu-right">
                                                                        <asp:LinkButton ID="lbEditClassroomAssignment" runat="server" CssClass="dropdown-item" OnClick="lbEditClassroomAssignment_Click"><i class="fas fa-edit"></i> Edit</asp:LinkButton>
                                                                        <button class="dropdown-item delete-gridview" data-pk='<%# Item.ChildClassroomPK %>' data-hf="hfDeleteClassroomAssignmentPK" data-target="#divDeleteClassroomAssignmentModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                                    </div>
                                                                </div>
                                                                <!-- Need to use labels so that values are maintained after postback (inputs get cleared because of an interaction with DataTables and the repeater) -->
                                                                <asp:Label ID="lblClassroomAssignmentPK" runat="server" Visible="false" Text='<%# Item.ChildClassroomPK %>'></asp:Label>
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
                                                    <div class="col-lg-6">
                                                        <div class="form-group">
                                                            <dx:BootstrapDateEdit ID="deAssignDate" runat="server" Caption="Assign Date" 
                                                                EditFormat="Date" EditFormatString="MM/dd/yyyy"
                                                                UseMaskBehavior="true" ClientInstanceName="deAssignDate" 
                                                                OnValidation="deAssignDate_Validation" AllowMouseWheel="false" 
                                                                PickerDisplayMode="Calendar" MinDate="01/01/1900">
                                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                                <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                                                <CalendarProperties ShowClearButton="true"></CalendarProperties>
                                                                <ValidationSettings ValidationGroup="vgClassroomAssignment" ErrorDisplayMode="ImageWithText">
                                                                    <RequiredField IsRequired="true" ErrorText="Assign Date is required!" />
                                                                </ValidationSettings>
                                                            </dx:BootstrapDateEdit>
                                                        </div>
                                                    </div>
                                                    <div class="col-lg-6">
                                                        <div class="form-group">
                                                            <dx:BootstrapComboBox ID="ddClassroom" runat="server" Caption="Classroom" NullText="--Select--"
                                                                TextField="IdAndName" ValueField="ClassroomPK" ValueType="System.Int32" 
                                                                IncrementalFilteringMode="Contains" AllowMouseWheel="false">
                                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                                <ValidationSettings ValidationGroup="vgClassroomAssignment" ErrorDisplayMode="ImageWithText">
                                                                    <RequiredField IsRequired="true" ErrorText="Classroom is required!" />
                                                                </ValidationSettings>
                                                            </dx:BootstrapComboBox>
                                                        </div>
                                                    </div>
                                                </div>
                                                <div class="row">
                                                    <div class="col-lg-6">
                                                        <div class="form-group">
                                                            <dx:BootstrapDateEdit ID="deLeaveDate" runat="server" Caption="Leave Date" EditFormat="Date" AllowNull="true"
                                                                EditFormatString="MM/dd/yyyy" UseMaskBehavior="true"
                                                                ClientInstanceName="deLeaveDate" OnValidation="deLeaveDate_Validation" 
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
                                                    <div class="col-lg-6">
                                                        <dx:BootstrapComboBox ID="ddLeaveReason" runat="server" Caption="Leave Reason" NullText="--Select--" AllowNull="true"
                                                            TextField="Description" ValueField="CodeChildLeaveReasonPK" ValueType="System.Int32"
                                                            IncrementalFilteringMode="Contains"  AllowMouseWheel="false"
                                                            OnValidation="ddLeaveReason_Validation" ClientInstanceName="ddLeaveReason" 
                                                            ClearButton-DisplayMode="Always">
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
                                                    <uc:Submit ID="submitClassroomAssignment" runat="server" 
                                                        ValidationGroup="vgClassroomAssignment"
                                                        ControlCssClass="center-content"
                                                        OnSubmitClick="submitClassroomAssignment_Click" 
                                                        OnCancelClick="submitClassroomAssignment_CancelClick" 
                                                        OnValidationFailed="submitClassroomAssignment_ValidationFailed" />
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
        <uc:Submit ID="submitChild" runat="server" ValidationGroup="vgChild"
            ControlCssClass="center-content"
            OnSubmitClick="submitChild_Click" OnCancelClick="submitChild_CancelClick" 
            OnValidationFailed="submitChild_ValidationFailed"></uc:Submit>
    </div>
    <div class="modal" id="divDeleteChildNoteModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete Note</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this note?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteChildNote" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteChildNoteModal" OnClick="lbDeleteChildNote_Click"><i class="fas fa-check"></i> Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
    <div class="modal" id="divDeleteChildStatusModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete Status</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this status?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteChildStatus" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteChildStatusModal" OnClick="lbDeleteChildStatus_Click"><i class="fas fa-check"></i> Yes</asp:LinkButton>
                </div>
            </div>
        </div>
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
</asp:Content>
