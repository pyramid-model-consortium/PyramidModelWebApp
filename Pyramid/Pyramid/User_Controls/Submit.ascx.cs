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
        public string SubmitButtonBootstrapType { get; set; }
        public string SubmitButtonIcon { get; set; }
        public string SubmitButtonText { get; set; }
        public string SubmittingButtonText { get; set; }
        public bool? ShowSubmitButton { get; set; }
        public bool? ShowCancelButton { get; set; }

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
                //Call the submit method
                if (this.SubmitClick != null)
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
        /// This is the standard click event for the cancel button and
        /// it will call the method that was assigned to its event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void lbCancel_Click(object sender, EventArgs e)
        {
            //Call the proper method
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

            if (!string.IsNullOrWhiteSpace(SubmitButtonText))
                btnSubmit.Text = SubmitButtonText;

            if (!string.IsNullOrWhiteSpace(ControlCssClass))
                pnlSubmitControl.CssClass = "submit-control " + ControlCssClass;

            //Set the submit button render option
            BootstrapRenderOption renderOption;
            string submittingButtonClass;
            if (!string.IsNullOrWhiteSpace(SubmitButtonBootstrapType))
            {
                switch (SubmitButtonBootstrapType.ToLower())
                {
                    case "danger":
                        renderOption = BootstrapRenderOption.Danger;
                        submittingButtonClass = "btn btn-danger";
                        break;
                    case "dark":
                        renderOption = BootstrapRenderOption.Dark;
                        submittingButtonClass = "btn btn-dark";
                        break;
                    case "default":
                        renderOption = BootstrapRenderOption.Default;
                        submittingButtonClass = "btn btn-default";
                        break;
                    case "info":
                        renderOption = BootstrapRenderOption.Info;
                        submittingButtonClass = "btn btn-info";
                        break;
                    case "light":
                        renderOption = BootstrapRenderOption.Light;
                        submittingButtonClass = "btn btn-light";
                        break;
                    case "link":
                        renderOption = BootstrapRenderOption.Link;
                        submittingButtonClass = "btn btn-link";
                        break;
                    case "primary":
                        renderOption = BootstrapRenderOption.Primary;
                        submittingButtonClass = "btn btn-primary";
                        break;
                    case "secondary":
                        renderOption = BootstrapRenderOption.Secondary;
                        submittingButtonClass = "btn btn-secondary";
                        break;
                    case "warning":
                        renderOption = BootstrapRenderOption.Warning;
                        submittingButtonClass = "btn btn-warning";
                        break;
                    default:
                        renderOption = BootstrapRenderOption.Success;
                        submittingButtonClass = "btn btn-success";
                        break;
                }
            }
            else
            {
                renderOption = BootstrapRenderOption.Success;
                submittingButtonClass = "btn btn-success";
            }
            btnSubmit.SettingsBootstrap.RenderOption = renderOption;
            lbSubmitting.CssClass = submittingButtonClass;

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
            lbCancel.Visible = (ShowCancelButton.HasValue ? ShowCancelButton.Value : true);
        }
    }
}