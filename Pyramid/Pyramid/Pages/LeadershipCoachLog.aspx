<%@ Page Title="Leadership Coach Log" Language="C#" MasterPageFile="~/MasterPages/Dashboard.master" AutoEventWireup="true" CodeBehind="LeadershipCoachLog.aspx.cs" Inherits="Pyramid.Pages.LeadershipCoachLog" %>

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
            $('[ID$="lnkLCDashboard"]').addClass('active');

            //Show/hide the view only fields
            setViewOnlyVisibility();

            //Set up the click events for the help buttons
            $('#btnLeadershipCoachHelp').popover({
                trigger: 'hover focus',
                title: 'Help',
                html: true,
                content: 'In order for a leadership coach to appear in this list, a program must be selected.<br/>' +
                    'Leadership Coaches will only be included if they have an account in PIDS with a Leadership Coach role that is associated with the selected program.'
            });
            $('#btnInvolvedCoachesHelp').popover({
                trigger: 'hover focus',
                title: 'Help',
                html: true,
                content: 'In order for coaches to appear in this list, a program must be selected, the type of log must be selected, and the date/month of the form must be entered.<br/>' +
                    'Coaches will only be included if they meet the following criteria:<br/> ' +
                    '<ol><li>They must have been entered into the Pyramid Model Professionals Dashboard.</li>' +
                    '<li>They must have started at the program prior to the date of this form.</li>' +
                    '<li>They must not have left the program as of the date of this form.</li>' +
                    '<li>They must have a classroom coach job function that is active as of the date of this form.</li></ol>'
            });
            $('#btnLeadershipTeamHelp').popover({
                trigger: 'hover focus',
                title: 'Help',
                html: true,
                content: 'In order for a team member to appear in this list, a program must be selected, the type of log must be selected, and the date/month of the form must be entered.<br/>' +
                    'Team members will only be included if they meet the following criteria:<br/> ' +
                    '<ol><li>They must have been entered into the Program Leadership Team Dashboard.</li>' +
                    '<li>They must have started at the program prior to the date of this form.</li>' +
                    '<li>They must not have left the program as of the date of this form.</li></ol>'
            });
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

        function showHideLogTypeDivs() {
            //Determine if this is a monthly log
            var isMonthlyLog = ddIsMonthly.GetValue();

            //Show/hide the type sections sections
            if (isMonthlyLog === true) {
                $('.type-monthly-div').removeClass('hidden');
                $('.type-single-div').addClass('hidden');
            }
            else if (isMonthlyLog === false) {
                $('.type-single-div').removeClass('hidden');
                $('.type-monthly-div').addClass('hidden');
            }
            else {
                $('.type-monthly-div').addClass('hidden');
                $('.type-single-div').addClass('hidden');
            }
        }

        function handleTeamMemberEngagement() {
            //Get the team members selected
            var itemsSelected = lbTeamMemberEngagement.GetSelectedValues();

            //Set the value of the hidden field
            $('[ID$="hfTeamMemberEngagement"]').val(itemsSelected);
        }

        function handleOtherSpecify(s, e) {
            //Get the controls
            var control = ASPxClientControl.Cast(s);
            var jQueryControl = $('#' + control.name);  //This is necessary for the siblings calls below
            var lblOtherCode = jQueryControl.siblings('.list-box-other-code-label');
            var specifyDiv = jQueryControl.siblings('.list-box-other-specify-div');

            //Get the specify code from the label
            var specifyCode = Number(lblOtherCode.text());

            //Get the control selected values (returns an array of objects)
            var selectedValues = control.GetSelectedValues();

            //Show/hide the specify div
            if (selectedValues !== null && selectedValues.length > 0) {
                if (selectedValues.includes(specifyCode)) {
                    specifyDiv.removeClass('hidden');
                }
                else {
                    specifyDiv.addClass('hidden');
                }
            }
            else {
                specifyDiv.addClass('hidden');
            }
        }
    </script>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Label ID="lblPageTitle" runat="server" Text="" CssClass="h2"></asp:Label>
    <hr />
    <asp:HiddenField ID="hfViewOnly" runat="server" Value="False" />
    <asp:UpdatePanel ID="upMessaging" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <uc:Messaging ID="msgSys" runat="server" />
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="submitLeadershipCoachLog" />
        </Triggers>
    </asp:UpdatePanel>
    <asp:UpdatePanel ID="upLCL" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
        <ContentTemplate>
            <asp:HiddenField ID="hfLeadershipCoachLogPK" runat="server" Value="" />
            <div class="row">
                <div class="col-lg-12">
                    <div class="card bg-light">
                        <div class="card-header">
                            <div class="row">
                                <div class="col-lg-8">
                                    Leadership Coach Log
                                </div>
                                <div class="col-lg-4">
                                    <dx:BootstrapButton ID="btnPrintPreview" runat="server" Text="Save and Download/Print" OnClick="btnPrintPreview_Click"
                                        SettingsBootstrap-RenderOption="primary" ValidationGroup="vgLCL" data-validation-group="vgLCL">
                                        <CssClasses Icon="fas fa-print" Control="float-right btn-loader" />
                                    </dx:BootstrapButton>
                                </div>
                            </div>
                        </div>
                        <div class="card-body">
                            <div class="row">
                                <div class="col-lg-4">
                                    <dx:BootstrapComboBox ID="ddProgram" runat="server" Caption="Program" NullText="--Select--"
                                        TextField="ProgramName" ValueField="ProgramPK" ValueType="System.Int32" AllowNull="true"
                                        IncrementalFilteringMode="Contains" AllowMouseWheel="false" AutoPostBack="true"
                                        OnValueChanged="ddProgram_ValueChanged">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgLCL" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                            <RequiredField IsRequired="true" ErrorText="Program is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </div>
                                <div class="col-lg-4">
                                    <asp:Label runat="server" AssociatedControlID="lblProgramIDNumber" CssClass="d-block col-form-label" Text="Program ID Number"></asp:Label>
                                    <asp:Label ID="lblProgramIDNumber" runat="server"></asp:Label>
                                </div>
                                <div class="col-lg-4">
                                    <asp:Label runat="server" AssociatedControlID="lblProgramTypes" CssClass="d-block col-form-label" Text="Program Type(s)"></asp:Label>
                                    <asp:Label ID="lblProgramTypes" runat="server"></asp:Label>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-4">
                                    <dx:BootstrapComboBox ID="ddLeadershipCoach" runat="server" Caption="Leadership Coach" 
                                        NullText="--Select--" ValueType="System.String" AllowNull="true"
                                        TextField="FullName" ValueField="UserName"
                                        IncrementalFilteringMode="Contains" AllowMouseWheel="false"
                                        ClientInstanceName="ddLeadershipCoach" OnValidation="ddLeadershipCoach_Validation">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgLCL" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Leadership Coach is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                    <button id="btnLeadershipCoachHelp" type="button" class="btn btn-link p-0"><i class="fas fa-question-circle"></i>&nbsp;Help</button>
                                </div>
                                <div class="col-lg-4">
                                    <dx:BootstrapListBox ID="lbInvolvedCoaches" runat="server" Caption="Classroom Coach(es)/Practice-Base Coach(es)"
                                        SelectionMode="CheckColumn" AllowCustomValues="true"
                                        TextField="EmployeeIDAndName" ValueField="ProgramEmployeePK" ValueType="System.Int32"
                                        OnValidation="lbInvolvedCoaches_Validation">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgLCL" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                            <RequiredField IsRequired="false" ErrorText="At least one option must be selected!" />
                                        </ValidationSettings>
                                    </dx:BootstrapListBox>
                                    <button id="btnInvolvedCoachesHelp" type="button" class="btn btn-link p-0"><i class="fas fa-question-circle"></i>&nbsp;Help</button>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-3">
                                    <dx:BootstrapComboBox ID="ddIsMonthly" runat="server" Caption="Type of Log" 
                                        NullText="--Select--" ValueType="System.Boolean" AllowNull="true"
                                        IncrementalFilteringMode="Contains" AllowMouseWheel="false"
                                        ClientInstanceName="ddIsMonthly">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ClientSideEvents Init="showHideLogTypeDivs" SelectedIndexChanged="showHideLogTypeDivs" />
                                        <Items>
                                            <dx:BootstrapListEditItem Text="Cumulative record of engagements over one month period" Value="True"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Text="Single engagement/encounter" Value="False"></dx:BootstrapListEditItem>
                                        </Items>
                                        <ValidationSettings ValidationGroup="vgLCL" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Type of Log is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </div>
                                <div class="col-lg-3 type-single-div hidden">
                                    <dx:BootstrapDateEdit ID="deDateCompleted" runat="server" Caption="Date Completed" EditFormat="Date"
                                        EditFormatString="MM/dd/yyyy" UseMaskBehavior="true" NullText="--Select--"
                                        AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900"
                                        AutoPostBack="true" OnValueChanged="LogDate_ValueChanged" OnValidation="deDateCompleted_Validation">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                        <ValidationSettings ValidationGroup="vgLCL" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                            <RequiredField IsRequired="false" ErrorText="Date Completed is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapDateEdit>
                                </div>
                                <div class="col-lg-3 type-single-div hidden">
                                    <dx:BootstrapTextBox ID="txtTotalDurationHours" runat="server" Caption="Total Duration (Hours)" MaxLength="2"
                                        OnValidation="txtTotalDurationHours_Validation">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgLCL" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                            <RequiredField IsRequired="false" ErrorText="Required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapTextBox>
                                    <dx:BootstrapTextBox ID="txtTotalDurationMinutes" runat="server" Caption="Total Duration (Minutes)" MaxLength="2"
                                        OnValidation="txtTotalDurationMinutes_Validation">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgLCL" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                            <RequiredField IsRequired="false" ErrorText="Required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapTextBox>
                                </div>
                                <div class="col-lg-3 type-monthly-div hidden">
                                    <dx:BootstrapDateEdit ID="deLCLMonth" runat="server" Caption="Month of log" EditFormat="Date"
                                        EditFormatString="MM/yyyy" PickerType="Months" UseMaskBehavior="true" NullText="--Select--"
                                        AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900"
                                        AutoPostBack="true" OnValueChanged="LogDate_ValueChanged" OnValidation="deLCLMonth_Validation">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                        <ValidationSettings ValidationGroup="vgLCL" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                            <RequiredField IsRequired="false" ErrorText="Month of log is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapDateEdit>
                                </div>
                                <div class="col-lg-3 type-monthly-div hidden">
                                    <dx:BootstrapTextBox ID="txtMonthlyNumEngagements" runat="server" Caption="Number of communication/engagements" MaxLength="2"
                                        OnValidation="txtMonthlyNumEngagements_Validation">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgLCL" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                            <RequiredField IsRequired="false" ErrorText="Number of communication/engagements is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapTextBox>
                                </div>
                                <div class="col-lg-3 type-monthly-div hidden">
                                    <dx:BootstrapTextBox ID="txtMonthlyNumAttemptedEngagements" runat="server" Caption="Number of attempted communication/engagements" MaxLength="2"
                                        OnValidation="txtMonthlyNumAttemptedEngagements_Validation">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgLCL" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                            <RequiredField IsRequired="false" ErrorText="Number of attempted communication/engagements is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapTextBox>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-4">
                                    <dx:BootstrapComboBox ID="ddCyclePhase" runat="server" Caption="What phase of implementation is the program currently in?" 
                                        NullText="--Select--" ValueType="System.Int32" AllowNull="true"
                                        IncrementalFilteringMode="Contains" AllowMouseWheel="false"
                                        ClientInstanceName="ddCyclePhase">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <Items>
                                            <dx:BootstrapListEditItem Text="1" Value="1"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Text="2" Value="2"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Text="3" Value="3"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Text="4" Value="4"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Text="5" Value="5"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Text="6" Value="6"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Text="7" Value="7"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Text="8" Value="8"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Text="9" Value="9"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Text="10" Value="10"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Text="Not applicable" Value="99"></dx:BootstrapListEditItem>
                                        </Items>
                                        <ValidationSettings ValidationGroup="vgLCL" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </div>
                                <div class="col-lg-4">
                                    <div>
                                        <dx:BootstrapComboBox ID="ddTimelyProgression" runat="server" Caption="For the phase you identified, what is the likelihood that the program will make progress on the BOQ critical elements and professional development activities for the phase in a timely way?" 
                                            NullText="--Select--" ValueType="System.Int32" AllowNull="true"
                                            TextField="Description" ValueField="CodeLCLResponsePK" 
                                            IncrementalFilteringMode="Contains" AllowMouseWheel="false">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ValidationSettings ValidationGroup="vgLCL" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                <RequiredField IsRequired="true" ErrorText="Required!" />
                                            </ValidationSettings>
                                        </dx:BootstrapComboBox>
                                    </div>
                                    <div class="mt-2 mb-2">
                                        <asp:Label runat="server" CssClass="d-block col-form-label" AssociatedControlID="lblMostRecentBOQDate" Text="Most Recent BOQ Date"></asp:Label>
                                        <asp:Label ID="lblMostRecentBOQDate" runat="server" Text=""></asp:Label>
                                    </div>
                                </div>
                                <div class="col-lg-4">
                                    <dx:BootstrapComboBox ID="ddGoalCompletionLikelihood" runat="server" Caption="What is the likelihood of completing most or all of the goals of the program's action plan within the expected time frames?" 
                                        NullText="--Select--" ValueType="System.Int32" AllowNull="true"
                                        TextField="Description" ValueField="CodeLCLResponsePK" 
                                        IncrementalFilteringMode="Contains" AllowMouseWheel="false">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgLCL" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                            <RequiredField IsRequired="true" ErrorText="Required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-8">
                                    <dx:BootstrapListBox ID="lbTeamMemberEngagement" runat="server" Caption="Leadership team members that had a communication/engagement"
                                        SelectionMode="CheckColumn" AllowCustomValues="true" EnableMultiColumn="true"
                                        ValueField="PLTMemberPK" ValueType="System.Int32" ClientInstanceName="lbTeamMemberEngagement"
                                        OnValidation="lbTeamMemberEngagement_Validation" Width="100%">
                                        <CssClasses Control="mw-100" />
                                        <ClientSideEvents Init="handleTeamMemberEngagement" SelectedIndexChanged="handleTeamMemberEngagement" />
                                        <Fields>
                                            <dx:BootstrapListBoxField FieldName="MemberIDAndName" SettingsMultiColumn-Width="100%" SettingsMultiColumn-Caption="ID and Name" />
                                            <dx:BootstrapListBoxField FieldName="EmailAddress" SettingsMultiColumn-Width="100%" SettingsMultiColumn-Caption="Email" />
                                            <dx:BootstrapListBoxField FieldName="Roles" SettingsMultiColumn-Width="100%" SettingsMultiColumn-Caption="Role(s)" />
                                            <dx:BootstrapListBoxField FieldName="StartDate" SettingsMultiColumn-Width="100%" SettingsMultiColumn-Caption="Start Date" />
                                        </Fields>
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgLCL" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="false" ErrorText="Required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapListBox>
                                    <asp:HiddenField ID="hfTeamMemberEngagement" runat="server" Value="" />
                                    <button id="btnLeadershipTeamHelp" type="button" class="btn btn-link p-0"><i class="fas fa-question-circle"></i>&nbsp;Help</button>
                                </div>
                                <div class="col-lg-4">
                                    <dx:BootstrapMemo ID="txtOtherEngagementSpecify" runat="server" Caption="Other individuals that had a communication/engagement" 
                                        MaxLength="250" Rows="2">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgLCL" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="false" ErrorText="Required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapMemo>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-4 mb-5">
                                    <dx:BootstrapListBox ID="lbDomain1" runat="server" Caption="Domain 1: Planning"
                                        SelectionMode="CheckColumn" AllowCustomValues="true"
                                        TextField="Description" ValueField="CodeLCLResponsePK" ValueType="System.Int32"
                                        OnValidation="lbDomain6_Validation">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgLCL" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                            <RequiredField IsRequired="true" ErrorText="At least one option must be selected!" />
                                        </ValidationSettings>
                                    </dx:BootstrapListBox>
                                </div>
                                <div class="col-lg-4 mb-5">
                                    <dx:BootstrapListBox ID="lbDomain2" runat="server" Caption="Domain 2: Meetings"
                                        SelectionMode="CheckColumn" AllowCustomValues="true"
                                        TextField="Description" ValueField="CodeLCLResponsePK" ValueType="System.Int32"
                                        OnValidation="lbDomain6_Validation">
                                        <CssClasses Control="list-box-other-control" />
                                        <ClientSideEvents Init="handleOtherSpecify" SelectedIndexChanged="handleOtherSpecify" />
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgLCL" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                            <RequiredField IsRequired="true" ErrorText="At least one option must be selected!" />
                                        </ValidationSettings>
                                    </dx:BootstrapListBox>
                                    <asp:Label ID="lblDomain2SpecifyCode" runat="server" CssClass="list-box-other-code-label hidden"></asp:Label>
                                    <div class="list-box-other-specify-div hidden">
                                        <dx:BootstrapMemo ID="txtOtherDomainTwoSpecify" runat="server" Caption="Specify" 
                                            MaxLength="250" Rows="2" OnValidation="txtOtherDomainTwoSpecify_Validation">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ValidationSettings ValidationGroup="vgLCL" ErrorDisplayMode="ImageWithText">
                                                <RequiredField IsRequired="false" ErrorText="Required!" />
                                            </ValidationSettings>
                                        </dx:BootstrapMemo>
                                    </div>
                                </div>
                                <div class="col-lg-4 mb-5">
                                    <dx:BootstrapListBox ID="lbDomain3" runat="server" Caption="Domain 3: Internal Coach/Program Support"
                                        SelectionMode="CheckColumn" AllowCustomValues="true"
                                        TextField="Description" ValueField="CodeLCLResponsePK" ValueType="System.Int32"
                                        OnValidation="lbDomain6_Validation">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgLCL" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                            <RequiredField IsRequired="true" ErrorText="At least one option must be selected!" />
                                        </ValidationSettings>
                                    </dx:BootstrapListBox>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-4 mb-5">
                                    <dx:BootstrapListBox ID="lbDomain4" runat="server" Caption="Domain 4: Behavior Support"
                                        SelectionMode="CheckColumn" AllowCustomValues="true"
                                        TextField="Description" ValueField="CodeLCLResponsePK" ValueType="System.Int32"
                                        OnValidation="lbDomain6_Validation">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgLCL" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                            <RequiredField IsRequired="true" ErrorText="At least one option must be selected!" />
                                        </ValidationSettings>
                                    </dx:BootstrapListBox>
                                </div>
                                <div class="col-lg-4 mb-5">
                                    <dx:BootstrapListBox ID="lbDomain5" runat="server" Caption="Domain 5: Family Support"
                                        SelectionMode="CheckColumn" AllowCustomValues="true"
                                        TextField="Description" ValueField="CodeLCLResponsePK" ValueType="System.Int32"
                                        OnValidation="lbDomain6_Validation">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgLCL" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                            <RequiredField IsRequired="true" ErrorText="At least one option must be selected!" />
                                        </ValidationSettings>
                                    </dx:BootstrapListBox>
                                </div>
                                <div class="col-lg-4 mb-5">
                                    <dx:BootstrapListBox ID="lbDomain6" runat="server" Caption="Domain 6: Data Support"
                                        SelectionMode="CheckColumn" AllowCustomValues="true"
                                        TextField="Description" ValueField="CodeLCLResponsePK" ValueType="System.Int32"
                                        OnValidation="lbDomain6_Validation">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgLCL" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                            <RequiredField IsRequired="true" ErrorText="At least one option must be selected!" />
                                        </ValidationSettings>
                                    </dx:BootstrapListBox>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-4 mb-5">
                                    <dx:BootstrapListBox ID="lbIdentifiedProgramStrengths" runat="server" Caption="Identified Program Strengths"
                                        SelectionMode="CheckColumn" AllowCustomValues="true"
                                        TextField="Description" ValueField="CodeLCLResponsePK" ValueType="System.Int32"
                                        OnValidation="lbIdentifiedProgramStrengths_Validation">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgLCL" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                            <RequiredField IsRequired="true" ErrorText="At least one option must be selected!" />
                                        </ValidationSettings>
                                    </dx:BootstrapListBox>
                                </div>
                                <div class="col-lg-4 mb-5">
                                    <dx:BootstrapListBox ID="lbIdentifiedProgramBarriers" runat="server" Caption="Identified Program Barriers"
                                        SelectionMode="CheckColumn" AllowCustomValues="true"
                                        TextField="Description" ValueField="CodeLCLResponsePK" ValueType="System.Int32"
                                        OnValidation="lbIdentifiedProgramBarriers_Validation">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgLCL" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                            <RequiredField IsRequired="true" ErrorText="At least one option must be selected!" />
                                        </ValidationSettings>
                                    </dx:BootstrapListBox>
                                </div>
                                <div class="col-lg-4 mb-5">
                                    <dx:BootstrapListBox ID="lbSiteResources" runat="server" Caption="Program has the following resources dedicated to data collection"
                                        SelectionMode="CheckColumn" AllowCustomValues="true"
                                        TextField="Description" ValueField="CodeLCLResponsePK" ValueType="System.Int32"
                                        OnValidation="lbSiteResources_Validation">
                                        <CssClasses Control="list-box-other-control" />
                                        <ClientSideEvents Init="handleOtherSpecify" SelectedIndexChanged="handleOtherSpecify" />
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgLCL" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                            <RequiredField IsRequired="true" ErrorText="At least one option must be selected!" />
                                        </ValidationSettings>
                                    </dx:BootstrapListBox>
                                    <asp:Label ID="lblSiteResourcesSpecifyCode" runat="server" CssClass="list-box-other-code-label hidden"></asp:Label>
                                    <div class="list-box-other-specify-div hidden">
                                        <dx:BootstrapMemo ID="txtOtherSiteResourcesSpecify" runat="server" Caption="Specify" 
                                            MaxLength="250" Rows="2" OnValidation="txtOtherSiteResourcesSpecify_Validation">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ValidationSettings ValidationGroup="vgLCL" ErrorDisplayMode="ImageWithText">
                                                <RequiredField IsRequired="false" ErrorText="Required!" />
                                            </ValidationSettings>
                                        </dx:BootstrapMemo>
                                    </div>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-4 mb-5">
                                    <dx:BootstrapListBox ID="lbTopicsDiscussed" runat="server" Caption="Which of the following equity-focused topics did you discuss with program leaders during this engagement/reporting period?"
                                        SelectionMode="CheckColumn" AllowCustomValues="true"
                                        TextField="Description" ValueField="CodeLCLResponsePK" ValueType="System.Int32"
                                        OnValidation="lbTopicsDiscussed_Validation">
                                        <CssClasses Control="list-box-other-control" />
                                        <ClientSideEvents Init="handleOtherSpecify" SelectedIndexChanged="handleOtherSpecify" />
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgLCL" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                            <RequiredField IsRequired="true" ErrorText="At least one option must be selected!" />
                                        </ValidationSettings>
                                    </dx:BootstrapListBox>
                                    <asp:Label ID="lblTopicsDiscussedSpecifyCode" runat="server" CssClass="list-box-other-code-label hidden"></asp:Label>
                                    <div class="list-box-other-specify-div hidden">
                                        <dx:BootstrapMemo ID="txtOtherTopicsDiscussedSpecify" runat="server" Caption="Specify" 
                                            MaxLength="250" Rows="2" OnValidation="txtOtherTopicsDiscussedSpecify_Validation">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ValidationSettings ValidationGroup="vgLCL" ErrorDisplayMode="ImageWithText">
                                                <RequiredField IsRequired="false" ErrorText="Required!" />
                                            </ValidationSettings>
                                        </dx:BootstrapMemo>
                                    </div>
                                </div>
                                <div class="col-lg-4 mb-5">
                                    <label>How much targeted training was provided in this reporting period?</label>
                                    <dx:BootstrapTextBox ID="txtTargetedTrainingHours" runat="server" Caption="Hours" MaxLength="2"
                                        OnValidation="txtTargetedTrainingHours_Validation">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgLCL" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                            <RequiredField IsRequired="false" ErrorText="Required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapTextBox>
                                    <dx:BootstrapTextBox ID="txtTargetedTrainingMinutes" runat="server" Caption="Minutes" MaxLength="2"
                                        OnValidation="txtTargetedTrainingMinutes_Validation">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgLCL" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                            <RequiredField IsRequired="false" ErrorText="Required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapTextBox>
                                </div>
                                <div class="col-lg-4 mb-5">
                                    <dx:BootstrapListBox ID="lbTrainingsCovered" runat="server" Caption="Please indicate the targeted training topics covered in this reporting period"
                                        SelectionMode="CheckColumn" AllowCustomValues="true"
                                        TextField="Description" ValueField="CodeLCLResponsePK" ValueType="System.Int32"
                                        OnValidation="lbTrainingsCovered_Validation">
                                        <CssClasses Control="list-box-other-control" />
                                        <ClientSideEvents Init="handleOtherSpecify" SelectedIndexChanged="handleOtherSpecify" />
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgLCL" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                            <RequiredField IsRequired="true" ErrorText="At least one option must be selected!" />
                                        </ValidationSettings>
                                    </dx:BootstrapListBox>
                                    <asp:Label ID="lblTrainingsCoveredSpecifyCode" runat="server" CssClass="list-box-other-code-label hidden"></asp:Label>
                                    <div class="list-box-other-specify-div hidden">
                                        <dx:BootstrapMemo ID="txtOtherTrainingsCoveredSpecify" runat="server" Caption="Specify" 
                                            MaxLength="250" Rows="2" OnValidation="txtOtherTrainingsCoveredSpecify_Validation">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ValidationSettings ValidationGroup="vgLCL" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                <RequiredField IsRequired="false" ErrorText="Required!" />
                                            </ValidationSettings>
                                        </dx:BootstrapMemo>
                                    </div>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-6">
                                    <dx:BootstrapMemo ID="txtThinkNarrative" runat="server" Caption="Think: What is your current impression of this program?" 
                                        MaxLength="5000" Rows="4">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgLCL" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                            <RequiredField IsRequired="true" ErrorText="Required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapMemo>
                                </div>
                                <div class="col-lg-6">
                                    <dx:BootstrapMemo ID="txtActNarrative" runat="server" Caption="Act: What are your next steps to continue supporting this program?" 
                                        MaxLength="5000" Rows="4">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgLCL" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                            <RequiredField IsRequired="true" ErrorText="Required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapMemo>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-lg-6 mb-5">
                                    <dx:BootstrapMemo ID="txtHighlightsNarrative" runat="server" Caption="Highlights: Any highlights, stories, or additional information that you want to note" 
                                        MaxLength="5000" Rows="4">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgLCL" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="false" ErrorText="Required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapMemo>
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
                                    <ValidationSettings ValidationGroup="vgLCL" ErrorDisplayMode="ImageWithText">
                                        <RequiredField IsRequired="false" ErrorText="Required!" />
                                    </ValidationSettings>
                                </dx:BootstrapCheckBox>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div class="page-footer">
                <uc:Submit ID="submitLeadershipCoachLog" runat="server" ValidationGroup="vgLCL"
                    ControlCssClass="center-content"
                    OnSubmitClick="submitLeadershipCoachLog_Click" 
                    OnCancelClick="submitLeadershipCoachLog_CancelClick" 
                    OnValidationFailed="submitLeadershipCoachLog_ValidationFailed" />
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>