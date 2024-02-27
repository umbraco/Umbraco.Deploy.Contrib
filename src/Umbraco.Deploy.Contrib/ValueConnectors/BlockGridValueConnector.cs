using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Services;
using Umbraco.Deploy.Core.Connectors.ValueConnectors.Services;

namespace Umbraco.Deploy.Contrib.ValueConnectors;

/// <summary>
/// A Deploy connector for the Block Grid property editor.
/// </summary>
/// <seealso cref="Umbraco.Deploy.Contrib.ValueConnectors.BlockEditorValueConnector" />
[Obsolete("Deploy 10.3.0 adds an explicit binding to use Umbraco.Deploy.Infrastructure.Connectors.ValueConnectors.BlockEditorValueConnector instead to support recursive migrators. This class will be removed in a future version.")]
public class BlockGridValueConnector : BlockEditorValueConnector
{
    /// <inheritdoc />
    public override IEnumerable<string> PropertyEditorAliases => new[]
    {
        "Umbraco.BlockGrid"
    };

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
