Update Indicator SET Name = N'Эозинофилы', Sort = 17 where ShortName = 'E%'
update Indicator set Sort = 33 where ShortName = 'PNR'
update Indicator set Sort = 34 where ShortName = 'BLR'
update Indicator set Sort = 38 where ShortName = 'MLR'
delete from AnalysisIndicator where IndicatorId in
(
    select Id from Indicator where ShortName in ('MON','BAS','NEU','LYM','EOS')
)
delete from Indicator where ShortName in ('MON','BAS','NEU','LYM','EOS')


delete from ReferenceIntervalValues where IndicatorId not in
(
    select Id from Indicator where InAnalysis = 1 and FormulaType is not null
)