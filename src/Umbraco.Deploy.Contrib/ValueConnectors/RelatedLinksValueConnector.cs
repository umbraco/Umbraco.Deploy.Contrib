using System;
using System.Collections.Generic;
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
        "caption": "Page B Internal Link made here",
        "link": 1059,
        "newWindow": false,
        "internal": 1059,
        "edit": false,
        "isInternal": true,
        "internalName": "Page B",
        "internalIcon": "icon-document",
        "type": "internal",
        "title": "Page B Internal Link made here"
      }
    ]
    */

    /// <summary>
    /// Implements a value connector for the old related links editor (storing integer ids).
    /// </summary>
    public class RelatedLinksValueConnector : IValueConnector
    {
        private readonly IEntityService _entityService;

        public RelatedLinksValueConnector(IEntityService entityService)
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

            var relatedLinks = JsonConvert.DeserializeObject<JArray>(value);

            if (relatedLinks == null)
                return string.Empty;

            foreach (var relatedLink in relatedLinks)
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
                    dependencies.Add(new ArtifactDependency(udi, false, ArtifactDependencyMode.Exist));

                    //Set the current relatedlink 'internal' & 'link' properties to UDIs & not int's
                    relatedLink["link"] = udi.ToString();
                    relatedLink["internal"] = udi.ToString();
                }
            }

            //Serialise the new updated object with UDIs in it to JSON to transfer across the wire
            return JsonConvert.SerializeObject(relatedLinks);
        }

        public void SetValue(IContentBase content, string alias, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                content.SetValue(alias, value);
                return;
            }

            var relatedLinks = JsonConvert.DeserializeObject<JArray>(value);

            foreach (var relatedLink in relatedLinks)
            {
                //Get the value from the JSON object
                var isInternal = Convert.ToBoolean(relatedLink["isInternal"]);

                //We are only concerned about internal links
                if (!isInternal)
                    continue;

                var relatedLinkValue = relatedLink["link"].ToString();

                //Check if related links is stored as an int
                if (int.TryParse(relatedLinkValue, out var relatedLinkInt))
                {
                    //Update the JSON back to the int ids on this env
                    relatedLink["link"] = relatedLinkInt;
                    relatedLink["internal"] = relatedLinkInt;
                }
                else
                {
                    //Get the UDI value in the JSON
                    var pickedUdi = GuidUdi.Parse(relatedLinkValue);

                    //Lets use entitiy sevice to get the int ID for this item on the new environment
                    //Get the Id corresponding to the Guid
                    //it *should* succeed when deploying, due to dependencies management
                    //nevertheless, assume it can fail, and then create an invalid localLink
                    var idAttempt = _entityService.GetIdForKey(pickedUdi.Guid, UmbracoObjectTypes.Document);

                    //Update the JSON back to the int ids on this env
                    relatedLink["link"] = idAttempt.Success ? idAttempt.Result : 0;
                    relatedLink["internal"] = idAttempt.Success ? idAttempt.Result : 0;
                }
            }

            //Save the updated JSON with replaced UDIs for int IDs
            content.SetValue(alias, JsonConvert.SerializeObject(relatedLinks));
        }

#pragma warning disable CS0618 // Type or member is obsolete
        public virtual IEnumerable<string> PropertyEditorAliases => new[] { Constants.PropertyEditors.RelatedLinksAlias };
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
