using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Services;
using Umbraco.Deploy.Core.Connectors.ValueConnectors.Services;

namespace Umbraco.Deploy.Contrib.Connectors.ValueConnectors
{
    /// <summary>
    /// A Deploy connector for the BlockList property editor
    /// </summary>
    public class BlockListValueConnector : BlockEditorValueConnector
    {
        public override IEnumerable<string> PropertyEditorAliases => new[] { "Umbraco.BlockList" };

        public BlockListValueConnector(IContentTypeService contentTypeService, Lazy<ValueConnectorCollection> valueConnectors, ILogger<BlockListValueConnector> logger)
            : base(contentTypeService, valueConnectors, logger)
        { }
    }
}
