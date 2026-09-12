using System.Xml;
using System.Text.RegularExpressions; 

// microsoft your documentation is ASS
// also genuinely screw every big news website we need to make news accessible again

class newsRead
{
    static async Task Main(string[] args)
    {
        string newsUrl = "https://feeds.npr.org/1001/rss.xml"; // thank GOD this worked
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("User-Agent",  "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36" ); // remnant of the old bbci struggle bullshit. it works so im keeping it

            while (true) // runs a forever loop so that the news can update without the program just closing
            {
                CancellationTokenSource? dateTimeCancellation = null;
                Task? dateTimeTask = null;
                try
                {
                    using CancellationTokenSource spinnerCancellation = new CancellationTokenSource();
                    Task spinnerTask = ShowSpinner(spinnerCancellation.Token); // ok this is obvious enough

                    string xmlContent; // now it instantiates a new variable xmlContent
                    try
                    {
                        xmlContent = await client.GetStringAsync(newsUrl); // it then gets a huge chunk of text all in one string 
                    }
                    finally
                    {
                        spinnerCancellation.Cancel(); // it then cancels the spinner (since we have the content now)
                        await spinnerTask;
                        Console.Write("\r" + new string(' ', Console.WindowWidth - 1) + "\r"); // clears the window
                    }

                    xmlContent = Regex.Replace(xmlContent, @"&(?!(amp|lt|gt|apos|quot);)", "&amp;"); // this took some time but basically i wanted to avoid being slimed out by potential shitty formatting
                    XmlDocument doc = new XmlDocument(); // the rest is obvious
                    doc.LoadXml(xmlContent);
                    XmlNodeList nodes = doc.GetElementsByTagName("item");
                    foreach (XmlNode item in nodes) // now we read through everything and print each headline!
                    {
                        string? headLine = item.SelectSingleNode("title")?.InnerText;
                        string? cleanHeadline = headLine != null ? System.Net.WebUtility.HtmlDecode(headLine) : null;
                        if (!string.IsNullOrEmpty(cleanHeadline)) //now that we have the decoded headline we print it!
                        {
                            Console.WriteLine(cleanHeadline);
                        }
                    }

                    dateTimeCancellation = new CancellationTokenSource();
                    dateTimeTask = Task.Run(() => fuckyou(dateTimeCancellation.Token));
                    await Task.Delay(TimeSpan.FromMinutes(15)); // now things change so we need to make it update just enough to keep the user up to date (if anyone actually runs this program long enough that is)
                }

                catch (Exception e) // yeah it's just a lot easier to show the user a huge scary block i can fix up then an ominous "Broken"
                {
                    Console.WriteLine(e);
                }
                finally
                {
                    if (dateTimeCancellation != null && dateTimeTask != null)
                    {
                        dateTimeCancellation.Cancel();
                        await dateTimeTask;
                        dateTimeCancellation.Dispose();
                        Console.WriteLine();
                    }
                }
            }
        }
    }

    static async Task fuckyou(CancellationToken cancellationToken) // this thing just DIDNT want to WORK........
    {
        try
        {
            while (true)
            {
                Console.Write($"\rCurrent time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}"); // LOOK AT WHAT I HAD TO DO
                await Task.Delay(1000, cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }

    static async Task ShowSpinner(CancellationToken cancellationToken) // this is repetitive i just copied it 
    {
        string[] frames = { "/", "-", "\\", "|" };
        int frame = 0;

        try
        {
            while (true)
            {
                Console.Write($"\r loading {frames[frame++ % frames.Length]}");
                await Task.Delay(100, cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }
}