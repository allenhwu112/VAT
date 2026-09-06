using Microsoft.AspNetCore.Mvc;
using Vat.Api.Data;
using Vat.Api.Models;

namespace Vat.Api.Controllers;

[ApiController]
[Route("VAT_API/clients")]
public sealed class ClientsController(IClientRepository repository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<object>> GetAll(CancellationToken cancellationToken)
    {
        var clients = await repository.QueryAsync(null, cancellationToken);
        return Ok(new { data = clients });
    }

    [HttpGet("{clientId:int}")]
    public async Task<ActionResult<object>> GetById(
        int clientId,
        CancellationToken cancellationToken)
    {
        var clients = await repository.QueryAsync(clientId, cancellationToken);
        var client = clients.SingleOrDefault();

        return client is null
            ? NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "找不到客戶",
                Detail = "找不到指定的客戶資料。",
            })
            : Ok(new { data = client });
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateClientRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = await repository.CreateAsync(request, cancellationToken);
            return CreatedAtAction(
                nameof(GetById),
                new { clientId = client.ClientId },
                new { data = client });
        }
        catch (ClientConflictException exception)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "客戶資料衝突",
                Detail = exception.Message,
            });
        }
    }

    [HttpPut("{clientId:int}")]
    public async Task<IActionResult> Update(
        int clientId,
        UpdateClientRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = await repository.UpdateAsync(clientId, request, cancellationToken);
            return Ok(new { data = client });
        }
        catch (ClientNotFoundException exception)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "找不到客戶",
                Detail = exception.Message,
            });
        }
        catch (ClientConflictException exception)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "客戶資料衝突",
                Detail = exception.Message,
            });
        }
    }

    [HttpDelete("{clientId:int}")]
    public async Task<IActionResult> Delete(
        int clientId,
        CancellationToken cancellationToken)
    {
        try
        {
            await repository.DeleteAsync(clientId, cancellationToken);
            return NoContent();
        }
        catch (ClientNotFoundException exception)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "找不到客戶",
                Detail = exception.Message,
            });
        }
    }
}
