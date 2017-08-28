using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Hosting;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Deploy;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Umbraco.Deploy.Contrib.Connectors.ValueConnectors
{
    /// <summary>
    /// Implements a value connector for the url picker editor (Imulus.UrlPicker)
    /// https://our.umbraco.org/projects/backoffice-extensions/urlpicker/
    /// </summary>
    public class UrlPickerValueConnector : IValueConnector
    {
        private readonly IEntityService _entityService;

        private readonly Version UrlPickerVersionStoringArray = new Version(0, 15, 0, 1);

        private static Lazy<Version> UrlPickerVersion
        {
            get
            {
                return new Lazy<Version>(() =>
                {
                    // we cannot just get the assembly and get version from that, as we need to check the FileVersion
                    // (apparently that is different from the assembly version...)
                    var urlPickerAssemblyPath = HostingEnvironment.MapPath("~/bin/UrlPicker.dll");
                    if (System.IO.File.Exists(urlPickerAssemblyPath))
                    {
                        var fileVersionInfo = FileVersionInfo.GetVersionInfo(urlPickerAssemblyPath);
                        return new Version(fileVersionInfo.FileVersion);
                    }
                    return null;
                });
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlPickerValueConnector"/> class.
        /// </summary>
        /// <param name="entityService"></param>
        public UrlPickerValueConnector(IEntityService entityService)
        {
            if (entityService == null) throw new ArgumentNullException(nameof(entityService));

            _entityService = entityService;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<string> PropertyEditorAliases => new[] {"Imulus.UrlPicker"};

        /// <inheritdoc/>
        public string GetValue(Property property, ICollection<ArtifactDependency> dependencies)
        {
            var svalue = property?.Value as string;
            if (string.IsNullOrWhiteSpace(svalue))
                return string.Empty;

            IEnumerable<UrlPickerPropertyData> urlPickerPropertyDatas;

            // If using an old version of the UrlPicker - data is serialized as a single object
            // Otherwise data is serialized as an array of objects, even if only one is selected.
            if (UrlPickerVersion.Value < UrlPickerVersionStoringArray)
            {
                var urlPickerPropertyData = JsonConvert.DeserializeObject<UrlPickerPropertyData>(svalue);
                urlPickerPropertyDatas = new List<UrlPickerPropertyData> {urlPickerPropertyData};
            }
            else
            {
                urlPickerPropertyDatas = JsonConvert.DeserializeObject<IEnumerable<UrlPickerPropertyData>>(svalue)
                    .ToList();
            }


            foreach (var urlPicker in urlPickerPropertyDatas)
            {
                // If the contentId/mediaId of the TypeData is set try get the GuidUdi for the content/media and
                // mark it as a dependency we need to deploy.
                // We need the Guid for the content/media because the integer value could be different in the different environments.
                GuidUdi contentGuidUdi;
                if (TryGetGuidUdi(urlPicker.TypeData.ContentId, UmbracoObjectTypes.Document,
                    Constants.UdiEntityType.Document, out contentGuidUdi))
                {
                    dependencies.Add(new ArtifactDependency(contentGuidUdi, false, ArtifactDependencyMode.Exist));
                    urlPicker.TypeData.ContentId = contentGuidUdi.Guid;
                }

                // The picker can have values set for both content and media even though only one of them is "active".
                // We still need to resolve the value for both settings.
                GuidUdi mediaGuidUdi;
                if (TryGetGuidUdi(urlPicker.TypeData.MediaId, UmbracoObjectTypes.Media, Constants.UdiEntityType.Media,
                    out mediaGuidUdi))
                {
                    dependencies.Add(new ArtifactDependency(mediaGuidUdi, false, ArtifactDependencyMode.Exist));
                    urlPicker.TypeData.MediaId = mediaGuidUdi.Guid;
                }
            }
            // If using an old version of the UrlPicker - data is serialized as a single object
            // Otherwise data is serialized as an array of objects, even if only one is selected.
            if (UrlPickerVersion.Value < UrlPickerVersionStoringArray)
            {
                return JsonConvert.SerializeObject(urlPickerPropertyDatas.FirstOrDefault());
            }
            return JsonConvert.SerializeObject(urlPickerPropertyDatas);
        }

        /// <inheritdoc/>
        public void SetValue(IContentBase content, string alias, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                content.SetValue(alias, value);
                return;
            }

            IEnumerable<UrlPickerPropertyData> urlPickerPropertyDatas;

            // If using an old version of the UrlPicker - data is serialized as a single object
            // Otherwise data is serialized as an array of objects, even if only one is selected.
            if (UrlPickerVersion.Value < UrlPickerVersionStoringArray)
            {
                var urlPickerPropertyData = JsonConvert.DeserializeObject<UrlPickerPropertyData>(value);
                urlPickerPropertyDatas = new List<UrlPickerPropertyData> {urlPickerPropertyData};
            }
            else
            {
                urlPickerPropertyDatas = JsonConvert.DeserializeObject<IEnumerable<UrlPickerPropertyData>>(value).ToList();
            }

            foreach (var urlPicker in urlPickerPropertyDatas)
            {
                // When we set the value we want to switch the Guid value of the contentId/mediaId to the integervalue
                // as this is what the UrlPicker uses to lookup it's content/media
                int contentId;
                if (TryGetId(urlPicker.TypeData.ContentId, UmbracoObjectTypes.Document, out contentId))
                    urlPicker.TypeData.ContentId = contentId;

                // The picker can have values set for both content and media even though only one of them is "active".
                // We still need to resolve the value for both settings.
                int mediaId;
                if (TryGetId(urlPicker.TypeData.MediaId, UmbracoObjectTypes.Media, out mediaId))
                    urlPicker.TypeData.MediaId = mediaId;
            }

            // If using an old version of the UrlPicker - data is serialized as a single object
            // Otherwise data is serialized as an array of objects, even if only one is selected.
            if (UrlPickerVersion.Value < UrlPickerVersionStoringArray)
            {
                content.SetValue(alias, JsonConvert.SerializeObject(urlPickerPropertyDatas.FirstOrDefault()));
            }
            else
            {
                content.SetValue(alias, JsonConvert.SerializeObject(urlPickerPropertyDatas));
            }
        }

        private bool TryGetGuidUdi(object value, UmbracoObjectTypes umbracoObjectType, string entityType,
            out GuidUdi udi)
        {
            int id;
            if (value != null && int.TryParse(value.ToString(), out id))
            {
                var guidAttempt = _entityService.GetKeyForId(id, umbracoObjectType);
                if (guidAttempt.Success)
                {
                    udi = new GuidUdi(entityType, guidAttempt.Result);
                    return true;
                }
            }
            udi = null;
            return false;
        }

        private bool TryGetId(object value, UmbracoObjectTypes umbracoObjectType, out int id)
        {
            Guid key;
            if (value != null && Guid.TryParse(value.ToString(), out key))
            {
                var intAttempt = _entityService.GetIdForKey(key, umbracoObjectType);
                if (intAttempt.Success)
                {
                    id = intAttempt.Result;
                    return true;
                }
            }
            id = 0;
            return false;
        }

        internal class UrlPickerPropertyData
        {
            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("meta")]
            public Meta Meta { get; set; }

            [JsonProperty("typeData")]
            public TypeData TypeData { get; set; }

            [JsonProperty("disabled")]
            public bool Disabled { get; set; }
        }

        internal class Meta
        {
            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("newWindow")]
            public bool NewWindow { get; set; }
        }

        internal class TypeData
        {
            [JsonProperty("url")]
            public string Url { get; set; }

            [JsonProperty("contentId")]
            public object ContentId { get; set; }

            [JsonProperty("mediaId")]
            public object MediaId { get; set; }
        }
    }
}