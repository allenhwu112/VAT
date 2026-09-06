using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Vat.Api.Models;

public abstract class EmployeeRequestBase : IValidatableObject
{
    [Required(ErrorMessage = "姓名為必填欄位。")]
    [StringLength(100, ErrorMessage = "姓名不可超過 100 個字元。")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "簡稱為必填欄位。")]
    [StringLength(50, ErrorMessage = "簡稱不可超過 50 個字元。")]
    public string? ShortName { get; set; }

    [Required(ErrorMessage = "性別為必填欄位。")]
    [RegularExpression("^[MF]$", ErrorMessage = "性別格式不正確。")]
    public string? Gender { get; set; }

    [Required(ErrorMessage = "身份證字號為必填欄位。")]
    [RegularExpression("^[A-Z][0-9]{9}$", ErrorMessage = "身份證字號格式不正確。")]
    public string? NationalId { get; set; }

    [StringLength(30, ErrorMessage = "聯絡電話不可超過 30 個字元。")]
    public string? ContactPhone { get; set; }

    [StringLength(255, ErrorMessage = "地址不可超過 255 個字元。")]
    public string? Address { get; set; }

    public DateOnly? BirthDate { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            yield return new ValidationResult("姓名為必填欄位。", [nameof(Name)]);
        }

        if (string.IsNullOrWhiteSpace(ShortName))
        {
            yield return new ValidationResult("簡稱為必填欄位。", [nameof(ShortName)]);
        }

        if (string.IsNullOrWhiteSpace(Gender) || !Regex.IsMatch(Gender, "^[MF]$"))
        {
            yield return new ValidationResult("性別格式不正確。", [nameof(Gender)]);
        }

        if (string.IsNullOrWhiteSpace(NationalId) || !Regex.IsMatch(NationalId, "^[A-Z][0-9]{9}$"))
        {
            yield return new ValidationResult("身份證字號格式不正確。", [nameof(NationalId)]);
        }
    }
}

public sealed class CreateEmployeeRequest : EmployeeRequestBase
{
    [Required(ErrorMessage = "密碼為必填欄位。")]
    [StringLength(255, ErrorMessage = "密碼不可超過 255 個字元。")]
    public string? Password { get; set; }
}

public sealed class UpdateEmployeeRequest : EmployeeRequestBase
{
    [StringLength(255, ErrorMessage = "密碼不可超過 255 個字元。")]
    public string? Password { get; set; }
}

public sealed record EmployeeResponse(
    int EmployeeId,
    string Name,
    string ShortName,
    string Gender,
    string NationalId,
    string? ContactPhone,
    string? Address,
    DateOnly? BirthDate);
