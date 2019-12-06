
namespace Umbraco.Deploy.Contrib.Connectors.GridCellValueConnectors
{
    using Deploy.Connectors.ValueConnectors.Services;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using Umbraco.Core;
    using Umbraco.Core.Deploy;
    using Umbraco.Core.Logging;
    using Umbraco.Core.Models;
    using Umbraco.Core.Services;

    public class DocTypeGridEditorCellValueConnector : IGridCellValueConnector
	{
		private readonly ILogger _logger;
		private readonly IContentTypeService _contentTypeService;
		private readonly Lazy<ValueConnectorCollection> _valueConnectorsLazy;
		private ValueConnectorCollection ValueConnectors => _valueConnectorsLazy.Value;

		public DocTypeGridEditorCellValueConnector(ILogger logger, IContentTypeService contentTypeService, Lazy<ValueConnectorCollection> valueConnectors)
		{
			this._logger = logger;
			this._contentTypeService = contentTypeService ?? throw new ArgumentNullException(nameof(contentTypeService));
			this._valueConnectorsLazy = valueConnectors ?? throw new ArgumentNullException(nameof(valueConnectors));
		}

		public bool IsConnector(string view)
		{
			return !string.IsNullOrWhiteSpace(view) && view.Contains("doctypegrideditor");
		}

		public string GetValue(GridValue.GridControl gridControl, ICollection<ArtifactDependency> dependencies)
		{
			// cancel if there's no values
			if (gridControl.Value == null || gridControl.Value.HasValues == false) return null;

			this._logger.Debug<DocTypeGridEditorCellValueConnector>($"GetValue() - Grid Values: {gridControl.Value}");

			var docTypeGridEditorContent = JsonConvert.DeserializeObject<DocTypeGridEditorValue>(gridControl.Value.ToString());

			// if an 'empty' dtge item has been added - it has no ContentTypeAlias set .. just return and don't throw.
			if (docTypeGridEditorContent == null || string.IsNullOrWhiteSpace(docTypeGridEditorContent.ContentTypeAlias))
			{
				this._logger.Debug<DocTypeGridEditorCellValueConnector>("GetValue() - DTGE Empty without ContentTypeAlias");
				return null;
			}

			this._logger.Debug<DocTypeGridEditorCellValueConnector>($"GetValue() - ContentTypeAlias - {docTypeGridEditorContent.ContentTypeAlias}");

			// check if the doc type exist - else abort packaging
			var contentType = this._contentTypeService.Get(docTypeGridEditorContent.ContentTypeAlias);

			if (contentType == null)
			{
				this._logger.Debug<DocTypeGridEditorCellValueConnector>("GetValue() - Missing ContentType");
				throw new InvalidOperationException($"Could not resolve the Content Type for the Doc Type Grid Editor property: {docTypeGridEditorContent.ContentTypeAlias}");
			}

			// add content type as a dependency
			dependencies.Add(new ArtifactDependency(contentType.GetUdi(), false, ArtifactDependencyMode.Match));

			// find all properties
			var propertyTypes = contentType.CompositionPropertyTypes;

			foreach (var propertyType in propertyTypes)
			{
				this._logger.Debug<DocTypeGridEditorCellValueConnector>($"GetValue() - PropertyTypeAlias - {propertyType.Alias}");

				// test if there's a value for the given property
				if (this.IsValueNull(docTypeGridEditorContent, propertyType, out var value)) continue;

				// if the value is an Udi then add it as a dependency
				if (this.AddUdiDependency(dependencies, value)) continue;

				JToken jtokenValue;
				var parsedValue = string.Empty;

				// throws if not found - no need for a null check
				var propValueConnector = this.ValueConnectors.Get(propertyType);

				this._logger.Debug<DocTypeGridEditorCellValueConnector>($"GetValue() - PropertyValueConnectorAlias - {propValueConnector.PropertyEditorAliases}");
				this._logger.Debug<DocTypeGridEditorCellValueConnector>($"GetValue() - propertyTypeValue - {value}");
				this._logger.Debug<DocTypeGridEditorCellValueConnector>($"GetValue() - IsJson - {this.IsJson(value)}");

				//properties like MUP / Nested Content seems to be in json and it breaks, so pass it back as jtokenValue right away
				if (this.IsJson(value))
				{
					jtokenValue = this.GetJTokenValue(value);
					this._logger.Debug<DocTypeGridEditorCellValueConnector>($"GetValue() - Inner JtokenValue - Eg MUP/NestedContent {jtokenValue}");
				}
				else
				{
					parsedValue = propValueConnector.ToArtifact(value, propertyType, dependencies);

					this._logger.Debug<DocTypeGridEditorCellValueConnector>($"GetValue() - ParsedValue - {parsedValue}");

					// if that's the case we'll need to add it as a json object instead of string to avoid it being escaped
					jtokenValue = this.GetJTokenValue(parsedValue);
				}

				if (jtokenValue != null)
				{
					this._logger.Debug<DocTypeGridEditorCellValueConnector>($"GetValue() - JtokenValue - {jtokenValue}");
					docTypeGridEditorContent.Value[propertyType.Alias] = jtokenValue;
				}
				else
				{
					docTypeGridEditorContent.Value[propertyType.Alias] = parsedValue;
				}
			}

			var resolvedValue = JsonConvert.SerializeObject(docTypeGridEditorContent);

			this._logger.Debug<DocTypeGridEditorCellValueConnector>($"GetValue() - ResolvedValue - {resolvedValue}");

			return resolvedValue;
		}

