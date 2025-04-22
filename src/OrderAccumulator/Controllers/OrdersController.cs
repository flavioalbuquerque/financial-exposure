using Microsoft.AspNetCore.Mvc;
using OrderAccumulator.Models;
using OrderAccumulator.Services.Interfaces;

namespace OrderAccumulator.Controllers;

/// <summary>
/// Controle de Ordens.
/// </summary>
[ApiController]
[Route("[controller]")]
public class OrdersController : ControllerBase
{
    private readonly ILogger<OrdersController> _logger;
    private readonly IExposureService _exposureService;

    public OrdersController(ILogger<OrdersController> logger,
                            IExposureService exposureService)
    {
        _logger = logger;
        _exposureService = exposureService;
    }

    /// <summary>
    /// Consultar ordens de um símbolo
    /// </summary>
    /// <remarks>
    /// Obtém as ordens geradas para o símbolo especificado.
    /// </remarks>
    /// <param name="symbol">Símbolo (exemplo: PETR4, VALE3, VIIA4).</param>
    /// <returns>Lista de ordens para o símbolo.</returns>
    [HttpGet("symbol/{symbol}", Name = "GetOrders")]
    [ProducesResponseType(typeof(IReadOnlyList<Order>), StatusCodes.Status200OK)] 
    [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status400BadRequest)] 
    [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status500InternalServerError)] 
    public IActionResult GetOrders(string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            return BadRequest("O parâmetro 'symbol' é obrigatório.");
        }

        var orders = _exposureService.GetOrders(symbol.ToUpperInvariant());
        return Ok(orders);
    }
    
    /// <summary>
    /// Consultar todas as ordens
    /// </summary>
    /// <remarks>
    /// Obtém as ordens geradas para todos os símbolos.
    /// </remarks>
    /// <returns>Ordens de todos os símbolos.</returns>
    [HttpGet("symbol/all", Name = "GetAllOrders")]
    [ProducesResponseType(typeof(IReadOnlyDictionary<string, List<Order>>), StatusCodes.Status200OK)] 
    [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status500InternalServerError)] 
    public IActionResult GetAllOrders()
    {
        var orders = _exposureService.GetAllOrders();
        return Ok(orders);
    }
    
    /// <summary>
    /// Excluir as ordens de um símbolo
    /// </summary>
    /// <remarks>
    /// Exclui as ordens um determinado símbolos.
    /// </remarks>
    /// <returns>Sucesso ou erro.</returns>
    [HttpDelete("symbol/{symbol}", Name = "DeleteOrders")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status500InternalServerError)] 
    public IActionResult DeleteOrders(string symbol)
    {
        _exposureService.DeleteOrders(symbol.ToUpperInvariant());
        return NoContent();
    }
    
    /// <summary>
    /// Excluir todas as ordens
    /// </summary>
    /// <remarks>
    /// Exclui as ordens de todos os símbolos.
    /// </remarks>
    /// <returns>Sucesso ou erro.</returns>
    [HttpDelete("symbol/all", Name = "DeleteAllOrders")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status500InternalServerError)] 
    public IActionResult DeleteAllOrders()
    {
        _exposureService.DeleteAllOrders();
        return NoContent();
    }
    
}