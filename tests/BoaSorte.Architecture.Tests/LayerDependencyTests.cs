using System.Reflection;
using NetArchTest.Rules;

namespace BoaSorte.Architecture.Tests;

/// <summary>
/// Enforces the dependency rule from specs/constitution.md §4:
/// Api → Application → Domain; Infrastructure implements Application ports.
/// </summary>
public class LayerDependencyTests
{
    private const string DomainNamespace = "BoaSorte.Domain";
    private const string ApplicationNamespace = "BoaSorte.Application";
    private const string InfrastructureNamespace = "BoaSorte.Infrastructure";
    private const string ApiNamespace = "BoaSorte.Api";

    private static readonly Assembly DomainAssembly = typeof(Domain.AssemblyMarker).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(Application.AssemblyMarker).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(Infrastructure.AssemblyMarker).Assembly;

    [Fact]
    public void Domain_does_not_depend_on_other_layers_or_frameworks()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                ApplicationNamespace,
                InfrastructureNamespace,
                ApiNamespace,
                "Microsoft.EntityFrameworkCore",
                "Microsoft.AspNetCore",
                "Microsoft.Extensions")
            .GetResult();

        AssertSuccessful(result);
    }

    [Fact]
    public void Domain_assembly_references_only_the_base_library()
    {
        var references = DomainAssembly.GetReferencedAssemblies()
            .Select(a => a.Name!)
            .Where(name => !name.StartsWith("System", StringComparison.Ordinal) && name != "netstandard")
            .ToList();

        Assert.Empty(references);
    }

    [Fact]
    public void Application_does_not_depend_on_infrastructure_api_or_frameworks()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                InfrastructureNamespace,
                ApiNamespace,
                "Microsoft.EntityFrameworkCore",
                "Microsoft.AspNetCore",
                "Npgsql")
            .GetResult();

        AssertSuccessful(result);
    }

    [Fact]
    public void Infrastructure_does_not_depend_on_api()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        AssertSuccessful(result);
    }

    [Fact]
    public void Domain_types_have_no_public_mutable_setters()
    {
        var offenders = DomainAssembly.GetExportedTypes()
            .SelectMany(type => type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .Where(property => property.SetMethod is { IsPublic: true } setter && !IsInitOnly(setter))
                .Select(property => $"{type.Name}.{property.Name}"))
            .ToList();

        Assert.Empty(offenders);
    }

    private static bool IsInitOnly(MethodInfo setter) =>
        setter.ReturnParameter.GetRequiredCustomModifiers()
            .Any(modifier => modifier.FullName == "System.Runtime.CompilerServices.IsExternalInit");

    private static void AssertSuccessful(NetArchTest.Rules.TestResult result)
    {
        var offenders = result.FailingTypeNames ?? [];
        Assert.True(result.IsSuccessful, $"Violations: {string.Join(", ", offenders)}");
    }
}
