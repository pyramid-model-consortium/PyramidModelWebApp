<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Classroom.ascx.cs" Inherits="Pyramid.User_Controls.Classroom" %>
<%@ Register Assembly="DevExpress.Web.Bootstrap.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>

<asp:HiddenField ID="hfClassroomPK" runat="server" Value="0" />
<asp:HiddenField ID="hfProgramFK" runat="server" Value="0" />
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
                <ValidationSettings ValidationGroup="vgClassroom" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                    <RequiredField IsRequired="false" ErrorText="ID Number is required!" />
                </ValidationSettings>
            </dx:BootstrapTextBox>
            <button type="button" class="btn btn-link p-0" data-toggle="popover" data-trigger="hover focus" title="Help" data-content="This field is not required, but will be automatically populated in the format of CLID-[number] if you do not enter an ID number.  You can either leave the system-generated ID number there or create your own ID number."><i class="fas fa-question-circle"></i>&nbsp;Help</button>
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