using DevExpress.XtraReports.UI;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;

namespace Pyramid.Reports.PreBuiltReports
{
    public partial class RptBOQCWLTChange : Pyramid.Reports.PreBuiltReports.MasterReports.RptLogoMaster
    {
        public RptBOQCWLTChange()
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

            //Get the current and previous hub FK
            string currentHubFK = (GetCurrentColumnValue("HubFK") == null ? null : GetCurrentColumnValue("HubFK").ToString());
            string previousHubFK = (GetPreviousColumnValue("HubFK") == null ? null : GetPreviousColumnValue("HubFK").ToString());

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

            //Make sure the current value is an int
            if (int.TryParse(currentValue, out int intCurrentValue))
            {
                //To hold the label text
                string labelText;

                //Get the text
                switch (intCurrentValue)
                {
                    case 0:
                        labelText = "Not In Place";
                        break;
                    case 1:
                        labelText = "Needs Improvement";
                        break;
                    case 2:
                        labelText = "In Place";
                        break;
                    default:
                        labelText = "ERROR!";
                        break;
                }

                //Set the label text
                label.Text = labelText;

                //Make sure the current and previous value exist
                if (int.TryParse(previousValue, out int intPreviousValue))
                {
                    //Only check the values if the program FKs match
                    if (currentHubFK != null && previousHubFK != null
                        && currentHubFK == previousHubFK)
                    {
                        //Set the color of the text if the indicator changed value
                        if (intCurrentValue > intPreviousValue)
                        {
                            label.ForeColor = Color.Green;
                        }
                        else if (intCurrentValue < intPreviousValue)
                        {
                            label.ForeColor = Color.Red;
                        }
                        else
                        {
                            label.ForeColor = Color.Black;
                        }
                    }
                    else
                    {
                        //Reset to black font
                        label.ForeColor = Color.Black;
                    }
                }
                else
                {
                    //Reset to black font
                    label.ForeColor = Color.Black;
                }
            }
            else
            {
                //Error values
                label.ForeColor = Color.Red;
                label.Text = "ERROR!";
            }
        }
    }
}
