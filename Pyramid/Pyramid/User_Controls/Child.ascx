<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Child.ascx.cs" Inherits="Pyramid.User_Controls.Child" %>
<%@ Register Assembly="DevExpress.Web.Bootstrap.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>
<%@ Register TagPrefix="uc" TagName="Messaging" Src="~/User_Controls/MessagingSystem.ascx" %>

<script>
    //This function controls whether or not the specify field for gender is shown
    function showHideGenderSpecify() {
        //Get the gender
        var selectedGender = ddGender.GetText();

        //Set the specify div visibility
        if (selectedGender.toLowerCase() == 'other') {
            $('#divGenderSpecify').slideDown();
        }
        else {
            //Clear the specify text box and hide the specify div
            txtGenderSpecify.SetValue('');
            $('#divGenderSpecify').slideUp();
        }
    }

    //This function controls whether or not the specify field for ethnicity is shown
    function showHideEthnicitySpecify() {
        //Get the ethnicity
        var selectedEthnicity = ddEthnicity.GetText();

        //Set the specify div visibility
        if (selectedEthnicity.toLowerCase() == 'other') {
            $('#divEthnicitySpecify').slideDown();
        }
        else {
            //Clear the specify text box and hide the specify div
            txtEthnicitySpecify.SetValue('');
            $('#divEthnicitySpecify').slideUp();
        }
    }

    //This function controls whether or not the specify field for race is shown
    function showHideRaceSpecify() {
        //Get the race
        var selectedRace = ddRace.GetText();

        //Set the specify div visibility
        if (selectedRace.toLowerCase() == 'other') {
            $('#divRaceSpecify').slideDown();
        }
        else {
            //Clear the specify text box and hide the specify div
            txtRaceSpecify.SetValue('');
            $('#divRaceSpecify').slideUp();
        }
    }

    //This function controls whether or not the specify field for the discharge reason is shown
    //based on the value in the ddDischargeReason Combo Box
    function showHideDischargeReasonSpecify() {
        //Get the discharge reason
        var dischargeReason = ddDischargeReason.GetText();

        //If the discharge reason is other, show the specify div
        if (dischargeReason.toLowerCase() == 'other') {
            $('#divDischargeReasonSpecify').slideDown();
        }
        else {
            //The discharge reason is not other, clear the specify text box and hide the specify div
            txtDischargeReasonSpecify.SetValue('');
            $('#divDischargeReasonSpecify').slideUp();
        }
    }

    //This function controls whether or not the parent permission document controls are displayed
    //based on the value in the hfHasParentPermissionDocument hidden field
    function showHidePermissionUpload() {
        //Get the boolean value from the hidden field
        var hasParentPermissionDocument = $('[ID$="hfHasParentPermissionDocument"]').val();

        //Based on the boolean, set the display of the document controls
        if (hasParentPermissionDocument === 'true') {
            $('#divParentPermissionFileUpload').hide();
            $('#divParentPermissionFileDisplay').show();
            btnCancelDocumentUpdate.SetVisible(true);
        }
        else {
            $('#divParentPermissionFileUpload').show();
            $('#divParentPermissionFileDisplay').hide();
            btnCancelDocumentUpdate.SetVisible(false);
        }
    }

    //This function fires when the user clicks the update document button
    function btnUpdateDocument_Click(s, e) {
        //Prevent the postback
        e.processOnServer = false;

        //Set the display of the document divs
        $('#divParentPermissionFileUpload').show();
        $('#divParentPermissionFileDisplay').hide();
    }

    //This function fires when the user clicks the cancel update button
    function btnCancelDocumentUpdate_Click(s, e) {
        //Prevent the postback
        e.processOnServer = false;

        //Set the display of the document divs
        $('#divParentPermissionFileUpload').hide();
        $('#divParentPermissionFileDisplay').show();
    }

    //This function fires when the user clicks the delete document button
    function btnDeletePermissionFile_Click(s, e) {
        //Prevent the postback
        e.processOnServer = false;

        //Show the modal
        $('#divDeletePermissionFileModal').modal('show');
    }

    //Validate the enrollment date field
    function validateEnrollmentDate(s, e) {
        var enrollmentDate = e.value;
        var dischargeDate = deDischargeDate.GetDate();
        var DOB = deDOB.GetDate();

        if (enrollmentDate == null) {
            e.isValid = false;
            e.errorText = "Enrollment Date is required!";
        }
        else if (enrollmentDate > new Date()) {
            e.isValid = false;
            e.errorText = "Enrollment Date cannot be in the future!";
        }
        else if (dischargeDate != null && enrollmentDate >= dischargeDate) {
            e.isValid = false;
            e.errorText = "Enrollment Date must be before the discharge date!";
        }
        else if (DOB != null && enrollmentDate < DOB) {
            e.isValid = false;
            e.errorText = "Enrollment Date cannot be before the Child's Date of Birth!";
        }
    }

    //Validate the discharge date field
    function validateDischargeDate(s, e) {
        var dischargeDate = e.value;
        var enrollmentDate = deEnrollmentDate.GetDate();
        var dischargeReason = ddDischargeReason.GetValue();

        if (dischargeDate == null && dischargeReason != null) {
            e.isValid = false;
            e.errorText = "Discharge Date is required if you have a Discharge Reason!";
        }
        else if (enrollmentDate == null && dischargeDate != null) {
            e.isValid = false;
            e.errorText = "Enrollment Date must be entered before the Discharge Date!";
        }
        else if (dischargeDate != null && dischargeDate < enrollmentDate) {
            e.isValid = false;
            e.errorText = "Discharge Date must be after the Enrollment Date!";
        }
        else if (dischargeDate != null && dischargeDate > new Date()) {
            e.isValid = false;
            e.errorText = "Discharge Date cannot be in the future!";
        }
    }

    //Validate the discharge reason field
    function validateDischargeReason(s, e) {
        var dischargeReason = e.value;
        var dischargeDate = deDischargeDate.GetDate();

        if (dischargeDate != null && dischargeReason == null) {
            e.isValid = false;
            e.errorText = "Discharge Reason is required if you have a Discharge Date!"
        }
        else {
            e.isValid = true;
        }
    }

    //Validate the discharge reason specify field
    function validateDischargeReasonSpecify(s, e) {
        var reasonSpecify = e.value;
        var dischargeReason = ddDischargeReason.GetText();

        if ((reasonSpecify == null || reasonSpecify == ' ') && dischargeReason.toLowerCase() == 'other') {
            e.isValid = false;
            e.errorText = "Specify Discharge Reason is required when the 'Other' discharge reason is selected!";
        }
        else {
            e.isValid = true;
        }
    }
