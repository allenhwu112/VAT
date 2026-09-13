using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Vat.Api.Models;

public abstract class ClientRequestBase : IValidatableObject
{
    [StringLength(4, ErrorMessage = "客編必須為 1 碼英文加 3 碼數字。")]
    public string? ClientCode { get; set; }

    [Required(ErrorMessage = "統編為必填欄位。")]
    [StringLength(8, ErrorMessage = "統編必須為 8 碼數字。")]
    public string? TaxId { get; set; }

    [StringLength(100, ErrorMessage = "客戶全稱不可超過 100 個字元。")]
    public string? FullName { get; set; }

    [StringLength(50, ErrorMessage = "簡稱不可超過 50 個字元。")]
    public string? ShortName { get; set; }

    [StringLength(100, ErrorMessage = "負責人不可超過 100 個字元。")]
    public string? ResponsiblePerson { get; set; }

    [StringLength(255, ErrorMessage = "地址不可超過 255 個字元。")]
    public string? Address { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrWhiteSpace(ClientCode)
            && !Regex.IsMatch(ClientCode.Trim(), "^[A-Za-z][0-9]{3}$"))
        {
            yield return new ValidationResult("客編格式不正確。", [nameof(ClientCode)]);
        }

        if (string.IsNullOrWhiteSpace(TaxId)
            || !Regex.IsMatch(TaxId.Trim(), "^[0-9]{8}$"))
        {
            yield return new ValidationResult("統編格式不正確。", [nameof(TaxId)]);
        }

    }
}

public sealed class CreateClientRequest : ClientRequestBase
{
}

public sealed class UpdateClientRequest : ClientRequestBase
{
}

public sealed record ClientResponse(
    int ClientId,
    string? ClientCode,
    string TaxId,
    string? FullName,
    string? ShortName,
    string? ResponsiblePerson,
    string? Address);
