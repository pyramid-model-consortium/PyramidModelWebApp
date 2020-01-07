<%@ Page Title="Select Role" Language="C#" MasterPageFile="~/MasterPages/NotLoggedIn.master" AutoEventWireup="true" CodeBehind="SelectRole.aspx.cs" Inherits="Pyramid.Account.SelectRole" %>
<%@ Register TagPrefix="uc" TagName="Messaging" Src="~/User_Controls/MessagingSystem.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ScriptContent" runat="server">
    <script>
        $(document).ready(function () {
            //Run the initial page init
            initializePage();
        });

        //Initializes the page
        function initializePage() {
            //Allow date format for DataTables sorting
            $.fn.dataTable.moment('MM/DD/YYYY');

            //Initialize the datatable
            if (!$.fn.dataTable.isDataTable('#tblUserRoles')) {
                $('#tblUserRoles').DataTable({
                    responsive: true,
                    columnDefs: [
                        { orderable: false, targets: [2] }
                    ],
                    order: [[ 1, 'asc' ], [ 0, 'asc' ]],
                    pageLength: 10,
                    dom: 'frtp',
                    language: {
                        searchPlaceholder: "Enter text to search...",
                        search: "",
                    }
                });
                $('.dataTables_filter input').removeClass('form-control-sm');
            }
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <uc:Messaging ID="msgSys" runat="server" />
    <div class="row">
        <div class="col-md-12">
            <asp:Repeater ID="repeatUserRoles" runat="server" ItemType="Pyramid.Models.UserProgramRole">
                <HeaderTemplate>
                    <table id="tblUserRoles" class="table table-striped table-bordered table-hover">
                        <thead>
                            <tr>
                                <th>Role</th>
                                <th>Program</th>
                                <th class="all"></th>
                            </tr>
                        </thead>
                        <tbody>
                </HeaderTemplate>
                <ItemTemplate>
                            <tr>
                                <td><%# Item.CodeProgramRole.RoleName %></td>
                                <td><%# Item.Program.ProgramName %></td>
                                <td class="text-center">
                                    <asp:LinkButton ID="lbSelectRole" runat="server" CssClass="btn btn-loader btn-primary" OnClick="lbSelectRole_Click"><i class="fas fa-mouse-pointer"></i> Select</asp:LinkButton>
                                    <asp:HiddenField ID="hfUserProgramRolePK" runat="server" Value='<%# Item.UserProgramRolePK %>' />
                                    <asp:HiddenField ID="hfProgramRoleFK" runat="server" Value='<%# Item.CodeProgramRole.CodeProgramRolePK %>' />
                                    <asp:HiddenField ID="hfProgramRoleName" runat="server" Value='<%# Item.CodeProgramRole.RoleName %>' />
                                    <asp:HiddenField ID="hfprogramRoleAllowedToEdit" runat="server" Value='<%# Item.CodeProgramRole.AllowedToEdit %>' />
                                    <asp:HiddenField ID="hfProgramFK" runat="server" Value='<%# Item.ProgramFK %>' />
                                    <asp:HiddenField ID="hfProgramName" runat="server" Value='<%# Item.Program.ProgramName %>' />
                                </td>
                            </tr>
                </ItemTemplate>
                <FooterTemplate>
                        </tbody>
                    </table>
                </FooterTemplate>
            </asp:Repeater>
        </div>
    </div>
</asp:Content>
