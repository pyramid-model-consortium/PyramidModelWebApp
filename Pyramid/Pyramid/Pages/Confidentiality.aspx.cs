using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.AspNet.Identity;
using Pyramid.Code;
using Pyramid.Models;

namespace Pyramid.Pages
{
    public partial class Confidentiality : System.Web.UI.Page
    {
        private ProgramAndRoleFromSession currentProgramRole;
        private State currentState;

        protected void Page_Load(object sender, EventArgs e)
        {
            //Get the user's program role from session
            currentProgramRole = Utilities.GetProgramRoleFromSession(Session);

            //Remove the session variable
            Session.Remove(Utilities.SessionKey.CONFIDENTIALITY_ACCEPTED);

            //Check to see if the user has accepted the agreement
            bool isAccepted = Utilities.IsConfidentialityAccepted(User.Identity.Name, currentProgramRole.CurrentStateFK.Value);

            using (PyramidContext context = new PyramidContext())
            {
                //Get the state from the database
                currentState = context.State.AsNoTracking()
                                        .Where(s => s.StatePK == currentProgramRole.CurrentStateFK.Value)
                                        .FirstOrDefault();
            }

            //Get the state abbreviation
            string stateAbbreviation = currentState.StatePK == (int)Utilities.StateFKs.NEW_YORK ? "NYS" : currentState.Abbreviation;

            //Set the username labels
            lblUsernameLabel.Text = string.Format("{0} PIDS Username:", stateAbbreviation);
            lblUsername.Text = User.Identity.Name;

            //Set the date label
            lblCurrentDate.Text = DateTime.Now.ToString("MM/dd/yyyy");

            //Check to see if the state is using the confidentiality
            if (currentState.ConfidentialityEnabled)
            {
                //Don't redirect before retrieving the link below as that method will notify admins if there is an
                //issue with the document
                //Get the confidentiality document link
                string confidentialityDocumentLink = Utilities.GetConfidentialityDocumentLink(currentState, 40);

                //Make sure the link exists
                if(!string.IsNullOrWhiteSpace(confidentialityDocumentLink))
                {
                    //The document exists
                    //Set the url of the document link
                    lnkViewDocument.NavigateUrl = confidentialityDocumentLink;

                    //Set the check box text
                    cbConfirm.Text = string.Format("I certify that I have read and agree to use {0} " +
                        "PIDS according to the terms of the {0} PIDS User Agreement", stateAbbreviation);

                    //Check to see if the confidentiality has a change date
                    if (currentState.ConfidentialityChangeDate.HasValue) {
                        //Set the agreement notification text
                        lblAgreementNotification.Text = string.Format("In order to continue using this system, you must accept the " +
                            "following user agreement.  (Last updated on {0})",
                            currentState.ConfidentialityChangeDate.Value.ToString("MM/dd/yyyy"));
                    }
                    else
                    {
                        //Set the agreement notification text
                        lblAgreementNotification.Text = "In order to continue using this system, you must accept the " +
                            "following user agreement.";
                    }
                }
                else
                {
                    //Hide the confidentiality agreement and notification section
                    divConfidentialityAgreement.Visible = false;

                    //Show the error section
                    divAgreementError.Visible = true;
                    lblAgreementError.Text = "We were unable to retrieve the user agreement, please submit a support ticket " +
                        "using the link below and include this error message in your description!";
                }

                //Check to see if the user has accepted the agreement
                if (isAccepted)
                {
                    //Redirect the user to the default page or return URL
                    Response.Redirect(Request.QueryString["ReturnUrl"] != null ?
                                            Request.QueryString["ReturnUrl"].ToString() :
                                            "/Default.aspx");
                }
            }
            else
            {
                //Redirect the user to the default page or return URL
                Response.Redirect(Request.QueryString["ReturnUrl"] != null ?
                                        Request.QueryString["ReturnUrl"].ToString() :
                                        "/Default.aspx");
            }
        }

        /// <summary>
        /// This method fires when the user toggles the cbConfirm check box
        /// </summary>
        /// <param name="sender">The cbConfirm BootstrapCheckBox</param>
        /// <param name="e">The CheckedChanged event arguments</param>
        protected void cbConfirm_CheckedChanged(object sender, EventArgs e)
        {
            submitConfidentiality.EnableSubmitButton = cbConfirm.Checked;
            submitConfidentiality.UpdateProperties();
        }

        /// <summary>
        /// This method fires when the validation fails for the submit control's validation group
        /// </summary>
        /// <param name="sender">The submitConfidentiality control</param>
        /// <param name="e">The click event arguments</param>
        protected void submitConfidentiality_ValidationFailed(object sender, EventArgs e)
        {
            //Tell the user that validation failed
            msgSys.ShowMessageToUser("warning", "Validation Error(s)", "Validation failed, see above for details.", 22000);
        }

        /// <summary>
        /// This method fires when the user clicks the accept button
        /// </summary>
        /// <param name="sender">The submitConfidentiality control</param>
        /// <param name="e">The click event arguments</param>
        protected void submitConfidentiality_Click(object sender, EventArgs e)
        {
            //Create a new confidentiality agreement object and set its values
            ConfidentialityAgreement agreement = new ConfidentialityAgreement();
            agreement.AgreementDate = DateTime.Now;
            agreement.StateFK = currentProgramRole.CurrentStateFK.Value;
            agreement.Username = User.Identity.Name;

            //Add the agreement object to the database
            using(PyramidContext context = new PyramidContext())
            {
                context.ConfidentialityAgreement.Add(agreement);
                context.SaveChanges();
            }

            //Remove the confidentiality accepted session object so that it
            //will refresh from the database once the user loads a page
            Session.Remove(Utilities.SessionKey.CONFIDENTIALITY_ACCEPTED);

            //Redirect the user to the default page or return URL
            Response.Redirect(Request.QueryString["ReturnUrl"] != null ?
                                    Request.QueryString["ReturnUrl"].ToString() :
                                    "/Default.aspx");
        }

        /// <summary>
        /// This method fires when the user clicks the decline button and it logs
        /// the user out and sends them to the login page
        /// </summary>
        /// <param name="sender">The submitConfidentiality control</param>
        /// <param name="e">The click event arguments</param>
        protected void submitConfidentiality_CancelClick(object sender, EventArgs e)
        {
            //Log the user out
            Context.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

            //Record the logout if a record for the login existed
            if (Session["LoginHistoryPK"] != null && !String.IsNullOrWhiteSpace(Session["LoginHistoryPK"].ToString()))
            {
                //Get the login history pk from session
                int historyPK = Convert.ToInt32(Session["LoginHistoryPK"].ToString());

                //Add the record to the database with the logout time
                using (PyramidContext context = new PyramidContext())
                {
                    LoginHistory history = context.LoginHistory.Find(historyPK);
                    history.LogoutTime = DateTime.Now;
                    history.LogoutType = "User declined the user agreement.";
                    context.SaveChanges();
                }
            }

            //Ensure that the user's session is clear
            Session.Abandon();

            //Redirect the user to the login page
            Response.Redirect("/Account/Login.aspx?messageType=DeclinedConfidentiality");
        }
    }
}