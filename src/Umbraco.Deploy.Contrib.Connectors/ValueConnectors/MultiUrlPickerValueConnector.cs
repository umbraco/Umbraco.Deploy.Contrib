using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Deploy;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Deploy.Environments;

namespace Umbraco.Deploy.Contrib.Connectors.ValueConnectors
{
    public class MultiUrlPickerValueConnector : IValueConnector
    {
        private readonly IEntityService _entityService;
        private readonly IMediaPaths _mediaPaths;
        private readonly IMediaService _mediaService;
        private readonly ILogger _logger;

        // Used to fetch the udi from a umb://-based url
        private static readonly Regex MediaUdiSrcRegex = new Regex(@"(?<udi>umb://media/[A-z0-9]+)", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiUrlPickerValueConnector"/> class.
        /// Source found here: https://github.com/rasmusjp/umbraco-multi-url-picker
        /// MultiUrlPicker can have a list of links, these can be document links, links to files in the
        /// media library, or just a text string with a link to whatever. We need to resolve if its a link
        /// to a document or a file in the media library, and transfer it as an artifact to the target environment.
        /// The object value is stored as json, and is an array of Links, defined with the following properties
        /// Link = {
        ///      id,
        ///      name,
        ///      url,
        ///      target,
        ///      isMedia,
        ///      icon
        /// }
        /// https://github.com/rasmusjp/umbraco-multi-url-picker/blob/master/src/RJP.MultiUrlPicker/App_Plugins/RJP.MultiUrlPicker/MultiUrlPicker.js#L120
        /// </summary>
        /// <param name="entityService">An <see cref="IEntityService"/> implementation.</param>
        /// <param name="mediaPaths"></param>
        /// <param name="mediaService"></param>
        /// <param name="logger"></param>
        public MultiUrlPickerValueConnector(IEntityService entityService, IMediaPaths mediaPaths, IMediaService mediaService, ILogger logger)
        {
            if (entityService == null) throw new ArgumentNullException(nameof(entityService));
            if (mediaService == null) throw new ArgumentNullException(nameof(mediaService));
            _entityService = entityService;
            _mediaPaths = mediaPaths;
            _mediaService = mediaService;
            _logger = logger;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<string> PropertyEditorAliases => new[]
        {
            "RJP.MultiUrlPicker"
        };

        public string GetValue(Property property, ICollection<ArtifactDependency> dependencies)
        {
            var svalue = property?.Value as string;
            if (string.IsNullOrWhiteSpace(svalue))
                return string.Empty;

            var linksJToken = JToken.Parse(svalue);

            if (linksJToken is JArray)
            {
                //Multiple links, parse as JArray    
                var links = JsonConvert.DeserializeObject<JArray>(svalue);
                if (links == null)
                    return string.Empty;

                foreach (var link in links)
                {
                    var isMedia = link["isMedia"] != null;
                    int intId;
                    string url;
                    // Only do processing if the Id is set on the element. OR if the url is set and its a media item
                    if (TryParseJTokenAttr(link, "id", out intId))
                    {
                        // Checks weather we are resolving a media item or a document
                        var objectTypeId = isMedia
                            ? UmbracoObjectTypes.Media
                            : UmbracoObjectTypes.Document;
                        var entityType = isMedia ? Constants.UdiEntityType.Media : Constants.UdiEntityType.Document;

                        var guidAttempt = _entityService.GetKeyForId(intId, objectTypeId);
                        if (guidAttempt.Success == false)
                            continue;

                        var udi = new GuidUdi(entityType, guidAttempt.Result);
                        // Add the artifact dependency
                        dependencies.Add(new ArtifactDependency(udi, false, ArtifactDependencyMode.Exist));

                        // Set the Id attribute to the udi
                        link["id"] = udi.ToString();
                    }
                    else if (isMedia && TryParseJTokenAttr(link, "url", out url))
                    {
                        // This state can happen due to an issue in RJP.MultiUrlPicker(or our linkPicker in RTE which it relies on), 
                        // where you edit a media link, and just hits "Select". 
                        // That will set the id to null, but the url will still be filled. We try to get the media item, and if so add it as 
                        // a dependency to the package. If we can't find it, we abort(aka continue)
                        //var media = _mediaService.GetMediaByPath(url);
                        var entry = _mediaPaths.Get(url);
                        if (entry == null)
                            continue;

                        // Add the artifact dependency
                        var udi = entry.Udi;
                        dependencies.Add(new ArtifactDependency(udi, false, ArtifactDependencyMode.Exist));

                        // Update the url on the item to the udi aka umb://media/fileguid
                        link["url"] = udi.ToString();
                    }
                }
                return JsonConvert.SerializeObject(links);
            }
            else if (linksJToken is JObject)
            {
                //Single link, parse as JToken    
                var linkToken = JsonConvert.DeserializeObject<JToken>(svalue);
                if (linkToken == null)
                    return string.Empty;

                var isMedia = linkToken["isMedia"] != null;
                int intId;
                string url;
                // Only do processing if the Id is set on the element. OR if the url is set and its a media item
                if (TryParseJTokenAttr(linkToken, "id", out intId))
                {
                    // Checks weather we are resolving a media item or a document
                    var objectTypeId = isMedia
                        ? UmbracoObjectTypes.Media
                        : UmbracoObjectTypes.Document;
                    var entityType = isMedia ? Constants.UdiEntityType.Media : Constants.UdiEntityType.Document;

                    var guidAttempt = _entityService.GetKeyForId(intId, objectTypeId);
                    if (guidAttempt.Success == false)
                    {

                        var udi = new GuidUdi(entityType, guidAttempt.Result);
                        // Add the artifact dependency
                        dependencies.Add(new ArtifactDependency(udi, false, ArtifactDependencyMode.Exist));

                        // Set the Id attribute to the udi
                        linkToken["id"] = udi.ToString();
                    }
                }
                else if (isMedia && TryParseJTokenAttr(linkToken, "url", out url))
                {
                    // This state can happen due to an issue in RJP.MultiUrlPicker(or our linkPicker in RTE which it relies on), 
                    // where you edit a media link, and just hits "Select". 
                    // That will set the id to null, but the url will still be filled. We try to get the media item, and if so add it as 
                    // a dependency to the package. If we can't find it, we abort(aka continue)
                    //var media = _mediaService.GetMediaByPath(url);
                    var entry = _mediaPaths.Get(url);
                    if (entry == null)
                    {
                        // Add the artifact dependency
                        var udi = entry.Udi;
                        dependencies.Add(new ArtifactDependency(udi, false, ArtifactDependencyMode.Exist));

                        // Update the url on the item to the udi aka umb://media/fileguid
                        linkToken["url"] = udi.ToString();
                    }
                }
                return JsonConvert.SerializeObject(linkToken);
            }

            //if none of the above...
            return string.Empty;
        }

        public void SetValue(IContentBase content, string alias, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                content.SetValue(alias, value);
                return;
            }

            var linksJToken = JToken.Parse(value);

            if (linksJToken is JArray)
            {
                //Multiple links, parse as JArray    
                var links = JsonConvert.DeserializeObject<JArray>(value);
                if (links != null)
                {
                    foreach (var link in links)
                    {
                        GuidUdi udi;
                        string url;
                        // Only do processing on an item if the Id or the url is set
                        if (TryParseJTokenAttr(link, "id", out udi))
                        {
                            // Check the type of the link
                            var nodeObjectType = link["isMedia"] != null
                                ? UmbracoObjectTypes.Media
                                : UmbracoObjectTypes.Document;

                            // Get the Id corresponding to the Guid
                            // it *should* succeed when deploying, due to dependencies management
                            // nevertheless, assume it can fail, and then create an invalid localLink
                            var idAttempt = _entityService.GetIdForKey(udi.Guid, nodeObjectType);
                            if (idAttempt)
                                link["id"] = idAttempt.Success ? idAttempt.Result : 0;
                        }
                        else if (TryParseJTokenAttr(link, "url", out url))
                        {
                            // Check weather the url attribut  of the link contains a udi, if so, replace it with the 
                            // path to the file, i.e. the regex replaces <udi> with /path/to/file
                            var newUrl = MediaUdiSrcRegex.Replace(url, match =>
                            {
                                var udiString = match.Groups["udi"].ToString();
                                GuidUdi foundUdi;
                                if (GuidUdi.TryParse(udiString, out foundUdi) && foundUdi.EntityType == Constants.UdiEntityType.Media)
                                {
                                    // (take care of nulls)
                                    var media = _mediaService.GetById(foundUdi.Guid);
                                    if (media != null)
                                        return media.GetUrl("umbracoFile", _logger);
                                }
                                return string.Empty;
                            });
                            link["url"] = newUrl;
                        }
                    }
                    content.SetValue(alias, JsonConvert.SerializeObject(links));
                }
            }
            else if (linksJToken is JObject)
            {
                //Single link, parse as JToken    
                var linkToken = JsonConvert.DeserializeObject<JToken>(value);

                GuidUdi udi;
                string url;
                // Only do processing on an item if the Id or the url is set
                if (TryParseJTokenAttr(linkToken, "id", out udi))
                {
                    // Check the type of the link
                    var nodeObjectType = linkToken["isMedia"] != null
                        ? UmbracoObjectTypes.Media
                        : UmbracoObjectTypes.Document;

                    // Get the Id corresponding to the Guid
                    // it *should* succeed when deploying, due to dependencies management
                    // nevertheless, assume it can fail, and then create an invalid localLink
                    var idAttempt = _entityService.GetIdForKey(udi.Guid, nodeObjectType);
                    if (idAttempt)
                        linkToken["id"] = idAttempt.Success ? idAttempt.Result : 0;
                }
                else if (TryParseJTokenAttr(linkToken, "url", out url))
                {
                    // Check weather the url attribut  of the link contains a udi, if so, replace it with the 
                    // path to the file, i.e. the regex replaces <udi> with /path/to/file
                    var newUrl = MediaUdiSrcRegex.Replace(url, match =>
                    {
                        var udiString = match.Groups["udi"].ToString();
                        GuidUdi foundUdi;
                        if (GuidUdi.TryParse(udiString, out foundUdi) && foundUdi.EntityType == Constants.UdiEntityType.Media)
                        {
                            // (take care of nulls)
                            var media = _mediaService.GetById(foundUdi.Guid);
                            if (media != null)
                                return media.GetUrl("umbracoFile", _logger);
                        }
                        return string.Empty;
                    });
                    linkToken["url"] = newUrl;
                }
                content.SetValue(alias, JsonConvert.SerializeObject(linkToken));
            }
        }

        private bool TryParseJTokenAttr(JToken link, string attrName, out int attrValue)
        {
            if (link[attrName] != null)
            {
                var val = link[attrName].ToString();
                return int.TryParse(val, out attrValue);
            }
            attrValue = 0;
            return false;
        }

        private bool TryParseJTokenAttr(JToken link, string attrName, out GuidUdi attrValue)
        {
            if (link[attrName] != null)
            {
                var val = link[attrName].ToString();
                return GuidUdi.TryParse(val, out attrValue);
            }
            attrValue = null;
            return false;
        }

        private bool TryParseJTokenAttr(JToken link, string attrName, out string strAttr)
        {
            if (link[attrName] != null)
            {
                var val = link[attrName].ToString();
                if (string.IsNullOrEmpty(val) == false)
                {
                    strAttr = val;
                    return true;
                }
            }
            strAttr = "";
            return false;
        }
    }
}
