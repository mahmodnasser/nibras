using System.Reflection;
using System.Runtime.CompilerServices;
using Nibras.Contracts.ServiceName;
using Shouldly;
using Xunit;

namespace Nibras.ServiceName.ContractTests;

/// <summary>
/// TC-TST-112 for this service's contract assembly: every public type is a record, an enum, or a static
/// class of string constants, and the assembly references no Nibras project except Nibras.Contracts.Shared.
/// </summary>
[Trait("TestCase", "TC-TST-112")]
public sealed class ContractShapeTests
{
    private static readonly Assembly Contract = typeof(RoutingKeys).Assembly;

    [Fact]
    public void Every_public_type_is_data_only()
    {
        var offenders = Contract.GetExportedTypes().Where(t => !IsDataOnly(t)).Select(t => t.FullName).ToList();

        offenders.ShouldBeEmpty();
    }

    [Fact]
    public void The_contract_references_no_nibras_project_except_contracts_shared()
    {
        var references = Contract.GetReferencedAssemblies()
            .Select(a => a.Name!)
            .Where(name => name.StartsWith("Nibras.", StringComparison.Ordinal))
            .ToList();

        references.ShouldAllBe(name => name == "Nibras.Contracts.Shared");
    }

    private static bool IsDataOnly(Type type)
    {
        if (type.IsEnum)
        {
            return true;
        }

        if (type is { IsAbstract: true, IsSealed: true })
        {
            // A static class: only string constants.
            return type.GetFields(BindingFlags.Public | BindingFlags.Static).All(f => f.IsLiteral && f.FieldType == typeof(string))
                && type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Length == 0;
        }

        // A record has a compiler-generated EqualityContract.
        return type.GetProperty("EqualityContract", BindingFlags.NonPublic | BindingFlags.Instance)?
            .GetMethod?.GetCustomAttribute<CompilerGeneratedAttribute>() is not null;
    }
}
