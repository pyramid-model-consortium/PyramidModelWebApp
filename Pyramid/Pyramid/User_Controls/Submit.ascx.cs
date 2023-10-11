using DevExpress.Web;
using System;
using DevExpress.Web.Bootstrap;

namespace Pyramid.User_Controls
{
    public partial class Submit : System.Web.UI.UserControl
    {
        public event EventHandler SubmitClick;
        public event EventHandler CancelClick;
        public event EventHandler ValidationFailed;
        public string ValidationGroup { get; set; }
        public string ControlCssClass { get; set; }
        public string SubmittingButtonText { get; set; }
        public string SubmitButtonBootstrapType { get; set; }
        public string SubmitButtonIcon { get; set; }
        public string SubmitButtonText { get; set; }
        public bool? UseSubmitConfirm { get; set; }
        public string SubmitConfirmModalText { get; set; }
        public string SubmitConfirmButtonText { get; set; }
        public bool? ShowSubmitButton { get; set; }
        public bool? EnableSubmitButton { get; set; }
        public string CancelButtonBootstrapType { get; set; }
        public string CancelButtonIcon { get; set; }
        public string CancelButtonText { get; set; }
        public bool? UseCancelConfirm { get; set; }
        public bool? ShowCancelButton { get; set; }
        public bool? EnableCancelButton { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //Update the properties
                UpdateProperties();
            }
        }

