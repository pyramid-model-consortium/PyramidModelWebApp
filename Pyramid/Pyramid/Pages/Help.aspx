<%@ Page Title="Help and Training" Language="C#" MasterPageFile="~/MasterPages/LoggedIn.master" AutoEventWireup="true" CodeBehind="Help.aspx.cs" Inherits="Pyramid.Pages.Help" %>

<%@ Register Assembly="DevExpress.Web.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Data.Linq" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.Bootstrap.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>
<%@ Register Assembly="DevExpress.Web.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web" TagPrefix="dx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ScriptContent" runat="server">
    <script type="text/javascript">
        $(document).ready(function () {
            //Run the initial page init
            initializePage();

            //Run the page init after every AJAX load
            var requestManager = Sys.WebForms.PageRequestManager.getInstance();
            requestManager.add_endRequest(initializePage);
        });

        //Initializes the page
        function initializePage() {
            //Get the state admin name
            var stateAdminName = $('[ID$="hfStateAdminName"]').val();

            //Set any labels or spans
            $('.state-admin-name').html(stateAdminName);

            //Set up the show event for the role permission collapse
            $('#collapseRolePermissions').on('shown.bs.collapse', function () {
                //Resize the datatable
                $('#tblRolePermissions').DataTable().columns.adjust().responsive.recalc();
            });

            //Initialize the datatables
            if (!$.fn.dataTable.isDataTable('#tblRolePermissions')) {
                $('#tblRolePermissions').DataTable({
                    responsive: {
                        details: {
                            type: 'column'
                        }
                    },
                    columnDefs: [
                        { className: 'control', orderable: false, targets: 0 },
                        { width: '10px', targets: 1 },
                        { visible: false, targets: 2 }
                    ],
                    ordering: false,
                    rowGroup: {
                        dataSrc: 2
                    },
                    pageLength: 22,
                    dom: 'frtp',
                    language: {
                        searchPlaceholder: "Enter text to search...",
                        search: "",
                    }
                });
            }
            $('.dataTables_filter input').removeClass('form-control-sm');
        }
    </script>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <asp:HiddenField ID="hfStateAdminName" runat="server" Value="state administration" />
    <div class="row">
        <div class="col-sm-12">
            <div class="card bg-light">
                <div class="collapse-module">
                    <div class="card-header">
                        <a class="collapsed collapse-module-button" data-toggle="collapse" href="#collapseTrainingVideos" aria-expanded="false" aria-controls="collapseTrainingVideos">
                            <i class="collapse-module-icon"></i>&nbsp;Training Videos
                        </a>
                    </div>
                    <div class="card-body pb-0">
                        <div id="collapseTrainingVideos" aria-expanded="false" class="collapse collapse-preview">
                            <div class="row">
                                <div class="col-sm-12">
                                    <div class="alert alert-primary">
                                        <i class="fas fa-info-circle"></i>&nbsp;All personal and organizational names that are shown in these videos are fictitious.  Any similarity to actual persons or organizations is purely coincidental.
                                    </div>
                                </div>
                            </div>
                            <div class="row">
                                <!-- User Account Training -->
                                <div class="col-lg-4 text-center">
                                    <div class="embed-responsive embed-responsive-16by9 border border-dark">
                                        <iframe class="embed-responsive-item" src="https://www.youtube-nocookie.com/embed/A_IkMuwT3iw" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
                                    </div>
                                </div>
                                <!-- Browser Training -->
                                <div class="col-lg-4 text-center">
                                    <div class="embed-responsive embed-responsive-16by9 border border-dark">
                                        <iframe class="embed-responsive-item" src="https://www.youtube-nocookie.com/embed/ZcnBIGqen1k" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
                                    </div>
                                </div>
                                <!-- Dashboard Training -->
                                <div class="col-lg-4 text-center">
                                    <div class="embed-responsive embed-responsive-16by9 border border-dark">
                                        <iframe class="embed-responsive-item" src="https://www.youtube-nocookie.com/embed/FYpKaytYxMA" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
                                    </div>
                                </div>
                            </div>
                            <div class="row">
                                <!-- Data-Entry Training -->
                                <div class="col-lg-4 text-center">
                                    <div class="embed-responsive embed-responsive-16by9 border border-dark">
                                        <iframe class="embed-responsive-item" src="https://www.youtube-nocookie.com/embed/pLGwjvIsy9E" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
                                    </div>
                                </div>
                                <!-- Uploading Files Training -->
                                <div class="col-lg-4 text-center">
                                    <div class="embed-responsive embed-responsive-16by9 border border-dark">
                                        <iframe class="embed-responsive-item" src="https://www.youtube-nocookie.com/embed/DfURLKbUCvs" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
                                    </div>
                                </div>
                                <!-- Deletion Training -->
                                <div class="col-lg-4 text-center">
                                    <div class="embed-responsive embed-responsive-16by9 border border-dark">
                                        <iframe class="embed-responsive-item" src="https://www.youtube-nocookie.com/embed/SI2SEpIgZDo" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
                                    </div>
                                </div>
                            </div>
                            <div class="row">
                                <!-- Reports Training -->
                                <div class="col-lg-4 text-center">
                                    <div class="embed-responsive embed-responsive-16by9 border border-dark">
                                        <iframe class="embed-responsive-item" src="https://www.youtube-nocookie.com/embed/x-iKy9wqwpE" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
                                    </div>
                                </div>
                                <!-- User Roles Training -->
                                <div class="col-lg-4 text-center">
                                    <div class="embed-responsive embed-responsive-16by9 border border-dark">
                                        <iframe class="embed-responsive-item" src="https://www.youtube-nocookie.com/embed/8jeBRPJ-Aa0" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
                                    </div>
                                </div>
                                <!-- News Training -->
                                <div class="col-lg-4 text-center">
                                    <div class="embed-responsive embed-responsive-16by9 border border-dark">
                                        <iframe class="embed-responsive-item" src="https://www.youtube-nocookie.com/embed/nDCcXkqltR4" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
                                    </div>
                                </div>
                            </div>
                            <div class="row">
                                <!-- Order of Entry Training -->
                                <div class="col-lg-4 text-center">
                                    <div class="embed-responsive embed-responsive-16by9 border border-dark">
                                        <iframe class="embed-responsive-item" src="https://www.youtube-nocookie.com/embed/fta62wS_oDA" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
                                    </div>
                                </div>
                                <!-- Error Training -->
                                <div class="col-lg-4 text-center">
                                    <div class="embed-responsive embed-responsive-16by9 border border-dark">
                                        <iframe class="embed-responsive-item" src="https://www.youtube-nocookie.com/embed/roRuh-ddYsc" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
                                    </div>
                                </div>
                                <!-- Help Page Training -->
                                <div class="col-lg-4 text-center">
                                    <div class="embed-responsive embed-responsive-16by9 border border-dark">
                                        <iframe class="embed-responsive-item" src="https://www.youtube-nocookie.com/embed/xk1og4-RDzg" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>
                                    </div>
                                </div>
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
                <div class="collapse-module">
                    <div class="card-header">
                        <a class="collapsed collapse-module-button" data-toggle="collapse" href="#collapseRolePermissions" aria-expanded="false" aria-controls="collapseRolePermissions">
                            <i class="collapse-module-icon"></i>&nbsp;User Role Descriptions and Permissions
                        </a>
                    </div>
                    <div class="card-body pb-0 pt-0">
                        <div id="collapseRolePermissions" aria-expanded="false" class="collapse">
                            <div class="mt-2 mb-2">
                                <p>User roles and their corresponding permissions to access/modify/delete forms are listed below.</p>
                                <p>Your current role will appear as the first role in the list.  You can check your role at any time by clicking on your username at the top of the screen.</p>
                                <asp:Repeater ID="repeatRolePermissions" runat="server" ItemType="Pyramid.Models.CodeProgramRolePermission">
                                    <HeaderTemplate>
                                        <table id="tblRolePermissions" class="table table-striped table-bordered table-hover">
                                            <thead>
                                                <tr>
                                                    <th data-priority="1"></th>
                                                    <th data-priority="1"></th>
                                                    <th data-priority="2">Role Name</th>
                                                    <th data-priority="3">Form Name</th>
                                                    <th data-priority="4">View Dashboard?</th>
                                                    <th data-priority="5">View Form?</th>
                                                    <th data-priority="6">Add New Forms?</th>
                                                    <th data-priority="7">Edit Existing Forms?</th>
                                                    <th data-priority="8">Delete Existing Forms?</th>
                                                </tr>
                                            </thead>
                                            <tbody>
                                    </HeaderTemplate>
                                    <ItemTemplate>
                                                <tr>
                                                    <td></td>
                                                    <td></td>
                                                    <td>
                                                        <p><%# Item.CodeProgramRole.RoleName %></p>
                                                        <p style="font-weight: normal !important;">
                                                            <%# Item.CodeProgramRole.RoleDescription %>
                                                        </p>
                                                        <p style="font-weight: normal !important;">
                                                            Allowed to view protected child information (names):&nbsp;<i class="fas fa-lg <%# (Item.CodeProgramRole.ViewPrivateChildInfo ? "fa-check green-text" : "fa-times red-text") %>"></i>
                                                            <br />
                                                            Allowed to view protected professional information (names, emails):&nbsp;<i class="fas fa-lg <%# (Item.CodeProgramRole.ViewPrivateEmployeeInfo ? "fa-check green-text" : "fa-times red-text") %>"></i>
                                                        </p>
                                                    </td>
                                                    <td><%# Item.CodeForm.FormName %></td>
                                                    <td><i class="fas fa-lg <%# (Item.AllowedToViewDashboard ? "fa-check green-text" : "fa-times red-text") %>"></i></td>
                                                    <td><i class="fas fa-lg <%# (Item.AllowedToView ? "fa-check green-text" : "fa-times red-text") %>"></i></td>
                                                    <td><i class="fas fa-lg <%# (Item.AllowedToAdd ? "fa-check green-text" : "fa-times red-text") %>"></i></td>
                                                    <td><i class="fas fa-lg <%# (Item.AllowedToEdit ? "fa-check green-text" : "fa-times red-text") %>"></i></td>
                                                    <td><i class="fas fa-lg <%# (Item.AllowedToDelete ? "fa-check green-text" : "fa-times red-text") %>"></i></td>
                                                </tr>
                                    </ItemTemplate>
                                    <FooterTemplate>
                                            </tbody>
                                        </table>
                                    </FooterTemplate>
                                </asp:Repeater>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <div class="row">
        <div class="col-lg-6">
            <div class="card bg-light">
                <div class="collapse-module">
                    <div class="card-header">
                        <a class="collapse-module-button" data-toggle="collapse" href="#collapseFAQ" aria-expanded="false" aria-controls="collapseFAQ">
                            <i class="collapse-module-icon"></i>&nbsp;Frequently Asked Questions
                        </a>
                    </div>
                    <div class="card-body pb-0 pt-0">
                        <div id="collapseFAQ" aria-expanded="false" class="collapse show m-3">
                            <div class="mb-2">
                                <a class="collapsed collapse-module-button" data-toggle="collapse" href="#collapseFAQ15" aria-expanded="false" aria-controls="collapseFAQ15">
                                    <i class="collapse-module-icon"></i>&nbsp;How do I add someone to PIDS?
                                </a>
                                <div id="collapseFAQ15" aria-expanded="false" class="collapse">
                                    <p class="ml-3 mt-3">
                                        If someone needs access to PIDS, you should contact your PIDS state leadership team and ask them
                                        to add the person to the system as a new user.  In that request, please supply them
                                        with the following:
                                    </p>
                                    <ul>
                                        <li>The new person's full name.</li>
                                        <li>The new person's primary email address.</li>
                                        <li>The PIDS program they should have access to.  This is usually the program that you are in.</li>
                                        <li>The user role(s) that the person should have.  A description of the roles and how they work is in the 'User Role Descriptions and Permissions' section above.</li>
                                    </ul>
                                </div>
                                <hr />
                            </div>
                            <div class="mb-2">
                                <a class="collapsed collapse-module-button" data-toggle="collapse" href="#collapseFAQ16" aria-expanded="false" aria-controls="collapseFAQ16">
                                    <i class="collapse-module-icon"></i>&nbsp;What should I do if a PIDS user leaves my program?
                                </a>
                                <div id="collapseFAQ16" aria-expanded="false" class="collapse">
                                    <p class="ml-3 mt-3">
                                        If a PIDS user leaves your program, please contact your PIDS state leadership team and ask them
                                        to revoke that user's access to PIDS.  Once the state leadership team does that, the user will
                                        no longer have access to your program's information.
                                    </p>
                                </div>
                                <hr />
                            </div>
                            <div class="mb-2">
                                <a class="collapsed collapse-module-button" data-toggle="collapse" href="#collapseFAQ10" aria-expanded="false" aria-controls="collapseFAQ10">
                                    <i class="collapse-module-icon"></i>&nbsp;Should I enter data from before the system went live?
                                </a>
                                <div id="collapseFAQ10" aria-expanded="false" class="collapse">
                                    <p class="ml-3 mt-3">
                                        We ask that you enter old Benchmarks of Quality forms, TPOT observations, 
                                        and TPITOS observations into the system, but
                                        you do not need to enter any other old items like Behavior Incident Reports if you don't want to.
                                        However, if you want to enter all your old data, that would be great!
                                    </p>
                                </div>
                                <hr />
                            </div>
                            <div class="mb-2">
                                <a class="collapsed collapse-module-button" data-toggle="collapse" href="#collapseFAQ9" aria-expanded="false" aria-controls="collapseFAQ9">
                                    <i class="collapse-module-icon"></i>&nbsp;How do I add my program's action plan to the system?
                                </a>
                                <div id="collapseFAQ9" aria-expanded="false" class="collapse">
                                    <p class="ml-3 mt-3">
                                        All action plans should be added to the system by uploading the document via the 'Uploaded Files'
                                        dashboard.
                                    </p>
                                </div>
                                <hr />
                            </div>
                            <div class="mb-2">
                                <a class="collapsed collapse-module-button" data-toggle="collapse" href="#collapseFAQ1" aria-expanded="false" aria-controls="collapseFAQ1">
                                    <i class="collapse-module-icon"></i>&nbsp;What files should I upload?
                                </a>
                                <div id="collapseFAQ1" aria-expanded="false" class="collapse">
                                    <p class="ml-3 mt-3">
                                        The main purpose of the 'Uploaded Files' dashboard is so that you can upload action plans.  
                                        However, you can upload any file that will be helpful to your program/hub.</p>
                                </div>
                                <hr />
                            </div>
                            <div class="mb-2">
                                <a class="collapsed collapse-module-button" data-toggle="collapse" href="#collapseFAQ2" aria-expanded="false" aria-controls="collapseFAQ2">
                                    <i class="collapse-module-icon"></i>&nbsp;What is the 'Source' field for file uploads?
                                </a>
                                <div id="collapseFAQ2" aria-expanded="false" class="collapse">
                                    <p class="ml-3 mt-3">
                                        The source field determines who will be able to see the file upload.  Uploads with a 
                                        source of program-wide will only show up for your program, and uploads with a source of hub-wide will 
                                        only show up for programs inside of your hub.  There are also cohort-wide and state-wide file uploads.  
                                        Those uploads are available to all programs inside of a cohort or state respectively.</p>
                                </div>
                                <hr />
                            </div>
                            <div class="mb-2">
                                <a class="collapsed collapse-module-button" data-toggle="collapse" href="#collapseFAQ3" aria-expanded="false" aria-controls="collapseFAQ3">
                                    <i class="collapse-module-icon"></i>&nbsp;Why did the system give me an error when I tried to delete something?
                                </a>
                                <div id="collapseFAQ3" aria-expanded="false" class="collapse">
                                    <p class="ml-3 mt-3">
                                        The system will not allow you to delete anything that has related records in the system.  
                                        For example, you cannot delete a classroom if that classroom was selected on a classroom coaching log 
                                        because the classroom is related to the coaching log and deleting the classroom would invalidate the 
                                        coaching log.</p>
                                </div>
                                <hr />
                            </div>
                            <div class="mb-2">
                                <a class="collapsed collapse-module-button" data-toggle="collapse" href="#collapseFAQ4" aria-expanded="false" aria-controls="collapseFAQ4">
                                    <i class="collapse-module-icon"></i>&nbsp;What items should I delete?
                                </a>
                                <div id="collapseFAQ4" aria-expanded="false" class="collapse">
                                    <p class="ml-3 mt-3">
                                        You should only delete items in the system that were entered accidentally or that 
                                        are duplicates of existing information.  If a child or professional is no longer with your 
                                        program, <b>don't</b> delete them!  Just discharge or separate them in the system.</p>
                                </div>
                                <hr />
                            </div>
                            <div class="mb-2">
                                <a class="collapsed collapse-module-button" data-toggle="collapse" href="#collapseFAQ5" aria-expanded="false" aria-controls="collapseFAQ5">
                                    <i class="collapse-module-icon"></i>&nbsp;How do I discharge a child?
                                </a>
                                <div id="collapseFAQ5" aria-expanded="false" class="collapse">
                                    <p class="ml-3 mt-3">
                                        To discharge a child, just go to the children dashboard, click the edit button 
                                        for the child's record, enter the discharge date and discharge reason in the data-entry form, 
                                        and save the form.</p>
                                </div>
                                <hr />
                            </div>
                            <div class="mb-2">
                                <a class="collapsed collapse-module-button" data-toggle="collapse" href="#collapseFAQ6" aria-expanded="false" aria-controls="collapseFAQ6">
                                    <i class="collapse-module-icon"></i>&nbsp;How do I remove a professional?
                                </a>
                                <div id="collapseFAQ6" aria-expanded="false" class="collapse">
                                    <p class="ml-3 mt-3">
                                        To remove a professional, just go to the professionals dashboard, click the edit button 
                                        for the professional's record, enter the separation date and separation reason in the data-entry form, 
                                        and save the form.
                                    </p>
                                    <p class="ml-3 mt-3">
                                        If the professional has access to PIDS, please contact your PIDS state
                                        leadership team and ask them to revoke the professional's access to PIDS.
                                    </p>
                                </div>
                                <hr />
                            </div>
                            <div class="mb-2">
                                <a class="collapsed collapse-module-button" data-toggle="collapse" href="#collapseFAQ14" aria-expanded="false" aria-controls="collapseFAQ14">
                                    <i class="collapse-module-icon"></i>&nbsp;How do I reinstate a separated professional?
                                </a>
                                <div id="collapseFAQ14" aria-expanded="false" class="collapse">
                                    <p class="ml-3 mt-3">
                                        To reinstate a separated professional, just go to the professionals dashboard, click the edit button 
                                        for the professional's record, remove the separation date and separation reason in the data-entry form, 
                                        and save the form.
                                    </p>
                                    <p class="ml-3 mt-3">
                                        If the professional had access to PIDS, and access was revoked after separation, please contact your PIDS state
                                        leadership team and ask them to reinstate the professional's access.
                                    </p>
                                </div>
                                <hr />
                            </div>
                            <div class="mb-2">
                                <a class="collapsed collapse-module-button" data-toggle="collapse" href="#collapseFAQ7" aria-expanded="false" aria-controls="collapseFAQ7">
                                    <i class="collapse-module-icon"></i>&nbsp;Why is a child or professional not showing up as an option on a 
                                    data-entry form?
                                </a>
                                <div id="collapseFAQ7" aria-expanded="false" class="collapse">
                                    <p class="ml-3 mt-3">
                                        This means one of two things.  The first possibility is that the child or professional is 
                                        not in the system yet.  Check the dashboard for the child or professional to make sure that they have been 
                                        entered into the system.  
                                        <br /><br />The second possibility is that the child or professional is not active in the program.  
                                        Check their record in the system to see if a discharge or separation date exists.  
                                        If there is a discharge or separation date, the child or professional will not be an option on data-entry 
                                        forms that are dated after that discharge or separation date.
                                    </p>
                                </div>
                                <hr />
                            </div>
                            <div class="mb-2">
                                <a class="collapsed collapse-module-button" data-toggle="collapse" href="#collapseFAQ12" aria-expanded="false" aria-controls="collapseFAQ12">
                                    <i class="collapse-module-icon"></i>&nbsp;Why is a TPOT or TPITOS observer not available as an option on the data-entry forms?
                                </a>
                                <div id="collapseFAQ12" aria-expanded="false" class="collapse">
                                    <div class="ml-3 mt-3">
                                        Observers need to fulfill the following criteria in order to show up as an option on the data-entry forms:
                                        <ul>
                                            <li>
                                                <asp:Label ID="lblTPOTTrainingFAQ" runat="server" Text=""></asp:Label>
                                            </li>
                                            <li>The training must be dated before the TPOT or TPITOS date.</li>
                                            <li>The training expiration date must be after the TPOT or TPITOS date.</li>
                                        </ul>
                                    </div>
                                </div>
                                <hr />
                            </div>
                            <div class="mb-2">
                                <a class="collapsed collapse-module-button" data-toggle="collapse" href="#collapseFAQ13" aria-expanded="false" aria-controls="collapseFAQ13">
                                    <i class="collapse-module-icon"></i>&nbsp;Why is a classroom coach not available as an option on the Classroom Coaching Log data-entry form?
                                </a>
                                <div id="collapseFAQ13" aria-expanded="false" class="collapse">
                                    <div class="ml-3 mt-3">
                                        Classroom coaches need to fulfill the following criteria in order to show up as an option on the Classroom Coaching Log data-entry form:
                                        <ul>
                                            <li>
                                                <asp:Label ID="lblCoachingTrainingFAQ" runat="server" Text=""></asp:Label>
                                            </li>
                                            <li>The training(s) must be dated before the coaching log date.</li>
                                        </ul>
                                    </div>
                                </div>
                                <hr />
                            </div>
                            <div class="mb-2">
                                <a class="collapsed collapse-module-button" data-toggle="collapse" href="#collapseFAQ8" aria-expanded="false" aria-controls="collapseFAQ8">
                                    <i class="collapse-module-icon"></i>&nbsp;Why haven't I received a reply to my support ticket?
                                </a>
                                <div id="collapseFAQ8" aria-expanded="false" class="collapse">
                                    <p class="ml-3 mt-3">
                                        All support ticket confirmations and responses will go to the email address that was 
                                        supplied by you on the support ticket page.  This email is usually the email address associated with your user 
                                        account, but you can manually change the address when you submit the support ticket.  If you didn't receive 
                                        an email with confirmation that your support ticket was submitted, please check your spam/junk email 
                                        folders.  If there is still no confirmation email, please submit the support ticket again and ensure that the email address on the support ticket
                                        form is correct.
                                        <br />
                                        <br />
                                        If you received the confirmation email, but have not received an email with a response to your ticket,
                                        please be patient and we will respond to you as quickly as possible.
                                    </p>
                                </div>
                                <hr />
                            </div>
                            <div class="mb-2">
                                <a class="collapsed collapse-module-button" data-toggle="collapse" href="#collapseFAQ11" aria-expanded="false" aria-controls="collapseFAQ11">
                                    <i class="collapse-module-icon"></i>&nbsp;Can I upload my Excel files that I used to send to <span class="state-admin-name">state administration</span>?
                                </a>
                                <div id="collapseFAQ11" aria-expanded="false" class="collapse">
                                    <p class="ml-3 mt-3">
                                        No.  The system does not have a process that would extract the data from those Excel files. 
                                        <span class="font-weight-bold">Also, you should never upload those Excel files via
                                        the 'Uploaded Files' dashboard because those Excel files contain information
                                        that some users may not be authorized to access.</span>
                                    </p>
                                </div>
                                <hr />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div class="col-lg-6">
            <div class="card bg-light">
                <div class="collapse-module">
                    <div class="card-header">
                        <a class="collapse-module-button" data-toggle="collapse" href="#collapseTips" aria-expanded="false" aria-controls="collapseTips">
                            <i class="collapse-module-icon"></i>&nbsp;Tips and Tricks
                        </a>
                    </div>
                    <div class="card-body pb-0 pt-0">
                        <div id="collapseTips" aria-expanded="false" class="collapse show m-3">
                            <div class="mb-2">
                                <a class="collapsed collapse-module-button" data-toggle="collapse" href="#collapseTIP0" aria-expanded="false" aria-controls="collapseTIP0">
                                    <i class="collapse-module-icon"></i>&nbsp;Children and Professionals
                                </a>
                                <div id="collapseTIP0" aria-expanded="false" class="collapse">
                                    <p class="ml-3 mt-3">
                                        Before you enter a new child or professional into the system,
                                        please use the search feature of the tables on the Children dashboard and Professional dashboard
                                        to check for the child or professional you are about to enter.  This will keep you from accidentally
                                        entering duplicate records for a child or professional and improve the quality of the data in the system.
                                    </p>
                                </div>
                                <hr />
                            </div>
                            <div class="mb-2">
                                <a class="collapsed collapse-module-button" data-toggle="collapse" href="#collapseTIP1" aria-expanded="false" aria-controls="collapseTIP1">
                                    <i class="collapse-module-icon"></i>&nbsp;Support Tickets
                                </a>
                                <div id="collapseTIP1" aria-expanded="false" class="collapse">
                                    <p class="ml-3 mt-3">
                                        To ensure that support is able to effectively help you resolve your issue,
                                        please include as much information about your issue as possible in your support ticket.
                                    </p>
                                </div>
                                <hr />
                            </div>
                            <div class="mb-2">
                                <a class="collapsed collapse-module-button" data-toggle="collapse" href="#collapseTIP2" aria-expanded="false" aria-controls="collapseTIP2">
                                    <i class="collapse-module-icon"></i>&nbsp;Navigating Data-Entry Forms
                                </a>
                                <div id="collapseTIP2" aria-expanded="false" class="collapse">
                                    <p class="ml-3 mt-3">
                                        You can use the 'Tab' key to navigate the data-entry forms in the system for
                                        more efficient data-entry and you can use the 'Enter' key to click buttons that you
                                        have focused by using the 'Tab' key.
                                    </p>
                                </div>
                                <hr />
                            </div>
                            <div class="mb-2">
                                <a class="collapsed collapse-module-button" data-toggle="collapse" href="#collapseTIP3" aria-expanded="false" aria-controls="collapseTIP3">
                                    <i class="collapse-module-icon"></i>&nbsp;Report Documentation
                                </a>
                                <div id="collapseTIP3" aria-expanded="false" class="collapse">
                                    <p class="ml-3 mt-3">
                                        All the reports on the reports page have documentation files accessible via the 'Documentation' button.
                                        These documentation files describe how the reports work and can answer many of the questions you may have
                                        about the reports.
                                    </p>
                                </div>
                                <hr />
                            </div>
                            <div class="mb-2">
                                <a class="collapsed collapse-module-button" data-toggle="collapse" href="#collapseTIP4" aria-expanded="false" aria-controls="collapseTIP4">
                                    <i class="collapse-module-icon"></i>&nbsp;Multiple Tabs
                                </a>
                                <div id="collapseTIP4" aria-expanded="false" class="collapse">
                                    <p class="ml-3 mt-3">
                                        You can open many of the links in the system in a new tab by right-clicking on the link
                                        and then clicking the 'Open Link in New Tab' option.
                                    </p>
                                </div>
                                <hr />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
