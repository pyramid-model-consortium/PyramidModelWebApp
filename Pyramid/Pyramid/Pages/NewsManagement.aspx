<%@ Page Title="Manage News" Language="C#" MasterPageFile="~/MasterPages/LoggedIn.master" AutoEventWireup="true" CodeBehind="NewsManagement.aspx.cs" Inherits="Pyramid.Pages.NewsManagement" %>

<%@ Register Assembly="DevExpress.Web.Bootstrap.v19.1, Version=19.1.6.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>
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
            //Allow date format for DataTables sorting
            $.fn.dataTable.moment('MM/DD/YYYY');

            //Initialize the datatables
            if (!$.fn.dataTable.isDataTable('#tblNewsItems')) {
                $('#tblNewsItems').DataTable({
                    responsive: {
                        details: {
                            type: 'column'
                        }
                    },
                    columnDefs: [
                        { targets: [3], orderable: false },
                        { className: 'control', orderable: false, targets: 0 }
                    ],
                    order: [[1, 'asc']],
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

        //Show the proper entry type option
        function showEntryTypeOption(s, e) {
            //Get the entry type
            var entryTypeText = ddEntryType.GetText().toLowerCase();

            if (entryTypeText) {
                //Show the proper option
                if (entryTypeText.includes('program')) {
                    $('.entry-type-option').addClass('hidden');
                    $('#divPrograms').removeClass('hidden');
                }
                else if (entryTypeText.includes('hub')) {
                    $('.entry-type-option').addClass('hidden');
                    $('#divHubs').removeClass('hidden');
                }
                else if (entryTypeText.includes('state')) {
                    $('.entry-type-option').addClass('hidden');
                    $('#divStates').removeClass('hidden');
                }
                else if (entryTypeText.includes('cohort')) {
                    $('.entry-type-option').addClass('hidden');
                    $('#divCohorts').removeClass('hidden');
                }
            }
        }

        //This function validates the program dropdown
        function validateEntryTypeOption(s, e) {
            //Get the entry type
            var entryTypeText = ddEntryType.GetText().toLowerCase();
            var selectedOption;

            //Perform validation
            if (entryTypeText) {
                if (entryTypeText.includes('program')) {
                    selectedOption = ddProgram.GetValue();

                    if (!selectedOption) {
                        e.isValid = false;
                        e.errorText = 'Required!';
                    }
                }
                else if (entryTypeText.includes('hub')) {
                    selectedOption = ddHub.GetValue();

                    if (!selectedOption) {
                        e.isValid = false;
                        e.errorText = 'Required!';
                    }
                }
                else if (entryTypeText.includes('state')) {
                    selectedOption = ddState.GetValue();

                    if (!selectedOption) {
                        e.isValid = false;
                        e.errorText = 'Required!';
                    }
                }
                else if (entryTypeText.includes('cohort')) {
                    selectedOption = ddCohort.GetValue();

                    if (!selectedOption) {
                        e.isValid = false;
                        e.errorText = 'Required!';
                    }
                }
            }
        }
    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Label ID="lblPageTitle" Text="" CssClass="h2" runat="server"></asp:Label>
    <hr />
    <asp:HiddenField ID="hfViewOnly" runat="server" Value="False" />
    <asp:UpdatePanel ID="upMessaging" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <uc:Messaging ID="msgSys" runat="server" />
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="lbAddNewsItem" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="repeatNewsItems" />
            <asp:AsyncPostBackTrigger ControlID="lbDeleteNewsItem" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="submitNewsItem" />
            <asp:AsyncPostBackTrigger ControlID="submitNewsEntry" />
        </Triggers>
    </asp:UpdatePanel>
    <div class="card bg-light">
        <div class="card-header">
            News Entry
        </div>
        <div class="card-body">
            <div class="row">
                <div class="col-lg-4">
                    <dx:BootstrapDateEdit ID="deEntryDate" runat="server" Caption="News Entry Date" EditFormat="Date"
                        EditFormatString="MM/dd/yyyy" UseMaskBehavior="true" NullText="--Select--"
                        AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900">
                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                        <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                        <ValidationSettings ValidationGroup="vgNewsEntry" ErrorDisplayMode="ImageWithText">
                            <RequiredField IsRequired="true" ErrorText="News Entry Date is required!" />
                        </ValidationSettings>
                    </dx:BootstrapDateEdit>
                </div>
                <div class="col-lg-4">
                    <dx:BootstrapComboBox ID="ddEntryType" runat="server" Caption="Type" NullText="--Select--"
                        TextField="Description" ValueField="CodeNewsEntryTypePK" ValueType="System.Int32"
                        IncrementalFilteringMode="Contains" AllowMouseWheel="false"
                        ClientInstanceName="ddEntryType">
                        <ClientSideEvents ValueChanged="showEntryTypeOption" Init="showEntryTypeOption" />
                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                        <ValidationSettings ValidationGroup="vgNewsEntry" ErrorDisplayMode="ImageWithText">
                            <RequiredField IsRequired="true" ErrorText="Type is required!" />
                        </ValidationSettings>
                    </dx:BootstrapComboBox>
                </div>
                <div id="divPrograms" class="col-lg-4 hidden entry-type-option">
                    <dx:BootstrapComboBox ID="ddProgram" runat="server" Caption="Program" NullText="--Select--"
                        TextField="ProgramName" ValueField="ProgramPK" ValueType="System.Int32"
                        IncrementalFilteringMode="Contains" AllowMouseWheel="false"
                        ClientInstanceName="ddProgram" OnValidation="NewsEntryTypeOption_Validation">
                        <ClientSideEvents Validation="validateEntryTypeOption" />
                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                        <ValidationSettings ValidationGroup="vgNewsEntry" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                            <RequiredField IsRequired="false" ErrorText="Program is required!" />
                        </ValidationSettings>
                    </dx:BootstrapComboBox>
                </div>
                <div id="divHubs" class="col-lg-4 hidden entry-type-option">
                    <dx:BootstrapComboBox ID="ddHub" runat="server" Caption="Hub" NullText="--Select--"
                        TextField="Name" ValueField="HubPK" ValueType="System.Int32"
                        IncrementalFilteringMode="Contains" AllowMouseWheel="false"
                        ClientInstanceName="ddHub" OnValidation="NewsEntryTypeOption_Validation">
                        <ClientSideEvents Validation="validateEntryTypeOption" />
                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                        <ValidationSettings ValidationGroup="vgNewsEntry" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                            <RequiredField IsRequired="false" ErrorText="Hub is required!" />
                        </ValidationSettings>
                    </dx:BootstrapComboBox>
                </div>
                <div id="divStates" class="col-lg-4 hidden entry-type-option">
                    <dx:BootstrapComboBox ID="ddState" runat="server" Caption="State" NullText="--Select--"
                        TextField="Name" ValueField="StatePK" ValueType="System.Int32"
                        IncrementalFilteringMode="Contains" AllowMouseWheel="false"
                        ClientInstanceName="ddState" OnValidation="NewsEntryTypeOption_Validation">
                        <ClientSideEvents Validation="validateEntryTypeOption" />
                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                        <ValidationSettings ValidationGroup="vgNewsEntry" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                            <RequiredField IsRequired="false" ErrorText="State is required!" />
                        </ValidationSettings>
                    </dx:BootstrapComboBox>
                </div>
                <div id="divCohorts" class="col-lg-4 hidden entry-type-option">
                    <dx:BootstrapComboBox ID="ddCohort" runat="server" Caption="Cohort" NullText="--Select--"
                        TextField="CohortName" ValueField="CohortPK" ValueType="System.Int32"
                        IncrementalFilteringMode="Contains" AllowMouseWheel="false"
                        ClientInstanceName="ddCohort" OnValidation="NewsEntryTypeOption_Validation">
                        <ClientSideEvents Validation="validateEntryTypeOption" />
                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                        <ValidationSettings ValidationGroup="vgNewsEntry" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                            <RequiredField IsRequired="false" ErrorText="Cohort is required!" />
                        </ValidationSettings>
                    </dx:BootstrapComboBox>
                </div>
            </div>
            <div id="divAddOnlyMessage" runat="server" visible="false" class="alert alert-primary">
                You must save the entry date and type before adding more information.
            </div>
            <div id="divEditOnly" runat="server" visible="false">
                <div class="row">
                    <div class="col-lg-12">
                        <asp:HiddenField ID="hfDeleteNewsItemPK" runat="server" Value="0" />
                        <asp:UpdatePanel ID="upNewsItems" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
                            <ContentTemplate>
                                <div class="card">
                                    <div class="card-header">
                                        Items
                                        <asp:LinkButton ID="lbAddNewsItem" runat="server" CssClass="btn btn-loader btn-primary float-right hide-on-view hidden" OnClick="lbAddNewsItem_Click"><i class="fas fa-plus"></i>&nbsp;Add New Item</asp:LinkButton>
                                    </div>
                                    <div class="card-body">
                                        <div class="row">
                                            <div class="col-lg-12">
                                                <div class="alert alert-primary">
                                                    The Order # field determines the news item order by sorting it in ascending order on the news page and home page.  (e.g., an item with an order # of 1 will always be shown before an item with an order # of 2)
                                                </div>
                                                <asp:Repeater ID="repeatNewsItems" runat="server" ItemType="Pyramid.Models.NewsItem">
                                                    <HeaderTemplate>
                                                        <table id="tblNewsItems" class="table table-striped table-bordered table-hover">
                                                            <thead>
                                                                <tr>
                                                                    <th data-priority="1"></th>
                                                                    <th data-priority="2">Order #</th>
                                                                    <th>Contents</th>
                                                                    <th data-priority="3"></th>
                                                                </tr>
                                                            </thead>
                                                            <tbody>
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <tr>
                                                            <td></td>
                                                            <td><%# Item.ItemNum %></td>
                                                            <td><%# Item.Contents %></td>
                                                            <td class="text-center">
                                                                <div class="btn-group">
                                                                    <button type="button" class="btn btn-secondary dropdown-toggle hide-on-view hidden" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                                                                        Actions
                                                                    </button>
                                                                    <div class="dropdown-menu dropdown-menu-right">
                                                                        <asp:LinkButton ID="lbEditNewsItem" runat="server" CssClass="dropdown-item" OnClick="lbEditNewsItem_Click"><i class="fas fa-edit"></i> Edit</asp:LinkButton>
                                                                        <button class="dropdown-item delete-gridview" data-pk='<%# Item.NewsItemPK %>' data-hf="hfDeleteNewsItemPK" data-target="#divDeleteNewsItemModal"><i class="fas fa-trash"></i>&nbsp;Delete</button>
                                                                    </div>
                                                                </div>
                                                                <asp:HiddenField ID="hfNewsItemPK" runat="server" Value='<%# Item.NewsItemPK %>' />
                                                            </td>
                                                        </tr>
                                                    </ItemTemplate>
                                                    <FooterTemplate>
                                                        </tbody>
                                                    </table>
                                                    </FooterTemplate>
                                                </asp:Repeater>
                                            </div>
                                        </div>
                                        <div class="row">
                                            <div class="col-lg-12">
                                                <div id="divAddEditNewsItem" runat="server" class="card mt-2" visible="false">
                                                    <div class="card-header">
                                                        <asp:Label ID="lblAddEditNewsItem" runat="server" Text=""></asp:Label>
                                                    </div>
                                                    <div class="card-body">
                                                        <div class="row">
                                                            <div class="col-xl-12">
                                                                <div class="form-group">
                                                                    <dx:BootstrapTextBox ID="txtItemNum" runat="server" Caption="Order #">
                                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                                        <MaskSettings Mask="099" PromptChar=" " ErrorText="Must be a valid number!" />
                                                                        <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgNewsItem">
                                                                            <RequiredField IsRequired="true" ErrorText="Order # is required!" />
                                                                        </ValidationSettings>
                                                                    </dx:BootstrapTextBox>
                                                                </div>
                                                            </div>
                                                            <div class="col-xl-12">
                                                                <div class="form-group">
                                                                    <dx:BootstrapMemo ID="txtNewsItemContents" runat="server" Caption="Contents" Rows="5">
                                                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                                        <ValidationSettings ValidationGroup="vgNewsItem" ErrorDisplayMode="ImageWithText">
                                                                            <RequiredField IsRequired="true" ErrorText="Contents are required!" />
                                                                        </ValidationSettings>
                                                                    </dx:BootstrapMemo>
                                                                </div>
                                                            </div>
                                                        </div>
                                                    </div>
                                                    <div class="card-footer">
                                                        <div class="center-content">
                                                            <asp:HiddenField ID="hfAddEditNewsItemPK" runat="server" Value="0" />
                                                            <uc:Submit ID="submitNewsItem" runat="server" ValidationGroup="vgNewsItem" OnSubmitClick="submitNewsItem_Click" OnCancelClick="submitNewsItem_CancelClick" OnValidationFailed="submitNewsItem_ValidationFailed" />
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </ContentTemplate>
                            <Triggers>
                                <asp:AsyncPostBackTrigger ControlID="lbDeleteNewsItem" EventName="Click" />
                                <asp:AsyncPostBackTrigger ControlID="submitNewsEntry" />
                            </Triggers>
                        </asp:UpdatePanel>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <div class="page-footer">
        <uc:Submit ID="submitNewsEntry" runat="server" ValidationGroup="vgNewsEntry" OnSubmitClick="submitNewsEntry_Click" OnCancelClick="submitNewsEntry_CancelClick" OnValidationFailed="submitNewsEntry_ValidationFailed" />
    </div>
    <div class="modal" id="divDeleteNewsItemModal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Delete News Item</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    Are you sure you want to delete this news item?
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No</button>
                    <asp:LinkButton ID="lbDeleteNewsItem" runat="server" CssClass="btn btn-loader btn-danger modal-delete" data-target="#divDeleteNewsItemModal" OnClick="lbDeleteNewsItem_Click"><i class="fas fa-check"></i>&nbsp;Yes</asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
