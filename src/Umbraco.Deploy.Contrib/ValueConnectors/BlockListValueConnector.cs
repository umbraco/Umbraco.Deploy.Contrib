using System;
using System.Collections.Generic;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Deploy.Connectors.ValueConnectors.Services;

namespace Umbraco.Deploy.Contrib.Connectors.ValueConnectors
{
    /// <summary>
    /// A Deploy connector for the BlockList property editor
    /// </summary>
    [Obsolete("Deploy 4.9.0 adds an explicit binding to use Umbraco.Deploy.Connectors.ValueConnectors.BlockEditorValueConnector instead to support recursive migrators. This class will be removed in a future version.")]
    public class BlockListValueConnector : BlockEditorValueConnector
    {
        public override IEnumerable<string> PropertyEditorAliases { get; } = new[]
        {
            "Umbraco.BlockList"
        };

        public BlockListValueConnector(IContentTypeService contentTypeService, Lazy<ValueConnectorCollection> valueConnectors, ILogger logger)
            : base(contentTypeService, valueConnectors, logger)
        { }
    }
}
