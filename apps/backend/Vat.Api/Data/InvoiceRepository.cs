using System.Data;
using Microsoft.Data.SqlClient;
using Vat.Api.Models;

namespace Vat.Api.Data;

public interface IInvoiceRepository
{
    Task<IReadOnlyList<InvoiceResponse>> QueryAsync(
        int? invoiceId,
        CancellationToken cancellationToken);

    Task<InvoiceResponse> CreateAsync(
        CreateInvoiceRequest request,
        CancellationToken cancellationToken);

    Task<InvoiceResponse> UpdateAsync(
        int invoiceId,
        UpdateInvoiceRequest request,
        CancellationToken cancellationToken);

    Task DeleteAsync(int invoiceId, CancellationToken cancellationToken);
}

public sealed class InvoiceNotFoundException : Exception
{
    public InvoiceNotFoundException()
        : base("找不到指定的發票管理資料。")
    {
    }
}

public sealed class InvoiceConflictException : Exception
{
    public InvoiceConflictException()
        : base("統編已存在於發票管理資料。")
    {
    }
}

public sealed class InvoiceRepository(IConfiguration configuration) : IInvoiceRepository
{
    private const string QueryProcedure = "dbo.Invoice_Query";
    private const string CommandProcedure = "dbo.Invoice_Command";

    private readonly string _connectionString = configuration.GetConnectionString("VatDatabase")
        ?? throw new InvalidOperationException("Missing configuration: ConnectionStrings:VatDatabase.");

    public async Task<IReadOnlyList<InvoiceResponse>> QueryAsync(
        int? invoiceId,
        CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(QueryProcedure, connection)
        {
            CommandType = CommandType.StoredProcedure,
        };
        command.Parameters.Add("@InvoiceId", SqlDbType.Int).Value = invoiceId ?? (object)DBNull.Value;

        var invoices = new List<InvoiceResponse>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            invoices.Add(MapInvoice(reader));
        }

