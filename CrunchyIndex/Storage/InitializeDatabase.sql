IF DB_ID('CrunchyIndex') is null
BEGIN
	CREATE DATABASE CrunchyIndex;
END

USE CrunchyIndex;

IF OBJECT_ID('Series') IS NOT NULL 
BEGIN
	DROP TABLE Series;
END

CREATE TABLE Series
(
	SeriesId NVARCHAR(450) NOT NULL,
	Title NVARCHAR(1000),
	Description NVARCHAR(MAX),
	Url NVARCHAR(1000),
	ImageUrl NVARCHAR(1000),
	Publisher NVARCHAR(1000),
	Year INT,
	WatchedEpisodeCount INT,
	RatingCount INT,
	AverageStars DECIMAL(18,2),
	AverageRatingCalculated DECIMAL(18,2),
	LastUpdatedUtc DATETIME,

	PRIMARY KEY (SeriesId),
);

IF OBJECT_ID('sp_InsertSeries') IS NOT NULL
BEGIN
	DROP PROCEDURE sp_InsertSeries
END

-- TODO: refactor stored procedures into separate files
GO
CREATE PROCEDURE sp_InsertSeries
	@SeriesId NVARCHAR(450),
	@Title NVARCHAR(1000),
	@Description NVARCHAR(MAX),
	@Url NVARCHAR(1000),
	@ImageUrl NVARCHAR(1000),
	@Publisher NVARCHAR(1000),
	@Year INT,
	@WatchedEpisodeCount INT,
	@RatingCount INT,
	@AverageStars DECIMAL(18,2),
	@AverageRatingCalculated DECIMAL(18,2),
	@LastUpdatedUtc DATETIME
AS

	BEGIN TRANSACTION

	IF EXISTS
	(
		SELECT *
		FROM Series WITH (UPDLOCK, SERIALIZABLE)
		WHERE SeriesId = @SeriesId
	)
	BEGIN
		UPDATE Series SET
			Title = @Title,
			Description = @Description,
			Url = @Url,
			ImageUrl = @ImageUrl,
			Publisher = @Publisher,
			Year = @Year,
			WatchedEpisodeCount = @WatchedEpisodeCount,
			RatingCount = @RatingCount,
			AverageStars = @AverageStars,
			AverageRatingCalculated = @AverageRatingCalculated,
			LastUpdatedUtc = @LastUpdatedUtc
		WHERE SeriesId = @SeriesId
	END
	ELSE
	BEGIN
		INSERT INTO Series VALUES
		(
			@SeriesId,
			@Title,
			@Description,
			@Url,
			@ImageUrl,
			@Publisher,
			@Year,
			@WatchedEpisodeCount,
			@RatingCount,
			@AverageStars,
			@AverageRatingCalculated,
			@LastUpdatedUtc
		);
	END

	COMMIT TRANSACTION

GO

SELECT *
FROM Series;