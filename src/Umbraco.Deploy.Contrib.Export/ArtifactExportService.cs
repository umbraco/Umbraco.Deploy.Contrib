using System;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using Umbraco.Core;
using Umbraco.Core.Deploy;
using Umbraco.Deploy;
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
                DeployComponent.DiskEntityService.WriteFiles(exportPath, artifacts, fileTypes);

                // Create export archive and delete temp directory
                var zipArchive = new FastZip();
                zipArchive.CreateZip(zipArchiveFilePath, exportPath, true, null);
                Directory.Delete(exportPath, true);

                scope.Complete();
            }
        }
    }
}
