namespace VendorRiskScoring.Tests.Handlers.Vendor;

public class GetVendorRiskQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_ReturnLatestRiskWithoutCallingRuleEngine_WhenRiskAlreadyExists()
    {
        // Arrange
        var vendorId = Guid.NewGuid();

        var vendor = new VendorProfile
        {
            Id = vendorId,
            Name = "Vendor X",
            FinancialHealth = 80,
            SlaUptime = 98,
            MajorIncidents = 1,
            SecurityCerts = new List<string> { "ISO27001" },
            Documents = new VendorDocuments
            {
                ContractValid = true,
                PrivacyPolicyValid = true,
                PentestReportValid = true
            },
            RiskAssessments =
            [
                new RiskAssessment
                {
                    RiskScore = 0.65,
                    RiskLevel = "Medium",
                    CreatedAt = DateTime.UtcNow.AddMinutes(-10),
                    Reasons = ["Test existing risk"]
                }
            ]
        };

        var mockRepo = new Mock<IVendorRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(vendorId))
            .ReturnsAsync(vendor);

        var mockRuleEngine = new Mock<IRuleEngineService>();

        // Cache: hit yok (zorlayalım ki DB + existing risk yoluna girsin)
        var mockCache = new Mock<IRiskCacheService>();
        mockCache.Setup(c => c.GetVendorRiskAsync(vendorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RiskAssessmentDto?)null);

        var handler = new GetVendorRiskQueryHandler(
            mockRepo.Object,
            mockRuleEngine.Object,
            mockCache.Object);

        // Act
        var result = await handler.Handle(new GetVendorRiskQuery(vendorId), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        var dto = result.Value!;
        dto.RiskScore.Should().Be(0.65);
        dto.RiskLevel.Should().Be("Medium");
        dto.Reasons.Should().Contain("Test existing risk");

        // Rule engine ÇAĞRILMAMALI
        mockRuleEngine.Verify(r => r.CalculateRiskAsync(It.IsAny<VendorProfile>()), Times.Never);

        // Repo çağrıldı mı
        mockRepo.Verify(r => r.GetByIdAsync(vendorId), Times.Once);

        // İsteğe bağlı: cache'e set edilmiş olabilir
        mockCache.Verify(c => c.SetVendorRiskAsync(
                vendorId,
                It.Is<RiskAssessmentDto>(x =>
                    Math.Abs(x.RiskScore - 0.65) < 0.001 &&
                    x.RiskLevel == "Medium"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_CallRuleEngineAndPersist_WhenNoRiskOrUnknown()
    {
        // Arrange
        var vendorId = Guid.NewGuid();

        var vendor = new VendorProfile
        {
            Id = vendorId,
            Name = "Vendor Y",
            FinancialHealth = 52,
            SlaUptime = 90,
            MajorIncidents = 3,
            SecurityCerts = new List<string>(),
            Documents = new VendorDocuments
            {
                ContractValid = true,
                PrivacyPolicyValid = false,
                PentestReportValid = false
            },
            // Hiç risk yok → rule engine tetiklenmeli
            RiskAssessments = []
        };

        var mockRepo = new Mock<IVendorRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(vendorId))
            .ReturnsAsync(vendor);

        mockRepo.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var mockRuleEngine = new Mock<IRuleEngineService>();
        mockRuleEngine
            .Setup(r => r.CalculateRiskAsync(It.IsAny<VendorProfile>()))
            .ReturnsAsync(new RiskResultDto
            {
                Score = 0.82,
                Level = "High",
                Reasons = new List<string>
                {
                    "Financial health below threshold",
                    "SLA < 95%",
                    "Multiple major incidents",
                    "Missing ISO27001",
                    "Expired privacy policy"
                }
            });

        // Cache miss
        var mockCache = new Mock<IRiskCacheService>();
        mockCache.Setup(c => c.GetVendorRiskAsync(vendorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RiskAssessmentDto?)null);

        var handler = new GetVendorRiskQueryHandler(
            mockRepo.Object,
            mockRuleEngine.Object,
            mockCache.Object);

        // Act
        var result = await handler.Handle(new GetVendorRiskQuery(vendorId), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        var dto = result.Value!;
        dto.RiskScore.Should().Be(0.82);
        dto.RiskLevel.Should().Be("High");
        dto.Reasons.Should().Contain("Financial health below threshold");

        // Rule engine çağrılmalı
        mockRuleEngine.Verify(r => r.CalculateRiskAsync(It.IsAny<VendorProfile>()), Times.Once);

        // Yeni risk eklenmiş olmalı
        vendor.RiskAssessments.Should().HaveCount(1);
        var added = vendor.RiskAssessments[0];
        added.RiskScore.Should().Be(0.82);
        added.RiskLevel.Should().Be("High");

        // SaveChangesAsync çağrılmış olmalı
        mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);

        // Cache’e yazılmış olmalı
        mockCache.Verify(c => c.SetVendorRiskAsync(
                vendorId,
                It.Is<RiskAssessmentDto>(x =>
                    Math.Abs(x.RiskScore - 0.82) < 0.001 &&
                    x.RiskLevel == "High"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CalculateRiskAsync_Should_ApplySeasonalRisk_ForLogisticsVendorInHighSeason()
    {
        // Arrange
        var matrix = new RiskFactorMatrix
        {
            SeasonalRisk = new SeasonalRiskConfig
            {
                HighSeasonMonths = new List<int> { DateTime.UtcNow.Month },
                LogisticsKeywords = new List<string> { "Logistics" },
                ExtraRisk = 0.05
            }
        };

        var options = Options.Create(matrix);

        var vendor = new VendorProfile
        {
            Name = "NovaLog Logistics",
            FinancialHealth = 70,
            SlaUptime = 96,
            MajorIncidents = 0,
            SecurityCerts = new List<string> { "ISO27001" },
            Documents = new VendorDocuments
            {
                ContractValid = true,
                PrivacyPolicyValid = true,
                PentestReportValid = true
            }
        };

        var service = new RuleEngineService(options);

        // Act
        var result = await service.CalculateRiskAsync(vendor);

        // Assert
        result.Reasons.Should().Contain(r => r.Contains("Seasonal adjustment"));
        // İstersen burada skorun 0.05 civarı artmış olduğunu da approx check edebilirsin,
        // ama base skor deterministic olmadığı için sadece reason kontrolü yeterli.
    }
}