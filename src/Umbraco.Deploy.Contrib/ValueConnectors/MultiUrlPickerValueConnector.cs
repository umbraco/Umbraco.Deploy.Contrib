using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Deploy.Core;
using Umbraco.Deploy.Core.Connectors.ValueConnectors;
using Umbraco.Extensions;

namespace Umbraco.Deploy.Contrib.ValueConnectors;

/// <summary>
/// A Deploy connector for the Umbraco.MultiUrlPicker property editor.
/// </summary>
/// <seealso cref="Umbraco.Deploy.Core.Connectors.ValueConnectors.ValueConnectorBase" />
[Obsolete("Deploy 12.1.0 adds an explicit binding to use Umbraco.Deploy.Infrastructure.Connectors.ValueConnectors.MultiUrlPickerValueConnector instead. This class will be removed in a future version.")]
public class MultiUrlPickerValueConnector : ValueConnectorBase
{
    private readonly IEntityService _entityService;
    private readonly IMediaService _mediaService;
    private readonly MediaUrlGeneratorCollection _mediaUrlGenerators;

    // Used to fetch the udi from a umb://-based url
    private static readonly Regex _mediaUdiSrcRegex = new Regex(@"(?<udi>umb://media/[A-z0-9]+)", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    /// <inheritdoc />
    public override IEnumerable<string> PropertyEditorAliases { get; } = new[]
    {
        "Umbraco.MultiUrlPicker"
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiUrlPickerValueConnector" /> class.
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
    /// <param name="entityService">An <see cref="IEntityService" /> implementation.</param>
    /// <param name="mediaService">The media service.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="mediaUrlGenerators">The media URL generators.</param>
    public MultiUrlPickerValueConnector(
        IEntityService entityService,
        IMediaService mediaService,
        ILogger<MultiUrlPickerValueConnector> logger,
        MediaUrlGeneratorCollection mediaUrlGenerators)
    {
        _entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
        _mediaService = mediaService ?? throw new ArgumentNullException(nameof(mediaService));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        _mediaUrlGenerators = mediaUrlGenerators;
    }

    public sealed override string? ToArtifact(object? value, IPropertyType propertyType, ICollection<ArtifactDependency> dependencies, IContextCache contextCache)
    {
        var svalue = value as string;
        if (string.IsNullOrWhiteSpace(svalue))
        {
            return null;
        }

        var valueAsJToken = JToken.Parse(svalue);
        if (valueAsJToken is JArray)
        {
            // Multiple links, parse as JArray
            var links = JsonConvert.DeserializeObject<JArray>(svalue);
            if (links == null)
            {
                return null;
            }

            foreach (var link in links)
            {
                var isMedia = link["isMedia"] != null;

                // Only do processing if the Id is set on the element. OR if the url is set and its a media item
                if (TryParseJTokenAttr(link, "id", out int intId))
                {
                    // Checks weather we are resolving a media item or a document
                    var objectTypeId = isMedia
                        ? UmbracoObjectTypes.Media
                        : UmbracoObjectTypes.Document;
                    var entityType = isMedia ? Constants.UdiEntityType.Media : Constants.UdiEntityType.Document;

                    var guidAttempt = contextCache.GetEntityKeyById(_entityService, intId, objectTypeId);
                    if (guidAttempt.Success == false)
                    {
                        continue;
                    }

                    var udi = new GuidUdi(entityType, guidAttempt.Result);

                    // Add the artifact dependency
                    dependencies.Add(new ArtifactDependency(udi, false, ArtifactDependencyMode.Exist));

                    // Set the Id attribute to the udi
                    link["id"] = udi.ToString();
                }
                else if (TryParseJTokenAttr(link, "udi", out GuidUdi? guidUdi))
                {
                    var entityExists = contextCache.EntityExists(_entityService, guidUdi.Guid);
                    if (!entityExists)
                    {
                        continue;
                    }

                    // Add the artifact dependency
                    dependencies.Add(new ArtifactDependency(guidUdi, false, ArtifactDependencyMode.Exist));
                }
                else if (isMedia && TryParseJTokenAttr(link, "url", out string url))
                {
                    // This state can happen due to an issue in RJP.MultiUrlPicker(or our linkPicker in RTE which it relies on),
                    // where you edit a media link, and just hit "Select".
                    // That will set the id to null, but the url will still be filled. We try to get the media item, and if so add it as
                    // a dependency to the package. If we can't find it, we abort(aka continue)
                    var entry = _mediaService.GetMediaByPath(url);
                    if (entry == null)
                    {
                        continue;
                    }

                    // Add the artifact dependency
                    var udi = entry.GetUdi();
                    dependencies.Add(new ArtifactDependency(udi, false, ArtifactDependencyMode.Exist));

                    // Update the url on the item to the udi aka umb://media/fileguid
                    link["url"] = udi.ToString();
                }
            }

            return JsonConvert.SerializeObject(links);
        }

        if (valueAsJToken is JObject)
        {
            // Single link, parse as JToken
            var link = JsonConvert.DeserializeObject<JToken>(svalue);
            if (link == null)
            {
                return string.Empty;
            }

            var isMedia = link["isMedia"] != null;

            // Only do processing if the Id is set on the element. OR if the url is set and its a media item
            if (TryParseJTokenAttr(link, "id", out int intId))
            {
                // Checks weather we are resolving a media item or a document
                var objectTypeId = isMedia
                    ? UmbracoObjectTypes.Media
                    : UmbracoObjectTypes.Document;
                var entityType = isMedia ? Constants.UdiEntityType.Media : Constants.UdiEntityType.Document;

                var guidAttempt = contextCache.GetEntityKeyById(_entityService, intId, objectTypeId);
                if (guidAttempt.Success)
                {
                    var udi = new GuidUdi(entityType, guidAttempt.Result);

                    // Add the artifact dependency
                    dependencies.Add(new ArtifactDependency(udi, false, ArtifactDependencyMode.Exist));

                    // Set the Id attribute to the udi
                    link["id"] = udi.ToString();
                }
            }
            else if (TryParseJTokenAttr(link, "udi", out GuidUdi? guidUdi))
            {
                if (contextCache.EntityExists(_entityService, guidUdi.Guid))
                {
                    // Add the artifact dependency
                    dependencies.Add(new ArtifactDependency(guidUdi, false, ArtifactDependencyMode.Exist));
                }
            }
            else if (isMedia && TryParseJTokenAttr(link, "url", out string url))
            {
                // This state can happen due to an issue in RJP.MultiUrlPicker(or our linkPicker in RTE which it relies on),
                // where you edit a media link, and just hits "Select".
                // That will set the id to null, but the url will still be filled. We try to get the media item, and if so add it as
                // a dependency to the package. If we can't find it, we abort(aka continue)
                var entry = _mediaService.GetMediaByPath(url);
                if (entry != null)
                {
                    // Add the artifact dependency
                    var udi = entry.GetUdi();
                    dependencies.Add(new ArtifactDependency(udi, false, ArtifactDependencyMode.Exist));

                    // Update the url on the item to the udi aka umb://media/fileguid
                    link["url"] = udi.ToString();
                }
            }

            return JsonConvert.SerializeObject(link);
        }

        // If none of the above...
        return string.Empty;
    }

    public sealed override object? FromArtifact(string? value, IPropertyType propertyType, object? currentValue, IContextCache contextCache)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var valueAsJToken = JToken.Parse(value);
        if (valueAsJToken is JArray links)
        {
            foreach (var link in links)
            {
                // Only do processing on an item if the Id or the url is set
                if (TryParseJTokenAttr(link, "id", out GuidUdi? udi))
                {
                    // Check the type of the link
                    var nodeObjectType = link["isMedia"] != null
                        ? UmbracoObjectTypes.Media
                        : UmbracoObjectTypes.Document;

                    // Get the Id corresponding to the Guid
                    // it *should* succeed when deploying, due to dependencies management
                    // nevertheless, assume it can fail, and then create an invalid localLink
                    var idAttempt = contextCache.GetEntityIdByKey(_entityService, udi.Guid, nodeObjectType);
                    if (idAttempt)
                    {
                        link["id"] = idAttempt.Success ? idAttempt.Result : 0;
                    }
                }
                else if (TryParseJTokenAttr(link, "url", out string url))
                {
                    // Check whether the url attribute of the link contains a udi, if so, replace it with the
                    // path to the file, i.e. the regex replaces <udi> with /path/to/file
                    var newUrl = _mediaUdiSrcRegex.Replace(url, match =>
                    {
                        var udiString = match.Groups["udi"].ToString();
                        if (UdiParser.TryParse(udiString, out var foundUdi) &&
                            foundUdi.EntityType == Constants.UdiEntityType.Media &&
                            foundUdi is GuidUdi guidUdi)
                        {
                            // (take care of nulls)
                            var media = _mediaService.GetById(guidUdi.Guid);
                            if (media != null)
                            {
                                return media.GetUrl("umbracoFile", _mediaUrlGenerators) ?? string.Empty;
                            }
                        }

                        return string.Empty;
                    });

                    link["url"] = newUrl;
                }
            }

            value = JsonConvert.SerializeObject(links);
        }
        else if (valueAsJToken is JObject link)
        {
            // Only do processing on an item if the Id or the url is set
            if (TryParseJTokenAttr(link, "id", out GuidUdi? udi))
            {
                // Check the type of the link
                var nodeObjectType = link["isMedia"] != null
                    ? UmbracoObjectTypes.Media
                    : UmbracoObjectTypes.Document;

                // Get the Id corresponding to the Guid
                // it *should* succeed when deploying, due to dependencies management
                // nevertheless, assume it can fail, and then create an invalid localLink
                var idAttempt = contextCache.GetEntityIdByKey(_entityService, udi.Guid, nodeObjectType);
                if (idAttempt)
                {
                    link["id"] = idAttempt.Success ? idAttempt.Result : 0;
                }
            }
            else if (TryParseJTokenAttr(link, "url", out string url))
            {
                // Check whether the url attribute of the link contains a udi, if so, replace it with the
                // path to the file, i.e. the regex replaces <udi> with /path/to/file
                var newUrl = _mediaUdiSrcRegex.Replace(url, match =>
                {
                    var udiString = match.Groups["udi"].ToString();
                    if (UdiParser.TryParse(udiString, out var foundUdi) &&
                        foundUdi.EntityType == Constants.UdiEntityType.Media &&
                        foundUdi is GuidUdi guidUdi)
                    {
                        // (take care of nulls)
                        var media = _mediaService.GetById(guidUdi.Guid);
                        if (media != null)
                        {
                            return media.GetUrl("umbracoFile", _mediaUrlGenerators) ?? string.Empty;
                        }
                    }

                    return string.Empty;
                });

                link["url"] = newUrl;
            }

            value = JsonConvert.SerializeObject(link);
        }

        return value;
    }

    private bool TryParseJTokenAttr(JToken link, string attrName, out int attrValue)
    {
        if (link[attrName] != null)
        {
            var val = link[attrName]?.ToString();
            return int.TryParse(val, out attrValue);
        }

        attrValue = 0;
        return false;
    }

    private bool TryParseJTokenAttr(JToken link, string attrName, [NotNullWhen(true)] out GuidUdi? attrValue)
    {
        if (link[attrName] != null)
        {
            var val = link[attrName]?.ToString();
            return UdiParser.TryParse(val, out attrValue);
        }

        attrValue = null;
        return false;
    }

    private bool TryParseJTokenAttr(JToken link, string attrName, out string strAttr)
    {
        if (link[attrName] != null)
        {
            var val = link[attrName]?.ToString();
            if (string.IsNullOrEmpty(val) == false)
            {
                strAttr = val;
                return true;
            }
        }

        strAttr = string.Empty;
        return false;
    }
}
