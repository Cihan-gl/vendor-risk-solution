namespace VendorRiskScoring.Infrastructure.Configurations;

public class VendorProfileConfiguration : IEntityTypeConfiguration<VendorProfile>
{
    public void Configure(EntityTypeBuilder<VendorProfile> builder)
    {
        builder.ToTable("Vendors");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(v => v.Name).IsUnique();

        builder.Property(v => v.FinancialHealth)
            .IsRequired();

        builder.Property(v => v.SlaUptime)
            .IsRequired();

        builder.Property(v => v.MajorIncidents)
            .IsRequired();

        // Value object: VendorDocuments'ı owned type olarak map ediyoruz
        builder.OwnsOne(v => v.Documents, nav =>
        {
            nav.Property(d => d.ContractValid)
                .HasColumnName("ContractValid")
                .IsRequired();

            nav.Property(d => d.PrivacyPolicyValid)
                .HasColumnName("PrivacyPolicyValid")
                .IsRequired();

            nav.Property(d => d.PentestReportValid)
                .HasColumnName("PentestReportValid")
                .IsRequired();
        });

        // İlişki: Vendor 1 - n RiskAssessment
        builder.HasMany(v => v.RiskAssessments)
            .WithOne(r => r.Vendor)
            .HasForeignKey(r => r.VendorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(v => v.CreatedAt)
            .IsRequired();
    }
}