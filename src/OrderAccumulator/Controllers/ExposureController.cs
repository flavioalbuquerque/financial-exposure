using Microsoft.AspNetCore.Mvc;
using OrderAccumulator.Services.Interfaces;

namespace OrderAccumulator.Controllers;

/// <summary>
/// Controle de Exposição Financeira.
/// </summary>
[ApiController]
[Route("[controller]")]
public class ExposureController : ControllerBase
{
    private readonly ILogger<ExposureController> _logger;
    private readonly IExposureService _exposureService;

    public ExposureController(ILogger<ExposureController> logger,
                              IExposureService exposureService)
    {
        _logger = logger;
        _exposureService = exposureService;
    }

    /// <summary>
    /// Obtém a exposição financeira para o símbolo especificado.
    /// </summary>
    /// <param name="symbol">Símbolo (exemplo: PETR4, VALE3, VIIA4).</param>
    /// <returns>Exposição financeira para o símbolo.</returns>
    [HttpGet("symbol/{symbol}", Name = "GetExposure")]
    [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)] 
    [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status400BadRequest)] 
    [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status500InternalServerError)] 
    public IActionResult GetExposure(string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            return BadRequest("O parâmetro 'symbol' é obrigatório.");
        }

        var exposure = _exposureService.GetExposure(symbol.ToUpperInvariant());
        return Ok(exposure);
    }
    
    /// <summary>
    /// Obtém a exposição financeira para todos os símbolos.
    /// </summary>
    /// <returns>Exposição financeira de todos os símbolos.</returns>
    [HttpGet("symbol/all", Name = "GetAllExposures")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)] 
    [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status500InternalServerError)] 
    public IActionResult GetAllExposures()
    {
        var exposures = _exposureService.GetAllExposures();
        return Ok(exposures);
    }
    
    /// <summary>
    /// Obtém a exposição financeira máxima padrão.
    /// </summary>
    /// <returns>Valor da exposição financeira máxima padrão.</returns>
    [HttpGet("default-max-exposure", Name = "GetDefaultMaxExposure")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)] 
    [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status500InternalServerError)] 
    public IActionResult GetDefaultMaxExposure()
    {
        var defaultMaxExposure = _exposureService.GetDefaultMaxExposure();
        return Ok(defaultMaxExposure);
    }
    
    /// <summary>
    /// Define a exposição financeira máxima padrão.
    /// </summary>
    /// <param name="defaultMaxExposure">Valor da exposição financeira máxima padrão</param>
    /// <returns>Sucesso ou falha.</returns>
    [HttpPut("default-max-exposure",Name = "SetDefaultMaxExposure")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)] 
    [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status400BadRequest)] 
    [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status500InternalServerError)] 
    public IActionResult SetDefaultMaxExposure(decimal defaultMaxExposure)
    {
        _exposureService.SetDefaultMaxExposure(defaultMaxExposure);
        return Ok();
    }
}