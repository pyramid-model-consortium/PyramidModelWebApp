<%@ Page Title="User Agreement" Language="C#" MasterPageFile="~/MasterPages/NotLoggedIn.master" AutoEventWireup="true" CodeBehind="Confidentiality.aspx.cs" Inherits="Pyramid.Pages.Confidentiality" %>

<%@ Register Assembly="DevExpress.Web.Bootstrap.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>
<%@ Register TagPrefix="uc" TagName="Submit" Src="~/User_Controls/Submit.ascx" %>
<%@ Register TagPrefix="uc" TagName="Messaging" Src="~/User_Controls/MessagingSystem.ascx" %>
<%@ Register TagPrefix="uc" TagName="AutoLogoff" Src="~/User_Controls/AutoLogoff.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ScriptContent" runat="server">
    <script type="text/javascript">
        //Initialize the confirm checkbox
        function initializeConfirmCheckbox(s, e) {
            //Record when the user clicks on the view document link
            $('[ID$="lnkViewDocument"]').on('click', function () {
                //Enable the confirm checkbox
                cbConfirm.SetEnabled(true);

                //Record that the user looked at the document
                $('[ID$="hfClickedLink"]').val('true');
            });

            //Check whether the user clicked the view document link
            var clickedLink = $('[ID$="hfClickedLink"]').val();

            //Enable/disable the confirm checkbox
            if (clickedLink === 'true') {
                cbConfirm.SetEnabled(true);
            }
            else {
                cbConfirm.SetEnabled(false);
            }
        }
    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <uc:AutoLogoff ID="autoLogoff" runat="server" />
    <asp:HiddenField ID="hfClickedLink" runat="server" Value="false" />
    <asp:UpdatePanel ID="upConfidentiality" runat="server" UpdateMode="Always">
        <ContentTemplate>
            <uc:Messaging ID="msgSys" runat="server" />
            <div id="divAgreementError" runat="server" class="row mb-2" visible="false">
                <div class="col-lg-12">
                    <div class="alert alert-danger">
                        <i class="fas fa-exclamation-triangle"></i>&nbsp;
                        <asp:Label ID="lblAgreementError" runat="server" Text=""></asp:Label>
                        <div class="center-content">
                            <a href="/SupportTicket.aspx" target="_blank" class="btn btn-link">
                                <i class="fas fa-headset"></i>
                                Submit a Support Ticket
                            </a>
                        </div>
                    </div>
                </div>
            </div>
            <div id="divConfidentialityAgreement" runat="server">
                <div class="row">
                    <div class="col-lg-12">
                        <div class="alert alert-primary">
                            <i class="fas fa-info-circle"></i>&nbsp;
                            <asp:Label ID="lblAgreementNotification" runat="server" Text=""></asp:Label>
                        </div>
                    </div>
                </div>
                <div class="row mb-2">
                    <div class="col-lg-3">
                        <asp:Label ID="lblUsernameLabel" runat="server" AssociatedControlID="lblUsername"></asp:Label>
                        <asp:Label ID="lblUsername" runat="server"></asp:Label>
                    </div>
                    <div class="col-lg-6">
                        <asp:Label ID="lblCurrentDateLabel" runat="server" AssociatedControlID="lblCurrentDate" Text="Current Date:"></asp:Label>
                        <asp:Label ID="lblCurrentDate" runat="server"></asp:Label>
                    </div>
                </div>
                <div class="row mb-2">
                    <div class="col-lg-12">
                        <dx:BootstrapHyperLink ID="lnkViewDocument" runat="server" Target="_blank" NavigateUrl="/Pages/ViewFile.aspx" Text="User Agreement" CssClasses-Control="btn btn-secondary" CssClasses-Icon="fas fa-file-contract"></dx:BootstrapHyperLink>
                    </div>
                </div>
                <div class="row mb-2">
                    <div class="col-lg-12">
                        <dx:BootstrapCheckBox ID="cbConfirm" runat="server" ClientEnabled="false" ClientInstanceName="cbConfirm" 
                            OnCheckedChanged="cbConfirm_CheckedChanged"
                            Text="I certify that I have read the above document and agree to use PIDS according to the terms of this Agreement" 
                            AutoPostBack="true">
                            <ClientSideEvents Init="initializeConfirmCheckbox" />
                            <ValidationSettings ValidationGroup="vgConfidentiality" ErrorDisplayMode="ImageWithText">
                                <RequiredField IsRequired="true" ErrorText="To accept the agreement you must read the document, check this box, and then click the 'Accept' button below." />
                            </ValidationSettings>
                        </dx:BootstrapCheckBox>
                    </div>
                </div>
                <div class="row">
                    <div class="col-lg-12 form-inline">
                        <uc:Submit ID="submitConfidentiality" runat="server" 
                            ValidationGroup="vgConfidentiality"
                            SubmitButtonIcon="fas fa-check" CancelButtonIcon="fas fa-times"
                            SubmitButtonText="Accept" CancelButtonText="Decline"
                            SubmitButtonBootstrapType="primary" CancelButtonBootstrapType="warning"
                            SubmittingButtonText="Processing..."
                            EnableSubmitButton="false"
                            OnCancelClick="submitConfidentiality_CancelClick"
                            OnSubmitClick="submitConfidentiality_Click"
                            OnValidationFailed="submitConfidentiality_ValidationFailed">
                        </uc:Submit>
                    </div>
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
