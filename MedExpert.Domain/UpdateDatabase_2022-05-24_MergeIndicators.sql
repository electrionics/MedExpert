declare @HBWrongId int
SELECT @HBWrongId = Id from Indicator where ShortName = 'Hb'
declare @HBCorrectId int
SELECT @HBCorrectId = Id from Indicator where ShortName = 'HB'
--SELECT @HBWrongId, @HBCorrectId, * from Indicator where Id in (@HBWrongId, @HBCorrectId)


UPDATE AnalysisIndicator SET IndicatorId = @HBCorrectId WHERE IndicatorId = @HBWrongId
UPDATE AnalysisSymptomIndicator SET IndicatorId = @HBCorrectId WHERE IndicatorId = @HBWrongId
UPDATE Indicator SET InAnalysis = 1, Sort = 1, Name =N'Гемоглобин' WHERE Id = @HBCorrectId
DELETE FROM Indicator WHERE Id = @HBWrongId