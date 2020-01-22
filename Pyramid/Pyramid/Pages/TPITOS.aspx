<%@ Page Title="TPITOS Observation" Language="C#" MasterPageFile="~/MasterPages/Dashboard.master" AutoEventWireup="true" CodeBehind="TPITOS.aspx.cs" Inherits="Pyramid.Pages.TPITOS" %>

<%@ Register Assembly="DevExpress.Web.v19.1, Version=19.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Data.Linq" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.Bootstrap.v19.1, Version=19.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.v19.1, Version=19.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>
<%@ Register TagPrefix="uc" TagName="Messaging" Src="~/User_Controls/MessagingSystem.ascx" %>
<%@ Register TagPrefix="uc" TagName="Submit" Src="~/User_Controls/Submit.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style type="text/css">
        /* Make sure the red flag types are visible */
        [ID$="lstBxRedFlags"] ul {
            min-height: 580px;
        }
    </style>
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
            $('#lnkTPITOSDashboard').addClass('active');

            //Allow date format for DataTables sorting
            $.fn.dataTable.moment('MM/DD/YYYY');

            //Initialize the datatables
            if (!$.fn.dataTable.isDataTable('#tblParticipants')) {
                $('#tblParticipants').DataTable({
                    responsive: true,
                    columnDefs: [
                        { orderable: false, targets: [2] }
                    ],
                    order: [[0, 'asc']],
                    pageLength: 10,
                    dom: 'frtp',
                    language: {
                        searchPlaceholder: "Enter text to search...",
                        search: "",
                    }
                });
            }
            if (!$.fn.dataTable.isDataTable('#tblKeyPractices')) {
                $('#tblKeyPractices').DataTable({
                    responsive: {
                        details: {
                            type: 'column'
                        }
                    },
                    columnDefs: [
                        { orderable: false, targets: [2, 3, 4, 5] },
                        { className: 'control', orderable: false, targets: 0 }
                    ],
                    order: [[1, 'asc']],
                    pageLength: 100,
                    dom: 'frtp',
                    language: {
                        searchPlaceholder: "Enter text to search...",
                        search: "",
                    }
                });
            }
            if (!$.fn.dataTable.isDataTable('#tblRedFlags')) {
                $('#tblRedFlags').DataTable({
                    responsive: {
                        details: {
                            type: 'column'
                        }
                    },
                    columnDefs: [
                        { orderable: false, targets: [2, 3, 4, 5] },
                        { className: 'control', orderable: false, targets: 0 }
                    ],
                    order: [[1, 'asc']],
                    pageLength: 100,
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

        //This function validates the start time field
        function validateStartTime(s, e) {
            //Get the start and end times
            var startTime = teObservationStartTime.GetDate();
            var endTime = teObservationEndTime.GetDate();

            //Perform validation
            if (startTime == null) {
                e.isValid = false;
                e.errorText = "Start Time is required!";
            }
            else if (endTime != null && startTime >= endTime) {
                e.isValid = false;
                e.errorText = "Start Time must be before the End Time!";
            }
        }

        //This function validates the start time field
        function validateEndTime(s, e) {
            //Get the start and end times
            var startTime = teObservationStartTime.GetDate();
            var endTime = teObservationEndTime.GetDate();

            //Perform validation
            if (endTime == null) {
                e.isValid = false;
                e.errorText = "End Time is required!";
            }
            else if (startTime >= endTime) {
                e.isValid = false;
                e.errorText = "End Time must be after the Start Time!";
            }
        }

        //This function is used to calculate the item percentages
        function calculateItemPercentage(s, e) {
            //Get the row using JQuery and the control name
            var row = $('#' + s.name).closest('.item-row');

            //Get the percent label for later
            var percentLabel = row.find('.item-percentage');

            //Get the yes and no textbox IDs and then controls
            var yesTextboxID = row.find('.item-yes').attr('id');
            var noTextboxID = row.find('.item-no').attr('id');
            var yesTextbox = ASPxClientControl.Cast(yesTextboxID);
            var noTextbox = ASPxClientControl.Cast(noTextboxID);

            //Get the number in the yes textbox and the number in the no textbox
            var numYes = Number(yesTextbox.GetValue());
            var numNo = Number(noTextbox.GetValue());

            //Get the total
            var numTotal = numYes + numNo;

            //Fill the percent label
            if (numYes > 0 && numNo <= 0) {
                //There is no numNo, set the label to 100%
                percentLabel.text('100%');
            }
            else if (numYes > 0 && numNo > 0) {
                //There is a numNo, calculate the percentage and set the label
                var percentage = Math.round((numYes / numTotal) * 100);
                percentLabel.text(percentage + '%');
            }
            else {
                //Can't calculate, set the label to 0%
                percentLabel.text('0%');
            }
        }

        //This function is used to calculate the red flag percentages
        function calculateRedFlagPercentage(s, e) {
            //Get the row using JQuery and the control name
            var row = $('#' + s.name).closest('.red-flag-row');

            //Get the percent label for later
            var percentLabel = row.find('.red-flag-percentage');

            //Get the yes and possible textbox IDs and then controls
            var yesTextboxID = row.find('.red-flag-yes').attr('id');
            var possibleTextboxID = row.find('.red-flag-possible').attr('id');
            var yesTextbox = ASPxClientControl.Cast(yesTextboxID);
            var possibleTextbox = ASPxClientControl.Cast(possibleTextboxID);

            //Get the number in the yes textbox and the number in the possible textbox
            var numYes = Number(yesTextbox.GetValue());
            var numPossible = Number(possibleTextbox.GetValue());

            //Fill the percent label
            if (numYes > 0 && numPossible <= 0) {
                //There is no numPossible, set the label to 100%
                percentLabel.text('100%');
            }
            else if (numYes > 0 && numPossible > 0) {
                //There is a numPossible, calculate the percentage and set the label
                var percentage = Math.round((numYes / numPossible) * 100);
                percentLabel.text(percentage + '%');
            }
            else {
                //Can't calculate, set the label to 0%
                percentLabel.text('0%');
            }
        }

        //This function is used to validate the TPITOS items
        function validateTPITOSItemNumbers(s, e) {
            //Get the calling control
            var control = $('#' + s.name);

            //Get the row
            var row = control.closest('.item-row');

            //To hold the values
            var fieldName;
            var otherTextboxID;
            var otherTextbox;
            var otherFieldName;

            if (control.hasClass('item-yes')) {
                //This is the # Yes field
                //Set the field name and set the other textbox variables
                fieldName = '# Yes';
                otherTextboxID = row.find('.item-no').attr('id');
                otherTextbox = ASPxClientControl.Cast(otherTextboxID);
                otherFieldName = '# No';
            }
            else {
                //This is the # No field
                //Set the field name and set the other textbox variables
                fieldName = '# No';
                otherTextboxID = row.find('.item-yes').attr('id');
                otherTextbox = ASPxClientControl.Cast(otherTextboxID);
                otherFieldName = '# Yes';
            }

            //Get the other textbox value
            otherTextboxValue = otherTextbox.GetValue();

            //Perform validation
            if (!e.value && otherTextboxValue) {
                //This field is invalid
                e.isValid = false;
                e.errorText = 'This is required when the ' + otherFieldName + ' field has a value!';
            }
            else if (e.value && !otherTextboxValue) {
                //The other textbox is invalid
                otherTextbox.SetIsValid(false);
                otherTextbox.SetErrorText('This is required when the ' + fieldName + ' field has a value!')
            }
        }

        //This function is used to validate the TPITOS items
        function validateTPITOSRedFlagNumbers(s, e) {
            //Get the calling control
            var control = $('#' + s.name);

            //Get the row
            var row = control.closest('.red-flag-row');

            //To hold the values
            var fieldName;
            var otherTextboxID;
            var otherTextbox;
            var otherFieldName;
            var otherTextboxValue;

            //Check to see which field this is
            if (control.hasClass('red-flag-yes')) {
                //This is the # Yes field
                //Set the field name and set the other textbox variables
                fieldName = '# Yes';
                otherTextboxID = row.find('.red-flag-possible').attr('id');
                otherTextbox = ASPxClientControl.Cast(otherTextboxID);
                otherFieldName = '# Possible';
            }
            else {
                //This is the # Possible field
                //Set the field name and set the other textbox variables
                fieldName = '# Possible';
                otherTextboxID = row.find('.red-flag-yes').attr('id');
                otherTextbox = ASPxClientControl.Cast(otherTextboxID);
                otherFieldName = '# Yes';
            }

            //Get the other textbox value
            otherTextboxValue = otherTextbox.GetValue();

            //Perform validation
            if (!e.value && otherTextboxValue) {
                //This field is invalid
                e.isValid = false;
                e.errorText = 'This is required when the ' + otherFieldName + ' field has a value!';
            }
            else if (e.value && !otherTextboxValue) {
                //The other field is invalid
                otherTextbox.SetIsValid(false);
                otherTextbox.SetErrorText('This is required when the ' + fieldName + ' field has a value!')
            }
            else if(fieldName == '# Possible' && e.value && otherTextboxValue) {
                //Make sure that the number possible is at least as high as the number yes
                if (Number(e.value) < Number(otherTextboxValue)) {
                    e.isValid = false;
                    e.errorText = 'This must be equal to or more than the ' + otherFieldName + ' field!';
                }
            }
        }
    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Label ID="lblPageTitle" Text="TPITOS" CssClass="h2" runat="server"></asp:Label>
    <hr />
    <asp:HiddenField ID="hfViewOnly" runat="server" Value="False" />
    <asp:UpdatePanel ID="upMessaging" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <uc:Messaging ID="msgSys" runat="server" />
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="deObservationDate" />
            <asp:AsyncPostBackTrigger ControlID="lbDeleteTPITOSParticipant" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="submitParticipant" />
            <asp:AsyncPostBackTrigger ControlID="submitTPITOS" />
        </Triggers>
    </asp:UpdatePanel>
    <asp:UpdatePanel ID="upBasicInfo" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <div class="row">
                <div class="col-lg-12">
                    <div class="card bg-light">
                        <div class="card-header">
                            Basic Information
                        </div>
                        <div class="card-body">
                            <div class="row">
                                <div class="col-md-6">
                                    <label>Program: </label>
                                    <asp:Label ID="lblProgram" runat="server" Text=""></asp:Label>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-4">
                                    <dx:BootstrapDateEdit ID="deObservationDate" runat="server" Caption="Observation Date" EditFormat="Date"
                                        EditFormatString="MM/dd/yyyy" UseMaskBehavior="true" NullText="--Select--"
                                        OnValueChanged="deObservationDate_ValueChanged" AutoPostBack="true"
                                        OnValidation="deObservationDate_Validation" 
                                        AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                        <ValidationSettings ValidationGroup="vgTPITOS" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                            <RequiredField IsRequired="true" ErrorText="Observation Date is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapDateEdit>
                                </div>
                                <div class="col-lg-4">
                                    <dx:BootstrapTimeEdit ID="teObservationStartTime" runat="server" Caption="Start Time" EditFormat="Time"
                                        EditFormatString="hh:mm tt" NullText="" ClientInstanceName="teObservationStartTime"
                                        SpinButtons-ClientVisible="false" OnValidation="teObservationStartTime_Validation">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ClientSideEvents Validation="validateStartTime" />
                                        <ValidationSettings ValidationGroup="vgTPITOS" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                            <RequiredField IsRequired="true" ErrorText="Start Time is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapTimeEdit>
                                </div>
                                <div class="col-lg-4">
                                    <dx:BootstrapTimeEdit ID="teObservationEndTime" runat="server" Caption="End Time" EditFormat="Time"
                                        EditFormatString="hh:mm tt" NullText="" ClientInstanceName="teObservationEndTime"
                                        SpinButtons-ClientVisible="false" OnValidation="teObservationEndTime_Validation">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ClientSideEvents Validation="validateEndTime" />
                                        <ValidationSettings ValidationGroup="vgTPITOS" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                            <RequiredField IsRequired="true" ErrorText="End Time is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapTimeEdit>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-4">
                                    <dx:BootstrapComboBox ID="ddClassroom" runat="server" Caption="Classroom" NullText="--Select--"
                                        TextField="IdAndName" ValueField="ClassroomPK" ValueType="System.Int32"
                                        IncrementalFilteringMode="Contains" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgTPITOS" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Classroom is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </div>
                                <div class="col-lg-4">
                                    <dx:BootstrapComboBox ID="ddObserver" runat="server" Caption="Observer" NullText="--Select--"
                                        TextField="ObserverName" ValueField="ProgramEmployeePK" ValueType="System.Int32"
                                        IncrementalFilteringMode="Contains" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgTPITOS" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Observer is required!  If this is not enabled, there are no active observers in the database as of the observation date." />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </div>
                                <div class="col-lg-4">
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-4">
                                    <dx:BootstrapTextBox ID="txtAdultsBegin" runat="server" Caption="Number of adults present at beginning">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <MaskSettings Mask="099" PromptChar=" " ErrorText="Must be a valid number!" />
                                        <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgTPITOS">
                                            <RequiredField IsRequired="true" ErrorText="Required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapTextBox>
                                </div>
                                <div class="col-lg-4">
                                    <dx:BootstrapTextBox ID="txtAdultsEnd" runat="server" Caption="Number of adults present at end">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <MaskSettings Mask="099" PromptChar=" " ErrorText="Must be a valid number!" />
                                        <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgTPITOS">
                                            <RequiredField IsRequired="true" ErrorText="Required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapTextBox>
                                </div>
                                <div class="col-lg-4">
                                    <dx:BootstrapTextBox ID="txtAdultsEntered" runat="server" Caption="Number of adults who entered during observation">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <MaskSettings Mask="099" PromptChar=" " ErrorText="Must be a valid number!" />
                                        <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgTPITOS">
                                            <RequiredField IsRequired="true" ErrorText="Required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapTextBox>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-4">
                                    <dx:BootstrapTextBox ID="txtChildrenBegin" runat="server" Caption="Number of children present at beginning">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <MaskSettings Mask="099" PromptChar=" " ErrorText="Must be a valid number!" />
                                        <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgTPITOS">
                                            <RequiredField IsRequired="true" ErrorText="Required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapTextBox>
                                </div>
                                <div class="col-lg-4">
                                    <dx:BootstrapTextBox ID="txtChildrenEnd" runat="server" Caption="Number of children present at end">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <MaskSettings Mask="099" PromptChar=" " ErrorText="Must be a valid number!" />
                                        <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgTPITOS">
                                            <RequiredField IsRequired="true" ErrorText="Required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapTextBox>
                                </div>
                                <div class="col-lg-4">
                                    <dx:BootstrapMemo ID="txtNotes" runat="server" Caption="Notes" Rows="5">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgTPITOS" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="false" />
                                        </ValidationSettings>
                                    </dx:BootstrapMemo>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-12">
                                    <div id="divAddOnlyMessage" runat="server" visible="false" class="alert alert-primary">
                                        You must save the basic information before adding TPITOS participants, Key Practices, and Red Flags.
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="submitTPITOS" />
        </Triggers>
    </asp:UpdatePanel>
    <div id="divEditOnly" runat="server" visible="false">
        <div class="row">
            <div class="col-lg-12">
                <asp:HiddenField ID="hfDeleteTPITOSParticipantPK" runat="server" Value="0" />
                <asp:UpdatePanel ID="upParticipants" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                    <ContentTemplate>
                        <div class="card bg-light">
                            <div class="card-header">
                                TPITOS Participants
                                <asp:LinkButton ID="lbAddTPITOSParticipant" runat="server" CssClass="btn btn-loader btn-primary float-right hide-on-view hidden" OnClick="lbAddTPITOSParticipant_Click"><i class="fas fa-plus"></i>&nbsp;Add New TPITOS Participant</asp:LinkButton>
                            </div>
                            <div class="card-body">
                                <div class="row mt-2">
                                    <div class="col-lg-12">
                                        <label>All TPITOS Participants</label>
                                        <asp:Repeater ID="repeatParticipants" runat="server" ItemType="Pyramid.Models.TPITOSParticipant">
                                            <HeaderTemplate>
                                                <table id="tblParticipants" class="table table-striped table-bordered table-hover">
                                                    <thead>
                                                        <tr>
                                                            <th data-priority="1">Employee</th>
                                                            <th>Observation Role</th>
                                                            <th data-priority="2"></th>
                                                        </tr>
                                                    </thead>
                                                    <tbody>
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <tr>
                                                    <td><%# Item.ProgramEmployee.FirstName + " " + Item.ProgramEmployee.LastName %></td>
                                                    <td><%# Item.CodeParticipantType.Description %></td>
                                                    <td class="text-center">
                                                        <div class="btn-group">
                                                            <button type="button" class="btn btn-secondary dropdown-toggle hide-on-view hidden" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                                Actions
                                                            </button>
                                                            <div class="dropdown-menu dropdown-menu-right">
                                                                <asp:LinkButton ID="lbEditTPITOSParticipant" runat="server" CssClass="dropdown-item" OnClick="lbEditTPITOSParticipant_Click"><i class="fas fa-edit"></i> Edit</asp:LinkButton>
                                                                <button class="dropdown-item delete-gridview" data-pk='<%# Item.TPITOSParticipantPK %>' data-hf="hfDeleteTPITOSParticipantPK" data-target="#divDeleteTPITOSParticipantModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                            </div>
                                                        </div>
                                                        <asp:HiddenField ID="hfTPITOSParticipantPK" runat="server" Value='<%# Item.TPITOSParticipantPK %>' />
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
                                        <div id="divAddEditTPITOSParticipant" runat="server" class="card mt-2" visible="false">
                                            <div class="card-header">
                                                <asp:Label ID="lblAddEditParticipant" runat="server" Text=""></asp:Label>
                                            </div>
                                            <div class="card-body">
                                                <div class="row">
                                                    <div class="col-lg-6">
                                                        <dx:BootstrapComboBox ID="ddParticipant" runat="server" Caption="TPITOS Participant" NullText="--Select--"
                                                            TextField="ParticipantName" ValueField="ProgramEmployeePK" ValueType="System.Int32"
                                                            IncrementalFilteringMode="Contains" AllowMouseWheel="false"
                                                            OnValidation="ddParticipant_Validation">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <ValidationSettings ValidationGroup="vgParticipant" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="true" ErrorText="TPITOS Participant is required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapComboBox>
                                                    </div>
                                                    <div class="col-lg-6">
                                                        <dx:BootstrapComboBox ID="ddParticipantRole" runat="server" Caption="TPITOS Participant Role" NullText="--Select--"
                                                            TextField="Description" ValueField="CodeParticipantTypePK" ValueType="System.Int32"
                                                            IncrementalFilteringMode="Contains" AllowMouseWheel="false">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <ValidationSettings ValidationGroup="vgParticipant" ErrorDisplayMode="ImageWithText">
                                                                <RequiredField IsRequired="true" ErrorText="TPITOS Participant Role is required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapComboBox>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="card-footer">
                                                <div class="center-content">
                                                    <asp:HiddenField ID="hfAddEditParticipantPK" runat="server" Value="0" />
                                                    <uc:Submit ID="submitParticipant" runat="server" ValidationGroup="vgParticipant" OnSubmitClick="submitParticipant_Click" OnCancelClick="submitParticipant_CancelClick" OnValidationFailed="submitParticipant_ValidationFailed" />
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="deObservationDate" />
                        <asp:AsyncPostBackTrigger ControlID="lbDeleteTPITOSParticipant" EventName="Click" />
                    </Triggers>
                </asp:UpdatePanel>
            </div>
        </div>
        <asp:UpdatePanel ID="upSubscales" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
            <ContentTemplate>
                <div class="row">
                    <div class="col-lg-12">
                        <div class="card bg-light">
                            <div class="card-header">
                                Subscale 1. Key Practices
                            </div>
                            <div class="card-body">
                                <div class="row mt-2">
                                    <div class="col-md-12">
                                        <label>Key Practice Counts</label>
                                        <table id="tblKeyPractices" class="table table-striped table-bordered table-hover">
                                            <thead>
                                                <tr>
                                                    <th data-priority="1"></th>
                                                    <th data-priority="3">TPITOS Item #</th>
                                                    <th>TPITOS Item Description</th>
                                                    <th data-priority="2"># Yes</th>
                                                    <th data-priority="2"># No</th>
                                                    <th data-priority="4">% Yes</th>
                                                </tr>
                                            </thead>
                                            <tbody>
                                                <tr class="item-row">
                                                    <td></td>
                                                    <td>1</td>
                                                    <td>Provides opportunities for communication and building relationships</td>
                                                    <td>
                                                        <dx:BootstrapTextBox ID="txtItem1NumYes" runat="server" NullText="N/O" OnValidation="TPITOSItemNumbers_Validation">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <CssClasses Control="item-yes" />
                                                            <ClientSideEvents ValueChanged="calculateItemPercentage" Validation="validateTPITOSItemNumbers" Init="calculateItemPercentage" />
                                                            <MaskSettings Mask="999" PromptChar=" " ErrorText="Must be a valid number!" />
                                                            <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgTPITOS" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="false" ErrorText="Required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapTextBox>
                                                    </td>
                                                    <td>
                                                        <dx:BootstrapTextBox ID="txtItem1NumNo" runat="server" NullText="N/O" OnValidation="TPITOSItemNumbers_Validation">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <CssClasses Control="item-no" />
                                                            <ClientSideEvents ValueChanged="calculateItemPercentage" Validation="validateTPITOSItemNumbers" />
                                                            <MaskSettings Mask="999" PromptChar=" " ErrorText="Must be a valid number!" />
                                                            <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgTPITOS" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="false" ErrorText="Required" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapTextBox>
                                                    </td>
                                                    <td>
                                                        <label class="item-percentage"></label>
                                                    </td>
                                                </tr>
                                                <tr class="item-row">
                                                    <td></td>
                                                    <td>2</td>
                                                    <td>Demonstrates warmth and responsivity to individual children</td>
                                                    <td>
                                                        <dx:BootstrapTextBox ID="txtItem2NumYes" runat="server" NullText="N/O" OnValidation="TPITOSItemNumbers_Validation">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <CssClasses Control="item-yes" />
                                                            <ClientSideEvents ValueChanged="calculateItemPercentage" Validation="validateTPITOSItemNumbers" Init="calculateItemPercentage" />
                                                            <MaskSettings Mask="999" PromptChar=" " ErrorText="Must be a valid number!" />
                                                            <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgTPITOS" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="false" ErrorText="Required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapTextBox>
                                                    </td>
                                                    <td>
                                                        <dx:BootstrapTextBox ID="txtItem2NumNo" runat="server" NullText="N/O" OnValidation="TPITOSItemNumbers_Validation">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <CssClasses Control="item-no" />
                                                            <ClientSideEvents ValueChanged="calculateItemPercentage" Validation="validateTPITOSItemNumbers" />
                                                            <MaskSettings Mask="999" PromptChar=" " ErrorText="Must be a valid number!" />
                                                            <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgTPITOS" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="false" ErrorText="Required" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapTextBox>
                                                    </td>
                                                    <td>
                                                        <label class="item-percentage"></label>
                                                    </td>
                                                </tr>
                                                <tr class="item-row">
                                                    <td></td>
                                                    <td>3</td>
                                                    <td>Promotes positive peer interactions</td>
                                                    <td>
                                                        <dx:BootstrapTextBox ID="txtItem3NumYes" runat="server" NullText="N/O" OnValidation="TPITOSItemNumbers_Validation">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <CssClasses Control="item-yes" />
                                                            <ClientSideEvents ValueChanged="calculateItemPercentage" Validation="validateTPITOSItemNumbers" Init="calculateItemPercentage" />
                                                            <MaskSettings Mask="999" PromptChar=" " ErrorText="Must be a valid number!" />
                                                            <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgTPITOS" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="false" ErrorText="Required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapTextBox>
                                                    </td>
                                                    <td>
                                                        <dx:BootstrapTextBox ID="txtItem3NumNo" runat="server" NullText="N/O" OnValidation="TPITOSItemNumbers_Validation">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <CssClasses Control="item-no" />
                                                            <ClientSideEvents ValueChanged="calculateItemPercentage" Validation="validateTPITOSItemNumbers" />
                                                            <MaskSettings Mask="999" PromptChar=" " ErrorText="Must be a valid number!" />
                                                            <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgTPITOS" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="false" ErrorText="Required" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapTextBox>
                                                    </td>
                                                    <td>
                                                        <label class="item-percentage"></label>
                                                    </td>
                                                </tr>
                                                <tr class="item-row">
                                                    <td></td>
                                                    <td>4</td>
                                                    <td>Promotes children's active engagement</td>
                                                    <td>
                                                        <dx:BootstrapTextBox ID="txtItem4NumYes" runat="server" NullText="N/O" OnValidation="TPITOSItemNumbers_Validation">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <CssClasses Control="item-yes" />
                                                            <ClientSideEvents ValueChanged="calculateItemPercentage" Validation="validateTPITOSItemNumbers" Init="calculateItemPercentage" />
                                                            <MaskSettings Mask="999" PromptChar=" " ErrorText="Must be a valid number!" />
                                                            <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgTPITOS" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="false" ErrorText="Required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapTextBox>
                                                    </td>
                                                    <td>
                                                        <dx:BootstrapTextBox ID="txtItem4NumNo" runat="server" NullText="N/O" OnValidation="TPITOSItemNumbers_Validation">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <CssClasses Control="item-no" />
                                                            <ClientSideEvents ValueChanged="calculateItemPercentage" Validation="validateTPITOSItemNumbers" />
                                                            <MaskSettings Mask="999" PromptChar=" " ErrorText="Must be a valid number!" />
                                                            <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgTPITOS" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="false" ErrorText="Required" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapTextBox>
                                                    </td>
                                                    <td>
                                                        <label class="item-percentage"></label>
                                                    </td>
                                                </tr>
                                                <tr class="item-row">
                                                    <td></td>
                                                    <td>5</td>
                                                    <td>Responsive to children's expression of emotions and teaches about feelings</td>
                                                    <td>
                                                        <dx:BootstrapTextBox ID="txtItem5NumYes" runat="server" NullText="N/O" OnValidation="TPITOSItemNumbers_Validation">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <CssClasses Control="item-yes" />
                                                            <ClientSideEvents ValueChanged="calculateItemPercentage" Validation="validateTPITOSItemNumbers" Init="calculateItemPercentage" />
                                                            <MaskSettings Mask="999" PromptChar=" " ErrorText="Must be a valid number!" />
                                                            <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgTPITOS" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="false" ErrorText="Required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapTextBox>
                                                    </td>
                                                    <td>
                                                        <dx:BootstrapTextBox ID="txtItem5NumNo" runat="server" NullText="N/O" OnValidation="TPITOSItemNumbers_Validation">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <CssClasses Control="item-no" />
                                                            <ClientSideEvents ValueChanged="calculateItemPercentage" Validation="validateTPITOSItemNumbers" />
                                                            <MaskSettings Mask="999" PromptChar=" " ErrorText="Must be a valid number!" />
                                                            <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgTPITOS" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="false" ErrorText="Required" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapTextBox>
                                                    </td>
                                                    <td>
                                                        <label class="item-percentage"></label>
                                                    </td>
                                                </tr>
                                                <tr class="item-row">
                                                    <td></td>
                                                    <td>6</td>
                                                    <td>Communicates and provides feedback about developmentally appropriate behavioral expectations</td>
                                                    <td>
                                                        <dx:BootstrapTextBox ID="txtItem6NumYes" runat="server" NullText="N/O" OnValidation="TPITOSItemNumbers_Validation">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <CssClasses Control="item-yes" />
                                                            <ClientSideEvents ValueChanged="calculateItemPercentage" Validation="validateTPITOSItemNumbers" Init="calculateItemPercentage" />
                                                            <MaskSettings Mask="999" PromptChar=" " ErrorText="Must be a valid number!" />
                                                            <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgTPITOS" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="false" ErrorText="Required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapTextBox>
                                                    </td>
                                                    <td>
                                                        <dx:BootstrapTextBox ID="txtItem6NumNo" runat="server" NullText="N/O" OnValidation="TPITOSItemNumbers_Validation">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <CssClasses Control="item-no" />
                                                            <ClientSideEvents ValueChanged="calculateItemPercentage" Validation="validateTPITOSItemNumbers" />
                                                            <MaskSettings Mask="999" PromptChar=" " ErrorText="Must be a valid number!" />
                                                            <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgTPITOS" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="false" ErrorText="Required" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapTextBox>
                                                    </td>
                                                    <td>
                                                        <label class="item-percentage"></label>
                                                    </td>
                                                </tr>
                                                <tr class="item-row">
                                                    <td></td>
                                                    <td>7</td>
                                                    <td>Responds to children in distress and manages challenging behaviors</td>
                                                    <td>
                                                        <dx:BootstrapTextBox ID="txtItem7NumYes" runat="server" NullText="N/O" OnValidation="TPITOSItemNumbers_Validation">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <CssClasses Control="item-yes" />
                                                            <ClientSideEvents ValueChanged="calculateItemPercentage" Validation="validateTPITOSItemNumbers" Init="calculateItemPercentage" />
                                                            <MaskSettings Mask="999" PromptChar=" " ErrorText="Must be a valid number!" />
                                                            <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgTPITOS" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="false" ErrorText="Required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapTextBox>
                                                    </td>
                                                    <td>
                                                        <dx:BootstrapTextBox ID="txtItem7NumNo" runat="server" NullText="N/O" OnValidation="TPITOSItemNumbers_Validation">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <CssClasses Control="item-no" />
                                                            <ClientSideEvents ValueChanged="calculateItemPercentage" Validation="validateTPITOSItemNumbers" />
                                                            <MaskSettings Mask="999" PromptChar=" " ErrorText="Must be a valid number!" />
                                                            <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgTPITOS" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="false" ErrorText="Required" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapTextBox>
                                                    </td>
                                                    <td>
                                                        <label class="item-percentage"></label>
                                                    </td>
                                                </tr>
                                                <tr class="item-row">
                                                    <td></td>
                                                    <td>8</td>
                                                    <td>Uses specific strategies or modifications for children with disabilities/delays, or who are dual-language learners</td>
                                                    <td>
                                                        <dx:BootstrapTextBox ID="txtItem8NumYes" runat="server" NullText="N/O" OnValidation="TPITOSItemNumbers_Validation">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <CssClasses Control="item-yes" />
                                                            <ClientSideEvents ValueChanged="calculateItemPercentage" Validation="validateTPITOSItemNumbers" Init="calculateItemPercentage" />
                                                            <MaskSettings Mask="999" PromptChar=" " ErrorText="Must be a valid number!" />
                                                            <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgTPITOS" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="false" ErrorText="Required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapTextBox>
                                                    </td>
                                                    <td>
                                                        <dx:BootstrapTextBox ID="txtItem8NumNo" runat="server" NullText="N/O" OnValidation="TPITOSItemNumbers_Validation">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <CssClasses Control="item-no" />
                                                            <ClientSideEvents ValueChanged="calculateItemPercentage" Validation="validateTPITOSItemNumbers" />
                                                            <MaskSettings Mask="999" PromptChar=" " ErrorText="Must be a valid number!" />
                                                            <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgTPITOS" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="false" ErrorText="Required" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapTextBox>
                                                    </td>
                                                    <td>
                                                        <label class="item-percentage"></label>
                                                    </td>
                                                </tr>
                                                <tr class="item-row">
                                                    <td></td>
                                                    <td>9</td>
                                                    <td>Conveys predictability through carefully planned schedule, routines, and transitions</td>
                                                    <td>
                                                        <dx:BootstrapTextBox ID="txtItem9NumYes" runat="server" NullText="N/O" OnValidation="TPITOSItemNumbers_Validation">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <CssClasses Control="item-yes" />
                                                            <ClientSideEvents ValueChanged="calculateItemPercentage" Validation="validateTPITOSItemNumbers" Init="calculateItemPercentage" />
                                                            <MaskSettings Mask="999" PromptChar=" " ErrorText="Must be a valid number!" />
                                                            <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgTPITOS" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="false" ErrorText="Required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapTextBox>
                                                    </td>
                                                    <td>
                                                        <dx:BootstrapTextBox ID="txtItem9NumNo" runat="server" NullText="N/O" OnValidation="TPITOSItemNumbers_Validation">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <CssClasses Control="item-no" />
                                                            <ClientSideEvents ValueChanged="calculateItemPercentage" Validation="validateTPITOSItemNumbers" />
                                                            <MaskSettings Mask="999" PromptChar=" " ErrorText="Must be a valid number!" />
                                                            <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgTPITOS" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="false" ErrorText="Required" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapTextBox>
                                                    </td>
                                                    <td>
                                                        <label class="item-percentage"></label>
                                                    </td>
                                                </tr>
                                                <tr class="item-row">
                                                    <td></td>
                                                    <td>10</td>
                                                    <td>Environment is arranged to foster social-emotional development</td>
                                                    <td>
                                                        <dx:BootstrapTextBox ID="txtItem10NumYes" runat="server" NullText="N/O" OnValidation="TPITOSItemNumbers_Validation">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <CssClasses Control="item-yes" />
                                                            <ClientSideEvents ValueChanged="calculateItemPercentage" Validation="validateTPITOSItemNumbers" Init="calculateItemPercentage" />
                                                            <MaskSettings Mask="999" PromptChar=" " ErrorText="Must be a valid number!" />
                                                            <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgTPITOS" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="false" ErrorText="Required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapTextBox>
                                                    </td>
                                                    <td>
                                                        <dx:BootstrapTextBox ID="txtItem10NumNo" runat="server" NullText="N/O" OnValidation="TPITOSItemNumbers_Validation">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <CssClasses Control="item-no" />
                                                            <ClientSideEvents ValueChanged="calculateItemPercentage" Validation="validateTPITOSItemNumbers" />
                                                            <MaskSettings Mask="999" PromptChar=" " ErrorText="Must be a valid number!" />
                                                            <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgTPITOS" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="false" ErrorText="Required" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapTextBox>
                                                    </td>
                                                    <td>
                                                        <label class="item-percentage"></label>
                                                    </td>
                                                </tr>
                                                <tr class="item-row">
                                                    <td></td>
                                                    <td>11</td>
                                                    <td>Collaborates with his/her peers to support children's social emotional development</td>
                                                    <td>
                                                        <dx:BootstrapTextBox ID="txtItem11NumYes" runat="server" NullText="N/O" OnValidation="TPITOSItemNumbers_Validation">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <CssClasses Control="item-yes" />
                                                            <ClientSideEvents ValueChanged="calculateItemPercentage" Validation="validateTPITOSItemNumbers" Init="calculateItemPercentage" />
                                                            <MaskSettings Mask="999" PromptChar=" " ErrorText="Must be a valid number!" />
                                                            <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgTPITOS" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="false" ErrorText="Required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapTextBox>
                                                    </td>
                                                    <td>
                                                        <dx:BootstrapTextBox ID="txtItem11NumNo" runat="server" NullText="N/O" OnValidation="TPITOSItemNumbers_Validation">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <CssClasses Control="item-no" />
                                                            <ClientSideEvents ValueChanged="calculateItemPercentage" Validation="validateTPITOSItemNumbers" />
                                                            <MaskSettings Mask="999" PromptChar=" " ErrorText="Must be a valid number!" />
                                                            <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgTPITOS" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="false" ErrorText="Required" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapTextBox>
                                                    </td>
                                                    <td>
                                                        <label class="item-percentage"></label>
                                                    </td>
                                                </tr>
                                                <tr class="item-row">
                                                    <td></td>
                                                    <td>12</td>
                                                    <td>Has effective strategies for engaging parents in supporting their child's social-emotional development and addressing challenging behavior</td>
                                                    <td>
                                                        <dx:BootstrapTextBox ID="txtItem12NumYes" runat="server" NullText="N/O" OnValidation="TPITOSItemNumbers_Validation">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <CssClasses Control="item-yes" />
                                                            <ClientSideEvents ValueChanged="calculateItemPercentage" Validation="validateTPITOSItemNumbers" Init="calculateItemPercentage" />
                                                            <MaskSettings Mask="999" PromptChar=" " ErrorText="Must be a valid number!" />
                                                            <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgTPITOS" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="false" ErrorText="Required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapTextBox>
                                                    </td>
                                                    <td>
                                                        <dx:BootstrapTextBox ID="txtItem12NumNo" runat="server" NullText="N/O" OnValidation="TPITOSItemNumbers_Validation">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <CssClasses Control="item-no" />
                                                            <ClientSideEvents ValueChanged="calculateItemPercentage" Validation="validateTPITOSItemNumbers" />
                                                            <MaskSettings Mask="999" PromptChar=" " ErrorText="Must be a valid number!" />
                                                            <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgTPITOS" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="false" ErrorText="Required" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapTextBox>
                                                    </td>
                                                    <td>
                                                        <label class="item-percentage"></label>
                                                    </td>
                                                </tr>
                                                <tr class="item-row">
                                                    <td></td>
                                                    <td>13</td>
                                                    <td>Has effective strategies for communicating with families and promoting family involvement in the classroom</td>
                                                    <td>
                                                        <dx:BootstrapTextBox ID="txtItem13NumYes" runat="server" NullText="N/O" OnValidation="TPITOSItemNumbers_Validation">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <CssClasses Control="item-yes" />
                                                            <ClientSideEvents ValueChanged="calculateItemPercentage" Validation="validateTPITOSItemNumbers" Init="calculateItemPercentage" />
                                                            <MaskSettings Mask="999" PromptChar=" " ErrorText="Must be a valid number!" />
                                                            <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgTPITOS" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="false" ErrorText="Required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapTextBox>
                                                    </td>
                                                    <td>
                                                        <dx:BootstrapTextBox ID="txtItem13NumNo" runat="server" NullText="N/O" OnValidation="TPITOSItemNumbers_Validation">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <CssClasses Control="item-no" />
                                                            <ClientSideEvents ValueChanged="calculateItemPercentage" Validation="validateTPITOSItemNumbers" />
                                                            <MaskSettings Mask="999" PromptChar=" " ErrorText="Must be a valid number!" />
                                                            <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgTPITOS" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="false" ErrorText="Required" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapTextBox>
                                                    </td>
                                                    <td>
                                                        <label class="item-percentage"></label>
                                                    </td>
                                                </tr>
                                            </tbody>
                                        </table>
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
                                Subscale 2. Red Flags
                            </div>
                            <div class="card-body">
                                <div class="row mt-2">
                                    <div class="col-md-12">
                                        <label>Red Flag Counts</label>
                                        <table id="tblRedFlags" class="table table-striped table-bordered table-hover">
                                            <thead>
                                                <tr>
                                                    <th data-priority="1"></th>
                                                    <th data-priority="3"></th>
                                                    <th>Red Flags</th>
                                                    <th data-priority="2"># Yes</th>
                                                    <th data-priority="2"># Possible</th>
                                                    <th data-priority="4">% Yes</th>
                                                </tr>
                                            </thead>
                                            <tbody>
                                                <tr class="red-flag-row">
                                                    <td></td>
                                                    <td>1</td>
                                                    <td>Observed Teacher</td>
                                                    <td>
                                                        <dx:BootstrapTextBox ID="txtLeadTeacherRedFlagsYes" runat="server" NullText="N/O" OnValidation="TPITOSRedFlagNumbers_Validation">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <CssClasses Control="red-flag-yes" />
                                                            <ClientSideEvents Init="calculateRedFlagPercentage" Validation="validateTPITOSRedFlagNumbers" ValueChanged="calculateRedFlagPercentage" />
                                                            <MaskSettings Mask="999" PromptChar=" " ErrorText="Must be a valid number!" />
                                                            <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgTPITOS" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="false" ErrorText="Required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapTextBox>
                                                    </td>
                                                    <td>
                                                        <dx:BootstrapTextBox ID="txtLeadTeacherRedFlagsPossible" runat="server" NullText="N/O" OnValidation="TPITOSRedFlagNumbers_Validation">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <CssClasses Control="red-flag-possible" />
                                                            <ClientSideEvents ValueChanged="calculateRedFlagPercentage" Validation="validateTPITOSRedFlagNumbers" />
                                                            <MaskSettings Mask="999" PromptChar=" " ErrorText="Must be a valid number!" />
                                                            <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgTPITOS" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="false" ErrorText="Required" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapTextBox>
                                                    </td>
                                                    <td>
                                                        <label class="red-flag-percentage"></label>
                                                    </td>
                                                </tr>
                                                <tr class="red-flag-row">
                                                    <td></td>
                                                    <td>2</td>
                                                    <td>Other Teacher (if scored)</td>
                                                    <td>
                                                        <dx:BootstrapTextBox ID="txtOtherTeacherRedFlagsYes" runat="server" NullText="N/O" OnValidation="TPITOSRedFlagNumbers_Validation">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <CssClasses Control="red-flag-yes" />
                                                            <ClientSideEvents ValueChanged="calculateRedFlagPercentage" Validation="validateTPITOSRedFlagNumbers" Init="calculateRedFlagPercentage" />
                                                            <MaskSettings Mask="999" PromptChar=" " ErrorText="Must be a valid number!" />
                                                            <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgTPITOS" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="false" ErrorText="Required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapTextBox>
                                                    </td>
                                                    <td>
                                                        <dx:BootstrapTextBox ID="txtOtherTeacherRedFlagsPossible" runat="server" NullText="N/O" OnValidation="TPITOSRedFlagNumbers_Validation">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <CssClasses Control="red-flag-possible" />
                                                            <ClientSideEvents ValueChanged="calculateRedFlagPercentage" Validation="validateTPITOSRedFlagNumbers" />
                                                            <MaskSettings Mask="999" PromptChar=" " ErrorText="Must be a valid number!" />
                                                            <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgTPITOS" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="false" ErrorText="Required" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapTextBox>
                                                    </td>
                                                    <td>
                                                        <label class="red-flag-percentage"></label>
                                                    </td>
                                                </tr>
                                                <tr class="red-flag-row">
                                                    <td></td>
                                                    <td>3</td>
                                                    <td>Classroom</td>
                                                    <td>
                                                        <dx:BootstrapTextBox ID="txtClassroomRedFlagsYes" runat="server" NullText="N/O" OnValidation="TPITOSRedFlagNumbers_Validation">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <CssClasses Control="red-flag-yes" />
                                                            <ClientSideEvents Init="calculateRedFlagPercentage" Validation="validateTPITOSRedFlagNumbers" ValueChanged="calculateRedFlagPercentage" />
                                                            <MaskSettings Mask="999" PromptChar=" " ErrorText="Must be a valid number!" />
                                                            <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgTPITOS" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="false" ErrorText="Required!" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapTextBox>
                                                    </td>
                                                    <td>
                                                        <dx:BootstrapTextBox ID="txtClassroomRedFlagsPossible" runat="server" NullText="N/O" OnValidation="TPITOSRedFlagNumbers_Validation">
                                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                            <CssClasses Control="red-flag-possible" />
                                                            <ClientSideEvents ValueChanged="calculateRedFlagPercentage" Validation="validateTPITOSRedFlagNumbers" />
                                                            <MaskSettings Mask="999" PromptChar=" " ErrorText="Must be a valid number!" />
                                                            <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgTPITOS" EnableCustomValidation="true">
                                                                <RequiredField IsRequired="false" ErrorText="Required" />
                                                            </ValidationSettings>
                                                        </dx:BootstrapTextBox>
                                                    </td>
                                                    <td>
                                                        <label class="red-flag-percentage"></label>
                                                    </td>
                                                </tr>
                                            </tbody>
                                        </table>
                                    </div>
                                </div>
                                <hr />
                                <label>Red Flag Type Legend</label>
                                <div class="row">
                                    <div class="col-sm-12 h5">
                                        <span class="badge badge-info text-wrap mb-2">(C) = Classroom Red Flag</span>
                                        <span class="badge badge-info text-wrap mb-2">(T) = Teacher Red Flag</span>
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-lg-12">
                                        <dx:BootstrapListBox ID="lstBxRedFlags" runat="server" Caption="Types of Red Flags Observed" 
                                            SelectionMode="CheckColumn" EnableSelectAll="false"
                                            ValueField="CodeTPITOSRedFlagPK" ValueType="System.Int32" TextField="Description">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <CssClasses Control="mw-100" />
                                            <FilteringSettings ShowSearchUI="true" EditorNullTextDisplayMode="Unfocused" />
                                            <ValidationSettings ValidationGroup="vgTPITOS" ErrorDisplayMode="ImageWithText">
                                                <RequiredField IsRequired="false" ErrorText="At least one Red Flag type must be selected!" />
                                            </ValidationSettings>
                                        </dx:BootstrapListBox>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="submitTPITOS" />
            </Triggers>
        </asp:UpdatePanel>
    </div>
    <div class="page-footer">
        <uc:Submit ID="submitTPITOS" runat="server" ValidationGroup="vgTPITOS" OnSubmitClick="submitTPITOS_Click" OnCancelClick="submitTPITOS_CancelClick" OnValidationFailed="submitTPITOS_ValidationFailed" />
    </div>
    <div class="modal" id="divDeleteTPITOSParticipantModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete TPITOS Participant</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this TPITOS participant?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteTPITOSParticipant" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteTPITOSParticipantModal" OnClick="lbDeleteTPITOSParticipant_Click"><i class="fas fa-check"></i> Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