		public void SetValue(GridValue.GridControl gridControl)
		{
			if (string.IsNullOrWhiteSpace(gridControl.Value.ToString())) return;

			// For some reason the control value isn't properly parsed so we need this extra step to parse it into a JToken
			gridControl.Value = JToken.Parse(gridControl.Value.ToString());

			this._logger.Debug<DocTypeGridEditorCellValueConnector>($"SetValue() - GridControlValue - {gridControl.Value}");

			// cancel if there's no values
			if (gridControl.Value == null || gridControl.Value.HasValues == false) return;

			var docTypeGridEditorContent = JsonConvert.DeserializeObject<DocTypeGridEditorValue>(gridControl.Value.ToString());

			if (docTypeGridEditorContent == null) return;

			this._logger.Debug<DocTypeGridEditorCellValueConnector>($"SetValue() - ContentTypeAlias - {docTypeGridEditorContent.ContentTypeAlias}");

			// check if the doc type exist - else abort packaging
			var contentType = _contentTypeService.Get(docTypeGridEditorContent.ContentTypeAlias);

			if (contentType == null)
				throw new InvalidOperationException($"Could not resolve the Content Type for the Doc Type Grid Editor property: {docTypeGridEditorContent.ContentTypeAlias}");


			this._logger.Debug<DocTypeGridEditorCellValueConnector>($"SetValue() - ContentType - {contentType}");


			// find all properties
			var propertyTypes = contentType.CompositionPropertyTypes;

			foreach (var propertyType in propertyTypes)
			{
				// test if there's a value for the given property
				object value;
				object convertedValue;

				if (!docTypeGridEditorContent.Value.TryGetValue(propertyType.Alias, out value) || value == null)
				{
					this._logger.Debug<DocTypeGridEditorCellValueConnector>("SetValue() - Value Is Null");
					continue;
				}

				// throws if not found - no need for a null check
				var propValueConnector = this.ValueConnectors.Get(propertyType);
				propValueConnector.FromArtifact(value.ToString(), propertyType, "");

				this._logger.Debug<DocTypeGridEditorCellValueConnector>($"SetValue() - PropertyValueConnecterType - {propValueConnector.GetType()}");
				this._logger.Debug<DocTypeGridEditorCellValueConnector>($"SetValue() - Value - {value}");

				//don't convert if it's json / udi values
				//properties like Content/Media picker, single/multi are all in udis
				//properties like MUP / Nested Content are in json
				if (this.IsJson(value) || this.IsUdi(value))
				{
					this._logger.Debug<DocTypeGridEditorCellValueConnector>($"SetValue() - IsJsonValue / IsUdiValue - {value}");
					convertedValue = value.ToString();
				}
				else
				{
					//using mockContent to get the converted values
					var mockProperty = new Property(propertyType);
					var mockContent = new Content("mockContentGrid", -1, new ContentType(-1), new PropertyCollection(new List<Property> { mockProperty }));
					convertedValue = mockContent.GetValue(mockProperty.Alias);

					this._logger.Debug<DocTypeGridEditorCellValueConnector>($"SetValue() - ConvertedValue - Before - {convertedValue}");
				}

				// integers needs to be converted into strings for DTGE to work
				if (convertedValue is int)
				{
					docTypeGridEditorContent.Value[propertyType.Alias] = convertedValue.ToString();
				}
				else if (convertedValue == null)
				{
					//Assign the null back - otherwise the check for JSON will fail as we cant convert a null to a string
					//NOTE: LinkPicker2 for example if no link set is returning a null as opposed to empty string
					docTypeGridEditorContent.Value[propertyType.Alias] = null;
				}
				else
				{
					this._logger.Debug<DocTypeGridEditorCellValueConnector>($"SetValue() - ConvertedValue - After - {convertedValue}");

					// test if the value is a json object (thus could be a nested complex editor)
					// if that's the case we'll need to add it as a json object instead of string to avoid it being escaped
					var jtokenValue = this.GetJTokenValue(convertedValue);

					if (jtokenValue != null)
					{
						this._logger.Debug<DocTypeGridEditorCellValueConnector>($"SetValue() - jtokenValue - {jtokenValue}");
						docTypeGridEditorContent.Value[propertyType.Alias] = jtokenValue;
					}
					else
					{
						docTypeGridEditorContent.Value[propertyType.Alias] = convertedValue;
					}
				}
			}

			var jtokenObj = JToken.FromObject(docTypeGridEditorContent);

			this._logger.Debug<DocTypeGridEditorCellValueConnector>($"SetValue() - jtokenObject - {jtokenObj}");

			gridControl.Value = jtokenObj;
		}

