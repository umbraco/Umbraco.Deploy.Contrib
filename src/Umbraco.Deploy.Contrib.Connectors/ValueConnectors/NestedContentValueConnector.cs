using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Deploy;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Deploy.Connectors.ValueConnectors.Services;

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

        public NestedContentValueConnector(IContentTypeService contentTypeService, Lazy<ValueConnectorCollection> valueConnectors, ILogger logger)
        {
            if (contentTypeService == null) throw new ArgumentNullException(nameof(contentTypeService));
            if (valueConnectors == null) throw new ArgumentNullException(nameof(valueConnectors));
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            _contentTypeService = contentTypeService;
            _valueConnectorsLazy = valueConnectors;
            _logger = logger;
        }

        // Our.Umbraco.NestedContent is the original NestedContent package
        // Umbraco.NestedContent is Core NestedContent (introduced in v7.7)
        public string ToArtifact(object value, PropertyType propertyType, ICollection<ArtifactDependency> dependencies)
        {
            var svalue = value as string;
            if (string.IsNullOrWhiteSpace(svalue))
                return null;

            if (svalue.DetectIsJson() == false)
                return null;

            var nestedContent = new List<NestedContentValue>();
            if (svalue.Trim().StartsWith("{"))
                nestedContent.Add(JsonConvert.DeserializeObject<NestedContentValue>(svalue));
            else
                nestedContent.AddRange(JsonConvert.DeserializeObject<NestedContentValue[]>(svalue));

            if (nestedContent.All(x => x == null))
                return null;

            var allContentTypes = nestedContent.Select(x => x.ContentTypeAlias)
                .Distinct()
                .ToDictionary(a => a, a => _contentTypeService.Get(a));

            //Ensure all of these content types are found
            if (allContentTypes.Values.Any(contentType => contentType == null))
            {
                throw new InvalidOperationException($"Could not resolve these content types for the Nested Content property: {string.Join(",", allContentTypes.Where(x => x.Value == null).Select(x => x.Key))}");
            }

            //Ensure that these content types have dependencies added
            foreach (var contentType in allContentTypes.Values)
            {
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

                    var propType = contentType.CompositionPropertyTypes.FirstOrDefault(x => x.Alias == key);

                    if (propType == null)
                    {
                        _logger.Debug<NestedContentValueConnector>($"No property type found with alias {key} on content type {contentType.Alias}");
                        continue;
                    }

                    // throws if not found - no need for a null check
                    var propValueConnector = ValueConnectors.Get(propType);

                    // this should be enough for all other value connectors to work with
                    // as all they should need is the value, and the property type infos
                    //var mockProperty = new Property(propType);
                    //var mockProperty = new Property(propType, row.PropertyValues[key]);
                    var val = row.PropertyValues[key];
                    object parsedValue = propValueConnector.ToArtifact(val, propType, dependencies);

                    // getting Map image value umb://media/43e7401fb3cd48ceaa421df511ec703c to (nothing) - why?!
                    _logger.Debug<NestedContentValueConnector>("Map " + key + " value '" + row.PropertyValues[key] + "' to '" + parsedValue
                        + "' using " + propValueConnector.GetType() + " for " + propType);

                    // test if the value is a json object (thus could be a nested complex editor)
                    // if that's the case we'll need to add it as a json object instead of string to avoid it being escaped
                    JToken jtokenValue = parsedValue != null && parsedValue.ToString().DetectIsJson() ? JToken.Parse(parsedValue.ToString()) : null;
                    if (jtokenValue != null)
                    {
                        parsedValue = jtokenValue;
                    }
                    else if (parsedValue != null)
                    {
                        parsedValue = parsedValue.ToString();
                    }

                    row.PropertyValues[key] = parsedValue;
                }
            }

            value = JsonConvert.SerializeObject(nestedContent);
            return (string)value;
        }

        public object FromArtifact(string value, PropertyType propertyType, object currentValue)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            if (value.DetectIsJson() == false)
                return value;

            var nestedContent = JsonConvert.DeserializeObject<NestedContentValue[]>(value);

            if (nestedContent == null)
                return value;

            var allContentTypes = nestedContent.Select(x => x.ContentTypeAlias)
                .Distinct()
                .ToDictionary(a => a, a => _contentTypeService.Get(a));

            //Ensure all of these content types are found
            if (allContentTypes.Values.Any(contentType => contentType == null))
            {
                throw new InvalidOperationException($"Could not resolve these content types for the Nested Content property: {string.Join(",", allContentTypes.Where(x => x.Value == null).Select(x => x.Key))}");
            }

            var mocks = new Dictionary<IContentType, IContent>();

            foreach (var row in nestedContent)
            {
                var contentType = allContentTypes[row.ContentTypeAlias];

                // note
                // the way we do it here, doing content.SetValue() several time on the same content, reduces
                // allocations and should be ok because SetValue does not care about the previous value - would
                // be different for the overloads that manage eg files for uploads (not sure how NestedContent
                // deals with them really)

                // we need a fake content instance to pass in to the value connector, since the value connector
                // wants to SetValue on an object - then we can extract the value back from that object to set
                // it correctly on the real instance
                IContent mockContent;
                if (!mocks.TryGetValue(contentType, out mockContent))
                    mockContent = mocks[contentType] = new Content("NC_" + Guid.NewGuid(), -1, contentType);

                foreach (var key in row.PropertyValues.Keys.ToArray())
                {
                    // key is a system property that is added by NestedContent in Core v7.7
                    // see note in NestedContentValue - leave it unchanged
                    if (key == "key")
                        continue;

                    var innerPropertyType = contentType.CompositionPropertyTypes.FirstOrDefault(x => x.Alias == key);

                    if (innerPropertyType == null)
                    {
                        _logger.Debug<NestedContentValueConnector>($"No property type found with alias {key} on content type {contentType.Alias}");
                        continue;
                    }

                    // throws if not found - no need for a null check
                    var propValueConnector = ValueConnectors.Get(innerPropertyType);

                    var rowValue = row.PropertyValues[key];

                    if (rowValue != null)
                    {
                        var convertedValue = propValueConnector.FromArtifact(rowValue.ToString(), innerPropertyType, null);
                        if (convertedValue == null)
                        {
                            row.PropertyValues[key] = null;
                        }
                        // integers needs to be converted into strings
                        else if (convertedValue is int)
                        {
                            row.PropertyValues[key] = convertedValue.ToString();
                        }
                        else
                        {
                            // test if the value is a json object (thus could be a nested complex editor)
                            // if that's the case we'll need to add it as a json object instead of string to avoid it being escaped
                            JToken jtokenValue = convertedValue.ToString().DetectIsJson() ? JToken.Parse(convertedValue.ToString()) : null;
                            if (jtokenValue != null)
                            {
                                row.PropertyValues[key] = jtokenValue;
                            }
                            else
                            {
                                row.PropertyValues[key] = convertedValue;
                            }
                        }
                    }
                    else
                    {
                        row.PropertyValues[key] = rowValue;
                    }
                }
            }

            // Note: NestedContent does not use formatting when serializing JSON values.
            value = JArray.FromObject(nestedContent).ToString(Formatting.None);
            return value;
        }

        public virtual IEnumerable<string> PropertyEditorAliases => new[] { "Our.Umbraco.NestedContent", "Umbraco.NestedContent" };

        // cannot inject ValueConnectorCollection else of course it creates a circular (recursive) dependency,
        // so we have to inject it lazily and use the lazy value when actually needing it
        private ValueConnectorCollection ValueConnectors => _valueConnectorsLazy.Value;

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

            // starting with v7.7, Core's NestedContent implement "key" as a system property
            // but since we are supporting pre-v7.7 including the NestedContent package, we
            // cannot do it this way - it's all managed "manually" when dealing with
            // PropertyValues.
            //[JsonProperty("key")]
            //public Guid Key { get; set; }

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
