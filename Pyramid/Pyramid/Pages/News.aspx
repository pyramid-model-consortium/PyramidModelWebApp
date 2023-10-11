<%@ Page Title="News" Language="C#" MasterPageFile="~/MasterPages/LoggedIn.master" AutoEventWireup="true" CodeBehind="News.aspx.cs" Inherits="Pyramid.Pages.News" %>

<%@ Register Assembly="DevExpress.Web.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Data.Linq" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.Bootstrap.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>
<%@ Register TagPrefix="uc" TagName="Messaging" Src="~/User_Controls/MessagingSystem.ascx" %>

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
            setTimeout(function () {
                //Hide the loading div
                $('#divLoading').slideUp(600);
            }, 100);

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

        //This function shows the loading div
        function showLoadingDiv() {
            setTimeout(function () {
                $('#divLoading').slideDown(400);
            }, 100);
        }
    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <asp:HiddenField ID="hfViewOnly" runat="server" Value="False" />
    <asp:UpdatePanel ID="upDashboardMessaging" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <uc:Messaging ID="msgSys" runat="server" />
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="lbDeleteNews" />
        </Triggers>
    </asp:UpdatePanel>
    <asp:HiddenField ID="hfDeleteNewsPK" runat="server" Value="0" />
    <div class="row">
        <div class="col-lg-12">
            <div class="card bg-light">
                <div class="card-header">
                    All News
                    <a href="/Pages/NewsManagement.aspx?NewsEntryPK=0&action=Add" class="btn btn-loader btn-primary float-right hide-on-view hidden"><i class="fas fa-plus"></i>&nbsp;Add New News Entry</a>
                </div>
                <div class="card-body">
                    <asp:UpdatePanel runat="server" ID="upAllNews">
                        <ContentTemplate>
                            <div class="row">
                                <div class="col-lg-4">
                                    <dx:BootstrapDateEdit ID="deLimitDate" runat="server" Caption="View News After" EditFormat="Date"
                                        EditFormatString="MM/dd/yyyy" UseMaskBehavior="true" NullText="--Select--" 
                                        OnValueChanged="FilterNews" AutoPostBack="true" 
                                        AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                        <ClientSideEvents ValueChanged="showLoadingDiv" />
                                        <ValidationSettings ValidationGroup="vgFilterNews" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapDateEdit>
                                </div>
                                <div class="col-lg-4">
                                    <dx:BootstrapComboBox ID="ddEntryType" runat="server" Caption="Filter by Type" NullText="--Select--"
                                        TextField="Description" ValueField="CodeNewsEntryTypePK" ValueType="System.Int32"
                                        IncrementalFilteringMode="Contains" AllowMouseWheel="false"
                                        OnValueChanged="FilterNews" AutoPostBack="true">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ClientSideEvents ValueChanged="showLoadingDiv" />
                                        <ValidationSettings ValidationGroup="vgFilterNews" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Type is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </div>
                                <div class="col-lg-4">
                                    <br />
                                    <div id="divLoading" class="alert alert-primary" style="display: none;">
                                        <span class="spinner-border"></span>&nbsp;Loading...
                                    </div>
                                </div>
                            </div>
                            <div id="divResults" class="card">
                                <div class="card-header">
                                    <i class="fas fa-newspaper"></i>&nbsp;Results
                                </div>
                                <div class="card-body">
                                    <asp:Literal ID="ltlNews" runat="server"></asp:Literal>
                                </div>
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
            </div>
        </div>
    </div>
    <div class="modal" id="divDeleteNewsModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete News</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete the news entry for this date?
                    <div class="mt-2">
                        All news items associated with this entry will be deleted as well!
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteNews" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteNewsModal" OnClick="lbDeleteNews_Click"><i class="fas fa-check"></i>&nbsp;Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
