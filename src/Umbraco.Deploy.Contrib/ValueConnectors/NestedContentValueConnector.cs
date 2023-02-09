using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Deploy.Core;
using Umbraco.Deploy.Core.Connectors;
using Umbraco.Deploy.Core.Connectors.ValueConnectors;
using Umbraco.Deploy.Core.Connectors.ValueConnectors.Services;
using Umbraco.Deploy.Infrastructure.Extensions;
using Umbraco.Extensions;

namespace Umbraco.Deploy.Contrib.ValueConnectors
{
    /// <summary>
    /// A Deploy connector for the NestedContent property editor
    /// </summary>
    public class NestedContentValueConnector : ValueConnectorBase
    {
        private readonly IContentTypeService _contentTypeService;
        private readonly Lazy<ValueConnectorCollection> _valueConnectorsLazy;
        private readonly ILogger<NestedContentValueConnector> _logger;

        /// <inheritdoc />
        /// <remarks>
        /// Our.Umbraco.NestedContent is the original NestedContent package
        ///  Umbraco.NestedContent is Core NestedContent (introduced in v7.7)
        /// </remarks>
        public override IEnumerable<string> PropertyEditorAliases { get; } = new[]
        {
            "Umbraco.NestedContent"
        };

        // cannot inject ValueConnectorCollection as it creates a circular (recursive) dependency,
        // so we have to inject it lazily and use the lazy value when actually needing it
        private ValueConnectorCollection ValueConnectors => _valueConnectorsLazy.Value;

