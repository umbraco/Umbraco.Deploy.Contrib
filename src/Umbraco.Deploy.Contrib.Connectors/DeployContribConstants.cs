namespace Umbraco.Deploy.Contrib.Connectors
{
    internal static class DeployContribConstants
    {
        public static class CacheKeys
        {
            // TODO (V10): Remove this and use the value defined in DeployConstants.CacheKeys
            public const string DeployConnectorCachePrefix = "DeployConnectorCache";

            public const string DeployConnectorCacheContentTypesFormat = DeployConnectorCachePrefix + "ContentTypes_{0}";
        }
    }
}
