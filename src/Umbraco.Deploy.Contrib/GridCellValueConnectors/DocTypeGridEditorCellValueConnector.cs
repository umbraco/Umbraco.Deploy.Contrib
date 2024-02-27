using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Deploy;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Deploy.ValueConnectors;

namespace Umbraco.Deploy.Contrib.Connectors.GridCellValueConnectors
{
    public class DocTypeGridEditorCellValueConnector : GridCellValueConnectorBase
    {
        private readonly IContentTypeService _contentTypeService;
        private readonly Lazy<ValueConnectorCollection> _valueConnectorsLazy;

        public DocTypeGridEditorCellValueConnector(IContentTypeService contentTypeService, Lazy<ValueConnectorCollection> valueConnectors)
        {
            if (contentTypeService == null) throw new ArgumentNullException(nameof(contentTypeService));
            if (valueConnectors == null) throw new ArgumentNullException(nameof(valueConnectors));
            _contentTypeService = contentTypeService;
            _valueConnectorsLazy = valueConnectors;
        }

        // cannot inject ValueConnectorCollection else of course it creates a circular (recursive) dependency,
        // so we have to inject it lazily and use the lazy value when actually needing it
        private ValueConnectorCollection ValueConnectors => _valueConnectorsLazy.Value;

        public override string GetValue(GridValue.GridControl control, Property property, ICollection<ArtifactDependency> dependencies)
        {
            // cancel if there's no values
            if (control.Value == null || control.Value.HasValues == false)
                return null;

            var docTypeGridEditorContent = JsonConvert.DeserializeObject<DocTypeGridEditorValue>(control.Value.ToString());

            // if an 'empty' dtge item has been added - it has no ContentTypeAlias set .. just return and don't throw.
            if (docTypeGridEditorContent == null || string.IsNullOrWhiteSpace(docTypeGridEditorContent.ContentTypeAlias))
                return null;

            // check if the doc type exist - else abort packaging
            var contentType = _contentTypeService.GetContentType(docTypeGridEditorContent.ContentTypeAlias);
            if (contentType == null)
            {
                throw new InvalidOperationException(
                    $"Could not resolve the Content Type for the Doc Type Grid Editor property: {docTypeGridEditorContent.ContentTypeAlias}");
            }

            // add content type as a dependency
            dependencies.Add(new ArtifactDependency(contentType.GetUdi(), false, ArtifactDependencyMode.Match));

            // find all properties
            var propertyTypes = contentType.CompositionPropertyTypes;
            foreach (var propertyType in propertyTypes)
            {
                // test if there's a value for the given property
                object value;
                if (!docTypeGridEditorContent.Value.TryGetValue(propertyType.Alias, out value) || value == null)
                    continue;

                Udi udi;
                // if the value is an Udi then add it as a dependency
                if (Udi.TryParse(value.ToString(), out udi))
                {
                    dependencies.Add(new ArtifactDependency(udi, false, ArtifactDependencyMode.Match));
                    continue;
                }

                // throws if not found - no need for a null check
                var propValueConnector = ValueConnectors.Get(propertyType);

                var mockProperty = new Property(propertyType, value);
                var parsedValue = propValueConnector.GetValue(mockProperty, dependencies);
                // test if the value is a json object (thus could be a nested complex editor)
                // if that's the case we'll need to add it as a json object instead of string to avoid it being escaped
                var jtokenValue = parsedValue != null && parsedValue.DetectIsJson() ? JToken.Parse(parsedValue) : null;
                if (jtokenValue != null)
                {
                    docTypeGridEditorContent.Value[propertyType.Alias] = jtokenValue;
                }
                else
                {
                    docTypeGridEditorContent.Value[propertyType.Alias] = parsedValue;
                }

            }

            var resolvedValue = JsonConvert.SerializeObject(docTypeGridEditorContent);
            return resolvedValue;
        }

        // The Doc Type Grid Editor has a view stored in "App_Plugins/DocTypeGridEditor/Views/doctypegrideditor.html"
        public override bool IsConnector(string view)
        {
            if (string.IsNullOrWhiteSpace(view))
                return false;

            return view.Contains("doctypegrideditor");
        }

