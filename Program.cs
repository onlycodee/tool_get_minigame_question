using HtmlAgilityPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace CShard_Console
{
    class Program
    {
        static string GetWebPage(string url)
        {
            WebRequest request = WebRequest.Create(url);
            // If required by the server, set the credentials.
            request.Credentials = CredentialCache.DefaultCredentials;
            // Get the response.
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            while (response == null || response.StatusCode != HttpStatusCode.OK)
            {
                System.Threading.Thread.Sleep(1000);
                response = (HttpWebResponse)request.GetResponse();
            }
            // Display the status.
            //Console.WriteLine(response.StatusDescription);
            // Get the stream containing content returned by the server.
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            string responseFromServer = reader.ReadToEnd();
            // Display the content.
            //Console.WriteLine(responseFromServer);
            // Cleanup the streams and the response.
            reader.Close();
            dataStream.Close();
            response.Close();
            return responseFromServer;
        }
        static Dictionary<string,string> replacements = new Dictionary<string, string>();
        static Regex regex = new Regex("(&#[0-9]{2,6};)");
        static string EntityToUnicode(string html)
        {
            foreach (Match match in regex.Matches(html))
            {
                if (!replacements.ContainsKey(match.Value))
                {
                    var unicode = HttpUtility.HtmlDecode(match.Value);
                    if (unicode.Length == 1)
                    {
                        replacements.Add(match.Value, unicode);
                    }
                }
            }
            foreach (var replacement in replacements)
            {
                html = html.Replace(replacement.Key, replacement.Value);
            }
            return html;
        }

        static int MAX_ANSWERS = 500;

        static void Main(string[] args)
        {
            StreamWriter writer = new StreamWriter("P:/c#/CShard_Console/data.txt", false);
            string themeUrl = "https://dailythemedcrosswordanswers.com/";
            string themeHtml = GetWebPage(themeUrl);
            HtmlDocument themeDocument = new HtmlDocument();
            themeDocument.LoadHtml(themeHtml);
            HtmlNodeCollection themeNodes = themeDocument.DocumentNode.SelectNodes("//div[@class='entry-content']/ul/li/strong/a");
            int questionCount = 0;
            foreach (HtmlNode themNode in themeNodes)
            {
                if (questionCount >= MAX_ANSWERS) break;
                string levelUrl = themNode.Attributes["href"].Value;
                string levelHtml = GetWebPage(levelUrl);
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(levelHtml);
                HtmlNodeCollection levels = document.DocumentNode.SelectNodes("//div[@class='entry-content']/div[@class='list']/h5/li/a");
                foreach (HtmlNode level in levels)
                {
                    if (questionCount >= MAX_ANSWERS) break;
                    string levelLink = level.Attributes["href"].Value;
                    string questionHtml = GetWebPage(levelLink);
                    HtmlDocument questionDoc = new HtmlDocument();
                    questionDoc.LoadHtml(questionHtml);
                    //HtmlNodeCollection coll = document.DocumentNode.SelectNodes("//div[@class='entry-content']/div[@class='list']/li");
                    //Console.WriteLine(questionHtml);
                    HtmlNodeCollection questions = questionDoc.DocumentNode.SelectNodes("//div[@class='entry-content']/div[@class='list']/li/a");
                    //Console.WriteLine("length: " + questions.Count);
                    foreach (var question in questions)
                    {
                        if (questionCount >= MAX_ANSWERS) break;
                        string questionDetailLink = question.Attributes["href"].Value;
                        string questionDetailHtml = GetWebPage(questionDetailLink);
                        HtmlDocument questionDetailDoc = new HtmlDocument();
                        questionDetailDoc.LoadHtml(questionDetailHtml);
                        //Console.WriteLine(questionDetailHtml);
                        var questionStr = questionDetailDoc.DocumentNode.SelectSingleNode("//h1").InnerText;
                        var answer = questionDetailDoc.DocumentNode.SelectSingleNode("//div[@class='entry-content']/div[@class='answers']").InnerText;
                        questionStr = EntityToUnicode(questionStr);
                        if (questionStr.Contains(" Answers"))
                        {
                            questionStr = questionStr.Remove(questionStr.Length - 8);
                            if (questionStr.EndsWith("."))
                            {
                                questionStr = questionStr.Remove(questionStr.Length - 1);
                            }
                            if (questionStr.Contains("What ") || questionStr.Contains("Who ") || questionStr.Contains("Which ") || questionStr.Contains("Where"))
                            {
                                questionStr += "?";
                            }
                        }
                        //Console.WriteLine("question: " + questionStr + " answer: " + answer);
                        if (!questionStr.Contains("wds") && questionStr.Length >= 30 && answer.Length >= 3 && answer.Length <= 8)
                        {
                            questionCount++;
                            writer.WriteLine(questionStr);
                            writer.WriteLine(answer);
                            Console.WriteLine(questionCount);
                        }
                    }
                }
            }

            writer.Close();
            Console.WriteLine("Doneeeeeeeeeeeeeeeeeeeeee");
        }
    }
}
