using System;
using System.Collections.Generic;

namespace CrunchyIndex.Models
{
    public class Index
    {
        public Dictionary<string, Series> SeriesIdToSeriesMap { get; internal set; }
        public Dictionary<string, HashSet<string>> GenreIdToSeriesIdsMap { get; internal set; }

        public Index()
        {
            this.SeriesIdToSeriesMap = new Dictionary<string, Series>();
            this.GenreIdToSeriesIdsMap = new Dictionary<string, HashSet<string>>();

            var l = new List<string>();
            var i = new List<int>();
        }
    }
}
