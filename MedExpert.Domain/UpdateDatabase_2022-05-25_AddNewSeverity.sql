
ALTER TABLE AnalysisSymptom
ADD CombinedSubtreeSeverity decimal(18,3) null
GO


DROP INDEX [IX_AnalysisSymptom_SymptomId] ON [dbo].[AnalysisSymptom]
CREATE NONCLUSTERED INDEX [IX_AnalysisSymptom_SymptomId] ON [dbo].[AnalysisSymptom]
(
	[SymptomId] ASC
)
INCLUDE([AnalysisId],[Severity],[CombinedSubtreeSeverity]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO