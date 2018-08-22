using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security;
using CrunchyIndex.Data;
using CrunchyIndex.Models;
using OpenQA.Selenium;
using WebAutomation;

namespace CrunchyIndex
{
    public class TaskRunner
    {
        private WebAccessor WebAccessor;
        private readonly char[] ReviewCountTrimChars = new char[] { '(', ')' };

        public TaskRunner(WebAccessor webAccessor)
        {
            this.WebAccessor = webAccessor;
        }

        public bool LogIn()
        {
            this.WebAccessor.NavigateTo(Config.Page.LogIn.Url);
            this.WebAccessor.WaitFor(() => this.WebAccessor.ElementExists(Config.Page.LogIn.ElementUserNameXPath), "redirect to login");

            Console.Write("username:  ");
            var username = Console.ReadLine();

            Console.Write("password:  ");
            // TODO: don't downgrade from SecureString to String
            using (var password = ReadSecureStringFromConsole())
            {
                var credential = new NetworkCredential(username, password);
                this.WebAccessor.Type(credential.UserName, Config.Page.LogIn.ElementUserNameXPath);
                this.WebAccessor.Type(credential.Password, Config.Page.LogIn.ElementPasswordXPath, true);
            }

            this.WebAccessor.Click(Config.Page.LogIn.ElementLogInLinkXPath);
            var userIdCookie = this.WebAccessor.Driver.Manage().Cookies.GetCookieNamed(Config.Cookie.UserId);
            return userIdCookie != null;
        }

        private SecureString ReadSecureStringFromConsole()
        {
            var secureString = new SecureString();
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);

                // ignore keys out of range
                if ((key.Key >= ConsoleKey.A && key.Key <= ConsoleKey.Z) ||
                    (key.Key >= ConsoleKey.D0 && key.Key <= ConsoleKey.D9))
                {
                    secureString.AppendChar(key.KeyChar);
                    Console.Write("*");
                }
            } while (key.Key != ConsoleKey.Enter);

