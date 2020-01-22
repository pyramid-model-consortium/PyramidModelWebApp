<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/MasterPages/Dashboard.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Pyramid._Default" %>

<%@ Register TagPrefix="uc" TagName="Messaging" Src="~/User_Controls/MessagingSystem.ascx" %>
<%@ Register Assembly="DevExpress.Web.Bootstrap.v19.1, Version=19.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>

<asp:Content ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .news-div {
            position: fixed;
            bottom: 10px;
            left: 10px;
            z-index: 999;
            max-width: 25%;
            min-width: 250px;
        }

        .news-div-content {
            max-height: 450px;
            overflow: auto;
        }
        
        @media (max-height: 600px) {
            .news-div-content {
                max-height: 350px;
            }
        }
    </style>
</asp:Content>
<asp:Content ContentPlaceHolderID="ScriptContent" runat="server">
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
            $('#lnkHomeDashboard').addClass('active');

            //When the user hides the news, store the date it was hidden in a cookie
            jQuery("#btnHideNews").on("click", function (e) {
                //Prevent postback
                e.preventDefault();
                e.stopPropagation();

                //Get date the news was hidden
                var date = new Date();
                var dateString = ((date.getMonth() + 1) + '/' + date.getDate() + '/' + date.getFullYear());

                //Set the news cookie and hide the news
                Cookies.set('newsHidden', dateString, { expires: 365, sameSite: 'lax' });
                jQuery("#divNews").slideUp(400, "linear", function () {
                    jQuery("#btnShowNews").fadeIn();
                });
            });

            //When the user shows the news, remove the hidden cookie and show the news
            jQuery("#btnShowNews").on("click", function (e) {
                //Prevent postback
                e.preventDefault();
                e.stopPropagation();

                //Remove the news cookie
                if (Cookies.get('newsHidden') !== undefined) {
                    Cookies.remove('newsHidden');
                }

                //Show the news
                jQuery("#divNews").slideDown();
                jQuery("#btnShowNews").hide();
            });

            if (Cookies.get('newsHidden') !== undefined) {
                //Get the date of the most recent news
                var mostRecentNewsDateText = jQuery("#divNews .news-entry-date").first().text();
                var mostRecentNewsDate = new Date(mostRecentNewsDateText);

                //Get the date the news was hidden
                var comparisonDate = new Date(Cookies.get('newsHidden'));

                //If the last news date doesn't exist or it is before, or on the hide date, don't show the news
                if (!mostRecentNewsDateText || mostRecentNewsDate <= comparisonDate) {
                    jQuery("#btnShowNews").show();
                }
                else {
                    jQuery("#btnShowNews").hide();
                    jQuery("#divNews").slideDown();
                }
            }
            else {
                jQuery("#divNews").slideDown();
                jQuery("#btnShowNews").hide();
            }
        }
    </script>
</asp:Content>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <asp:UpdatePanel ID="upDefault" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <uc:Messaging ID="msgSys" runat="server" />
            <div id="divFireworks" runat="server">
                <div class="before"></div>
                <div class="after"></div>
            </div>
            <div class="row">
                <div class="col-sm-12 col-md-10">
                    <div class="text-center mt-2">
                        <h2>Welcome to the Pyramid Model Implementation Data System!</h2>
                        <dx:BootstrapImage ID="bsImgLargeLogo" runat="server" AlternateText="Large Site Logo" CssClasses-Control="mt-3 large-logo" ImageUrl="/Content/images/GenericLogo.png" Height="20%" Width="40%"></dx:BootstrapImage>
                        <div class="mt-4">
                            <asp:LinkButton ID="lbEnableFireworks" runat="server" CssClass="btn btn-loader btn-primary" OnClick="lbEnableFireworks_Click"><i class="fas fa-toggle-off"></i> Enable Fireworks</asp:LinkButton>
                            <asp:LinkButton ID="lbDisableFireworks" runat="server" CssClass="btn btn-loader btn-primary" OnClick="lbDisableFireworks_Click"><i class="fas fa-toggle-on"></i> Disable Fireworks</asp:LinkButton>
                        </div>
                    </div>
                </div>
            </div>
            <div class="row">
                <div class="col-md-12">
                    <button id="btnShowNews" class="btn btn-primary"><span class="fas fa-plus"></span>&nbsp;Show News</button>
                </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
    <div id="divNews" class="news-div" style="display:none;">
        <div class="card">
            <div class="card-header">
                <i class="fas fa-newspaper"></i>&nbsp;News
                <a href="/Pages/News.aspx" class="btn btn-primary float-right"><i class="fas fa-newspaper"></i>&nbsp;All News</a>
            </div>
            <div class="card-body">
                <div class="news-div-content">
                    <asp:Literal ID="ltlNews" runat="server"></asp:Literal>
                </div>
                <hr />
                <div class="mt-2">
                    <button id="btnHideNews" class="btn btn-primary"><span class="fas fa-minus"></span>&nbsp;Hide News</button>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
