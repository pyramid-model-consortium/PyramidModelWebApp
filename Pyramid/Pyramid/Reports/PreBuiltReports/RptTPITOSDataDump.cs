﻿using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;

namespace Pyramid.Reports.PreBuiltReports
{
    public partial class RptTPITOSDataDump : Pyramid.Reports.PreBuiltReports.MasterReports.RptDataDumpMaster
    {
        public RptTPITOSDataDump()
        {
            InitializeComponent();
        }
        public override void PrintingSystem_XlSheetCreated(object sender, DevExpress.XtraPrinting.XlSheetCreatedEventArgs e)
        {
            //To hold the Excel worksheet name
            string sheetName;

            //Set the worksheet name based on the index
            switch (e.Index)
            {
                case 0:
                    sheetName = "TPITOS";
                    break;
                case 1:
                    sheetName = "Participants";
                    break;
                case 2:
                    sheetName = "Red Flags";
                    break;
                case 3:
                    sheetName = "Criteria";
                    break;
                default:
                    sheetName = "";
                    break;
            }

            //If the worksheet name has value, set the Excel worksheet name
            if (!string.IsNullOrWhiteSpace(sheetName))
            {
                e.SheetName = sheetName;
            }
        }

    }
}
