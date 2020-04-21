﻿using HtmlAgilityPack;
using Steam.Models.DOTA2;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;

namespace DotaDb.Data
{
    public class BlogFeedService
    {
        public async Task<IEnumerable<DotaBlogFeedItem>> GetDotaBlogFeedItemsAsync()
        {
            List<DotaBlogFeedItem> dotaBlogFeedItems = new List<DotaBlogFeedItem>();

            string url = "http://blog.dota2.com/feed/";

            using (XmlReader reader = XmlReader.Create(url))
            {
                SyndicationFeed feed = SyndicationFeed.Load(reader);
                foreach (var item in feed.Items)
                {
                    var creator = GetExtensionElementValue<string>(item, "creator");
                    var contentEncoded = GetExtensionElementValue<string>(item, "encoded");
                    var category = item.Categories.FirstOrDefault();
                    var link = item.Links.FirstOrDefault();
                    var description = item.Summary.Text.Substring(0, item.Summary.Text.IndexOf("&#8230;"));
                    description = WebUtility.HtmlDecode(description);
                    description += ". . .";

                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(contentEncoded);
                    var imgTag = doc.DocumentNode.Descendants("img").FirstOrDefault();

                    dotaBlogFeedItems.Add(new DotaBlogFeedItem()
                    {
                        Category = category?.Name ?? "Unknown",
                        Author = creator,
                        Description = description,
                        PublishDate = item.PublishDate.LocalDateTime,
                        Link = link?.Uri.ToString() ?? "Unknown",
                        Title = item.Title.Text,
                        ContentEncoded = contentEncoded,
                        ImageUrl = imgTag?.GetAttributeValue("src", null)
                    });
                }
            }

            return await Task.FromResult(dotaBlogFeedItems);
        }
        private static T GetExtensionElementValue<T>(SyndicationItem item, string extensionElementName)
        {
            return item.ElementExtensions.Where(ee => ee.OuterName == extensionElementName).First().GetObject<T>();
        }
    }
}