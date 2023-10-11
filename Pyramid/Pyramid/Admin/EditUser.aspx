<%@ Page Title="Edit User" Language="C#" MasterPageFile="~/MasterPages/LoggedIn.Master" AutoEventWireup="true" CodeBehind="EditUser.aspx.cs" Inherits="Pyramid.Admin.EditUser" %>

<%@ Register TagPrefix="uc" TagName="Messaging" Src="~/User_Controls/MessagingSystem.ascx" %>
<%@ Register TagPrefix="uc" TagName="Submit" Src="~/User_Controls/Submit.ascx" %>
<%@ Register Assembly="DevExpress.Web.Bootstrap.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ContentPlaceHolderID="ScriptContent" runat="server">
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

            //Initialize the datatable
            if (!$.fn.dataTable.isDataTable('#tblUserRoles')) {
                $('#tblUserRoles').DataTable({
                    responsive: {
                        details: {
                            type: 'column'
                        }
                    },
                    columnDefs: [
                        { orderable: false, targets: [3] },
                        { className: 'control', orderable: false, targets: 0 }
                    ],
                    order: [[2, 'asc'], [1, 'asc']],
                    stateSave: true,
                    stateDuration: 60,
                    pageLength: 10,
                    dom: 'frtp',
                    language: {
                        searchPlaceholder: "Enter text to search...",
                        search: "",
                    }
                });
            }
            $('.dataTables_filter input').removeClass('form-control-sm');

            //Show the change password modal on button click
            $('[ID$="btnChangePassword"]').off('click').on('click', function (e) {
                //Prevent a postback
                e.preventDefault();

                //Clear all inputs in the modal
                $('#divChangePasswordModal input').val('');

                //Show the modal
                $('#divChangePasswordModal').modal('show');
            });

            //Hide the modal after the user confirms the change
            $('[ID$="btnSavePassword"]').off('click').on('click', function (e) {
                if (ASPxClientEdit.ValidateGroup('vgChangePassword')) {
                    $('#divChangePasswordModal').modal('hide');
                }
                else {
                    //Call the client validation failed method
                    clientValidationFailed();
                }
            });

            //Set the add role button click event
            setAddRoleButtonClick();
        }

        //Validate the password confirmation
        function validateConfirmPassword(s, e) {
            var confirmedPassword = e.value;
            var password = txtPassword.GetText();

            if (confirmedPassword != password) {
                e.isValid = false;
                e.errorText = "Password confirmation does not match!";
            }
            else {
                e.isValid = true;
            }
        }

        //Validate the program role
        function validateProgramRole(s, e) {
            //Get the new role PK
            var newRolePK = e.value;

            //Get the new role's program
            var newProgram = ddProgram.GetSelectedItem();

            //Only continue if the user selected a program
            if (newProgram != null) {
                //Get the program PK
                var newProgramPK = newProgram.value;

                //Get the existing roles
                var existingRoles = $('#tblUserRoles tbody tr');

                //Loop through the existing roles
                existingRoles.each(function () {
                    //Get the role FK
                    var existingRoleFK = $(this).find('[ID*="hfProgramRoleFK"]').val();
                    //Get the program FK
                    var existingRoleProgramFK = $(this).find('[ID*="hfProgramFK"]').val();

                    //If the new role matches an existing roles, this is invalid
                    if (newRolePK == existingRoleFK && newProgramPK == existingRoleProgramFK) {
                        e.isValid = false;
                        e.errorText = 'Program Role already exists!';
                        return false;
                    }
                });
            }
        }

        //This function shows a loading button for the Add Role button if the 
        //validation succeeds
        function setAddRoleButtonClick() {
            //Get the add role button
            var btnAddRole = $('[ID$="btnAddRole"]');

            //Hide the loading button
            $('.btn-loading').hide();

            //Show the add role button
            btnAddRole.show();

            //Create the click event
            btnAddRole.off('click').on('click', function () {
                //Get the validation group
                var validationGroup = 'vgAddRole';

                if (ASPxClientEdit.ValidateGroup(validationGroup)) {
                    //Validation succeeded
                    //Get the loading button
                    var loadingButton = btnAddRole.clone();

                    //Set the loading button attributes and content
                    loadingButton.attr('id', this.id + '_loading');
                    loadingButton.addClass('.btn-loading');
                    loadingButton.html('<span class="spinner-border spinner-border-sm"></span>&nbsp;Loading...');
                    loadingButton.off('click').on('click', function (e) {
                        e.preventDefault();
                        e.stopPropagation();
                    });

                    //Prevent clicks on the Add Role button
                    btnAddRole.off('click').on('click', function (e) {
                        e.preventDefault();
                        e.stopPropagation();
                    });

                    //Append the loading button
                    btnAddRole.after(loadingButton);

                    //Hide the Add Role button
                    btnAddRole.hide();

                    //Show the loading button
                    loadingButton.show();
                }
                else {
                    //Call the client validation failed method
                    clientValidationFailed();
                }
            });
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:UpdatePanel ID="upEditUser" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <uc:Messaging ID="msgSys" runat="server" />
            <div class="row">
                <div class="col-lg-12">
                    <div class="card bg-light">
                        <div class="card-header">Basic Information</div>
                        <div class="card-body">
                            <div class="row">
                                <div class="col-md-4">
                                    <div class="form-group">
                                        <dx:BootstrapTextBox ID="txtFirstName" runat="server" Caption="First Name">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ValidationSettings ValidationGroup="vgEditUser" ErrorDisplayMode="ImageWithText">
                                                <RequiredField IsRequired="true" ErrorText="First Name is required!" />
                                            </ValidationSettings>
                                        </dx:BootstrapTextBox>
                                    </div>
                                </div>
                                <div class="col-md-4">
                                    <div class="form-group">
                                        <dx:BootstrapTextBox ID="txtLastName" runat="server" Caption="Last Name">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ValidationSettings ValidationGroup="vgEditUser" ErrorDisplayMode="ImageWithText">
                                                <RequiredField IsRequired="true" ErrorText="Last Name is required!" />
                                            </ValidationSettings>
                                        </dx:BootstrapTextBox>
                                    </div>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-4">
                                    <div class="form-group">
                                        <dx:BootstrapTextBox ID="txtEmail" runat="server" Caption="Email">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ValidationSettings ValidationGroup="vgEditUser" ErrorDisplayMode="ImageWithText">
                                                <RequiredField IsRequired="true" ErrorText="Email is required!" />
                                                <RegularExpression ErrorText="Invalid Email!" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" />
                                            </ValidationSettings>
                                        </dx:BootstrapTextBox>
                                    </div>
                                </div>
                                <div class="col-md-4">
                                    <div class="form-group">
                                        <dx:BootstrapTextBox ID="txtPhoneNumber" runat="server" Caption="Personal Phone Number" OnValidation="txtPhoneNumber_Validation">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <MaskSettings Mask="+1 (999) 999-9999" IncludeLiterals="None" ErrorText="Must be a valid phone number!" />
                                            <ValidationSettings ValidationGroup="vgEditUser" ErrorDisplayMode="ImageWithText">
                                                <RequiredField IsRequired="false" ErrorText="Personal Phone Number is required!" />
                                            </ValidationSettings>
                                        </dx:BootstrapTextBox>
                                    </div>
                                </div>
                                <div class="col-md-4">
                                    <div class="form-group">
                                        <dx:BootstrapTextBox ID="txtWorkPhoneNumber" runat="server" Caption="Work Phone Number" OnValidation="txtWorkPhoneNumber_Validation">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <MaskSettings Mask="+1 (999) 999-9999 \e\x\t\. 999999" IncludeLiterals="None" ErrorText="Must be a valid phone number!" />
                                            <ValidationSettings ValidationGroup="vgEditUser" ErrorDisplayMode="ImageWithText">
                                                <RequiredField IsRequired="false" ErrorText="Work Phone Number is required!" />
                                            </ValidationSettings>
                                        </dx:BootstrapTextBox>
                                    </div>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-4">
                                    <div class="form-group">
                                        <dx:BootstrapTextBox ID="txtStreet" runat="server" Caption="Street Address">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ValidationSettings ValidationGroup="vgEditUser" ErrorDisplayMode="ImageWithText">
                                                <RequiredField IsRequired="false" ErrorText="Street Address is required!" />
                                            </ValidationSettings>
                                        </dx:BootstrapTextBox>
                                    </div>
                                </div>
                                <div class="col-md-4">
                                    <div class="form-group">
                                        <dx:BootstrapTextBox ID="txtCity" runat="server" Caption="City">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ValidationSettings ValidationGroup="vgEditUser" ErrorDisplayMode="ImageWithText">
                                                <RequiredField IsRequired="false" ErrorText="City is required!" />
                                            </ValidationSettings>
                                        </dx:BootstrapTextBox>
                                    </div>
                                </div>
                                <div class="col-md-4">
                                    <div class="form-group">
                                        <dx:BootstrapComboBox ID="ddState" runat="server" Caption="State" NullText="--Select--" AllowNull="true"
                                            TextField="Name" ValueField="Name" ValueType="System.String" ClearButton-DisplayMode="Always">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ValidationSettings ValidationGroup="vgEditUser" ErrorDisplayMode="ImageWithText">
                                                <RequiredField IsRequired="false" ErrorText="State is required!" />
                                            </ValidationSettings>
                                        </dx:BootstrapComboBox>
                                    </div>
                                </div>
                                <div class="col-md-4">
                                    <div class="form-group">
                                        <dx:BootstrapTextBox ID="txtZIPCode" runat="server" Caption="ZIP Code">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ValidationSettings ValidationGroup="vgEditUser" ErrorDisplayMode="ImageWithText">
                                                <RequiredField IsRequired="false" ErrorText="ZIP Code is required!" />
                                            </ValidationSettings>
                                        </dx:BootstrapTextBox>
                                    </div>
                                </div>
                                <div class="col-md-4">
                                    <div class="form-group">
                                        <dx:BootstrapTextBox ID="txtRegionLocation" runat="server" Caption="What region are you located in?">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ValidationSettings ValidationGroup="vgEditUser" ErrorDisplayMode="ImageWithText">
                                                <RequiredField IsRequired="false" ErrorText="Region Location is required!" />
                                            </ValidationSettings>
                                        </dx:BootstrapTextBox>
                                    </div>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-4">
                                    <div class="form-group">
                                        <dx:BootstrapComboBox ID="ddIdentityRole" runat="server" Caption="Identity Role" NullText="--Select--"
                                            TextField="Name" ValueField="Id" ValueType="System.String"
                                            IncrementalFilteringMode="StartsWith" AllowMouseWheel="false">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ValidationSettings ValidationGroup="vgEditUser" ErrorDisplayMode="ImageWithText">
                                                <RequiredField IsRequired="true" ErrorText="Identity Role is required!" />
                                            </ValidationSettings>
                                        </dx:BootstrapComboBox>
                                    </div>
                                </div>
                                <div class="col-md-4">
                                    <div class="form-group">
                                        <dx:BootstrapComboBox ID="ddAccountEnabled" runat="server" NullText="--Select--" Caption="Account Enabled?"
                                            ValueType="System.Boolean" IncrementalFilteringMode="StartsWith">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <ValidationSettings ErrorDisplayMode="ImageWithText" ValidationGroup="vgCoachingLog">
                                                <RequiredField IsRequired="true" ErrorText="Account Enabled is required!" />
                                            </ValidationSettings>
                                            <Items>
                                                <dx:BootstrapListEditItem Text="Yes" Value="True"></dx:BootstrapListEditItem>
                                                <dx:BootstrapListEditItem Text="No" Value="False"></dx:BootstrapListEditItem>
                                            </Items>
                                        </dx:BootstrapComboBox>
                                    </div>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-4">
                                    <div class="form-group">
                                        <dx:BootstrapDateEdit ID="deLockoutEndDate" runat="server" Caption="Lockout End Date" AllowNull="true"
                                            EditFormatString="MM/dd/yyyy" UseMaskBehavior="true" AllowMouseWheel="false"
                                            PickerDisplayMode="Calendar" MinDate="01/01/1900">
                                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                            <SettingsAdaptivity Mode="OnWindowInnerWidth" SwitchToModalAtWindowInnerWidth="750" />
                                            <CalendarProperties ShowClearButton="true"></CalendarProperties>
                                            <ValidationSettings ValidationGroup="vgEditUser" ErrorDisplayMode="ImageWithText">
                                                <RequiredField IsRequired="false" ErrorText="Lockout End Date is required!" />
                                            </ValidationSettings>
                                        </dx:BootstrapDateEdit>
                                    </div>
                                </div>
                            </div>
                            <div id="divChangePassword" runat="server" class="row">
                                <div class="col-md-6">
                                    <div class="form-group">
                                        <label for="btnChangePassword" class="control-label">Password</label>
                                        <button id="btnChangePassword" type="button" class="d-block btn btn-primary"><i class="fas fa-key"></i>&nbsp;Change Password</button>
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
                        <div class="card-header">Program Roles</div>
                        <div class="card-body">
                            <label>All Program Roles</label>
                            <asp:Repeater ID="repeatUserRoles" runat="server" ItemType="Pyramid.Models.UserProgramRole">
                                <HeaderTemplate>
                                    <table id="tblUserRoles" class="table table-striped table-bordered table-hover">
                                        <thead>
                                            <tr>
                                                <th data-priority="1"></th>
                                                <th data-priority="2">Role</th>
                                                <th>Program</th>
                                                <th data-priority="3"></th>
                                            </tr>
                                        </thead>
                                        <tbody>
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <tr>
                                        <td></td>
                                        <td>
                                            <%# Item.CodeProgramRole.RoleName %>
                                            <asp:HiddenField ID="hfProgramRoleFK" runat="server" Value='<%# Item.ProgramRoleCodeFK %>' />
                                        </td>
                                        <td>
                                            <%# Item.Program.ProgramName %>
                                            <asp:HiddenField ID="hfProgramFK" runat="server" Value='<%# Item.ProgramFK %>' />
                                        </td>
                                        <td>
                                            <asp:LinkButton ID="lbDeleteRole" runat="server" CssClass="btn btn-loader btn-danger" OnClick="lbDeleteRole_Click"><i class="fas fa-trash"></i> Remove</asp:LinkButton>
                                            <!-- Need to use labels so that values are maintained after postback (inputs get cleared because of an interaction with DataTables and the repeater) -->
                                            <asp:Label ID="lblUserProgramRolePK" runat="server" Visible="false" Text='<%# Item.UserProgramRolePK %>'></asp:Label>
                                        </td>
                                    </tr>
                                </ItemTemplate>
                                <FooterTemplate>
                                    </tbody>
                                        </table>
                                </FooterTemplate>
                            </asp:Repeater>
                            <div class="mt-2 card">
                                <div class="card-header">
                                    Add New Role
                                </div>
                                <div class="card-body">
                                    <div class="row">
                                        <div class="col-md-4">
                                            <div class="form-group">
                                                <dx:BootstrapComboBox ID="ddProgramRole" runat="server" Caption="Role" NullText="--Select--"
                                                    TextField="RoleName" ValueField="CodeProgramRolePK" ValueType="System.Int32"
                                                    IncrementalFilteringMode="StartsWith" AllowMouseWheel="false"
                                                    OnValidation="ddProgramRole_Validation" ClientInstanceName="ddProgramRole" EnableClientSideAPI="true">
                                                    <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                    <ClientSideEvents Validation="validateProgramRole" />
                                                    <ValidationSettings ValidationGroup="vgAddRole" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                                        <RequiredField IsRequired="true" ErrorText="Role is required!" />
                                                    </ValidationSettings>
                                                </dx:BootstrapComboBox>
                                            </div>
                                        </div>
                                        <div class="col-md-4">
                                            <div class="form-group">
                                                <dx:BootstrapComboBox ID="ddProgram" runat="server" Caption="Program (Hub)" NullText="--Select--"
                                                    TextField="ProgramName" ValueField="ProgramPK" ValueType="System.Int32"
                                                    IncrementalFilteringMode="Contains" AllowMouseWheel="false"
                                                    ClientInstanceName="ddProgram">
                                                    <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                                                    <ValidationSettings ValidationGroup="vgAddRole" ErrorDisplayMode="ImageWithText">
                                                        <RequiredField IsRequired="true" ErrorText="Program is required!" />
                                                    </ValidationSettings>
                                                </dx:BootstrapComboBox>
                                            </div>
                                        </div>
                                        <div class="col-md-4 d-flex align-items-end">
                                            <div class="form-group">
                                                <dx:BootstrapButton ID="btnAddRole" runat="server" Text="Add Role" OnClick="btnAddRole_Click" ValidationGroup="vgAddRole" SettingsBootstrap-RenderOption="success">
                                                    <CssClasses Icon="fas fa-plus" />
                                                </dx:BootstrapButton>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                </div>
            </div>
            <div class="page-footer">
                <uc:Submit ID="submitUser" runat="server" ValidationGroup="vgEditUser"
                    ControlCssClass="center-content"
                    OnSubmitClick="submitUser_Click" OnCancelClick="submitUser_CancelClick"
                    OnValidationFailed="submitUser_ValidationFailed" />
            </div>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="btnSavePassword" />
        </Triggers>
    </asp:UpdatePanel>
    <div class="modal fade" id="divChangePasswordModal" tabindex="-1" role="dialog" aria-labelledby="passwordModalTitle">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="passwordModalTitle">Change Password</h5>
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                        <span aria-hidden="true">&times;</span>
                    </button>
                </div>
                <div class="modal-body">
                    <div class="alert alert-primary mt-2">
                        <i class="fas fa-info-circle"></i>&nbsp;Password Requirements:
                        <ul>
                            <li>Must be at least 8 characters in length.</li>
                            <li>Must contain at least 1 uppercase letter.</li>
                            <li>Must contain at least 1 lowercase letter.</li>
                            <li>Must contain at least 1 number.</li>
                            <li>Must contain at least 1 special symbol (e.g. !?@&).</li>
                        </ul>
                    </div>
                    <div class="form-group">
                        <dx:BootstrapTextBox ID="txtPassword" ClientInstanceName="txtPassword" Password="true" runat="server" Caption="New Password">
                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                            <ValidationSettings ValidationGroup="vgChangePassword" ErrorDisplayMode="ImageWithText">
                                <RequiredField IsRequired="true" ErrorText="New Password is required!" />
                            </ValidationSettings>
                        </dx:BootstrapTextBox>
                    </div>
                    <div class="form-group">
                        <dx:BootstrapTextBox ID="txtConfirmPassword" runat="server" Password="true" Caption="Confirm New Password" EnableClientSideAPI="true" OnValidation="txtConfirmPassword_Validation">
                            <CaptionSettings RequiredMarkDisplayMode="Hidden" ShowColon="false" />
                            <ClientSideEvents Validation="validateConfirmPassword" />
                            <ValidationSettings ValidationGroup="vgChangePassword" ErrorDisplayMode="ImageWithText" EnableCustomValidation="true">
                                <RequiredField IsRequired="true" ErrorText="Confirm New Password is required!" />
                            </ValidationSettings>
                        </dx:BootstrapTextBox>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;Close</button>
                    <dx:BootstrapButton ID="btnSavePassword" runat="server" Text="Save" OnClick="btnSavePassword_Click" AutoPostBack="true" ValidationGroup="vgChangePassword" SettingsBootstrap-RenderOption="success">
                        <CssClasses Icon="fas fa-save" Control="btn-loader" />
                    </dx:BootstrapButton>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
