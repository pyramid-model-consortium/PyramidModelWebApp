<%@ Page Title="Program Address" Language="C#" MasterPageFile="~/MasterPages/Dashboard.master" AutoEventWireup="true" CodeBehind="ProgramAddress.aspx.cs" Inherits="Pyramid.Pages.ProgramAddress" %>

<%@ Register Assembly="DevExpress.Web.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Data.Linq" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.Bootstrap.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>
<%@ Register TagPrefix="uc" TagName="Submit" Src="~/User_Controls/Submit.ascx" %>
<%@ Register TagPrefix="uc" TagName="Messaging" Src="~/User_Controls/MessagingSystem.ascx" %>

<%@ Register TagPrefix="dx" Namespace="DevExpress.Web" Assembly="DevExpress.Web.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ScriptContent" runat="server">
    <script type="text/javascript">
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
            $('#lnkPLTDashboard').addClass('active');

            
            //Show/hide the view only fields
            setViewOnlyVisibility();

           
        }

        function setViewOnlyVisibility() {
            //Hide controls if this is a view
            var isView = $('[ID$="hfViewOnly"]').val();
            if (isView == 'True') {
                $('.hide-on-view').addClass('hidden');
            }
            else {
                $('.hide-on-view').removeClass('hidden');
            }
        }
    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Label ID="lblPageTitle" runat="server" Text="Program Address" CssClass="h2"></asp:Label>
    <hr />
    <asp:HiddenField ID="hfViewOnly" runat="server" Value="False" />
    <asp:UpdatePanel ID="upMessaging" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <uc:Messaging ID="msgSys" runat="server" />
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="btnPrintPreview" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="submitProgramAddress" />
        </Triggers>
    </asp:UpdatePanel>
    <asp:UpdatePanel ID="upBasicInfo" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <ContentTemplate>
            <asp:HiddenField ID="hfProgramAddressPK" runat="server" Value="" />
            <div class="row">
                <div class="col-lg-12">
                    <div class="card bg-light">
                        <div class="card-header">
                            <div class="row">
                                <div class="col-lg-8">
                                    Basic Information
                                </div>
                                <div class="col-lg-4">
                                    <dx:BootstrapButton ID="btnPrintPreview" runat="server" Text="Save and Download/Print" OnClick="btnPrintPreview_Click"
                                        SettingsBootstrap-RenderOption="primary" ValidationGroup="vgProgramAddress" data-validation-group="vgProgramAddress">
                                        <CssClasses Icon="fas fa-print" Control="float-right btn-loader" />
                                    </dx:BootstrapButton>
                                </div>
                            </div>
                        </div>
                        <div class="card-body">
                            <div class="row">
                                <div class="col-md-6">
                                    <label>Program: </label>
                                    <asp:Label ID="lblProgram" runat="server" Text=""></asp:Label>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-8">
                                    <div class="form-group">
                                        <dx:BootstrapMemo ID="txtStreet" runat="server" Caption="Street Address" MaxLength="300" rows="1">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ValidationSettings ValidationGroup="vgProgramAddress" ErrorDisplayMode="ImageWithText">
                                                <RequiredField IsRequired="true" ErrorText="Street is required!" />
                                            </ValidationSettings>
                                        </dx:BootstrapMemo>
                                    </div>
                                </div>
                                <div class="col-lg-4">
                                    <div class="form-group">
                                        <dx:BootstrapTextBox ID="txtCity" runat="server" Caption="City" MaxLength="100">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ValidationSettings ValidationGroup="vgProgramAddress" ErrorDisplayMode="ImageWithText">
                                                <RequiredField IsRequired="true" ErrorText="City is required!" />
                                            </ValidationSettings>
                                        </dx:BootstrapTextBox>
                                    </div>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-4">
                                    <div class="form-group">
                                        <dx:BootstrapComboBox ID="ddState" runat="server" Caption="State" NullText="--Select--" AllowNull="true"
                                            TextField="Name" ValueField="Name" ValueType="System.String">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ValidationSettings ValidationGroup="vgProgramAddress" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                <RequiredField IsRequired="true" ErrorText="State is required!" />
                                            </ValidationSettings>
                                        </dx:BootstrapComboBox>
                                    </div>
                                </div>
                                <div class="col-lg-4">
                                    <div class="form-group">
                                        <dx:BootstrapTextBox ID="txtZIPCode" runat="server" Caption="ZIP Code" MaxLength="50">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ValidationSettings ValidationGroup="vgProgramAddress" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                <RequiredField IsRequired="true" ErrorText="ZIP Code is required!" />
                                            </ValidationSettings>
                                        </dx:BootstrapTextBox>
                                    </div>
                                </div>
                                <div class="col-lg-4">
                                    <div class="form-group">
                                        <dx:BootstrapTextBox ID="txtLicenseNumber" runat="server" Caption="License Number" MaxLength="50">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ValidationSettings ValidationGroup="vgProgramAddress" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                <RequiredField IsRequired="false" ErrorText="License Number is required!" />
                                            </ValidationSettings>
                                        </dx:BootstrapTextBox>
                                    </div>
                                </div>
                                <div class="col-lg-4">
                                    <div class="form-group">
                                        <dx:BootstrapComboBox ID="ddMailing" runat="server" Caption="Is this the Program’s Mailing Address?" NullText="--Select--"
                                            IncrementalFilteringMode="StartsWith" AllowMouseWheel="false" ValueType="System.Boolean">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ValidationSettings ValidationGroup="vgProgramAddress" ErrorDisplayMode="ImageWithText">
                                                <RequiredField IsRequired="true" ErrorText="Required!" />
                                            </ValidationSettings>
                                            <Items>
                                                <dx:BootstrapListEditItem Value="True" Text="Yes"></dx:BootstrapListEditItem>
                                                <dx:BootstrapListEditItem Value="False" Text="No"></dx:BootstrapListEditItem>
                                            </Items>
                                        </dx:BootstrapComboBox>
                                    </div>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-12">
                                    <dx:BootstrapMemo ID="txtNote" runat="server" Caption="Notes" Rows="3" MaxLength="5000">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgProgramAddress" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="false" />
                                        </ValidationSettings>
                                    </dx:BootstrapMemo>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="submitProgramAddress" />
        </Triggers>
    </asp:UpdatePanel>
    <div class="page-footer">
        <uc:Submit ID="submitProgramAddress" runat="server" ValidationGroup="vgProgramAddress"
            ControlCssClass="center-content"
            OnSubmitClick="submitProgramAddress_Click" OnCancelClick="submitProgramAddress_CancelClick"
            OnValidationFailed="submitProgramAddress_ValidationFailed" />
    </div>
</asp:Content>
