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
    public class BlockListValueConnector : BlockEditorValueConnector
    {
        public override IEnumerable<string> PropertyEditorAliases => new[] { "Umbraco.BlockList" };

        public BlockListValueConnector(IContentTypeService contentTypeService, Lazy<ValueConnectorCollection> valueConnectors, ILogger logger)
            : base(contentTypeService, valueConnectors, logger)
        { }
    }
}
