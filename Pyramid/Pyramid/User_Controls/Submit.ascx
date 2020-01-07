<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Submit.ascx.cs" Inherits="Pyramid.User_Controls.Submit" %>
<%@ Register Assembly="DevExpress.Web.Bootstrap.v19.1, Version=19.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>

<asp:Panel ID="pnlSubmitControl" runat="server" DefaultButton="btnSubmit" CssClass="submit-control center-content">
    <asp:HiddenField ID="hfValidationGroup" runat="server" Value="" />
    <asp:LinkButton ID="lbCancel" runat="server" CssClass="btn btn-loader btn-secondary mr-2" OnClick="lbCancel_Click"><i class="fas fa-times"></i>&nbsp;Cancel</asp:LinkButton>
    <dx:BootstrapButton ID="btnSubmit" runat="server" Text="Save" OnClick="btnSubmit_Click" SettingsBootstrap-RenderOption="success">
        <CssClasses Icon="fas fa-save" Control="button-submit" />
    </dx:BootstrapButton>
    <asp:LinkButton ID="lbSubmitting" runat="server" CssClass="btn btn-success" style="display: none">
    </asp:LinkButton>
</asp:Panel>