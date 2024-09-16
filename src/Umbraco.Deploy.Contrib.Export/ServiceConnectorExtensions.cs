using System;
using System.Collections.Generic;
using umbraco.BusinessLogic;
using Umbraco.Core;
using Umbraco.Core.Deploy;
using Umbraco.Core.Logging;

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
                IArtifact artifact;

                try
                {
                    artifact = serviceConnector.GetArtifact(udi);
                }
                catch (Exception ex)
                {
                    LogHelper.Error<Log>($"Error getting artifact: {udi}", ex);
                    continue;
                }

                if (artifact is IArtifact)
                {
                    yield return artifact;
                }
            }
        }
    }
}
