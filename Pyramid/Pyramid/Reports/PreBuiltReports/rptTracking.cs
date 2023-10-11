using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
using Pyramid.Models;
using System.Linq;
using System.Data.Entity;
using System.Collections.Generic;

namespace Pyramid.Reports.PreBuiltReports
{
    public partial class RptTracking : Pyramid.Reports.PreBuiltReports.MasterReports.RptLogoMaster
    {
        public RptTracking()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This method fills the legend with the abbreviations and definitions for the social emotional screens
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lblLegend_BeforePrint(object sender, System.ComponentModel.CancelEventArgs e)
        {
            using (PyramidContext context = new PyramidContext())
            {
                //Get the se screen abbreviations
                List<string> abbreviations = context.CodeScreenType.AsNoTracking()
                                                        .Where(cst => cst.Abbreviation != cst.Description)
                                                        .OrderBy(cst => cst.OrderBy)
                                                        .Select(cst => cst.Abbreviation + " = " + cst.Description).ToList();

                //Add the ASQ:SE
                abbreviations.Insert(0, "ASQ:SE = Ages and Stages Questionnaires Social Emotional");

                //Set the label text
                lblLegend.Text = string.Join(" | ", abbreviations);
            }
        }
    }
}
