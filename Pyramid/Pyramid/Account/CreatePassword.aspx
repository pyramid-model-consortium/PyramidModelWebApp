<%@ Page Title="Create Password" Language="C#" MasterPageFile="~/MasterPages/NotLoggedIn.master" AutoEventWireup="true" CodeBehind="CreatePassword.aspx.cs" Inherits="Pyramid.Account.CreatePassword" %>
<%@ Register TagPrefix="uc" TagName="Messaging" Src="~/User_Controls/MessagingSystem.ascx" %>
<%@ Register TagPrefix="uc" TagName="Submit" Src="~/User_Controls/Submit.ascx" %>
<%@ Register Assembly="DevExpress.Web.Bootstrap.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>

<asp:Content runat="server" ID="ScriptContent" ContentPlaceHolderID="ScriptContent">
    <script>
        //Validate the password confirmation
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
<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <asp:UpdatePanel ID="upCreatePassword" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <uc:messaging id="msgSys" runat="server" />
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
            <div class="form-horizontal">
                <div class="form-group">
                    <dx:BootstrapTextBox ID="txtUsername" runat="server" Caption="Username">
                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                        <ValidationSettings ValidationGroup="vgCreatePassword" ErrorDisplayMode="ImageWithText">
                            <RequiredField IsRequired="true" ErrorText="Username is required!" />
                        </ValidationSettings>
                    </dx:BootstrapTextBox>
                </div>
                <div class="form-group">
                    <dx:BootstrapTextBox ID="txtPassword" ClientInstanceName="txtPassword" Password="true" runat="server" Caption="New Password">
                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                        <ValidationSettings ValidationGroup="vgCreatePassword" ErrorDisplayMode="ImageWithText">
                            <RequiredField IsRequired="true" ErrorText="New Password is required!" />
                        </ValidationSettings>
                    </dx:BootstrapTextBox>
                </div>
                <div class="form-group">
                    <dx:BootstrapTextBox ID="txtConfirmPassword" runat="server" Password="true" Caption="Confirm New Password" EnableClientSideAPI="true" OnValidation="txtConfirmPassword_Validation">
                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                        <ClientSideEvents Validation="validateConfirmPassword" />
                        <ValidationSettings ValidationGroup="vgCreatePassword" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                            <RequiredField IsRequired="true" ErrorText="Confirm New Password is required!" />
                        </ValidationSettings>
                    </dx:BootstrapTextBox>
                </div>
                <div class="form-group">
                    <!-- Just adding a class to the submit removes the centering -->
                    <uc:Submit ID="submitCreatePassword" runat="server" 
                        ShowCancelButton="true" SubmitButtonText="Save Changes" 
                        ValidationGroup="vgCreatePassword"
                        OnSubmitClick="submitCreatePassword_Click"
                        OnCancelClick="submitCreatePassword_CancelClick"
                        OnValidationFailed="submitCreatePassword_ValidationFailed" />
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
