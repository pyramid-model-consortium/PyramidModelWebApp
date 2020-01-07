using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Native;
using DevExpress.DataAccess.Web;
using System.Collections.Generic;
using System.Linq;

namespace Pyramid.Reports.UserReports
{
    /// <summary>
    /// This class supplies the connection strings to the DevExpress web report designer
    /// </summary>
    public class PyramidReportConnectionStringProvider : IDataSourceWizardConnectionStringsProvider
    {
        public Dictionary<string, string> GetConnectionDescriptions()
        {
            //Get the connection strings
            Dictionary<string, string> connections = AppConfigHelper.GetConnections().Keys.ToDictionary(x => x, x => x);

            //Get the report connection
            KeyValuePair<string, string> reportConnection = connections.Where(c => c.Key == "PyramidReporting").FirstOrDefault();

            //Create a new Dictionary and add the report connection
            Dictionary<string, string> reportingConnections = new Dictionary<string, string>();
            reportingConnections.Add(reportConnection.Key, reportConnection.Value);

            //Return the report connections
            return reportingConnections;
        }

        public DataConnectionParametersBase GetDataConnectionParameters(string name)
        {
            return AppConfigHelper.LoadConnectionParameters(name);
        }
    }
}