using DevExpress.XtraReports.Web.ReportDesigner;
using Elmah;
using System;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.UI;
using System.Web.Http;
using DevExpress.XtraReports.Web.WebDocumentViewer;

namespace Pyramid
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            GlobalConfiguration.Configure(WebApiConfig.Register);

            //DevExpress web report designer
            DevExpress.XtraReports.Web.Extensions.ReportStorageWebExtension.RegisterExtensionGlobal(new Reports.UserReports.PyramidReportStorage());
            DefaultReportDesignerContainer.RegisterDataSourceWizardConnectionStringsProvider<Reports.UserReports.PyramidReportConnectionStringProvider>();

            //Register a custom exception handler for ALL DevEx web document viewer server-side exceptions.
            //This allows us to send errors to Elmah and diagnose issues where the users cannot see reports.
            DefaultWebDocumentViewerContainer.Register<IWebDocumentViewerExceptionHandler, Code.DevExpressHelpers.CustomWebDocumentViewerExceptionHandler>();

            //Initialize the WebDocumentViewer as suggested here:
            //https://docs.devexpress.com/XtraReports/DevExpress.XtraReports.Web.ASPxWebDocumentViewer.StaticInitialize?utm_source=SupportCenter&utm_medium=website&utm_campaign=docs-feedback&utm_content=T1060395
            DevExpress.XtraReports.Web.ASPxWebDocumentViewer.StaticInitialize();

            /*
             * DO NOT CHANGE THE BELOW MAPPING NAMES!!!
             */

            //Register the bundles with the ScriptManager
            ScriptManager.ScriptResourceMapping.AddDefinition("jquery",
                new ScriptResourceDefinition
                {
                    Path = "~/bundles/JQuery"
                }
            );

            ScriptManager.ScriptResourceMapping.AddDefinition("MsAjaxBundle",
                new ScriptResourceDefinition
                {
                    Path = "~/bundles/MsAjaxJs"
                }
            );

            ScriptManager.ScriptResourceMapping.AddDefinition("bootstrap",
                new ScriptResourceDefinition
                {
                    Path = "~/bundles/Bootstrap"
                }
            );

            ScriptManager.ScriptResourceMapping.AddDefinition("WebFormsBundle",
                new ScriptResourceDefinition
                {
                    Path = "~/bundles/WebFormsJs"
                }
            );

            ScriptManager.ScriptResourceMapping.AddDefinition("DataTables",
                new ScriptResourceDefinition
                {
                    Path = "~/bundles/DataTables"
                }
            );

            ScriptManager.ScriptResourceMapping.AddDefinition("Inputmask",
                new ScriptResourceDefinition
                {
                    Path = "~/bundles/Inputmask"
                }
            );

            ScriptManager.ScriptResourceMapping.AddDefinition("DevExpress",
                new ScriptResourceDefinition
                {
                    Path = "~/bundles/DevExpress"
                }
            );

            ScriptManager.ScriptResourceMapping.AddDefinition("DevExpressAdditional",
                new ScriptResourceDefinition
                {
                    Path = "~/bundles/DevExpressAdditional"
                }
            );

            ScriptManager.ScriptResourceMapping.AddDefinition("SiteSpecific",
                new ScriptResourceDefinition
                {
                    Path = "~/bundles/SiteSpecific"
                }
            );
        }

        /// <summary>
        /// This method fires when the ELMAH error log filters, and it ensures
        /// that HttpRequestValidationExceptions are logged (they are ignored
        /// by default)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ErrorLog_Filtering(object sender, ExceptionFilterEventArgs e)
        {
            //Check to see if the exception is a HttpRequestValidationException
            if (e.Exception.GetBaseException() is HttpRequestValidationException)
            {
                //Log the exception with the old method (bypasses filters)
                ErrorLog.GetDefault(HttpContext.Current).Log(new Elmah.Error(e.Exception));
            }
        }
    }
}