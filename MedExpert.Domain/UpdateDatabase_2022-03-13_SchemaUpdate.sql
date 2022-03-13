ALTER TABLE Analysis ADD COLUMN Date datetime NOT NULL 
GO

CREATE TABLE [dbo].[AnalysisSymptom](
    [AnalysisId] [int] NOT NULL,
    [SymptomId] [int] NOT NULL,
    [Match] [decimal](18, 3) NOT NULL,
    [Unmatch] [decimal](18, 3) NOT NULL,
    [Expressiveness] [decimal](18, 3) NOT NULL,
    CONSTRAINT [PK_AnalysisSymptom] PRIMARY KEY CLUSTERED
(
    [AnalysisId] ASC,
[SymptomId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
    CONSTRAINT [IX_AnalysisSymptom] UNIQUE NONCLUSTERED
(
    [SymptomId] ASC,
[AnalysisId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY]
GO

ALTER TABLE [dbo].[AnalysisSymptom]  WITH CHECK ADD  CONSTRAINT [FK_AnalysisSymptom_Analysis] FOREIGN KEY([AnalysisId])
    REFERENCES [dbo].[Analysis] ([Id])
GO


ALTER TABLE [dbo].[AnalysisSymptom]  WITH CHECK ADD  CONSTRAINT [FK_AnalysisSymptom_Symptom] FOREIGN KEY([SymptomId])
    REFERENCES [dbo].[Symptom] ([Id])
GO

ALTER TABLE Indicator ALTER COLUMN ShortName nvarchar(50) NOT NULL COLLATE SQL_Latin1_General_CP1_CS_AS
GO 

ALTER TABLE Specialist ADD COLUMN ApplyToSexOnly int NULL 
GO

ALTER TABLE Symptom ADD COLUMN ApplyToSexOnly int NULL 
GO

ALTER TABLE Symptom ADD COLUMN CategoryId int NOT NULL DEFAULT(1)
GO

CREATE TABLE [dbo].[SymptomCategory](
    [Id] [int] NOT NULL,
    [Name] [nvarchar](100) NOT NULL,
    [DisplayName] [nvarchar](100) NOT NULL,
    CONSTRAINT [PK_SymptomCategory] PRIMARY KEY CLUSTERED
(
[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY]
GO
    	
INSERT INTO SymptomCategory (Id, Name, DisplayName) VALUES (1, 'Ilness', 'Болезни')
INSERT INTO SymptomCategory (Id, Name, DisplayName) VALUES (2, 'RecommendedPreparation', 'Рекомендуемые препараты')
INSERT INTO SymptomCategory (Id, Name, DisplayName) VALUES (3, 'ToxicPreparation', 'Противопоказанные препараты')
GO    

ALTER TABLE [dbo].[Symptom]  WITH CHECK ADD  CONSTRAINT [FK_Symptom_SymptomCategory] FOREIGN KEY([CategoryId])
    REFERENCES [dbo].[SymptomCategory] ([Id])
GO

ALTER TABLE Symptom ALTER COLUMN [Name] [nvarchar](300) NOT NULL
GO