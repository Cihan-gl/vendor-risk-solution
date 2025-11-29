namespace VendorRiskScoring.Infrastructure.Configurations;

public class RiskAssessmentConfiguration : IEntityTypeConfiguration<RiskAssessment>
{
    public void Configure(EntityTypeBuilder<RiskAssessment> builder)
    {
        builder.ToTable("RiskAssessments");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.RiskScore)
            .IsRequired();

        builder.Property(r => r.RiskLevel)
            .IsRequired()
            .HasMaxLength(20);
    }
}