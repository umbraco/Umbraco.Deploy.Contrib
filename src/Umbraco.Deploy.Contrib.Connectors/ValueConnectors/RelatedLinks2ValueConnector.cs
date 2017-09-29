using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Deploy;
using Umbraco.Core.Models;

namespace Umbraco.Deploy.Contrib.Connectors.ValueConnectors
{
    /*
    [
      {
        "caption": "Page A Internal Link",
        "link": "umb://document/a3e8d3e344a64be9871a54cae6b65008",
        "newWindow": false,
        "internal": "umb://document/a3e8d3e344a64be9871a54cae6b65008",
        "edit": false,
        "isInternal": true,
        "internalName": "Page A",
        "internalIcon": "icon-document",
        "type": "internal",
        "title": "Page A Internal Link"
      }
    ]
    */

    /// <summary>
    /// Implements a value connector for the new related links editor (storing Udis).
    /// </summary>
    public class RelatedLinks2ValueConnector : IValueConnector
    {
        public string GetValue(Property property, ICollection<ArtifactDependency> dependencies)
        {
            
            var value = property.Value as string;

            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (value.DetectIsJson() == false)
                return null;

            var relatedLinks = JsonConvert.DeserializeObject<IEnumerable<RelatedLinkUdiModel>>(value);

            if (relatedLinks == null)
                return null;

            //Loop over array of links (Some may be external) so we can skip those
            //For each internal link get the current UDI of the picked node add it to the list of dependencies
            foreach (var internalLink in relatedLinks.Where(x => x.IsInternal))
            {
                //As the page was picked we need to add it to collection of depenencies for this deployment
                if (internalLink.Link != null)
                    dependencies.Add(new ArtifactDependency(internalLink.Link, false, ArtifactDependencyMode.Exist));
            }

            //As this property editor already stores UDIs and not ints
            //We need to return the original JSON string as we have not modified or replaced the values
            return property.Value.ToString();
        }


        public void SetValue(IContentBase content, string alias, string value)
        {
            //Save the JSON on the env we are deploying to
            //We don't need to swap UDIs out back for int's so a simple setValue is all we need
            content.SetValue(alias, value);
        }

        public virtual IEnumerable<string> PropertyEditorAliases => new[] { Constants.PropertyEditors.RelatedLinks2Alias };

        /// <summary>
        /// This is a subset of the full JSON object
        /// As we only need to know if a link is internal & the value of the internal link UDI that was picked
        /// </summary>
        public class RelatedLinkUdiModel
        {
            [JsonProperty("internal")]
            public Udi Link { get; set; }

            [JsonProperty("isInternal")]
            public bool IsInternal { get; set; }
        }
    }
}
