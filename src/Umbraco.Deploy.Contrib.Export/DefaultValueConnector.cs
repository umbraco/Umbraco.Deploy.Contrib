using System;
using System.Collections.Generic;
using System.Globalization;
using Umbraco.Core.Configuration;
using Umbraco.Core.Deploy;
using Umbraco.Core.Models;
using Umbraco.Deploy.Configuration;

namespace UmbracoDeploy.Contrib.Export
{
    public sealed class DefaultValueConnector : IValueConnector
    {
        public IEnumerable<string> PropertyEditorAliases { get; } = new[] { "*" };

        public string GetValue(Property property, ICollection<ArtifactDependency> dependencies)
            => property.Value is IConvertible convertibleValue
            ? convertibleValue.ToString(CultureInfo.InvariantCulture)
            : property.Value?.ToString();

        public void SetValue(IContentBase content, string alias, string value)
            => content.SetValue(alias, value);

        public static void SetDefault()
            => UmbracoConfig.For.DeploySettings().ValueConnectors.Add(new DefaultValueConnectorSetting());

        private sealed class DefaultValueConnectorSetting : IValueConnectorSetting
        {
            public string Alias => "*";

            public string TypeName => typeof(DefaultValueConnector).AssemblyQualifiedName;
        }
    }
}