</script>

<uc:Messaging ID="childControlMsgSys" runat="server" />
<asp:HiddenField ID="hfChildPK" runat="server" Value="0" />
<asp:HiddenField ID="hfChildProgramPK" runat="server" Value="0" />
<asp:HiddenField ID="hfProgramFK" runat="server" Value="0" />
<div class="row">
    <div class="col-lg-4">
        <div class="form-group">
            <dx:BootstrapTextBox ID="txtFirstName" runat="server" Caption="First Name">
                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                <ValidationSettings ValidationGroup="vgChild" ErrorDisplayMode="ImageWithText">
                    <RequiredField IsRequired="true" ErrorText="First Name is required!" />
                </ValidationSettings>
            </dx:BootstrapTextBox>
        </div>
    </div>
    <div class="col-lg-4">
        <div class="form-group">
            <dx:BootstrapTextBox ID="txtLastName" runat="server" Caption="Last Name">
                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                <ValidationSettings ValidationGroup="vgChild" ErrorDisplayMode="ImageWithText">
                    <RequiredField IsRequired="true" ErrorText="Last Name is required!" />
                </ValidationSettings>
            </dx:BootstrapTextBox>
        </div>
    </div>
    <div class="col-lg-4">
        <div class="form-group">
            <dx:BootstrapDateEdit ID="deDOB" runat="server" Caption="Date of Birth" EditFormat="Date" EditFormatString="MM/dd/yyyy"
                AllowNull="true" UseMaskBehavior="true" AllowMouseWheel="false" ClientInstanceName="deDOB" 
                PickerDisplayMode="Calendar" MinDate="01/01/1900">
                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                <CalendarProperties ShowClearButton="true"></CalendarProperties>
                <ValidationSettings ValidationGroup="vgChild" ErrorDisplayMode="ImageWithText">
                    <RequiredField IsRequired="true" ErrorText="Date of Birth is required!" />
                </ValidationSettings>
            </dx:BootstrapDateEdit>
        </div>
    </div>
