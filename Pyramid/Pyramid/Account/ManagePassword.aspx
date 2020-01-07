<%@ Page Title="Change Password" Language="C#" MasterPageFile="~/MasterPages/LoggedIn.Master" AutoEventWireup="true" CodeBehind="ManagePassword.aspx.cs" Inherits="Pyramid.Account.ManagePassword" %>

<%@ Register TagPrefix="uc" TagName="Messaging" Src="~/User_Controls/MessagingSystem.ascx" %>
<%@ Register Assembly="DevExpress.Web.Bootstrap.v19.1, Version=19.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>

<asp:Content ID="ScriptContent" ContentPlaceHolderID="ScriptContent" runat="server">
    <script>
        //Validate the password confirmation
        function validateConfirmPassword(s, e) {
            var confirmedPassword = e.value;
            var password = txtNewPassword.GetText();

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
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <asp:UpdatePanel ID="upChangePassword" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <uc:Messaging ID="msgSys" runat="server" />
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
            <div class="form-group">
                <div class="col-md-8">
                    <dx:BootstrapTextBox ID="txtCurrentPassword" Password="true" runat="server" Caption="Current Password">
                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                        <ValidationSettings ValidationGroup="vgChangePassword" ErrorDisplayMode="ImageWithText">
                            <RequiredField IsRequired="true" ErrorText="Current Password is required!" />
                        </ValidationSettings>
                    </dx:BootstrapTextBox>
                </div>
            </div>
            <div class="form-group">
                <div class="col-md-8">
                    <dx:BootstrapTextBox ID="txtNewPassword" Password="true" ClientInstanceName="txtNewPassword" runat="server" Caption="New Password">
                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                        <ValidationSettings ValidationGroup="vgChangePassword" ErrorDisplayMode="ImageWithText">
                            <RequiredField IsRequired="true" ErrorText="New Password is required!" />
                        </ValidationSettings>
                    </dx:BootstrapTextBox>
                </div>
            </div>
            <div class="form-group">
                <div class="col-md-8">
                    <dx:BootstrapTextBox ID="txtConfirmNewPassword" runat="server" Password="true" Caption="Confirm New Password" EnableClientSideAPI="true" OnValidation="txtConfirmPassword_Validation">
                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                        <ClientSideEvents Validation="validateConfirmPassword" />
                        <ValidationSettings ValidationGroup="vgChangePassword" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                            <RequiredField IsRequired="true" ErrorText="Confirm New Password is required!" />
                        </ValidationSettings>
                    </dx:BootstrapTextBox>
                </div>
            </div>
            <div class="form-group">
                <div class="col-md-offset-2 col-md-10">
                    <a href="/Account/Manage.aspx" class="btn btn-loader btn-secondary"><i class="fas fa-times"></i>&nbsp;Cancel</a>
                    <dx:BootstrapButton ID="btnChangePassword" runat="server" Text="Save Changes" OnClick="btnChangePassword_Click" ValidationGroup="vgChangePassword" SettingsBootstrap-RenderOption="Success">
                        <CssClasses Icon="fas fa-save" Control="btn-loader" />
                    </dx:BootstrapButton>
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
