using System;
using System.Data;
using Pyramid.Models;
using System.Linq;
using System.Data.Entity;
using DevExpress.DataProcessing;

namespace Pyramid.Reports.PreBuiltReports.FormReports
{
    public partial class RptCWLTAgencyType : Pyramid.Reports.PreBuiltReports.MasterReports.RptFormReportMaster
    {
        public RptCWLTAgencyType()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This method executes before the report prints and it fills out the report information using LINQ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RptCWLTAgencyType_BeforePrint(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //To hold the necessary objects
            Models.CWLTAgencyType currentCWLTAgencyType;

            //Get the PK
            int formPK = Convert.ToInt32(Parameters["ParamFormPK"].Value);

            using (PyramidContext context = new PyramidContext())
            {
                //Get the CWLT Agency Type object
                currentCWLTAgencyType = context.CWLTAgencyType
                                            .Include(cat => cat.State)
                                            .AsNoTracking()
                                            .Where(cat => cat.CWLTAgencyTypePK == formPK)
                                            .FirstOrDefault();
            }

            //------ Basic Information ------
            //Set the text for the labels
            lblAgencyTypeState.Text = currentCWLTAgencyType.State.Name;
            lblAgencyTypeName.Text = currentCWLTAgencyType.Name;
            lblAgencyTypeDescription.Text = currentCWLTAgencyType.Description;
        }
    }
}
