using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Serialization;
using Umbraco.Deploy.Contrib.Migrators.Legacy;
using Umbraco.Deploy.Core.Migrators;
using Umbraco.Extensions;

namespace Umbraco.Deploy.Contrib.Tests.Migrators.Legacy;

[TestFixture]
internal sealed class DropDownPropertyTypeMigratorTests
{
    private const string PropertyAlias = "dropdown";

    // Added by PrevalueArtifactMigrator to the editor aliases of content artifacts.
    private const string PrevalueEditorAliasPrefix = "MigratePrevalue.";

    private PropertyTypeMigratorCollection _migrators = null!;

    [SetUp]
    public void SetUp()
    {
        var builder = new PropertyTypeMigratorCollectionBuilder();
        builder.AddLegacyMigrators();

        var services = new ServiceCollection();
        services.AddSingleton<IJsonSerializer>(new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory()));
        builder.RegisterWith(services);

        _migrators = services.BuildServiceProvider().GetRequiredService<PropertyTypeMigratorCollection>();
    }

    [TestCase("Umbraco.DropDown")]
    [TestCase("Umbraco.DropDownMultiple")]
    [TestCase("Umbraco.DropdownlistPublishingKeys")]
    [TestCase("Umbraco.DropdownlistMultiplePublishKeys")]
    public Task Legacy_Editor_Is_Migrated_To_Single_Dropdown(string artifactEditorAlias)
        => AssertMigratedAsync(artifactEditorAlias, Constants.PropertyEditors.Aliases.SingleDropDown);

    [Test]
    public Task Prevalue_Prefixed_Flexible_Editor_Is_Migrated_To_Single_Dropdown()
        => AssertMigratedAsync(PrevalueEditorAliasPrefix + Constants.PropertyEditors.Aliases.MultipleDropDown, Constants.PropertyEditors.Aliases.SingleDropDown);

    [TestCase("Umbraco.DropDown")]
    [TestCase("Umbraco.DropDownMultiple")]
    [TestCase("Umbraco.DropdownlistPublishingKeys")]
    [TestCase("Umbraco.DropdownlistMultiplePublishKeys")]
    public Task Legacy_Editor_Is_Migrated_To_Flexible_Dropdown(string artifactEditorAlias)
        => AssertMigratedAsync(artifactEditorAlias, Constants.PropertyEditors.Aliases.MultipleDropDown);

    [Test]
    public Task Prevalue_Prefixed_Flexible_Editor_Is_Migrated_To_Flexible_Dropdown()
        => AssertMigratedAsync(PrevalueEditorAliasPrefix + Constants.PropertyEditors.Aliases.MultipleDropDown, Constants.PropertyEditors.Aliases.MultipleDropDown);

    [TestCase(Constants.PropertyEditors.Aliases.MultipleDropDown)]
    [TestCase(Constants.PropertyEditors.Aliases.CheckBoxList)]
    [TestCase(PrevalueEditorAliasPrefix + Constants.PropertyEditors.Aliases.CheckBoxList)]
    public async Task Other_Artifact_Editor_Is_Not_Migrated_To_Single_Dropdown(string artifactEditorAlias)
    {
        var (success, value) = await TryMigrateAsync(artifactEditorAlias, Constants.PropertyEditors.Aliases.SingleDropDown, "a;;b").ConfigureAwait(false);

        Assert.Multiple(() =>
        {
            Assert.That(success, Is.False);
            Assert.That(value, Is.EqualTo("a;;b"));
        });
    }

    [Test]
    public async Task Single_Dropdown_Empty_Value_Migrates_To_Null()
    {
        var (success, value) = await TryMigrateAsync("Umbraco.DropDown", Constants.PropertyEditors.Aliases.SingleDropDown, ";;").ConfigureAwait(false);

        Assert.Multiple(() =>
        {
            Assert.That(success, Is.True);
            Assert.That(value, Is.Null);
        });
    }

    private async Task AssertMigratedAsync(string artifactEditorAlias, string propertyTypeEditorAlias)
    {
        var (success, value) = await TryMigrateAsync(artifactEditorAlias, propertyTypeEditorAlias, "a;;b").ConfigureAwait(false);

        Assert.Multiple(() =>
        {
            Assert.That(success, Is.True);
            Assert.That(value, Is.EqualTo("""["a","b"]"""));
        });
    }

    private Task<(bool Success, object? Value)> TryMigrateAsync(string artifactEditorAlias, string propertyTypeEditorAlias, string value)
    {
        var propertyType = new Mock<IPropertyType>();
        propertyType.SetupGet(x => x.Alias).Returns(PropertyAlias);
        propertyType.SetupGet(x => x.PropertyEditorAlias).Returns(propertyTypeEditorAlias);

        var propertyEditorAliases = new Dictionary<string, string> { [PropertyAlias] = artifactEditorAlias };

        return _migrators.TryMigrateAsync(propertyType.Object, value, propertyEditorAliases, Mock.Of<IContextCache>());
    }
}
