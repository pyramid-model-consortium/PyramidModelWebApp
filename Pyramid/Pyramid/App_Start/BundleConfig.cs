using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using System.Web.UI;

namespace Pyramid
{
    public class BundleConfig
    {
        // For more information on Bundling, visit https://go.microsoft.com/fwlink/?LinkID=303951
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/WebFormsJs").Include(
                            "~/Scripts/WebForms/WebForms.js",
                            "~/Scripts/WebForms/WebUIValidation.js",
                            "~/Scripts/WebForms/MenuStandards.js",
                            "~/Scripts/WebForms/Focus.js",
                            "~/Scripts/WebForms/GridView.js",
                            "~/Scripts/WebForms/DetailsView.js",
                            "~/Scripts/WebForms/TreeView.js",
                            "~/Scripts/WebForms/WebParts.js"));

            // Order is very important for these files to work, they have explicit dependencies
            bundles.Add(new ScriptBundle("~/bundles/MsAjaxJs").Include(
                    "~/Scripts/WebForms/MsAjax/MicrosoftAjax.js",
                    "~/Scripts/WebForms/MsAjax/MicrosoftAjaxApplicationServices.js",
                    "~/Scripts/WebForms/MsAjax/MicrosoftAjaxTimer.js",
                    "~/Scripts/WebForms/MsAjax/MicrosoftAjaxWebForms.js"));

            //Make sure that JQuery is the first bundle loaded on an implementing page
            bundles.Add(new ScriptBundle("~/bundles/JQuery").Include("~/Scripts/jquery-3.4.1.js"));

            //Popper is necessary for bootstrap
            bundles.Add(new ScriptBundle("~/bundles/Bootstrap").Include("~/Scripts/umd/popper.js", 
                "~/Scripts/umd/popper-utils.js", 
                "~/Scripts/bootstrap.js", 
                "~/Scripts/bootstrap-notify.js",
                "~/Scripts/bootstrap-datepicker.js",
                "~/Scripts/bootstrap-select.js"));

            //DataTables needs JQuery and Bootstrap before it
            bundles.Add(new ScriptBundle("~/bundles/DataTables").Include("~/Scripts/moment.js", 
                "~/Scripts/datatables.js",                
                "~/Scripts/datatables-moment-plugin.js"));

            //Inputmask needs JQuery before it
            bundles.Add(new ScriptBundle("~/bundles/Inputmask").Include("~/Scripts/jquery.inputmask.bundle.js"));

            //DevExpress needs JQuery before it
            bundles.Add(new ScriptBundle("~/bundles/DevExpress").Include("~/Scripts/jquery-ui.js",
                "~/Scripts/cldr.js",
                "~/Scripts/event.js",
                "~/Scripts/supplemental.js",
                "~/Scripts/unresolved.js",
                "~/Scripts/globalize.js",
                "~/Scripts/message.js",
                "~/Scripts/number.js",
                "~/Scripts/date.js",
                "~/Scripts/currency.js",
                "~/Scripts/knockout.js",
                "~/Scripts/ace.js"));

            //Need to add this bundle after main DevExpress bundle
            bundles.Add(new ScriptBundle("~/bundles/DevExpressAdditional").Include("~/Scripts/ext-language_tools.js",
                "~/Scripts/theme-ambiance.js",
                "~/Scripts/theme-dreamweaver.js"));

            //Put any site-wide JavaScript files here
            bundles.Add(new ScriptBundle("~/bundles/SiteSpecific").Include("~/Scripts/js.cookie-2.2.0.min.js"));

            //Put any reports page specific JavaScript files here
            bundles.Add(new ScriptBundle("~/bundles/ReportsPage").Include("~/Scripts/reports-page.js"));

            // Use the Development version of Modernizr to develop with and learn from. Then, when you’re
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need
            //bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
            //                "~/Scripts/modernizr-*"));
        }
    }
}