        return invoices;
    }

    public Task<InvoiceResponse> CreateAsync(
        CreateInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        return ExecuteWriteAsync(
            action: "CREATE",
            invoiceId: null,
            taxId: request.TaxId,
            clientShortName: request.ClientShortName,
            electronicInvoice: request.ElectronicInvoice,
            cashRegister: request.CashRegister,
            threeCashRegister: request.ThreeCashRegister,
            twoPartInvoice: request.TwoPartInvoice,
            twoPartInvoiceCopy: request.TwoPartInvoiceCopy,
            threePartInvoice: request.ThreePartInvoice,
            threePartInvoiceCopy: request.ThreePartInvoiceCopy,
            cancellationToken);
    }

    public Task<InvoiceResponse> UpdateAsync(
        int invoiceId,
        UpdateInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        return ExecuteWriteAsync(
            action: "UPDATE",
            invoiceId,
            taxId: request.TaxId,
            clientShortName: request.ClientShortName,
            electronicInvoice: request.ElectronicInvoice,
            cashRegister: request.CashRegister,
            threeCashRegister: request.ThreeCashRegister,
            twoPartInvoice: request.TwoPartInvoice,
            twoPartInvoiceCopy: request.TwoPartInvoiceCopy,
            threePartInvoice: request.ThreePartInvoice,
            threePartInvoiceCopy: request.ThreePartInvoiceCopy,
            cancellationToken);
    }

    public async Task DeleteAsync(int invoiceId, CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = CreateCommand(
                connection,
                action: "DELETE",
                invoiceId,
                taxId: null,
                clientShortName: null,
                electronicInvoice: null,
                cashRegister: null,
                threeCashRegister: null,
                twoPartInvoice: null,
                twoPartInvoiceCopy: null,
                threePartInvoice: null,
                threePartInvoiceCopy: null);

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (SqlException exception) when (exception.Number == 53007)
        {
            throw new InvoiceNotFoundException();
        }
        catch (SqlException exception) when (exception.Number is 2601 or 2627)
        {
            throw new InvoiceConflictException();
        }
    }

    private async Task<InvoiceResponse> ExecuteWriteAsync(
        string action,
        int? invoiceId,
        string? taxId,
        string? clientShortName,
        bool? electronicInvoice,
        int? cashRegister,
        int? threeCashRegister,
        int? twoPartInvoice,
        int? twoPartInvoiceCopy,
        int? threePartInvoice,
        int? threePartInvoiceCopy,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = CreateCommand(
                connection,
                action,
                invoiceId,
                taxId,
                clientShortName,
                electronicInvoice,
                cashRegister,
                threeCashRegister,
                twoPartInvoice,
                twoPartInvoiceCopy,
                threePartInvoice,
                threePartInvoiceCopy);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                throw new InvoiceNotFoundException();
            }

            return MapInvoice(reader);
        }
        catch (SqlException exception) when (exception.Number == 53007)
        {
            throw new InvoiceNotFoundException();
        }
        catch (SqlException exception) when (exception.Number is 2601 or 2627)
        {
            throw new InvoiceConflictException();
        }
    }

    private static SqlCommand CreateCommand(
        SqlConnection connection,
        string action,
        int? invoiceId,
        string? taxId,
        string? clientShortName,
        bool? electronicInvoice,
        int? cashRegister,
        int? threeCashRegister,
        int? twoPartInvoice,
        int? twoPartInvoiceCopy,
        int? threePartInvoice,
        int? threePartInvoiceCopy)
    {
        var command = new SqlCommand(CommandProcedure, connection)
        {
            CommandType = CommandType.StoredProcedure,
        };

        command.Parameters.Add("@Action", SqlDbType.VarChar, 6).Value = action;
        command.Parameters.Add("@InvoiceId", SqlDbType.Int).Value = invoiceId ?? (object)DBNull.Value;
        command.Parameters.Add("@TaxId", SqlDbType.VarChar, 8).Value =
            taxId?.Trim() ?? (object)DBNull.Value;
        command.Parameters.Add("@ClientShortName", SqlDbType.NVarChar, 50).Value =
            clientShortName?.Trim() ?? (object)DBNull.Value;
        command.Parameters.Add("@ElectronicInvoice", SqlDbType.Bit).Value =
            electronicInvoice ?? (object)DBNull.Value;
        command.Parameters.Add("@CashRegister", SqlDbType.Int).Value =
            cashRegister ?? (object)DBNull.Value;
        command.Parameters.Add("@ThreeCashRegister", SqlDbType.Int).Value =
            threeCashRegister ?? (object)DBNull.Value;
        command.Parameters.Add("@TwoPartInvoice", SqlDbType.Int).Value =
            twoPartInvoice ?? (object)DBNull.Value;
        command.Parameters.Add("@TwoPartInvoiceCopy", SqlDbType.Int).Value =
            twoPartInvoiceCopy ?? (object)DBNull.Value;
        command.Parameters.Add("@ThreePartInvoice", SqlDbType.Int).Value =
            threePartInvoice ?? (object)DBNull.Value;
        command.Parameters.Add("@ThreePartInvoiceCopy", SqlDbType.Int).Value =
            threePartInvoiceCopy ?? (object)DBNull.Value;

        return command;
    }

    private static InvoiceResponse MapInvoice(SqlDataReader reader)
    {
        return new InvoiceResponse(
            InvoiceId: reader.GetInt32(reader.GetOrdinal("InvoiceId")),
            TaxId: reader.GetString(reader.GetOrdinal("TaxId")),
            ClientShortName: reader.GetString(reader.GetOrdinal("ClientShortName")),
            ElectronicInvoice: reader.GetBoolean(reader.GetOrdinal("ElectronicInvoice")),
            CashRegister: reader.GetInt32(reader.GetOrdinal("CashRegister")),
            ThreeCashRegister: reader.GetInt32(reader.GetOrdinal("ThreeCashRegister")),
            TwoPartInvoice: reader.GetInt32(reader.GetOrdinal("TwoPartInvoice")),
            TwoPartInvoiceCopy: reader.GetInt32(reader.GetOrdinal("TwoPartInvoiceCopy")),
            ThreePartInvoice: reader.GetInt32(reader.GetOrdinal("ThreePartInvoice")),
            ThreePartInvoiceCopy: reader.GetInt32(reader.GetOrdinal("ThreePartInvoiceCopy")));
    }
}
