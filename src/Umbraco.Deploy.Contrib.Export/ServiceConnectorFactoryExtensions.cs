using System.Collections.Generic;
using System.Linq;
using umbraco.BusinessLogic;
using Umbraco.Core;
using Umbraco.Core.Deploy;
using Umbraco.Core.Logging;
using Umbraco.Deploy.ServiceConnectors;

namespace UmbracoDeploy.Contrib.Export
{
    public static class ServiceConnectorFactoryExtensions
    {
        public static IEnumerable<IArtifact> GetArtifacts(this IServiceConnectorFactory serviceConnectorFactory, IEnumerable<Udi> udis)
        {
            foreach (var udiByType in udis.Distinct().GroupBy(x => x.EntityType))
            {
                if (serviceConnectorFactory.GetConnector(udiByType.Key) is IServiceConnector serviceConnector)
                {
                    foreach (var udi in udiByType)
                    {
                        LogHelper.Info<Log>($"Getting Artifact: {udi}");

                        if (serviceConnector.GetArtifact(udi) is IArtifact artifact)
                        {
                            yield return artifact;
                        }
                    }
                }
            }
        }

        public static IEnumerable<IArtifact> GetArtifacts(this IServiceConnectorFactory serviceConnectorFactory, IEnumerable<Udi> udis, string selector)
        {
            foreach (var udiByType in udis.Distinct().GroupBy(x => x.EntityType))
            {
                if (serviceConnectorFactory.GetConnector(udiByType.Key) is IServiceConnector serviceConnector)
                {
                    foreach (var udi in udiByType)
                    {
                        var namedUdiRange = serviceConnector.GetRange(udi, selector);

                        LogHelper.Info<Log>($"Getting Artifacts for named range: {namedUdiRange}");

                        foreach (var artifact in serviceConnector.GetArtifacts(namedUdiRange))
                        {
                            yield return artifact;
                        }
                    }
                }
            }
        }

        public static IEnumerable<IArtifact> GetArtifacts(this IServiceConnectorFactory serviceConnectorFactory, IEnumerable<Udi> udis, string[] dependencyEntityTypes)
            => GetArtifactsRecursive(serviceConnectorFactory, GetArtifacts(serviceConnectorFactory, udis), dependencyEntityTypes);

        public static IEnumerable<IArtifact> GetArtifacts(this IServiceConnectorFactory serviceConnectorFactory, IEnumerable<Udi> udis, string selector, string[] dependencyEntityTypes)
            => GetArtifactsRecursive(serviceConnectorFactory, GetArtifacts(serviceConnectorFactory, udis, selector), dependencyEntityTypes);

        private static IEnumerable<IArtifact> GetArtifactsRecursive(IServiceConnectorFactory serviceConnectorFactory, IEnumerable<IArtifact> artifacts, string[] dependencyEntityTypes)
        {
            var returnedUdis = new HashSet<Udi>();

            foreach (IArtifact artifact in artifacts)
            {
                if (!returnedUdis.Add(artifact.Udi))
                {
                    // Already returned, so prevent recursive loop
                    continue;
                }

                // Recurse artifact dependencies
                foreach (var dependencyArtifact in GetArtifactsRecursive(serviceConnectorFactory, artifact.Dependencies, dependencyEntityTypes, returnedUdis))
                {
                    yield return dependencyArtifact;
                }

                yield return artifact;
            }
        }

        private static IEnumerable<IArtifact> GetArtifactsRecursive(IServiceConnectorFactory serviceConnectorFactory, IEnumerable<ArtifactDependency> artifactDependencies, string[] entityTypes, ISet<Udi> returnedUdis)
        {
            // Only process specified entity types
            var udis = artifactDependencies.Select(x => x.Udi).Where(x => entityTypes.Contains(x.EntityType));

            foreach (var dependencyArtifact in GetArtifacts(serviceConnectorFactory, udis))
            {
                if (!returnedUdis.Add(dependencyArtifact.Udi))
                {
                    // Already returned, so prevent recursive loop
                    continue;
                }

                // Recurse artifact dependencies
                foreach (var recursiveDependencyArtifact in GetArtifactsRecursive(serviceConnectorFactory, dependencyArtifact.Dependencies, entityTypes, returnedUdis))
                {
                    yield return recursiveDependencyArtifact;
                }

                yield return dependencyArtifact;
            }
        }
    }
}
