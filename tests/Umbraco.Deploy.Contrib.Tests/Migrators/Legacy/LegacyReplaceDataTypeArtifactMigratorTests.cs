using NUnit.Framework;
using Umbraco.Deploy.Contrib.Migrators.Legacy;

namespace Umbraco.Deploy.Contrib.Tests.Migrators.Legacy;

[TestFixture]
internal sealed class LegacyReplaceDataTypeArtifactMigratorTests
{
    [TestCase("1", true)]
    [TestCase("true", true)]
    [TestCase("True", true)]
    [TestCase("TRUE", true)]
    [TestCase("0", false)]
    [TestCase("anything-else", false)]
    public void ReplaceIntegerWithBoolean_ConvertsStringValue(string value, bool expected)
    {
        IDictionary<string, object> configuration = new Dictionary<string, object> { ["key"] = value };

        TestMigrator.ReplaceIntegerWithBoolean(ref configuration, "key");

        Assert.That(configuration["key"], Is.EqualTo(expected));
    }

    [Test]
    public void ReplaceIntegerWithBoolean_LeavesExistingBooleanUnchanged()
    {
        IDictionary<string, object> configuration = new Dictionary<string, object> { ["key"] = true };

        TestMigrator.ReplaceIntegerWithBoolean(ref configuration, "key");

        Assert.That(configuration["key"], Is.EqualTo(true));
    }

    [Test]
    public void ReplaceIntegerWithBoolean_IsNoOpForMissingKey()
    {
        IDictionary<string, object> configuration = new Dictionary<string, object>();

        TestMigrator.ReplaceIntegerWithBoolean(ref configuration, "key");

        Assert.That(configuration, Does.Not.ContainKey("key"));
    }

    [Test]
    public void ReplaceStringWithInteger_ConvertsNumericString()
    {
        IDictionary<string, object> configuration = new Dictionary<string, object> { ["key"] = "42" };

        TestMigrator.ReplaceStringWithInteger(ref configuration, "key");

        Assert.That(configuration["key"], Is.EqualTo(42));
    }

    [Test]
    public void ReplaceStringWithInteger_LeavesExistingIntegerUnchanged()
    {
        IDictionary<string, object> configuration = new Dictionary<string, object> { ["key"] = 7 };

        TestMigrator.ReplaceStringWithInteger(ref configuration, "key");

        Assert.That(configuration["key"], Is.EqualTo(7));
    }

    [Test]
    public void ReplaceStringWithInteger_RemovesInvalidValue_WhenNotKeepingInvalid()
    {
        IDictionary<string, object> configuration = new Dictionary<string, object> { ["key"] = "not-a-number" };

        TestMigrator.ReplaceStringWithInteger(ref configuration, "key");

        Assert.That(configuration, Does.Not.ContainKey("key"));
    }

    [Test]
    public void ReplaceStringWithInteger_KeepsInvalidValue_WhenKeepingInvalid()
    {
        IDictionary<string, object> configuration = new Dictionary<string, object> { ["key"] = "not-a-number" };

        TestMigrator.ReplaceStringWithInteger(ref configuration, "key", keepInvalid: true);

        Assert.That(configuration["key"], Is.EqualTo("not-a-number"));
    }

    [Test]
    public void ReplaceKey_RenamesKeyPreservingValue()
    {
        IDictionary<string, object> configuration = new Dictionary<string, object> { ["oldKey"] = "value" };

        TestMigrator.ReplaceKey(ref configuration, "oldKey", "newKey");

        Assert.Multiple(() =>
        {
            Assert.That(configuration, Does.Not.ContainKey("oldKey"));
            Assert.That(configuration["newKey"], Is.EqualTo("value"));
        });
    }

    [Test]
    public void ReplaceKey_IsNoOpForMissingKey()
    {
        IDictionary<string, object> configuration = new Dictionary<string, object>();

        TestMigrator.ReplaceKey(ref configuration, "oldKey", "newKey");

        Assert.That(configuration, Is.Empty);
    }

    [Test]
    public void ReplaceAliasesWithKeys_ResolvesAliasesAndDropsUnresolved()
    {
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        var aliasToKey = new Dictionary<string, Guid?>
        {
            ["a"] = first,
            ["b"] = null, // unresolved -> dropped
            ["c"] = second,
        };

        IDictionary<string, object> configuration = new Dictionary<string, object> { ["key"] = "a,b,c" };

        TestMigrator.ReplaceAliasesWithKeys(ref configuration, "key", alias => aliasToKey.GetValueOrDefault(alias));

        Assert.That(configuration["key"], Is.EqualTo($"{first},{second}"));
    }

    /// <summary>
    /// Exposes the <c>protected static</c> helpers of <see cref="LegacyReplaceDataTypeArtifactMigratorBase" />
    /// for testing. Declared <c>abstract</c> and never instantiated, so the base constructor (and its
    /// dependencies) is never invoked.
    /// </summary>
    private abstract class TestMigrator : LegacyReplaceDataTypeArtifactMigratorBase
    {
        protected TestMigrator()
            : base("from", "to", "toUi", null!, null!)
        {
        }

        public static new void ReplaceKey(ref IDictionary<string, object> configuration, string oldKey, string newKey)
            => LegacyReplaceDataTypeArtifactMigratorBase.ReplaceKey(ref configuration, oldKey, newKey);

        public static new void ReplaceIntegerWithBoolean(ref IDictionary<string, object> configuration, string key)
            => LegacyReplaceDataTypeArtifactMigratorBase.ReplaceIntegerWithBoolean(ref configuration, key);

        public static new void ReplaceStringWithInteger(ref IDictionary<string, object> configuration, string key, bool keepInvalid = false)
            => LegacyReplaceDataTypeArtifactMigratorBase.ReplaceStringWithInteger(ref configuration, key, keepInvalid);

        public static new void ReplaceAliasesWithKeys(ref IDictionary<string, object> configuration, string key, Func<string, Guid?> getKeyByAlias)
            => LegacyReplaceDataTypeArtifactMigratorBase.ReplaceAliasesWithKeys(ref configuration, key, getKeyByAlias);
    }
}
