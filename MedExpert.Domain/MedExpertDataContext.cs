using Microsoft.EntityFrameworkCore;

using MedExpert.Domain.Entities;

namespace MedExpert.Domain
{
    public class MedExpertDataContext: DbContext
    {
        public MedExpertDataContext(DbContextOptions<MedExpertDataContext> options) : base(options)
        {
        }
        
        public MedExpertDataContext()
        {
        }
        
        private static readonly OptionRecompileInterceptor Interceptor = new();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.AddInterceptors(Interceptor);
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Base
            
            modelBuilder.Entity<DeviationLevel>(entity =>
            {
                entity.HasKey(x => x.Id);
                
                entity.HasIndex(index => new {index.Alias},
                    "UK_DeviationLevel_Alias");
            });

            modelBuilder.Entity<Indicator>(entity =>
            {
                entity.HasKey(x => x.Id);
                
                entity.HasIndex(index => new {index.InAnalysis},
                    "IX_Indicator");
                entity.HasIndex(index => new {index.ShortName}, "UK_Indicator_ShortName");
            });

            modelBuilder.Entity<Specialist>(entity =>
            {
                entity.HasKey(x => x.Id);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(x => x.Id);
            });

            modelBuilder.Entity<Lookup>(entity =>
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
                
                entity.HasIndex(index => new {index.AnalysisId}, "IX_AnalysisIndicator");
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
            
            modelBuilder.Entity<AnalysisSymptom>(entity =>
            {
                entity.HasKey(x => new { x.AnalysisId, x.SymptomId });
                entity.HasOne(x => x.Analysis)
                    .WithMany(x => x.AnalysisSymptoms)
                    .HasForeignKey(x => x.AnalysisId);
                entity.HasOne(x => x.Symptom)
                    .WithMany(x => x.AnalysisSymptoms)
                    .HasForeignKey(x => x.SymptomId);
                entity.Ignore(x => x.MatchedIndicatorIds);
                
                entity.HasIndex(index => new {index.AnalysisId, index.SymptomId}, "IX_AnalysisSymptom");
                entity.HasIndex(index => new {index.SymptomId}, "IX_AnalysisSymptom_SymptomId");
            });
            
            modelBuilder.Entity<AnalysisSymptomIndicator>(entity =>
            {
                entity.HasKey(x => new { x.AnalysisId, x.SymptomId, x.IndicatorId });
                entity.HasOne(x => x.Analysis)
                    .WithMany(x => x.AnalysisSymptomIndicators)
                    .HasForeignKey(x => x.AnalysisId);
                entity.HasOne(x => x.Symptom)
                    .WithMany(x => x.AnalysisSymptomIndicators)
                    .HasForeignKey(x => x.SymptomId);
                entity.HasOne(x => x.Indicator)
                    .WithMany(x => x.AnalysisSymptomIndicators)
                    .HasForeignKey(x => x.IndicatorId);
                
                entity.HasIndex(index => new {index.SymptomId}, "IX_AnalysisSymptomIndicator_SymptomId");
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

            modelBuilder.Entity<SymptomCategory>(entity =>
            {
                entity.HasKey(x => x.Id);
            });
            
            modelBuilder.Entity<Symptom>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasOne(x => x.Parent)
                    .WithMany(x => x.Children)
                    .HasForeignKey(x => x.ParentSymptomId)
                    .IsRequired(false);
                entity.HasOne(x => x.Category)
                    .WithMany(x => x.Symptoms)
                    .HasForeignKey(x => x.CategoryId);
                
                entity.HasIndex(index => new {index.SpecialistId, index.ApplyToSexOnly, index.IsDeleted},
                    "IX_Symptom");
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

                entity.HasIndex(index => new {index.SymptomId, index.IndicatorId},
                    "IX_SymptomIndicatorDeviationLevel_SymptomIdIndicatorId");
            });
            
            #endregion
        }
    }
}