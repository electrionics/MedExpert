using MedExpert.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedExpert.Domain
{
    public class MedExpertDataContext: DbContext
    {
        public MedExpertDataContext(DbContextOptions<MedExpertDataContext> options) : base(options)
        {
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Base
            
            modelBuilder.Entity<DeviationLevel>(entity =>
            {
                entity.HasKey(x => x.Id);
            });

            modelBuilder.Entity<Indicator>(entity =>
            {
                entity.HasKey(x => x.Id);
            });

            modelBuilder.Entity<Specialist>(entity =>
            {
                entity.HasKey(x => x.Id);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(x => x.Id);
            });

            #endregion

            #region Analysis
            
            modelBuilder.Entity<Analysis>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasOne(x => x.User)
                    .WithMany(x => x.Analyses)
                    .HasForeignKey(x => x.UserId);
            });
            
            modelBuilder.Entity<AnalysisIndicator>(entity =>
            {
                entity.HasKey(x => new { x.AnalysisId, x.IndicatorId });
                entity.HasOne(x => x.Analysis)
                    .WithMany(x => x.AnalysisIndicators)
                    .HasForeignKey(x => x.AnalysisId);
                entity.HasOne(x => x.Indicator)
                    .WithMany(x => x.AnalysisIndicators)
                    .HasForeignKey(x => x.IndicatorId);
                entity.HasOne(x => x.DeviationLevel)
                    .WithMany(x => x.AnalysisIndicators)
                    .HasForeignKey(x => x.DeviationLevelId);
            });
            
            modelBuilder.Entity<AnalysisDeviationLevel>(entity =>
            {
                entity.HasKey(x => new { x.AnalysisId, x.DeviationLevelId });
                entity.HasOne(x => x.Analysis)
                    .WithMany(x => x.AnalysisDeviationLevels)
                    .HasForeignKey(x => x.AnalysisId);
                entity.HasOne(x => x.DeviationLevel)
                    .WithMany(x => x.AnalysisDeviationLevels)
                    .HasForeignKey(x => x.DeviationLevelId);
            });

            #endregion

            #region Reference Interval
            
            modelBuilder.Entity<ReferenceIntervalApplyCriteria>(entity =>
            {
                entity.HasKey(x => x.Id);
            });
            
            modelBuilder.Entity<ReferenceIntervalValues>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasOne(x => x.Indicator)
                    .WithMany(x => x.ReferenceIntervalValues)
                    .HasForeignKey(x => x.IndicatorId);
                entity.HasOne(x => x.ApplyCriteria)
                    .WithMany(x => x.ReferenceIntervalValues)
                    .HasForeignKey(x => x.ApplyCriteriaId);
            });

            #endregion

            #region Symptom

            modelBuilder.Entity<Symptom>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasOne(x => x.Parent)
                    .WithMany(x => x.Children)
                    .HasForeignKey(x => x.ParentSymptomId)
                    .IsRequired(false);
            });

            modelBuilder.Entity<SymptomIndicatorDeviationLevel>(entity =>
            {
                entity.HasKey(x => new { x.SymptomId, x.IndicatorId });
                entity.HasOne(x => x.Symptom)
                    .WithMany(x => x.SymptomIndicatorDeviationLevels)
                    .HasForeignKey(x => x.SymptomId);
                entity.HasOne(x => x.Indicator)
                    .WithMany(x => x.SymptomIndicatorDeviationLevels)
                    .HasForeignKey(x => x.IndicatorId);
                entity.HasOne(x => x.DeviationLevel)
                    .WithMany(x => x.SymptomIndicatorDeviationLevels)
                    .HasForeignKey(x => x.DeviationLevelId);
            });
            
            #endregion
        }
    }
}