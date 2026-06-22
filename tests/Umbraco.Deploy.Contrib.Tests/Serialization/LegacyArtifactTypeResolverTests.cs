using System.Reflection;
using NUnit.Framework;
using Umbraco.Cms.Core.Semver;
using Umbraco.Deploy.Contrib.Connectors.Serialization;

namespace Umbraco.Deploy.Contrib.Tests.Serialization;

[TestFixture]
internal class LegacyArtifactTypeResolverTests
{
    private static readonly MethodInfo _resolveTypeNameMethod = typeof(LegacyArtifactTypeResolver)
        .GetMethod("ResolveTypeName", BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException("Could not find the protected ResolveTypeName method.");

    private static string ResolveTypeName(LegacyArtifactTypeResolver resolver, string typeName)
        => (string)_resolveTypeNameMethod.Invoke(resolver, [typeName])!;

    // The legacy switch groups the artifacts under .Content/.ContentType; the base resolver then
    // rewrites the "Umbraco.Deploy.Artifacts" namespace to "Umbraco.Deploy.Infrastructure.Artifacts".
    // These cases pin the full end-to-end resolution from a v7 (pre-v4) type name to the current type.
    [TestCase("Umbraco.Deploy.Artifacts.DocumentArtifact", "Umbraco.Deploy.Infrastructure.Artifacts.Content.DocumentArtifact")]
    [TestCase("Umbraco.Deploy.Artifacts.MediaArtifact", "Umbraco.Deploy.Infrastructure.Artifacts.Content.MediaArtifact")]
    [TestCase("Umbraco.Deploy.Artifacts.MemberArtifact", "Umbraco.Deploy.Infrastructure.Artifacts.Content.MemberArtifact")]
    [TestCase("Umbraco.Deploy.Artifacts.DocumentTypeArtifact", "Umbraco.Deploy.Infrastructure.Artifacts.ContentType.DocumentTypeArtifact")]
    [TestCase("Umbraco.Deploy.Artifacts.MediaTypeArtifact", "Umbraco.Deploy.Infrastructure.Artifacts.ContentType.MediaTypeArtifact")]
    [TestCase("Umbraco.Deploy.Artifacts.MemberTypeArtifact", "Umbraco.Deploy.Infrastructure.Artifacts.ContentType.MemberTypeArtifact")]
    [TestCase("Umbraco.Deploy.Artifacts.RelationTypeArtifact", "Umbraco.Deploy.Infrastructure.Artifacts.RelationTypeArtifact")]
    public void ResolveTypeName_MapsLegacyContentTypeNames(string legacyTypeName, string expectedTypeName)
    {
        var resolver = new LegacyArtifactTypeResolver();

        var result = ResolveTypeName(resolver, legacyTypeName);

        Assert.That(result, Is.EqualTo(expectedTypeName));
    }

    [Test]
    public void ResolveTypeName_PassesThroughUnknownTypeName()
    {
        var resolver = new LegacyArtifactTypeResolver();

        var result = ResolveTypeName(resolver, "My.Custom.Namespace.SomeArtifact");

        Assert.That(result, Is.EqualTo("My.Custom.Namespace.SomeArtifact"));
    }

    [TestCase(1, 0, 0)]
    [TestCase(2, 0, 0)]
    [TestCase(2, 9, 9)]
    public void CanResolve_ReturnsTrue_ForVersionsBelowMaxVersion(int major, int minor, int patch)
    {
        var resolver = new LegacyArtifactTypeResolver();

        Assert.That(resolver.CanResolve(new SemVersion(major, minor, patch)), Is.True);
    }

    // MaxVersion (3.0.0) is an exclusive upper bound.
    [TestCase(3, 0, 0)]
    [TestCase(4, 0, 0)]
    [TestCase(7, 0, 0)]
    public void CanResolve_ReturnsFalse_ForVersionsAtOrAboveMaxVersion(int major, int minor, int patch)
    {
        var resolver = new LegacyArtifactTypeResolver();

        Assert.That(resolver.CanResolve(new SemVersion(major, minor, patch)), Is.False);
    }
}
