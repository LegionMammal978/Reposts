using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;

class C0
{
    const string PROCESSED_FILE = "/home/lm978/reposts_processed.txt";
    const string USER_AGENT = "linux:LM978_Reposts:1.0 (by /u/LegionMammal978)";
    const string WEB_BROWSER = "firefox";

    static void Main()
    {
        List<string> lSsE0 = new List<string>(File.ReadAllLines(PROCESSED_FILE));
        foreach (string s0 in F0())
        {
            if (lSsE0.Contains(s0))
                continue;
            Console.WriteLine("Analyzing {0}...", s0);
            JObject jo0 = F1("https://api.reddit.com/by_id/t3_" + s0.Split('/')[4]);
            List<string> lSsE1 = new List<string>();
            HttpWebRequest hwr0 = WebRequest.CreateHttp("http://karmadecay.com" + s0);
            hwr0.UserAgent = USER_AGENT;
            using (HttpWebResponse hwr1 = (HttpWebResponse)hwr0.GetResponse())
            using (Stream s1 = hwr1.GetResponseStream())
            using (StreamReader sr0 = new StreamReader(s1))
            {
                HtmlDocument hd0 = new HtmlDocument();
                hd0.Load(sr0);
                try
                {
                    foreach (HtmlNode hn0 in hd0.DocumentNode.SelectNodes("//div[@class='title']/a"))
                        lSsE1.Add(hn0.Attributes["href"].Value);
                }
                catch
                {
                    Console.Write("KD error. Opening post in browser. Press Enter to continue... ");
                    Process.Start(WEB_BROWSER, "https://reddit.com" + s0);
                    Console.ReadLine();
                    lSsE0.Add(s0);
                    File.WriteAllLines(PROCESSED_FILE, lSsE0);
                    continue;
                }
            }
            lSsE1.RemoveAt(0);
            for (int i0 = 0; i0 < lSsE1.Count; i0++)
            {
                JObject jo1 = F1("https://api.reddit.com/by_id/t3_" + lSsE1[i0].Split('/')[6]);
                if (jo1["data"]["children"].Value<JArray>().Count == 0 || jo1["data"]["children"][0]["data"]["created_utc"].Value<long>() > jo0["data"]["children"][0]["data"]["created_utc"].Value<long>())
                {
                    lSsE1.RemoveAt(i0);
                    i0--;
                    continue;
                }
                if ("gifs".Equals(jo1["data"]["children"][0]["data"]["subreddit"].Value<string>()))
                {
                    if (String.IsNullOrEmpty(jo1["data"]["children"][0]["data"]["link_flair_text"].Value<string>()) && !"[deleted]".Equals(jo1["data"]["children"][0]["data"]["author"].Value<string>()))
                        continue;
                    lSsE1.RemoveAt(i0);
                    i0--;
                    continue;
                }
                if (jo1["data"]["children"][0]["data"]["score"].Value<int>() < 1500 || jo0["data"]["children"][0]["data"]["created_utc"].Value<long>() - jo1["data"]["children"][0]["data"]["created_utc"].Value<long>() > 1209600)
                {
                    lSsE1.RemoveAt(i0);
                    i0--;
                    continue;
                }
            }
            if (lSsE1.Count == 0)
            {
                Console.WriteLine("Found no candidates.");
                lSsE0.Add(s0);
                File.WriteAllLines(PROCESSED_FILE, lSsE0);
                continue;
            }
            Console.WriteLine("Found {0} candidates. Opening in browser.", lSsE1.Count);
            Process.Start(WEB_BROWSER, "http://karmadecay.com" + s0);
            Thread.Sleep(250);
            Process.Start(WEB_BROWSER, "https://reddit.com" + s0);
            Thread.Sleep(250);
            for (int i0 = 0; i0 < lSsE1.Count; i0++)
            {
                Console.WriteLine("{0}: {1}", (char)(i0 + 'A'), lSsE1[i0]);
                Process.Start(WEB_BROWSER, lSsE1[i0]);
                Thread.Sleep(250);
            }
            string s2;
            do
            {
                Console.Write("Are any candidates viable? ");
                s2 = Console.ReadLine();
            } while (s2.Length == 0);
            if (s2[0] != 'y' && s2[0] != 'Y')
            {
                lSsE0.Add(s0);
                File.WriteAllLines(PROCESSED_FILE, lSsE0);
                continue;
            }
            Console.WriteLine("Opening report template in browser...");
            Process.Start(WEB_BROWSER, "https://www.reddit.com/message/compose?message=" + Uri.EscapeDataString(String.Format("https://redd.it/{0} is a repost, see http://karmadecay.com{1}", s0.Split('/')[4], s0)) + "&subject=No%20Reposts%20or%20Recent%20Popular%20Crossposts&to=/r/gifs");
            Thread.Sleep(250);
            Console.Write("Press Enter to continue... ");
            Console.ReadLine();
            lSsE0.Add(s0);
            File.WriteAllLines(PROCESSED_FILE, lSsE0);
        }
    }

    static IEnumerable<string> F0(string s0 = "")
    {
        JObject jo0 = F1("https://api.reddit.com/r/gifs/new?limit=100" + s0);
        foreach (JToken jt0 in jo0["data"]["children"])
            if (!jt0["data"]["over_18"].Value<bool>() && jt0["data"]["score"].Value<int>() >= 20)
                yield return jt0["data"]["permalink"].Value<string>();
        if (jo0["data"]["after"].Value<string>() != null)
            foreach (string s1 in F0("&after=" + jo0["data"]["after"].Value<string>()))
                yield return s1;
    }

    static JObject F1(string s0)
    {
        HttpWebRequest hwr0 = WebRequest.CreateHttp(s0);
        hwr0.UserAgent = USER_AGENT;
        using (HttpWebResponse hwr1 = (HttpWebResponse)hwr0.GetResponse())
        using (Stream s1 = hwr1.GetResponseStream())
        using (StreamReader sr0 = new StreamReader(s1))
        using (JsonTextReader jtr0 = new JsonTextReader(sr0))
            return JObject.Load(jtr0);
    }
}