using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Serialization;
using Umbraco.Deploy.Core.Migrators;
using Umbraco.Deploy.Infrastructure.Artifacts;
using Umbraco.Deploy.Infrastructure.Migrators;
using Umbraco.Extensions;

namespace Umbraco.Deploy.Contrib.Tests.Migrators.Legacy;

/// <summary>
/// Runs legacy (Umbraco 7) data type artifacts through the artifact migrators in the order a site gets them: Contrib's legacy migrators first, then Deploy's default migrators.
/// </summary>
[TestFixture]
internal sealed class LegacyToV19DataTypeMigrationChainTests
{
    // Deploy registers its default artifact migrators in an internal method and most of the v19 split migrators are internal types.
    // The relevant ones are therefore created by full type name, in the order Deploy appends them, so a Deploy rename or removal fails with a clear message.
    private static readonly string[] _deployMigratorTypeNames =
    [
        "Umbraco.Deploy.Infrastructure.Migrators.ReplaceMediaPickerDataTypeArtifactMigrator",
        "Umbraco.Deploy.Infrastructure.Migrators.DefaultLegacyDataTypeConfigurationArtifactMigrator",
        "Umbraco.Deploy.Infrastructure.Migrators.DefaultEditorUiAliasDataTypeArtifactMigrator",
        "Umbraco.Deploy.Infrastructure.Migrators.ReplaceTypedLabelDataTypeArtifactMigrator",
        "Umbraco.Deploy.Infrastructure.Migrators.ReplaceSingleMediaPickerDataTypeArtifactMigrator",
        "Umbraco.Deploy.Infrastructure.Migrators.ReplaceSingleUrlPickerDataTypeArtifactMigrator",
        "Umbraco.Deploy.Infrastructure.Migrators.ReplaceSingleDropDownDataTypeArtifactMigrator",
        "Umbraco.Deploy.Infrastructure.Migrators.MultiUrlPickerValidationLimitDataTypeArtifactMigrator",
    ];

    private static readonly string[] _expectedItems = ["a", "b"];

    private static readonly SemVersion _legacyVersion = new(2, 0, 0);

    private ArtifactMigratorCollection _migrators = null!;

    [SetUp]
    public void SetUp()
    {
        var valueEditor = new Mock<IDataValueEditor>();
        valueEditor.SetupGet(x => x.ValueType).Returns(ValueTypes.Json);

        var ioHelper = Mock.Of<IIOHelper>();
        var propertyEditors = new PropertyEditorCollection(new DataEditorCollection(() =>
        [
            CreateDataEditor(Constants.PropertyEditors.Aliases.MultipleDropDown, new TestConfigurationEditor<DropDownFlexibleConfiguration>(ioHelper), valueEditor.Object),
            CreateDataEditor(Constants.PropertyEditors.Aliases.MediaPicker3, new TestConfigurationEditor<MediaPicker3Configuration>(ioHelper), valueEditor.Object),
            CreateDataEditor(Constants.PropertyEditors.Aliases.MultiUrlPicker, new TestConfigurationEditor<MultiUrlPickerConfiguration>(ioHelper), valueEditor.Object),
            CreateDataEditor(Constants.PropertyEditors.Aliases.Label, new TestConfigurationEditor<EmptyConfiguration>(ioHelper), valueEditor.Object),
        ]));

        var services = new ServiceCollection();
        services.AddSingleton(propertyEditors);
        services.AddSingleton<IConfigurationEditorJsonSerializer>(new SystemTextConfigurationEditorJsonSerializer(new DefaultJsonSerializerEncoderFactory()));
        services.AddSingleton(Mock.Of<IContentTypeService>());
        services.AddSingleton(Mock.Of<IMediaTypeService>());
        services.AddSingleton(Mock.Of<IMemberTypeService>());

        var builder = new ArtifactMigratorCollectionBuilder();
        builder.Append(_deployMigratorTypeNames.Select(GetDeployMigratorType));
        builder.AddLegacyMigrators(); // Inserts at the beginning, like a site's composer does
        builder.RegisterWith(services);

        _migrators = services.BuildServiceProvider().GetRequiredService<ArtifactMigratorCollection>();
    }

    [Test]
    public void Legacy_Single_Dropdown_Ends_Up_As_Single_Dropdown()
    {
        var artifact = Migrate("Umbraco.DropDown", """{"1":"a","2":"b"}""");

        Assert.Multiple(() =>
        {
            Assert.That(artifact.EditorAlias, Is.EqualTo(Constants.PropertyEditors.Aliases.SingleDropDown));
            Assert.That(GetConfiguration(artifact).ContainsKey("multiple"), Is.False);
            Assert.That(GetConfiguration(artifact)["items"]!.AsArray().Select(x => x!.GetValue<string>()), Is.EqualTo(_expectedItems));
        });
    }

    [Test]
    public void Legacy_Multiple_Dropdown_Ends_Up_As_Flexible_Dropdown()
    {
        var artifact = Migrate("Umbraco.DropDownMultiple", """{"1":"a","2":"b"}""");

        Assert.Multiple(() =>
        {
            Assert.That(artifact.EditorAlias, Is.EqualTo(Constants.PropertyEditors.Aliases.MultipleDropDown));
            Assert.That(GetConfiguration(artifact).ContainsKey("multiple"), Is.False);
            Assert.That(GetConfiguration(artifact)["items"]!.AsArray().Select(x => x!.GetValue<string>()), Is.EqualTo(_expectedItems));
        });
    }

