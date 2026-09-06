using System.Data;
using Microsoft.Data.SqlClient;
using Vat.Api.Models;

namespace Vat.Api.Data;

public interface IClientRepository
{
    Task<IReadOnlyList<ClientResponse>> QueryAsync(int? clientId, CancellationToken cancellationToken);

    Task<ClientResponse> CreateAsync(
        CreateClientRequest request,
        CancellationToken cancellationToken);

    Task<ClientResponse> UpdateAsync(
        int clientId,
        UpdateClientRequest request,
        CancellationToken cancellationToken);

    Task DeleteAsync(int clientId, CancellationToken cancellationToken);
}

public sealed class ClientNotFoundException : Exception
{
    public ClientNotFoundException()
        : base("找不到指定的客戶資料。")
    {
    }
}

public sealed class ClientConflictException : Exception
{
    public ClientConflictException()
        : base("統編已存在。")
    {
    }
}

public sealed class ClientRepository(IConfiguration configuration) : IClientRepository
{
    private const string QueryProcedure = "dbo.Client_Query";
    private const string CommandProcedure = "dbo.Client_Command";

    private readonly string _connectionString = configuration.GetConnectionString("VatDatabase")
        ?? throw new InvalidOperationException("Missing configuration: ConnectionStrings:VatDatabase.");

    public async Task<IReadOnlyList<ClientResponse>> QueryAsync(
        int? clientId,
        CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(QueryProcedure, connection)
        {
            CommandType = CommandType.StoredProcedure,
        };
        command.Parameters.Add("@ClientId", SqlDbType.Int).Value = clientId ?? (object)DBNull.Value;

        var clients = new List<ClientResponse>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            clients.Add(MapClient(reader));
        }

        return clients;
    }

    public Task<ClientResponse> CreateAsync(
        CreateClientRequest request,
        CancellationToken cancellationToken)
    {
        return ExecuteWriteAsync(
            action: "CREATE",
            clientId: null,
            taxId: request.TaxId,
            fullName: request.FullName,
            shortName: request.ShortName,
            responsiblePerson: request.ResponsiblePerson,
            address: request.Address,
            cancellationToken);
    }

    public Task<ClientResponse> UpdateAsync(
        int clientId,
        UpdateClientRequest request,
        CancellationToken cancellationToken)
    {
        return ExecuteWriteAsync(
            action: "UPDATE",
            clientId,
            taxId: request.TaxId,
            fullName: request.FullName,
            shortName: request.ShortName,
            responsiblePerson: request.ResponsiblePerson,
            address: request.Address,
            cancellationToken);
    }

    public async Task DeleteAsync(int clientId, CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = CreateCommand(
                connection,
                action: "DELETE",
                clientId,
                taxId: null,
                fullName: null,
                shortName: null,
                responsiblePerson: null,
                address: null);

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (SqlException exception) when (exception.Number == 52007)
        {
            throw new ClientNotFoundException();
        }
        catch (SqlException exception) when (exception.Number is 2601 or 2627)
        {
            throw new ClientConflictException();
        }
    }

    private async Task<ClientResponse> ExecuteWriteAsync(
        string action,
        int? clientId,
        string? taxId,
        string? fullName,
        string? shortName,
        string? responsiblePerson,
        string? address,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = CreateCommand(
                connection,
                action,
                clientId,
                taxId,
                fullName,
                shortName,
                responsiblePerson,
                address);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                throw new ClientNotFoundException();
            }

            return MapClient(reader);
        }
        catch (SqlException exception) when (exception.Number == 52007)
        {
            throw new ClientNotFoundException();
        }
        catch (SqlException exception) when (exception.Number is 2601 or 2627)
        {
            throw new ClientConflictException();
        }
    }

    private static SqlCommand CreateCommand(
        SqlConnection connection,
        string action,
        int? clientId,
        string? taxId,
        string? fullName,
        string? shortName,
        string? responsiblePerson,
        string? address)
    {
        var command = new SqlCommand(CommandProcedure, connection)
        {
            CommandType = CommandType.StoredProcedure,
        };

        command.Parameters.Add("@Action", SqlDbType.VarChar, 6).Value = action;
        command.Parameters.Add("@ClientId", SqlDbType.Int).Value = clientId ?? (object)DBNull.Value;
        command.Parameters.Add("@TaxId", SqlDbType.VarChar, 8).Value =
            taxId?.Trim() ?? (object)DBNull.Value;
        command.Parameters.Add("@FullName", SqlDbType.NVarChar, 100).Value =
            fullName?.Trim() ?? (object)DBNull.Value;
        command.Parameters.Add("@ShortName", SqlDbType.NVarChar, 50).Value =
            shortName?.Trim() ?? (object)DBNull.Value;
        command.Parameters.Add("@ResponsiblePerson", SqlDbType.NVarChar, 100).Value =
            responsiblePerson?.Trim() ?? (object)DBNull.Value;
        command.Parameters.Add("@Address", SqlDbType.NVarChar, 255).Value =
            address?.Trim() ?? (object)DBNull.Value;

        return command;
    }

    private static ClientResponse MapClient(SqlDataReader reader)
    {
        return new ClientResponse(
            ClientId: reader.GetInt32(reader.GetOrdinal("ClientId")),
            TaxId: reader.GetString(reader.GetOrdinal("TaxId")),
            FullName: reader.GetString(reader.GetOrdinal("FullName")),
            ShortName: reader.GetString(reader.GetOrdinal("ShortName")),
            ResponsiblePerson: reader.GetString(reader.GetOrdinal("ResponsiblePerson")),
            Address: reader.GetString(reader.GetOrdinal("Address")));
    }
}
