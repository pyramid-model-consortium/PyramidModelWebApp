<%@ Page Title="Report Designer" Language="C#" MasterPageFile="~/MasterPages/LoggedIn.master" AutoEventWireup="true" CodeBehind="ReportDesigner.aspx.cs" Inherits="Pyramid.Reports.UserReports.ReportDesigner" %>
<%@ Register Assembly="DevExpress.XtraReports.v19.1.Web.WebForms, Version=19.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.XtraReports.Web" TagPrefix="dx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ScriptContent" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row">
        <div class="col-md-12">
            <dx:ASPxReportDesigner ID="ASPxReportDesigner1" runat="server"></dx:ASPxReportDesigner>
        </div>
    </div>
</asp:Content>
