USE CrunchyIndex

SELECT
  COUNT(*) AS SeriesCount,
  SUM(CASE WHEN WatchedEpisodeCount > 0 THEN 1 ELSE 0 END) AS WatchedSeries
FROM Series

SELECT 
  Title, 
  RatingCount, 
  AverageRatingCalculated AS Rating, 
  WatchedEpisodeCount AS Watched
FROM Series
WHERE RatingCount > 30
  AND WatchedEpisodeCount < 1
ORDER BY AverageRatingCalculated DESC