</div>
<div class="row">
    <div class="col-lg-4">
        <div class="form-group">
            <dx:BootstrapTextBox ID="txtProgramID" runat="server" Caption="ID Number" MaxLength="100"
                OnValidation="txtProgramID_Validation">
                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                <ValidationSettings ValidationGroup="vgChild" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                    <RequiredField IsRequired="false" ErrorText="ID Number is required!" />
                </ValidationSettings>
            </dx:BootstrapTextBox>
            <button type="button" class="btn btn-link p-0" data-toggle="popover" data-trigger="hover focus" title="Help" data-content="This field is not required, but will be automatically populated in the format of CID-[number] if you do not enter an ID number.  You can either leave the system-generated ID number there or create your own ID number."><i class="fas fa-question-circle"></i>&nbsp;Help</button>
        </div>
    </div>
    <div class="col-lg-4">
        <div class="form-group">
            <dx:BootstrapDateEdit ID="deEnrollmentDate" runat="server" Caption="Enrollment Date" EditFormat="Date"
                EditFormatString="MM/dd/yyyy" UseMaskBehavior="true" 
                ClientInstanceName="deEnrollmentDate" 
                OnValidation="deEnrollmentDate_Validation" AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900">
                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                <ClientSideEvents Validation="validateEnrollmentDate" />
                <CalendarProperties ShowClearButton="true"></CalendarProperties>
                <ValidationSettings ValidationGroup="vgChild" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                    <RequiredField IsRequired="true" ErrorText="Enrollment Date is required!" />
                </ValidationSettings>
            </dx:BootstrapDateEdit>
        </div>
    </div>
</div>
<div class="row">
    <div class="col-lg-4">
        <div class="form-group">
            <dx:BootstrapComboBox ID="ddGender" runat="server" Caption="Gender" NullText="--Select--"
                TextField="Description" ValueField="CodeGenderPK" ValueType="System.Int32" 
                IncrementalFilteringMode="StartsWith" AllowMouseWheel="false" ClientInstanceName="ddGender">
                <ClientSideEvents Init="showHideGenderSpecify" SelectedIndexChanged="showHideGenderSpecify" />
                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                <ValidationSettings ValidationGroup="vgChild" ErrorDisplayMode="ImageWithText">
                    <RequiredField IsRequired="true" ErrorText="Gender is required!" />
                </ValidationSettings>
            </dx:BootstrapComboBox>
            <div id="divGenderSpecify" style="display: none">
                <dx:BootstrapTextBox ID="txtGenderSpecify" runat="server" Caption="Specify Gender" MaxLength="100"
                    OnValidation="txtGenderSpecify_Validation" ClientInstanceName="txtGenderSpecify">
                    <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                    <ValidationSettings ValidationGroup="vgChild" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                    </ValidationSettings>
                </dx:BootstrapTextBox>
            </div>
        </div>
    </div>
    <div class="col-lg-4">
        <div class="form-group">
            <dx:BootstrapComboBox ID="ddEthnicity" runat="server" Caption="Ethnicity" NullText="--Select--"
                TextField="Description" ValueField="CodeEthnicityPK" ValueType="System.Int32" 
                IncrementalFilteringMode="StartsWith" AllowMouseWheel="false" ClientInstanceName="ddEthnicity">
                <ClientSideEvents Init="showHideEthnicitySpecify" SelectedIndexChanged="showHideEthnicitySpecify" />
                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                <ValidationSettings ValidationGroup="vgChild" ErrorDisplayMode="ImageWithText">
                    <RequiredField IsRequired="true" ErrorText="Ethnicity is required!" />
                </ValidationSettings>
            </dx:BootstrapComboBox>
            <div id="divEthnicitySpecify" style="display: none">
                <dx:BootstrapTextBox ID="txtEthnicitySpecify" runat="server" Caption="Specify Ethnicity" MaxLength="100"
                    OnValidation="txtEthnicitySpecify_Validation" ClientInstanceName="txtEthnicitySpecify">
                    <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                    <ValidationSettings ValidationGroup="vgChild" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                    </ValidationSettings>
                </dx:BootstrapTextBox>
            </div>
        </div>
    </div>
    <div class="col-lg-4">
        <div class="form-group">
            <dx:BootstrapComboBox ID="ddRace" runat="server" Caption="Race" NullText="--Select--"
                TextField="Description" ValueField="CodeRacePK" ValueType="System.Int32" 
                IncrementalFilteringMode="StartsWith" AllowMouseWheel="false" ClientInstanceName="ddRace">
                <ClientSideEvents Init="showHideRaceSpecify" SelectedIndexChanged="showHideRaceSpecify" />
                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                <ValidationSettings ValidationGroup="vgChild" ErrorDisplayMode="ImageWithText">
                    <RequiredField IsRequired="true" ErrorText="Race is required!" />
                </ValidationSettings>
            </dx:BootstrapComboBox>
            <div id="divRaceSpecify" style="display: none">
                <dx:BootstrapTextBox ID="txtRaceSpecify" runat="server" Caption="Specify Race" MaxLength="100"
                    OnValidation="txtRaceSpecify_Validation" ClientInstanceName="txtRaceSpecify">
                    <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                    <ValidationSettings ValidationGroup="vgChild" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                    </ValidationSettings>
                </dx:BootstrapTextBox>
            </div>
        </div>
    </div>
