using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Deploy;

namespace UmbracoDeploy.Contrib.Export
{
    public static class ServiceConnectorExtensions
    {
        public static IEnumerable<IArtifact> GetArtifacts(this IServiceConnector serviceConnector, NamedUdiRange namedUdiRange)
        {
            var udis = new List<Udi>();
            serviceConnector.Explode(namedUdiRange, udis);

            foreach (var udi in udis)
            {
                if (serviceConnector.GetArtifact(udi) is IArtifact artifact)
                {
                    yield return artifact;
                }
            }
        }
    }
}