        public override void SetValue(GridValue.GridControl control, Property property)
        {
            var emptyValue = control.Value.ToString();
            if (string.IsNullOrWhiteSpace(emptyValue))
            {
                return;
            }

            // For some reason the control value isn't properly parsed so we need this extra step to parse it into a JToken
            control.Value = JToken.Parse(control.Value.ToString());
            // cancel if there's no values
            if (control.Value == null || control.Value.HasValues == false)
                return;

            var docTypeGridEditorContent = JsonConvert.DeserializeObject<DocTypeGridEditorValue>(control.Value.ToString());

            if (docTypeGridEditorContent == null)
                return;

            // check if the doc type exist - else abort packaging
            var contentType = _contentTypeService.GetContentType(docTypeGridEditorContent.ContentTypeAlias);
            if (contentType == null)
            {
                throw new InvalidOperationException(
                    $"Could not resolve the Content Type for the Doc Type Grid Editor property: {docTypeGridEditorContent.ContentTypeAlias}");
            }

            // find all properties
            var propertyTypes = contentType.CompositionPropertyTypes;
            foreach (var propertyType in propertyTypes)
            {
                // test if there's a value for the given property
                object value;
                if (!docTypeGridEditorContent.Value.TryGetValue(propertyType.Alias, out value) || value == null)
                    continue;

                // throws if not found - no need for a null check
                var propValueConnector = ValueConnectors.Get(propertyType);

                var mockProperty = new Property(propertyType);
                var mockContent = new Content("mockContentGrid", -1, new ContentType(-1),
                    new PropertyCollection(new List<Property> {mockProperty}));

                propValueConnector.SetValue(mockContent, mockProperty.Alias, value.ToString());
                var convertedValue = mockContent.GetValue(mockProperty.Alias);

                // integers needs to be converted into strings for DTGE to work
                if (convertedValue is int)
                {
                    docTypeGridEditorContent.Value[propertyType.Alias] = convertedValue.ToString();
                }
                else if (convertedValue == null)
                {
                    //Assign the null back - otherwise the check for JSON will fail as we cant convert a null to a string
                    //NOTE: LinkPicker2 for example if no link set is returning a null as opposed to empty string
                    docTypeGridEditorContent.Value[propertyType.Alias] = null;
                }
                else
                {
                    // test if the value is a json object (thus could be a nested complex editor)
                    // if that's the case we'll need to add it as a json object instead of string to avoid it being escaped
                    var jtokenValue = convertedValue.ToString().DetectIsJson()
                        ? JToken.Parse(convertedValue.ToString())
                        : null;
                    if (jtokenValue != null)
                    {
                        docTypeGridEditorContent.Value[propertyType.Alias] = jtokenValue;
                    }
                    else
                    {
                        docTypeGridEditorContent.Value[propertyType.Alias] = convertedValue;
                    }
                }
            }

            control.Value = JToken.FromObject(docTypeGridEditorContent);
        }
    }

    /// <summary>
    /// The value of a DocTypeGridEditor cell
    /// <example>
    /// An example of the JSON stored for DocTypeGridEditor is:
    /// <![CDATA[
    ///    [
    ///      {{"dtgeContentTypeAlias": "listingModule", "value": {
    ///    "name": "Template: Product Listing",
    ///    "title": "Lorem Ipsum",
    ///    "caption": "Lorem IpsumLorem IpsumLorem IpsumLorem IpsumLorem Ipsum",
    /// "hideSearchForm": "0",
    ///"primaryFeatures": [
    ///  {
    ///        "name": "Lorem Ipsum",
    ///        "ncContentTypeAlias": "featuredProduct",
    ///        "product": "939",
    ///        "heading": "Lorem Ipsum",
    ///        "subheading": "Lorem IpsumLorem IpsumLorem Ipsum"
    ///      },
    ///      {
    ///        "name": "Lorem Ipsum",
    ///        "ncContentTypeAlias": "featuredProduct",
    ///        "product": "1001",
    ///        "heading": "Lorem Ipsum",
    ///        "subheading": "Lorem IpsumLorem IpsumLorem Ipsum"
    ///   }
    ///    ],
    ///   "secondaryFeatures": "10010,9992,10993,9966,10009",
    ///   "sampleSize": 5
    ///  },
    ///  "id": "3c9c58fe-f127-7661-6765-7e14783ed479"
    ///}}
    ///    ]
    /// ]]>
    /// </example>
    /// </summary>
    public class DocTypeGridEditorValue
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("dtgeContentTypeAlias")]
        public string ContentTypeAlias { get; set; }

        [JsonProperty("value")]
        public Dictionary<string, object> Value { get; set; }
    }
}
