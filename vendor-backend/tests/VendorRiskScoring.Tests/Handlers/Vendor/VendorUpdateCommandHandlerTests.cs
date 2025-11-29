namespace VendorRiskScoring.Tests.Handlers.Vendor;

public class VendorUpdateCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_UpdateVendor_And_RecalculateRisk()
    {
        // Arrange
        var vendorId = Guid.NewGuid();

        var existingVendor = new VendorProfile
        {
            Id = vendorId,
            Name = "Old Vendor",
            FinancialHealth = 70,
            SlaUptime = 99.5,
            MajorIncidents = 2,
            SecurityCerts = new List<string> { "SOC2" },
            Documents = new VendorDocuments
            {
                ContractValid = false,
                PrivacyPolicyValid = false,
                PentestReportValid = false
            },
            RiskAssessments = new List<RiskAssessment>()
        };

        var command = new VendorUpdateCommand
        {
            Id = vendorId,
            Name = "Updated Vendor",
            FinancialHealth = 90,
            SlaUptime = 99.99,
            MajorIncidents = 0,
            SecurityCerts = new List<string> { "ISO27001" },
            Documents = new VendorDocumentsDto
            {
                ContractValid = true,
                PrivacyPolicyValid = true,
                PentestReportValid = true
            }
        };

        var mockRepo = new Mock<IVendorRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(vendorId))
            .ReturnsAsync(existingVendor);
        mockRepo.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var mockRuleEngine = new Mock<IRuleEngineService>();
        mockRuleEngine.Setup(s => s.CalculateRiskAsync(It.IsAny<VendorProfile>()))
            .ReturnsAsync(new RiskResultDto
            {
                Score = 0.10, // 0-1 arası; düşük risk
                Level = "Low",
                Reasons = new List<string> { "Çok iyi SLA", "ISO sertifikası" }
            });

        var mockCache = new Mock<IRiskCacheService>();
        mockCache.Setup(c => c.RemoveVendorRiskAsync(vendorId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new VendorUpdateCommandHandler(
            mockRepo.Object,
            mockRuleEngine.Object,
            mockCache.Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert - Result wrapper
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        result.Errors.Should().BeEmpty();

        // Entity üstündeki değişiklikler
        existingVendor.Name.Should().Be(command.Name);
        existingVendor.FinancialHealth.Should().Be(command.FinancialHealth);
        existingVendor.SlaUptime.Should().Be(command.SlaUptime);
        existingVendor.MajorIncidents.Should().Be(command.MajorIncidents);
        existingVendor.SecurityCerts.Should().BeEquivalentTo(command.SecurityCerts);

        existingVendor.Documents.ContractValid.Should().BeTrue();
        existingVendor.Documents.PrivacyPolicyValid.Should().BeTrue();
        existingVendor.Documents.PentestReportValid.Should().BeTrue();

        // Risk history
        existingVendor.RiskAssessments.Should().HaveCount(1);
        var risk = existingVendor.RiskAssessments[0];
        risk.RiskLevel.Should().Be("Low");
        Math.Abs(risk.RiskScore - 0.10).Should().BeLessThan(0.0001);
        risk.Reasons.Should().Contain("Çok iyi SLA");

        mockRuleEngine.Verify(s => s.CalculateRiskAsync(It.IsAny<VendorProfile>()), Times.Once);
        mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);

        // Cache invalidate edilmeli
        mockCache.Verify(c => c.RemoveVendorRiskAsync(
                vendorId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFound_WhenVendorDoesNotExist()
    {
        // Arrange
        var vendorId = Guid.NewGuid();
        var command = new VendorUpdateCommand
        {
            Id = vendorId,
            Name = "Does not matter"
        };

        var mockRepo = new Mock<IVendorRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(vendorId))
            .ReturnsAsync((VendorProfile?)null);

        var mockRuleEngine = new Mock<IRuleEngineService>();
        var mockCache = new Mock<IRiskCacheService>();

        var handler = new VendorUpdateCommandHandler(
            mockRepo.Object,
            mockRuleEngine.Object,
            mockCache.Object
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        result.Errors.Should().ContainSingle()
            .Which.Should().Be("Vendor bulunamadı.");

        mockRuleEngine.Verify(s => s.CalculateRiskAsync(It.IsAny<VendorProfile>()), Times.Never);
        mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);

        // Vendor bulunamadığı için cache'e dokunulmamalı
        mockCache.Verify(c => c.RemoveVendorRiskAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}