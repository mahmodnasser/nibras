using System.Reflection;
using Nibras.BuildingBlocks.Testing.Isolation;
using Shouldly;
using Xunit;

namespace Nibras.TenantIsolation.Tests.Generator;

/// <summary>
/// The generator and its gate (document 16 part 3.1): every service in the build has a target, every surface of every
/// target has a generated test, and no generated class outlives its target. With <c>NIBRAS_ISOLATION_REGENERATE</c> set
/// to the Generated folder (<c>tools/isolation/regenerate.*</c>) it rewrites the classes instead of comparing them.
/// <c>Service=all</c> keeps the gate in a service scope's filtered run (<c>tools/ci/ci.mjs</c>, <c>generatedArgs</c>).
/// </summary>
[Trait("Service", "all")]
public sealed class SurfaceCoverageTests
{
    public const string RegenerateVariable = "NIBRAS_ISOLATION_REGENERATE";

    private const string Regenerate = "run tools/isolation/regenerate.sh or regenerate.ps1, review the diff and commit it";

    private static readonly Assembly Suite = typeof(SurfaceCoverageTests).Assembly;

    public static TheoryData<string> TargetNames() => [.. TargetTypes().Select(t => t.FullName!)];

    [Fact]
    public void Every_service_api_in_the_build_has_an_isolation_target()
    {
        var targeted = TargetTypes().Select(t => New(t).Service).ToHashSet(StringComparer.Ordinal);

        var untargeted = Directory.EnumerateFiles(AppContext.BaseDirectory, "Nibras.*.Api.dll")
            .Select(file => Path.GetFileNameWithoutExtension(file)["Nibras.".Length..^".Api".Length])
            .Where(service => !targeted.Contains(service))
            .Order(StringComparer.Ordinal);

        untargeted.ShouldBeEmpty("every service needs a target in tests/TenantIsolation.Tests/Targets/ saying how its host starts and how a tenant is seeded");
    }

    [Theory]
    [MemberData(nameof(TargetNames))]
    public async Task Every_surface_of_a_target_has_a_generated_test(string targetType)
    {
        var type = Suite.GetType(targetType, throwOnError: true)!;
        var target = New(type);
        var surfaces = (await IsolationSurfaces.DiscoverAsync(target, TestContext.Current.CancellationToken)).Select(s => s.Name).ToList();

        if (Environment.GetEnvironmentVariable(RegenerateVariable) is { Length: > 0 } folder)
        {
            var reference = type.FullName!["Nibras.TenantIsolation.Tests.".Length..];
            await File.WriteAllTextAsync(
                Path.Combine(folder, IsolationGenerator.FileName(target.Service)),
                IsolationGenerator.Emit(target.Service, reference, surfaces),
                TestContext.Current.CancellationToken);
            return;
        }

        var generated = Suite.GetType(IsolationGenerator.Namespace + "." + IsolationGenerator.ClassName(target.Service));
        generated.ShouldNotBeNull($"{target.Service} has no generated isolation tests; {Regenerate}");
        var listed = (string[])generated.GetField("Surfaces")!.GetValue(null)!;
        listed.ShouldBe(surfaces, ignoreOrder: false, customMessage: $"{target.Service}'s endpoints or consumers changed since its isolation tests were generated; {Regenerate}");
    }

    [Fact]
    public void Every_generated_class_belongs_to_a_target()
    {
        var expected = TargetTypes().Select(t => IsolationGenerator.ClassName(New(t).Service)).ToHashSet(StringComparer.Ordinal);

        var orphans = Suite.GetTypes()
            .Where(t => t is { IsClass: true, IsNested: false, Namespace: IsolationGenerator.Namespace } && !expected.Contains(t.Name))
            .Select(t => t.Name);

        orphans.ShouldBeEmpty($"a generated class whose service has no target any more; {Regenerate} after deleting it");
    }

    private static IsolationTarget New(Type type) => (IsolationTarget)Activator.CreateInstance(type)!;

    private static IEnumerable<Type> TargetTypes() =>
        Suite.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IsolationTarget).IsAssignableFrom(t) && t.GetConstructor(Type.EmptyTypes) is not null)
            .OrderBy(t => t.FullName, StringComparer.Ordinal);
}
