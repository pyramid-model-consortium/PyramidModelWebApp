<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Classroom.ascx.cs" Inherits="Pyramid.User_Controls.Classroom" %>
<%@ Register Assembly="DevExpress.Web.Bootstrap.v19.1, Version=19.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>

<script>
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
</script>

<asp:HiddenField ID="hfClassroomPK" runat="server" Value="0" />
<asp:HiddenField ID="hfProgramFK" runat="server" Value="0" />
<asp:HiddenField ID="hfUsedIDs" runat="server" Value="" />
<div class="row">
    <div class="col-lg-4">
        <div class="form-group">
            <dx:BootstrapTextBox ID="txtName" runat="server" Caption="Classroom Name">
                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                <ValidationSettings ValidationGroup="vgClassroom" ErrorDisplayMode="ImageWithText">
                    <RequiredField IsRequired="true" ErrorText="Classroom Name is required!" />
                </ValidationSettings>
            </dx:BootstrapTextBox>
        </div>
    </div>
    <div class="col-lg-4">
        <div class="form-group">
            <dx:BootstrapTextBox ID="txtProgramID" runat="server" Caption="ID Number" MaxLength="100"
                OnValidation="txtProgramID_Validation">
                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                <ClientSideEvents Validation="validateProgramID" />
                <ValidationSettings ValidationGroup="vgClassroom" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                    <RequiredField IsRequired="true" ErrorText="ID Number is required!" />
                </ValidationSettings>
            </dx:BootstrapTextBox>
        </div>
    </div>
    <div class="col-lg-4">
        <div class="form-group">
            <dx:BootstrapTextBox ID="txtLocation" runat="server" Caption="Location">
                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                <ValidationSettings ValidationGroup="vgClassroom" ErrorDisplayMode="ImageWithText">
                    <RequiredField IsRequired="false" ErrorText="Location is required!" />
                </ValidationSettings>
            </dx:BootstrapTextBox>
        </div>
    </div>
</div>
<div class="row">
    <div class="col-lg-4">
        <div class="form-group">
            <dx:BootstrapComboBox ID="ddInfantToddler" runat="server" Caption="Infant/Toddler Classroom?" NullText="--Select--"
                IncrementalFilteringMode="StartsWith" AllowMouseWheel="false" ValueType="System.Boolean">
                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                <ValidationSettings ValidationGroup="vgClassroom" ErrorDisplayMode="ImageWithText">
                    <RequiredField IsRequired="true" ErrorText="Infant/Toddler Classroom is required!" />
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
            <dx:BootstrapComboBox ID="ddPreschool" runat="server" Caption="Preschool Classroom?" NullText="--Select--"
                IncrementalFilteringMode="StartsWith" AllowMouseWheel="false" ValueType="System.Boolean">
                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                <ValidationSettings ValidationGroup="vgClassroom" ErrorDisplayMode="ImageWithText">
                    <RequiredField IsRequired="true" ErrorText="Preschool Classroom is required!" />
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
            <dx:BootstrapComboBox ID="ddServedSubstitute" runat="server" Caption="Currently Served by Substitute?" NullText="--Select--"
                IncrementalFilteringMode="StartsWith" AllowMouseWheel="false" ValueType="System.Boolean">
                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                <ValidationSettings ValidationGroup="vgClassroom" ErrorDisplayMode="ImageWithText">
                    <RequiredField IsRequired="true" ErrorText="Currently Served by Substitute is required!" />
                </ValidationSettings>
                <Items>
                    <dx:BootstrapListEditItem Value="True" Text="Yes"></dx:BootstrapListEditItem>
                    <dx:BootstrapListEditItem Value="False" Text="No"></dx:BootstrapListEditItem>
                </Items>
            </dx:BootstrapComboBox>
        </div>
    </div>
</div>