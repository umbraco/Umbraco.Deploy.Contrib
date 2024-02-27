using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Deploy;
using Umbraco.Core.Models;
using Umbraco.Deploy;
using Umbraco.Deploy.Artifacts;
using Umbraco.Deploy.Files;

namespace UmbracoDeploy.Contrib.Export
{
    public static class ArtifactExportService
    {
        public static void ExportArtifacts(IEnumerable<Udi> udis, string zipArchiveFilePath, params string[] dependencyEntityTypes)
        {
            var artifacts = DeployComponent.ServiceConnectorFactory.GetArtifacts(udis, dependencyEntityTypes);

            ExportArtifacts(artifacts, zipArchiveFilePath);
        }

        public static void ExportArtifacts(IEnumerable<Udi> udis, string selector, string zipArchiveFilePath, params string[] dependencyEntityTypes)
        {
            var artifacts = DeployComponent.ServiceConnectorFactory.GetArtifacts(udis, selector, dependencyEntityTypes);

            ExportArtifacts(artifacts, zipArchiveFilePath);
        }

        public static void ExportArtifacts(IEnumerable<IArtifact> artifacts, string zipArchiveFilePath)
        {
            using (DeployComponent.DiskEntityService.SuppressFileTypeCollection(out FileTypeCollection fileTypes))
            using (var scope = ApplicationContext.Current.GetScopeProvider().CreateScope())
            {
                // Export to temp directory
                var exportPath = Path.Combine(Path.GetTempPath(), "Deploy", "export-" + Guid.NewGuid());
                DeployComponent.DiskEntityService.WriteArtifacts(exportPath, artifacts);
                AddPropertyEditorAliases(exportPath, artifacts.OfType<ContentArtifactBase>());
                DeployComponent.DiskEntityService.WriteFiles(exportPath, artifacts, fileTypes);

                // Create export archive and delete temp directory
                var zipArchive = new FastZip();
                zipArchive.CreateZip(zipArchiveFilePath, exportPath, true, null);
                Directory.Delete(exportPath, true);

                scope.Complete();
            }
        }


        private static void AddPropertyEditorAliases(string path, IEnumerable<ContentArtifactBase> artifacts)
        {
            var contentTypes = ApplicationContext.Current.Services.ContentTypeService.GetAllContentTypes();

            foreach (var artifact in artifacts)
            {
                var filePath = Path.Combine(path, artifact.Udi.EntityType + "__" + artifact.Udi.Guid.ToString("n") + ".uda");

                JObject json;
                using (var file = System.IO.File.OpenText(filePath))
                using (var reader = new JsonTextReader(file))
                {
                    json = (JObject)JToken.ReadFrom(reader);
                }

                json["PropertyEditorAliases"] = GetPropertyEditorAliases(contentTypes, artifact);

                using (var file = System.IO.File.CreateText(filePath))
                using (var writer = new JsonTextWriter(file))
                {
                    writer.Formatting = Formatting.Indented;

                    json.WriteTo(writer);
                }
            }
        }

        private static JToken GetPropertyEditorAliases(IEnumerable<IContentType> contentTypes, ContentArtifactBase artifact)
        {
            var propertyEditorAliases = new Dictionary<string, string>();

            foreach (var contentType in contentTypes)
            {
                string prefix;
                if (contentType.Key == artifact.ContentType.Guid)
                {
                    // Add property editor aliases for the content type
                    prefix = null;
                }
                else if (artifact.Dependencies.Any(x => x.Udi is GuidUdi guidUdi && guidUdi.EntityType == Constants.UdiEntityType.DocumentType && guidUdi.Guid == contentType.Key))
                {
                    // Add prefixed property editor aliases for related content types (e.g. used by Nested Content)
                    prefix = contentType.Alias + ".";
                }
                else
                {
                    continue;
                }

                foreach (var propertyType in contentType.CompositionPropertyTypes)
                {
                    propertyEditorAliases[prefix + propertyType.Alias] = propertyType.PropertyEditorAlias;
                }
            }

            return JToken.FromObject(propertyEditorAliases);
        }
    }
}
