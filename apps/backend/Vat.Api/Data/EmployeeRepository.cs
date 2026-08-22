using System.Data;
using Microsoft.Data.SqlClient;
using Vat.Api.Models;

namespace Vat.Api.Data;

public interface IEmployeeRepository
{
    Task<IReadOnlyList<EmployeeResponse>> QueryAsync(int? employeeId, CancellationToken cancellationToken);

    Task<EmployeeResponse> CreateAsync(
        CreateEmployeeRequest request,
        CancellationToken cancellationToken);

    Task<EmployeeResponse> UpdateAsync(
        int employeeId,
        UpdateEmployeeRequest request,
        CancellationToken cancellationToken);

    Task DeleteAsync(int employeeId, CancellationToken cancellationToken);
}

public sealed class EmployeeNotFoundException : Exception
{
    public EmployeeNotFoundException()
        : base("找不到指定的員工資料。")
    {
    }
}

public sealed class EmployeeConflictException : Exception
{
    public EmployeeConflictException()
        : base("身份證字號已存在。")
    {
    }
}

public sealed class EmployeeRepository(IConfiguration configuration) : IEmployeeRepository
{
    private const string QueryProcedure = "dbo.Employee_Query";
    private const string CommandProcedure = "dbo.Employee_Command";

    private readonly string _connectionString = configuration.GetConnectionString("VatDatabase")
        ?? throw new InvalidOperationException("Missing configuration: ConnectionStrings:VatDatabase.");

    public async Task<IReadOnlyList<EmployeeResponse>> QueryAsync(
        int? employeeId,
        CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(QueryProcedure, connection)
        {
            CommandType = CommandType.StoredProcedure,
        };
        command.Parameters.Add("@EmployeeId", SqlDbType.Int).Value = employeeId ?? (object)DBNull.Value;

        var employees = new List<EmployeeResponse>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            employees.Add(MapEmployee(reader));
        }

        return employees;
    }

    public async Task<EmployeeResponse> CreateAsync(
        CreateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        return await ExecuteWriteAsync(
            action: "CREATE",
            employeeId: null,
            name: request.Name,
            shortName: request.ShortName,
            gender: request.Gender,
            nationalId: request.NationalId,
            password: request.Password,
            cancellationToken);
    }

    public async Task<EmployeeResponse> UpdateAsync(
        int employeeId,
        UpdateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        return await ExecuteWriteAsync(
            action: "UPDATE",
            employeeId,
            name: request.Name,
            shortName: request.ShortName,
            gender: request.Gender,
            nationalId: request.NationalId,
            password: request.Password,
            cancellationToken);
    }

    public async Task DeleteAsync(int employeeId, CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = CreateCommand(
                connection,
                action: "DELETE",
                employeeId,
                name: null,
                shortName: null,
                gender: null,
                nationalId: null,
                password: null);

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (SqlException exception) when (exception.Number == 51007)
        {
            throw new EmployeeNotFoundException();
        }
        catch (SqlException exception) when (exception.Number is 2601 or 2627)
        {
            throw new EmployeeConflictException();
        }
    }

    private async Task<EmployeeResponse> ExecuteWriteAsync(
        string action,
        int? employeeId,
        string? name,
        string? shortName,
        string? gender,
        string? nationalId,
        string? password,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = CreateCommand(
                connection,
                action,
                employeeId,
                name,
                shortName,
                gender,
                nationalId,
                password);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                throw new EmployeeNotFoundException();
            }

            return MapEmployee(reader);
        }
        catch (SqlException exception) when (exception.Number == 51007)
        {
            throw new EmployeeNotFoundException();
        }
        catch (SqlException exception) when (exception.Number is 2601 or 2627)
        {
            throw new EmployeeConflictException();
        }
    }

    private static SqlCommand CreateCommand(
        SqlConnection connection,
        string action,
        int? employeeId,
        string? name,
        string? shortName,
        string? gender,
        string? nationalId,
        string? password)
    {
        var command = new SqlCommand(CommandProcedure, connection)
        {
            CommandType = CommandType.StoredProcedure,
        };

        command.Parameters.Add("@Action", SqlDbType.VarChar, 6).Value = action;
        command.Parameters.Add("@EmployeeId", SqlDbType.Int).Value = employeeId ?? (object)DBNull.Value;
        command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = name?.Trim() ?? (object)DBNull.Value;
        command.Parameters.Add("@ShortName", SqlDbType.NVarChar, 50).Value =
            shortName?.Trim() ?? (object)DBNull.Value;
        command.Parameters.Add("@Gender", SqlDbType.Char, 1).Value =
            gender?.Trim().ToUpperInvariant() ?? (object)DBNull.Value;
        command.Parameters.Add("@NationalId", SqlDbType.VarChar, 10).Value =
            nationalId?.Trim().ToUpperInvariant() ?? (object)DBNull.Value;
        command.Parameters.Add("@Password", SqlDbType.NVarChar, 255).Value =
            password ?? (object)DBNull.Value;

        return command;
    }

    private static EmployeeResponse MapEmployee(SqlDataReader reader)
    {
        return new EmployeeResponse(
            EmployeeId: reader.GetInt32(reader.GetOrdinal("EmployeeId")),
            Name: reader.GetString(reader.GetOrdinal("Name")),
            ShortName: reader.GetString(reader.GetOrdinal("ShortName")),
            Gender: reader.GetString(reader.GetOrdinal("Gender")).Trim(),
            NationalId: reader.GetString(reader.GetOrdinal("NationalId")));
    }
}