    [TestCase("0", Constants.PropertyEditors.Aliases.SingleDropDown)]
    [TestCase("1", Constants.PropertyEditors.Aliases.MultipleDropDown)]
    [TestCase(null, Constants.PropertyEditors.Aliases.MultipleDropDown)]
    public void Legacy_Flexible_Dropdown_Ends_Up_As_Single_Or_Flexible_Dropdown(string? multiple, string expectedEditorAlias)
    {
        var configuration = multiple is null
            ? """{"1":"a","2":"b"}"""
            : $$"""{"multiple":"{{multiple}}","1":"a","2":"b"}""";

        var artifact = Migrate(Constants.PropertyEditors.Aliases.MultipleDropDown, configuration);

        Assert.Multiple(() =>
        {
            Assert.That(artifact.EditorAlias, Is.EqualTo(expectedEditorAlias));
            Assert.That(GetConfiguration(artifact).ContainsKey("multiple"), Is.False);
            Assert.That(GetConfiguration(artifact)["items"]!.AsArray().Select(x => x!.GetValue<string>()), Is.EqualTo(_expectedItems));
        });
    }

    [Test]
    public void Legacy_Single_Media_Picker_Ends_Up_As_Single_Media_Picker()
    {
        var artifact = Migrate("Umbraco.MediaPicker2", """{"onlyImages":"1"}""");

        Assert.Multiple(() =>
        {
            Assert.That(artifact.EditorAlias, Is.EqualTo("Umbraco.MediaPicker.Single"));
            Assert.That(GetConfiguration(artifact).ContainsKey("multiple"), Is.False);
        });
    }

    [Test]
    public void Legacy_Multiple_Media_Picker_Ends_Up_As_Media_Picker()
    {
        var artifact = Migrate("Umbraco.MultipleMediaPicker", """{"onlyImages":"1"}""");

        Assert.Multiple(() =>
        {
            Assert.That(artifact.EditorAlias, Is.EqualTo(Constants.PropertyEditors.Aliases.MediaPicker3));
            Assert.That(GetConfiguration(artifact).ContainsKey("multiple"), Is.False, "Deploy removes the multiple key from multi-value pickers once the single variant is split off.");
        });
    }

    [Test]
    public void Legacy_Related_Links_With_Max_One_Ends_Up_As_Single_Url_Picker()
    {
        var artifact = Migrate("Umbraco.RelatedLinks2", """{"maxNumber":"1"}""");

        Assert.Multiple(() =>
        {
            Assert.That(artifact.EditorAlias, Is.EqualTo("Umbraco.UrlPicker.Single"));
            Assert.That(GetConfiguration(artifact).ContainsKey("maxNumber"), Is.False);
            Assert.That(GetConfiguration(artifact).ContainsKey("validationLimit"), Is.False);
        });
    }

    [Test]
    public void Legacy_Related_Links_With_Higher_Max_Ends_Up_As_Multi_Url_Picker()
    {
        var artifact = Migrate("Umbraco.RelatedLinks2", """{"maxNumber":"3"}""");

        Assert.Multiple(() =>
        {
            Assert.That(artifact.EditorAlias, Is.EqualTo(Constants.PropertyEditors.Aliases.MultiUrlPicker));
            Assert.That(GetConfiguration(artifact).ContainsKey("maxNumber"), Is.False);
            Assert.That(GetConfiguration(artifact)["validationLimit"]!["max"]!.GetValue<int>(), Is.EqualTo(3));
        });
    }

    [Test]
    public void Legacy_No_Edit_Ends_Up_As_Label()
    {
        var artifact = Migrate("Umbraco.NoEdit", null);

        Assert.That(artifact.EditorAlias, Is.EqualTo(Constants.PropertyEditors.Aliases.Label));
    }

    private static Type GetDeployMigratorType(string fullName)
    {
        var assembly = typeof(ReplaceDataTypeArtifactMigratorBase).Assembly;

        return assembly.GetType(fullName)
            ?? throw new AssertionException($"Deploy migrator type '{fullName}' was not found in {assembly.GetName().Name}. Deploy may have renamed or removed it; update this chain test.");
    }

    private static IDataEditor CreateDataEditor(string alias, IConfigurationEditor configurationEditor, IDataValueEditor valueEditor)
    {
        var dataEditor = new Mock<IDataEditor>();
        dataEditor.SetupGet(x => x.Alias).Returns(alias);
        dataEditor.Setup(x => x.GetConfigurationEditor()).Returns(configurationEditor);
        dataEditor.Setup(x => x.GetValueEditor()).Returns(valueEditor);

        return dataEditor.Object;
    }

    private DataTypeArtifact Migrate(string editorAlias, string? configuration)
    {
        var artifact = new DataTypeArtifact(new GuidUdi(Constants.UdiEntityType.DataType, Guid.NewGuid()))
        {
            EditorAlias = editorAlias,
            Configuration = configuration,
        };

        return (DataTypeArtifact)_migrators.Migrate(typeof(DataTypeArtifact), _legacyVersion, artifact)!;
    }

    private static JsonObject GetConfiguration(DataTypeArtifact artifact)
        => JsonNode.Parse(artifact.Configuration!)!.AsObject();

    private sealed class TestConfigurationEditor<TConfiguration>(IIOHelper ioHelper) : ConfigurationEditor<TConfiguration>(ioHelper)
        where TConfiguration : new()
    { }

    private sealed class EmptyConfiguration
    { }
}