</div>
<div class="row">
    <div class="col-lg-4">
        <div class="form-group">
            <dx:BootstrapComboBox ID="ddDLL" runat="server" Caption="Dual Language Learner (DLL)" NullText="--Select--"
                IncrementalFilteringMode="StartsWith" AllowMouseWheel="false" ValueType="System.Boolean">
                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                <ValidationSettings ValidationGroup="vgChild" ErrorDisplayMode="ImageWithText">
                    <RequiredField IsRequired="true" ErrorText="DLL is required!" />
                </ValidationSettings>
                <Items>
                    <dx:BootstrapListEditItem Value="True" Text="Yes"></dx:BootstrapListEditItem>
                    <dx:BootstrapListEditItem Value="False" Text="No"></dx:BootstrapListEditItem>
                </Items>
            </dx:BootstrapComboBox>
        </div>
    </div>
    <div class="col-lg-4">
        <div class="form-group">
            <dx:BootstrapComboBox ID="ddIEP" runat="server" Caption="Individualized Education Program (IEP)" NullText="--Select--"
                IncrementalFilteringMode="StartsWith" AllowMouseWheel="false" ValueType="System.Boolean">
                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                <ValidationSettings ValidationGroup="vgChild" ErrorDisplayMode="ImageWithText">
                    <RequiredField IsRequired="true" ErrorText="IEP is required!" />
                </ValidationSettings>
                <Items>
                    <dx:BootstrapListEditItem Value="True" Text="Yes"></dx:BootstrapListEditItem>
                    <dx:BootstrapListEditItem Value="False" Text="No"></dx:BootstrapListEditItem>
                </Items>
            </dx:BootstrapComboBox>
        </div>
    </div>
</div>
<div class="row">
    <div class="col-lg-4">
        <div class="form-group">
            <dx:BootstrapComboBox ID="ddHasParentPermission" runat="server" Caption="Parent/Guardian Permission" NullText="--Select--" AllowNull="true" ClearButton-DisplayMode="Always"
                IncrementalFilteringMode="StartsWith" AllowMouseWheel="false" ValueType="System.Boolean" OnValidation="ddHasParentPermission_Validation">
                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                <ValidationSettings ValidationGroup="vgChild" ErrorDisplayMode="ImageWithText">
                    <RequiredField IsRequired="true" ErrorText="Parent/Guardian Permission must be answered yes in order to continue!" />
                </ValidationSettings>
                <Items>
                    <dx:BootstrapListEditItem Value="True" Text="Yes"></dx:BootstrapListEditItem>
                </Items>
            </dx:BootstrapComboBox>
            <button id="btnParentPermissionHelp" type="button" class="btn btn-link p-0" data-toggle="popover" data-trigger="hover focus" title="Help" data-content="Selecting yes in this field indicates the parent/guardian has provided permission for this child's information to be entered into PIDS."><i class="fas fa-question-circle"></i>&nbsp;Help</button>
        </div>
    </div>
    <div class="col-lg-4">
        <div class="form-group">
            <label>Parent/Guardian Permission Document</label>
            <asp:HiddenField ID="hfHasParentPermissionDocument" runat="server" Value="false" />
            <div id="divParentPermissionFileUpload" style="display: none;">
                <label id="lblUploadControl">Document to upload</label>
                <dx:BootstrapUploadControl ID="bucParentPermissionDocument" runat="server" ShowUploadButton="false" aria-labelledby="lblUploadControl">
                    <ClientSideEvents Init="showHidePermissionUpload" />
                    <ValidationSettings MaxFileSize="20000000" AllowedFileExtensions=".pdf,.doc,.docx,.jpeg,.jpg,.png" />
                </dx:BootstrapUploadControl>
                <small>Allowed file extensions: .pdf, .doc, .docx, .jpeg, .jpg, .png</small>
                <br />
                <small>Maximum file size: 20 MB.</small>
                <div class="mt-2">
                    <dx:BootstrapButton ID="btnCancelDocumentUpdate" runat="server" Text="Cancel Update" ClientInstanceName="btnCancelDocumentUpdate">
                        <ClientSideEvents Click="btnCancelDocumentUpdate_Click" />
                        <CssClasses Icon="fas fa-times" Control="btn btn-secondary" />
                    </dx:BootstrapButton>
                </div>
            </div>
            <div id="divParentPermissionFileDisplay" style="display: none;">
                <div class="btn-group">
                    <button type="button" class="btn btn-secondary dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                        Actions
                    </button>
                    <div class="dropdown-menu dropdown-menu-right">
                        <asp:HyperLink ID="lnkDisplayParentPermissionFile" runat="server" Target="_blank" CssClass="dropdown-item"><i class="fas fa-file-download"></i>&nbsp;View/Download</asp:HyperLink>
                        <dx:BootstrapButton ID="btnUpdateDocument" runat="server" Text="Update Document">
                            <ClientSideEvents Click="btnUpdateDocument_Click" />
                            <CssClasses Icon="fas fa-file-upload" Control="dropdown-item" />
                        </dx:BootstrapButton>
                        <dx:BootstrapButton ID="btnDeletePermissionFile" runat="server" Text="Delete Document">
                            <ClientSideEvents Click="btnDeletePermissionFile_Click" />
                            <CssClasses Icon="fas fa-trash" Control="dropdown-item" />
                        </dx:BootstrapButton>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>
