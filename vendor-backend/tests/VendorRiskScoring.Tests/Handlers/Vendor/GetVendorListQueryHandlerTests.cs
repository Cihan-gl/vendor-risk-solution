namespace VendorRiskScoring.Tests.Handlers.Vendor;

public class GetVendorListQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_ReturnMappedVendorList()
    {
        // Arrange
        var vendors = new List<VendorProfile>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Vendor A",
                FinancialHealth = 85,
                SlaUptime = 99.9,
                MajorIncidents = 0,
                SecurityCerts = new List<string> { "ISO27001" },
                Documents = new VendorDocuments
                {
                    ContractValid = true,
                    PrivacyPolicyValid = true,
                    PentestReportValid = true
                },
                RiskAssessments =
                [
                    new()
                    {
                        RiskScore = 0.78,
                        RiskLevel = "Medium",
                        Reasons = new List<string> { "SLA düşük" }
                    }
                ]
            }
        };

        var mockRepo = new Mock<IVendorRepository>();

        mockRepo.Setup(r => r.GetAllQueryable())
            .Returns(vendors.AsQueryable());

        var handler = new GetVendorListQueryHandler(mockRepo.Object);

        // Act
        var result = await handler.Handle(
            new GetVendorListQuery(),
            CancellationToken.None);

        // Assert - Result objesini kontrol et
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        var paged = result.Value!;
        paged.Items.Should().HaveCount(1);

        var vendorDto = paged.Items[0];
        vendorDto.Id.Should().Be(vendors[0].Id);
        vendorDto.Name.Should().Be("Vendor A");
        vendorDto.FinancialHealth.Should().Be(85);
        vendorDto.SlaUptime.Should().Be(99.9);
        vendorDto.MajorIncidents.Should().Be(0);

        vendorDto.LatestRisk.Should().NotBeNull();
        vendorDto.LatestRisk!.RiskScore.Should().Be(0.78);
        vendorDto.LatestRisk.RiskLevel.Should().Be("Medium");
        vendorDto.LatestRisk.Reasons.Should().Contain("SLA düşük");

        mockRepo.Verify(r => r.GetAllQueryable(), Times.Once);
    }
}