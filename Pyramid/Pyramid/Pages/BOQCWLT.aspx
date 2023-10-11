<%@ Page Title="Community Wide BOQ" Language="C#" MasterPageFile="~/MasterPages/Dashboard.master" AutoEventWireup="true" CodeBehind="BOQCWLT.aspx.cs" Inherits="Pyramid.Pages.BOQCWLT" %>

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
            $('[ID$="lnkCWLTDashboard"]').addClass('active');

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
    <asp:Label ID="lblPageTitle" runat="server" Text="Community-Wide Benchmarks of Quality" CssClass="h2"></asp:Label>
    <hr />
    <asp:HiddenField ID="hfViewOnly" runat="server" Value="False" />
    <asp:UpdatePanel ID="upBOQCWLT" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <uc:Messaging ID="msgSys" runat="server" />
            <asp:HiddenField ID="hfBOQCWLTPK" runat="server" Value="" />
            <!--main content-->
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
                                        SettingsBootstrap-RenderOption="primary" ValidationGroup="vgBOQCWLT" data-validation-group="vgBOQCWLT">
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
                                        AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900" OnValueChanged="deFormDate_ValueChanged" AutoPostBack="true">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                        <CalendarProperties ShowClearButton="true"></CalendarProperties>
                                        <ValidationSettings ValidationGroup="vgBOQCWLT" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Form Date is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapDateEdit>
                                </div>
                                <div class="col-md-4">
                                    <dx:BootstrapTagBox ID="tbTeamMembers" runat="server" Caption="Team Members"
                                        AllowCustomTags="false" TextField="MemberIDAndName" ValueField="CWLTMemberPK">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgBOQCWLT" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="At least one team member must be selected!" />
                                        </ValidationSettings>
                                    </dx:BootstrapTagBox>
                                    <button id="lnkTeamMembersHelp" type="button" class="btn btn-link p-0"
                                        data-toggle="popover" data-trigger="focus hover" 
                                        title="Help" 
                                        data-content="The form date must be entered before you can select any team members.  If the form date has been entered and a team member is still not selectable, that means the team member is either not in the system or has a leave date that is before the form date.">
                                        <i class="fas fa-question-circle"></i>&nbsp;Help
                                    </button>
                                </div>
                                <div class="col-md-4">
                                    <br />
                                    <asp:Label runat="server" AssociatedControlID="lblHubName" CssClass="d-block">Hub</asp:Label>
                                    <asp:Label ID="lblHubName" runat="server" Text=""></asp:Label>
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
                            <label>Critical Element Legend</label>
                            <div class="row">
                                <div class="col-sm-12 h5">
                                    <span class="badge badge-info text-wrap mb-2">CLTMT = Community Leadership Team Membership and Teaming</span>
                                    <span class="badge badge-info text-wrap mb-2">FD = Funding</span>
                                    <span class="badge badge-info text-wrap mb-2">CV = Communication and Visibility</span>
                                    <span class="badge badge-info text-wrap mb-2">IDS = Implementation and Demonstration Sites</span>
                                    <span class="badge badge-info text-wrap mb-2">FM = Families</span>
                                    <span class="badge badge-info text-wrap mb-2">BS = Behavior Support</span>
                                    <span class="badge badge-info text-wrap mb-2">PD = Professional Development</span>
                                    <span class="badge badge-info text-wrap mb-2">MIO = Monitoring Implementation and Outcomes</span>
                                </div>
                            </div>
                            <br />
                            <label>All Indicators</label>
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
                                        <td>CLTMT</td>
                                        <td>1</td>
                                        <td>The Community Leadership Team (CLT) has representation from key stakeholders (e.g., practitioner, family members, program administrators) and program agencies (Head Start, childcare, school district, early childhood mental health, and other organizations) invested in promoting the social-emotional skills of young children. The CLT includes member(s) that represent the diverse families in the community.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator1" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents Init="calculateIndicatorTotal" ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQCWLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 1 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>CLTMT</td>
                                        <td>2</td>
                                        <td>The CLT includes members that can assist in outreach to programs serving children and families who may be historically marginalized and increase the diversity of children and families who receive services from Pyramid Model programs.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator2" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQCWLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 2 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>CLTMT</td>
                                        <td>3</td>
                                        <td>A team member has been identified as the Community Leadership Team Coordinator. A process has been established for the identification of a new coordinator when needed.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator3" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQCWLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 3 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>CLTMT</td>
                                        <td>4</td>
                                        <td>The CLT has established a clearly written mission that addresses the community-wide implementation of the Pyramid Model. Team members can clearly communicate the purpose of the CLT.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator4" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQCWLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 4 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>CLTMT</td>
                                        <td>5</td>
                                        <td>Members of the CLT have clear roles and responsibilities, including a data coordinator, for contributing to the functioning of the team and achievement of the mission. The CLT uses effective teaming strategies to ensure meetings are productive and builds a sense of ownership among all team members.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator5" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQCWLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 5 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>CLTMT</td>
                                        <td>6</td>
                                        <td>The CLT establishes a process for the recruitment of diverse members and orientation of new members.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator6" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQCWLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 6 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>CLTMT</td>
                                        <td>7</td>
                                        <td>The CLT develops a written action plan that addresses all critical elements and guides the work of the team. This plan includes strategies for sustainability and scale-up for community-wide implementation of the Pyramid Model. The team reviews the plan and updates their progress at each meeting.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator7" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQCWLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 1 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>CLTMT</td>
                                        <td>8</td>
                                        <td>The CLT meets at least monthly.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator8" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQCWLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 8 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>FD</td>
                                        <td>9</td>
                                        <td>The CLT identifies funding sources to cover activities for at least three years including funds that are cost-shared, braided, layered, or from coordinated resources.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator9" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQCWLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 9 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>FD</td>
                                        <td>10</td>
                                        <td>The CLT identifies the fiscal resources needed to support new implementation sites including additional program implementation coaches for sustainability, and scale-up.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator10" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQCWLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 10 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>FD</td>
                                        <td>11</td>
                                        <td>The CLT identifies resources to support implementation sites to attend training, purchase materials, and assist with other expenses related to implementation.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator11" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQCWLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 11 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>FD</td>
                                        <td>12</td>
                                        <td>The CLT considers the needs of low-resource programs and identifies strategies and resources for supporting their participation.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator12" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQCWLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 12 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>CV</td>
                                        <td>13</td>
                                        <td>The CLT develops and provides awareness presentations to recruit early childhood programs (e.g., centers, schools, family childcare homes) to become implementation sites.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator13" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQCWLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 13 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>CV</td>
                                        <td>14</td>
                                        <td>The CLT engages in community outreach to programs and neighborhoods that serve children and families from historically marginalized groups. Outreach efforts include building partnerships with community leaders and using cultural brokering strategies to increase participation from these programs.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator14" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQCWLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 14 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>CV</td>
                                        <td>15</td>
                                        <td>Dissemination strategies are implemented to ensure that diverse stakeholders and communities are kept aware of activities and accomplishments (e.g., website, newsletter, conferences).</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator15" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQCWLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 15 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>CV</td>
                                        <td>16</td>
                                        <td>The CLT develops a written communication process for regular feedback from staff who are charged with Program-Wide Pyramid Model Implementation, including program implementation coaches and implementation sites.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator16" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQCWLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 16 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>IDS</td>
                                        <td>17</td>
                                        <td>The CLT establishes readiness criteria that are used in the recruitment and selection of new implementation sites.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator17" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQCWLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 17 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>IDS</td>
                                        <td>18</td>
                                        <td>The CLT implements a process to identify and select new implementation sites. This process should include an intentional plan to expand the diversity of programs that are included and recruit programs that serve children and communities that are historically marginalized.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator18" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQCWLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 18 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>IDS</td>
                                        <td>19</td>
                                        <td>CLT develops formal agreements for programs participating in the initiative as implementation sites. The agreement includes the criteria for a Program Leadership team, at least one practitioner coach, and the collection of data.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator19" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQCWLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 19 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>IDS</td>
                                        <td>20</td>
                                        <td>CLT has a recruitment and selection process for demonstration sites and partners with them to provide data that demonstrate effectiveness of Program-Wide Pyramid Model Implementation. These sites serve as a model for interested community programs, policy makers and other stakeholders and support scale-up of Pyramid Model efforts.  Demonstration sites are selected from implementation sites.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator20" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQCWLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 20 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>IDS</td>
                                        <td>21</td>
                                        <td>The CLT establishes a recruitment schedule for new programs to expand the number of implementation sites in the community.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator21" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQCWLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 21 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>FM</td>
                                        <td>22</td>
                                        <td>The CLT provides guidance and information to programs on effective family engagement and partnerships that includes guidance on equity, anti-racist, and anti-bias practices.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator22" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQCWLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 22 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>FM</td>
                                        <td>23</td>
                                        <td>The CLT develops mechanisms for family members to provide feedback at least annually on the quality of Pyramid Model implementation experienced by their children.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator23" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQCWLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 23 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>BS</td>
                                        <td>24</td>
                                        <td>The CLT develops guidance or a policy statement for community programs to eliminate the use of exclusionary and harsh discipline practices and encourage the use of the Pyramid Model to promote social and emotional skill development and prevent challenging behavior.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator24" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQCWLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 24 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>BS</td>
                                        <td>25</td>
                                        <td>The CLT identifies community resources for the provision of behavior supports, mental health services, and other specialized services that might be needed to assist children with social, emotional, and behavioral support needs and their families.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator25" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQCWLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 25 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>BS</td>
                                        <td>26</td>
                                        <td>The CLT establishes a process for implementation sites to access assistance for the provision of behavior supports for children with persistent challenging behavior.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator26" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQCWLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 26 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>PD</td>
                                        <td>27</td>
                                        <td>The CLT uses a professional development network of program implementation coaches who work directly with program leadership teams for program-wide implementation of the Pyramid Model.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator27" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQCWLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 27 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>PD</td>
                                        <td>28</td>
                                        <td>The CLT ensures that the professional development network of program implementation coaches have training and technical assistance competence in the Pyramid Model, collaborative teaming, practice-based coaching, data decision-making, culturally responsive practices and addressing implicit bias in early childhood programs and classrooms.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator28" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQCWLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 28 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>PD</td>
                                        <td>29</td>
                                        <td>The CLT supports the provision of training in Pyramid Model practices, anti-bias and culturally responsive practices, the inclusion of children with or at-risk for disabilities, practice-based coaching, behavior support facilitation, data decision-making tools, and training program-wide leadership teams in the implementation process.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator29" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQCWLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 29 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>PD</td>
                                        <td>30</td>
                                        <td>A program implementation coach is available to meet regularly with each emerging program leadership team and as needed with established teams.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator30" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQCWLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 30 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>PD</td>
                                        <td>31</td>
                                        <td>The CLT provides refresher trainings and opportunities for networking with peers from existing teams.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator31" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQCWLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 31 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>PD</td>
                                        <td>32</td>
                                        <td>The CLT guides implementation sites in identifying resources to support the provision of practice-based coaching to practitioners for the high-fidelity implementation and sustainability of the Pyramid Model.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator32" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQCWLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 32 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>MIO</td>
                                        <td>33</td>
                                        <td>Training, materials, and support are available to program implementation coaches and implementation sites on the data collection process, data submission, data analysis, and data-based decision-making for improving outcomes for children, families, practitioners, programs, and communities.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator33" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQCWLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 33 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>MIO</td>
                                        <td>34</td>
                                        <td>The CLT develops and implements a process for gathering data from participating sites on their fidelity of implementation and outcomes.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator34" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQCWLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 34 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>MIO</td>
                                        <td>35</td>
                                        <td>The CLT establishes a process to systematically collect and review data on the use of exclusionary or harsh discipline practices by participating sites and receives data from sites that is disaggregated by gender, race, ethnicity, DLL, and IEP status.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator35" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQCWLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 35 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>MIO</td>
                                        <td>36</td>
                                        <td>Implementation site data are reviewed to target future community-wide professional development needs.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator36" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQCWLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 36 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>MIO</td>
                                        <td>37</td>
                                        <td>The CLT action plan is updated as needed based on the ongoing data-based outcomes to support scale-up and sustain of Pyramid Model implementation.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator37" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQCWLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 37 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>MIO</td>
                                        <td>38</td>
                                        <td>The CLT prepares an annual evaluation report that includes a summary on the extent to which program-wide implementation is being achieved and sustained, the impact of program-wide implementation on child, practitioner, and program outcomes, and the impact of training and coaching.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator38" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQCWLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 38 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td>MIO</td>
                                        <td>39</td>
                                        <td>The CLT engages in celebration and acknowledgement of outcomes and accomplishments annually with community stakeholders and implementation sites.</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator39" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQCWLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 39 is required!" />
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
                                            <label class="float-right">Total In Place</label>
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
                <uc:Submit ID="submitBOQCWLT" runat="server" ValidationGroup="vgBOQCWLT"
                    ControlCssClass="center-content"
                    OnSubmitClick="submitBOQCWLT_Click" OnCancelClick="submitBOQCWLT_CancelClick"
                    OnValidationFailed="submitBOQCWLT_ValidationFailed" />
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
