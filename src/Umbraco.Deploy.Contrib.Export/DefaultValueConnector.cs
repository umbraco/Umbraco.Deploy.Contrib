using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Umbraco.Core.Configuration;
using Umbraco.Core.Deploy;
using Umbraco.Core.Models;
using Umbraco.Deploy.Configuration;

namespace UmbracoDeploy.Contrib.Export
{
    /// <summary>
    /// Implements a default value connector that uses the property storage type.
    /// </summary>
    public sealed class DefaultValueConnector : IValueConnector
    {
        /// <inheritdoc />
        public IEnumerable<string> PropertyEditorAliases { get; } = new[] { "*" };

        /// <inheritdoc />
        public string GetValue(Property property, ICollection<ArtifactDependency> dependencies)
        {
            var value = property.Value;
            if (value == null)
            {
                return null;
            }

            // Use value storage type to determine the best conversion to artifact value
            switch (GetDataTypeDatabaseType(property.PropertyType))
            {
                // Use ISO 8601 date
                case DataTypeDatabaseType.Date:
                    if (value is DateTime dateValue)
                    {
                        return dateValue.ToString("o", DateTimeFormatInfo.InvariantInfo);
                    }
                    break;
                // Use invariant format (dots as decimal place/no separators) for numbers
                case DataTypeDatabaseType.Decimal:
                    if (value is decimal decimalValue)
                    {
                        return decimalValue.ToString(NumberFormatInfo.InvariantInfo);
                    }
                    break;
                case DataTypeDatabaseType.Integer:
                    if (value is int intValue)
                    {
                        return intValue.ToString(NumberFormatInfo.InvariantInfo);
                    }
                    break;
            }

            // Keep strings as-is
            if (value is string stringValue)
            {
                return stringValue;
            }

            // Try to convert to string using the invariant culture
            if (value is IConvertible convertibleValue)
            {
                return convertibleValue.ToString(CultureInfo.InvariantCulture);
            }

            // Finally, simply convert to string
            return value.ToString();
        }

        /// <inheritdoc />
        public void SetValue(IContentBase content, string alias, string value)
        {
            // Use value storage type to determine the best conversion to property value
            switch (GetDataTypeDatabaseType(content.Properties[alias].PropertyType))
            {
                // In some (most) cases, depending on database and/or system settings, Umbraco will output a DateTime as a UTC formatted string,
                // although the value is actually a non-UTC date ('Z' has been appended).
                // We need to ensure that even if a DateTime inside an artifact looks like a UTC value, it will be parsed as if it wasn't.
                case DataTypeDatabaseType.Date:
                    if (DateTime.TryParseExact(value, "o", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.RoundtripKind, out DateTime dateValue))
                    {
                        content.SetValue(alias, dateValue);
                        return;
                    }
                    break;
                case DataTypeDatabaseType.Decimal:
                    if (decimal.TryParse(value, NumberStyles.Number, NumberFormatInfo.InvariantInfo, out decimal decimalValue))
                    {
                        content.SetValue(alias, decimalValue);
                        return;
                    }
                    break;
                case DataTypeDatabaseType.Integer:
                    if (int.TryParse(value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out int intValue))
                    {
                        content.SetValue(alias, intValue);
                        return;
                    }
                    break;
            }

            // Simply keep the string
            content.SetValue(alias, value);
        }

        private DataTypeDatabaseType GetDataTypeDatabaseType(PropertyType propertyType)
            => (DataTypeDatabaseType)propertyType.GetType().GetProperty("DataTypeDatabaseType", BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.NonPublic).GetValue(propertyType);

        public static void SetDefault()
            => UmbracoConfig.For.DeploySettings().ValueConnectors.Add(new DefaultValueConnectorSetting());

        private sealed class DefaultValueConnectorSetting : IValueConnectorSetting
        {
            public string Alias => "*";

            public string TypeName => typeof(DefaultValueConnector).AssemblyQualifiedName;
        }
    }
}
