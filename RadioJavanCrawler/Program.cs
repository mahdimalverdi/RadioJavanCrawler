using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace RadioJavanCrawler
{
    class Program
    {
        private class Song
        {
            public Song(int id, string artist, string name, string lyrics, string path, string coverPath)
            {
                Id = id;
                Artist = artist ?? throw new ArgumentNullException(nameof(artist));
                Name = name ?? throw new ArgumentNullException(nameof(name));
                Lyrics = lyrics;
                Path = path;
                CoverPath = coverPath;
            }

            public int Id { get; }
            public string Artist { get; }
            public string Name { get; }
            public string Lyrics { get; }
            public string Path { get; }
            public string CoverPath { get; }
        }

        private static readonly List<Song> songs = new List<Song>();
        private static string[] urls = new string[]
            {
                "https://www.radiojavan.com/mp3s/mp3/Shadmehr-Aghili-Jange-Delam",
                "https://www.radiojavan.com/mp3s/mp3/Ali-Lohrasbi-Donyaye-Bi-To",
                "https://www.radiojavan.com/mp3s/mp3/Mohsen-Chavoshi-Sale-Bi-Bahar",
                "https://www.radiojavan.com/mp3s/mp3/Mohsen-Yeganeh-Behet-Ghol-Midam",
                "https://www.radiojavan.com/mp3s/mp3/Mohsen-Ebrahimzadeh-Taghche-Bala",
                "https://www.radiojavan.com/mp3s/mp3/Homayoun-Shajarian-Yek-Nafas-Arezouye-To",
                "https://www.radiojavan.com/mp3s/mp3/Hamed-Homayoun-Chatre-Khis",
                "https://www.radiojavan.com/mp3s/mp3/Shajarian-Baroon",
                "https://www.radiojavan.com/mp3s/mp3/Amirhossein-Eftekhari-Gerehe-Moo",
                "https://www.radiojavan.com/mp3s/mp3/Yas-Sefareshi",
                "https://www.radiojavan.com/mp3s/mp3/Anita-To-Faghat-Boro?playlist=0f450eb2f6ee&index=0",
                "https://www.radiojavan.com/mp3s/mp3/Shajarian-Jane-Jahan-(Tasnif)?playlist=6fc82bcc7292&index=0",
                "https://www.radiojavan.com/mp3s/mp3/Naser-Zeynali-Tavalod?playlist=04077936f4ff&index=0",
                "https://www.radiojavan.com/mp3s/mp3/Sirvan-Khosravi-Divoonegi?playlist=04d09dd296d2&index=0",
                "https://www.radiojavan.com/playlists/playlist/mp3/0acc4ffba36b",
                "https://www.radiojavan.com/mp3s/mp3/Puzzle-Roozaye-Behtar?playlist=1249011caf74&index=0",
                "https://www.radiojavan.com/mp3s/mp3/Shadmehr-Aghili-Mosafer?playlist=fd68aef8fe67&index=0",
                "https://www.radiojavan.com/mp3s/mp3/Sirvan-Khosravi-Ghabe-Akse-Khali?playlist=fb5953b84c68&index=0",
                "https://www.radiojavan.com/mp3s/mp3/Martik-Faghat-Yekbaar?playlist=43b324641a2a&index=0",
                "https://www.radiojavan.com/mp3s/mp3/Mehdi-Jahani-Mizane-Par?playlist=604c599a9433&index=0",
                "https://www.radiojavan.com/mp3s/mp3/Alireza-JJ-Balance-(Ft-Mehrad-Hidden-Sohrab-MJ-Sepehr-Khalse)?playlist=a418e671c0b3&index=0",
                "https://www.radiojavan.com/mp3s/mp3/Anita-To-Faghat-Boro?playlist=e367649f527f&index=0",
                "https://www.radiojavan.com/mp3s/mp3/Sirvan-Khosravi-Divoonegi?playlist=0cc48c1c5eba&index=0",
                "https://www.radiojavan.com/mp3s/mp3/X-Band-Tak-Khal-(Ft-Wink)?playlist=19054aeaf694&index=0",
                "https://www.radiojavan.com/mp3s/mp3/Ebi-Akharin-Bar?playlist=14af15307e15&index=0",
                "https://www.radiojavan.com/mp3s/mp3/Siavash-Ghomayshi-Alaki?playlist=0f365df7850c&index=0",
                "https://www.radiojavan.com/mp3s/mp3/Ebi-Geryeh-Nakon?playlist=359deb697042&index=0",
                "https://www.radiojavan.com/mp3s/mp3/Ebi-Gheseh-Eshgh?playlist=7efb8584b333&index=0",
                "https://www.radiojavan.com/mp3s/mp3/Googoosh-Pol?playlist=d648679ae626&index=0"
            };;

        [Obsolete]
        static void Main(string[] args)
        {
            var driver = new ChromeDriver(); ;
            driver.Navigate().GoToUrl(urls[0]);

            ExtractSongs(driver);

            var json = JsonSerializer.Serialize(songs, new JsonSerializerOptions() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });

            File.WriteAllText("songs.json", json);

            Console.WriteLine(songs.Last().Id);
        }

        private static void ExtractSongs(ChromeDriver driver)
        {
            int id = 1;
            Task.Delay(1000).Wait();

            Acceptookies(driver);

            foreach (var url in urls)
            {
                driver.Navigate().GoToUrl(url);
                for (int i = 0; i < 50; i++)
                {
                    id = ExtractSong(driver, id);
                }
            }
        }

        private static int ExtractSong(ChromeDriver driver, int id)
        {
            try
            {
                Task.Delay(1000).Wait();
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(1));
                IWebElement artistElement = driver.FindElement(By.CssSelector(".farsiText > .artist"));
                IWebElement songElement = driver.FindElement(By.CssSelector(".farsiText > .song"));
                var lyricsElemendButton = driver.FindElement(By.LinkText("Lyrics"));
                lyricsElemendButton.Click();
                string text = Getlyrics(driver);

                string src = GetCover(driver);

                var path = $"https://host2.rj-mw1.com/media/mp3/mp3-256/{driver.Url.Split("/").Last().Split("?")[0]}.mp3";

                Song item = new Song(id++, artistElement.Text, songElement.Text, text, path, src);

                if (!songs.Any(x => x.Name == item.Name && x.Artist == item.Artist))
                {
                    songs.Add(item);
                }

                Next(driver);

                Console.WriteLine(id);
            }
            catch
            {

            }

            return id;
        }

        private static string GetCover(ChromeDriver driver)
        {
            IWebElement coverElement = driver.FindElement(By.CssSelector(".artwork > img"));
            var src = coverElement.GetAttribute("src");
            return src;
        }

        private static string Getlyrics(ChromeDriver driver)
        {
            var lyricsElements = driver.FindElements(By.CssSelector(".lyricsFarsi"));
            if (lyricsElements.Any())
            {
                return lyricsElements[0].Text;
            }
            return null;
        }

        private static void Acceptookies(ChromeDriver driver)
        {
            var acceptookiesElemendButton = driver.FindElement(By.LinkText("Accept Cookies"));
            acceptookiesElemendButton.Click();
        }

        private static void Next(ChromeDriver driver)
        {
            var nextButtonElemet = driver.FindElement(By.CssSelector("#mp3_next"));
            nextButtonElemet.Click();
        }
    }
}
