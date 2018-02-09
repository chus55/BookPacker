using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace BookPacker
{
    public class DataExtractor
    {
        public static BookData ParseThisWebsite(string website)
        {
            if (Path.GetExtension(website) != ".html")
            {
                PrintNotHtmlError(website);
                return null;
            }
            try
            {
                String source = File.ReadAllText(website);
                source = WebUtility.HtmlDecode(source);
                HtmlDocument htmlSource = new HtmlDocument();
                htmlSource.LoadHtml(source);
                BookData bookData = new BookData();
                
                var titleAndAuthorNode = GetTitleAndAuthorNode(htmlSource);

                string title = GetInnerTextFromNodeByTagAndAttributeContaining(titleAndAuthorNode, "span", "id", "btAsinTitle");

                List<string> authors = GetAuthorsFromNode(titleAndAuthorNode);
                bookData.Authors = authors.ToArray();

                string selectedPrice = GetSelectedPrice(htmlSource);
                bookData.SelectedPrice = float.Parse(selectedPrice);

                var priceNode = GetPriceNode(htmlSource);
                List<BookFormats> bookFormatList = new List<BookFormats>();
                if (priceNode == null)
                {
                    string format = GetFormatFromTitle(title);
                    bookFormatList.Add(new BookFormats() { Format = format, Price = float.Parse(selectedPrice) });
                }
                else
                {
                    var formatSubNodes = GetSubnodesFromNodeByTagsAndContainsDoubleDown(priceNode, "tr", "td", "class", "tmm_bookTitle");
                    bookFormatList = BuildBookFormatListFromSubNodes(formatSubNodes);
                }
                title = RemoveAmazonFormatSpecification(title);
                bookData.Title = title;
                bookData.FormatPrices = bookFormatList.ToArray();

                var productDetailsNode = GetProductDetailsNode(htmlSource);
                
                string shippingWeight = GetInnerTextFromNodeByTagAndContains(productDetailsNode, "li", "Shipping Weight:");
                shippingWeight = GetNumberFromString(shippingWeight);
                bookData.ShippingWeight = float.Parse(shippingWeight);
                
                string isbn10 = GetInnerTextFromNodeButContains(productDetailsNode, "li", "ISBN-10: ");
                bookData.Isbn10 = isbn10;
                
                string isbn13 = GetInnerTextFromNodeButContains(productDetailsNode, "li", "ISBN-13: ");
                bookData.Isbn13 = isbn13;

                return bookData;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static List<BookData> ParseFolderWithWebsites(string folderPath)
        {
            DirectoryInfo dir = new DirectoryInfo(folderPath);
            FileInfo[] htmlSources = dir.GetFiles().ToArray();
            List<BookData> bookDataList = new List<BookData>();

            foreach (var htmlFile in htmlSources)
            {
                if (Path.GetExtension(htmlFile.FullName) == ".html")
                    bookDataList.Add(ParseThisWebsite(htmlFile.FullName));
            }
            return bookDataList;
        }

        private static void PrintNotHtmlError(string website)
        {
            Console.WriteLine("\"" + website + "\" is not a valid html document.");
            Console.WriteLine("Skipping...");
        }

        private static string GetSelectedPrice(HtmlDocument htmlSource)
        {
            string price = GetInnerTextByTagAndAttributeContaining(htmlSource, "span", "class", "bb_price");
            return GetNumberFromString(price);
        }
        private static string GetFormatFromTitle(string title)
        {
            string regex = @"\[(.*?)\]";
            Regex re = new Regex(regex);
            var matches = re.Matches(title);
            if (matches.Count > 0)
            {
                string returnFormat = matches[0].Value;
                returnFormat = returnFormat.Replace("[", string.Empty);
                returnFormat = returnFormat.Replace("]", string.Empty);
                return returnFormat;
            }
            return "Normal";
        }

        private static List<BookFormats> BuildBookFormatListFromSubNodes(List<HtmlNode> subNodes)
        {
            List<BookFormats> returnbookFormatsList = new List<BookFormats>();
            foreach (var node in subNodes)
            {
                string format = GetInnerTextFromNodeByTagAndAttributeContaining(node, "td", "class", "tmm_bookTitle");
                format = format.Trim();
                string price = GetInnerTextFromNodeByTagAndAttributeContaining(node, "td", "class", "price");
                price = GetNumberFromString(price);
                if (!string.IsNullOrEmpty(format) && !string.IsNullOrEmpty(price))
                    returnbookFormatsList.Add(new BookFormats(){Format = format, Price = float.Parse(price)});
            }
            return returnbookFormatsList;
        }

        private static List<HtmlNode> BuildDescendantListFromNodeTagAttributeContains(HtmlNode node, string tag, string attribute, string contains)
        {
            return node.Descendants().Where
            (x => x.Name == tag && x.Attributes[attribute] != null &&
                  x.Attributes[attribute].Value.Contains(contains)).ToList();
        }

        private static HtmlNode FindDescendantTagAttributeContain(List<HtmlNode> nodetList, string tag, string attribute, string contains)
        {
            foreach (var descendant in nodetList)
            {
                List<HtmlNode> descendantList = BuildDescendantListFromNodeTagAttributeContains(descendant, tag, attribute, contains);
                if (descendantList.Count > 0)
                    return descendant;
            }
            return null;
        }

        private static List<HtmlNode> GetSubnodesFromNodeByTagsAndContainsDoubleDown(HtmlNode node, string returnTag, string tagToFind, string attribute, string contains)
        {
            List<HtmlNode> descendantList = node.Descendants(returnTag).ToList();
            List<HtmlNode> returnNodeList = new List<HtmlNode>();

            foreach (var descendant in descendantList)
            {
                List<HtmlNode> tdDescendantList = BuildDescendantListFromNodeTagAttributeContains(descendant, tagToFind, attribute, contains);
                if (tdDescendantList.Count > 0)
                    returnNodeList.Add(descendant);
            }
            return returnNodeList;
        }

        private static HtmlNode GetPriceNode(HtmlDocument htmlSite)
        {
            List<HtmlNode> divDescendantList = htmlSite.DocumentNode.Descendants().Where
            (x => x.Name == "div" && x.Attributes["class"] != null).ToList();
            return FindDescendantTagAttributeContain(divDescendantList, "td", "class", "tmm_bookTitle");
        }

        private static List<HtmlNode> BuildDescendantListFromSourceTagAttributeContain(HtmlDocument htmlSite, string tag, string attribute, string contains)
        {
            return htmlSite.DocumentNode.Descendants().Where
            (x => x.Name == tag && x.Attributes[attribute] != null &&
                  x.Attributes[attribute].Value.Contains(contains)).ToList();
        }

        private static HtmlNode GetTitleAndAuthorNode(HtmlDocument htmlSite)
        {
            List<HtmlNode> divDescendantList = BuildDescendantListFromSourceTagAttributeContain(htmlSite, "div", "class", "buying");
            var returnNode = FindDescendantTagAttributeContain(divDescendantList, "h1", "class", "parseasinTitle");
            if (returnNode != null)
                return returnNode;
            throw new Exception("Could not find title and author node.");
        }

        private static HtmlNode GetProductDetailsNode(HtmlDocument htmlSite)
        {
            List<HtmlNode> tdDescendantList = BuildDescendantListFromSourceTagAttributeContain(htmlSite, "td", "class", "bucket");
            var returnNode = FindDescendantTagInnerTextContain(tdDescendantList, "li", "Shipping Weight:");
            if (returnNode != null)
                return returnNode;
            throw new Exception("Could not find title and author node.");
        }

        private static List<HtmlNode> BuildDescendantListNodeTagInnerTextContain(HtmlNode node, string tag, string contains)
        {
            return node.Descendants().Where
                (x => x.Name == tag && x.InnerText.Contains(contains)).ToList();
        }

        private static HtmlNode FindDescendantTagInnerTextContain(List<HtmlNode> nodeList, string tag, string contains)
        {
            foreach (var descendant in nodeList)
            {
                List<HtmlNode> descendantList = BuildDescendantListNodeTagInnerTextContain(descendant, tag, contains);
                if (descendantList.Count > 0)
                    return descendant;
            }
            return null;
        }

        private static List<string> GetAuthorsFromNode(HtmlNode titleAndAuthorNode)
        {
            List<string> authorList = new List<string>();
            List<HtmlNode> descendantList = titleAndAuthorNode.Descendants().Where
                (x => x.Name == "a" && x.Attributes["href"] != null).ToList();
            foreach (var author in descendantList)
            {
                authorList.Add(author.InnerText);
            }
            return authorList;
        }

        private static string GetNumberFromString(string str)
        {
            try
            {
                return Regex.Replace(str, "[^0-9.]", "");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Couldn't extract number from string.");
            }
            return null;
        }

        private static string RemoveAmazonFormatSpecification(string title)
        {
            string regex = @"\[.*\]";
            return Regex.Replace(title, regex, string.Empty);
        }

        private static string GetInnerTextFromNodeButContains(HtmlNode node, string tagName, string contains)
        {
            string innerText = GetInnerTextFromNodeByTagAndContains(node, tagName, contains);
            return innerText.Replace(contains, string.Empty);
        }

        private static string GetInnerTextByTagAndAttributeContaining(HtmlDocument htmlSite, string tagName, string attribute, string contains)
        {
            List<HtmlNode> descendantList = BuildDescendantListFromSourceTagAttributeContain(htmlSite, tagName, attribute, contains);
            return descendantList.First().InnerText;
        }

        private static string GetInnerTextFromNodeByTagAndContains(HtmlNode node, string tagName, string contains)
        {
            List<HtmlNode> descendantList = BuildDescendantListNodeTagInnerTextContain(node, tagName, contains);
            return descendantList.First().InnerText;
        }

        private static string GetInnerTextFromNodeByTagAndAttributeContaining(HtmlNode node, string tagName, string attribute, string contains)
        {
            List<HtmlNode> descendantList = BuildDescendantListFromNodeTagAttributeContains(node, tagName, attribute, contains);
            return descendantList.First().InnerText;
        }
    }
}
