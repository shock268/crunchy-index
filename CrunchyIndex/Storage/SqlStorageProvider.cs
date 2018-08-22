using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using CrunchyIndex.Models;

namespace CrunchyIndex.Storage
{
    public class SqlStorageProvider : IStorageProvider, IDisposable
    {
        private SqlConnection SqlConnection;

        public SqlStorageProvider()
        {
            Trace.TraceInformation("Connecting to local SQL...");
            this.SqlConnection = new SqlConnection("Server=localhost;Database=CrunchyIndex;Trusted_Connection=Yes");
            this.SqlConnection.Open();
        }

        public void WriteIndex(Index index)
        {
            foreach (var series in index.SeriesIdToSeriesMap.Values)
            {
                var command = new SqlCommand("sp_InsertSeries", this.SqlConnection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@SeriesId", series.Id));
                command.Parameters.Add(new SqlParameter("@Title", series.Title));
                command.Parameters.Add(new SqlParameter("@Description", series.Description));
                command.Parameters.Add(new SqlParameter("@Url", series.Url));
                command.Parameters.Add(new SqlParameter("@ImageUrl", series.ImageUrl));
                command.Parameters.Add(new SqlParameter("@Publisher", series.Publisher));
                command.Parameters.Add(new SqlParameter("@Year", series.Year));
                command.Parameters.Add(new SqlParameter("@WatchedEpisodeCount", series.WatchedEpisodeCount));
                command.Parameters.Add(new SqlParameter("@RatingCount", series.Rating.RatingCount));
                command.Parameters.Add(new SqlParameter("@AverageStars", series.Rating.AverageStars));
                command.Parameters.Add(new SqlParameter("@AverageRatingCalculated", series.Rating.AverageRatingCalculated));
                command.Parameters.Add(new SqlParameter("@LastUpdatedUtc", series.LastUpdatedUtc));

                try
                {
                    Trace.TraceInformation("[SQL] [WRITE] {0}", series.Id);
                    command.ExecuteNonQuery();
                }
                catch (SqlException e)
                {
                    if (!e.Message.Contains("Violation of PRIMARY KEY constraint"))
                    {
                        throw e;
                    }
                }
            }

            foreach (var genreId in index.GenreIdToSeriesIdsMap.Keys)          
            {
                foreach (var seriesId in index.GenreIdToSeriesIdsMap[genreId])
                {
                    // TODO: handle add or update
                    var command = new SqlCommand("sp_InsertGenre", this.SqlConnection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@SeriesId", seriesId));
                    command.Parameters.Add(new SqlParameter("@GenreId", genreId));

                    try
                    {
                        Trace.TraceInformation("[SQL] [WRITE] {0} {1}", seriesId, genreId);
                        command.ExecuteNonQuery();
                    }
                    catch (SqlException e)
                    {
                        if (!e.Message.Contains("Violation of PRIMARY KEY constraint"))
                        {
                            throw e;
                        }
                    }
                }
            }
        }


        public Index ReadIndex()
        {
            var index = new Index();
            
            var command = new SqlCommand("select * from Series");
            command.Connection = this.SqlConnection;
            Trace.TraceInformation("[SQL] {0}", command.CommandText);

            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var seriesId = (string)reader[0];
                index.SeriesIdToSeriesMap.Add(seriesId, new Series() { Id = seriesId });
                Trace.TraceInformation("[SQL] [READ] {0}", seriesId);
            }

            return index;
        }

        public void WriteUserActivity(UserActivity userActivity)
        {
            throw new NotImplementedException();
        }

        public UserActivity ReadUserActivity()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            this.SqlConnection.Close();
        }
    }
}
