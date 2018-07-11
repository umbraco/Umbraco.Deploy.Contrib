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
using Umbraco.Deploy.ValueConnectors;

namespace Umbraco.Deploy.Contrib.Connectors.ValueConnectors
{
    /// <summary>
    /// A Deploy valueconnector for the property editors based on InnerContent
    /// </summary>
    public abstract class InnerContentConnector : IValueConnector
    {
        private readonly IContentTypeService _contentTypeService;
        private readonly Lazy<ValueConnectorCollection> _valueConnectorsLazy;

        protected InnerContentConnector(IContentTypeService contentTypeService, Lazy<ValueConnectorCollection> valueConnectors)
        {
            if (contentTypeService == null) throw new ArgumentNullException(nameof(contentTypeService));
            if (valueConnectors == null) throw new ArgumentNullException(nameof(valueConnectors));
            _contentTypeService = contentTypeService;
            _valueConnectorsLazy = valueConnectors;
        }

        public virtual IEnumerable<string> PropertyEditorAliases => new[] { "Our.Umbraco.InnerContent" };

        // cannot inject ValueConnectorCollection else of course it creates a circular (recursive) dependency,
        // so we have to inject it lazily and use the lazy value when actually needing it
        private ValueConnectorCollection ValueConnectors => _valueConnectorsLazy.Value;

        /// <summary>
        /// Gets the deploy property corresponding to a content property.
        /// </summary>
        /// <param name="property">The content property.</param>
        /// <param name="dependencies">The content dependencies.</param>
        /// <returns>The deploy property value.</returns>
        /// <remarks>
        /// This will iterate through each row of inner content and determine the Property Type for each row, then it will
        /// resolve the value from the row's underlying ValueConnector so that the whole thing can be re-serialized/normalized for transfer.
        /// In order to do this we need to create a fake property to pass in to the underlying IValueConnector.
        /// </remarks>
        public string GetValue(Property property, ICollection<ArtifactDependency> dependencies)
        {
            var value = property.Value as string;

            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (value.DetectIsJson() == false)
                return null;

            var innerContent = JsonConvert.DeserializeObject<InnerContentValue[]>(value);

            if (innerContent == null)
                return null;

            var distinctContentTypes = new Dictionary<GuidUdi, IContentType>();
            foreach (var innerContentItem in innerContent)
            {
                IContentType contentType = null;
                if (innerContentItem.IcContentTypeGuid.HasValue)
                    contentType = _contentTypeService.GetContentType(innerContentItem.IcContentTypeGuid.Value);
                if (contentType == null)
                    contentType = _contentTypeService.GetContentType(innerContentItem.IcContentTypeAlias);
                if (contentType == null)
                    throw new InvalidOperationException($"Could not resolve these content types for the Inner Content property with key: {innerContentItem.Key}, and name: {innerContentItem.Name}");

                // ensure the content type is added as a unique dependency
                var contentTypeUdi = contentType.GetUdi();
                if (distinctContentTypes.ContainsKey(contentTypeUdi) == false)
                {
                    distinctContentTypes.Add(contentTypeUdi, contentType);
                    dependencies.Add(new ArtifactDependency(contentTypeUdi, false, ArtifactDependencyMode.Match));
                }

                if (innerContentItem.PropertyValues != null)
                {
                    foreach (var key in innerContentItem.PropertyValues.Keys.ToArray())
                    {
                        var propertyType = contentType.CompositionPropertyTypes.FirstOrDefault(x => x.Alias == key);

                        if (propertyType == null)
                        {
                            LogHelper.Debug<InnerContentConnector>($"No Property Type found with alias {key} on Content Type {contentType.Alias}");
                            continue;
                        }

                        // throws if not found - no need for a null check
                        var propValueConnector = ValueConnectors.Get(propertyType);

                        // this should be enough for all other value connectors to work with
                        // as all they should need is the value, and the property type infos
                        var mockProperty = new Property(propertyType, innerContentItem.PropertyValues[key]);

                        object parsedValue = propValueConnector.GetValue(mockProperty, dependencies);

                        // test if the value is a json object (thus could be a nested complex editor)
                        // if that's the case we'll need to add it as a json object instead of string to avoid it being escaped
						JToken jtokenValue;
	                    if (TryParseJToken(parsedValue, out jtokenValue))
                        {
                            parsedValue = jtokenValue;
                        }
                        else if (parsedValue != null)
                        {
                            parsedValue = parsedValue.ToString();
                        }

                        innerContentItem.PropertyValues[key] = parsedValue;
                    }
                }
            }

            value = JsonConvert.SerializeObject(innerContent);
            return value;
        }

        /// <summary>
        /// Sets a content property value using a deploy property.
        /// </summary>
        /// <param name="content">The content item.</param>
        /// <param name="alias">The property alias.</param>
        /// <param name="value">The deploy property value.</param>
        /// <remarks>
        /// This is a bit tricky because for each row of inner content we need to pass the value of it to it's related
        /// IValueConnector because each row is essentially a Property Type - though a fake one.
        /// So to do this we have to create a fake content item for the underlying IValueConnector to set it's value on and
        /// then we can extract that value from it to put on the main serialized object to set on the real IContentBase item.
        /// </remarks>
        public void SetValue(IContentBase content, string alias, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                content.SetValue(alias, value);
                return;
            }

