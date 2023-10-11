<%@ Page Title="State Leadership Team BOQ" Language="C#" MasterPageFile="~/MasterPages/Dashboard.master" AutoEventWireup="true" CodeBehind="BOQSLT.aspx.cs" Inherits="Pyramid.Pages.BOQSLT" %>

<%@ Register Assembly="DevExpress.Web.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Data.Linq" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.Bootstrap.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>
<%@ Register TagPrefix="uc" TagName="Messaging" Src="~/User_Controls/MessagingSystem.ascx" %>
<%@ Register TagPrefix="uc" TagName="Submit" Src="~/User_Controls/Submit.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style type="text/css">
        .group-header {
            font-weight: bold;
            text-align: center;
        }

        .group-slt {
            background-color: #fde9d9 !important;
        }

        .group-fe {
            background-color: #daeef3 !important;
        }

        .group-idps {
            background-color: #e5dfec !important;
        }

        .group-pd {
            background-color: #eaf1dd !important;
        }

        .group-eddm {
            background-color: #f2dbdb !important;
        }
    </style>
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
            $('[ID$="lnkSLTDashboard"]').addClass('active');

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
                        { orderable: false, targets: [4, 5] },
                        { className: 'control', orderable: false, targets: 0 },
                        { visible: false, targets: 1 }
                    ],
                    order: [[3, 'asc']],
                    rowGroup: {
                        dataSrc: 1,
                        className: 'group-header',
                        startRender: function (rows, group) {
                            //This is for the group headers at the start of the group
                            //Get the class names from the group content
                            var className = $(group).attr('class');

                            //Create the return object with the proper class and content
                            var returnValue = '<tr class="' + className + '"><td colspan="5">' + group + '</td></tr>';

                            //Return the object
                            return $(returnValue);
                        }
                    },
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
    <asp:Label ID="lblPageTitle" runat="server" Text="State Leadership Team Benchmarks of Quality" CssClass="h2"></asp:Label>
    <hr />
    <asp:HiddenField ID="hfViewOnly" runat="server" Value="False" />
    <asp:UpdatePanel ID="upBOQSLT" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <uc:Messaging ID="msgSys" runat="server" />
            <asp:HiddenField ID="hfBOQSLTPK" runat="server" Value="" />
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
                                        SettingsBootstrap-RenderOption="primary" ValidationGroup="vgBOQSLT" data-validation-group="vgBOQSLT">
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
                                        <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Form Date is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapDateEdit>
                                </div>
                                <div class="col-md-4">
                                    <dx:BootstrapTagBox ID="tbTeamMembers" runat="server" Caption="Team Members"
                                        AllowCustomTags="false" TextField="MemberIDAndName" ValueField="SLTMemberPK">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
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
                                    <asp:Label runat="server" AssociatedControlID="lblStateName" CssClass="d-block">State</asp:Label>
                                    <asp:Label ID="lblStateName" runat="server" Text=""></asp:Label>
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
                                    <span class="badge badge-info text-wrap mb-2">SML = SLT Membership and Logistics</span>
                                    <span class="badge badge-info text-wrap mb-2">AP = Action Planning</span>
                                    <span class="badge badge-info text-wrap mb-2">SCS = SLT Coordination and Staffing</span>
                                    <span class="badge badge-info text-wrap mb-2">SF = SLT Funding</span>
                                    <span class="badge badge-info text-wrap mb-2">SCV = SLT Communication & Visibility</span>
                                    <span class="badge badge-info text-wrap mb-2">APCL = Authority, Priority, and Communication Linkages</span>
                                    <span class="badge badge-info text-wrap mb-2">FPC = Family Participation and Communication</span>
                                    <span class="badge badge-info text-wrap mb-2">IPS = Implementation Programs/Sites</span>
                                    <span class="badge badge-info text-wrap mb-2">DPS = Demonstration Programs/Sites</span>
                                    <span class="badge badge-info text-wrap mb-2">IC = Implementation Communities</span>
                                    <span class="badge badge-info text-wrap mb-2">PC = Program Coaches</span>
                                    <span class="badge badge-info text-wrap mb-2">OSTA = Ongoing Support and Technical Assistance</span>
                                    <span class="badge badge-info text-wrap mb-2">DBDM = Data-Based Decision Making</span>
                                </div>
                            </div>
                            <br />
                            <label>All Indicators</label>
                            <table class="table table-striped table-bordered table-hover" id="tblIndicators">
                                <thead>
                                    <tr>
                                        <th data-priority="1"></th>
                                        <th></th>
                                        <th>Critical Elements</th>
                                        <th data-priority="3">Indicator Number</th>
                                        <th>Benchmark of Quality</th>
                                        <th data-priority="2"></th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-slt">State Leadership Team (SLT)</span></td>
                                        <td>SML</td>
                                        <td>1</td>
                                        <td>The SLT has written criteria for membership which ensures broad representation from a range of stakeholders, programs, and agencies (e.g., early childhood special education, early intervention, higher education, Head Start, families, child care, mental health). [Planning Stage]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator1" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents Init="calculateIndicatorTotal" ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 1 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-slt">State Leadership Team (SLT)</span></td>
                                        <td>SML</td>
                                        <td>2</td>
                                        <td>The SLT establishes a clear, written mission/vision. [Planning Stage]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator2" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 2 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-slt">State Leadership Team (SLT)</span></td>
                                        <td>SML</td>
                                        <td>3</td>
                                        <td>State Leadership Team members are able to clearly communicate the vision and mission of the State Leadership Team. [Planning Stage]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator3" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 3 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-slt">State Leadership Team (SLT)</span></td>
                                        <td>SML</td>
                                        <td>4</td>
                                        <td>The SLT adopts written ground rules and logistics including criteria for membership, no substitutes at meetings, agreeing to decisions made in ones' absence, all agencies will share resources, all members attend Pyramid Model training, uses effective meeting strategies to ensure meetings are engaging and all members' voices are heard. [Planning Stage]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator4" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 4 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-slt">State Leadership Team (SLT)</span></td>
                                        <td>SML</td>
                                        <td>5</td>
                                        <td>The SLT records decisions from each SLT meeting. [Every Stage]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator5" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 5 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-slt">State Leadership Team (SLT)</span></td>
                                        <td>SML</td>
                                        <td>6</td>
                                        <td>The SLT evaluates each meeting and uses the data to improve meetings (see SLT Meeting Planning and Evaluation Package). [Every Stage]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator6" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 6 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-slt">State Leadership Team (SLT)</span></td>
                                        <td>SML</td>
                                        <td>7</td>
                                        <td>The SLT achieves consistent attendance and quality of meetings (75% average attendance over the year; and at least an average of 4 on the 5-point meeting evaluations). [Every Stage]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator7" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 1 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-slt">State Leadership Team (SLT)</span></td>
                                        <td>SML</td>
                                        <td>8</td>
                                        <td>The SLT meets at least monthly during Planning and Implementation Stages and as needed during the Scale-up Stage. [Every Stage]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator8" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 8 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-slt">State Leadership Team (SLT)</span></td>
                                        <td>SML</td>
                                        <td>9</td>
                                        <td>The SLT has a process in place for membership succession within their own agencies (replacing themselves) that ensures continued commitment, understanding, and progress of State Team work. [Sustainability planning, beginning with the Planning Stage]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator9" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 9 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-slt">State Leadership Team (SLT)</span></td>
                                        <td>SML</td>
                                        <td>10</td>
                                        <td>The SLT has process in place for orienting new members. [Beginning with Planning Stage]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator10" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 10 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-slt">State Leadership Team (SLT)</span></td>
                                        <td>AP</td>
                                        <td>11</td>
                                        <td>SLT develops an action plan that includes objectives related to all critical elements of these benchmarks. The action plan guides the work of the Team including designation of work groups, if necessary. The action plan has both short- and long-term objectives. [Every Stage]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator11" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 11 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-slt">State Leadership Team (SLT)</span></td>
                                        <td>AP</td>
                                        <td>12</td>
                                        <td>The SLT reviews the action plan and updates their progress at each meeting. The action plan has an evaluation component for each action item and the evaluation is reviewed at each meeting. [Every Stage]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator12" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 12 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-slt">State Leadership Team (SLT)</span></td>
                                        <td>AP</td>
                                        <td>13</td>
                                        <td>The SLT includes in the action plan sustainability and scale-up objectives and strategies for increasing the number of settings and services using the Pyramid Model with the goal of achieving statewide, high-fidelity implementation over time. [Every Stage]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator13" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 13 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-slt">State Leadership Team (SLT)</span></td>
                                        <td>AP</td>
                                        <td>14</td>
                                        <td>The SLT action plan includes strategies for institutionalizing and embedding the Pyramid Model into state infrastructures such as Quality Rating Systems and Early Learning Guidelines, etc. [Sustainability planning & Scale-up Stage]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator14" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 14 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-slt">State Leadership Team (SLT)</span></td>
                                        <td>AP</td>
                                        <td>15</td>
                                        <td>The SLT annually reviews its mission/vision statement, action-plan outcomes and other evaluation data, SLT membership, ground rules, and logistics, and makes revisions as necessary. The annual review includes a celebration of accomplishments. [Every Stage]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator15" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 15 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-slt">State Leadership Team (SLT)</span></td>
                                        <td>SCS</td>
                                        <td>16</td>
                                        <td>A SLT member serves as Team Coordinator or Chair (i.e., lead contact) to represent the Team and work with staff to facilitate the work of the SLT and to coordinate Practitioner and Program communication. [Planning Stage]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator16" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 16 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-slt">State Leadership Team (SLT)</span></td>
                                        <td>SCS</td>
                                        <td>17</td>
                                        <td>The Pyramid Model initiative and SLT are supported by staff funded to implement the work. [Beginning with Initial Implementation Stage]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator17" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 17 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-slt">State Leadership Team (SLT)</span></td>
                                        <td>SCS</td>
                                        <td>18</td>
                                        <td>The SLT’s sustainability and scale-up planning (in action plan) includes adequate and appropriate professional and administrative staffing. [Beginning with Planning Stage]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator18" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 18 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-slt">State Leadership Team (SLT)</span></td>
                                        <td>SF</td>
                                        <td>19</td>
                                        <td>The SLT identifies funding sources to cover activities for at least three years including additional Program Coaches and sites. [Sustainability planning & Scale-up Stage]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator19" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 19 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-slt">State Leadership Team (SLT)</span></td>
                                        <td>SF</td>
                                        <td>20</td>
                                        <td>SLT members contribute resources for the work of the action plan (staffing, materials, training, etc.). [Every Stage]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator20" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 20 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-slt">State Leadership Team (SLT)</span></td>
                                        <td>SCV</td>
                                        <td>21</td>
                                        <td>The SLT develops an annual written report on the progress and outcome data and distributes it to programs, funders, and policy makers. [Beginning with Initial Implementation]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator21" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 21 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-slt">State Leadership Team (SLT)</span></td>
                                        <td>SCV</td>
                                        <td>22</td>
                                        <td>The SLT identifies and implements dissemination strategies to ensure that stakeholders are kept aware of activities and accomplishments (e.g., website, newsletter, conferences). [Every Stage]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator22" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 22 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-slt">State Leadership Team (SLT)</span></td>
                                        <td>SCV</td>
                                        <td>23</td>
                                        <td>The SLT develops a written awareness and marketing plan that includes a presentation (e.g., presentation based on annual data and report) to policy makers and current and potential funders. It is used to recruit programs and individuals to participate in the Pyramid Model initiative. [Initial Implementation Stage]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator23" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 23 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-slt">State Leadership Team (SLT)</span></td>
                                        <td>APCL</td>
                                        <td>24</td>
                                        <td>The Pyramid Model aligns with the goals and objectives of each agency represented on the SLT. [Every Stage]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator24" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 24 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-slt">State Leadership Team (SLT)</span></td>
                                        <td>APCL</td>
                                        <td>25</td>
                                        <td>Each SLT representative is authorized to make decisions for their agency related to the Pyramid Model initiative and/or is able to return a decision to the SLT within two-weeks. [Every Stage]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator25" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 25 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-slt">State Leadership Team (SLT)</span></td>
                                        <td>APCL</td>
                                        <td>26</td>
                                        <td>SLT members engage in activities within their agency that result in support for the Pyramid Model iitiative (e.g., succession planning, presenting annual reports, orientation presentations). [Every Stage]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator26" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 26 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-slt">State Leadership Team (SLT)</span></td>
                                        <td>APCL</td>
                                        <td>27</td>
                                        <td>The SLT develops written communication protocols for regular feedback from staff who are charged with implementing the Pyramid Model as well as the Program Coaches, demonstration sites, implementation sites, and communities. The protocols focus on bringing to light any challenges that need to be attended to by the SLT and that cannot be resolved by individual programs or staff. [Initial Implementation Stage]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator27" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 27 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-fe">Family Engagement</span></td>
                                        <td>FPC</td>
                                        <td>28</td>
                                        <td>The SLT includes representation from family organizations. [Planning Stage]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator28" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 28 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-fe">Family Engagement</span></td>
                                        <td>FPC</td>
                                        <td>29</td>
                                        <td>The SLT makes training opportunities related to the Pyramid Model available for families. [Every Stage]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator29" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 29 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-fe">Family Engagement</span></td>
                                        <td>FPC</td>
                                        <td>30</td>
                                        <td>The SLT develops and employs mechanisms for communicating with families about the initiative. [Every Stage]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator30" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 30 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-fe">Family Engagement</span></td>
                                        <td>FPC</td>
                                        <td>31</td>
                                        <td>The SLT develops mechanisms for family members to provide feedback at least annually on the quality of the Pyramid Model experienced by their children. [Every Stage]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator31" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 31 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-idps">Implementation and Demonstration Programs/Sites</span></td>
                                        <td>IPS</td>
                                        <td>32</td>
                                        <td>The SLT develops readiness criteria, recruitment and selection procedures, and MOUs for programs participating in the initiative as Implementation Programs/Sites. Implementation Programs/Sites have a Program Leadership Team and at least one Practitioner coach. [Initial Implementation Stage]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator32" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 32 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-idps">Implementation and Demonstration Programs/Sites</span></td>
                                        <td>DPS</td>
                                        <td>33</td>
                                        <td>The SLT has recruitment and selection process and MOUs for Demonstration Programs/Sites and partners with them to provide data that show the effectiveness of the Pyramid Model. The sites provide tours and information for interested parties. Demonstration sites are selected from the Implementation programs/sites. [Initial Implementation Stage]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator33" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 33 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-idps">Implementation and Demonstration Programs/Sites</span></td>
                                        <td>IC</td>
                                        <td>34</td>
                                        <td>The SLT (where appropriate) develops readiness criteria, recruitment and acceptance procedures, and MOUs for community entities to participate in the initiative. All participating communities agree to have a Community Leadership Team and Program Coaches to support Program Leadership Teams and Practitioner Coaches. [Scale-up Stage]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator34" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 34 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-idps">Implementation and Demonstration Programs/Sites</span></td>
                                        <td>IC</td>
                                        <td>35</td>
                                        <td>The SLT develops statewide capacity (funding, staffing) for training and supporting new Program and Community Leadership Teams and Program Coaches in the high-fidelity adoption and implementation process while continuing to support the high fidelity of the original implementation and demonstration programs. [Implementation & Scale-up Stage]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator35" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 35 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-pd">Professional Development</span></td>
                                        <td>PC</td>
                                        <td>36</td>
                                        <td>The SLT establishes a statewide network of professional-development (PD) experts to build and sustain high-fidelity implementation to serve as Program Coaches and to support Practitioner Coaches. [Implementation Stage]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator36" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 36 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-pd">Professional Development</span></td>
                                        <td>PC</td>
                                        <td>37</td>
                                        <td>The SLT develops an identification process, recruitment and acceptance criteria, and MOUs for Program Coaches. [Initial Implementation Stage]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator37" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 37 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-pd">Professional Development</span></td>
                                        <td>PC</td>
                                        <td>38</td>
                                        <td>The SLT develops statewide Pyramid Model training capacity that includes providing ongoing training and support for Program Coaches who, in turn, train and support community and program staff and Leadership Teams. [Implementation Stage]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator38" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 38 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-pd">Professional Development</span></td>
                                        <td>PC</td>
                                        <td>39</td>
                                        <td>The SLT creates and puts in place a quality-assurance mechanism (e.g., certification, approval) to ensure that Program Coaches are able to provide training in the Pyramid Model accurately and effectively; and that Practitioner Coaches are able to coach practitioners to implementation fidelity resulting in success for children, families and providers. [Implementation Stage]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator39" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 39 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-pd">Professional Development</span></td>
                                        <td>PC</td>
                                        <td>40</td>
                                        <td>The SLT implements a plan ensuring that programs and communities statewide have access to Program Coaches, including necessary resources and on-site coaching that result in high-fidelity implementation and sustainability of the Pyramid Model. [Implementation Stage]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator40" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 40 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-pd">Professional Development</span></td>
                                        <td>OSTA</td>
                                        <td>41</td>
                                        <td>The SLT employs a technical-assistance plan for ongoing support and resources for the Program Coaches, demonstration sites, implementation sites and communities to ensure high-fidelity implementation and sustainability. Such support includes planning for turn-over and succession of key individuals. [Sustainability planning & Scale-up Stage]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator41" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 41 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-pd">Professional Development</span></td>
                                        <td>OSTA</td>
                                        <td>42</td>
                                        <td>A Program Coach is available to meet at least twice a month with each emerging Program Leadership Team (emerging teams are teams that have not met the high-fidelity implementation criteria) face to face or by distance. [Implementation Stage]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator42" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 42 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-pd">Professional Development</span></td>
                                        <td>OSTA</td>
                                        <td>43</td>
                                        <td>A Program Coach is available to meet at least monthly by distance and quarterly face to face with Program Leadership Teams who have been implementing the Pyramid Model for at least one year with high fidelity. [Sustainability planning]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator43" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 43 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-eddm">Evaluation/Data-Based Decision Making</span></td>
                                        <td>DBDM</td>
                                        <td>44</td>
                                        <td>All programs, communities, and Program Coaches submit the data agreed upon in their respective MOUs. [Implementation Stage & Sustainability planning]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator44" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 44 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-eddm">Evaluation/Data-Based Decision Making</span></td>
                                        <td>DBDM</td>
                                        <td>45</td>
                                        <td>Training, materials, and support are available to Program Coaches, programs, and communities on what data to collect, why, and how to use the data for making decisions for improving outcomes for children, providers, programs, and communities as well as how to submit the data. [Every Stage]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator45" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 45 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-eddm">Evaluation/Data-Based Decision Making</span></td>
                                        <td>DBDM</td>
                                        <td>46</td>
                                        <td>A process is in place for programs and communities to enter and summarize the data elements above as well as training on how to use the data for program improvement. [Every Stage]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator46" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 46 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-eddm">Evaluation/Data-Based Decision Making</span></td>
                                        <td>DBDM</td>
                                        <td>47</td>
                                        <td>A process is in place for the SLT to access the data or summaries of the data described above. The SLT uses these data as part of their action plan for regular evaluation as well as the annual evaluation report. [Every Stage]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator47" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 47 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-eddm">Evaluation/Data-Based Decision Making</span></td>
                                        <td>DBDM</td>
                                        <td>48</td>
                                        <td>The SLT annually prepares an evaluation report that describes: a) the extent to which program- and community-wide high-fidelity adoption is being implemented, sustained, and scaled-up; b) the impact of program-wide adoption and/or community-wide adoption on child, provider, and program outcomes; and c) the impact of training and coaching. The SLT uses the evaluation report for their own progress monitoring and planning as well as for providing a public report on outcomes. [Every Stage]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator48" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 48 is required!" />
                                                </ValidationSettings>
                                            </dx:BootstrapComboBox>
                                        </td>
                                    </tr>
                                    <tr class="indicator-row">
                                        <td></td>
                                        <td><span class="group-eddm">Evaluation/Data-Based Decision Making</span></td>
                                        <td>DBDM</td>
                                        <td>49</td>
                                        <td>The SLT provides a public celebration of outcomes and accomplishments annually. [Every Stage]</td>
                                        <td>
                                            <dx:BootstrapComboBox ID="ddIndicator49" runat="server" Width="280px" NullText="--Select--"
                                                ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <CssClasses Control="indicator-combo-box" />
                                                <ClientSideEvents ValueChanged="calculateIndicatorTotal" />
                                                <Items>
                                                    <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="1" Text="Emerging/Needs Improvement"></dx:BootstrapListEditItem>
                                                    <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                                </Items>
                                                <ValidationSettings ValidationGroup="vgBOQSLT" ErrorDisplayMode="ImageWithText">
                                                    <RequiredField IsRequired="true" ErrorText="Indicator 49 is required!" />
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
                <uc:Submit ID="submitBOQSLT" runat="server" ValidationGroup="vgBOQSLT"
                    ControlCssClass="center-content"
                    OnSubmitClick="submitBOQSLT_Click" OnCancelClick="submitBOQSLT_CancelClick"
                    OnValidationFailed="submitBOQSLT_ValidationFailed" />
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>