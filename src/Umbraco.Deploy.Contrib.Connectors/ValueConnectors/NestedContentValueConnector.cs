using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Deploy;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Deploy.Connectors.ValueConnectors.Services;
using Umbraco.Deploy.Work;

namespace Umbraco.Deploy.Contrib.Connectors.ValueConnectors
{
    /// <summary>
    /// A Deploy connector for the NestedContent property editor
    /// </summary>
    public class NestedContentValueConnector : IValueConnector
    {
        private readonly IContentTypeService _contentTypeService;
        private readonly Lazy<ValueConnectorCollection> _valueConnectorsLazy;
        private readonly ILogger _logger;
        private readonly IWorkCacheAdaptor _workCacheAdaptor;

        // Our.Umbraco.NestedContent is the original NestedContent package
        // Umbraco.NestedContent is Core NestedContent (introduced in v7.7)
        public virtual IEnumerable<string> PropertyEditorAliases => new[] { "Our.Umbraco.NestedContent", "Umbraco.NestedContent" };

        // cannot inject ValueConnectorCollection as it creates a circular (recursive) dependency,
        // so we have to inject it lazily and use the lazy value when actually needing it
        private ValueConnectorCollection ValueConnectors => _valueConnectorsLazy.Value;

        // TODO (V11): Remove obsolete constructor.

        [Obsolete("Please use the constructor taking all paramenters. This constructor will be removed in a future version.")]
        public NestedContentValueConnector(
            IContentTypeService contentTypeService,
            Lazy<ValueConnectorCollection> valueConnectors,
            ILogger logger)
            : this(
                contentTypeService,
                valueConnectors,
                logger,
                Current.Factory.GetInstance<IWorkCacheAdaptor>())
        {
        }

        public NestedContentValueConnector(
            IContentTypeService contentTypeService,
            Lazy<ValueConnectorCollection> valueConnectors,
            ILogger logger,
            IWorkCacheAdaptor workCacheAdaptor)
        {
            if (contentTypeService == null) throw new ArgumentNullException(nameof(contentTypeService));
            if (valueConnectors == null) throw new ArgumentNullException(nameof(valueConnectors));
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (workCacheAdaptor == null) throw new ArgumentNullException(nameof(workCacheAdaptor));

            _contentTypeService = contentTypeService;
            _valueConnectorsLazy = valueConnectors;
            _logger = logger;
            _workCacheAdaptor = workCacheAdaptor;
        }

        public string ToArtifact(object value, PropertyType propertyType, ICollection<ArtifactDependency> dependencies)
        {
            _logger.Debug<NestedContentValueConnector>("Converting {PropertyType} to artifact.", propertyType.Alias);
            var svalue = value as string;

            if (string.IsNullOrWhiteSpace(svalue))
            {
                _logger.Debug<NestedContentValueConnector>($"Value is null or whitespace. Skipping conversion to artifact.");
                return null;
            }

            if (svalue.DetectIsJson() == false)
            {
                _logger.Warn<NestedContentValueConnector>("Value '{Value}' is not a json string. Skipping conversion to artifact.", svalue);
                return null;
            }

            var nestedContent = new List<NestedContentValue>();
            if (svalue.Trim().StartsWith("{"))
            {
                nestedContent.Add(JsonConvert.DeserializeObject<NestedContentValue>(svalue));
            }
            else
            {
                nestedContent.AddRange(JsonConvert.DeserializeObject<NestedContentValue[]>(svalue));
            }

            if (nestedContent.All(x => x == null))
            {
                _logger.Warn<NestedContentValueConnector>("Value contained no elements. Skipping conversion to artifact.");
                return null;
            }

            var allContentTypes = nestedContent.Select(x => x.ContentTypeAlias)
                .Distinct()
                .ToDictionary(a => a, a => GetContentType(a));

            //Ensure all of these content types are found
            if (allContentTypes.Values.Any(contentType => contentType == null))
            {
                throw new InvalidOperationException($"Could not resolve these content types for the Nested Content property: {string.Join(",", allContentTypes.Where(x => x.Value == null).Select(x => x.Key))}.");
            }

            //Ensure that these content types have dependencies added
            foreach (var contentType in allContentTypes.Values)
            {
                _logger.Debug<NestedContentValueConnector>("Adding dependency for content type {ContentType}.", contentType.Alias);
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
                        _logger.Warn<NestedContentValueConnector>("No property type found with alias {PropertyType} on content type {ContentType}.", key, propertyType.Alias);
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
                    object parsedValue = propertyValueConnector.ToArtifact(preparedValue, innerPropertyType, dependencies);

                    // getting Map image value umb://media/43e7401fb3cd48ceaa421df511ec703c to (nothing) - why?!
                    _logger.Debug<NestedContentValueConnector>("Mapped {Key} value '{PropertyValue}' to '{ParsedValue}' using {PropertyValueConnectorType} for {PropertyType}.", key, row.PropertyValues[key], parsedValue, propertyValueConnector.GetType(), innerPropertyType.Alias);

                    parsedValue = parsedValue?.ToString();

                    row.PropertyValues[key] = parsedValue;
                }
            }

