using System.Linq;
using System.Collections.Generic;

namespace CrunchyIndex.Models
{
    public class UserActivity
    {
        public LinkedList<string> WatchedEpisodeUrlsMostRecentFirst { get; private set; }
        public HashSet<string> WatchedEpisodeUrls { get; private set; }
        public Dictionary<string, int> WatchedEpisodeCountPerSeries { get; private set; }

        public UserActivity()
        {
            this.WatchedEpisodeUrlsMostRecentFirst = new LinkedList<string>();
            this.WatchedEpisodeUrls = new HashSet<string>();
            this.WatchedEpisodeCountPerSeries = new Dictionary<string, int>();
        }

        public void AddWatchedEpisodesMostRecentFirst(IEnumerable<string> watchedEpisodeUrls)
        {
            foreach (var watchedEpisodeUrl in watchedEpisodeUrls.Reverse())
            {
                if (WatchedEpisodeUrls.Contains(watchedEpisodeUrl))
                {
                    WatchedEpisodeUrlsMostRecentFirst.Remove(watchedEpisodeUrl);
                    WatchedEpisodeUrlsMostRecentFirst.AddFirst(watchedEpisodeUrl);
                }
                else
                {
                    WatchedEpisodeUrls.Add(watchedEpisodeUrl);
                    WatchedEpisodeUrlsMostRecentFirst.AddFirst(watchedEpisodeUrl);
                }
            }

            this.CalculateWatchedEpisodeCountPerSeries();
        }

        private void CalculateWatchedEpisodeCountPerSeries()
        {
            this.WatchedEpisodeCountPerSeries = new Dictionary<string, int>();
            foreach (var watchedEpisodeUrl in this.WatchedEpisodeUrls)
            {
                string seriesId = Series.GetIdFromUrl(watchedEpisodeUrl);

                if (this.WatchedEpisodeCountPerSeries.ContainsKey(seriesId))
                {
                    this.WatchedEpisodeCountPerSeries[seriesId]++;
                }
                else
                {
                    this.WatchedEpisodeCountPerSeries[seriesId] = 1;
                }
            }
        }
    }
}
