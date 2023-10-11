<%@ Page Title="SLT Agency" Language="C#" MasterPageFile="~/MasterPages/Dashboard.master" AutoEventWireup="true" CodeBehind="SLTAgency.aspx.cs" Inherits="Pyramid.Pages.SLTAgency" %>

<%@ Register Assembly="DevExpress.Web.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Data.Linq" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.Bootstrap.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>
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
            $('#lnkSLTDashboard').addClass('active');

            //Allow date format for DataTables sorting
            $.fn.dataTable.moment('MM/DD/YYYY');

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
    <asp:Label ID="lblPageTitle" Text="SLT Agency" CssClass="h2" runat="server"></asp:Label>
    <hr />
    <asp:HiddenField ID="hfViewOnly" runat="server" Value="False" />
    <asp:UpdatePanel ID="upSLTAgency" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <uc:Messaging ID="msgSys" runat="server" />
            <asp:HiddenField ID="hfSLTAgencyPK" runat="server" Value="" />
            <div class="row">
                <div class="col-lg-12">
                    <div class="card bg-light">
                        <div class="card-header">
                            <div class="row">
                                <div class="col-lg-8">
                                    State Leadership Team Agency
                                </div>
                                <div class="col-lg-4">
                                    <dx:BootstrapButton ID="btnPrintPreview" runat="server" Text="Save and Download/Print" OnClick="btnPrintPreview_Click" 
                                        SettingsBootstrap-RenderOption="primary" ValidationGroup="vgSLTAgency" data-validation-group="vgSLTAgency">
                                        <CssClasses Icon="fas fa-print" Control="float-right btn-loader" />
                                    </dx:BootstrapButton>
                                </div>
                            </div>
                        </div>
                        <div class="card-body">
                            <div class="row">
                                <div class="col-lg-6">
                                    <label>State: </label>
                                    <asp:Label ID="lblState" runat="server" Text=""></asp:Label>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-8">
                                    <div class="form-group">
                                        <dx:BootstrapMemo ID="txtName" runat="server" Caption="Name" MaxLength="500" Rows="1">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ValidationSettings ValidationGroup="vgSLTAgency" ErrorDisplayMode="ImageWithText">
                                                <RequiredField IsRequired="true" ErrorText="Name is required!" />
                                            </ValidationSettings>
                                        </dx:BootstrapMemo>
                                    </div>
                                </div>
                                <div class="col-lg-4">
                                    <div class="form-group">
                                        <dx:BootstrapTextBox ID="txtPhoneNumber" runat="server" Caption="Phone Number" MaxLength="40" OnValidation="txtPhoneNumber_Validation">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <MaskSettings Mask="+1 (999) 999-9999 \e\x\t\. 999999" IncludeLiterals="None" ErrorText="Must be a valid phone number!" />
                                            <ValidationSettings ValidationGroup="vgSLTAgency" ErrorDisplayMode="ImageWithText">
                                                <RequiredField IsRequired="false" ErrorText="Phone Number is required!" />
                                            </ValidationSettings>
                                        </dx:BootstrapTextBox>
                                    </div>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-8">
                                    <div class="form-group">
                                        <dx:BootstrapMemo ID="txtWebsite" runat="server" Caption="Website" MaxLength="3000" Rows="1"
                                            ClientInstanceName="txtWebsite">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ValidationSettings ValidationGroup="vgSLTAgency" ErrorDisplayMode="ImageWithText">
                                                <RequiredField IsRequired="false" ErrorText="Website is required!" />
                                            </ValidationSettings>
                                        </dx:BootstrapMemo>
                                    </div>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-12">
                                    <b>Agency Address</b>
                                </div>
                                <div class="col-lg-8">
                                    <div class="form-group">
                                        <dx:BootstrapMemo ID="txtAddressStreet" runat="server" Caption="Street Address" MaxLength="500" Rows="1">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ValidationSettings ValidationGroup="vgSLTAgency" ErrorDisplayMode="ImageWithText">
                                                <RequiredField IsRequired="false" ErrorText="Street Address is required!" />
                                            </ValidationSettings>
                                        </dx:BootstrapMemo>
                                    </div>
                                </div>
                                <div class="col-lg-4">
                                    <div class="form-group">
                                        <dx:BootstrapTextBox ID="txtAddressCity" runat="server" Caption="City" MaxLength="250">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ValidationSettings ValidationGroup="vgSLTAgency" ErrorDisplayMode="ImageWithText">
                                                <RequiredField IsRequired="false" ErrorText="City is required!" />
                                            </ValidationSettings>
                                        </dx:BootstrapTextBox>
                                    </div>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-3">
                                    <div class="form-group">
                                        <dx:BootstrapTextBox ID="txtAddressState" runat="server" Caption="State" MaxLength="2">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ValidationSettings ValidationGroup="vgSLTAgency" ErrorDisplayMode="ImageWithText">
                                                <RequiredField IsRequired="false" ErrorText="State is required!" />
                                            </ValidationSettings>
                                        </dx:BootstrapTextBox>
                                    </div>
                                </div>
                                <div class="col-lg-3">
                                    <div class="form-group">
                                        <dx:BootstrapTextBox ID="txtAddressZIPCode" runat="server" Caption="ZIP Code" MaxLength="10">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ValidationSettings ValidationGroup="vgSLTAgency" ErrorDisplayMode="ImageWithText">
                                                <RequiredField IsRequired="false" ErrorText="ZIP Code is required!" />
                                            </ValidationSettings>
                                        </dx:BootstrapTextBox>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div class="page-footer">
                <uc:Submit ID="submitSLTAgency" runat="server" ValidationGroup="vgSLTAgency"
                    ControlCssClass="center-content"
                    OnSubmitClick="submitSLTAgency_Click" OnCancelClick="submitSLTAgency_CancelClick" 
                    OnValidationFailed="submitSLTAgency_ValidationFailed" />
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
