<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Submit.ascx.cs" Inherits="Pyramid.User_Controls.Submit" %>
<%@ Register Assembly="DevExpress.Web.Bootstrap.v22.2, Version=22.2.4.0, Culture=neutral, PublicKeyToken=b88d1754d700e49a" Namespace="DevExpress.Web.Bootstrap" TagPrefix="dx" %>

<!--
The JavaScript for the submit controls is on the Site.Master page.
-->

<asp:Panel ID="pnlSubmitControl" runat="server" DefaultButton="btnSubmit" CssClass="submit-control">
    <asp:HiddenField ID="hfValidationGroup" runat="server" Value="" />
    <asp:HiddenField ID="hfUseCancelConfirmation" runat="server" Value="" />
    <asp:HiddenField ID="hfUseSubmitConfirmation" runat="server" Value="" />
    <dx:BootstrapButton ID="btnCancel" runat="server" Text="Cancel" OnClick="btnCancel_Click" SettingsBootstrap-RenderOption="secondary" CausesValidation="false">
        <CssClasses Icon="fas fa-times" Control="mr-2 btn-cancel" />
    </dx:BootstrapButton>
    <dx:BootstrapButton ID="btnSubmit" runat="server" Text="Save" OnClick="btnSubmit_Click" SettingsBootstrap-RenderOption="success">
        <CssClasses Icon="fas fa-save" Control="button-submit" />
    </dx:BootstrapButton>
    <asp:LinkButton ID="lbSubmitting" runat="server" CssClass="btn btn-success" style="display: none">
    </asp:LinkButton>
    <div class="modal cancel-confirm-modal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Confirmation</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    <div class="mb-4">
                        Are you sure you want to cancel?  Information that has not been saved will be lost.
                    </div>
                    <div class="collapse-module">
                        <a class="collapsed collapse-module-button small" data-toggle="collapse" href="#collapseDisableCancelConfirmations" aria-expanded="false" aria-controls="collapseDisableCancelConfirmations">
                            <i class="collapse-module-icon"></i>&nbsp;Tired of seeing these confirmations?
                        </a>
                        <div id="collapseDisableCancelConfirmations" aria-expanded="false" class="collapse">
                            <p class="ml-3 mt-3">
                                Cancel confirmations can be disabled by following these steps:
                            </p>
                            <ol>
                                <li>Open a new tab by clicking here: <a target="_blank" href="/Default.aspx"><i class="fas fa-link"></i>&nbsp;New Tab</a></li>
                                <li>In the new tab, click on your username in the navigation bar and then click the 'Manage Account' link.</li>
                                <li>Scroll down to the 'Customization Options' section.</li>
                                <li>In the 'Cancel Confirmations' drop-down list, select the 'Disabled' option.</li>
                                <li>Click the 'Save Customization Options' button at the bottom of the section.</li>
                                <li>You're done!  The next time you click a 'Cancel' button, you will not be prompted to confirm.  Note: You may have to navigate to a new page before the change is applied.</li>
                            </ol>
                        </div>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No, return</button>
                    <dx:BootstrapButton ID="btnConfirmCancel" runat="server" Text="Yes, cancel" OnClick="btnConfirmCancel_Click" SettingsBootstrap-RenderOption="primary" CausesValidation="false">
                        <CssClasses Icon="fas fa-check" />
                    </dx:BootstrapButton>
                </div>
            </div>
        </div>
    </div>
    <div class="modal submit-confirm-modal">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Confirmation</h4>
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                </div>
                <div class="modal-body">
                    <asp:Label ID="lblSubmitConfirmText" runat="server"></asp:Label>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal"><i class="fas fa-times"></i>&nbsp;No, return</button>
                    <dx:BootstrapButton ID="btnConfirmSubmit" runat="server" Text="Yes" OnClick="btnConfirmSubmit_Click" SettingsBootstrap-RenderOption="primary" CausesValidation="false">
                        <CssClasses Icon="fas fa-check" />
                    </dx:BootstrapButton>
                </div>
            </div>
        </div>
    </div>
</asp:Panel>