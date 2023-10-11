using System;
using DevExpress.XtraReports.Web.WebDocumentViewer;

namespace Pyramid.Code.DevExpressHelpers
{
	public class CustomWebDocumentViewerExceptionHandler : WebDocumentViewerExceptionHandler
	{
		public override string GetExceptionMessage(Exception ex)
		{
			//Log the error via ELMAH
			Utilities.LogException(ex);

			//Return the message if it exists
			if (ex != null && !string.IsNullOrWhiteSpace(ex.Message))
			{
				return ex.Message;
			}
			else
            {
				return "No message found...";
            }
		}
	}
}