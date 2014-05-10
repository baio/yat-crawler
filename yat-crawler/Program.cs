
using Abot.Core;
using Abot.Crawler;
using Abot.Poco;
using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.WindowsAzure.Jobs;
using Microsoft.WindowsAzure;
using System.Configuration;
using Microsoft.WindowsAzure.StorageClient;

namespace Abot.Demo
{
    class Program
    {


        static void Main(string[] args)
        {
            //JobHost host = new JobHost();
            Craw();
        }



        public static void Craw()
        {
            
            log4net.Config.XmlConfigurator.Configure();
            PrintDisclaimer();

            Uri uriToCrawl = new Uri("http://rasp.yandex.ru/station/2000003?direction=all&type=suburban"); //GetSiteToCrawl(args);

            IWebCrawler crawler;

            //Uncomment only one of the following to see that instance in action
            //crawler = GetDefaultWebCrawler();
            crawler = GetManuallyConfiguredWebCrawler();
            //crawler = GetCustomBehaviorUsingLambdaWebCrawler();

            //Subscribe to any of these asynchronous events, there are also sychronous versions of each.
            //This is where you process data about specific events of the crawl
            crawler.PageCrawlCompletedAsync += crawler_ProcessPageCrawlCompleted;

            //Start the crawl
            //This is a synchronous call
            CrawlResult result = crawler.Crawl(uriToCrawl);

            //Now go view the log.txt file that is in the same directory as this executable. It has
            //all the statements that you were trying to read in the console window :).
            //Not enough data being logged? Change the app.config file's log4net log level from "INFO" TO "DEBUG"

            PrintDisclaimer();
        
        }


        private static IWebCrawler GetManuallyConfiguredWebCrawler()
        {
            //Create a config object manually

            var linkParser = new HapHyperLinkParser();

            return new PoliteWebCrawler(null, null, null, null, null, new MyLinkParser(), null, null, null);
        }


        private class MyLinkParser : HapHyperLinkParser
        {
            public override System.Collections.Generic.IEnumerable<Uri> GetLinks(CrawledPage crawledPage)
            {
                var uris = base.GetLinks(crawledPage);

                var res = new List<Uri>();


                if (crawledPage.CrawlDepth < 2)
                {
                    foreach (var uri in uris)
                    {
                        string link = null;
                        var baseLink = "http://" + uri.Host + uri.AbsolutePath;


                        if (uri.AbsoluteUri.Contains("http://rasp.yandex.ru/thread/"))
                        {
                            link = baseLink;
                        }
                        else if (uri.AbsoluteUri.Contains("http://rasp.yandex.ru/station/"))
                        {
                            var strNum = uri.Segments[uri.Segments.Length - 1];

                            int num = 0;

                            if (int.TryParse(strNum, out num))
                            {
                                if (num >= 2000000 && num <= 2000010)
                                {
                                    link = baseLink + "?direction=all&type=suburban";
                                }
                            }
                        }

                        if (link != null)
                        {
                            var item = new Uri(link);

                            if (link != null && !res.Contains(item))
                                res.Add(item);
                        }

                    }
                }

                return res;
            }
        }

        

        private static void PrintDisclaimer()
        {
            PrintAttentionText("The demo is configured to only crawl a total of 10 pages and will wait 1 second in between http requests. This is to avoid getting you blocked by your isp or the sites you are trying to crawl. You can change these values in the app.config or Abot.Console.exe.config file.");
        }

        private static void PrintAttentionText(string text)
        {
            ConsoleColor originalColor = System.Console.ForegroundColor;
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.WriteLine(text);
            System.Console.ForegroundColor = originalColor;
        }

        static CloudBlobContainer container;

        static void crawler_ProcessPageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            Uri stationUri = null;

            if (e.CrawledPage.Uri.AbsoluteUri.StartsWith("http://rasp.yandex.ru/thread/"))
            {
                stationUri = e.CrawledPage.ParentUri;
            }
            else if (e.CrawledPage.Uri.AbsoluteUri.StartsWith("http://rasp.yandex.ru/station/"))
            {
                //stationUri = e.CrawledPage.Uri;
                return;
            }
            else throw new Exception("Unexpected URI");

            string name = System.Web.HttpUtility.UrlEncode(e.CrawledPage.Uri.ToString());
            string content = e.CrawledPage.Content.Text;

            if (container == null)
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureJobsData"].ConnectionString);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                container = blobClient.GetContainerReference("yat-moscow-subtrains");
                container.CreateIfNotExist();                
            }

            CloudBlob blob = container.GetBlobReference(name);
            blob.DeleteIfExists();
            blob.UploadText(content);


        }

    }
}