using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Deploy;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Umbraco.Deploy.Contrib.Connectors.ValueConnectors
{
    /// <summary>
    /// A Deploy connector for the nuPickers property editor, when used with Umbraco content & media.
    /// </summary>
    public class NuPickersValueConnector : IValueConnector
    {
        private readonly IEntityService _entityService;

        private enum SaveFormat
        {
            CSV,
            JSON,
            XML
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NuPickersValueConnector"/> class.
        /// </summary>
        /// <param name="entityService">An <see cref="IEntityService"/> implementation.</param>
        public NuPickersValueConnector(IEntityService entityService)
        {
            _entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
        }

        public IEnumerable<string> PropertyEditorAliases => Enumerable.Empty<string>();

        /// <summary>
        /// Gets the deploy property corresponding to a content property.
        /// </summary>
        /// <param name="property">The content property.</param>
        /// <param name="dependencies">The content dependencies.</param>
        /// <returns>The deploy property value.</returns>
        public string GetValue(Property property, ICollection<ArtifactDependency> dependencies)
        {
            // get the property value
            var value = property.Value as string;

            if (string.IsNullOrWhiteSpace(value))
                return null;

            // parse the value - checking the format - CSV, XML or JSON
            SaveFormat format;
            var items = ParseValue(value, out format);

            var result = new List<KeyValuePair<string, string>>();

            // loop over the values
            foreach (var item in items)
            {
                int nodeId;
                if (int.TryParse(item.Key, out nodeId))
                {
                    // if an INT, attempt to get the UDI
                    var guidUdi = GetGuidUdi(nodeId);
                    if (guidUdi != null)
                    {
                        dependencies.Add(new ArtifactDependency(guidUdi, false, ArtifactDependencyMode.Exist));
                        result.Add(new KeyValuePair<string, string>(guidUdi.ToString(), item.Value));
                    }
                    else
                    {
                        // the value isn't a UDI, assume it's "just a string"
                        result.Add(item);
                    }
                }
                else
                {
                    // the value isn't a node ID, assume it's "just a string"
                    result.Add(item);
                }
            }

            // return the value in the correct format - CSV, XML, JSON, (whatevs)
            return SerializeValue(result, format);
        }

        /// <summary>
        /// Sets a content property value using a deploy property.
        /// </summary>
        /// <param name="content">The content item.</param>
        /// <param name="alias">The property alias.</param>
        /// <param name="value">The deploy property value.</param>
        public void SetValue(IContentBase content, string alias, string value)
        {
            // parse the value - checking the format - CSV, XML or JSON
            SaveFormat format;
            var items = ParseValue(value, out format);

            var result = new List<KeyValuePair<string, string>>();

            // loop over the values
            foreach (var item in items)
            {
                GuidUdi guidUdi;
                if (GuidUdi.TryParse(item.Key, out guidUdi) && guidUdi.Guid != Guid.Empty)
                {
                    // if an UDI, attempt to get the INT
                    var nodeId = GetIntId(guidUdi.Guid);
                    if (nodeId > 0)
                    {
                        result.Add(new KeyValuePair<string, string>(nodeId.ToString(), item.Value));
                    }
                    else
                    {
                        // the value isn't a node ID, assume it's "just a string"
                        result.Add(item);
                    }
                }
                else
                {
                    // the value isn't a UDI, assume it's "just a string"
                    result.Add(item);
                }
            }

            // re-assemble the values into the correct format - CSV, XML, JSON, (whatevs)
            content.SetValue(alias, SerializeValue(result, format));
        }

        private IEnumerable<KeyValuePair<string, string>> ParseValue(string value, out SaveFormat format)
        {
            // code taken from nuPickers
            // ref: https://github.com/uComponents/nuPickers/blob/master/source/nuPickers/Shared/SaveFormat/SaveFormat.cs#L43
            if (!string.IsNullOrWhiteSpace(value))
            {
                switch (value[0])
                {
                    case '[':
                        format = SaveFormat.JSON;
                        return JsonConvert.DeserializeObject<JArray>(value).Select(x => new KeyValuePair<string, string>(x["key"].ToString(), x["label"].ToString()));

                    case '<':
                        format = SaveFormat.XML;
                        return XDocument.Parse(value).Descendants("Picked").Select(x => new KeyValuePair<string, string>(x.Attribute("Key").Value, x.Value));

                    default:
                        format = SaveFormat.CSV;
                        return value.Split(',').Select(x => new KeyValuePair<string, string>(x, null)); // NOTE: label is null
                }
            }

            format = SaveFormat.CSV;
            return Enumerable.Empty<KeyValuePair<string, string>>();
        }

        private string SerializeValue(IEnumerable<KeyValuePair<string, string>> value, SaveFormat format)
        {
            // ref: https://github.com/uComponents/nuPickers/wiki/Save-Formats
            // ref: https://github.com/uComponents/nuPickers/blob/master/source/nuPickers/Shared/SaveFormat/SaveFormatResource.js#L16

            switch (format)
            {
                case SaveFormat.JSON:
                    var json = value.Select(x => $"{{ 'key': '{x.Key}', 'label': '{x.Value}' }}");
                    return string.Concat("[", string.Join(",", json, "]"));

                case SaveFormat.XML:
                    var xml = value.Select(x => $"<Picked Key=\"{x.Key}\"><![CDATA[{x.Value}]]></Picked>");
                    return string.Concat("<Picker>", string.Join(",", xml, "</Picker>"));

                case SaveFormat.CSV:
                default:
                    return string.Join(",", value.Select(x => x.Key));
            }
        }

        private GuidUdi GetGuidUdi(int id)
        {
            var keyForId = _entityService.GetKeyForId(id, UmbracoObjectTypes.Document);
            if (keyForId.Success)
                return new GuidUdi(Constants.UdiEntityType.Document, keyForId.Result);

            keyForId = _entityService.GetKeyForId(id, UmbracoObjectTypes.Media);
            if (keyForId.Success)
                return new GuidUdi(Constants.UdiEntityType.Media, keyForId.Result);

            return null;
        }

        private int GetIntId(Guid id)
        {
            var idForKey = _entityService.GetIdForKey(id, UmbracoObjectTypes.Document);
            if (idForKey.Success)
                return idForKey.Result;

            idForKey = _entityService.GetIdForKey(id, UmbracoObjectTypes.Media);
            if (idForKey.Success)
                return idForKey.Result;

            return -1;
        }
    }
}
