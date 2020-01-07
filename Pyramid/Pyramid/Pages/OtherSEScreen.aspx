<%@ Page Title="Other Social Emotional Screening" Language="C#" MasterPageFile="~/MasterPages/Dashboard.master" AutoEventWireup="true" CodeBehind="OtherSEScreen.aspx.cs" Inherits="Pyramid.Pages.OtherSEScreen" %>

<%@ Register Assembly="DevExpress.Web.v19.1, Version=19.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Data.Linq" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.Bootstrap.v19.1, Version=19.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.v19.1, Version=19.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>
<%@ Register TagPrefix="uc" TagName="Messaging" Src="~/User_Controls/MessagingSystem.ascx" %>
<%@ Register TagPrefix="uc" TagName="Submit" Src="~/User_Controls/Submit.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ScriptContent" runat="server">
    <script>
        $(document).ready(function () {
            //Run the initial page init
            initializePage();

            //Run the page init after every AJAX load
            var requestManager = Sys.WebForms.PageRequestManager.getInstance();
            requestManager.add_endRequest(initializePage);
        });

        //Initializes the page
        function initializePage() {
            //Highlight the correct dashboard link
            $('#lnkOtherSEScreenDashboard').addClass('active');
            
            //Show/hide the view only fields
            setViewOnlyVisibility();
        }
    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Label ID="lblPageTitle" Text="OtherSEScreen" CssClass="h2" runat="server"></asp:Label>
    <hr />
    <asp:HiddenField ID="hfViewOnly" runat="server" Value="False" />
    <asp:UpdatePanel ID="upMessaging" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <uc:Messaging ID="msgSys" runat="server" />
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="submitOtherSEScreen" />
        </Triggers>
    </asp:UpdatePanel>
    <asp:UpdatePanel ID="upOtherSEScreen" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <div class="row">
                <div class="col-lg-12">
                    <div class="card bg-light">
                        <div class="card-header">
                            Other Social Emotional Screening
                        </div>
                        <div class="card-body">
                            <div class="row">
                                <div class="col-md-6">
                                    <label>Program: </label>
                                    <asp:Label ID="lblProgram" runat="server" Text=""></asp:Label>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-4">
                                    <dx:BootstrapDateEdit ID="deScreenDate" runat="server" Caption="Screen Date" EditFormat="Date"
                                        EditFormatString="MM/dd/yyyy" UseMaskBehavior="true" NullText="--Select--"
                                        OnValueChanged="deScreenDate_ValueChanged" AutoPostBack="true" 
                                        AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                        <ValidationSettings ValidationGroup="vgOtherSEScreen" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Screen Date is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapDateEdit>
                                </div>
                                <div class="col-md-4">
                                    <dx:BootstrapComboBox ID="ddScreenType" runat="server" Caption="Screen Type" NullText="--Select--"
                                        TextField="Description" ValueField="CodeScreenTypePK" ValueType="System.Int32" 
                                        IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgOtherSEScreen" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Screen Type is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </div>
                                <div class="col-md-4">
                                    <dx:BootstrapComboBox ID="ddChild" runat="server" Caption="Child" NullText="--Select--"
                                        TextField="IdAndName" ValueField="ChildPK" ValueType="System.Int32"
                                        IncrementalFilteringMode="Contains" AllowMouseWheel="false"
                                        OnSelectedIndexChanged="ddChild_SelectedIndexChanged" AutoPostBack="true">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgOtherSEScreen" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Child is required!  If this is not enabled, the child was not active as of the incident date." />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                    <asp:Label ID="lblAge" runat="server" Text="Label" Visible="false"></asp:Label>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-4">
                                    <dx:BootstrapTextBox ID="txtScore" runat="server" Caption="Score">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <MaskSettings Mask="0999" PromptChar=" " ErrorText="Score must be a valid number!" />
                                        <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgOtherSEScreen">
                                            <RequiredField IsRequired="true" ErrorText="Score is required" />
                                        </ValidationSettings>
                                    </dx:BootstrapTextBox>
                                </div>
                                <div class="col-md-4">
                                    <dx:BootstrapComboBox ID="ddScoreType" runat="server" Caption="Score Type" NullText="--Select--"
                                        TextField="Description" ValueField="CodeScoreTypePK" ValueType="System.Int32" 
                                        IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgOtherSEScreen" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Score Type is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </div>
                                <div class="col-md-4">
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div class="page-footer">
                <uc:Submit ID="submitOtherSEScreen" runat="server" ValidationGroup="vgOtherSEScreen" OnSubmitClick="submitOtherSEScreen_Click" OnCancelClick="submitOtherSEScreen_CancelClick" OnValidationFailed="submitOtherSEScreen_ValidationFailed" />
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
