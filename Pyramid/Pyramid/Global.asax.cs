using DevExpress.XtraReports.Web.ReportDesigner;
using System;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.UI;

namespace Pyramid
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //DevExpress web report designer
            DevExpress.XtraReports.Web.Extensions.ReportStorageWebExtension.RegisterExtensionGlobal(new Reports.UserReports.PyramidReportStorage());
            DefaultReportDesignerContainer.RegisterDataSourceWizardConnectionStringsProvider<Reports.UserReports.PyramidReportConnectionStringProvider>();

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
    }
}