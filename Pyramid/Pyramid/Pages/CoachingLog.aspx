<%@ Page Title="Classroom Coaching Log" Language="C#" MasterPageFile="~/MasterPages/Dashboard.master" AutoEventWireup="true" CodeBehind="CoachingLog.aspx.cs" Inherits="Pyramid.Pages.CoachingLog" %>

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
            $('#lnkCoachingLogDashboard').addClass('active');

            //Set up the click events for the help buttons
            $('#btnCoachHelp').popover({
                trigger: 'hover focus',
                title: 'Help',
                html: true,
                content: 'In order for a professional to appear in this list, ' +
                    'the professional must have their coaching training added to their professional record by a state administrator ' +
                    'with a training date that is on or before the date of this coaching log.'
            });

            $('#btnCoacheeHelp').popover({
                trigger: 'hover focus',
                title: 'Help',
                html: true,
                content: 'In order for a professional to appear in this list, ' +
                    'the professional must have a current teacher or teacher assistant job function in their ' +
                    'professional record as of the date of this coaching log.'
            });

            //Allow date format for DataTables sorting
            $.fn.dataTable.moment('MM/DD/YYYY');

            //Show/hide the view only fields
            setViewOnlyVisibility();
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

        //This function controls whether or not the specify field for the other observation is shown
        //based on the text in the ddOBSOther Combo Box
        function showHideOBSOtherSpecify() {
            //Get the text from ddOBSOther
            var other = ddOBSOther.GetText();

            //If the text is yes, show the specify div
            if (other.toLowerCase() == 'yes') {
                $('#divOBSOtherSpecify').slideDown();
            }
            else {
                //The text is not yes, clear the specify text box and hide the specify div
                txtOBSOtherSpecify.SetValue('');
                $('#divOBSOtherSpecify').slideUp();
            }
        }

        //Validate the observation other specify field
        function validateOBSOtherSpecify(s, e) {
            var otherSpecify = e.value;
            var other = ddOBSOther.GetText();

            if ((otherSpecify == null || otherSpecify == ' ') && other.toLowerCase() == 'yes') {
                e.isValid = false;
                e.errorText = "Specify is required when the Other is set to yes!";
            }
            else {
                e.isValid = true;
            }
        }

        //This function controls whether or not the specify field for the other meetings is shown
        //based on the text in the ddMEETOther Combo Box
        function showHideMEETOtherSpecify() {
            //Get the text from ddMEETOther
            var other = ddMEETOther.GetText();

            //If the text is yes, show the specify div
            if (other.toLowerCase() == 'yes') {
                $('#divMEETOtherSpecify').slideDown();
            }
            else {
                //The text is not yes, clear the specify text box and hide the specify div
                txtMEETOtherSpecify.SetValue('');
                $('#divMEETOtherSpecify').slideUp();
            }
        }

        //Validate the meetings other specify field
        function validateMEETOtherSpecify(s, e) {
            var otherSpecify = e.value;
            var other = ddMEETOther.GetText();

            if ((otherSpecify == null || otherSpecify == ' ') && other.toLowerCase() == 'yes') {
                e.isValid = false;
                e.errorText = "Specify is required when the Other is set to yes!";
            }
            else {
                e.isValid = true;
            }
        }
    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Label ID="lblPageTitle" Text="CoachingLog" CssClass="h2" runat="server"></asp:Label>
    <hr />
    <asp:HiddenField ID="hfViewOnly" runat="server" Value="False" />
    <asp:UpdatePanel ID="upCoachingLog" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <uc:Messaging ID="msgSys" runat="server" />
            <asp:HiddenField ID="hfCoachingLogPK" runat="server" Value="" />
            <div class="row">
                <div class="col-lg-12">
                    <div class="card bg-light">
                        <div class="card-header">
                            <div class="row">
                                <div class="col-md-8">
                                    Basic Information
                                </div>
                                <div class="col-md-4">
                                    <dx:BootstrapButton ID="btnPrintPreview" runat="server" Text="Save and Download/Print" OnClick="btnPrintPreview_Click"
                                        SettingsBootstrap-RenderOption="primary" ValidationGroup="vgCoachingLog" data-validation-group="vgCoachingLog">
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
                                    <dx:BootstrapDateEdit ID="deLogDate" runat="server" Caption="Coaching Date" EditFormat="Date"
                                        EditFormatString="MM/dd/yyyy" UseMaskBehavior="true" NullText="--Select--"
                                        OnValueChanged="deLogDate_ValueChanged" AutoPostBack="true"
                                        OnValidation="deLogDate_Validation"
                                        AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                        <ValidationSettings ValidationGroup="vgCoachingLog" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                            <RequiredField IsRequired="true" ErrorText="Coaching Date is required!" />
                                        </ValidationSettings>
                                    </dx:BootstrapDateEdit>
                                </div>
                                <div class="col-md-4">
                                    <dx:BootstrapTextBox ID="txtDurationMinutes" runat="server" Caption="Duration (minutes)">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <MaskSettings Mask="099" PromptChar=" " ErrorText="Duration (minutes) must be a valid number!" />
                                        <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgCoachingLog">
                                            <RequiredField IsRequired="true" ErrorText="Duration is required" />
                                        </ValidationSettings>
                                    </dx:BootstrapTextBox>
                                </div>
                                <div class="col-md-4">
                                    <dx:BootstrapComboBox ID="ddCoach" runat="server" Caption="Coach" NullText="--Select--"
                                        TextField="CoachName" ValueField="ProgramEmployeePK" ValueType="System.Int32"
                                        IncrementalFilteringMode="Contains" AllowMouseWheel="false"
                                        OnValidation="ddCoach_Validation" ClientInstanceName="ddCoach">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgCoachingLog" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                            <RequiredField IsRequired="true" ErrorText="Coach is required!  If this control is not enabled, there are no coaches active at the coaching date." />
                                        </ValidationSettings>
                                    </dx:BootstrapComboBox>
                                    <button id="btnCoachHelp" type="button" class="btn btn-link p-0"><i class="fas fa-question-circle"></i>&nbsp;Help</button>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-4">
                                    <dx:BootstrapTagBox ID="tbCoachees" runat="server" Caption="Coachees"
                                        AllowCustomTags="false" TextField="CoacheeIDAndName" ValueField="ProgramEmployeePK"
                                        OnValidation="tbCoachees_Validation" LoadDropDownOnDemand="false" IncrementalFilteringMode="StartsWith">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgCoachingLog" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                            <RequiredField IsRequired="true" ErrorText="At least one coachee must be selected!" />
                                        </ValidationSettings>
                                    </dx:BootstrapTagBox>
                                    <button id="btnCoacheeHelp" type="button" class="btn btn-link p-0"><i class="fas fa-question-circle"></i>&nbsp;Help</button>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div class="row">
                <div class="col-lg-12">
                    <div class="card bg-light">
                        <div class="card-header">
                            Observations
                        </div>
                        <div class="card-body">
                            <div class="row">
                                <div class="col-md-4">
                                    <dx:BootstrapComboBox ID="ddOBSObserving" runat="server" NullText="--Select--" Caption="Observing"
                                        ValueType="System.Boolean" IncrementalFilteringMode="StartsWith">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgCoachingLog">
                                            <RequiredField IsRequired="true" ErrorText="Observing is required!" />
                                        </ValidationSettings>
                                        <Items>
                                            <dx:BootstrapListEditItem Text="Yes" Value="True"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Text="No" Value="False"></dx:BootstrapListEditItem>
                                        </Items>
                                    </dx:BootstrapComboBox>
                                </div>
                                <div class="col-md-4">
                                    <dx:BootstrapComboBox ID="ddOBSModeling" runat="server" NullText="--Select--" Caption="Modeling"
                                        ValueType="System.Boolean" IncrementalFilteringMode="StartsWith">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgCoachingLog">
                                            <RequiredField IsRequired="true" ErrorText="Modeling is required!" />
                                        </ValidationSettings>
                                        <Items>
                                            <dx:BootstrapListEditItem Text="Yes" Value="True"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Text="No" Value="False"></dx:BootstrapListEditItem>
                                        </Items>
                                    </dx:BootstrapComboBox>
                                </div>
                                <div class="col-md-4">
                                    <dx:BootstrapComboBox ID="ddOBSVerbalSupport" runat="server" NullText="--Select--" Caption="Verbal Support"
                                        ValueType="System.Boolean" IncrementalFilteringMode="StartsWith">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgCoachingLog">
                                            <RequiredField IsRequired="true" ErrorText="Verbal Support is required!" />
                                        </ValidationSettings>
                                        <Items>
                                            <dx:BootstrapListEditItem Text="Yes" Value="True"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Text="No" Value="False"></dx:BootstrapListEditItem>
                                        </Items>
                                    </dx:BootstrapComboBox>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-4">
                                    <dx:BootstrapComboBox ID="ddOBSSideBySide" runat="server" NullText="--Select--" Caption="Side by Side Gestural Support"
                                        ValueType="System.Boolean" IncrementalFilteringMode="StartsWith">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgCoachingLog">
                                            <RequiredField IsRequired="true" ErrorText="Side by Side Gestural Support is required!" />
                                        </ValidationSettings>
                                        <Items>
                                            <dx:BootstrapListEditItem Text="Yes" Value="True"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Text="No" Value="False"></dx:BootstrapListEditItem>
                                        </Items>
                                    </dx:BootstrapComboBox>
                                </div>
                                <div class="col-md-4">
                                    <dx:BootstrapComboBox ID="ddOBSProblemSolving" runat="server" NullText="--Select--" Caption="Problem Solving Discussion"
                                        ValueType="System.Boolean" IncrementalFilteringMode="StartsWith">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgCoachingLog">
                                            <RequiredField IsRequired="true" ErrorText="Problem Solving Discussion is required!" />
                                        </ValidationSettings>
                                        <Items>
                                            <dx:BootstrapListEditItem Text="Yes" Value="True"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Text="No" Value="False"></dx:BootstrapListEditItem>
                                        </Items>
                                    </dx:BootstrapComboBox>
                                </div>
                                <div class="col-md-4">
                                    <dx:BootstrapComboBox ID="ddOBSReflectiveConversation" runat="server" NullText="--Select--" Caption="Reflective Conversation"
                                        ValueType="System.Boolean" IncrementalFilteringMode="StartsWith">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgCoachingLog">
                                            <RequiredField IsRequired="true" ErrorText="Reflective Conversation is required!" />
                                        </ValidationSettings>
                                        <Items>
                                            <dx:BootstrapListEditItem Text="Yes" Value="True"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Text="No" Value="False"></dx:BootstrapListEditItem>
                                        </Items>
                                    </dx:BootstrapComboBox>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-4">
                                    <dx:BootstrapComboBox ID="ddOBSEnvironment" runat="server" NullText="--Select--" Caption="Help With Environmental Arrangements"
                                        ValueType="System.Boolean" IncrementalFilteringMode="StartsWith">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgCoachingLog">
                                            <RequiredField IsRequired="true" ErrorText="Help With Environmental Arrangements is required!" />
                                        </ValidationSettings>
                                        <Items>
                                            <dx:BootstrapListEditItem Text="Yes" Value="True"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Text="No" Value="False"></dx:BootstrapListEditItem>
                                        </Items>
                                    </dx:BootstrapComboBox>
                                </div>
                                <div class="col-md-4">
                                    <dx:BootstrapComboBox ID="ddOBSOtherHelp" runat="server" NullText="--Select--" Caption="Other Help in the Classroom"
                                        ValueType="System.Boolean" IncrementalFilteringMode="StartsWith">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgCoachingLog">
                                            <RequiredField IsRequired="true" ErrorText="Other Help in the Classroom is required!" />
                                        </ValidationSettings>
                                        <Items>
                                            <dx:BootstrapListEditItem Text="Yes" Value="True"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Text="No" Value="False"></dx:BootstrapListEditItem>
                                        </Items>
                                    </dx:BootstrapComboBox>
                                </div>
                                <div class="col-md-4">
                                    <dx:BootstrapComboBox ID="ddOBSConductTPOT" runat="server" NullText="--Select--" Caption="Conduct TPOT"
                                        ValueType="System.Boolean" IncrementalFilteringMode="StartsWith">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgCoachingLog">
                                            <RequiredField IsRequired="true" ErrorText="Conduct TPOT is required!" />
                                        </ValidationSettings>
                                        <Items>
                                            <dx:BootstrapListEditItem Text="Yes" Value="True"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Text="No" Value="False"></dx:BootstrapListEditItem>
                                        </Items>
                                    </dx:BootstrapComboBox>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-4">
                                    <dx:BootstrapComboBox ID="ddOBSConductTPITOS" runat="server" NullText="--Select--" Caption="Conduct TPITOS"
                                        ValueType="System.Boolean" IncrementalFilteringMode="StartsWith">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgCoachingLog">
                                            <RequiredField IsRequired="true" ErrorText="Conduct TPITOS is required!" />
                                        </ValidationSettings>
                                        <Items>
                                            <dx:BootstrapListEditItem Text="Yes" Value="True"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Text="No" Value="False"></dx:BootstrapListEditItem>
                                        </Items>
                                    </dx:BootstrapComboBox>
                                </div>
                                <div class="col-md-4">
                                    <dx:BootstrapComboBox ID="ddOBSOther" runat="server" NullText="--Select--" Caption="Other"
                                        ValueType="System.Boolean" IncrementalFilteringMode="StartsWith" ClientInstanceName="ddOBSOther">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ClientSideEvents Init="showHideOBSOtherSpecify" SelectedIndexChanged="showHideOBSOtherSpecify" />
                                        <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgCoachingLog">
                                            <RequiredField IsRequired="true" ErrorText="Other is required!" />
                                        </ValidationSettings>
                                        <Items>
                                            <dx:BootstrapListEditItem Text="Yes" Value="True"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Text="No" Value="False"></dx:BootstrapListEditItem>
                                        </Items>
                                    </dx:BootstrapComboBox>
                                    <div id="divOBSOtherSpecify" style="display: none">
                                        <dx:BootstrapTextBox ID="txtOBSOtherSpecify" runat="server" Caption="Specify" MaxLength="500"
                                            OnValidation="txtOBSOtherSpecify_Validation" ClientInstanceName="txtOBSOtherSpecify">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ClientSideEvents Validation="validateOBSOtherSpecify" />
                                            <ValidationSettings ValidationGroup="vgCoachingLog" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                            </ValidationSettings>
                                        </dx:BootstrapTextBox>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div class="row">
                <div class="col-lg-12">
                    <div class="card bg-light">
                        <div class="card-header">
                            Meetings
                        </div>
                        <div class="card-body">
                            <div class="row">
                                <div class="col-md-4">
                                    <dx:BootstrapComboBox ID="ddMEETProblemSolving" runat="server" NullText="--Select--" Caption="Problem Solving Discussion"
                                        ValueType="System.Boolean" IncrementalFilteringMode="StartsWith">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgCoachingLog">
                                            <RequiredField IsRequired="true" ErrorText="Problem Solving Discussion is required!" />
                                        </ValidationSettings>
                                        <Items>
                                            <dx:BootstrapListEditItem Text="Yes" Value="True"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Text="No" Value="False"></dx:BootstrapListEditItem>
                                        </Items>
                                    </dx:BootstrapComboBox>
                                </div>
                                <div class="col-md-4">
                                    <dx:BootstrapComboBox ID="ddMEETReflectiveConversation" runat="server" NullText="--Select--" Caption="Reflective Conversation"
                                        ValueType="System.Boolean" IncrementalFilteringMode="StartsWith">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgCoachingLog">
                                            <RequiredField IsRequired="true" ErrorText="Reflective Conversation is required!" />
                                        </ValidationSettings>
                                        <Items>
                                            <dx:BootstrapListEditItem Text="Yes" Value="True"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Text="No" Value="False"></dx:BootstrapListEditItem>
                                        </Items>
                                    </dx:BootstrapComboBox>
                                </div>
                                <div class="col-md-4">
                                    <dx:BootstrapComboBox ID="ddMEETEnvironment" runat="server" NullText="--Select--" Caption="Help With Environmental Arrangements"
                                        ValueType="System.Boolean" IncrementalFilteringMode="StartsWith">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgCoachingLog">
                                            <RequiredField IsRequired="true" ErrorText="Help With Environmental Arrangements is required!" />
                                        </ValidationSettings>
                                        <Items>
                                            <dx:BootstrapListEditItem Text="Yes" Value="True"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Text="No" Value="False"></dx:BootstrapListEditItem>
                                        </Items>
                                    </dx:BootstrapComboBox>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-4">
                                    <dx:BootstrapComboBox ID="ddMEETRoleplay" runat="server" NullText="--Select--" Caption="Role Play"
                                        ValueType="System.Boolean" IncrementalFilteringMode="StartsWith">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgCoachingLog">
                                            <RequiredField IsRequired="true" ErrorText="Role Play is required!" />
                                        </ValidationSettings>
                                        <Items>
                                            <dx:BootstrapListEditItem Text="Yes" Value="True"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Text="No" Value="False"></dx:BootstrapListEditItem>
                                        </Items>
                                    </dx:BootstrapComboBox>
                                </div>
                                <div class="col-md-4">
                                    <dx:BootstrapComboBox ID="ddMEETVideo" runat="server" NullText="--Select--" Caption="Video Feedback"
                                        ValueType="System.Boolean" IncrementalFilteringMode="StartsWith">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgCoachingLog">
                                            <RequiredField IsRequired="true" ErrorText="Video Feedback is required!" />
                                        </ValidationSettings>
                                        <Items>
                                            <dx:BootstrapListEditItem Text="Yes" Value="True"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Text="No" Value="False"></dx:BootstrapListEditItem>
                                        </Items>
                                    </dx:BootstrapComboBox>
                                </div>
                                <div class="col-md-4">
                                    <dx:BootstrapComboBox ID="ddMEETGraphic" runat="server" NullText="--Select--" Caption="Graphic Feedback"
                                        ValueType="System.Boolean" IncrementalFilteringMode="StartsWith">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgCoachingLog">
                                            <RequiredField IsRequired="true" ErrorText="Graphic Feedback is required!" />
                                        </ValidationSettings>
                                        <Items>
                                            <dx:BootstrapListEditItem Text="Yes" Value="True"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Text="No" Value="False"></dx:BootstrapListEditItem>
                                        </Items>
                                    </dx:BootstrapComboBox>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-4">
                                    <dx:BootstrapComboBox ID="ddMEETGoalSetting" runat="server" NullText="--Select--" Caption="Goal Setting/Action Planning"
                                        ValueType="System.Boolean" IncrementalFilteringMode="StartsWith">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgCoachingLog">
                                            <RequiredField IsRequired="true" ErrorText="Goal Setting/Action Planning is required!" />
                                        </ValidationSettings>
                                        <Items>
                                            <dx:BootstrapListEditItem Text="Yes" Value="True"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Text="No" Value="False"></dx:BootstrapListEditItem>
                                        </Items>
                                    </dx:BootstrapComboBox>
                                </div>
                                <div class="col-md-4">
                                    <dx:BootstrapComboBox ID="ddMEETPerformance" runat="server" NullText="--Select--" Caption="Performance Feedback"
                                        ValueType="System.Boolean" IncrementalFilteringMode="StartsWith">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgCoachingLog">
                                            <RequiredField IsRequired="true" ErrorText="Performance Feedback is required!" />
                                        </ValidationSettings>
                                        <Items>
                                            <dx:BootstrapListEditItem Text="Yes" Value="True"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Text="No" Value="False"></dx:BootstrapListEditItem>
                                        </Items>
                                    </dx:BootstrapComboBox>
                                </div>
                                <div class="col-md-4">
                                    <dx:BootstrapComboBox ID="ddMEETMaterial" runat="server" NullText="--Select--" Caption="Material Provision"
                                        ValueType="System.Boolean" IncrementalFilteringMode="StartsWith">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgCoachingLog">
                                            <RequiredField IsRequired="true" ErrorText="Material Provision is required!" />
                                        </ValidationSettings>
                                        <Items>
                                            <dx:BootstrapListEditItem Text="Yes" Value="True"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Text="No" Value="False"></dx:BootstrapListEditItem>
                                        </Items>
                                    </dx:BootstrapComboBox>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-4">
                                    <dx:BootstrapComboBox ID="ddMEETDemonstration" runat="server" NullText="--Select--" Caption="Demonstration"
                                        ValueType="System.Boolean" IncrementalFilteringMode="StartsWith">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgCoachingLog">
                                            <RequiredField IsRequired="true" ErrorText="Conduct TPITOS is required!" />
                                        </ValidationSettings>
                                        <Items>
                                            <dx:BootstrapListEditItem Text="Yes" Value="True"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Text="No" Value="False"></dx:BootstrapListEditItem>
                                        </Items>
                                    </dx:BootstrapComboBox>
                                </div>
                                <div class="col-md-4">
                                    <dx:BootstrapComboBox ID="ddMEETOther" runat="server" NullText="--Select--" Caption="Other"
                                        ValueType="System.Boolean" IncrementalFilteringMode="StartsWith" ClientInstanceName="ddMEETOther">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ClientSideEvents Init="showHideMEETOtherSpecify" SelectedIndexChanged="showHideMEETOtherSpecify" />
                                        <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgCoachingLog">
                                            <RequiredField IsRequired="true" ErrorText="Other is required!" />
                                        </ValidationSettings>
                                        <Items>
                                            <dx:BootstrapListEditItem Text="Yes" Value="True"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Text="No" Value="False"></dx:BootstrapListEditItem>
                                        </Items>
                                    </dx:BootstrapComboBox>
                                    <div id="divMEETOtherSpecify" style="display: none">
                                        <dx:BootstrapTextBox ID="txtMEETOtherSpecify" runat="server" Caption="Specify" MaxLength="500"
                                            OnValidation="txtMEETOtherSpecify_Validation" ClientInstanceName="txtMEETOtherSpecify">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ClientSideEvents Validation="validateMEETOtherSpecify" />
                                            <ValidationSettings ValidationGroup="vgCoachingLog" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                            </ValidationSettings>
                                        </dx:BootstrapTextBox>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div class="row">
                <div class="col-lg-12">
                    <div class="card bg-light">
                        <div class="card-header">
                            Follow-Up
                        </div>
                        <div class="card-body">
                            <div class="row">
                                <div class="col-md-4">
                                    <dx:BootstrapComboBox ID="ddFUEmail" runat="server" NullText="--Select--" Caption="Email"
                                        ValueType="System.Boolean" IncrementalFilteringMode="StartsWith">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgCoachingLog">
                                            <RequiredField IsRequired="true" ErrorText="Email is required!" />
                                        </ValidationSettings>
                                        <Items>
                                            <dx:BootstrapListEditItem Text="Yes" Value="True"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Text="No" Value="False"></dx:BootstrapListEditItem>
                                        </Items>
                                    </dx:BootstrapComboBox>
                                </div>
                                <div class="col-md-4">
                                    <dx:BootstrapComboBox ID="ddFUPhone" runat="server" NullText="--Select--" Caption="Phone"
                                        ValueType="System.Boolean" IncrementalFilteringMode="StartsWith">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgCoachingLog">
                                            <RequiredField IsRequired="true" ErrorText="Phone is required!" />
                                        </ValidationSettings>
                                        <Items>
                                            <dx:BootstrapListEditItem Text="Yes" Value="True"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Text="No" Value="False"></dx:BootstrapListEditItem>
                                        </Items>
                                    </dx:BootstrapComboBox>
                                </div>
                                <div class="col-md-4">
                                    <dx:BootstrapComboBox ID="ddFUInPerson" runat="server" NullText="--Select--" Caption="In Person"
                                        ValueType="System.Boolean" IncrementalFilteringMode="StartsWith">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgCoachingLog">
                                            <RequiredField IsRequired="true" ErrorText="In Person is required!" />
                                        </ValidationSettings>
                                        <Items>
                                            <dx:BootstrapListEditItem Text="Yes" Value="True"></dx:BootstrapListEditItem>
                                            <dx:BootstrapListEditItem Text="No" Value="False"></dx:BootstrapListEditItem>
                                        </Items>
                                    </dx:BootstrapComboBox>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div class="row">
                <div class="col-lg-12">
                    <div class="card bg-light">
                        <div class="card-header">
                            Narrative
                        </div>
                        <div class="card-body">
                            <div class="row">
                                <div class="col-md-12">
                                    <dx:BootstrapMemo ID="txtNarrative" runat="server" Rows="5" MaxLength="5000">
                                        <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                        <ValidationSettings ValidationGroup="vgCoachingLog" ErrorDisplayMode="ImageWithText">
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
                <uc:Submit ID="submitCoachingLog" runat="server" ValidationGroup="vgCoachingLog"
                    ControlCssClass="center-content"
                    OnSubmitClick="submitCoachingLog_Click" OnCancelClick="submitCoachingLog_CancelClick"
                    OnValidationFailed="submitCoachingLog_ValidationFailed" />
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
