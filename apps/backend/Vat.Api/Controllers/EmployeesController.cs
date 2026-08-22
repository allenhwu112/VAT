using Microsoft.AspNetCore.Mvc;
using Vat.Api.Data;
using Vat.Api.Models;

namespace Vat.Api.Controllers;

[ApiController]
[Route("api/employees")]
public sealed class EmployeesController(IEmployeeRepository repository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<object>> GetAll(CancellationToken cancellationToken)
    {
        var employees = await repository.QueryAsync(null, cancellationToken);
        return Ok(new { data = employees });
    }

    [HttpGet("{employeeId:int}")]
    public async Task<ActionResult<object>> GetById(
        int employeeId,
        CancellationToken cancellationToken)
    {
        var employees = await repository.QueryAsync(employeeId, cancellationToken);
        var employee = employees.SingleOrDefault();

        return employee is null
            ? NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "找不到員工",
                Detail = "找不到指定的員工資料。",
            })
            : Ok(new { data = employee });
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var employee = await repository.CreateAsync(request, cancellationToken);
            return CreatedAtAction(
                nameof(GetById),
                new { employeeId = employee.EmployeeId },
                new { data = employee });
        }
        catch (EmployeeConflictException exception)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "員工資料衝突",
                Detail = exception.Message,
            });
        }
    }

    [HttpPut("{employeeId:int}")]
    public async Task<IActionResult> Update(
        int employeeId,
        UpdateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var employee = await repository.UpdateAsync(employeeId, request, cancellationToken);
            return Ok(new { data = employee });
        }
        catch (EmployeeNotFoundException exception)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "找不到員工",
                Detail = exception.Message,
            });
        }
        catch (EmployeeConflictException exception)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "員工資料衝突",
                Detail = exception.Message,
            });
        }
    }

    [HttpDelete("{employeeId:int}")]
    public async Task<IActionResult> Delete(
        int employeeId,
        CancellationToken cancellationToken)
    {
        try
        {
            await repository.DeleteAsync(employeeId, cancellationToken);
            return NoContent();
        }
        catch (EmployeeNotFoundException exception)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "找不到員工",
                Detail = exception.Message,
            });
        }
    }
}
