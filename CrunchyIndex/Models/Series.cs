using System;

namespace CrunchyIndex.Models
{
    public class Series
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string ImageUrl { get; set; }
        public string Publisher { get; set; }
        public int Year { get; set; }

        public int WatchedEpisodeCount { get; set; }
        public Rating Rating { get; set; }
        public DateTime LastUpdatedUtc { get; set; }

        public static string GetIdFromUrl(string url)
        {
            Uri episodeUri;
            Uri.TryCreate(url, UriKind.Absolute, out episodeUri);
            var splitString = episodeUri.LocalPath.Split('/');
            string seriesId = splitString[1];
            //string episodeId = splitString[2];

            return seriesId;
        }
    }
}
