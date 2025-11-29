namespace VendorRiskScoring.Tests.Handlers.Vendor;

public class VendorCreateCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_CreateVendor_And_ReturnId()
    {
        // Arrange
        var command = new VendorCreateCommand
        {
            Name = "Acme Corp",
            FinancialHealth = 85,
            SlaUptime = 99.8,
            MajorIncidents = 0,
            SecurityCerts = ["ISO27001"],
            Documents = new VendorDocumentsDto
            {
                ContractValid = true,
                PrivacyPolicyValid = true,
                PentestReportValid = false
            }
        };

        var fakeId = Guid.NewGuid();

        var mockRuleEngine = new Mock<IRuleEngineService>();
        mockRuleEngine.Setup(s => s.CalculateRiskAsync(It.IsAny<VendorProfile>()))
            .ReturnsAsync(new RiskResultDto
            {
                Score = 0.75,
                Level = "Medium",
                Reasons = new List<string> { "Finansal skor orta düzeyde" }
            });

        var mockRepo = new Mock<IVendorRepository>();
        mockRepo.Setup(r => r.AddAsync(It.IsAny<VendorProfile>()))
            .Callback<VendorProfile>(v => v.Id = fakeId)
            .Returns(Task.CompletedTask);

        mockRepo.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var handler = new VendorCreateCommandHandler(
            mockRuleEngine.Object,
            mockRepo.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(fakeId);

        mockRepo.Verify(r => r.AddAsync(It.Is<VendorProfile>(v =>
            v.Name == command.Name &&
            v.FinancialHealth == command.FinancialHealth &&
            v.SlaUptime == command.SlaUptime &&
            v.MajorIncidents == command.MajorIncidents &&
            v.SecurityCerts.SequenceEqual(command.SecurityCerts) &&
            v.Documents.ContractValid == command.Documents.ContractValid &&
            v.Documents.PrivacyPolicyValid == command.Documents.PrivacyPolicyValid &&
            v.Documents.PentestReportValid == command.Documents.PentestReportValid &&
            v.RiskAssessments.Count == 1 &&
            v.RiskAssessments[0].RiskLevel == "Medium" &&
            Math.Abs(v.RiskAssessments[0].RiskScore - 0.75) < 0.0001
        )), Times.Once);

        mockRuleEngine.Verify(s => s.CalculateRiskAsync(It.IsAny<VendorProfile>()), Times.Once);
        mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}