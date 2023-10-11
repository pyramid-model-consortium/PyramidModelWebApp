using System;
using System.Data;
using Pyramid.Models;
using System.Linq;
using System.Data.Entity;
using DevExpress.DataProcessing;

namespace Pyramid.Reports.PreBuiltReports.FormReports
{
    public partial class RptSLTWorkGroup : Pyramid.Reports.PreBuiltReports.MasterReports.RptFormReportMaster
    {
        public RptSLTWorkGroup()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This method executes before the report prints and it fills out the report information using LINQ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RptSLTWorkGroup_BeforePrint(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //To hold the necessary objects
            Models.SLTWorkGroup currentSLTWorkGroup;

            //Get the PK
            int formPK = Convert.ToInt32(Parameters["ParamFormPK"].Value);

            using (PyramidContext context = new PyramidContext())
            {
                //Get the SLT Work Group object
                currentSLTWorkGroup = context.SLTWorkGroup
                                            .Include(sa => sa.State)
                                            .AsNoTracking()
                                            .Where(sa => sa.SLTWorkGroupPK == formPK)
                                            .FirstOrDefault();
            }

            //------ Basic Information ------
            //Set the text for the labels
            lblSLTWorkGroupState.Text = currentSLTWorkGroup.State.Name;
            lblSLTWorkGroupName.Text = currentSLTWorkGroup.WorkGroupName;
        }
    }
}
