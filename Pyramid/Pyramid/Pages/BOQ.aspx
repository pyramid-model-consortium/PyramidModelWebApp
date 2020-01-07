<%@ Page Title="Benchmarks of Quality 2.0" Language="C#" MasterPageFile="~/MasterPages/Dashboard.master" AutoEventWireup="true" CodeBehind="BOQ.aspx.cs" Inherits="Pyramid.Pages.BOQ" %>

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
            $('[ID$="lnkBOQDashboard"]').addClass('active');

            //Allow date format for DataTables sorting
            $.fn.dataTable.moment('MM/DD/YYYY');

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

            //Show/hide the view only fields
            setViewOnlyVisibility();
        }
    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Label ID="lblPageTitle" runat="server" Text="Benchmarks of Quality" CssClass="h2"></asp:Label>
    <hr />
    <asp:HiddenField ID="hfViewOnly" runat="server" Value="False" />
    <asp:UpdatePanel ID="upMessaging" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <uc:Messaging ID="msgSys" runat="server" />
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="submitBOQ" />
        </Triggers>
    </asp:UpdatePanel>
    <!--main content-->
    <div class="row">
        <div class="col-lg-12">
            <div class="card bg-light">
                <div class="card-header">
                    General Information
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
                                <ValidationSettings ValidationGroup="vgBOQ" ErrorDisplayMode="ImageWithText">
                                    <RequiredField IsRequired="true" ErrorText="Form Date is required!" />
                                </ValidationSettings>
                            </dx:BootstrapDateEdit>
                        </div>
                        <div class="col-md-4">
                            <dx:BootstrapMemo ID="txtTeamMembers" runat="server" Caption="Team Members" CaptionSettings-RequiredMarkDisplayMode="Hidden" CaptionSettings-ShowColon="false">
                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                <ValidationSettings ValidationGroup="vgBOQ" ErrorDisplayMode="ImageWithText">
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
                    <label>BOQ Critical Element Legend</label>
                    <div class="row">
                        <div class="col-sm-12 h5">
                            <span class="badge badge-info text-wrap mb-2">ELT = Establish Leadership Team</span>
                            <span class="badge badge-info text-wrap mb-2">SBI = Staff Buy-in</span>
                            <span class="badge badge-info text-wrap mb-2">FE = Family Engagement</span>
                            <span class="badge badge-info text-wrap mb-2">PWE = Program-Wide Expectations</span>
                            <span class="badge badge-info text-wrap mb-2">PDSSP = Professional Development and Staff Support Plan</span>
                            <span class="badge badge-info text-wrap mb-2">PRCB = Procedures for Responding to Challenging Behavior</span>
                            <span class="badge badge-info text-wrap mb-2">MIO = Monitoring Implementation and Outcomes</span>
                        </div>
                    </div>
                    <br />
                    <label>All BOQ Indicators</label>
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
                            <tr>
                                <td></td>
                                <td>ELT</td>
                                <td>1</td>
                                <td>Team has broad representation that includes at a minimum a teacher, administrator, a member who will provide coaching to teachers, a member with expertise in behavior support and a family member. Other team members might include a teaching assistant, related service specialists, a community member, and other program personnel.</td>
                                <td>
                                    <dx:BootstrapComboBox ID="ddIndicator1" runat="server" Width="200px" NullText="--Select--"
                                        ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <Items>
                                            <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                        </Items>
                                        <ValidationSettings ValidationGroup="vgBOQ" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Indicator 1 is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td>ELT</td>
                                <td>2</td>
                                <td>Team has administrative support. Administrator attends meetings and trainings, is active in problem-solving to ensure the success of the initiative, and is visibly supportive of the adoption of the model.</td>
                                <td>
                                    <dx:BootstrapComboBox ID="ddIndicator2" runat="server" Width="200px" NullText="--Select--"
                                        ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <Items>
                                            <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                        </Items>
                                        <ValidationSettings ValidationGroup="vgBOQ" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Indicator 2 is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td>ELT</td>
                                <td>3</td>
                                <td>Team has regular meetings. Team meetings are scheduled at least 1x per month for a minimum of 1 hour. Team member attendance is consistent</td>
                                <td>
                                    <dx:BootstrapComboBox ID="ddIndicator3" runat="server" Width="200px" NullText="--Select--"
                                        ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <Items>
                                            <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                        </Items>
                                        <ValidationSettings ValidationGroup="vgBOQ" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Indicator 3 is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td>ELT</td>
                                <td>4</td>
                                <td>Team has established a clear mission/purpose. The team purpose or mission statement is written. Team members are able to clearly communicate the purpose of the leadership team.</td>
                                <td>
                                    <dx:BootstrapComboBox ID="ddIndicator4" runat="server" Width="200px" NullText="--Select--"
                                        ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <Items>
                                            <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                        </Items>
                                        <ValidationSettings ValidationGroup="vgBOQ" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Indicator 4 is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td>ELT</td>
                                <td>5</td>
                                <td>Program has a child discipline policy statement that includes the promotion of social and emotional skills, use of positive guidance and prevention approaches and eliminates the use of suspension and expulsion.</td>
                                <td>
                                    <dx:BootstrapComboBox ID="ddIndicator5" runat="server" Width="200px" NullText="--Select--"
                                        ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <Items>
                                            <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                        </Items>
                                        <ValidationSettings ValidationGroup="vgBOQ" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Indicator 5 is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td>ELT</td>
                                <td>6</td>
                                <td>Team develops an implementation plan that includes all critical elements. A written implementation plan guides the work of the team. The team reviews the plan and updates their progress at each meeting. Action steps are identified to ensure achievement of the goals.</td>
                                <td>
                                    <dx:BootstrapComboBox ID="ddIndicator6" runat="server" Width="200px" NullText="--Select--"
                                        ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <Items>
                                            <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                        </Items>
                                        <ValidationSettings ValidationGroup="vgBOQ" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Indicator 6 is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td>ELT</td>
                                <td>7</td>
                                <td>Team reviews and revises the plan at least annually.</td>
                                <td>
                                    <dx:BootstrapComboBox ID="ddIndicator7" runat="server" Width="200px" NullText="--Select--"
                                        ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <Items>
                                            <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                        </Items>
                                        <ValidationSettings ValidationGroup="vgBOQ" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Indicator 1 is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td>SBI</td>
                                <td>8</td>
                                <td>A staff poll is conducted in which at least 80% of staff indicate they are aware of and supportive of the need for a program wide effort for (a) addressing children's social emotional competence and challenging behavior, (b) using culturally responsive practices, and (c) addressing implicit bias.</td>
                                <td>
                                    <dx:BootstrapComboBox ID="ddIndicator8" runat="server" Width="200px" NullText="--Select--"
                                        ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <Items>
                                            <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                        </Items>
                                        <ValidationSettings ValidationGroup="vgBOQ" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Indicator 8 is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td>SBI</td>
                                <td>9</td>
                                <td>Staff input and feedback is obtained throughout the process - coffee break with the director, focus group, suggestion box. Leadership team provides update on the process and data on the outcomes to program staff on a regular basis.</td>
                                <td>
                                    <dx:BootstrapComboBox ID="ddIndicator9" runat="server" Width="200px" NullText="--Select--"
                                        ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <Items>
                                            <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                        </Items>
                                        <ValidationSettings ValidationGroup="vgBOQ" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Indicator 9 is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td>FE</td>
                                <td>10</td>
                                <td>Family input is solicited as part of the planning and decision-making process. Families are informed of the initiative and asked to provide feedback on program-wide adoption and mechanisms for promoting family involvement in the initiative (e.g., suggestions box, focus group).</td>
                                <td>
                                    <dx:BootstrapComboBox ID="ddIndicator10" runat="server" Width="200px" NullText="--Select--"
                                        ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <Items>
                                            <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                        </Items>
                                        <ValidationSettings ValidationGroup="vgBOQ" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Indicator 10 is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td>FE</td>
                                <td>11</td>
                                <td>There are multiple mechanisms for sharing the program wide plan with families including narrative documents, conferences, and parent meetings to ensure that all families are informed of the initiative.</td>
                                <td>
                                    <dx:BootstrapComboBox ID="ddIndicator11" runat="server" Width="200px" NullText="--Select--"
                                        ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <Items>
                                            <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                        </Items>
                                        <ValidationSettings ValidationGroup="vgBOQ" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Indicator 11 is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td>FE</td>
                                <td>12</td>
                                <td>Family involvement in the initiative is supported through a variety of mechanisms including home teaching suggestions, information on supporting social development, and the outcomes of the initiative. Information is shared through a variety of formats (e.g., meetings, home visit discussions, newsletters in multiple languages, open house, websites, family friendly handouts, workshops, rollout events, access to staff with bilingual capacity).</td>
                                <td>
                                    <dx:BootstrapComboBox ID="ddIndicator12" runat="server" Width="200px" NullText="--Select--"
                                        ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <Items>
                                            <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                        </Items>
                                        <ValidationSettings ValidationGroup="vgBOQ" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Indicator 12 is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td>FE</td>
                                <td>13</td>
                                <td>Families are involved in planning for individual children in a meaningful and proactive way. Families are encouraged to team with program staff in the development of individualized plans of support for children including the development of strategies that may be used in the home and community.</td>
                                <td>
                                    <dx:BootstrapComboBox ID="ddIndicator13" runat="server" Width="200px" NullText="--Select--"
                                        ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <Items>
                                            <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                        </Items>
                                        <ValidationSettings ValidationGroup="vgBOQ" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Indicator 13 is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td>PWE</td>
                                <td>14</td>
                                <td>2-5 positively stated program wide expectations are developed</td>
                                <td>
                                    <dx:BootstrapComboBox ID="ddIndicator14" runat="server" Width="200px" NullText="--Select--"
                                        ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <Items>
                                            <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                        </Items>
                                        <ValidationSettings ValidationGroup="vgBOQ" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Indicator 14 is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td>PWE</td>
                                <td>15</td>
                                <td>Expectations are written in a way that applies to both children and staff. When expectations are discussed, the application of expectations to program staff and children is acknowledged.</td>
                                <td>
                                    <dx:BootstrapComboBox ID="ddIndicator15" runat="server" Width="200px" NullText="--Select--"
                                        ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <Items>
                                            <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                        </Items>
                                        <ValidationSettings ValidationGroup="vgBOQ" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Indicator 15 is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td>PWE</td>
                                <td>16</td>
                                <td>Expectations are developmentally appropriate and linked to concrete rules for behavior within activities or settings.</td>
                                <td>
                                    <dx:BootstrapComboBox ID="ddIndicator16" runat="server" Width="200px" NullText="--Select--"
                                        ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <Items>
                                            <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                        </Items>
                                        <ValidationSettings ValidationGroup="vgBOQ" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Indicator 16 is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td>PWE</td>
                                <td>17</td>
                                <td>Program staff and families are involved in the identification of the program-wide expectations that address needs, cultural norms and values of the program and community.</td>
                                <td>
                                    <dx:BootstrapComboBox ID="ddIndicator17" runat="server" Width="200px" NullText="--Select--"
                                        ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <Items>
                                            <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                        </Items>
                                        <ValidationSettings ValidationGroup="vgBOQ" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Indicator 17 is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td>PWE</td>
                                <td>18</td>
                                <td>Expectations are shared with families and staff assist families in the translation of the expectations to rules in the home.</td>
                                <td>
                                    <dx:BootstrapComboBox ID="ddIndicator18" runat="server" Width="200px" NullText="--Select--"
                                        ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <Items>
                                            <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                        </Items>
                                        <ValidationSettings ValidationGroup="vgBOQ" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Indicator 18 is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td>PWE</td>
                                <td>19</td>
                                <td>Expectations are posted in classrooms and in common areas in ways that are meaningful to children, staff and families.</td>
                                <td>
                                    <dx:BootstrapComboBox ID="ddIndicator19" runat="server" Width="200px" NullText="--Select--"
                                        ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <Items>
                                            <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                        </Items>
                                        <ValidationSettings ValidationGroup="vgBOQ" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Indicator 19 is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td>PWE</td>
                                <td>20</td>
                                <td>Strategies for acknowledging children’s use of the expectations are developmentally appropriate and used by all program staff including administrative and support staff (e.g., clerical, bus drivers, kitchen staff).</td>
                                <td>
                                    <dx:BootstrapComboBox ID="ddIndicator20" runat="server" Width="200px" NullText="--Select--"
                                        ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <Items>
                                            <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                        </Items>
                                        <ValidationSettings ValidationGroup="vgBOQ" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Indicator 20 is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td>PDSSP</td>
                                <td>21</td>
                                <td>A plan for providing ongoing support, training, and coaching in each classroom on the Pyramid Model including culturally responsive practices and implicit bias is developed and implemented.</td>
                                <td>
                                    <dx:BootstrapComboBox ID="ddIndicator21" runat="server" Width="200px" NullText="--Select--"
                                        ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <Items>
                                            <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                        </Items>
                                        <ValidationSettings ValidationGroup="vgBOQ" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Indicator 21 is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td>PDSSP</td>
                                <td>22</td>
                                <td>Practice-based coaching is used to assist classroom staff with implementing the Pyramid Model practices to fidelity.</td>
                                <td>
                                    <dx:BootstrapComboBox ID="ddIndicator22" runat="server" Width="200px" NullText="--Select--"
                                        ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <Items>
                                            <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                        </Items>
                                        <ValidationSettings ValidationGroup="vgBOQ" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Indicator 22 is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td>PDSSP</td>
                                <td>23</td>
                                <td>Staff responsible for facilitating behavior support processes are identified and trained.</td>
                                <td>
                                    <dx:BootstrapComboBox ID="ddIndicator23" runat="server" Width="200px" NullText="--Select--"
                                        ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <Items>
                                            <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                        </Items>
                                        <ValidationSettings ValidationGroup="vgBOQ" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Indicator 23 is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td>PDSSP</td>
                                <td>24</td>
                                <td>A needs assessment and/or observation tool is used to determine training needs on Pyramid Model practices.</td>
                                <td>
                                    <dx:BootstrapComboBox ID="ddIndicator24" runat="server" Width="200px" NullText="--Select--"
                                        ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <Items>
                                            <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                        </Items>
                                        <ValidationSettings ValidationGroup="vgBOQ" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Indicator 24 is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td>PDSSP</td>
                                <td>25</td>
                                <td>All teachers have an individualized professional development or action plan related to implementing Pyramid Model and culturally responsive practices with fidelity.</td>
                                <td>
                                    <dx:BootstrapComboBox ID="ddIndicator25" runat="server" Width="200px" NullText="--Select--"
                                        ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <Items>
                                            <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                        </Items>
                                        <ValidationSettings ValidationGroup="vgBOQ" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Indicator 25 is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td>PDSSP</td>
                                <td>26</td>
                                <td>A process for training new staff in Pyramid Model and culturally responsive practices is developed.</td>
                                <td>
                                    <dx:BootstrapComboBox ID="ddIndicator26" runat="server" Width="200px" NullText="--Select--"
                                        ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <Items>
                                            <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                        </Items>
                                        <ValidationSettings ValidationGroup="vgBOQ" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Indicator 26 is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td>PDSSP</td>
                                <td>27</td>
                                <td>Incentives and strategies for acknowledging staff effort in the implementation of Pyramid Model practices are implemented.</td>
                                <td>
                                    <dx:BootstrapComboBox ID="ddIndicator27" runat="server" Width="200px" NullText="--Select--"
                                        ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <Items>
                                            <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                        </Items>
                                        <ValidationSettings ValidationGroup="vgBOQ" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Indicator 27 is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td>PRCB</td>
                                <td>28</td>
                                <td>Teachers have received training related to potential bias when responding to behavior challenges and have strategies to reflect on their responses to individual children.</td>
                                <td>
                                    <dx:BootstrapComboBox ID="ddIndicator28" runat="server" Width="200px" NullText="--Select--"
                                        ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <Items>
                                            <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                        </Items>
                                        <ValidationSettings ValidationGroup="vgBOQ" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Indicator 28 is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td>PRCB</td>
                                <td>29</td>
                                <td>Program staff respond to children’s problem behavior appropriately using evidence-based approaches that are positive, sensitive to family values, culture and home language, and provide the child with guidance about the desired appropriate behavior and program-wide expectations.</td>
                                <td>
                                    <dx:BootstrapComboBox ID="ddIndicator29" runat="server" Width="200px" NullText="--Select--"
                                        ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <Items>
                                            <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                        </Items>
                                        <ValidationSettings ValidationGroup="vgBOQ" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Indicator 29 is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td>PRCB</td>
                                <td>30</td>
                                <td>A process for responding to crisis situations related to problem behavior is developed. Teachers can identify how to request assistance when needed. A plan for addressing the child’s individual behavior support needs is initiated following requests for crisis assistance.</td>
                                <td>
                                    <dx:BootstrapComboBox ID="ddIndicator30" runat="server" Width="200px" NullText="--Select--"
                                        ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <Items>
                                            <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                        </Items>
                                        <ValidationSettings ValidationGroup="vgBOQ" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Indicator 30 is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td>PRCB</td>
                                <td>31</td>
                                <td>Teachers have opportunities to problem solve with colleagues and family members around problem behavior. Teachers are encouraged to gain support in developing ideas for addressing problem behavior within the classroom (e.g., peer-support, classroom mentor meeting, brainstorming session).</td>
                                <td>
                                    <dx:BootstrapComboBox ID="ddIndicator31" runat="server" Width="200px" NullText="--Select--"
                                        ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <Items>
                                            <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                        </Items>
                                        <ValidationSettings ValidationGroup="vgBOQ" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Indicator 31 is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td>PRCB</td>
                                <td>32</td>
                                <td>A team-based process for addressing individual children with persistent challenging behavior is developed. Teachers can identify the steps for initiating the team-based process including fostering the participation of the family in the process.</td>
                                <td>
                                    <dx:BootstrapComboBox ID="ddIndicator32" runat="server" Width="200px" NullText="--Select--"
                                        ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <Items>
                                            <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                        </Items>
                                        <ValidationSettings ValidationGroup="vgBOQ" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Indicator 32 is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td>PRCB</td>
                                <td>33</td>
                                <td>An individual or individuals with behavioral expertise are identified for coaching staff and families throughout the process of developing and implementing individualized intensive interventions for children in need of behavior support plans.</td>
                                <td>
                                    <dx:BootstrapComboBox ID="ddIndicator33" runat="server" Width="200px" NullText="--Select--"
                                        ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <Items>
                                            <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                        </Items>
                                        <ValidationSettings ValidationGroup="vgBOQ" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Indicator 33 is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td>PRCB</td>
                                <td>34</td>
                                <td>Strategies for partnering with families when there are problem behavior concerns are identified. Teachers have strategies for initiating parent contact and partnering with the family to develop strategies to promote appropriate behavior.</td>
                                <td>
                                    <dx:BootstrapComboBox ID="ddIndicator34" runat="server" Width="200px" NullText="--Select--"
                                        ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <Items>
                                            <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                        </Items>
                                        <ValidationSettings ValidationGroup="vgBOQ" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Indicator 34 is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td>MIO</td>
                                <td>35</td>
                                <td>Data are collected, summarized with visual displays, and reviewed by the leadership team on a regular basis.</td>
                                <td>
                                    <dx:BootstrapComboBox ID="ddIndicator35" runat="server" Width="200px" NullText="--Select--"
                                        ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <Items>
                                            <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                        </Items>
                                        <ValidationSettings ValidationGroup="vgBOQ" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Indicator 35 is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td>MIO</td>
                                <td>36</td>
                                <td>The program leadership team monitors implementation fidelity of the components of program-wide implementation and uses data for decision making about their implementation goals.</td>
                                <td>
                                    <dx:BootstrapComboBox ID="ddIndicator36" runat="server" Width="200px" NullText="--Select--"
                                        ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <Items>
                                            <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                        </Items>
                                        <ValidationSettings ValidationGroup="vgBOQ" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Indicator 36 is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td>MIO</td>
                                <td>37</td>
                                <td>The program measures implementation fidelity of the use of Pyramid Model practices by classroom teachers and uses data on implementation fidelity to make decisions about professional development and coaching support.</td>
                                <td>
                                    <dx:BootstrapComboBox ID="ddIndicator37" runat="server" Width="200px" NullText="--Select--"
                                        ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <Items>
                                            <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                        </Items>
                                        <ValidationSettings ValidationGroup="vgBOQ" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Indicator 37 is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td>MIO</td>
                                <td>38</td>
                                <td>The program collects data on behavior incidents and program actions in response to behavior and uses those data to address child and teacher support needs.</td>
                                <td>
                                    <dx:BootstrapComboBox ID="ddIndicator38" runat="server" Width="200px" NullText="--Select--"
                                        ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <Items>
                                            <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                        </Items>
                                        <ValidationSettings ValidationGroup="vgBOQ" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Indicator 38 is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td>MIO</td>
                                <td>39</td>
                                <td>Behavior incident and monthly program action data are analyzed on a regular basis to identify potential issues related to disciplinary action bias.</td>
                                <td>
                                    <dx:BootstrapComboBox ID="ddIndicator39" runat="server" Width="200px" NullText="--Select--"
                                        ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <Items>
                                            <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                        </Items>
                                        <ValidationSettings ValidationGroup="vgBOQ" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Indicator 39 is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td>MIO</td>
                                <td>40</td>
                                <td>Program-level data are summarized and shared with program staff and families on a regular basis.</td>
                                <td>
                                    <dx:BootstrapComboBox ID="ddIndicator40" runat="server" Width="200px" NullText="--Select--"
                                        ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <Items>
                                            <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                        </Items>
                                        <ValidationSettings ValidationGroup="vgBOQ" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Indicator 40 is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                                <td>MIO</td>
                                <td>41</td>
                                <td>Data are used for ongoing monitoring, problem solving, ensuring child response to intervention, and program improvement.</td>
                                <td>
                                    <dx:BootstrapComboBox ID="ddIndicator41" runat="server" Width="200px" NullText="--Select--"
                                        ValueType="System.Int32" IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <Items>
                                            <dx:BootstrapListEditItem Value="0" Text="Not In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="1" Text="Partially In Place"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Value="2" Text="In Place"></dx:BootstrapListEditItem>
                                        </Items>
                                        <ValidationSettings ValidationGroup="vgBOQ" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Indicator 41 is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    </div>
    <div class="page-footer">
        <uc:Submit ID="submitBOQ" runat="server" ValidationGroup="vgBOQ" OnSubmitClick="submitBOQ_Click" OnCancelClick="submitBOQ_CancelClick" OnValidationFailed="submitBOQ_ValidationFailed" />
    </div>
</asp:Content>
