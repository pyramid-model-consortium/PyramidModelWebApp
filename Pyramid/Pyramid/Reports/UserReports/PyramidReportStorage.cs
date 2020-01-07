using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Configuration;
using DevExpress.XtraReports.Web.Extensions;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Pyramid.Reports.UserReports
{
    /// <summary>
    /// This class is required for the DevExpress web report designer and it connects to the Azure
    /// Blob storage account for this application and stores reports in the reports container
    /// </summary>
    public class PyramidReportStorage : ReportStorageWebExtension
    {
        readonly CloudBlobContainer container;
        public PyramidReportStorage()
        {
            //Get the BLOB client
            CloudBlobClient blobClient = CloudStorageAccount.Parse(WebConfigurationManager.ConnectionStrings["PyramidStorage"].ConnectionString).CreateCloudBlobClient();

            //Get the reports container
            container = blobClient.GetContainerReference("reports");
        }

        public override bool CanSetData(string url)
        {
            CloudBlockBlob blob = container.GetBlockBlobReference(url);
            return blob.Exists();
        }
        public override bool IsValidUrl(string url)
        {
            return true;
        }

        public override byte[] GetData(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                    return null;
                var blob = container.GetBlockBlobReference(url);
                if (!blob.Exists())
                    throw new ArgumentException("Report not found", "url");
                var byteArr = new byte[blob.StreamWriteSizeInBytes];
                blob.DownloadToByteArray(byteArr, 0);
                return byteArr;
            }
            catch (StorageException e)
            {
                System.Diagnostics.Debug.WriteLine("Operation failed. " + e.Message);
                throw;
            }
        }

        public override Dictionary<string, string> GetUrls()
        {
            Dictionary<string, string> reports = new Dictionary<string, string>();
            try
            {
                foreach (ICloudBlob blob in container.ListBlobs())
                    reports.Add(blob.Name, blob.Name);
            }
            catch (StorageException e)
            {
                System.Diagnostics.Debug.WriteLine("Operation failed." + e.Message);
            }
            return reports;
        }

        public override void SetData(DevExpress.XtraReports.UI.XtraReport report, string url)
        {
            CloudBlockBlob blob = container.GetBlockBlobReference(url);
            if (blob.Exists())
            {
                using (var stream = new MemoryStream())
                {
                    report.SaveLayoutToXml(stream);
                    stream.Position = 0;
                    blob.UploadFromStream(stream);
                }
            }
        }

        public override string SetNewData(DevExpress.XtraReports.UI.XtraReport report, string defaultUrl)
        {
            string id = defaultUrl;
            CloudBlockBlob blob = container.GetBlockBlobReference(defaultUrl);
            if (blob.Exists())
            {
                id = defaultUrl + Guid.NewGuid().ToString("N");
                blob = container.GetBlockBlobReference(id);
            }
            using (var stream = new MemoryStream())
            {
                report.SaveLayoutToXml(stream);
                stream.Position = 0;
                blob.UploadFromStream(stream);
            }
            return id;
        }
    }
}
