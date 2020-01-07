<%@ Page Title="ASQ:SE Screening" Language="C#" MasterPageFile="~/MasterPages/Dashboard.master" AutoEventWireup="true" CodeBehind="ASQSE.aspx.cs" Inherits="Pyramid.Pages.ASQSE" %>

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
            $('#lnkASQSEDashboard').addClass('active');

            //Allow date format for DataTables sorting
            $.fn.dataTable.moment('MM/DD/YYYY');

            //Initialize the datatables
            if (!$.fn.dataTable.isDataTable('#tblScores')) {
                $('#tblScores').DataTable({
                    responsive: {
                        details: {
                            type: 'column'
                        }
                    },
                    columnDefs: [
                        { orderable: false, targets: [0, 1, 2, 3, 4, 5] },
                        { className: 'control', orderable: false, targets: 0 }
                    ],
                    pageLength: 25,
                    dom: 'frtp',
                    language: {
                        searchPlaceholder: "Enter text to search...",
                        search: "",
                    }
                });
            }
            $('.dataTables_filter input').removeClass('form-control-sm');
            
            //Show/hide the view only fields
            setViewOnlyVisibility();
        }
    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Label ID="lblPageTitle" Text="ASQ:SE" CssClass="h2" runat="server"></asp:Label>
    <hr />
    <asp:HiddenField ID="hfViewOnly" runat="server" Value="False" />
    <asp:UpdatePanel ID="upMessaging" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <uc:Messaging ID="msgSys" runat="server" />
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="submitASQSE" />
        </Triggers>
    </asp:UpdatePanel>
    <asp:UpdatePanel ID="upASQSE" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <div class="row">
                <div class="col-lg-12">
                    <div class="card bg-light">
                        <div class="card-header">
                            ASQ:SE Screening
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
                                    <dx:BootstrapDateEdit ID="deFormDate" runat="server" Caption="Screen Date" EditFormat="Date"
                                        EditFormatString="MM/dd/yyyy" UseMaskBehavior="true" NullText="--Select--"
                                        OnValueChanged="deFormDate_ValueChanged" AutoPostBack="true" 
                                        AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                        <ValidationSettings ValidationGroup="vgASQSE" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Form Date is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapDateEdit>
                                </div>
                                <div class="col-md-4">
                                    <dx:BootstrapComboBox ID="ddChild" runat="server" Caption="Child" NullText="--Select--"
                                        TextField="IdAndName" ValueField="ChildPK" ValueType="System.Int32"
                                        IncrementalFilteringMode="Contains" AllowMouseWheel="false"
                                        OnSelectedIndexChanged="ddChild_SelectedIndexChanged" AutoPostBack="true">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgASQSE" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Child is required!  If this is not enabled, the child was not active as of the incident date." />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                    <asp:Label ID="lblAge" runat="server" Text="Label" Visible="false"></asp:Label>
                                </div>
                                <div class="col-md-4">
                                    <dx:BootstrapComboBox ID="ddVersion" runat="server" Caption="Version" NullText="--Select--" ValueType="System.Int32"
                                        IncrementalFilteringMode="Contains" AllowMouseWheel="false"
                                        OnSelectedIndexChanged="ddVersion_SelectedIndexChanged" AutoPostBack="true">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgASQSE" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Version is required" />
                                        </ValidationSettings>
                                        <Items>
                                            <dx:BootstrapListEditItem Text="ASQ:SE-2" Value="2"></dx:BootstrapListEditItem>
                                        </Items>
                                    </dx:BootstrapComboBox>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-4">
                                    <dx:BootstrapComboBox ID="ddInterval" runat="server" Caption="Interval" NullText="--Select--" 
                                        TextField="Description" ValueType="System.Int32"
                                        ValueField="CodeASQSEIntervalPK" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false" 
                                        OnSelectedIndexChanged="ddInterval_SelectedIndexChanged" AutoPostBack="true">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgASQSE" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Interval is required" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </div>
                                <div class="col-md-4">
                                    <dx:BootstrapComboBox ID="ddDemographicSheet" runat="server" NullText="--Select--" 
                                        Caption="Demographic Info Sheet" ValueType="System.Boolean" IncrementalFilteringMode="StartsWith">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgASQSE">
                                            <RequiredField IsRequired="true" ErrorText="The Demograhic Info Sheet is required." />
                                        </ValidationSettings>
                                        <Items>
                                            <dx:BootstrapListEditItem Text="Yes" Value="True"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Text="No" Value="False"></dx:BootstrapListEditItem>
                                        </Items>
                                    </dx:BootstrapComboBox>
                                </div>
                                <div class="col-md-4">
                                    <dx:BootstrapComboBox ID="ddPhysicianLetter" runat="server" NullText="--Select--" 
                                        Caption="Physician Information Letter" ValueType="System.Boolean" IncrementalFilteringMode="StartsWith">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgASQSE">
                                            <RequiredField IsRequired="true" ErrorText="The Physician Information Letter is required." />
                                        </ValidationSettings>
                                        <Items>
                                            <dx:BootstrapListEditItem Text="Yes" Value="True"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Text="No" Value="False"></dx:BootstrapListEditItem>
                                        </Items>
                                    </dx:BootstrapComboBox>
                                </div>
                            </div>
                            <div class="row mt-2">
                                <div class="col-md-12">
                                    <label>Scores</label>
                                    <table id="tblScores" class="table table-striped table-bordered table-hover">
                                        <thead>
                                            <tr>
                                                <th data-priority="1"></th>
                                                <th data-priority="3">Area</th>
                                                <th data-priority="2">Score</th>
                                                <th data-priority="4">Score Type</th>
                                                <th>Monitoring Zone</th>
                                                <th>Cutoff Score</th>
                                            </tr>
                                        </thead>
                                        <tbody>
                                            <tr>
                                                <td></td>
                                                <td>Total Score</td>
                                                <td>
                                                    <dx:BootstrapTextBox ID="txtTotalScore" runat="server"
                                                        OnValidation="txtTotalScore_Validation" 
                                                        OnValueChanged="txtTotalScore_ValueChanged" AutoPostBack="true">
                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                        <MaskSettings Mask="099" PromptChar=" " ErrorText="Total Score must be a valid number!" />
                                                        <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgASQSE">
                                                            <RequiredField IsRequired="true" ErrorText="Total Score is required" />
                                                        </ValidationSettings>
                                                    </dx:BootstrapTextBox>
                                                </td>
                                                <td>
                                                    <asp:Label ID="lblScoreType" runat="server"  Text=""></asp:Label>
                                                </td>
                                                <td>
                                                    <asp:Label ID="lblMonitoringZone" runat="server" Text=""></asp:Label>
                                                </td>
                                                <td>
                                                    <asp:Label ID="lblCutoffScore" runat="server" Text=""></asp:Label>
                                                </td>
                                            </tr>
                                        </tbody>
                                    </table>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div class="page-footer">
                <uc:Submit ID="submitASQSE" runat="server" ValidationGroup="vgASQSE" OnSubmitClick="submitASQSE_Click" OnCancelClick="submitASQSE_CancelClick" OnValidationFailed="submitASQSE_ValidationFailed" />
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