<div class="row">
    <div class="col-lg-4">
        <div class="form-group">
            <dx:BootstrapDateEdit ID="deDischargeDate" runat="server" Caption="Discharge Date" EditFormat="Date" AllowNull="true"
                EditFormatString="MM/dd/yyyy" UseMaskBehavior="true"
                ClientInstanceName="deDischargeDate" 
                OnValidation="deDischargeDate_Validation" AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900">
                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                <ClientSideEvents Validation="validateDischargeDate" />
                <CalendarProperties ShowClearButton="true"></CalendarProperties>
                <ValidationSettings ValidationGroup="vgChild" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                    <RequiredField IsRequired="false" ErrorText="Discharge Date is required!" />
                </ValidationSettings>
            </dx:BootstrapDateEdit>
        </div>
    </div>
    <div class="col-lg-4">
        <div class="form-group">
            <dx:BootstrapComboBox ID="ddDischargeReason" runat="server" Caption="Discharge Reason" NullText="--Select--" AllowNull="true"
                TextField="Description" ValueField="CodeDischargeReasonPK" ValueType="System.Int32"
                IncrementalFilteringMode="Contains" AllowMouseWheel="false"
                OnValidation="ddDischargeReason_Validation" ClientInstanceName="ddDischargeReason"
                ClearButton-DisplayMode="Always">
                <ClientSideEvents Validation="validateDischargeReason" Init="showHideDischargeReasonSpecify" SelectedIndexChanged="showHideDischargeReasonSpecify" />
                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                <ValidationSettings ValidationGroup="vgChild" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                    <RequiredField IsRequired="false" ErrorText="Discharge Reason is required!" />
                </ValidationSettings>
            </dx:BootstrapComboBox>
            <div id="divDischargeReasonSpecify" style="display: none">
                <dx:BootstrapTextBox ID="txtDischargeReasonSpecify" runat="server" Caption="Specify Discharge Reason" MaxLength="500"
                    OnValidation="txtDischargeReasonSpecify_Validation" ClientInstanceName="txtDischargeReasonSpecify">
                    <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                    <ClientSideEvents Validation="validateDischargeReasonSpecify" />
                    <ValidationSettings ValidationGroup="vgChild" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                    </ValidationSettings>
                </dx:BootstrapTextBox>
            </div>
        </div>
    </div>
</div>
<div class="modal" id="divDeletePermissionFileModal">
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <h4 class="modal-title">Delete Parent/Guardian Permission Document</h4>
                <button type="button" class="close" data-dismiss="modal">&times;</button>
            </div>
            <div class="modal-body">
                Are you sure you want to delete this Parent/Guardian Permission Document?
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                <asp:LinkButton ID="lbConfirmPermissionFileDelete" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeletePermissionFileModal" OnClick="lbConfirmPermissionFileDelete_Click"><i class="fas fa-check"></i>&nbsp;Yes</asp:LinkButton>
            </div>
        </div>
    </div>
</div>