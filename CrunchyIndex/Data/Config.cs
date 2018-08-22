namespace CrunchyIndex.Data
{
    public static class Config
    {
        public struct Task
        {
            public struct UpdateUserActivity
            {
                public const int MaxPageCount = 1;
            }

            public struct AddSeriesToIndex
            {
                public const int MaxSeriesToAdd = 1000;
            }

            public struct UpdateSeriesToIndex
            {
                public const int MaxSeriesToUpdate = 1500;
                public const int MaxAllowedStaleDays = 30;
            }

            public struct AddGenresToIndex
            {
                public const int MaxPageCount = 100;
            }
        }

        public struct Storage
        {
            public const bool ReadFromStorage = true;
            public const bool WriteToStorage = true;
        }

        public struct Cookie
        {
            public const string UserId = "c_userid";
        }

        public struct Page
        {
            public struct Home
            {
                public const string Url = "https://www.crunchyroll.com";
                public const string ElementLogInXPath = "//li[@class='login']/a";
            }

            public struct LogIn
            {
                public const string Url = "https://www.crunchyroll.com/login";
                public const string ElementUserNameXPath = "//input[@name='login_form[name]']";
                public const string ElementPasswordXPath = "//input[@name='login_form[password]']";
                public const string ElementLogInLinkXPath = "//button[@type='submit']";
            }

            public struct History
            {
                public const string Url = "https://www.crunchyroll.com/home/history";
                public const string ElementLoadMoreLinkXPath = "//div[@id='main_content']/a";
                public const string ElementWatchedVideoLinksXPath = "//div[@id='main_content']//li/div/a";
            }

            public struct AnimeAlphabetical
            {
                public const string Url = "https://www.crunchyroll.com/videos/anime/alpha?group=all";
                public const string ElementSeriesXPath = "//div[@id='main_content']/div[@class='videos-column-container cf']//a";
            }

            public struct AnimeGenres
            {
                public const string Url = "https://www.crunchyroll.com/videos/anime/genres";
                public const string UrlFormat = "https://www.crunchyroll.com/videos/anime/genres/{0}?group=all";
                public const string ElementGenresXPath = "//div[@class='genre-selectors selectors']//input";
                public const string ElementSeriesXPath = "//div[@id='main_content']/ul//a";
            }

            public struct Series
            {
                public const string UrlFormat = "https://www.crunchyroll.com/{0}";
                public const string ElementTitleXPath = "//div[@id='container']/h1/span";
                public const string ElementDescriptionMoreButtonXPath = "//div[@class='right']//p[@itemprop='description']//a";
                public const string ElementDescriptionXPath = "//div[@class='right']//p[@itemprop='description']/span[@class='trunc-desc']";
                public const string ElementDescriptionExpandedXPath = "//div[@class='right']//p[@itemprop='description']/span[@class='more']";
                public const string ElementPublisherXPath = "//div[@class='right']//li/ul/li[contains(.,'Publisher')]";
                public const string ElementYearXPath = "//div[@class='right']//li/ul/li[contains(.,'Year:')]";
                public const string ElementImageXPath = "//img[@itemprop='image']";
                public const string ElementAvgRatingXPath = "//span[@itemprop='ratingValue']";
                public const string ElementStarReviewCountXPathFormat = "//li[@class='{0}-star cf']/div[3]";
            }
        }
    }
}
