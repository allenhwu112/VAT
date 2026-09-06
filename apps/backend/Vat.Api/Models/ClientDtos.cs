using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Vat.Api.Models;

public abstract class ClientRequestBase : IValidatableObject
{
    [Required(ErrorMessage = "統編為必填欄位。")]
    [StringLength(8, ErrorMessage = "統編必須為 8 碼數字。")]
    public string? TaxId { get; set; }

    [Required(ErrorMessage = "客戶全稱為必填欄位。")]
    [StringLength(100, ErrorMessage = "客戶全稱不可超過 100 個字元。")]
    public string? FullName { get; set; }

    [Required(ErrorMessage = "簡稱為必填欄位。")]
    [StringLength(50, ErrorMessage = "簡稱不可超過 50 個字元。")]
    public string? ShortName { get; set; }

    [Required(ErrorMessage = "負責人為必填欄位。")]
    [StringLength(100, ErrorMessage = "負責人不可超過 100 個字元。")]
    public string? ResponsiblePerson { get; set; }

    [Required(ErrorMessage = "地址為必填欄位。")]
    [StringLength(255, ErrorMessage = "地址不可超過 255 個字元。")]
    public string? Address { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(TaxId)
            || !Regex.IsMatch(TaxId.Trim(), "^[0-9]{8}$"))
        {
            yield return new ValidationResult("統編格式不正確。", [nameof(TaxId)]);
        }

        if (string.IsNullOrWhiteSpace(FullName))
        {
            yield return new ValidationResult("客戶全稱為必填欄位。", [nameof(FullName)]);
        }

        if (string.IsNullOrWhiteSpace(ShortName))
        {
            yield return new ValidationResult("簡稱為必填欄位。", [nameof(ShortName)]);
        }

        if (string.IsNullOrWhiteSpace(ResponsiblePerson))
        {
            yield return new ValidationResult("負責人為必填欄位。", [nameof(ResponsiblePerson)]);
        }

        if (string.IsNullOrWhiteSpace(Address))
        {
            yield return new ValidationResult("地址為必填欄位。", [nameof(Address)]);
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
    string TaxId,
    string FullName,
    string ShortName,
    string ResponsiblePerson,
    string Address);
