<%@ Page Title="Behavior Incident Report" Language="C#" MasterPageFile="~/MasterPages/Dashboard.master" AutoEventWireup="true" CodeBehind="BehaviorIncident.aspx.cs" Inherits="Pyramid.Pages.BehaviorIncident" %>

<%@ Register Assembly="DevExpress.Web.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Data.Linq" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.Bootstrap.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>
<%@ Register TagPrefix="uc" TagName="Classroom" Src="~/User_Controls/Classroom.ascx" %>
<%@ Register TagPrefix="uc" TagName="Submit" Src="~/User_Controls/Submit.ascx" %>
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
            //Highlight the correct dashboard link
            $('#lnkBehaviorIncidentDashboard').addClass('active');

            //Set up the click events for the help buttons
            $('#btnChildHelp').popover({
                trigger: 'hover focus',
                title: 'Help',
                html: true,
                content: 'The incident date and time must be entered before ' +
                    'you can select the child.  If the incident date and time have been entered and the child ' +
                    'is still not selectable, the child is either not in the system or the child ' +
                    'has a discharge date that is before the incident date.'
            });
            
            $('#btnClassroomHelp').popover({
                trigger: 'hover focus',
                title: 'Help',
                html: true,
                content: 'A child must be selected before any classrooms appear in this list. ' +
                    'If a child is selected and the classroom is still not selectable, the child ' +
                    'is not assigned to a classroom as of the incident date.'
            });

            //Show/hide the view only fields
            setViewOnlyVisibility();

            //Get the submit button and submitting button
            var lbRefreshClassrooms = $('[ID$="lbRefreshClassrooms"]');
            var lbRefreshingClassrooms = $('[ID$="lbRefreshingClassrooms"]');

            //Show the submit button
            lbRefreshClassrooms.show();

            //Hide the submitting button
            lbRefreshingClassrooms.hide();
        
            //Set the click event for the submit button
            lbRefreshClassrooms.off('click').on('click', function () {
                //If the validation succeeds, prevent clicks on the submit button
                lbRefreshClassrooms.off('click').on('click', function (e) {
                    e.preventDefault();
                    e.stopPropagation();
                });

                //Hide the submit button
                lbRefreshClassrooms.hide();

                //Show the submitting button and prevent clicks on it
                lbRefreshingClassrooms.show().off('click').on('click', function (e) {
                    e.preventDefault();
                    e.stopPropagation();
                });
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

        //This function controls whether or not the specify field for the problem behavior is shown
        //based on the value in the ddProblemBehavior Combo Box
        function showHideProblemBehaviorSpecify() {
            //Get the problem behavior
            var problemBehavior = ddProblemBehavior.GetText();

            //If the problem behavior is other, show the specify div
            if (problemBehavior.toLowerCase() == 'other') {
                $('#divProblemBehaviorSpecify').slideDown();
            }
            else {
                //The problem behavior is not other, clear the specify text box and hide the specify div
                txtProblemBehaviorSpecify.SetValue('');
                $('#divProblemBehaviorSpecify').slideUp();
            }
        }

        //Validate the problem behavior specify field
        function validateProblemBehaviorSpecify(s, e) {
            var behaviorSpecify = e.value;
            var problemBehavior = ddProblemBehavior.GetText();

            if ((behaviorSpecify == null || behaviorSpecify == ' ') && problemBehavior.toLowerCase() == 'other') {
                e.isValid = false;
                e.errorText = "Specify Problem Behavior is required when the 'Other' problem behavior is selected!";
            }
            else {
                e.isValid = true;
            }
        }

        //This function controls whether or not the specify field for the activity is shown
        //based on the value in the ddActivity Combo Box
        function showHideActivitySpecify() {
            //Get the activity
            var activity = ddActivity.GetText();

            //If the activity is other, show the specify div
            if (activity.toLowerCase() == 'other') {
                $('#divActivitySpecify').slideDown();
            }
            else {
                //The activity is not other, clear the specify text box and hide the specify div
                txtActivitySpecify.SetValue('');
                $('#divActivitySpecify').slideUp();
            }
        }

        //Validate the activity specify field
        function validateActivitySpecify(s, e) {
            var activitySpecify = e.value;
            var activity = ddActivity.GetText();

            if ((activitySpecify == null || activitySpecify == ' ') && activity.toLowerCase() == 'other') {
                e.isValid = false;
                e.errorText = "Specify Activity is required when the 'Other' activity is selected!";
            }
            else {
                e.isValid = true;
            }
        }

        //This function controls whether or not the specify field for the others involved is shown
        //based on the value in the ddOthersInvolved Combo Box
        function showHideOthersInvolvedSpecify() {
            //Get the others involved
            var othersInvolved = ddOthersInvolved.GetText();

            //If the others involved is other, show the specify div
            if (othersInvolved.toLowerCase() == 'other') {
                $('#divOthersInvolvedSpecify').slideDown();
            }
            else {
                //The others involved is not other, clear the specify text box and hide the specify div
                txtOthersInvolvedSpecify.SetValue('');
                $('#divOthersInvolvedSpecify').slideUp();
            }
        }

        //Validate the others involved specify field
        function validateOthersInvolvedSpecify(s, e) {
            var othersInvolvedSpecify = e.value;
            var othersInvolved = ddOthersInvolved.GetText();

            if ((othersInvolvedSpecify == null || othersInvolvedSpecify == ' ') && othersInvolved.toLowerCase() == 'other') {
                e.isValid = false;
                e.errorText = "Specify Others Involved is required when the 'Other' others involved is selected!";
            }
            else {
                e.isValid = true;
            }
        }

        //This function controls whether or not the specify field for the possible motivation is shown
        //based on the value in the ddPossibleMotivation Combo Box
        function showHidePossibleMotivationSpecify() {
            //Get the possible motivation
            var possibleMotivation = ddPossibleMotivation.GetText();

            //If the possible motivation is other, show the specify div
            if (possibleMotivation.toLowerCase() == 'other') {
                $('#divPossibleMotivationSpecify').slideDown();
            }
            else {
                //The possible motivation is not other, clear the specify text box and hide the specify div
                txtPossibleMotivationSpecify.SetValue('');
                $('#divPossibleMotivationSpecify').slideUp();
            }
        }

        //Validate the possible motivation specify field
        function validatePossibleMotivationSpecify(s, e) {
            var possibleMotivationSpecify = e.value;
            var possibleMotivation = ddPossibleMotivation.GetText();

            if ((possibleMotivationSpecify == null || possibleMotivationSpecify == ' ')
                    && possibleMotivation.toLowerCase() == 'other') {
                e.isValid = false;
                e.errorText = "Specify Possible Motivation is required when the 'Other' possible motivation is selected!";
            }
            else {
                e.isValid = true;
            }
        }

        //This function controls whether or not the specify field for the strategy response is shown
        //based on the value in the ddStrategyResponse Combo Box
        function showHideStrategyResponseSpecify() {
            //Get the strategy response
            var strategyResponse = ddStrategyResponse.GetText();

            //If the strategy response is other, show the specify div
            if (strategyResponse.toLowerCase() == 'other') {
                $('#divStrategyResponseSpecify').slideDown();
            }
            else {
                //The strategy response is not other, clear the specify text box and hide the specify div
                txtStrategyResponseSpecify.SetValue('');
                $('#divStrategyResponseSpecify').slideUp();
            }
        }

        //Validate the strategy response specify field
        function validateStrategyResponseSpecify(s, e) {
            var strategyResponseSpecify = e.value;
            var strategyResponse = ddStrategyResponse.GetText();

            if ((strategyResponseSpecify == null || strategyResponseSpecify == ' ')
                    && strategyResponse.toLowerCase() == 'other') {
                e.isValid = false;
                e.errorText = "Specify Strategy Response is required when the 'Other' strategy response is selected!";
            }
            else {
                e.isValid = true;
            }
        }

        //This function controls whether or not the specify field for the admin follow-up is shown
        //based on the value in the ddAdminFollowUp Combo Box
        function showHideAdminFollowUpSpecify() {
            //Get the admin follow-up
            var adminFollowUp = ddAdminFollowUp.GetText();

            //If the admin follow-up is other, show the specify div
            if (adminFollowUp.toLowerCase() == 'other') {
                $('#divAdminFollowUpSpecify').slideDown();
            }
            else {
                //The admin follow-up is not other, clear the specify text box and hide the specify div
                txtAdminFollowUpSpecify.SetValue('');
                $('#divAdminFollowUpSpecify').slideUp();
            }
        }

        //Validate the admin follow-up specify field
        function validateAdminFollowUpSpecify(s, e) {
            var adminFollowUpSpecify = e.value;
            var adminFollowUp = ddAdminFollowUp.GetText();

            if ((adminFollowUpSpecify == null || adminFollowUpSpecify == ' ') && adminFollowUp.toLowerCase() == 'other') {
                e.isValid = false;
                e.errorText = "Specify Admin Follow-up is required when the 'Other' admin follow-up is selected!";
            }
            else {
                e.isValid = true;
            }
        }
    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Label ID="lblPageTitle" runat="server" Text="Behavior Incident" CssClass="h2"></asp:Label>
    <hr />
    <asp:HiddenField ID="hfViewOnly" runat="server" Value="False" />
    <asp:UpdatePanel ID="upBIR" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <uc:Messaging ID="msgSys" runat="server" />
            <asp:HiddenField ID="hfBIRPK" runat="server" Value="" />
            <!--main content-->
            <div class="row">
                <div class="col-lg-12">
                    <div class="card bg-light">
                        <div class="card-header">
                            <div class="row">
                                <div class="col-md-8">
                                    Behavior Incident Report
                                </div>
                                <div class="col-md-4">
                                    <dx:BootstrapButton ID="btnPrintPreview" runat="server" Text="Save and Download/Print" OnClick="btnPrintPreview_Click"
                                        SettingsBootstrap-RenderOption="primary" ValidationGroup="vgBehaviorIncident" data-validation-group="vgBehaviorIncident">
                                        <CssClasses Icon="fas fa-print" Control="float-right btn-loader" />
                                    </dx:BootstrapButton>
                                </div>
                            </div>
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
                                    <dx:BootstrapDateEdit ID="deIncidentDatetime" runat="server" Caption="Incident Datetime"
                                        EditFormat="DateTime" EditFormatString="MM/dd/yyyy hh:mm tt"
                                        UseMaskBehavior="true" NullText="--Select--"
                                        OnValueChanged="deIncidentDatetime_ValueChanged" AutoPostBack="true"
                                        AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                        <TimeSectionProperties Visible="true" />
                                        <CalendarProperties ShowClearButton="true"></CalendarProperties>
                                        <ValidationSettings ValidationGroup="vgBehaviorIncident" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Incident Datetime is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapDateEdit>
                                </div>
                                <div class="col-md-4">
                                    <dx:BootstrapComboBox ID="ddChild" runat="server" Caption="Child" NullText="--Select--"
                                        TextField="IdAndName" ValueField="ChildPK" ValueType="System.Int32"
                                        IncrementalFilteringMode="Contains" AllowMouseWheel="false"
                                        OnSelectedIndexChanged="ddChild_SelectedIndexChanged" AutoPostBack="true">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgBehaviorIncident" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Child is required!  If this is not enabled, the child was not active as of the incident date." />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                    <asp:LinkButton ID="lbEditChild" runat="server" Visible="false" OnClick="lbEditChild_Click"><i class="fas fa-edit"></i>&nbsp;Edit Child</asp:LinkButton>
                                    <button id="btnChildHelp" type="button" class="btn btn-link pt-0"><i class="fas fa-question-circle"></i>&nbsp;Help</button>
                                </div>
                                <div class="col-md-4">
                                    <dx:BootstrapComboBox ID="ddClassroom" runat="server" Caption="Classroom" NullText="--Select--"
                                        TextField="IdAndName" ValueField="ClassroomPK" ValueType="System.Int32"
                                        IncrementalFilteringMode="Contains" AllowMouseWheel="false" ReadOnly="true">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgBehaviorIncident" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Classroom is required!  If this is not enabled, the child is not assigned to a classroom as of the incident date." />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                    <asp:LinkButton ID="lbRefreshClassrooms" runat="server" OnClick="lbRefreshClassrooms_Click"><i class="fas fa-sync-alt"></i>&nbsp;Refresh</asp:LinkButton>
                                    <asp:LinkButton ID="lbRefreshingClassrooms" runat="server" Style="display: none">
                                        <span class="spinner-border spinner-border-sm"></span>
                                        Loading...
                                    </asp:LinkButton>
                                    <button id="btnClassroomHelp" type="button" class="btn btn-link pt-0"><i class="fas fa-question-circle"></i>&nbsp;Help</button>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-4">
                                    <dx:BootstrapComboBox ID="ddProblemBehavior" runat="server" Caption="Problem Behavior" NullText="--Select--"
                                        TextField="Description" ValueField="CodeProblemBehaviorPK" ValueType="System.Int32"
                                        IncrementalFilteringMode="Contains" ClientInstanceName="ddProblemBehavior" AllowMouseWheel="false">
                                        <ClientSideEvents Init="showHideProblemBehaviorSpecify" SelectedIndexChanged="showHideProblemBehaviorSpecify" />
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgBehaviorIncident" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Problem Behavior is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                    <div id="divProblemBehaviorSpecify" style="display: none">
                                        <dx:BootstrapTextBox ID="txtProblemBehaviorSpecify" runat="server" Caption="Specify Problem Behavior" MaxLength="500"
                                            OnValidation="txtProblemBehaviorSpecify_Validation" ClientInstanceName="txtProblemBehaviorSpecify">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ClientSideEvents Validation="validateProblemBehaviorSpecify" />
                                            <ValidationSettings ValidationGroup="vgBehaviorIncident" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                            </ValidationSettings>
                                        </dx:BootstrapTextBox>
                                    </div>
                                </div>
                                <div class="col-md-4">
                                    <dx:BootstrapComboBox ID="ddActivity" runat="server" Caption="Activity" NullText="--Select--"
                                        TextField="Description" ValueField="CodeActivityPK" ValueType="System.Int32"
                                        IncrementalFilteringMode="Contains" ClientInstanceName="ddActivity" AllowMouseWheel="false">
                                        <ClientSideEvents Init="showHideActivitySpecify" SelectedIndexChanged="showHideActivitySpecify" />
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgBehaviorIncident" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Activity is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                    <div id="divActivitySpecify" style="display: none">
                                        <dx:BootstrapTextBox ID="txtActivitySpecify" runat="server" Caption="Specify Activity" MaxLength="500"
                                            OnValidation="txtActivitySpecify_Validation" ClientInstanceName="txtActivitySpecify">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ClientSideEvents Validation="validateActivitySpecify" />
                                            <ValidationSettings ValidationGroup="vgBehaviorIncident" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                            </ValidationSettings>
                                        </dx:BootstrapTextBox>
                                    </div>
                                </div>
                                <div class="col-md-4">
                                    <dx:BootstrapComboBox ID="ddOthersInvolved" runat="server" Caption="Others Involved" NullText="--Select--"
                                        TextField="Description" ValueField="CodeOthersInvolvedPK" ValueType="System.Int32"
                                        IncrementalFilteringMode="Contains" ClientInstanceName="ddOthersInvolved" AllowMouseWheel="false">
                                        <ClientSideEvents Init="showHideOthersInvolvedSpecify" SelectedIndexChanged="showHideOthersInvolvedSpecify" />
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgBehaviorIncident" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Others Involved is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                    <div id="divOthersInvolvedSpecify" style="display: none">
                                        <dx:BootstrapTextBox ID="txtOthersInvolvedSpecify" runat="server" Caption="Specify Others Involved" MaxLength="500"
                                            OnValidation="txtOthersInvolvedSpecify_Validation" ClientInstanceName="txtOthersInvolvedSpecify">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ClientSideEvents Validation="validateOthersInvolvedSpecify" />
                                            <ValidationSettings ValidationGroup="vgBehaviorIncident" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                            </ValidationSettings>
                                        </dx:BootstrapTextBox>
                                    </div>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-4">
                                    <dx:BootstrapComboBox ID="ddPossibleMotivation" runat="server" Caption="Possible Motivation" NullText="--Select--"
                                        TextField="Description" ValueField="CodePossibleMotivationPK" ValueType="System.Int32"
                                        IncrementalFilteringMode="Contains" ClientInstanceName="ddPossibleMotivation" AllowMouseWheel="false">
                                        <ClientSideEvents Init="showHidePossibleMotivationSpecify" SelectedIndexChanged="showHidePossibleMotivationSpecify" />
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgBehaviorIncident" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Possible Motivation is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                    <div id="divPossibleMotivationSpecify" style="display: none">
                                        <dx:BootstrapTextBox ID="txtPossibleMotivationSpecify" runat="server" Caption="Specify Possible Motivation" MaxLength="500"
                                            OnValidation="txtPossibleMotivationSpecify_Validation" ClientInstanceName="txtPossibleMotivationSpecify">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ClientSideEvents Validation="validatePossibleMotivationSpecify" />
                                            <ValidationSettings ValidationGroup="vgBehaviorIncident" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                            </ValidationSettings>
                                        </dx:BootstrapTextBox>
                                    </div>
                                </div>
                                <div class="col-md-4">
                                    <dx:BootstrapComboBox ID="ddStrategyResponse" runat="server" Caption="Strategy Response" NullText="--Select--"
                                        TextField="Description" ValueField="CodeStrategyResponsePK" ValueType="System.Int32"
                                        IncrementalFilteringMode="Contains" ClientInstanceName="ddStrategyResponse" AllowMouseWheel="false">
                                        <ClientSideEvents Init="showHideStrategyResponseSpecify" SelectedIndexChanged="showHideStrategyResponseSpecify" />
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgBehaviorIncident" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Strategy Response is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                    <div id="divStrategyResponseSpecify" style="display: none">
                                        <dx:BootstrapTextBox ID="txtStrategyResponseSpecify" runat="server" Caption="Specify Strategy Response" MaxLength="500"
                                            OnValidation="txtStrategyResponseSpecify_Validation" ClientInstanceName="txtStrategyResponseSpecify">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ClientSideEvents Validation="validateStrategyResponseSpecify" />
                                            <ValidationSettings ValidationGroup="vgBehaviorIncident" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                            </ValidationSettings>
                                        </dx:BootstrapTextBox>
                                    </div>
                                </div>
                                <div class="col-md-4">
                                    <dx:BootstrapComboBox ID="ddAdminFollowUp" runat="server" Caption="Admin Follow-up" NullText="--Select--"
                                        TextField="Description" ValueField="CodeAdminFollowUpPK" ValueType="System.Int32"
                                        IncrementalFilteringMode="Contains" ClientInstanceName="ddAdminFollowUp" AllowMouseWheel="false">
                                        <ClientSideEvents Init="showHideAdminFollowUpSpecify" SelectedIndexChanged="showHideAdminFollowUpSpecify" />
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgBehaviorIncident" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Admin Follow-up is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                    <div id="divAdminFollowUpSpecify" style="display: none">
                                        <dx:BootstrapTextBox ID="txtAdminFollowUpSpecify" runat="server" Caption="Specify Admin Follow-up" MaxLength="500"
                                            OnValidation="txtAdminFollowUpSpecify_Validation" ClientInstanceName="txtAdminFollowUpSpecify">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ClientSideEvents Validation="validateAdminFollowUpSpecify" />
                                            <ValidationSettings ValidationGroup="vgBehaviorIncident" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                            </ValidationSettings>
                                        </dx:BootstrapTextBox>
                                    </div>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-4">
                                    <dx:BootstrapMemo ID="txtBehaviorDescription" runat="server" Caption="Behavior Description" Rows="5">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgBehaviorIncident" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="true" ErrorText="Behavior Description is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapMemo>
                                </div>
                                <div class="col-md-8">
                                    <dx:BootstrapMemo ID="txtNotes" runat="server" Caption="Notes" Rows="5">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgBehaviorIncident" ErrorDisplayMode="ImageWithText">
                                            <RequiredField IsRequired="false" />
                                        </ValidationSettings>
                                    </dx:BootstrapMemo>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div class="page-footer">
                <uc:Submit ID="submitBehaviorIncident" runat="server" ValidationGroup="vgBehaviorIncident"
                    ControlCssClass="center-content"
                    OnSubmitClick="submitBehaviorIncident_Click" OnCancelClick="submitBehaviorIncident_CancelClick"
                    OnValidationFailed="submitBehaviorIncident_ValidationFailed"></uc:Submit>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
