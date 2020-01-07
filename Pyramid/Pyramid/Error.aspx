<%@ Page Title="An Error Occurred" Language="C#" MasterPageFile="~/MasterPages/NotLoggedIn.Master" AutoEventWireup="true" CodeBehind="Error.aspx.cs" Inherits="Pyramid.Error" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row">
        <div class="col-md-12">
            <div class="alert alert-warning" style="font-size: large;">
                <p>You have encountered an error in the Pyramid application. </p>
                <p>To help us correct this problem and keep you updated on the status of this error, please submit a support ticket with the following information:</p>
                <ul>
                    <li>What you were doing when the error occurred</li>
                    <li>Your role in the application</li>
                    <li>Any additional details you can supply</li>
                </ul>
                <p><strong>Thank you!</strong></p>
                <div class="d-flex justify-content-center">
                    <a href="/SupportTicket.aspx" class="btn btn-loader btn-secondary w-100">
                        <i class="fas fa-headset"></i>
                        Submit a Support Ticket
                    </a>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
