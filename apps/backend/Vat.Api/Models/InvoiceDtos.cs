using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Vat.Api.Models;

public abstract class InvoiceRequestBase : IValidatableObject
{
    [Required(ErrorMessage = "統編為必填欄位。")]
    [StringLength(8, ErrorMessage = "統編必須為 8 碼數字。")]
    public string? TaxId { get; set; }

    [Required(ErrorMessage = "客戶簡稱為必填欄位。")]
    [StringLength(50, ErrorMessage = "客戶簡稱不可超過 50 個字元。")]
    public string? ClientShortName { get; set; }

    public bool ElectronicInvoice { get; set; }

    [Required(ErrorMessage = "收銀機數量為必填欄位。")]
    [Range(0, 99, ErrorMessage = "收銀機數量必須介於 0 到 99。")]
    public int? CashRegister { get; set; }

    [Required(ErrorMessage = "三收銀數量為必填欄位。")]
    [Range(0, 99, ErrorMessage = "三收銀數量必須介於 0 到 99。")]
    public int? ThreeCashRegister { get; set; }

    [Required(ErrorMessage = "二聯式數量為必填欄位。")]
    [Range(0, 99, ErrorMessage = "二聯式數量必須介於 0 到 99。")]
    public int? TwoPartInvoice { get; set; }

    [Required(ErrorMessage = "二聯式副聯數量為必填欄位。")]
    [Range(0, 99, ErrorMessage = "二聯式副聯數量必須介於 0 到 99。")]
    public int? TwoPartInvoiceCopy { get; set; }

    [Required(ErrorMessage = "三聯式數量為必填欄位。")]
    [Range(0, 99, ErrorMessage = "三聯式數量必須介於 0 到 99。")]
    public int? ThreePartInvoice { get; set; }

    [Required(ErrorMessage = "三聯式副聯數量為必填欄位。")]
    [Range(0, 99, ErrorMessage = "三聯式副聯數量必須介於 0 到 99。")]
    public int? ThreePartInvoiceCopy { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(TaxId)
            || !Regex.IsMatch(TaxId.Trim(), "^[0-9]{8}$"))
        {
            yield return new ValidationResult("統編格式不正確。", [nameof(TaxId)]);
        }

        if (string.IsNullOrWhiteSpace(ClientShortName))
        {
            yield return new ValidationResult("客戶簡稱為必填欄位。", [nameof(ClientShortName)]);
        }
    }
}

public sealed class CreateInvoiceRequest : InvoiceRequestBase
{
}

public sealed class UpdateInvoiceRequest : InvoiceRequestBase
{
}

public sealed record InvoiceResponse(
    int InvoiceId,
    string TaxId,
    string ClientShortName,
    bool ElectronicInvoice,
    int CashRegister,
    int ThreeCashRegister,
    int TwoPartInvoice,
    int TwoPartInvoiceCopy,
    int ThreePartInvoice,
    int ThreePartInvoiceCopy);
