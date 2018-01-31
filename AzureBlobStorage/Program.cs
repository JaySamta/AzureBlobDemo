using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure; // Namespace for CloudConfigurationManager
using Microsoft.WindowsAzure.Storage; //  Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Blob; //  Namespace for Blob storage types
using Microsoft.Azure; //Namespace for CloudConfigurationManager
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;

namespace AzureBlobStorage
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString")); //Note :- Parse the connection string and return a reference to the storage account.
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient(); //Note :- The CloudBlobClient class enables you to retrieve containers and blobs stored in Blob storage. 

                CloudBlobContainer container = blobClient.GetContainerReference("images"); //Note :- Retrieve a reference to a container.
                container.CreateIfNotExists(); //Note:- Create the container if it doesn't already exist.

                //   setPermission(container);
                //  uploadBlob(container);
                //   string AccessURL = GetBlobSasUri(container, "Daily Report Format.txt", null);
                //ReadBlobList(container);// Note :- Retrive List of Blob.
                //DownloadBlob(container);  // Download all Blob of Container.

                // Delete Blob.
                DeleteBlob(container, "MyBlob.txt");
            }
            catch (Exception ex)
            { }
        }
        public static void DeleteBlob(CloudBlobContainer container,string BlobName)
        {
            try
            {
                if (container != null)
                {
                    CloudBlockBlob blockBlob = container.GetBlockBlobReference(BlobName);
                    blockBlob.Delete();
                }
            }
            catch(Exception Ex)
            {

            }
        }
        public static void DownloadBlob(CloudBlobContainer container)
        {
            try
            {
                foreach (IListBlobItem item in container.ListBlobs(null, false))
                {
                    string blobname = item.Uri.Segments.Last();

                    string newUrl;
                    while ((newUrl = Uri.UnescapeDataString(blobname)) != blobname)
                        blobname = newUrl;
                    CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobname);
                    using (var fileStream = System.IO.File.OpenWrite(@"D:\Jay Samta\Projects\AzureBlobStorage\AzureBlobStorage\Download Blob\" + blobname + ""))
                    {
                        blockBlob.DownloadToStream(fileStream);
                    }
                }
            }
            catch (Exception ex)
            { }
        }
        public static void uploadBlob(CloudBlobContainer container)
        {
            try
            {
                CloudBlockBlob blockBlob = container.GetBlockBlobReference("MyBlob.txt"); //Note:-Retrieve reference to a blob named "myblob".
                using (var fileStream = System.IO.File.OpenRead(@"D:\Jay Samta\Projects\AzureBlobStorage\AzureBlobStorage\Upload Blob\CreditNoteLineItems.txt"))
                {
                    blockBlob.UploadFromStream(fileStream);
                }
            }
            catch(Exception ex)
            { }

        }
        public static void setPermission(CloudBlobContainer container)
        {
            try
            {
                // Note :-set Permission of Blob Public or Private --> BlobContainerPublicAccessType.Blob means its Access publically.
                container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
            }
            catch(Exception ex)
            {
            }
        }

        public static void ReadBlobList(CloudBlobContainer container)
        {
            try
            {
                System.IO.StreamWriter file = new System.IO.StreamWriter(@"D:\Jay Samta\Projects\AzureBlobStorage\AzureBlobStorage\ReadBlob\BlobList.txt");
                string[] lines = { };
                foreach (IListBlobItem item in container.ListBlobs(null, false))
                {
                    if (item.GetType() == typeof(CloudBlockBlob))
                    {
                        CloudBlockBlob blob = (CloudBlockBlob)item;
                        file.WriteLine("Block blob of length {0}: {1}", blob.Properties.Length, blob.Uri);

                        //Console.WriteLine("Block blob of length {0}: {1}", blob.Properties.Length, blob.Uri);
                    }
                    else if (item.GetType() == typeof(CloudPageBlob))
                    {
                        CloudPageBlob pageBlob = (CloudPageBlob)item;

                        Console.WriteLine("Page blob of length {0}: {1}", pageBlob.Properties.Length, pageBlob.Uri);

                    }
                    else if (item.GetType() == typeof(CloudBlobDirectory))
                    {
                        CloudBlobDirectory directory = (CloudBlobDirectory)item;

                        Console.WriteLine("Directory: {0}", directory.Uri);
                    }
                }
                file.Close();
            }
            catch (Exception ex)
            { }
        }
        private static string GetBlobSasUri(CloudBlobContainer container, string blobName, string policyName = null)
        {
            try
            {
                string sasBlobToken;

                // Get a reference to a blob within the container.
                // Note that the blob may not exist yet, but a SAS can still be created for it.
                CloudBlockBlob blob = container.GetBlockBlobReference(blobName);

                if (policyName == null)
                {
                    // Create a new access policy and define its constraints.
                    // Note that the SharedAccessBlobPolicy class is used both to define the parameters of an ad-hoc SAS, and
                    // to construct a shared access policy that is saved to the container's shared access policies.
                    SharedAccessBlobPolicy adHocSAS = new SharedAccessBlobPolicy()
                    {
                        // When the start time for the SAS is omitted, the start time is assumed to be the time when the storage service receives the request.
                        // Omitting the start time for a SAS that is effective immediately helps to avoid clock skew.

                        //SharedAccessExpiryTime = DateTime.UtcNow.AddHours(24), // Comment By Jay
                        SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(60),
                        Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.Write | SharedAccessBlobPermissions.Create
                    };

                    // Generate the shared access signature on the blob, setting the constraints directly on the signature.
                    sasBlobToken = blob.GetSharedAccessSignature(adHocSAS);

                    Console.WriteLine("SAS for blob (ad hoc): {0}", sasBlobToken);
                    Console.WriteLine();
                }
                else
                {
                    // Generate the shared access signature on the blob. In this case, all of the constraints for the
                    // shared access signature are specified on the container's stored access policy.
                    sasBlobToken = blob.GetSharedAccessSignature(null, policyName);
                    Console.WriteLine("SAS for blob (stored access policy): {0}", sasBlobToken);
                    Console.WriteLine();
                }

                // Return the URI string for the container, including the SAS token.
                return blob.Uri + sasBlobToken;
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
