using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Services;
using Umbraco.Deploy.Core.Connectors.ValueConnectors.Services;

namespace Umbraco.Deploy.Contrib.ValueConnectors;

/// <summary>
/// A Deploy connector for the Block List property editor.
/// </summary>
/// <seealso cref="Umbraco.Deploy.Contrib.ValueConnectors.BlockEditorValueConnector" />
[Obsolete("Deploy 10.3.0 adds an explicit binding to use Umbraco.Deploy.Infrastructure.Connectors.ValueConnectors.BlockEditorValueConnector instead to support recursive migrators. This class will be removed in a future version.")]
public class BlockListValueConnector : BlockEditorValueConnector
{
    public override IEnumerable<string> PropertyEditorAliases { get; } = new[]
    {
        "Umbraco.BlockList"
    };

    public BlockListValueConnector(IContentTypeService contentTypeService, Lazy<ValueConnectorCollection> valueConnectors, ILogger<BlockListValueConnector> logger)
        : base(contentTypeService, valueConnectors, logger)
    { }
}
