<%@ Page Title="BOQ FCC V2" Language="C#" MasterPageFile="~/MasterPages/Dashboard.master" AutoEventWireup="true" CodeBehind="BOQFCCV2.aspx.cs" Inherits="Pyramid.Pages.BOQFCCV2" %>

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
                                <div class="col-md-3">
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
                                <div class="col-md-3">
                                    <dx:BootstrapMemo ID="txtTeamMembers" runat="server" Caption="Team Members" CaptionSettings-RequiredMarkDisplayMode="Hidden" CaptionSettings-ShowColon="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Team Members are required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapMemo>
                                </div>
                                <div class="col-lg-3">
                                    <dx:BootstrapComboBox ID="ddProgram" runat="server" Caption="Program" NullText="--Select--"
                                        TextField="ProgramName" ValueField="ProgramPK" ValueType="System.Int32" AllowNull="true"
                                        IncrementalFilteringMode="Contains" AllowMouseWheel="false" AutoPostBack="true"
                                        OnValueChanged="ddProgram_ValueChanged">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                            <RequiredField IsRequired="true" ErrorText="Program is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </div>
                                <div class="col-lg-3">
                                    <asp:Label runat="server" AssociatedControlID="lblProgramLocation" CssClass="d-block col-form-label" Text="Program Location"></asp:Label>
                                    <asp:Label ID="lblProgramLocation" runat="server"></asp:Label>
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
                                    <span class="badge badge-info text-wrap mb-2">FE = Family Engagement</span>
                                    <span class="badge badge-info text-wrap mb-2">PE = Program Expectations</span>
                                    <span class="badge badge-info text-wrap mb-2">PD = Professional Development</span>
                                    <span class="badge badge-info text-wrap mb-2">IPP = Implementation of Pyramid Practices</span>
                                    <span class="badge badge-info text-wrap mb-2">PRCB = Procedures for Responding to Challenging Behavior</span>
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
                                        <td>Leader (owner/provider) has committed to and is visibly supportive of the implementation of the Pyramid Model.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator1" runat="server" Width="200px" NullText="--Select--" AllowNull="true"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents Init="calculateIndicatorTotal" ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
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
                                        <td>Provider has established a clear mission or purpose related to the Pyramid Model and developed a written mission statement. All staff (when applicable in large FCCH) can clearly communicate the purpose of the Pyramid Model.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator2" runat="server" Width="200px" NullText="--Select--" AllowNull="true"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
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
                                        <td>An implementation plan that includes all critical elements is established. A written implementation plan guides the work of the FCCH. The plan is reviewed and updated on a regular basis (planning with staff in large FCCH). Action steps are identified to ensure achievement of the goals.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator3" runat="server" Width="200px" NullText="--Select--" AllowNull="true"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
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
                                        <td>Provider (and staff when applicable) are supportive of the use of the Pyramid Model promotion, prevention, and intervention practices in a manner that is culturally responsive and includes examining implicit bias.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator4" runat="server" Width="200px" NullText="--Select--" AllowNull="true"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
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
                                        <td>Provider has a child discipline policy that includes the promotion of social and emotional skills and the use of positive guidance and prevention practices and makes a commitment to the elimination of suspension and expulsion.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator5" runat="server" Width="200px" NullText="--Select--" AllowNull="true"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 5 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>FE</td>
                                        <td>6</td>
                                        <td>Family input is solicited as part of the planning and decision-making process. Families are informed of the initiative and asked to provide feedback on the Pyramid Model implementation.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator6" runat="server" Width="200px" NullText="--Select--" AllowNull="true"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 6 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>FE</td>
                                        <td>7</td>
                                        <td>Family engagement is supported through a variety of mechanisms including home teaching suggestions, information on supporting social-emotional development, and the strategies used in the program. Information is shared through a variety of formats (e.g., meetings, home visits, discussions, newsletters, open house, websites or social media, family friendly handouts, workshops, roll-out events).</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator7" runat="server" Width="200px" NullText="--Select--" AllowNull="true"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 7 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>FE</td>
                                        <td>8</td>
                                        <td>Families are involved in planning for individual children in a meaningful and proactive way. Families are encouraged to team with FCCH staff in the development of individualized plans of support for children including the development of strategies that might be used in the home and community.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator8" runat="server" Width="200px" NullText="--Select--" AllowNull="true"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 8 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>PE</td>
                                        <td>9</td>
                                        <td>2-5 positively stated program wide expectations are developed with input from program staff and families.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator9" runat="server" Width="200px" NullText="--Select--" AllowNull="true"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 9 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>PE</td>
                                        <td>10</td>
                                        <td>Expectations are written in a way that applies to both children and staff. When expectations are discussed, the application of expectations to program staff and children is acknowledged.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator10" runat="server" Width="200px" NullText="--Select--" AllowNull="true"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 10 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>PE</td>
                                        <td>11</td>
                                        <td>Expectations are developmentally appropriate and linked to concrete rules for behavior within activities and settings.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator11" runat="server" Width="200px" NullText="--Select--" AllowNull="true"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 11 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>PE</td>
                                        <td>12</td>
                                        <td>Expectations are posted in all learning areas (e.g., indoors and outside) and in common areas in ways that are meaningful to children, staff, and families.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator12" runat="server" Width="200px" NullText="--Select--" AllowNull="true"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 12 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>PE</td>
                                        <td>13</td>
                                        <td>Instruction on expectations is embedded throughout the day using a variety of teaching strategies within large group activities, small group activities, and individual interactions with children. Instruction occurs daily.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator13" runat="server" Width="200px" NullText="--Select--" AllowNull="true"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 13 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>PE</td>
                                        <td>14</td>
                                        <td>The provider and staff regularly acknowledge child engagement in expectations and rules in a developmentally appropriate manner.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator14" runat="server" Width="200px" NullText="--Select--" AllowNull="true"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 14 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>PD</td>
                                        <td>15</td>
                                        <td>Practice-based coaching is used to assist providers with implementing Pyramid Model practices to fidelity.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator15" runat="server" Width="200px" NullText="--Select--" AllowNull="true"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 15 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>PD</td>
                                        <td>16</td>
                                        <td>A plan for providing ongoing support, training, and coaching in the FCCH on the Pyramid Model including culturally responsive practices and implicit bias is developed and implemented.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator16" runat="server" Width="200px" NullText="--Select--" AllowNull="true"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 15 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>PD</td>
                                        <td>17</td>
                                        <td>A needs assessment and/or observation tool is used to determine training needs on Pyramid Model practices.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator17" runat="server" Width="200px" NullText="--Select--" AllowNull="true"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 17 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>IPP</td>
                                        <td>18</td>
                                        <td>Provider and program staff are proficient at teaching social and emotional skills within daily activities in a manner that is meaningful to children and promotes skill acquisition.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator18" runat="server" Width="200px" NullText="--Select--" AllowNull="true"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 18 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>IPP</td>
                                        <td>19</td>
                                        <td>Provider and program staff respond to children’s challenging behavior appropriately using evidence-based approaches that are positive and provide the child with guidance about the desired appropriate behavior.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator19" runat="server" Width="200px" NullText="--Select--" AllowNull="true"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 19 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>IPP</td>
                                        <td>20</td>
                                        <td>Provider and program staff provide targeted social emotional teaching to individual children or small groups of children who are at-risk for challenging behavior.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator20" runat="server" Width="200px" NullText="--Select--" AllowNull="true"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 20 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>PRCB</td>
                                        <td>21</td>
                                        <td>Strategies for responding to children’s challenging behavior are developed. Provider and staff use evidence-based approaches that are positive, sensitive to family values, culture, and home language, and provide the child with guidance about the desired appropriate behavior and program-wide expectations.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator21" runat="server" Width="200px" NullText="--Select--" AllowNull="true"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 21 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>PRCB</td>
                                        <td>22</td>
                                        <td>Provider has received training related to potential bias when responding to behavior challenges and have strategies to reflect on their responses to individual children.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator22" runat="server" Width="200px" NullText="--Select--" AllowNull="true"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 22 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>PRCB</td>
                                        <td>23</td>
                                        <td>A process for responding to crisis situations related to challenging behavior is developed.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator23" runat="server" Width="200px" NullText="--Select--" AllowNull="true"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 23 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>PRCB</td>
                                        <td>24</td>
                                        <td>A team-based process for addressing individual children with persistent challenging behavior is developed. Provider and staff can identify the steps for the process including fostering the participation of the family in the development of a plan.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator24" runat="server" Width="200px" NullText="--Select--" AllowNull="true"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 24 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>PRCB</td>
                                        <td>25</td>
                                        <td>Provider and program staff develop an individualized plan of behavior support for children with persistent challenging behavior.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator25" runat="server" Width="200px" NullText="--Select--" AllowNull="true"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 25 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>PRCB</td>
                                        <td>26</td>
                                        <td>Provider and staff initiate family contact and partner with the family to develop strategies to prevent challenging behavior and promote social-emotional skills.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator26" runat="server" Width="200px" NullText="--Select--" AllowNull="true"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 26 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>MIO</td>
                                        <td>27</td>
                                        <td>Implementation fidelity is measured regularly using the Benchmarks of Quality and a practice fidelity self-assessment or observation.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator27" runat="server" Width="200px" NullText="--Select--" AllowNull="true"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 27 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>MIO</td>
                                        <td>28</td>
                                        <td>The provider collects data on child outcomes (e.g., behavior incidents, child engagement).</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator28" runat="server" Width="200px" NullText="--Select--" AllowNull="true"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 28 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>MIO</td>
                                        <td>29</td>
                                        <td>Data are collected and summarized.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator29" runat="server" Width="200px" NullText="--Select--" AllowNull="true"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 29 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>MIO</td>
                                        <td>30</td>
                                        <td>Data are shared with program staff and families.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator30" runat="server" Width="200px" NullText="--Select--" AllowNull="true"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 30 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>MIO</td>
                                        <td>31</td>
                                        <td>Data are used for ongoing monitoring, problem solving, ensuring child response to intervention, and program improvement.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator31" runat="server" Width="200px" NullText="--Select--" AllowNull="true"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 31 is required!" />
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
            <div id="divIsComplete" runat="server" class="alert alert-primary mb-0">
                <p>
                    <i class="fas fa-info-circle"></i>&nbsp;
                    Check the box below once you have entered all the information on the form and believe 
                    that it is complete and ready to be included in reporting.
                </p>
                <dx:BootstrapCheckBox ID="chkIsComplete" runat="server" Text="The form is complete" 
                    AutoPostBack="true" OnCheckedChanged="chkIsComplete_CheckedChanged">
                    <SettingsBootstrap InlineMode="true" />
                    <ValidationSettings ValidationGroup="vgBOQFCC" ErrorDisplayMode="ImageWithText">
                        <RequiredField IsRequired="false" ErrorText="Required!" />
                    </ValidationSettings>
                </dx:BootstrapCheckBox>
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