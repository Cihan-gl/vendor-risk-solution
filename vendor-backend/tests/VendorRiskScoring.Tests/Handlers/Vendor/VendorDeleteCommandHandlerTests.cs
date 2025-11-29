namespace VendorRiskScoring.Tests.Handlers.Vendor;

public class VendorDeleteCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_ReturnNotFound_When_VendorNotFound()
    {
        // Arrange
        var vendorId = Guid.NewGuid();

        var mockRepo = new Mock<IVendorRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(vendorId))
            .ReturnsAsync((VendorProfile?)null);

        var mockCache = new Mock<IRiskCacheService>();

        var handler = new VendorDeleteCommandHandler(
            mockRepo.Object,
            mockCache.Object
        );

        // Act
        var result = await handler.Handle(new VendorDeleteCommand(vendorId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        result.Errors.Should().ContainSingle()
            .Which.Should().Be("Vendor bulunamadı.");

        mockRepo.Verify(r => r.DeleteAsync(It.IsAny<VendorProfile>()), Times.Never);
        mockRepo.Verify(r => r.SaveChangesAsync(), Times.Never);

        // Cache'e dokunulmamalı
        mockCache.Verify(c => c.RemoveVendorRiskAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_DeleteVendor_And_RemoveCache_When_Exists()
    {
        // Arrange
        var vendorId = Guid.NewGuid();
        var vendor = new VendorProfile { Id = vendorId, Name = "To Be Deleted" };

        var mockRepo = new Mock<IVendorRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(vendorId))
            .ReturnsAsync(vendor);

        mockRepo.Setup(r => r.DeleteAsync(vendor))
            .Returns(Task.CompletedTask);

        mockRepo.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var mockCache = new Mock<IRiskCacheService>();
        mockCache.Setup(c => c.RemoveVendorRiskAsync(vendorId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new VendorDeleteCommandHandler(
            mockRepo.Object,
            mockCache.Object
        );

        // Act
        var result = await handler.Handle(new VendorDeleteCommand(vendorId), CancellationToken.None);

        // Assert - Result wrapper
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(StatusCodes.Status204NoContent); 
        result.Errors.Should().BeEmpty();

        mockRepo.Verify(r => r.DeleteAsync(It.Is<VendorProfile>(v => v.Id == vendorId)), Times.Once);
        mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);

        mockCache.Verify(c => c.RemoveVendorRiskAsync(
                vendorId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}