<%@ Page Title="Bulk Training Addition" Language="C#" MasterPageFile="~/MasterPages/LoggedIn.master" AutoEventWireup="true" CodeBehind="BulkTraining.aspx.cs" Inherits="Pyramid.Admin.BulkTraining" %>

<%@ Register Assembly="DevExpress.Web.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Data.Linq" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.Bootstrap.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>
<%@ Register TagPrefix="uc" TagName="Messaging" Src="~/User_Controls/MessagingSystem.ascx" %>
<%@ Register TagPrefix="uc" TagName="Submit" Src="~/User_Controls/Submit.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ScriptContent" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <asp:UpdatePanel ID="upMessaging" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <uc:Messaging ID="msgSys" runat="server" />
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="bsGREmployees" />
        </Triggers>
    </asp:UpdatePanel>
    <div class="row">
        <div class="col-md-12">
            <div class="card bg-light">
                <div class="card-header">
                    Add Trainings
                </div>
                <div class="card-body">
                    <asp:UpdatePanel ID="upAllEmployees" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>
                            <div class="alert alert-primary">
                                Please enter and save training dates for one professional at a time as saving one professional's training dates will wipe out any unsaved training dates entered.
                                <br />
                                NOTE: Sorting, searching, and paging the table will wipe out any unsaved training dates.  
                            </div>
                            <label>Training Legend</label>
                            <div class="row">
                                <div class="col-sm-12 h5">
                                    <span class="badge badge-info text-wrap mb-2">PBC = Practice Based Coaching</span>
                                    <span id="spanPBCFCCInfo" runat="server" class="badge badge-info text-wrap mb-2">PBCFCC = Practice Based Coaching for Family and Group Family Child Care Coaches</span>
                                    <span id="spanICECPInfo" runat="server" class="badge badge-info text-wrap mb-2">ICECP = Introduction to Coaching Early Childhood Professionals</span>
                                    <span class="badge badge-info text-wrap mb-2">TPOT = TPOT Reliable Observer</span>
                                    <span class="badge badge-info text-wrap mb-2">TPITOS = TPITOS Reliable Observer</span>
                                </div>
                            </div>
                            <dx:BootstrapGridView ID="bsGREmployees" runat="server" EnableCallBacks="false" EnableRowsCache="true" 
                                KeyFieldName="ProgramEmployeePK" AutoGenerateColumns="false" DataSourceID="efEmployeeDataSource"
                                OnHtmlRowCreated="bsGREmployees_HtmlRowCreated">
                                <SettingsPager PageSize="15" />
                                <SettingsBootstrap Striped="false" />
                                <SettingsBehavior EnableRowHotTrack="true" />
                                <SettingsSearchPanel Visible="true" ShowApplyButton="true" />
                                <Settings ShowGroupPanel="false" />
                                <CssClasses IconHeaderSortUp="fas fa-long-arrow-alt-up" IconHeaderSortDown="fas fa-long-arrow-alt-down" IconShowAdaptiveDetailButton="fas fa-plus-circle" />
                                <SettingsAdaptivity AdaptivityMode="HideDataCells" AllowOnlyOneAdaptiveDetailExpanded="true" AdaptiveColumnPosition="Left"></SettingsAdaptivity>
                                <Columns>
                                    <dx:BootstrapGridViewDataColumn FieldName="EmployeePK" Caption="Employee Key" AdaptivePriority="9" Visible="false" />
                                    <dx:BootstrapGridViewDataColumn FieldName="EmployeeName" Caption="Professional" SortIndex="0" SortOrder="Ascending" AdaptivePriority="0" />
                                    <dx:BootstrapGridViewDataColumn FieldName="ProgramName" Caption="Program" AdaptivePriority="3" />
                                    <dx:BootstrapGridViewDataColumn FieldName="Trainings" Caption="Trainings" AdaptivePriority="2" Settings-AllowFilterBySearchPanel="False" Settings-AllowSort="False">
                                        <DataItemTemplate>
                                            <%# string.Join("<br/> ", (IEnumerable<string>)Eval("Trainings")) %>
                                        </DataItemTemplate>
                                    </dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewDataColumn FieldName="EmailAddress" Caption="Email" AdaptivePriority="5" />
                                    <dx:BootstrapGridViewDataColumn Name="CoachColumn" FieldName="PBCDate" Caption="PBC Trainings" AdaptivePriority="2" Settings-AllowFilterBySearchPanel="False" Settings-AllowSort="False" Width="180px">
                                        <DataItemTemplate>
                                            <dx:BootstrapDateEdit ID="dePBCDate" runat="server" Caption="PBC Date" EditFormat="Date" AllowNull="true"
                                                EditFormatString="MM/dd/yyyy" UseMaskBehavior="true" NullText="--Select--"
                                                AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                            </dx:BootstrapDateEdit>
                                            <dx:BootstrapDateEdit ID="dePBCFCCDate" runat="server" Caption="PBCFCC Date" EditFormat="Date" AllowNull="true"
                                                EditFormatString="MM/dd/yyyy" UseMaskBehavior="true" NullText="--Select--"
                                                AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                            </dx:BootstrapDateEdit>
                                            <dx:BootstrapDateEdit ID="deICECPDate" runat="server" Caption="ICECP Date" EditFormat="Date" AllowNull="true"
                                                EditFormatString="MM/dd/yyyy" UseMaskBehavior="true" NullText="--Select--"
                                                AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                            </dx:BootstrapDateEdit>
                                        </DataItemTemplate>
                                    </dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewDataColumn Name="ObserverColumn" FieldName="TPOTTrainingDate" Caption="Observer Trainings" AdaptivePriority="2" Settings-AllowFilterBySearchPanel="False" Settings-AllowSort="False" Width="180px">
                                        <DataItemTemplate>
                                            <dx:BootstrapDateEdit ID="deTPOTTrainingDate" runat="server" Caption="TPOT Date" EditFormat="Date" AllowNull="true"
                                                EditFormatString="MM/dd/yyyy" UseMaskBehavior="true" NullText="--Select--"
                                                AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                            </dx:BootstrapDateEdit>
                                            <dx:BootstrapDateEdit ID="deTPITOSTrainingDate" runat="server" Caption="TPITOS Date" EditFormat="Date" AllowNull="true"
                                                EditFormatString="MM/dd/yyyy" UseMaskBehavior="true" NullText="--Select--"
                                                AllowMouseWheel="false" PickerDisplayMode="Calendar" MinDate="01/01/1900">
                                                <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                            </dx:BootstrapDateEdit>
                                        </DataItemTemplate>
                                    </dx:BootstrapGridViewDataColumn>
                                    <dx:BootstrapGridViewButtonEditColumn Settings-AllowDragDrop="False" AdaptivePriority="0" CssClasses-DataCell="text-center" Width="100px">
                                        <DataItemTemplate>
                                            <asp:LinkButton ID="lbSaveTrainings" runat="server" CssClass="btn btn-success btn-loader" OnClick="lbSaveTrainings_Click">
                                                <i class="fas fa-save"></i>
                                                Save
                                            </asp:LinkButton>
                                        </DataItemTemplate>
                                    </dx:BootstrapGridViewButtonEditColumn>
                                </Columns>
                            </dx:BootstrapGridView>
                            <dx:EntityServerModeDataSource ID="efEmployeeDataSource" runat="server"
                                OnSelecting="efEmployeeDataSource_Selecting" />
                        </ContentTemplate>
                        <Triggers>

                        </Triggers>
                    </asp:UpdatePanel>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
