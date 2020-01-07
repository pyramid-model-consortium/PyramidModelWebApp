<%@ Page Title="Create User" Language="C#" MasterPageFile="~/MasterPages/LoggedIn.Master" AutoEventWireup="true" CodeBehind="CreateNewUser.aspx.cs" Inherits="Pyramid.Admin.CreateNewUser" %>

<%@ Register TagPrefix="uc" TagName="Messaging" Src="~/User_Controls/MessagingSystem.ascx" %>
<%@ Register TagPrefix="uc" TagName="Submit" Src="~/User_Controls/Submit.ascx" %>
<%@ Register Assembly="DevExpress.Web.v19.1, Version=19.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Data.Linq" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.Bootstrap.v19.1, Version=19.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ContentPlaceHolderID="ScriptContent" runat="server">
    <script>
        function validateConfirmPassword(s, e) {
            var confirmedPassword = e.value;
            var password = txtPassword.GetText();

            if (confirmedPassword != password) {
                e.isValid = false;
                e.errorText = "Password confirmation does not match!";
            }
            else {
                e.isValid = true;
            }
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <uc:Messaging ID="msgSys" runat="server" />
    <div class="card bg-light">
        <div class="card-header">User Information</div>
        <div class="card-body">
            <div class="row">
                <div class="col-md-6">
                    <div class="form-group">
                        <dx:BootstrapTextBox ID="txtUsername" runat="server" Caption="Username">
                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                            <ValidationSettings ValidationGroup="vgCreateUser" ErrorDisplayMode="ImageWithText">
                                <RequiredField IsRequired="true" ErrorText="Username is required!" />
                            </ValidationSettings>
                        </dx:BootstrapTextBox>
                    </div>
                </div>
                <div class="col-md-6">
                    <div class="form-group">
                        <div class="form-group">
                            <dx:BootstrapComboBox ID="ddIdentityRole" runat="server" Caption="Identity Role" NullText="--Select--"
                                TextField="Name" ValueField="Id" ValueType="System.String" 
                                IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                <ValidationSettings ValidationGroup="vgCreateUser" ErrorDisplayMode="ImageWithText">
                                    <RequiredField IsRequired="true" ErrorText="Identity Role is required!" />
                                </ValidationSettings>
                            </dx:BootstrapComboBox>
                        </div>
                    </div>
                </div>
            </div>
            <div class="row">
                <div class="col-md-6">
                    <div class="form-group">
                        <dx:BootstrapTextBox ID="txtFirstName" runat="server" Caption="First Name">
                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                            <ValidationSettings ValidationGroup="vgCreateUser" ErrorDisplayMode="ImageWithText">
                                <RequiredField IsRequired="true" ErrorText="First Name is required!" />
                            </ValidationSettings>
                        </dx:BootstrapTextBox>
                    </div>
                </div>
                <div class="col-md-6">
                    <div class="form-group">
                        <dx:BootstrapTextBox ID="txtLastName" runat="server" Caption="Last Name">
                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                            <ValidationSettings ValidationGroup="vgCreateUser" ErrorDisplayMode="ImageWithText">
                                <RequiredField IsRequired="true" ErrorText="Last Name is required!" />
                            </ValidationSettings>
                        </dx:BootstrapTextBox>
                    </div>
                </div>
            </div>
            <div class="row">
                <div class="col-md-6">
                    <div class="form-group">
                        <dx:BootstrapTextBox ID="txtEmail" runat="server" Caption="Email">
                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                            <ValidationSettings ValidationGroup="vgCreateUser" ErrorDisplayMode="ImageWithText">
                                <RequiredField IsRequired="true" ErrorText="Email is required!" />
                                <RegularExpression ErrorText="Invalid Email!" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" />
                            </ValidationSettings>
                        </dx:BootstrapTextBox>
                    </div>
                </div>
                <div class="col-md-6">
                    <div class="form-group">
                        <dx:BootstrapTextBox ID="txtPhoneNumber" runat="server" Caption="Phone Number">
                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                            <MaskSettings Mask="+1 (999) 999-9999" IncludeLiterals="None" ErrorText="Must be a valid phone number!" />
                            <ValidationSettings ValidationGroup="vgCreateUser" ErrorDisplayMode="ImageWithText">
                                <RequiredField IsRequired="false" ErrorText="Phone Number is required!" />
                            </ValidationSettings>
                        </dx:BootstrapTextBox>
                    </div>
                </div>
            </div>
            <div class="row">
                <div class="col-md-6">
                    <div class="form-group">
                        <dx:BootstrapComboBox ID="ddProgram" runat="server" Caption="Program (Hub)" NullText="--Select--"
                            TextField="ProgramName" ValueField="ProgramPK" ValueType="System.Int32" 
                            IncrementalFilteringMode="Contains" AllowMouseWheel="false">
                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                            <ValidationSettings ValidationGroup="vgCreateUser" ErrorDisplayMode="ImageWithText">
                                <RequiredField IsRequired="true" ErrorText="Program is required!" />
                            </ValidationSettings>
                        </dx:BootstrapComboBox>
                    </div>
                </div>
                <div class="col-md-6">
                    <div class="form-group">
                        <dx:BootstrapComboBox ID="ddProgramRole" runat="server" Caption="Program Role" NullText="--Select--"
                            TextField="RoleName" ValueField="CodeProgramRolePK" ValueType="System.Int32" 
                            IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                            <ValidationSettings ValidationGroup="vgCreateUser" ErrorDisplayMode="ImageWithText">
                                <RequiredField IsRequired="true" ErrorText="Program Role is required!" />
                            </ValidationSettings>
                        </dx:BootstrapComboBox>
                    </div>
                </div>
            </div>
            <div class="row">
                <div class="col-md-6">
                    <div class="form-group">
                        <dx:BootstrapTextBox ID="txtPassword" ClientInstanceName="txtPassword" Password="true" runat="server" Caption="Password">
                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                            <ValidationSettings ValidationGroup="vgCreateUser" ErrorDisplayMode="ImageWithText">
                                <RequiredField IsRequired="true" ErrorText="Password is required!" />
                            </ValidationSettings>
                        </dx:BootstrapTextBox>
                    </div>
                </div>
                <div class="col-md-6">
                    <div class="form-group">
                        <dx:BootstrapTextBox ID="txtConfirmPassword" runat="server" Password="true" Caption="Confirm Password" EnableClientSideAPI="true" OnValidation="txtConfirmPassword_Validation">
                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                            <ClientSideEvents Validation="validateConfirmPassword" />
                            <ValidationSettings ValidationGroup="vgCreateUser" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                <RequiredField IsRequired="true" ErrorText="Confirm Password is required!" />
                            </ValidationSettings>
                        </dx:BootstrapTextBox>
                    </div>
                </div>
            </div>
            <div class="row">
                <div class="col-md-12">
                    <div class="alert alert-primary mt-2">
                        <i class="fas fa-info-circle"></i>&nbsp;Password Requirements:
                        <ul>
                            <li>Must be at least 8 characters in length.</li>
                            <li>Must contain at least 1 uppercase letter.</li>
                            <li>Must contain at least 1 lowercase letter.</li>
                            <li>Must contain at least 1 number.</li>
                            <li>Must contain at least 1 special symbol (e.g. !?@&).</li>
                        </ul>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <div class="page-footer">
        <uc:Submit ID="submitUser" runat="server" ValidationGroup="vgCreateUser" OnSubmitClick="submitUser_Click" OnCancelClick="submitUser_CancelClick" OnValidationFailed="submitUser_ValidationFailed" />
    </div>
</asp:Content>
