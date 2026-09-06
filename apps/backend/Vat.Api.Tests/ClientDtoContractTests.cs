using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Vat.Api.Tests;

public sealed class ClientDtoContractTests
{
    private static readonly Assembly ApiAssembly =
        typeof(Vat.Api.Controllers.HealthController).Assembly;

    [Fact]
    public void Client_controller_uses_the_expected_resource_route()
    {
        var controllerType = ApiAssembly.GetType("Vat.Api.Controllers.ClientsController");

        Assert.NotNull(controllerType);
        var route = controllerType!.GetCustomAttribute<RouteAttribute>();
        Assert.Equal("VAT_API/clients", route?.Template);
    }

    [Fact]
    public void Client_and_health_controllers_use_the_VAT_API_prefix()
    {
        var employeeController = ApiAssembly.GetType("Vat.Api.Controllers.EmployeesController");
        var healthController = ApiAssembly.GetType("Vat.Api.Controllers.HealthController");

        Assert.NotNull(employeeController);
        Assert.NotNull(healthController);
        Assert.Equal(
            "VAT_API/employees",
            employeeController!.GetCustomAttribute<RouteAttribute>()?.Template);
        Assert.Equal(
            "VAT_API/health",
            healthController!.GetCustomAttribute<RouteAttribute>()?.Template);
    }

    [Fact]
    public void Client_create_request_requires_all_client_fields()
    {
        var requestType = GetTypeOrFail("Vat.Api.Models.CreateClientRequest");
        var request = Activator.CreateInstance(requestType)!;

        var errors = Validate(request);

        Assert.Contains(errors, error => error.MemberNames.Contains("TaxId"));
        Assert.Contains(errors, error => error.MemberNames.Contains("FullName"));
        Assert.Contains(errors, error => error.MemberNames.Contains("ShortName"));
        Assert.Contains(errors, error => error.MemberNames.Contains("ResponsiblePerson"));
        Assert.Contains(errors, error => error.MemberNames.Contains("Address"));
    }

    [Fact]
    public void Valid_client_create_and_update_requests_pass_validation()
    {
        var createType = GetTypeOrFail("Vat.Api.Models.CreateClientRequest");
        var createRequest = Activator.CreateInstance(createType)!;
        SetValidFields(createType, createRequest);

        var updateType = GetTypeOrFail("Vat.Api.Models.UpdateClientRequest");
        var updateRequest = Activator.CreateInstance(updateType)!;
        SetValidFields(updateType, updateRequest);

        Assert.Empty(Validate(createRequest));
        Assert.Empty(Validate(updateRequest));
    }

    [Fact]
    public void Client_validation_rejects_invalid_tax_ids_and_whitespace_fields()
    {
        var requestType = GetTypeOrFail("Vat.Api.Models.CreateClientRequest");
        var invalidTaxIdRequest = Activator.CreateInstance(requestType)!;
        SetValidFields(requestType, invalidTaxIdRequest);
        Set(requestType, invalidTaxIdRequest, "TaxId", "1234567A");

        var invalidTaxIdErrors = Validate(invalidTaxIdRequest);
        Assert.Contains(invalidTaxIdErrors, error => error.MemberNames.Contains("TaxId"));

        var whitespaceNameRequest = Activator.CreateInstance(requestType)!;
        SetValidFields(requestType, whitespaceNameRequest);
        Set(requestType, whitespaceNameRequest, "FullName", "   ");

        var whitespaceNameErrors = Validate(whitespaceNameRequest);
        Assert.True(
            whitespaceNameErrors.Any(error =>
                error.ErrorMessage == "客戶全稱為必填欄位。"
                || error.MemberNames.Contains("FullName")),
            string.Join(
                " | ",
                whitespaceNameErrors.Select(error =>
                    $"{error.ErrorMessage} [{string.Join(",", error.MemberNames)}]")));
    }

    [Fact]
    public void Client_validation_rejects_values_over_declared_limits()
    {
        var requestType = GetTypeOrFail("Vat.Api.Models.CreateClientRequest");
        var request = Activator.CreateInstance(requestType)!;
        SetValidFields(requestType, request);
        Set(requestType, request, "FullName", new string('全', 101));
        Set(requestType, request, "ShortName", new string('簡', 51));
        Set(requestType, request, "ResponsiblePerson", new string('人', 101));
        Set(requestType, request, "Address", new string('址', 256));

        var errors = Validate(request);

        Assert.Contains(errors, error => error.MemberNames.Contains("FullName"));
        Assert.Contains(errors, error => error.MemberNames.Contains("ShortName"));
        Assert.Contains(errors, error => error.MemberNames.Contains("ResponsiblePerson"));
        Assert.Contains(errors, error => error.MemberNames.Contains("Address"));
    }

    [Fact]
    public void Client_response_contains_only_the_public_client_fields()
    {
        var responseType = GetTypeOrFail("Vat.Api.Models.ClientResponse");

        Assert.Equal(typeof(int), responseType.GetProperty("ClientId")?.PropertyType);
        Assert.Equal(typeof(string), responseType.GetProperty("TaxId")?.PropertyType);
        Assert.Equal(typeof(string), responseType.GetProperty("FullName")?.PropertyType);
        Assert.Equal(typeof(string), responseType.GetProperty("ShortName")?.PropertyType);
        Assert.Equal(typeof(string), responseType.GetProperty("ResponsiblePerson")?.PropertyType);
        Assert.Equal(typeof(string), responseType.GetProperty("Address")?.PropertyType);
        Assert.Null(responseType.GetProperty("Password"));
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
        Set(type, target, "FullName", "測試客戶股份有限公司");
        Set(type, target, "ShortName", "測試客戶");
        Set(type, target, "ResponsiblePerson", "王小明");
        Set(type, target, "Address", "台北市中正區測試路 1 號");
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
