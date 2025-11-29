namespace VendorRiskScoring.API.Controllers;

[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class VendorController(IMediator mediator, ILogger<VendorController> logger) : ControllerBase
{
    /// <summary>Vendor’ın güncel risk skorunu döner.</summary>
    /// <param name="id">Vendor ID</param>
    /// <returns>Risk bilgisi</returns>
    /// <response code="200">Risk bilgisi bulundu</response>
    /// <response code="404">Vendor bulunamadı</response>
    [HttpGet("{id:guid}/risk")]
    [ProducesResponseType(typeof(Result<RiskAssessmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<RiskAssessmentDto>>> GetRisk(Guid id)
    {
        logger.LogInformation("GET vendor risk requested. VendorId={VendorId}", id);
        var result = await mediator.Send(new GetVendorRiskQuery(id));

        if (!result.IsSuccess)
        {
            logger.LogWarning("Vendor risk not found. VendorId={VendorId}, StatusCode={StatusCode}", id,
                result.StatusCode);
            return StatusCode(result.StatusCode, result);
        }

        logger.LogInformation("Vendor risk returned. VendorId={VendorId}, Level={RiskLevel}, Score={RiskScore}", id,
            result.Value!.RiskLevel, result.Value.RiskScore);
        return Ok(result);
    }

    /// <summary>Sistemde kayıtlı tüm vendor'ların listesini döner.</summary>
    [HttpGet("list-with-pagination")]
    [ProducesResponseType(typeof(Result<PaginatedList<VendorWithRiskDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] GetVendorListQuery request)
    {
        logger.LogInformation(
            "Request: GetVendorList PageIndex={PageIndex}, PageSize={PageSize}, Sort={Sort}, Query={Query}",
            request.PageIndex, request.PageSize, request.Sort, request.Query);
        var result = await mediator.Send(request);

        logger.LogInformation("Response: VendorList ReturnedCount={Count}", result.Value!.Items.Count);
        return Ok(result);
    }

    /// <summary>Yeni bir vendor oluşturur ve risk skoru hesaplar.</summary>
    /// <param name="command">Oluşturulacak vendor bilgileri</param>
    /// <returns>Oluşturulan vendor'ın ID bilgisi</returns>
    /// <response code="201">Vendor başarıyla oluşturuldu</response>
    [HttpPost]
    [ProducesResponseType(typeof(Result<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result<Guid>>> CreateVendor([FromBody] VendorCreateCommand command)
    {
        logger.LogInformation("Request: CreateVendor Name={Name}", command.Name);
        var result = await mediator.Send(command);

        if (!result.IsSuccess)
        {
            logger.LogWarning("CreateVendor failed Name={Name}, Errors={Errors}", command.Name, result.Errors);
            return BadRequest(result);
        }

        logger.LogInformation("Vendor created VendorId={VendorId}, Name={Name}", result.Value, command.Name);
        return CreatedAtAction(nameof(GetRisk), new { id = result.Value }, result);
    }

    /// <summary>Mevcut bir vendor'ın bilgilerini günceller ve yeni risk skorunu hesaplar.</summary>
    /// <param name="id">Güncellenecek vendor'ın ID değeri (route parametresi).</param>
    /// <param name="command">Güncellenmiş vendor bilgilerini içeren komut nesnesi.</param>
    /// <returns>İşlem sonucu bilgisi.</returns>
    /// <response code="204">Vendor başarıyla güncellendi.</response>
    /// <response code="400">Geçersiz istek (örneğin, ID uyuşmazlığı).</response>
    /// <response code="404">Belirtilen ID’ye sahip vendor bulunamadı.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result>> UpdateVendor(Guid id, [FromBody] VendorUpdateCommand command)
    {
        if (command.Id != id)
        {
            logger.LogWarning("UpdateVendor ID mismatch RouteId={RouteId}, BodyId={BodyId}", id, command.Id);
            return BadRequest(Result.Failure("URL ile body'deki ID eşleşmiyor", StatusCodes.Status400BadRequest));
        }

        logger.LogInformation("Request: UpdateVendor VendorId={VendorId}", id);
        var result = await mediator.Send(command);

        if (!result.IsSuccess)
        {
            logger.LogWarning("UpdateVendor failed VendorId={VendorId}, StatusCode={StatusCode}", id,
                result.StatusCode);
            return StatusCode(result.StatusCode, result);
        }

        logger.LogInformation("Vendor updated VendorId={VendorId}", id);
        return NoContent();
    }

    /// <summary>Belirli bir vendor'ı sistemden siler </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteVendor(Guid id)
    {
        logger.LogInformation("Request: DeleteVendor VendorId={VendorId}", id);
        var result = await mediator.Send(new VendorDeleteCommand(id));

        if (!result.IsSuccess)
        {
            logger.LogWarning("DeleteVendor failed VendorId={VendorId}, StatusCode={StatusCode}", id,
                result.StatusCode);
            return StatusCode(result.StatusCode, result);
        }

        logger.LogInformation("Vendor deleted VendorId={VendorId}", id);
        return NoContent();
    }
}