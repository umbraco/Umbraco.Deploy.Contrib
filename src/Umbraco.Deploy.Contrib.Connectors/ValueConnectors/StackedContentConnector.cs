using System;
using System.Collections.Generic;
using Umbraco.Core.Services;
using Umbraco.Deploy.ValueConnectors;

namespace Umbraco.Deploy.Contrib.Connectors.ValueConnectors
{
    /// <summary>
    /// A Deploy valueconnector for the StackedContent property editor
    /// </summary>
    public class StackedContentConnector : InnerContentConnector
    {
        public override IEnumerable<string> PropertyEditorAliases => new[] { "Our.Umbraco.StackedContent" };

        public StackedContentConnector(IContentTypeService contentTypeService, Lazy<ValueConnectorCollection> valueConnectors)
            : base(contentTypeService, valueConnectors)
        { }
    }
}
