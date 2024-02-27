using System;
using System.Collections.Generic;
using Umbraco.Core.Services;
using Umbraco.Deploy.ValueConnectors;

namespace Umbraco.Deploy.Contrib.Connectors.ValueConnectors
{
    /// <summary>
    /// A Deploy ValueConnector for the Content List property editor.
    /// https://github.com/umco/umbraco-content-list
    /// </summary>
    public class ContentListConnector : InnerContentConnector
    {
        public override IEnumerable<string> PropertyEditorAliases => new[] { "Our.Umbraco.ContentList" };

        public ContentListConnector(IContentTypeService contentTypeService, Lazy<ValueConnectorCollection> valueConnectors)
            : base(contentTypeService, valueConnectors)
        { }
    }
}