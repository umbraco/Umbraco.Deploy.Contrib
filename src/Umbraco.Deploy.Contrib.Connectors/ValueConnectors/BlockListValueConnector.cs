using System;
using System.Collections.Generic;
using Umbraco.Core.Cache;
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

        // TODO (V10): Remove this constructor.
        [Obsolete("Please use the constructor taking all parameters. This constructor will be removed in a future version.")]
        public BlockListValueConnector(IContentTypeService contentTypeService, Lazy<ValueConnectorCollection> valueConnectors, ILogger logger)
            : this(contentTypeService, valueConnectors, logger, Umbraco.Core.Composing.Current.AppCaches)
        {
        }

        public BlockListValueConnector(IContentTypeService contentTypeService, Lazy<ValueConnectorCollection> valueConnectors, ILogger logger, AppCaches appCaches)
            : base(contentTypeService, valueConnectors, logger, appCaches)
        { }
    }
}