            value = JsonConvert.SerializeObject(nestedContent);
            _logger.Debug<BlockEditorValueConnector>("Finished converting {PropertyType} to artifact.", propertyType.Alias);
            return (string)value;
        }

        private IContentType GetContentType(string alias) =>
            _workCacheAdaptor.GetCacheItem(
                    WorkCacheKeys.GetCacheKey(WorkCacheKeys.OperationKeys.DocumentTypeByAlias, alias),
                    () => _contentTypeService.Get(alias));

        public object FromArtifact(string value, PropertyType propertyType, object currentValue)
        {
            _logger.Debug<NestedContentValueConnector>("Converting {PropertyType} from artifact.", propertyType.Alias);
            if (string.IsNullOrWhiteSpace(value))
            {
                _logger.Debug<NestedContentValueConnector>($"Value is null or whitespace. Skipping conversion from artifact.");
                return value;
            }

            if (value.DetectIsJson() == false)
            {
                _logger.Warn<NestedContentValueConnector>("Value '{Value}' is not a json string. Skipping conversion from artifact.", value);
                return value;
            }

            var nestedContent = JsonConvert.DeserializeObject<NestedContentValue[]>(value);

            if (nestedContent == null || nestedContent.All(x => x == null))
            {
                _logger.Warn<NestedContentValueConnector>("Value contained no elements. Skipping conversion from artifact.");
                return value;
            }

            var allContentTypes = nestedContent.Select(x => x.ContentTypeAlias)
                .Distinct()
                .ToDictionary(a => a, a => _contentTypeService.Get(a));

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
                        _logger.Warn<NestedContentValueConnector>("No property type found with alias {PropertyType} on content type {ContentType}.", key, contentType.Alias);
                        continue;
                    }

                    // fetch the right value connector from the collection of connectors, intended for use with this property type.
                    // throws if not found - no need for a null check
                    var propertyValueConnector = ValueConnectors.Get(innerPropertyType);

                    var innerValue = row.PropertyValues[key];

                    if (innerValue != null)
                    {
                        // pass the artifact value and property type to the connector to get a real value from the artifact
                        var convertedValue = propertyValueConnector.FromArtifact(innerValue.ToString(), innerPropertyType, null);

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
                        else if (convertedValue is string convertedStringValue && convertedStringValue.DetectIsJson())
                        {
                            row.PropertyValues[key] = JToken.Parse(convertedStringValue);
                        }
                        else
                        {
                            row.PropertyValues[key] = convertedValue;
                        }
                        _logger.Debug<NestedContentValueConnector>("Mapped {Key} value '{PropertyValue}' to '{ConvertedValue}' using {PropertyValueConnectorType} for {PropertyType}.", key, innerValue, convertedValue, propertyValueConnector.GetType(), innerPropertyType.Alias);
                    }
                    else
                    {
                        row.PropertyValues[key] = innerValue;
                        _logger.Debug<NestedContentValueConnector>("{Key} value was null. Setting value as null without conversion.", key);
                    }
                }
            }

            // Note: NestedContent does not use formatting when serializing JSON values.
            value = JArray.FromObject(nestedContent).ToString(Formatting.None);

            _logger.Debug<NestedContentValueConnector>("Finished converting {PropertyType} from artifact.", propertyType.Alias);

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
