<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Child.ascx.cs" Inherits="Pyramid.User_Controls.Child" %>
<%@ Register Assembly="DevExpress.Web.Bootstrap.v19.1, Version=19.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>
<%@ Register TagPrefix="uc" TagName="Messaging" Src="~/User_Controls/MessagingSystem.ascx" %>

<script>
    //This function controls whether or not the specify field for the discharge reason is shown
    //based on the value in the ddDischargeReason Combo Box
    function showHideDischargeReasonSpecify() {
        //Get the discharge reason
        var dischargeReason = ddDischargeReason.GetText();

        //If the discharge reason is other, show the specify div
        if (dischargeReason.toLowerCase().includes('other')) {
            $('#divDischargeReasonSpecify').slideDown();
        }
        else {
            //The discharge reason is not other, clear the specify text box and hide the specify div
            txtDischargeReasonSpecify.SetValue('');
            $('#divDischargeReasonSpecify').slideUp();
        }
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

    //Validate the program ID field
    function validateProgramID(s, e) {
        var programID = e.value;
        var usedIDs = $('[ID$="hfUsedIDs"]').val();
        var usedIDArray = usedIDs.split(',');

        if (programID == null) {
            e.isValid = false;
            e.errorText = "ID Number is required!";
        }
        else if (usedIDArray.includes(programID)) {
            e.isValid = false;
            e.errorText = "That ID number is already taken!";
        }
        else {
            e.isValid = true;
        }
    }

    //Validate the discharge reason specify field
    function validateDischargeReasonSpecify(s, e) {
        var reasonSpecify = e.value;
        var dischargeReason = ddDischargeReason.GetText();

        if ((reasonSpecify == null || reasonSpecify == ' ') && dischargeReason.toLowerCase().includes('other')) {
            e.isValid = false;
            e.errorText = "Specify Discharge Reason is required when the 'Other' discharge reason is selected!";
        }
        else {
            e.isValid = true;
        }
    }
</script>

<asp:HiddenField ID="hfChildPK" runat="server" Value="0" />
<asp:HiddenField ID="hfChildProgramPK" runat="server" Value="0" />
<asp:HiddenField ID="hfProgramFK" runat="server" Value="0" />
<asp:HiddenField ID="hfUsedIDs" runat="server" Value="" />
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
                UseMaskBehavior="true" AllowMouseWheel="false" ClientInstanceName="deDOB" 
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
                <ClientSideEvents Validation="validateProgramID" />
                <ValidationSettings ValidationGroup="vgChild" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                    <RequiredField IsRequired="true" ErrorText="ID Number is required!" />
                </ValidationSettings>
            </dx:BootstrapTextBox>
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
    <div class="col-lg-4">
        <div class="form-group">
            <dx:BootstrapComboBox ID="ddGender" runat="server" Caption="Gender" NullText="--Select--"
                TextField="Description" ValueField="CodeGenderPK" ValueType="System.Int32" 
                IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                <ValidationSettings ValidationGroup="vgChild" ErrorDisplayMode="ImageWithText">
                    <RequiredField IsRequired="true" ErrorText="Gender is required!" />
                </ValidationSettings>
            </dx:BootstrapComboBox>
        </div>
    </div>
</div>
<div class="row">
    <div class="col-lg-4">
        <div class="form-group">
            <dx:BootstrapComboBox ID="ddEthnicity" runat="server" Caption="Ethnicity" NullText="--Select--"
                TextField="Description" ValueField="CodeEthnicityPK" ValueType="System.Int32" 
                IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                <ValidationSettings ValidationGroup="vgChild" ErrorDisplayMode="ImageWithText">
                    <RequiredField IsRequired="true" ErrorText="Ethnicity is required!" />
                </ValidationSettings>
            </dx:BootstrapComboBox>
        </div>
    </div>
    <div class="col-lg-4">
        <div class="form-group">
            <dx:BootstrapComboBox ID="ddRace" runat="server" Caption="Race" NullText="--Select--"
                TextField="Description" ValueField="CodeRacePK" ValueType="System.Int32" 
                IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                <ValidationSettings ValidationGroup="vgChild" ErrorDisplayMode="ImageWithText">
                    <RequiredField IsRequired="true" ErrorText="Race is required!" />
                </ValidationSettings>
            </dx:BootstrapComboBox>
        </div>
    </div>
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
</div>
<div class="row">
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
            <dx:BootstrapDateEdit ID="deDischargeDate" runat="server" Caption="Discharge Date" EditFormat="Date"
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
            <dx:BootstrapComboBox ID="ddDischargeReason" runat="server" Caption="Discharge Reason" NullText="--Select--"
                TextField="Description" ValueField="CodeDischargeReasonPK" ValueType="System.Int32"
                IncrementalFilteringMode="Contains" AllowMouseWheel="false"
                OnValidation="ddDischargeReason_Validation" ClientInstanceName="ddDischargeReason"
                AllowNull="true" ClearButton-DisplayMode="Always">
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