            Console.WriteLine();
            return secureString;
        }

        public void UpdateUserActivity(UserActivity userActivity)
        {
            var recentlyWatchedEpisodeUrls = new HashSet<string>();
            this.WebAccessor.NavigateTo(Config.Page.History.Url);
            const string loadMoreXPath = Config.Page.History.ElementLoadMoreLinkXPath;
            int pageCount = 0;
            do
            {
                BotDetectionMitigation.RandomizedWait();
                Trace.TraceInformation("[UpdateUserActivity] page count = {0}", ++pageCount);
            }
            while (pageCount < Config.Task.UpdateUserActivity.MaxPageCount && this.WebAccessor.ElementExists(loadMoreXPath) && this.WebAccessor.Click(loadMoreXPath));

            var watchedVideoLinks = this.WebAccessor.Driver.FindElementsByXPath(Config.Page.History.ElementWatchedVideoLinksXPath);
            for (int i = 0; i < watchedVideoLinks.Count; i++)
            {
                var watchedVideoUrl = watchedVideoLinks[i].GetAttribute("href");
                Trace.TraceInformation("[UpdateUserActivity] processing {0} of {1} [{2}]", i + 1, watchedVideoLinks.Count, watchedVideoUrl);
                recentlyWatchedEpisodeUrls.Add(watchedVideoUrl);
            }

            userActivity.AddWatchedEpisodesMostRecentFirst(recentlyWatchedEpisodeUrls);
        }

        public void AddUserActivityToIndex(UserActivity userActivity, Index index)
        {
            foreach (var seriesId in userActivity.WatchedEpisodeCountPerSeries.Keys)
            {
                if (!index.SeriesIdToSeriesMap.ContainsKey(seriesId))
                {
                    index.SeriesIdToSeriesMap[seriesId] = this.GetSeries(seriesId);
                }

                index.SeriesIdToSeriesMap[seriesId].WatchedEpisodeCount = userActivity.WatchedEpisodeCountPerSeries[seriesId];
            }

            var seriesIdsSortedByWatchedEpisodeCountDescending = userActivity.WatchedEpisodeCountPerSeries.Keys
                .OrderBy(s => s)
                .OrderByDescending(s => userActivity.WatchedEpisodeCountPerSeries[s]);

            foreach (var seriesId in seriesIdsSortedByWatchedEpisodeCountDescending)
            {
                Trace.TraceInformation("[AddUserActivityToIndex] [{0}] watched {1}", seriesId, userActivity.WatchedEpisodeCountPerSeries[seriesId]);
            }

            Trace.TraceInformation("[AddUserActivityToIndex] watched {0} episodes across {1} series", userActivity.WatchedEpisodeUrls.Count, index.SeriesIdToSeriesMap.Keys.Count());
        }

        public void AddGenresToIndex(Index index)
        {
            this.WebAccessor.NavigateTo(Config.Page.AnimeGenres.Url);

            var genreIds = new HashSet<string>();
            var genreElements = this.WebAccessor.Driver.FindElementsByXPath(Config.Page.AnimeGenres.ElementGenresXPath);
            foreach (var genreElement in genreElements)
            {
                genreIds.Add(genreElement.GetAttribute("value").Replace(' ', '_'));
            }

            foreach (var genreId in genreIds)
            {
                var seriesIds = new HashSet<string>();

                this.WebAccessor.NavigateTo(string.Format(Config.Page.AnimeGenres.UrlFormat, genreId));
                
                var seriesLinks = this.WebAccessor.Driver.FindElementsByXPath(Config.Page.AnimeGenres.ElementSeriesXPath);
                for (int i = 0; i < seriesLinks.Count; i++)
                {
                    var seriesId = Series.GetIdFromUrl(seriesLinks[i].GetAttribute("href"));
                    Trace.TraceInformation("[AddGenresToIndex] [{0}] processing {1} of {2} [{3}]", genreId, i + 1, seriesLinks.Count, seriesId);
                    seriesIds.Add(seriesId);
                }

                if (!index.GenreIdToSeriesIdsMap.ContainsKey(genreId))
                {
                    index.GenreIdToSeriesIdsMap[genreId] = seriesIds;
                }
                else
                {
                    foreach (var seriesId in seriesIds)
                    {
                        index.GenreIdToSeriesIdsMap[genreId].Add(seriesId);
                    }
                }
            }

            var genreIdsSortedBySeriesCountDescending = index.GenreIdToSeriesIdsMap.Keys.OrderByDescending(g => index.GenreIdToSeriesIdsMap[g].Count);
            foreach (var genreId in genreIdsSortedBySeriesCountDescending)
            {
                Trace.TraceInformation("[AddGenresToIndex] [{0}] has {1} series", genreId, index.GenreIdToSeriesIdsMap[genreId].Count);
            }
        }

        public int AddSeriesToIndex(Index index)
        {
            if (Config.Task.AddSeriesToIndex.MaxSeriesToAdd <= 0)
            {
                return 0;
            }

            this.WebAccessor.NavigateTo(Config.Page.AnimeAlphabetical.Url);
            var seriesLinkElements = this.WebAccessor.Driver.FindElementsByXPath(Config.Page.AnimeAlphabetical.ElementSeriesXPath);

            Trace.TraceInformation("[AddSeriesToIndex] found {0} series, index has {1}", seriesLinkElements.Count, index.SeriesIdToSeriesMap.Count);

            var seriesIdsToAdd = new HashSet<string>();
            int seriesToAddCount = 0;
            for (int i = 0; i < seriesLinkElements.Count && seriesToAddCount < Config.Task.AddSeriesToIndex.MaxSeriesToAdd; i++)
            {
                string seriesId = Series.GetIdFromUrl(seriesLinkElements[i].GetAttribute("href"));
                if (!index.SeriesIdToSeriesMap.ContainsKey(seriesId))
                {
                    seriesIdsToAdd.Add(seriesId);
                    seriesToAddCount++;
                }
            }

            foreach (var seriesId in seriesIdsToAdd)
            {
                index.SeriesIdToSeriesMap.Add(seriesId, this.GetSeries(seriesId));
            }

            Trace.TraceInformation("[AddSeriesToIndex] added {0} series", seriesToAddCount);
            return seriesToAddCount;
        }

        private Series GetSeries(string seriesId)
        {
            Trace.TraceInformation("[GetSeries] {0}", seriesId);

            var seriesUrl = string.Format(Config.Page.Series.UrlFormat, seriesId);
            this.WebAccessor.NavigateTo(seriesUrl);

            // TODO: handle missing (e.g. removed) series
            var series = new Series();

            series.Id = seriesId; 
            series.LastUpdatedUtc = DateTime.UtcNow;
            series.Url = seriesUrl;
            series.Title = this.WebAccessor.Driver.FindElementByXPath(Config.Page.Series.ElementTitleXPath).Text;
            series.ImageUrl = this.WebAccessor.Driver.FindElementByXPath(Config.Page.Series.ElementImageXPath).GetAttribute("src");
            series.Publisher = this.WebAccessor.Driver.FindElementByXPath(Config.Page.Series.ElementPublisherXPath).Text.Split(' ')[1];
            
            // if description is long enough, we need to click the "more" button and get value from a different XPath
            try
            {
                this.WebAccessor.Click(Config.Page.Series.ElementDescriptionMoreButtonXPath);
                series.Description = this.WebAccessor.Driver.FindElementByXPath(Config.Page.Series.ElementDescriptionExpandedXPath).Text;
            }
            catch (NoSuchElementException)
            {
                series.Description = this.WebAccessor.Driver.FindElementByXPath(Config.Page.Series.ElementDescriptionXPath).Text;
            }

            // optional
            try
            {
                series.Year = Int32.Parse(this.WebAccessor.Driver.FindElementByXPath(Config.Page.Series.ElementYearXPath).Text.Split(' ')[1]);
            }
            catch (NoSuchElementException) { }
            
            series.Rating = new Rating();
            series.Rating.AverageStars = Decimal.Parse(this.WebAccessor.Driver.FindElementByXPath(Config.Page.Series.ElementAvgRatingXPath).GetAttribute("content"));
            
            series.Rating.FiveStarReviewCount = this.GetStarReviewCount(5);
            series.Rating.FourStarReviewCount = this.GetStarReviewCount(4);
            series.Rating.ThreeStarReviewCount = this.GetStarReviewCount(3);
            series.Rating.TwoStarReviewCount = this.GetStarReviewCount(2);
            series.Rating.OneStarReviewCount = this.GetStarReviewCount(1);

            series.Rating.RatingCount =
                series.Rating.FiveStarReviewCount + 
                series.Rating.FourStarReviewCount + 
                series.Rating.ThreeStarReviewCount + 
                series.Rating.TwoStarReviewCount + 
                series.Rating.OneStarReviewCount;

            if (series.Rating.RatingCount > 0) {
                series.Rating.AverageRatingCalculated =
                    Math.Round(
                        (decimal)(5 * series.Rating.FiveStarReviewCount +
                        4 * series.Rating.FourStarReviewCount +
                        3 * series.Rating.ThreeStarReviewCount +
                        2 * series.Rating.TwoStarReviewCount +
                        series.Rating.OneStarReviewCount)
                        / series.Rating.RatingCount,
                    2);
            }

            return series;
        }

        private int GetStarReviewCount(int stars)
        {
            int starReviewCount;
            string starReviewCountXPath = String.Format(Config.Page.Series.ElementStarReviewCountXPathFormat, stars);
            string starReviewCountText = this.WebAccessor.Driver.FindElementByXPath(starReviewCountXPath).Text.Trim(ReviewCountTrimChars);
            // text is string.Empty if series has no reviews
            Int32.TryParse(starReviewCountText, out starReviewCount);
            return starReviewCount;
        }

        public int UpdatesSeriesToIndex(Index index)
        {
            if (Config.Task.UpdateSeriesToIndex.MaxSeriesToUpdate <= 0)
            {
                return 0;
            }

            this.WebAccessor.NavigateTo(Config.Page.AnimeAlphabetical.Url);
            var seriesLinkElements = this.WebAccessor.Driver.FindElementsByXPath(Config.Page.AnimeAlphabetical.ElementSeriesXPath);

            Trace.TraceInformation("[UpdatesSeriesToIndex] found {0} series, index has {1}", seriesLinkElements.Count, index.SeriesIdToSeriesMap.Count);

            var seriesIdsToUpdate = new HashSet<string>();
            int seriesToUpdateCount = 0;
            for (int i = 0; i < seriesLinkElements.Count && seriesToUpdateCount < Config.Task.UpdateSeriesToIndex.MaxSeriesToUpdate; i++)
            {
                string seriesId = Series.GetIdFromUrl(seriesLinkElements[i].GetAttribute("href"));
                if (index.SeriesIdToSeriesMap.ContainsKey(seriesId) &&
                    (DateTime.UtcNow - index.SeriesIdToSeriesMap[seriesId].LastUpdatedUtc).Days > Config.Task.UpdateSeriesToIndex.MaxAllowedStaleDays)
                {
                    seriesIdsToUpdate.Add(seriesId);
                    seriesToUpdateCount++;
                }
            }

            foreach (var seriesId in seriesIdsToUpdate)
            {
                // update index from web, but preserve any existing user activity
                var watchedEpisodeCount = index.SeriesIdToSeriesMap[seriesId].WatchedEpisodeCount;
                index.SeriesIdToSeriesMap[seriesId] = this.GetSeries(seriesId);
                index.SeriesIdToSeriesMap[seriesId].WatchedEpisodeCount = watchedEpisodeCount;
            }

            Trace.TraceInformation("[AddSeriesToIndex] added {0} series", seriesToUpdateCount);
            return seriesToUpdateCount;
        }
    }
}
