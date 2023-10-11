using System;
using System.Data;
using Pyramid.Models;
using System.Linq;
using System.Data.Entity;
using DevExpress.DataProcessing;

namespace Pyramid.Reports.PreBuiltReports.FormReports
{
    public partial class RptCWLTAgency : Pyramid.Reports.PreBuiltReports.MasterReports.RptFormReportMaster
    {
        public RptCWLTAgency()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This method executes before the report prints and it fills out the report information using LINQ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RptCWLTAgency_BeforePrint(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //To hold the necessary objects
            Models.CWLTAgency currentCWLTAgency;

            //Get the PK
            int formPK = Convert.ToInt32(Parameters["ParamFormPK"].Value);

            using (PyramidContext context = new PyramidContext())
            {
                //Get the CWLT Agency object
                currentCWLTAgency = context.CWLTAgency
                                            .Include(ca => ca.Hub)
                                            .Include(ca => ca.CWLTAgencyType)
                                            .AsNoTracking()
                                            .Where(ca => ca.CWLTAgencyPK == formPK)
                                            .FirstOrDefault();
            }

            //------ Basic Information ------
            //Set the text for the labels
            lblCWLTAgencyHub.Text = currentCWLTAgency.Hub.Name;
            lblCWLTAgencyName.Text = currentCWLTAgency.Name;
            lblCWLTAgencyType.Text = currentCWLTAgency.CWLTAgencyType.Name;
            lblPhoneNumber.Text = (string.IsNullOrWhiteSpace(currentCWLTAgency.PhoneNumber) ? "" : Code.Utilities.FormatPhoneNumber(currentCWLTAgency.PhoneNumber, "US"));
            lblWebsite.Text = currentCWLTAgency.Website;
            lblAddressStreet.Text = currentCWLTAgency.AddressStreet;
            lblAddressCity.Text = currentCWLTAgency.AddressCity;
            lblAddressState.Text = currentCWLTAgency.AddressState;
            lblAddressZIP.Text = currentCWLTAgency.AddressZIPCode;
        }
    }
}
