﻿using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;

namespace Pyramid.Reports.PreBuiltReports.FormReports
{
    public partial class RptASQSE : Pyramid.Reports.PreBuiltReports.MasterReports.RptFormReportMaster
    {
        public RptASQSE()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This method fires before the lblChildAgeMonths label is shown and it fills
        /// the label with the child's age in months
        /// </summary>
        /// <param name="sender">The lblChildAgeMonths XRLabel</param>
        /// <param name="e"></param>
        private void lblChildAgeMonths_BeforePrint(object sender, CancelEventArgs e)
        {
            //Get the label
            XRLabel label = (XRLabel)sender;

            //Get the birth date and ASQ:SE date
            DateTime birthDate = Convert.ToDateTime(GetCurrentColumnValue("BirthDate"));
            DateTime ASQSEDate = Convert.ToDateTime(GetCurrentColumnValue("ASQSEDate"));

            //Get the age of the child in months
            int ageDays = Code.Utilities.CalculateAgeDays(ASQSEDate, birthDate);
            double ageMonths = (ageDays / 30.417);

            //Set the label text
            label.Text = ageMonths.ToString("0.##") + " months old";
        }
    }
}
