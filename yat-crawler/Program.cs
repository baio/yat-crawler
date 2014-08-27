
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
using System.Linq;

namespace Abot.Demo
{
    class Program
    {


        static void Main(string[] args)
        {
            //JobHost host = new JobHost();
            Craw();

            System.Console.ReadLine();
        }

        public static void Craw()
        {
            
            log4net.Config.XmlConfigurator.Configure();
            
            IWebCrawler crawler;

            crawler = GetManuallyConfiguredWebCrawler();

            crawler.PageCrawlCompletedAsync += crawler_ProcessPageCrawlCompleted;

            Uri uriToCrawl = new Uri("http://www.1obl.ru/news/?PAGEN_1=1");
            crawler.Crawl(uriToCrawl);

            //stream.Close();
                 
        }


        private static IWebCrawler GetManuallyConfiguredWebCrawler()
        {
            //Create a config object manually

            var linkParser = new HapHyperLinkParser();

            CrawlConfiguration crawlConfig = new CrawlConfiguration();
            crawlConfig.MaxPagesToCrawl = 1100;

            return new PoliteWebCrawler(crawlConfig, null, null, null, null, new MyLinkParser(), null, null, null);
        }


        private class MyLinkParser : HapHyperLinkParser
        {
            int count = 1;

            public override System.Collections.Generic.IEnumerable<Uri> GetLinks(CrawledPage crawledPage)
            {
                if (crawledPage.Uri.AbsoluteUri.StartsWith("http://www.1obl.ru/news/?PAGEN_1="))
                {
                    var uris = crawledPage.CsQueryDocument[".main_newslist .news_list_item .item__title a"].ToList().Select(p => new Uri(crawledPage.Uri.Scheme + "://" + crawledPage.Uri.Host + p.GetAttribute("href"))).ToList();

                    count++;
                    uris.Add(new Uri("http://www.1obl.ru/news/?PAGEN_1=" + count));

                    return uris;
                }
                else
                {
                    return new Uri[0];
                }
            }
        }
        

        private static void PrintAttentionText(string text)
        {
            ConsoleColor originalColor = System.Console.ForegroundColor;
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.WriteLine(text);
            System.Console.ForegroundColor = originalColor;
        }

        static CloudBlobContainer container;
        static CloudBlobContainer container_tags;

        static StreamWriter stream;// = new StreamWriter("../../data/tags.txt", true);

        static void crawler_ProcessPageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {

            if (!e.CrawledPage.Uri.AbsoluteUri.StartsWith("http://www.1obl.ru/news/"))
            {
                throw new Exception("Unexpected URI");
            }


            string content = e.CrawledPage.CsQueryDocument["#post_body"].Text();
            string tagsContent = e.CrawledPage.CsQueryDocument["#main_section .news_item_tags"].Text();

            /*
            if (container == null)
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureJobsData"].ConnectionString);
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                container = blobClient.GetContainerReference("1obl");
                container.CreateIfNotExist();

                container_tags = blobClient.GetContainerReference("1obltags");
                container_tags.CreateIfNotExist();
            }
             */

            if (!string.IsNullOrEmpty(content))
            {
                string name = System.Web.HttpUtility.UrlEncode(e.CrawledPage.Uri.ToString());

                System.IO.File.WriteAllText("../../../data/contents/" + name + ".txt", content);


                /*
                CloudBlob blob = container.GetBlobReference(name);
                blob.DeleteIfExists();
                blob.UploadText(content);
                 */
            }

            /*
            if (!string.IsNullOrEmpty(tags))
            {
                tags = Regex.Replace(tags, @"\t", "");
                tags = Regex.Replace(tags, @", ", ",");
                tags = Regex.Replace(tags, @"( |\t|\r?\n)\1", "$1");
                tags = Regex.Replace(tags, @"(?:(?:\r?\n)+ +){2,}", @"\n");


                string name = System.Web.HttpUtility.UrlEncode(e.CrawledPage.Uri.ToString());
                CloudBlob blob = container_tags.GetBlobReference(name);
                blob.DeleteIfExists();
                blob.UploadText(tags);

            }
             */


            /*
            if (!string.IsNullOrEmpty(tagsContent))
            {
                var tags = Regex.Replace(tagsContent, @"[\t\r\n]", "");
                tags = Regex.Replace(tagsContent, @"\s+", " ");
                tags = tags.Trim();
                
                TextWriter.Synchronized(stream).WriteLine(tags);

            }
             */
        }

    }
}
