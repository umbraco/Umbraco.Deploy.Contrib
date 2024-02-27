//using System;
//using System.Collections.Generic;
//using Moq;
//using Newtonsoft.Json.Linq;
//using NUnit.Framework;
//using Umbraco.Core;
//using Umbraco.Core.Deploy;
//using Umbraco.Core.Models;
//using Umbraco.Core.Services;
//using Umbraco.Deploy.GridCellValueConnectors;
//using Umbraco.Deploy.ValueConnectors;

//namespace Umbraco.Deploy.Tests.Connectors
//{
//    [TestFixture]
//    public class LeBlenderGridCellValueConnectorTests
//    {
//        [Test]
//        [Ignore("Integration test for easier testing")]
//        public void GetValue()
//        {
//            var dataTypeServiceMock = new Mock<IDataTypeService>();
//            dataTypeServiceMock.Setup(x => x.GetDataTypeDefinitionById(new Guid("f38f0ac7-1d27-439c-9f3f-089cd8825a53")))
//                .Returns(new DataTypeDefinition(Constants.PropertyEditors.DropDownListMultipleAlias));
//            dataTypeServiceMock.Setup(x => x.GetDataTypeDefinitionById(new Guid("f0bc4bfb-b499-40d6-ba86-058885a5178c")))
//                .Returns(new DataTypeDefinition(Constants.PropertyEditors.NoEditAlias));
//            dataTypeServiceMock.Setup(x => x.GetDataTypeDefinitionById(new Guid("b91ca88a-24ed-46d6-907e-3512c2e5c786")))
//                .Returns(new DataTypeDefinition("Imulus.UrlPicker"));

//            var entityServiceMock = new Mock<IEntityService>();
//            var macroParserMock = new Mock<IMacroParser>();
//            var defaultValueConnector = new DefaultValueConnector();
//            var valueConnectors = new ValueConnectorCollection(new Dictionary<string, IValueConnector>
//            {
//                [Constants.PropertyEditors.DropDownListMultipleAlias] = defaultValueConnector,
//                [Constants.PropertyEditors.NoEditAlias] = defaultValueConnector,
//                ["Imulus.UrlPicker"] = new UrlPickerValueConnector(entityServiceMock.Object)
//            });

//            var connector = new LeBlenderGridCellValueConnector(dataTypeServiceMock.Object, macroParserMock.Object, new Lazy<ValueConnectorCollection>(() => valueConnectors));

//            var gridControl = new GridValue.GridControl()
//            {
//                Value =
//                    JToken.Parse(
//                        @"[{
//	                        ""dropdownMultiple"": {
//		                        ""value"": [
//			                        ""44"",
//			                        ""46""
//		                        ],
//		                        ""dataTypeGuid"": ""f38f0ac7-1d27-439c-9f3f-089cd8825a53"",
//		                        ""editorAlias"": ""dropdownMultiple"",
//		                        ""editorName"": ""Dropdown multiple""
//	                        },
//	                        ""label"": {
//		                        ""value"": null,
//		                        ""dataTypeGuid"": ""f0bc4bfb-b499-40d6-ba86-058885a5178c"",
//		                        ""editorAlias"": ""label"",
//		                        ""editorName"": ""Label""
//	                        },
//	                        ""urlPicker"": {
//		                        ""value"": ""[\n  {\n    \""type\"": \""content\"",\n    \""meta\"": {\n      \""title\"": \""Link to page 1\"",\n      \""newWindow\"": false\n    },\n    \""typeData\"": {\n      \""url\"": \""\"",\n      \""contentId\"": 1063,\n      \""mediaId\"": null\n    },\n    \""disabled\"": false\n  }\n]"",
//		                        ""dataTypeGuid"": ""b91ca88a-24ed-46d6-907e-3512c2e5c786"",
//		                        ""editorAlias"": ""urlPicker"",
//		                        ""editorName"": ""Url Picker""
//	                        }
//                        }]")

//            };

//            var value = connector.GetValue(gridControl, null, new List<ArtifactDependency>());

//            Assert.IsTrue(value.DetectIsJson());
//        }

//        [Test]
//        [Ignore("Integration test for easier testing")]
//        public void SetValue()
//        {
//            var dataTypeServiceMock = new Mock<IDataTypeService>();
//            dataTypeServiceMock.Setup(x => x.GetDataTypeDefinitionById(new Guid("f38f0ac7-1d27-439c-9f3f-089cd8825a53")))
//                .Returns(new DataTypeDefinition(Constants.PropertyEditors.DropDownListMultipleAlias));
//            dataTypeServiceMock.Setup(x => x.GetDataTypeDefinitionById(new Guid("f0bc4bfb-b499-40d6-ba86-058885a5178c")))
//                .Returns(new DataTypeDefinition(Constants.PropertyEditors.NoEditAlias));
//            dataTypeServiceMock.Setup(x => x.GetDataTypeDefinitionById(new Guid("b91ca88a-24ed-46d6-907e-3512c2e5c786")))
//                .Returns(new DataTypeDefinition("Imulus.UrlPicker"));

//            var entityServiceMock = new Mock<IEntityService>();
//            var macroParserMock = new Mock<IMacroParser>();
//            var defaultValueConnector = new DefaultValueConnector();
//            var valueConnectors = new ValueConnectorCollection(new Dictionary<string, IValueConnector>
//            {
//                [Constants.PropertyEditors.DropDownListMultipleAlias] = defaultValueConnector,
//                [Constants.PropertyEditors.NoEditAlias] = defaultValueConnector,
//                ["Imulus.UrlPicker"] = new UrlPickerValueConnector(entityServiceMock.Object)
//            });

//            var connector = new LeBlenderGridCellValueConnector(dataTypeServiceMock.Object, macroParserMock.Object, new Lazy<ValueConnectorCollection>(() => valueConnectors));

//            var gridControl = new GridValue.GridControl()
//            {
//                Value =
//                    JToken.Parse(
//                        @"[
//                            {
//                                ""dropdownMultiple"":{
//                                    ""value"":[
//                                    ""s44"",
//                                    ""s46""
//                                    ],
//                                    ""dataTypeGuid"":""f38f0ac7-1d27-439c-9f3f-089cd8825a53"",
//                                    ""editorAlias"":""dropdownMultiple"",
//                                    ""editorName"":""Dropdown multiple""
//                                },
//                                ""label"":{
//                                    ""value"":null,
//                                    ""dataTypeGuid"":""f0bc4bfb-b499-40d6-ba86-058885a5178c"",
//                                    ""editorAlias"":""label"",
//                                    ""editorName"":""Label""
//                                },
//                                ""urlPicker"":{
//                                    ""value"":""[{\""type\"":\""content\"",\""meta\"":{\""title\"":\""Link to page 1\"",\""newWindow\"":false},\""typeData\"":{\""url\"":\""\"",\""contentId\"":\""ef0bd0fc-46bf-402a-98c8-b6085eb117bd\"",\""mediaId\"":null},\""disabled\"":false}]"",
//                                    ""dataTypeGuid"":""b91ca88a-24ed-46d6-907e-3512c2e5c786"",
//                                    ""editorAlias"":""urlPicker"",
//                                    ""editorName"":""Url Picker""
//                                }
//                            }
//                        ]")

//            };

//            connector.SetValue(gridControl, new Property(new PropertyType(new DataTypeDefinition("mock"))));
//        }
//    }
//}
