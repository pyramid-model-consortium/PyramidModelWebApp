using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;

namespace Pyramid.Reports.PreBuiltReports
{
    public partial class RptBOQFCCChange : Pyramid.Reports.PreBuiltReports.MasterReports.RptLogoMaster
    {
        public RptBOQFCCChange()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This method executes before each indicator label is printed and it changes
        /// the font color if the indicator changed since the last BOQ
        /// </summary>
        /// <param name="sender">An XRLabel</param>
        /// <param name="e">The PrintEventArgs</param>
        private void IndicatorLabel_BeforePrint(object sender, CancelEventArgs e)
        {
            //Get the label
            XRLabel label = (XRLabel)sender;

            //Get the current and previous program FK
            string currentProgramFK = (GetCurrentColumnValue("ProgramFK") == null ? null : GetCurrentColumnValue("ProgramFK").ToString());
            string previousProgramFK = (GetPreviousColumnValue("ProgramFK") == null ? null : GetPreviousColumnValue("ProgramFK").ToString());

            //Only check the values if the program FKs match
            if (currentProgramFK != null && previousProgramFK != null
                    && currentProgramFK == previousProgramFK)
            {

                //To hold the field name
                string fieldName = "";

                //Loop through the label's expression bindings
                foreach (ExpressionBinding binding in label.ExpressionBindings)
                {
                    //Find the text expression binding
                    if (binding.PropertyName == "Text")
                    {
                        //Get the expression
                        fieldName = binding.Expression;

                        //Get the field name from the expression by trimming the starting and ending characters
                        fieldName = fieldName.Substring(1, fieldName.Length - 2);
                    }
                }

                //Get the current value and previous value
                string currentValue = (GetCurrentColumnValue(fieldName) == null ? null : GetCurrentColumnValue(fieldName).ToString().ToLower());
                string previousValue = (GetPreviousColumnValue(fieldName) == null ? null : GetPreviousColumnValue(fieldName).ToString().ToLower());

                //Make sure the current and previous value exist
                if (previousValue != null && currentValue != null)
                {
                    //Get the integer versions of the current and previous value
                    int currentValueInt = (currentValue == "na" ? -1 :
                                                currentValue == "not in place" ? 0 :
                                                currentValue == "partially in place" ? 1 :
                                                currentValue == "in place" ? 2 : 0);
                    int previousValueInt = (previousValue == "na" ? -1 : 
                                                previousValue == "not in place" ? 0 :
                                                previousValue == "partially in place" ? 1 :
                                                previousValue == "in place" ? 2 : 0);

                    //Set the color of the text if the indicator changed value
                    if (currentValueInt > previousValueInt)
                    {
                        label.ForeColor = Color.Green;
                    }
                    else if (currentValueInt < previousValueInt)
                    {
                        label.ForeColor = Color.Red;
                    }
                    else
                    {
                        label.ForeColor = Color.Black;
                    }
                }
            }
            else
            {
                //Reset to black font
                label.ForeColor = Color.Black;
            }
        }
    }
}