        /// <summary>
        /// This is the standard click event for the submit button and
        /// it ensures that the page is valid before calling the method
        /// that was assigned to its event handler
        /// </summary>
        /// <param name="sender">The btnSubmit DevExpress button</param>
        /// <param name="e">The Click event</param>
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            //Only call the assigned event if the validation group is valid
            if (ASPxEdit.AreEditorsValid(this.Page, ValidationGroup))
            {
                //Call the submit method if we aren't using confirmations
                if (this.SubmitClick != null && (UseSubmitConfirm.HasValue == false || UseSubmitConfirm.Value == false))
                {
                    this.SubmitClick(sender, e);
                }
            }
            else
            {
                //Call the validation failed method
                if(this.ValidationFailed != null)
                {
                    this.ValidationFailed(sender, e);
                }
            }
        }

        /// <summary>
        /// This is the click event for the submit confirmation button and
        /// it will call the method that was assigned to submit click event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnConfirmSubmit_Click(object sender, EventArgs e)
        {
            //Call the submit click method
            if (this.SubmitClick != null)
            {
                this.SubmitClick(sender, e);
            }
        }

        /// <summary>
        /// This is the standard click event for the cancel button and
        /// it will call the method that was assigned to its event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnCancel_Click(object sender, EventArgs e)
        {
            //Only call the cancel click if we aren't using confirmation
            if (this.CancelClick != null && (UseCancelConfirm.HasValue == false || UseCancelConfirm.Value == false))
            {
                this.CancelClick(sender, e);
            }
        }

        /// <summary>
        /// This is the click event for the cancel confirmation button and
        /// it will call the method that was assigned to cancel click event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnConfirmCancel_Click(object sender, EventArgs e)
        {
            //Call the cancel click method
            if (this.CancelClick != null)
            {
                this.CancelClick(sender, e);
            }
        }

        /// <summary>
        /// Update the controls properties
        /// </summary>
        public void UpdateProperties()
        {
            //Set the submit button properties if provided
            if (!string.IsNullOrWhiteSpace(SubmitButtonIcon))
                btnSubmit.CssClasses.Icon = SubmitButtonIcon;

            //Set the submit button text
            if (!string.IsNullOrWhiteSpace(SubmitButtonText))
                btnSubmit.Text = SubmitButtonText;

            //Set the cancel button icon
            if (!string.IsNullOrWhiteSpace(CancelButtonIcon))
                btnCancel.CssClasses.Icon = CancelButtonIcon;

            //Set the cancel button text
            if (!string.IsNullOrWhiteSpace(CancelButtonText))
                btnCancel.Text = CancelButtonText;

            //Set the control div css class if it exists
            if (!string.IsNullOrWhiteSpace(ControlCssClass))
                pnlSubmitControl.CssClass = "submit-control " + ControlCssClass;

            //Set the submit button render option
            BootstrapRenderOption submitButtonRenderOption;
            string submittingButtonClass;
            if (!string.IsNullOrWhiteSpace(SubmitButtonBootstrapType))
            {
                switch (SubmitButtonBootstrapType.ToLower())
                {
                    case "danger":
                        submitButtonRenderOption = BootstrapRenderOption.Danger;
                        submittingButtonClass = "btn btn-danger";
                        break;
                    case "dark":
                        submitButtonRenderOption = BootstrapRenderOption.Dark;
                        submittingButtonClass = "btn btn-dark";
                        break;
                    case "default":
                        submitButtonRenderOption = BootstrapRenderOption.Default;
                        submittingButtonClass = "btn btn-default";
                        break;
                    case "info":
                        submitButtonRenderOption = BootstrapRenderOption.Info;
                        submittingButtonClass = "btn btn-info";
                        break;
                    case "light":
                        submitButtonRenderOption = BootstrapRenderOption.Light;
                        submittingButtonClass = "btn btn-light";
                        break;
                    case "link":
                        submitButtonRenderOption = BootstrapRenderOption.Link;
                        submittingButtonClass = "btn btn-link";
                        break;
                    case "primary":
                        submitButtonRenderOption = BootstrapRenderOption.Primary;
                        submittingButtonClass = "btn btn-primary";
                        break;
                    case "secondary":
                        submitButtonRenderOption = BootstrapRenderOption.Secondary;
                        submittingButtonClass = "btn btn-secondary";
                        break;
                    case "warning":
                        submitButtonRenderOption = BootstrapRenderOption.Warning;
                        submittingButtonClass = "btn btn-warning";
                        break;
                    default:
                        submitButtonRenderOption = BootstrapRenderOption.Success;
                        submittingButtonClass = "btn btn-success";
                        break;
                }
            }
            else
            {
                submitButtonRenderOption = BootstrapRenderOption.Success;
                submittingButtonClass = "btn btn-success";
            }
            btnSubmit.SettingsBootstrap.RenderOption = submitButtonRenderOption;
            lbSubmitting.CssClass = submittingButtonClass;

            //Set the cancel button render option
            BootstrapRenderOption cancelButtonRenderOption;
            if (!string.IsNullOrWhiteSpace(CancelButtonBootstrapType))
            {
                switch (CancelButtonBootstrapType.ToLower())
                {
                    case "danger":
                        cancelButtonRenderOption = BootstrapRenderOption.Danger;
                        break;
                    case "dark":
                        cancelButtonRenderOption = BootstrapRenderOption.Dark;
                        break;
                    case "default":
                        cancelButtonRenderOption = BootstrapRenderOption.Default;
                        break;
                    case "info":
                        cancelButtonRenderOption = BootstrapRenderOption.Info;
                        break;
                    case "light":
                        cancelButtonRenderOption = BootstrapRenderOption.Light;
                        break;
                    case "link":
                        cancelButtonRenderOption = BootstrapRenderOption.Link;
                        break;
                    case "primary":
                        cancelButtonRenderOption = BootstrapRenderOption.Primary;
                        break;
                    case "secondary":
                        cancelButtonRenderOption = BootstrapRenderOption.Secondary;
                        break;
                    case "warning":
                        cancelButtonRenderOption = BootstrapRenderOption.Warning;
                        break;
                    default:
                        cancelButtonRenderOption = BootstrapRenderOption.Success;
                        break;
                }
            }
            else
            {
                cancelButtonRenderOption = BootstrapRenderOption.Secondary;
            }
            btnCancel.SettingsBootstrap.RenderOption = cancelButtonRenderOption;

            //Set the submit button validation group
            btnSubmit.ValidationGroup = ValidationGroup;

            //Set the submitting button text
            if (!string.IsNullOrWhiteSpace(SubmittingButtonText))
                lbSubmitting.Text = "<span class='spinner-border spinner-border-sm'></span>&nbsp;" + SubmittingButtonText;
            else
                lbSubmitting.Text = "<span class='spinner-border spinner-border-sm'></span>&nbsp;" + "Saving...";

            //Let the front end see the validation group
            hfValidationGroup.Value = ValidationGroup;

            //Show or hide the buttons
            btnSubmit.Visible = (ShowSubmitButton.HasValue ? ShowSubmitButton.Value : true);
            btnCancel.Visible = (ShowCancelButton.HasValue ? ShowCancelButton.Value : true);

            //Enable or disable the buttons
            btnSubmit.Enabled = (EnableSubmitButton.HasValue ? EnableSubmitButton.Value : true);
            btnCancel.Enabled = (EnableCancelButton.HasValue ? EnableCancelButton.Value : true);

            //Handle the submit confirmation property
            if (UseSubmitConfirm.HasValue && UseSubmitConfirm.Value == true)
            {
                hfUseSubmitConfirmation.Value = "True";
            }
            else
            {
                hfUseSubmitConfirmation.Value = "False";
            }

            //Set the submit confirmation modal text
            if (!string.IsNullOrWhiteSpace(SubmitConfirmModalText))
                lblSubmitConfirmText.Text = SubmitConfirmModalText;
            else
                lblSubmitConfirmText.Text = "Are you sure that you want to save this information?";

            //Set the submit confirmation button text
            if (!string.IsNullOrWhiteSpace(SubmitConfirmButtonText))
                btnConfirmSubmit.Text = SubmitConfirmButtonText;
            else
                btnConfirmSubmit.Text = "Yes, Save";

            //Handle the cancel confirmation property
            if (UseCancelConfirm.HasValue && UseCancelConfirm.Value == true)
            {
                hfUseCancelConfirmation.Value = "True";
            }
            else
            {
                hfUseCancelConfirmation.Value = "False";
            }
        }
    }
}