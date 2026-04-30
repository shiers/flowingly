using Flowingly.Import.Api.Contracts;
using Flowingly.Import.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Flowingly.Import.Api.Controllers;

[ApiController]
[Route("api/import")]
public sealed class ImportController : ControllerBase
{
    private readonly IImportApplicationService _service;

    public ImportController(IImportApplicationService service)
    {
        _service = service;
    }

    /// <summary>
    /// Parses raw text input, extracts marked-up fields, validates, and returns a structured result.
    /// </summary>
    [HttpPost("parse")]
    [ProducesResponseType(typeof(ParseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ParseResponse), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Parse([FromBody] ParseRequest request)
    {
        var response = _service.Parse(request.Text, request.TaxRatePercent);
        return response.Success ? Ok(response) : UnprocessableEntity(response);
    }
}
