using System.Text.Json.Nodes;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Serialization;
using Umbraco.Deploy.Contrib.Migrators.Legacy;
using Umbraco.Deploy.Core.Migrators;
using Umbraco.Deploy.Infrastructure.Artifacts;

namespace Umbraco.Deploy.Contrib.Tests.Migrators.Legacy;

[TestFixture]
internal sealed class DropDownDataTypeArtifactMigratorTests
{
    private IConfigurationEditorJsonSerializer _serializer = null!;
    private PropertyEditorCollection _propertyEditors = null!;

    [SetUp]
    public void SetUp()
    {
        _serializer = new SystemTextConfigurationEditorJsonSerializer(new DefaultJsonSerializerEncoderFactory());

        var valueEditor = new Mock<IDataValueEditor>();
        valueEditor.SetupGet(x => x.ValueType).Returns(ValueTypes.Json);

        var dataEditor = new Mock<IDataEditor>();
        dataEditor.SetupGet(x => x.Alias).Returns(Constants.PropertyEditors.Aliases.MultipleDropDown);
        dataEditor.Setup(x => x.GetConfigurationEditor()).Returns(() => new TestConfigurationEditor(Mock.Of<IIOHelper>()));
        dataEditor.Setup(x => x.GetValueEditor()).Returns(valueEditor.Object);

        _propertyEditors = new PropertyEditorCollection(new DataEditorCollection(() => [dataEditor.Object]));
    }

    [Test]
    public void Legacy_Single_Dropdown_Writes_Multiple_False()
    {
        var migrator = new DropDownDataTypeArtifactMigrator(_propertyEditors, _serializer);

        var artifact = Migrate(migrator, "Umbraco.DropDown", """{"1":"a","2":"b"}""");

        Assert.Multiple(() =>
        {
            Assert.That(artifact.EditorAlias, Is.EqualTo(Constants.PropertyEditors.Aliases.MultipleDropDown));
            Assert.That(GetMultiple(artifact), Is.False);
            Assert.That(GetItems(artifact), Is.EqualTo("a,b"));
        });
    }

    [Test]
    public void Legacy_Multiple_Dropdown_Writes_Multiple_True()
    {
        var migrator = new DropDownMultipleDataTypeArtifactMigrator(_propertyEditors, _serializer);

        var artifact = Migrate(migrator, "Umbraco.DropDownMultiple", """{"1":"a","2":"b"}""");

        Assert.Multiple(() =>
        {
            Assert.That(artifact.EditorAlias, Is.EqualTo(Constants.PropertyEditors.Aliases.MultipleDropDown));
            Assert.That(GetMultiple(artifact), Is.True);
            Assert.That(GetItems(artifact), Is.EqualTo("a,b"));
        });
    }

    [TestCase("1", true)]
    [TestCase("0", false)]
    public void Legacy_Single_Dropdown_Source_Multiple_Overrides_Default(string sourceMultiple, bool expected)
    {
        var migrator = new DropDownDataTypeArtifactMigrator(_propertyEditors, _serializer);

        var artifact = Migrate(migrator, "Umbraco.DropDown", $$"""{"multiple":"{{sourceMultiple}}","1":"a"}""");

        Assert.That(GetMultiple(artifact), Is.EqualTo(expected));
    }

    [TestCase("1", true)]
    [TestCase("0", false)]
    public void Legacy_Multiple_Dropdown_Source_Multiple_Overrides_Default(string sourceMultiple, bool expected)
    {
        var migrator = new DropDownMultipleDataTypeArtifactMigrator(_propertyEditors, _serializer);

        var artifact = Migrate(migrator, "Umbraco.DropDownMultiple", $$"""{"multiple":"{{sourceMultiple}}","1":"a"}""");

        Assert.That(GetMultiple(artifact), Is.EqualTo(expected));
    }

    [Test]
    public void Legacy_Dropdown_Without_Configuration_Writes_Default_Multiple()
    {
        var single = Migrate(new DropDownDataTypeArtifactMigrator(_propertyEditors, _serializer), "Umbraco.DropDown", null);
        var multiple = Migrate(new DropDownMultipleDataTypeArtifactMigrator(_propertyEditors, _serializer), "Umbraco.DropDownMultiple", null);

        Assert.Multiple(() =>
        {
            Assert.That(GetMultiple(single), Is.False);
            Assert.That(GetMultiple(multiple), Is.True);
        });
    }

    [Test]
    public void Flexible_Dropdown_Defaults_To_Multiple_True()
    {
        var migrator = new DropDownFlexibleDataTypeArtifactMigrator(_propertyEditors, _serializer);

        var artifact = Migrate(migrator, Constants.PropertyEditors.Aliases.MultipleDropDown, """{"1":"a"}""");

        Assert.Multiple(() =>
        {
            Assert.That(artifact.EditorAlias, Is.EqualTo(Constants.PropertyEditors.Aliases.MultipleDropDown));
            Assert.That(GetMultiple(artifact), Is.True);
            Assert.That(GetItems(artifact), Is.EqualTo("a"));
        });
    }

    [TestCase("1", true)]
    [TestCase("0", false)]
    public void Flexible_Dropdown_Source_Multiple_Overrides_Default(string sourceMultiple, bool expected)
    {
        var migrator = new DropDownFlexibleDataTypeArtifactMigrator(_propertyEditors, _serializer);

        var artifact = Migrate(migrator, Constants.PropertyEditors.Aliases.MultipleDropDown, $$"""{"multiple":"{{sourceMultiple}}","1":"a"}""");

        Assert.That(GetMultiple(artifact), Is.EqualTo(expected));
    }

    private static DataTypeArtifact Migrate(IArtifactMigrator migrator, string editorAlias, string? configuration)
    {
        var artifact = new DataTypeArtifact(new GuidUdi(Constants.UdiEntityType.DataType, Guid.NewGuid()))
        {
            EditorAlias = editorAlias,
            Configuration = configuration,
        };

        return (DataTypeArtifact)migrator.Migrate(artifact)!;
    }

    private static JsonObject GetConfiguration(DataTypeArtifact artifact)
        => JsonNode.Parse(artifact.Configuration!)!.AsObject();

    private static bool GetMultiple(DataTypeArtifact artifact)
    {
        var node = GetConfiguration(artifact)["multiple"];
        Assert.That(node, Is.Not.Null, "The 'multiple' key must be written explicitly.");
        Assert.That(node!.GetValueKind(), Is.AnyOf(System.Text.Json.JsonValueKind.True, System.Text.Json.JsonValueKind.False), "The 'multiple' value must be a JSON boolean.");

        return node.GetValue<bool>();
    }

    private static string GetItems(DataTypeArtifact artifact)
        => GetConfiguration(artifact)["items"]!.AsArray().Select(x => x!.GetValue<string>()).Aggregate((x, y) => $"{x},{y}");

    private sealed class TestConfigurationEditor(IIOHelper ioHelper) : ConfigurationEditor<DropDownFlexibleConfiguration>(ioHelper)
    { }
}
