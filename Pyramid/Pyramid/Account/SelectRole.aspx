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
                    responsive: {
                        details: {
                            type: 'column'
                        }
                    },
                    columnDefs: [
                        { orderable: false, targets: [5] },
                        { className: 'control', orderable: false, targets: 0 }
                    ],
                    order: [[4, 'asc'], [2, 'asc'], [1, 'asc']],
                    stateSave: true,
                    stateDuration: 60,
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
                                <th data-priority="1"></th>
                                <th data-priority="2">Role</th>
                                <th data-priority="4">Program</th>
                                <th data-priority="5">Hub</th>
                                <th data-priority="6">State</th>
                                <th data-priority="3"></th>
                            </tr>
                        </thead>
                        <tbody>
                </HeaderTemplate>
                <ItemTemplate>
                            <tr>
                                <td></td>
                                <td><%# Item.CodeProgramRole.RoleName %></td>
                                <td><%# Item.Program.ProgramName %></td>
                                <td><%# Item.Program.Hub.Name %></td>
                                <td><%# Item.Program.State.Name %></td>
                                <td class="text-center">
                                    <asp:LinkButton ID="lbSelectRole" runat="server" CssClass="btn btn-loader btn-primary" OnClick="lbSelectRole_Click"><i class="fas fa-mouse-pointer"></i> Select</asp:LinkButton>
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
        </div>
    </div>
</asp:Content>
