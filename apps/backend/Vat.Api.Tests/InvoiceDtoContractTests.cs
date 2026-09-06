using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Vat.Api.Tests;

public sealed class InvoiceDtoContractTests
{
    private static readonly Assembly ApiAssembly =
        typeof(Vat.Api.Controllers.HealthController).Assembly;

    private static readonly string[] QuantityPropertyNames =
    [
        "CashRegister",
        "ThreeCashRegister",
        "TwoPartInvoice",
        "TwoPartInvoiceCopy",
        "ThreePartInvoice",
        "ThreePartInvoiceCopy",
    ];

    [Fact]
    public void Invoice_controller_uses_the_expected_resource_route()
    {
        var controllerType = ApiAssembly.GetType("Vat.Api.Controllers.InvoicesController");

        Assert.NotNull(controllerType);
        Assert.Equal("VAT_API/invoices", controllerType!.GetCustomAttribute<RouteAttribute>()?.Template);
    }

    [Fact]
    public void Invoice_create_request_requires_tax_id_and_client_short_name()
    {
        var requestType = GetTypeOrFail("Vat.Api.Models.CreateInvoiceRequest");
        var request = Activator.CreateInstance(requestType)!;

        var errors = Validate(request);

        Assert.Contains(errors, error => error.MemberNames.Contains("TaxId"));
        Assert.Contains(errors, error => error.MemberNames.Contains("ClientShortName"));
        foreach (var propertyName in QuantityPropertyNames)
            Assert.Contains(errors, error => error.MemberNames.Contains(propertyName));
    }

    [Fact]
    public void Valid_invoice_create_and_update_requests_pass_validation()
    {
        var createType = GetTypeOrFail("Vat.Api.Models.CreateInvoiceRequest");
        var createRequest = Activator.CreateInstance(createType)!;
        SetValidFields(createType, createRequest);

        var updateType = GetTypeOrFail("Vat.Api.Models.UpdateInvoiceRequest");
        var updateRequest = Activator.CreateInstance(updateType)!;
        SetValidFields(updateType, updateRequest);

        Assert.Empty(Validate(createRequest));
        Assert.Empty(Validate(updateRequest));
    }

    [Fact]
    public void Invoice_validation_rejects_invalid_tax_ids_and_short_names()
    {
        var requestType = GetTypeOrFail("Vat.Api.Models.CreateInvoiceRequest");
        var invalidTaxIdRequest = Activator.CreateInstance(requestType)!;
        SetValidFields(requestType, invalidTaxIdRequest);
        Set(requestType, invalidTaxIdRequest, "TaxId", "1234567A");

        var invalidTaxIdErrors = Validate(invalidTaxIdRequest);
        Assert.Contains(invalidTaxIdErrors, error => error.MemberNames.Contains("TaxId"));

        var whitespaceShortNameRequest = Activator.CreateInstance(requestType)!;
        SetValidFields(requestType, whitespaceShortNameRequest);
        Set(requestType, whitespaceShortNameRequest, "ClientShortName", "   ");

        var whitespaceShortNameErrors = Validate(whitespaceShortNameRequest);
        Assert.Contains(
            whitespaceShortNameErrors,
            error => error.MemberNames.Contains("ClientShortName"));
    }

    [Fact]
    public void Invoice_validation_rejects_values_over_declared_limits()
    {
        var requestType = GetTypeOrFail("Vat.Api.Models.CreateInvoiceRequest");
        var request = Activator.CreateInstance(requestType)!;
        SetValidFields(requestType, request);
        Set(requestType, request, "ClientShortName", new string('簡', 51));
        Set(requestType, request, "CashRegister", 100);

        var errors = Validate(request);

        Assert.Contains(errors, error => error.MemberNames.Contains("ClientShortName"));
        Assert.Contains(errors, error => error.MemberNames.Contains("CashRegister"));
    }

    [Fact]
    public void Invoice_options_keep_the_checkbox_and_use_integer_quantities()
    {
        var requestType = GetTypeOrFail("Vat.Api.Models.CreateInvoiceRequest");
        var request = Activator.CreateInstance(requestType)!;

        Assert.Equal(false, requestType.GetProperty("ElectronicInvoice")!.GetValue(request));
        foreach (var propertyName in QuantityPropertyNames)
            Assert.Null(requestType.GetProperty(propertyName)!.GetValue(request));

        var responseType = GetTypeOrFail("Vat.Api.Models.InvoiceResponse");
        Assert.Equal(typeof(int), responseType.GetProperty("InvoiceId")?.PropertyType);
        Assert.Equal(typeof(string), responseType.GetProperty("TaxId")?.PropertyType);
        Assert.Equal(typeof(string), responseType.GetProperty("ClientShortName")?.PropertyType);
        Assert.Equal(typeof(bool), responseType.GetProperty("ElectronicInvoice")?.PropertyType);
        foreach (var propertyName in QuantityPropertyNames)
            Assert.Equal(typeof(int), responseType.GetProperty(propertyName)?.PropertyType);

        Assert.Equal(10, responseType.GetProperties().Length);
    }

    [Fact]
    public void Invoice_validation_accepts_zero_and_99_quantity_values()
    {
        var requestType = GetTypeOrFail("Vat.Api.Models.CreateInvoiceRequest");
        var request = Activator.CreateInstance(requestType)!;
        SetValidFields(requestType, request);
        Set(requestType, request, "CashRegister", 0);
        Set(requestType, request, "ThreeCashRegister", 99);

        Assert.Empty(Validate(request));
    }

    private static Type GetTypeOrFail(string typeName)
    {
        var type = ApiAssembly.GetType(typeName);
        Assert.NotNull(type);
        return type!;
    }

    private static void SetValidFields(Type type, object target)
    {
        Set(type, target, "TaxId", "12345678");
        Set(type, target, "ClientShortName", "測試客戶");
        Set(type, target, "ElectronicInvoice", true);
        foreach (var propertyName in QuantityPropertyNames)
            Set(type, target, propertyName, 0);
    }

    private static void Set(Type type, object target, string propertyName, object? value)
    {
        var property = type.GetProperty(propertyName);
        Assert.NotNull(property);
        property!.SetValue(target, value);
    }

    private static List<ValidationResult> Validate(object instance)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(
            instance,
            new ValidationContext(instance),
            results,
            validateAllProperties: true);
        return results;
    }
}
