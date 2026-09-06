using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Vat.Api.Tests;

public sealed class EmployeeDtoContractTests
{
    private static readonly Assembly ApiAssembly =
        typeof(Vat.Api.Controllers.HealthController).Assembly;

    [Fact]
    public void Employee_controller_uses_the_expected_resource_route()
    {
        var controllerType = ApiAssembly.GetType("Vat.Api.Controllers.EmployeesController");

        Assert.NotNull(controllerType);
        var route = controllerType!.GetCustomAttribute<RouteAttribute>();
        Assert.Equal("VAT_API/employees", route?.Template);
    }

    [Fact]
    public void Create_request_requires_password_and_employee_fields()
    {
        var requestType = GetTypeOrFail("Vat.Api.Models.CreateEmployeeRequest");
        var request = Activator.CreateInstance(requestType)!;

        Set(requestType, request, "Name", "王小明");
        Set(requestType, request, "ShortName", "小明");
        Set(requestType, request, "Gender", "M");
        Set(requestType, request, "NationalId", "A123456789");

        var errors = Validate(request);

        Assert.Contains(errors, error => error.MemberNames.Contains("Password"));
    }

    [Fact]
    public void Update_request_allows_a_blank_password_to_preserve_the_existing_value()
    {
        var requestType = GetTypeOrFail("Vat.Api.Models.UpdateEmployeeRequest");
        var request = Activator.CreateInstance(requestType)!;

        Set(requestType, request, "Name", "王小明");
        Set(requestType, request, "ShortName", "小明");
        Set(requestType, request, "Gender", "M");
        Set(requestType, request, "NationalId", "A123456789");
        Set(requestType, request, "Password", null);

        Assert.Empty(Validate(request));
    }

    [Fact]
    public void Employee_response_does_not_expose_password()
    {
        var responseType = GetTypeOrFail("Vat.Api.Models.EmployeeResponse");

        Assert.Null(responseType.GetProperty("Password"));
    }

    [Fact]
    public void Employee_requests_and_response_include_optional_contact_details()
    {
        var requestType = GetTypeOrFail("Vat.Api.Models.CreateEmployeeRequest");
        var responseType = GetTypeOrFail("Vat.Api.Models.EmployeeResponse");

        Assert.Equal(typeof(string), requestType.GetProperty("ContactPhone")?.PropertyType);
        Assert.Equal(typeof(string), requestType.GetProperty("Address")?.PropertyType);
        Assert.Equal(typeof(DateOnly?), requestType.GetProperty("BirthDate")?.PropertyType);
        Assert.Equal(typeof(string), responseType.GetProperty("ContactPhone")?.PropertyType);
        Assert.Equal(typeof(string), responseType.GetProperty("Address")?.PropertyType);
        Assert.Equal(typeof(DateOnly?), responseType.GetProperty("BirthDate")?.PropertyType);
    }

    [Fact]
    public void Optional_contact_details_allow_null_values()
    {
        var requestType = GetTypeOrFail("Vat.Api.Models.CreateEmployeeRequest");
        var request = Activator.CreateInstance(requestType)!;

        Set(requestType, request, "Name", "王小明");
        Set(requestType, request, "ShortName", "小明");
        Set(requestType, request, "Gender", "M");
        Set(requestType, request, "NationalId", "A123456789");
        Set(requestType, request, "Password", "temporary-password");
        Set(requestType, request, "ContactPhone", null);
        Set(requestType, request, "Address", null);
        Set(requestType, request, "BirthDate", null);

        Assert.Empty(Validate(request));
    }

    [Fact]
    public void Contact_details_reject_values_over_the_declared_limits()
    {
        var requestType = GetTypeOrFail("Vat.Api.Models.CreateEmployeeRequest");
        var request = Activator.CreateInstance(requestType)!;

        Set(requestType, request, "Name", "王小明");
        Set(requestType, request, "ShortName", "小明");
        Set(requestType, request, "Gender", "M");
        Set(requestType, request, "NationalId", "A123456789");
        Set(requestType, request, "Password", "temporary-password");
        Set(requestType, request, "ContactPhone", new string('0', 31));
        Set(requestType, request, "Address", new string('台', 256));

        var errors = Validate(request);

        Assert.Contains(errors, error => error.MemberNames.Contains("ContactPhone"));
        Assert.Contains(errors, error => error.MemberNames.Contains("Address"));
    }

    private static Type GetTypeOrFail(string typeName)
    {
        var type = ApiAssembly.GetType(typeName);
        Assert.NotNull(type);
        return type!;
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
