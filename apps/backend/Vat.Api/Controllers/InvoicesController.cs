using Microsoft.AspNetCore.Mvc;
using Vat.Api.Data;
using Vat.Api.Models;

namespace Vat.Api.Controllers;

[ApiController]
[Route("VAT_API/invoices")]
public sealed class InvoicesController(IInvoiceRepository repository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<object>> GetAll(CancellationToken cancellationToken)
    {
        var invoices = await repository.QueryAsync(null, cancellationToken);
        return Ok(new { data = invoices });
    }

    [HttpGet("{invoiceId:int}")]
    public async Task<ActionResult<object>> GetById(
        int invoiceId,
        CancellationToken cancellationToken)
    {
        var invoices = await repository.QueryAsync(invoiceId, cancellationToken);
        var invoice = invoices.SingleOrDefault();

        return invoice is null
            ? NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "找不到發票管理資料",
                Detail = "找不到指定的發票管理資料。",
            })
            : Ok(new { data = invoice });
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var invoice = await repository.CreateAsync(request, cancellationToken);
            return CreatedAtAction(
                nameof(GetById),
                new { invoiceId = invoice.InvoiceId },
                new { data = invoice });
        }
        catch (InvoiceConflictException exception)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "發票管理資料衝突",
                Detail = exception.Message,
            });
        }
    }

    [HttpPut("{invoiceId:int}")]
    public async Task<IActionResult> Update(
        int invoiceId,
        UpdateInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var invoice = await repository.UpdateAsync(invoiceId, request, cancellationToken);
            return Ok(new { data = invoice });
        }
        catch (InvoiceNotFoundException exception)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "找不到發票管理資料",
                Detail = exception.Message,
            });
        }
        catch (InvoiceConflictException exception)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "發票管理資料衝突",
                Detail = exception.Message,
            });
        }
    }

    [HttpDelete("{invoiceId:int}")]
    public async Task<IActionResult> Delete(
        int invoiceId,
        CancellationToken cancellationToken)
    {
        try
        {
            await repository.DeleteAsync(invoiceId, cancellationToken);
            return NoContent();
        }
        catch (InvoiceNotFoundException exception)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "找不到發票管理資料",
                Detail = exception.Message,
            });
        }
    }
}