        public NestedContentValueConnector(IContentTypeService contentTypeService, Lazy<ValueConnectorCollection> valueConnectors, ILogger<NestedContentValueConnector> logger)
        {
            _contentTypeService = contentTypeService ?? throw new ArgumentNullException(nameof(contentTypeService));
            _valueConnectorsLazy = valueConnectors ?? throw new ArgumentNullException(nameof(valueConnectors));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public sealed override string ToArtifact(object value, IPropertyType propertyType, ICollection<ArtifactDependency> dependencies, IContextCache contextCache)
        {
            _logger.LogDebug("Converting {PropertyType} to artifact.", propertyType.Alias);
            var valueAsString = value as string;

            if (string.IsNullOrWhiteSpace(valueAsString))
            {
                _logger.LogDebug($"Value is null or whitespace. Skipping conversion to artifact.");
                return null;
            }

            var nestedContent = new List<NestedContentValue>();
            if (valueAsString.Trim().StartsWith("{"))
            {
                 if (valueAsString.TryParseJson(out NestedContentValue nestedContentObjectValue))
                {
                    nestedContent.Add(nestedContentObjectValue);
                }
                else
                {
                    _logger.LogWarning("Value '{Value}' is not a JSON string. Skipping conversion to artifact.", valueAsString);
                    return null;
                }
            }
            else
            {
                if (valueAsString.TryParseJson(out NestedContentValue[] nestedContentCollectionValue))
                {
                    nestedContent.AddRange(nestedContentCollectionValue);
                }
                else
                {
                    _logger.LogWarning("Value '{Value}' is not a JSON string. Skipping conversion to artifact.", valueAsString);
                    return null;
                }
            }

            if (nestedContent.All(x => x == null))
            {
                _logger.LogWarning("Value contained no elements. Skipping conversion to artifact.");
                return null;
            }

            var allContentTypes = nestedContent.Select(x => x.ContentTypeAlias)
                .Distinct()
                .ToDictionary(a => a, a => contextCache.GetContentTypeByAlias(_contentTypeService, a));

            //Ensure all of these content types are found
            if (allContentTypes.Values.Any(contentType => contentType == null))
            {
                throw new InvalidOperationException($"Could not resolve these content types for the Nested Content property: {string.Join(",", allContentTypes.Where(x => x.Value == null).Select(x => x.Key))}.");
            }

            //Ensure that these content types have dependencies added
            foreach (var contentType in allContentTypes.Values)
            {
                _logger.LogDebug("Adding dependency for content type {ContentType}.", contentType.Alias);
                dependencies.Add(new ArtifactDependency(contentType.GetUdi(), false, ArtifactDependencyMode.Match));
            }

            foreach (var row in nestedContent)
            {
                var contentType = allContentTypes[row.ContentTypeAlias];

                foreach (var key in row.PropertyValues.Keys.ToArray())
                {
                    // key is a system property that is added by NestedContent in Core v7.7
                    // see note in NestedContentValue - leave it unchanged
                    if (key == "key")
                        continue;

                    var innerPropertyType = contentType.CompositionPropertyTypes.FirstOrDefault(x => x.Alias == key);

                    if (innerPropertyType == null)
                    {
                        _logger.LogWarning("No property type found with alias {PropertyType} on content type {ContentType}.", key, propertyType.Alias);
                        continue;
                    }

                    // fetch the right value connector from the collection of connectors, intended for use with this property type.
                    // throws if not found - no need for a null check
                    var propertyValueConnector = ValueConnectors.Get(innerPropertyType);

                    // pass the value, property type and the dependencies collection to the connector to get a "artifact" value
                    var innerValue = row.PropertyValues[key];

                    // connectors are expecting strings, not JTokens
                    object preparedValue = innerValue is JToken
                        ? innerValue?.ToString()
                        : innerValue;
                    object parsedValue = propertyValueConnector.ToArtifact(preparedValue, innerPropertyType, dependencies, contextCache);

                    // getting Map image value umb://media/43e7401fb3cd48ceaa421df511ec703c to (nothing) - why?!
                    _logger.LogDebug("Mapped {Key} value '{PropertyValue}' to '{ParsedValue}' using {PropertyValueConnectorType} for {PropertyType}.", key, row.PropertyValues[key], parsedValue, propertyValueConnector.GetType(), innerPropertyType.Alias);

                    parsedValue = parsedValue?.ToString();

                    row.PropertyValues[key] = parsedValue;
                }
            }

            value = JsonConvert.SerializeObject(nestedContent);
            _logger.LogDebug("Finished converting {PropertyType} to artifact.", propertyType.Alias);
            return (string)value;
        }

        public sealed override object FromArtifact(string value, IPropertyType propertyType, object currentValue, IContextCache contextCache)
        {
            _logger.LogDebug("Converting {PropertyType} from artifact.", propertyType.Alias);
            if (string.IsNullOrWhiteSpace(value))
            {
                _logger.LogDebug($"Value is null or whitespace. Skipping conversion from artifact.");
                return value;
            }

            if (!value.TryParseJson(out NestedContentValue[] nestedContent))
            {
                _logger.LogWarning("Value '{Value}' is not a json string. Skipping conversion from artifact.", value);
                return value;
            }

            if (nestedContent == null || nestedContent.All(x => x == null))
            {
                _logger.LogWarning("Value contained no elements. Skipping conversion from artifact.");
                return value;
            }

            var allContentTypes = nestedContent.Select(x => x.ContentTypeAlias)
                .Distinct()
                .ToDictionary(a => a, a => contextCache.GetContentTypeByAlias(_contentTypeService, a));

            //Ensure all of these content types are found
            if (allContentTypes.Values.Any(contentType => contentType == null))
            {
                throw new InvalidOperationException($"Could not resolve these content types for the Nested Content property: {string.Join(",", allContentTypes.Where(x => x.Value == null).Select(x => x.Key))}.");
            }

            foreach (var row in nestedContent)
            {
                var contentType = allContentTypes[row.ContentTypeAlias];

                foreach (var key in row.PropertyValues.Keys.ToArray())
                {
                    // key is a system property that is added by NestedContent in Core v7.7
                    // see note in NestedContentValue - leave it unchanged
                    if (key == "key")
                        continue;

                    var innerPropertyType = contentType.CompositionPropertyTypes.FirstOrDefault(x => x.Alias == key);

                    if (innerPropertyType == null)
                    {
                        _logger.LogWarning("No property type found with alias {PropertyType} on content type {ContentType}.", key, contentType.Alias);
                        continue;
                    }

                    // fetch the right value connector from the collection of connectors, intended for use with this property type.
                    // throws if not found - no need for a null check
                    var propertyValueConnector = ValueConnectors.Get(innerPropertyType);

                    var innerValue = row.PropertyValues[key];

                    if (innerValue != null)
                    {
                        // pass the artifact value and property type to the connector to get a real value from the artifact
                        var convertedValue = propertyValueConnector.FromArtifact(innerValue.ToString(), innerPropertyType, null, contextCache);

                        if (convertedValue == null)
                        {
                            row.PropertyValues[key] = null;
                        }
                        // integers needs to be converted into strings
                        else if (convertedValue is int)
                        {
                            row.PropertyValues[key] = convertedValue.ToString();
                        }
                        // json strings need to be converted into JTokens
                        else if (convertedValue is string convertedStringValue && convertedStringValue.TryParseJson(out JToken valueAsJToken))
                        {
                            row.PropertyValues[key] = valueAsJToken;
                        }
                        else
                        {
                            row.PropertyValues[key] = convertedValue;
                        }
                        _logger.LogDebug("Mapped {Key} value '{PropertyValue}' to '{ConvertedValue}' using {PropertyValueConnectorType} for {PropertyType}.", key, innerValue, convertedValue, propertyValueConnector.GetType(), innerPropertyType.Alias);
                    }
                    else
                    {
                        row.PropertyValues[key] = innerValue;
                        _logger.LogDebug("{Key} value was null. Setting value as null without conversion.", key);
                    }
                }
            }

            // Note: NestedContent does not use formatting when serializing JSON values.
            value = JArray.FromObject(nestedContent).ToString(Formatting.None);

            _logger.LogDebug("Finished converting {PropertyType} from artifact.", propertyType.Alias);

            return value;
        }

        /// <summary>
        /// The typed value stored for Nested Content
        /// </summary>
        /// <example>
        /// An example of the JSON stored for NestedContent is:
        /// <![CDATA[
        ///    [
        ///      {"name":"Content","ncContentTypeAlias":"nC1","text":"Hello","multiText":"world","rTE":"<p>asdfasdfasdfasdf</p>\n<p>asdf</p>\n<p><img style=\"width: 213px; height: 213px;\" src=\"/media/1050/profile_pic_cg_2015.jpg?width=213&amp;height=213\" alt=\"\" rel=\"1087\" data-id=\"1087\" /></p>\n<p>asdf</p>"},
        ///      {"name":"Content","ncContentTypeAlias":"nC1","text":"This is ","multiText":"pretty cool","rTE":""}
        ///    ]
        /// ]]>
        /// </example>
        public class NestedContentValue
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("ncContentTypeAlias")]
            public string ContentTypeAlias { get; set; }

            /// <summary>
            /// The remaining properties will be serialized to a dictionary
            /// </summary>
            /// <remarks>
            /// The JsonExtensionDataAttribute is used to put the non-typed properties into a bucket
            /// http://www.newtonsoft.com/json/help/html/DeserializeExtensionData.htm
            /// NestedContent serializes to string, int, whatever eg
            ///   "stringValue":"Some String","numericValue":125,"otherNumeric":null
            /// </remarks>
            [JsonExtensionData]
            public IDictionary<string, object> PropertyValues { get; set; }
        }
    }
}