            if (value.DetectIsJson() == false)
                return;

            var innerContent = JsonConvert.DeserializeObject<InnerContentValue[]>(value);

            if (innerContent == null)
                return;

            foreach (var innerContentItem in innerContent)
            {
                IContentType contentType = null;
                if (innerContentItem.IcContentTypeGuid.HasValue)
                    contentType = _contentTypeService.GetContentType(innerContentItem.IcContentTypeGuid.Value);
                if (contentType == null)
                    contentType = _contentTypeService.GetContentType(innerContentItem.IcContentTypeAlias);
                if (contentType == null)
                    throw new InvalidOperationException($"Could not resolve these content types for the Inner Content property with key: {innerContentItem.Key}, and name: {innerContentItem.Name}");

                var mocks = new Dictionary<IContentType, IContent>();

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
                    mockContent = mocks[contentType] = new Content("IC_" + Guid.NewGuid(), -1, contentType);

				if (innerContentItem.PropertyValues != null)
                {

	                foreach (var key in innerContentItem.PropertyValues.Keys.ToArray())
	                {
	                    var propertyType = contentType.CompositionPropertyTypes.FirstOrDefault(x => x.Alias == key);

	                    if (propertyType == null)
	                    {
	                        LogHelper.Debug<InnerContentConnector>($"No Property Type found with alias {key} on Content Type {contentType.Alias}");
	                        continue;
	                    }

	                    // throws if not found - no need for a null check
	                    var propValueConnector = ValueConnectors.Get(propertyType);

	                    var rowValue = innerContentItem.PropertyValues[key];

	                    if (rowValue != null)
	                    {
	                        propValueConnector.SetValue(mockContent, propertyType.Alias, rowValue.ToString());
	                        var convertedValue = mockContent.GetValue(propertyType.Alias);
	                        // integers needs to be converted into strings
	                        if (convertedValue is int)
	                        {
	                            innerContentItem.PropertyValues[key] = convertedValue.ToString();
	                        }
	                        else
	                        {
	                            // test if the value is a json object (thus could be a nested complex editor)
	                            // if that's the case we'll need to add it as a json object instead of string to avoid it being escaped
	                            JToken jtokenValue;
	                            if (TryParseJToken(convertedValue, out jtokenValue))
	                            {
	                                innerContentItem.PropertyValues[key] = jtokenValue;
	                            }
	                            else
	                            {
	                                innerContentItem.PropertyValues[key] = convertedValue;
	                            }
	                        }
	                    }
	                    else
	                    {
	                        innerContentItem.PropertyValues[key] = rowValue;
	                    }
	                }
				}	
            }

            // InnerContent does not use formatting when serializing JSON values
            value = JArray.FromObject(innerContent).ToString(Formatting.None);
            content.SetValue(alias, value);
        }

        private static bool TryParseJToken(object value, out JToken json)
        {
            json = default(JToken);

            if (value == null)
                return false;

            var s = value.ToString();
            if (string.IsNullOrWhiteSpace(s) || s.DetectIsJson() == false)
                return false;

            // we're okay with an empty catch here as that just means 'json' will stay null and we return false
            try
            {
                json = JToken.Parse(s);
            }
            catch { }

            return json != null;
        }

        /// <summary>
        /// The typed value stored for Inner Content
        /// </summary>
        /// <example>
        /// An example of the JSON stored for InnerContent is:
        /// []
        /// <![CDATA[
        ///    [
        ///      {"key":"66820fc4-dc8a-4859-8e18-29a1038b20cd","name":"Item 0","icon":"icon-document","icContentTypeAlias":"stacked","textstring":"text","mntp":"1376"},
        ///      {"key":"597991cc-e026-46c6-89d5-5ed6b9ef83e9","name":"Item 1","icon":"icon-document","icContentTypeAlias":"stacked","textstring":"other text","mntp":"1371,1373"}
        ///    ]
        /// ]]>
        /// </example>
        public class InnerContentValue
        {
            [JsonProperty("key")]
            public string Key { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("icon")]
            public string Icon { get; set; }

            [JsonProperty("icContentTypeAlias")]
            public string IcContentTypeAlias { get; set; }

            [JsonProperty("icContentTypeGuid")]
            public Guid? IcContentTypeGuid { get; set; }

            /// <summary>
            /// The remaining properties will be serialized to a dictionary
            /// </summary>
            /// <remarks>
            /// The JsonExtensionDataAttribute is used to put the non-typed properties into a bucket
            /// http://www.newtonsoft.com/json/help/html/DeserializeExtensionData.htm
            /// </remarks>
            [JsonExtensionData]
            public IDictionary<string, object> PropertyValues { get; set; }
        }
    }
}
