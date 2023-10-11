<%@ Page Title="BOQFCC" Language="C#" MasterPageFile="~/MasterPages/Dashboard.master" AutoEventWireup="true" CodeBehind="BOQFCC.aspx.cs" Inherits="Pyramid.Pages.BOQFCC" %>

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
            $('[ID$="lnkBOQFCCDashboard"]').addClass('active');

            //Allow date format for DataTables sorting
            $.fn.dataTable.moment('MM/DD/YYYY');

            //Show/hide the view only fields
            setViewOnlyVisibility();

            //Initialize the datatables
            if (!$.fn.dataTable.isDataTable('#tblIndicators')) {
                $('#tblIndicators').DataTable({
                    responsive: {
                        details: {
                            type: 'column'
                        }
                    },
                    columnDefs: [
                        { orderable: false, targets: [3, 4] },
                        { className: 'control', orderable: false, targets: 0 }
                    ],
                    order: [[2, 'asc']],
                    pageLength: 100,
                    dom: 'frtp',
                    language: {
                        searchPlaceholder: "Enter text to search...",
                        search: "",
                    }
                });
            }
            $('.dataTables_filter input').removeClass('form-control-sm');
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

        //This function is used to calculate the values for the items
        function calculateIndicatorTotal(s, e) {
            //Get the indicator rows
            var indicatorRows = $('.indicator-row');

            //Get the total label
            var totalLabel = $('#lblTotalInPlace');

            //To hold the totals
            var totalRows = indicatorRows.length;
            var totalInPlace = 0;
            var totalInPlacePercent = 0;

            //Loop through the indicator rows
            indicatorRows.each(function (index) {
                //Get the row
                var row = $(this);

                //Get the combo box
                var comboBoxID = row.find('.indicator-combo-box').attr('id');
                var comboBox = ASPxClientControl.Cast(comboBoxID);

                //Get the value in the combo box
                var indicatorValue = Number(comboBox.GetValue());

                //Check to see if the indicator is in place
                if (indicatorValue == 2) {
                    //Increment the total in place value
                    totalInPlace++;
                }
            });

            //Get the total in place percentage
            if (totalInPlace > 0) {
                totalInPlacePercent = Math.round(totalInPlace / totalRows * 100);
            }
            else {
                totalInPlacePercent = 0;
            }

            //Set the total label text
            totalLabel.text(totalInPlace.toString() + ' (' + totalInPlacePercent.toString() + '%)');
        }
    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Label ID="lblPageTitle" runat="server" Text="Benchmarks of Quality FCC" CssClass="h2"></asp:Label>
    <hr />
    <asp:HiddenField ID="hfViewOnly" runat="server" Value="False" />
    <asp:UpdatePanel ID="upBOQFCC" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <uc:Messaging ID="msgSys" runat="server" />
            <asp:HiddenField ID="hfBOQFCCPK" runat="server" Value="" />
            <!-- Main Content -->
            <div class="row">
                <div class="col-lg-12">
                    <div class="card bg-light">
                        <div class="card-header">
                            <div class="row">
                                <div class="col-md-8">
                                    General Information
                                </div>
                                <div class="col-md-4">
                                    <dx:BootstrapButton ID="btnPrintPreview" runat="server" Text="Save and Download/Print" OnClick="btnPrintPreview_Click"
                                        SettingsBootstrap-RenderOption="primary" ValidationGroup="vgBOQFCC" data-validation-group="vgBOQFCC">
                                        <CssClasses Icon="fas fa-print" Control="float-right btn-loader" />
                                    </dx:BootstrapButton>
                                </div>
                            </div>
                        </div>
                        <div class="card-body">
                            <div class="row">
                                <div class="col-md-4">
                                    <dx:BootstrapDateEdit ID="deFormDate" runat="server" Caption="Form Date" EditFormat="Date" EditFormatString="MM/dd/yyyy"
                                        UseMaskBehavior="true" NullText="--Select--"
                                        AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                        <CalendarProperties ShowClearButton="true"></CalendarProperties>
                                        <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Form Date is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapDateEdit>
                                </div>
                                <div class="col-md-4">
                                    <dx:BootstrapMemo ID="txtTeamMembers" runat="server" Caption="Team Members" CaptionSettings-RequiredMarkDisplayMode="Hidden" CaptionSettings-ShowColon="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Team Members are required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapMemo>
                                </div>
                                <div class="col-md-4">
                                    <br />
                                    <label>Program:</label>
                                    <asp:Label ID="lblProgramName" runat="server" Text=""></asp:Label>
                                    <br />
                                    <label>Location:</label>
                                    <asp:Label ID="lblProgramLocation" runat="server" Text=""></asp:Label>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="col-lg-12">
                    <div class="card bg-light">
                        <div class="card-header">
                            Indicators
                        </div>
                        <div class="card-body">
                            <label>BOQ FCC Critical Element Legend</label>
                            <div class="row">
                                <div class="col-sm-12 h5">
                                    <span class="badge badge-info text-wrap mb-2">EMPI = Establish and Maintain a Plan for Implementation</span>
                                    <span class="badge badge-info text-wrap mb-2">FI = Family Involvement</span>
                                    <span class="badge badge-info text-wrap mb-2">PWE = Program-Wide Expectations</span>
                                    <span class="badge badge-info text-wrap mb-2">STAPWE = Strategies for Teaching and Acknowledging the Program-Wide Expectations</span>
                                    <span class="badge badge-info text-wrap mb-2">IPMDAE = Implementation of the Pyramid Model is Demonstrated in All Environments</span>
                                    <span class="badge badge-info text-wrap mb-2">PRCB = Procedures for Responding to Challenging Behavior</span>
                                    <span class="badge badge-info text-wrap mb-2">PDSP = Professional Development and Support Plan</span>
                                    <span class="badge badge-info text-wrap mb-2">MIO = Monitoring Implementation and Outcomes</span>
                                </div>
                            </div>
                            <br />
                            <label>All BOQ FCC Indicators</label>
                            <table class="table table-striped table-bordered table-hover" id="tblIndicators">
                                <thead>
                                    <tr>
                                        <th data-priority="1"></th>
                                        <th>Critical Elements</th>
                                        <th data-priority="3">Indicator Number</th>
                                        <th>Benchmark of Quality</th>
                                        <th data-priority="2"></th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>EMPI</td>
                                        <td>1</td>
                                        <td>Leader (owner/provider) has committed to active problem-solving to ensure the success of the Pyramid Model initiative and the initiative is visibly supportive of the adoption of the model.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator1" runat="server" Width="200px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents Init="calculateIndicatorTotal" ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="99" Text="NA"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 1 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>EMPI</td>
                                        <td>2</td>
                                        <td>Provider has established a clear mission/purpose. The purpose or mission statement is written. All staff (if applicable) are able to clearly communicate the purpose of the Pyramid Model.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator2" runat="server" Width="200px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="99" Text="NA"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 2 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>EMPI</td>
                                        <td>3</td>
                                        <td>Provider has regular meetings with staff, when applicable, or planning time at least 1x per month for a minimum of 1 hour. Monthly planning is consistent.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator3" runat="server" Width="200px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="99" Text="NA"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 3 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>EMPI</td>
                                        <td>4</td>
                                        <td>An implementation plan that includes all critical elements is established. A written implementation plan guides the work of the FCC/GFCC. The plan is reviewed and updated each month. Action steps are identified to ensure achievement of the goals.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator4" runat="server" Width="200px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="99" Text="NA"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 4 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>EMPI</td>
                                        <td>5</td>
                                        <td>Staff are aware of and support for a system for addressing children’s social emotional development and challenging behavior is maintained.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator5" runat="server" Width="200px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="99" Text="NA"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 5 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>EMPI</td>
                                        <td>6</td>
                                        <td>FCC/GFCC reviews and revises the plan at least every six months and shares with families.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator6" runat="server" Width="200px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="99" Text="NA"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 6 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>FI</td>
                                        <td>7</td>
                                        <td>Family input is solicited as part of the planning process. Families are informed of the initiative and asked to provide feedback on the Pyramid Model adoption and mechanisms for promoting family involvement in the initiative.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator7" runat="server" Width="200px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="99" Text="NA"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 7 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>FI</td>
                                        <td>8</td>
                                        <td>There are multiple mechanisms for sharing the Pyramid Model plan with families including narrative documents, conferences, and parent meetings to ensure that all families are informed of the initiative.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator8" runat="server" Width="200px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="99" Text="NA"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 8 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>FI</td>
                                        <td>9</td>
                                        <td>Family involvement in the initiative is supported through a variety of mechanisms including home teaching suggestions, information on supporting social development, and the outcomes of the initiative. Information is shared through a variety of formats (e.g., meetings, home visit, discussions, newsletters, open house, websites, family friendly handouts, workshops, roll-out events).</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator9" runat="server" Width="200px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="99" Text="NA"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 9 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>FI</td>
                                        <td>10</td>
                                        <td>Families are involved in planning for their children in a meaningful and proactive way. Families are encouraged to team with FCC/GFCC staff in the development of individualized plans of support for children including the development of strategies that may be used in the home and community.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator10" runat="server" Width="200px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="99" Text="NA"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 10 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>PWE</td>
                                        <td>11</td>
                                        <td>At least 2-5 positively stated program wide expectations are developed.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator11" runat="server" Width="200px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="99" Text="NA"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 11 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>PWE</td>
                                        <td>12</td>
                                        <td>Expectations are written in a way that applies to both children and staff. When expectations are discussed, the application of expectations to both program staff and children is acknowledged.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator12" runat="server" Width="200px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="99" Text="NA"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 12 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>PWE</td>
                                        <td>13</td>
                                        <td>Expectations are developmentally appropriate and linked to concrete rules for behavior within activities and settings.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator13" runat="server" Width="200px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="99" Text="NA"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 13 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>PWE</td>
                                        <td>14</td>
                                        <td>All program staff are involved in the development of the expectations.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator14" runat="server" Width="200px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="99" Text="NA"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 14 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>PWE</td>
                                        <td>15</td>
                                        <td>Program staff and families are involved in the identification of the program-wide expectations that address needs, cultural norms and values of the program and community.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator15" runat="server" Width="200px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="99" Text="NA"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 15 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>PWE</td>
                                        <td>16</td>
                                        <td>Expectations are shared with families and staff assist families in the translation of the expectations to rules in the home.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator16" runat="server" Width="200px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="99" Text="NA"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 15 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>PWE</td>
                                        <td>17</td>
                                        <td>Expectations are posted in all learning areas (inside and outside) and in common areas in ways that are meaningful to children, staff and families.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator17" runat="server" Width="200px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="99" Text="NA"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 17 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>STAPWE</td>
                                        <td>18</td>
                                        <td>Instruction on expectations is embedded within large group activities, small group activities, and within individual interactions with children.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator18" runat="server" Width="200px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="99" Text="NA"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 18 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>STAPWE</td>
                                        <td>19</td>
                                        <td>A variety of teaching strategies are used: teaching the concept, talking about examples and non-examples, scaffolding children’s use of the expectations in the context of ongoing activities and routines. Instruction on expectations and rules occurs on a daily basis.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator19" runat="server" Width="200px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="99" Text="NA"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 19 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>STAPWE</td>
                                        <td>20</td>
                                        <td>Strategies for acknowledging children’s use of the expectations are developmentally appropriate and used by all program staff including owner/lead provider and support staff (e.g., assistant, substitutes, anyone who assists in the home, etc.).</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator20" runat="server" Width="200px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="99" Text="NA"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 20 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>IPMDAE</td>
                                        <td>21</td>
                                        <td>Provider and program staff have strategies in place to promote positive relationships with children, each other, and families and use those strategies on a daily basis.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator21" runat="server" Width="200px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="99" Text="NA"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 21 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>IPMDAE</td>
                                        <td>22</td>
                                        <td>Provider and program staff have arranged environments, materials, and curriculum in a manner that promotes social-emotional development and guides appropriate behavior.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator22" runat="server" Width="200px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="99" Text="NA"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 22 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>IPMDAE</td>
                                        <td>23</td>
                                        <td>Provider and program staff intentionally promote social and emotional skills within daily activities in a manner that is meaningful to children and promotes skill acquisition.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator23" runat="server" Width="200px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="99" Text="NA"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 23 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>IPMDAE</td>
                                        <td>24</td>
                                        <td>Provider and program staff respond to children’s problem behavior appropriately using evidence-based approaches that are positive and provide the child with guidance about the desired appropriate behavior.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator24" runat="server" Width="200px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="99" Text="NA"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 24 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>IPMDAE</td>
                                        <td>25</td>
                                        <td>Provider and program staff provide targeted social emotional teaching to individual children or small groups of children who are at-risk for challenging behavior.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator25" runat="server" Width="200px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="99" Text="NA"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 25 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>IPMDAE</td>
                                        <td>26</td>
                                        <td>Provider and program staff initiate the development of an individualized plan of behavior support for children with persistent challenging behavior.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator26" runat="server" Width="200px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="99" Text="NA"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 26 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>PRCB</td>
                                        <td>27</td>
                                        <td>Strategies for responding to problem behavior in the program are developed. Provider and staff use evidence-based approaches to respond to problem behavior in a manner that is developmentally appropriate and teaches the child the expected behavior.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator27" runat="server" Width="200px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="99" Text="NA"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 27 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>PRCB</td>
                                        <td>28</td>
                                        <td>A process for responding to crisis situations related to problem behavior is developed. Provider and staff can identify how to request assistance when needed. A plan for addressing the child’s individual behavior support needs is initiated following requests for crisis assistance.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator28" runat="server" Width="200px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="99" Text="NA"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 28 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>PRCB</td>
                                        <td>29</td>
                                        <td>A process for problem solving around problem behavior is developed. Provider and staff can identify a process that may be used to gain support in developing ideas for addressing problem behavior within the program (e.g., peer-support, coaching meeting, brainstorming session).</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator29" runat="server" Width="200px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="99" Text="NA"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 29 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>PRCB</td>
                                        <td>30</td>
                                        <td>A team-based process for addressing individual children with persistent challenging behavior is developed. Provider and staff can identify the steps for initiating the team-based process including fostering the participation of the family in the process.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator30" runat="server" Width="200px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="99" Text="NA"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 30 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>PRCB</td>
                                        <td>31</td>
                                        <td>An individual or individuals with behavioral expertise are identified for coaching program and families throughout the process of developing and implementing individualized intensive interventions for children in need of behavior support plans.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator31" runat="server" Width="200px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="99" Text="NA"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 31 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>PRCB</td>
                                        <td>32</td>
                                        <td>Strategies for partnering with families when there are problem behavior concerns are identified. Provider and staff have strategies for initiating parent contact and partnering with the family to develop strategies to promote appropriate behavior.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator32" runat="server" Width="200px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="99" Text="NA"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 32 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>PDSP</td>
                                        <td>33</td>
                                        <td>A plan for providing ongoing support, training, and coaching on the Pyramid Model practices is developed and implemented.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator33" runat="server" Width="200px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="99" Text="NA"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 33 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>PDSP</td>
                                        <td>34</td>
                                        <td>A data-driven Practiced Based Coaching model is used to assist staff with implementing the Pyramid Model practices to fidelity.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator34" runat="server" Width="200px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="99" Text="NA"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 34 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>PDSP</td>
                                        <td>35</td>
                                        <td>Provider and staff who are responsible for facilitating behavior support processes are identified and trained.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator35" runat="server" Width="200px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="99" Text="NA"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 35 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>PDSP</td>
                                        <td>36</td>
                                        <td>A needs assessment and/or observation tool is conducted with provider and staff to determine training needs on the adoption of the Pyramid Model.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator36" runat="server" Width="200px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="99" Text="NA"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 36 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>PDSP</td>
                                        <td>37</td>
                                        <td>Individualized professional development plans are developed with all providers and staff.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator37" runat="server" Width="200px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="99" Text="NA"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 37 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>PDSP</td>
                                        <td>38</td>
                                        <td>Group and individualized training strategies are identified and implemented.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator38" runat="server" Width="200px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="99" Text="NA"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 38 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>PDSP</td>
                                        <td>39</td>
                                        <td>Plans for training new staff/substitutes are identified and developed.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator39" runat="server" Width="200px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="99" Text="NA"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 39 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>PDSP</td>
                                        <td>40</td>
                                        <td>Incentives and strategies for acknowledging staff (when applicable) and families’ involvement are identified.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator40" runat="server" Width="200px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="99" Text="NA"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 40 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>PDSP</td>
                                        <td>41</td>
                                        <td>A plan for providing ongoing support, training, and coaching in the program on the Pyramid Model including culturally responsive practices and implicit bias is developed and implemented.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator41" runat="server" Width="200px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="99" Text="NA"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 41 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>MIO</td>
                                        <td>42</td>
                                        <td>Process for measuring implementation fidelity is used.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator42" runat="server" Width="200px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="99" Text="NA"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 42 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>MIO</td>
                                        <td>43</td>
                                        <td>Process for measuring outcomes is developed.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator43" runat="server" Width="200px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="99" Text="NA"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 43 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>MIO</td>
                                        <td>44</td>
                                        <td>Data are collected and summarized for program-level and child-level data.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator44" runat="server" Width="200px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="99" Text="NA"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 44 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>MIO</td>
                                        <td>45</td>
                                        <td>Data are shared with program staff and families.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator45" runat="server" Width="200px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="99" Text="NA"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 45 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>MIO</td>
                                        <td>46</td>
                                        <td>Data are used for ongoing monitoring, problem solving, ensuring child response to intervention, and program improvement.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator46" runat="server" Width="200px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="99" Text="NA"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 46 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>MIO</td>
                                        <td>47</td>
                                        <td>Implementation Plan is updated/revised as needed based on the ongoing data.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator47" runat="server" Width="200px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="99" Text="NA"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 47 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                </tbody>
                                <tfoot>
                                    <tr class="font-size-1-2">
                                        <td></td>
                                        <td></td>
                                        <td></td>
                                        <td>
                                            <label>Total In Place</label>
                                        </td>
                                        <td>
                                            <label id="lblTotalInPlace"></label>
                                        </td>
                                    </tr>
                                </tfoot>
                            </table>
                        </div>
                    </div>
                </div>
            </div>
            <div class="page-footer">
                <uc:Submit ID="submitBOQFCC" runat="server" ValidationGroup="vgBOQFCC"
                    ControlCssClass="center-content"
                    OnCancelClick="submitBOQFCC_CancelClick" OnSubmitClick="submitBOQFCC_Click"
                    OnValidationFailed="submitBOQFCC_ValidationFailed" />
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
