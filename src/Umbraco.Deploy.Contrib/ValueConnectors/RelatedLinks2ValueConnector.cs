using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Deploy;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

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
        private readonly IEntityService _entityService;

        public RelatedLinks2ValueConnector(IEntityService entityService)
        {
            if (entityService == null) throw new ArgumentNullException(nameof(entityService));
            _entityService = entityService;
        }

        public string GetValue(Property property, ICollection<ArtifactDependency> dependencies)
        {

            var value = property.Value as string;

            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (value.DetectIsJson() == false)
                return null;

            var relatedLinks = new List<RelatedLinkUdiModel>();
            try
            {
                relatedLinks = JsonConvert.DeserializeObject<List<RelatedLinkUdiModel>>(value);
            }
            catch (JsonSerializationException)
            {
                // We might be transferring related links stored as int id, parse with ints instead.

                var relatedLinksInt = JsonConvert.DeserializeObject<JArray>(value);

                if (relatedLinksInt == null)
                    return string.Empty;

                foreach (var relatedLink in relatedLinksInt)
                {
                    //Get the value from the JSON object
                    var isInternal = Convert.ToBoolean(relatedLink["isInternal"]);

                    //We are only concerned about internal links
                    if (!isInternal)
                        continue;

                    var linkIntId = Convert.ToInt32(relatedLink["link"]);

                    //get the guid corresponding to the id
                    //it *can* fail if eg the id points to a deleted content,
                    //and then we use an empty guid
                    var guidAttempt = _entityService.GetKeyForId(linkIntId, UmbracoObjectTypes.Document);
                    if (guidAttempt.Success)
                    {
                        // replace the picked content id by the corresponding Udi as a dependancy
                        var udi = new GuidUdi(Constants.UdiEntityType.Document, guidAttempt.Result);

                        relatedLinks.Add(new RelatedLinkUdiModel { Link = udi, IsInternal = true });
                    }
                }
            }

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
