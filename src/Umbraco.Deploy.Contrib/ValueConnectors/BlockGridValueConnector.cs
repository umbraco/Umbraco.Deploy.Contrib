using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Services;
using Umbraco.Deploy.Core.Connectors.ValueConnectors.Services;

namespace Umbraco.Deploy.Contrib.ValueConnectors
{
    /// <summary>
    /// A Deploy connector for the BlockGrid property editor.
    /// </summary>
    /// <seealso cref="Umbraco.Deploy.Contrib.ValueConnectors.BlockEditorValueConnector" />
    public class BlockGridValueConnector : BlockEditorValueConnector
    {
        /// <inheritdoc />
        public override IEnumerable<string> PropertyEditorAliases => new[] { "Umbraco.BlockGrid" };

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockGridValueConnector" /> class.
        /// </summary>
        /// <param name="contentTypeService">The content type service.</param>
        /// <param name="valueConnectors">The value connectors.</param>
        /// <param name="logger">The logger.</param>
        public BlockGridValueConnector(IContentTypeService contentTypeService, Lazy<ValueConnectorCollection> valueConnectors, ILogger<BlockGridValueConnector> logger)
            : base(contentTypeService, valueConnectors, logger)
        { }
    }
}
