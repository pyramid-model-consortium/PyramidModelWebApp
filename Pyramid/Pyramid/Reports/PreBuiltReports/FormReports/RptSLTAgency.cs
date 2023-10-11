using System;
using System.Data;
using Pyramid.Models;
using System.Linq;
using System.Data.Entity;
using DevExpress.DataProcessing;

namespace Pyramid.Reports.PreBuiltReports.FormReports
{
    public partial class RptSLTAgency : Pyramid.Reports.PreBuiltReports.MasterReports.RptFormReportMaster
    {
        public RptSLTAgency()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This method executes before the report prints and it fills out the report information using LINQ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RptSLTAgency_BeforePrint(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //To hold the necessary objects
            Models.SLTAgency currentSLTAgency;

            //Get the PK
            int formPK = Convert.ToInt32(Parameters["ParamFormPK"].Value);

            using (PyramidContext context = new PyramidContext())
            {
                //Get the SLT Agency object
                currentSLTAgency = context.SLTAgency
                                            .Include(sa => sa.State)
                                            .AsNoTracking()
                                            .Where(sa => sa.SLTAgencyPK == formPK)
                                            .FirstOrDefault();
            }

            //------ Basic Information ------
            //Set the text for the labels
            lblSLTAgencyState.Text = currentSLTAgency.State.Name;
            lblSLTAgencyName.Text = currentSLTAgency.Name;
            lblPhoneNumber.Text = (string.IsNullOrWhiteSpace(currentSLTAgency.PhoneNumber) ? "" : Code.Utilities.FormatPhoneNumber(currentSLTAgency.PhoneNumber, "US"));
            lblWebsite.Text = currentSLTAgency.Website;
            lblAddressStreet.Text = currentSLTAgency.AddressStreet;
            lblAddressCity.Text = currentSLTAgency.AddressCity;
            lblAddressState.Text = currentSLTAgency.AddressState;
            lblAddressZIP.Text = currentSLTAgency.AddressZIPCode;
        }
    }
}
