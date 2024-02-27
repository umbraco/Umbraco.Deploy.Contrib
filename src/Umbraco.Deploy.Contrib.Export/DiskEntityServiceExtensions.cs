using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Umbraco.Core;
using Umbraco.Core.Deploy;
using Umbraco.Deploy;
using Umbraco.Deploy.Artifacts;
using Umbraco.Deploy.Disk;
using Umbraco.Deploy.Files;

namespace UmbracoDeploy.Contrib.Export
{
    internal static partial class DiskEntityServiceExtensions
    {
        public static IDisposable SuppressFileTypeCollection(this IDiskEntityService diskEntityService, out FileTypeCollection fileTypes)
        {
            var field = diskEntityService.GetType().GetField("_fileTypes", BindingFlags.Instance | BindingFlags.NonPublic);
            var oldFileTypes = field.GetValue(diskEntityService) as FileTypeCollection;

            // Replace with empty file type collection (so all artifacts are written as UDA files)
            field.SetValue(diskEntityService, new FileTypeCollection(Enumerable.Empty<IFileType>()));
            
            fileTypes = oldFileTypes;

            // Restore on dispose
            return new InvokeOnDispose(() => field.SetValue(diskEntityService, oldFileTypes));
        }

        public static void WriteArtifacts(this IDiskEntityService diskEntityService, string path, IEnumerable<IArtifact> artifacts)
        {
            var field = diskEntityService.GetType().GetField("_afs", BindingFlags.Instance | BindingFlags.NonPublic);
            var oldArtifactFileSystem = field.GetValue(diskEntityService);

            // Replace artifact file system to use custom path to write artifacts to
            field.SetValue(diskEntityService, new ArtifactFileSystem(path));

            try
            {
                // Ensure directory exists
                Directory.CreateDirectory(path);

                diskEntityService.WriteArtifacts(artifacts);

            }
            finally
            {
                // Restore after artifacts are written
                field.SetValue(diskEntityService, oldArtifactFileSystem);
            }
        }

        public static void WriteFiles(this IDiskEntityService diskEntityService, string path, IEnumerable<IArtifact> artifacts, FileTypeCollection fileTypes)
        {
            foreach (var artifactByType in artifacts.GroupBy(x => x.Udi.EntityType))
            {
                if (!fileTypes.Contains(artifactByType.Key))
                {
                    continue;
                }

                var fileType = fileTypes[artifactByType.Key];
                foreach (var udi in artifactByType.Select(x => x.Udi as StringUdi).WhereNotNull())
                {
                    // Ensure directory exists (since the path might not exist yet)
                    var filePath = Path.Combine(path, artifactByType.Key, udi.Id);
                    if (Path.GetDirectoryName(filePath) is string directoryPath)
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    // Write file
                    using (Stream stream = fileType.GetStream(udi))
                    using (Stream destination = System.IO.File.OpenWrite(filePath))
                    {
                        stream.CopyTo(destination);
                    }
                }
            }
        }

        private sealed class InvokeOnDispose : IDisposable
        {
            private readonly Action action;

            public InvokeOnDispose(Action action) => this.action = action;

            public void Dispose() => action.Invoke();
        }
    }
}
