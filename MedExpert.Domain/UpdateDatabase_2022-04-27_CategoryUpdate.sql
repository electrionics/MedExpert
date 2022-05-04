CREATE TABLE [dbo].[AnalysisSymptomIndicator](
    [AnalysisId] [int] NOT NULL,
    [SymptomId] [int] NOT NULL,
    [IndicatorId] [int] NOT NULL,
     CONSTRAINT [PK_AnalysisSymptomIndicator] PRIMARY KEY CLUSTERED
    (
    [AnalysisId] ASC,
    [SymptomId] ASC,
    [IndicatorId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY]
    GO

ALTER TABLE [dbo].[AnalysisSymptomIndicator]  WITH CHECK ADD  CONSTRAINT [FK_AnalysisSymptomIndicator_Analysis] FOREIGN KEY([AnalysisId])
    REFERENCES [dbo].[Analysis] ([Id])
    GO

ALTER TABLE [dbo].[AnalysisSymptomIndicator]  WITH CHECK ADD  CONSTRAINT [FK_AnalysisSymptomIndicator_Indicator] FOREIGN KEY([IndicatorId])
    REFERENCES [dbo].[Indicator] ([Id])
    GO

ALTER TABLE [dbo].[AnalysisSymptomIndicator]  WITH CHECK ADD  CONSTRAINT [FK_AnalysisSymptomIndicator_Symptom] FOREIGN KEY([SymptomId])
    REFERENCES [dbo].[Symptom] ([Id])
    GO


ALTER TABLE Analysis
ADD Calculated bit NOT NULL DEFAULT(0)
GO 


ALTER TABLE AnalysisSymptom
ALTER COLUMN Expressiveness decimal(18,3) NULL
GO


CREATE TABLE [dbo].[Lookup](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [Name] [nvarchar](100) NOT NULL,
    [Value] [nvarchar](100) NOT NULL,
    CONSTRAINT [PK_Lookup] PRIMARY KEY CLUSTERED
(
    [Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
    CONSTRAINT [UK_Lookup_Name] UNIQUE NONCLUSTERED
(
    [Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY]
GO


INSERT INTO Lookup (Name, Value) VALUES ('CommonAnalysisSpecialist', '1')
INSERT INTO Lookup (Name, Value) VALUES ('CommonTreatmentSpecialist', '1')
GO

UPDATE SymptomCategory SET DisplayName = N'Болезни', Name = 'Illness' where Id = 1
UPDATE SymptomCategory SET DisplayName = N'Лечение', Name = 'Treatment' where Id = 2
DELETE FROM SymptomCategory WHERE Id = 3
GO

UPDATE Indicator SET InAnalysis = 1 WHERE ShortName IN ('N', 'L', 'M', 'E', 'PLR', 'NLR', 'MLR', 'LMR', 'ELR', 'SII')
GO