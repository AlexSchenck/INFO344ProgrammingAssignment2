using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Web.Services;
using WebRole1;

namespace WebRole1
{
    /// <summary>
    /// Summary description for getQuerySuggestions
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class getQuerySuggestions : System.Web.Services.WebService
    {
        private static String blobConnectionString =
            "DefaultEndpointsProtocol=https;AccountName=ansstorage;AccountKey=vVnfPu8VLmUMyO1R72znrXHWsYFcLxuvYvOPWFPedI0IorXCCjzek3I1jnAfbxMWcaGLJ+qgmUw4Jtffp3OxXw==";
        private static String blobName = "ansblob";
        private static Trie trie;
        private PerformanceCounter memProcess = new PerformanceCounter("Memory", "Available Mbytes");
        private String blobFileName = "WikipediaTitles";
        private String filePath;

        [WebMethod]
        public List<String> searchTrie(String query) 
        {
            List<String> results = trie.searchPrefix(query);
            
            return results;
        }

        [WebMethod]
        private void buildTrie(String fileName) 
        {
            trie = new Trie();
            
            using (StreamReader reader = new StreamReader(filePath + fileName))
            {
                String line;
                int check = 0;

                while ((line = reader.ReadLine()) != null)
                {
                    //Checks memory every 1000 adds
                    //If under 50MB, stops building Trie
                    if (check % 1000 == 0)
                        if (memProcess.NextValue() < 50)
                            break;

                    trie.addTitle(line);
                    check++;
                }
            }
        }

        //Returns text file of wikipedia titles from blob storage in the form of a blob item
        [WebMethod]
        public void downloadWiki() 
        {
            Debug.WriteLine("downloadWiki started!");
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(blobConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(blobName);
            filePath = System.IO.Path.GetTempFileName();

            foreach (IListBlobItem item in container.ListBlobs(null, false))
            {
                if (item.GetType() == typeof(CloudBlockBlob))
                {
                    CloudBlockBlob blob = (CloudBlockBlob)item;
                    if (blob.Name.Equals(blobFileName))
                    {
                        using (var fileStream = System.IO.File.OpenWrite(filePath + blob.Name))
                        {
                            blob.DownloadToStream(fileStream);
                            fileStream.Close();
                            Debug.WriteLine("downloadWiki finished!");
                            buildTrie(blobFileName);
                        }
                    }
                }
            }
        }
    }
}