		#region Private Methods

		private JToken GetJTokenValue(object value)
		{
			return value != null && this.IsJson(value) ? JToken.Parse(value.ToString()) : null;
		}

		private bool IsJson(object value)
		{
			return value.ToString().DetectIsJson();
		}

		private bool IsUdi(object value)
		{
			//for picker like content/media either single or multi, it comes with udi values
			return value.ToString().Contains("umb://");
		}

		private bool AddUdiDependency(ICollection<ArtifactDependency> dependencies, object value)
		{
			if (Udi.TryParse(value.ToString(), out var udi))
			{
				this._logger.Debug<DocTypeGridEditorCellValueConnector>($"GetValue() - Udi Dependency - {udi}");
				dependencies.Add(new ArtifactDependency(udi, false, ArtifactDependencyMode.Match));
				return true;
			}

			return false;
		}

		private bool IsValueNull(DocTypeGridEditorValue docTypeGridEditorContent, PropertyType propertyType, out object objVal)
		{
			if (!docTypeGridEditorContent.Value.TryGetValue(propertyType.Alias, out objVal) || objVal == null)
			{
				this._logger.Debug<DocTypeGridEditorCellValueConnector>("GetValue() - Value is null");
				return true;
			}

			return false;
		}

		#endregion

		#region DocTypeGridEditor model
		private class DocTypeGridEditorValue
		{
			[JsonProperty("id")]
			public Guid Id { get; set; }

			[JsonProperty("dtgeContentTypeAlias")]
			public string ContentTypeAlias { get; set; }

			[JsonProperty("value")]
			public Dictionary<string, object> Value { get; set; }
		}

		#endregion
	}
}
