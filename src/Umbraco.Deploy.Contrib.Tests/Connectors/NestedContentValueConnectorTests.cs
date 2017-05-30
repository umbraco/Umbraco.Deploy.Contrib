//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Newtonsoft.Json;
//using Umbraco.Core;
//using Umbraco.Core.Deploy;
//using Umbraco.Core.Models;
//using Umbraco.Core.Services;
//using Umbraco.Deploy.ValueConnectors;

//namespace Umbraco.Deploy.Contrib.Tests.Connectors
//{
//    [TestFixture]
//    public class NestedContentValueConnectorTests
//    {

//        [Test]
//        public void Get_Values_From_All_Sub_Property_Types()
//        {
//            //Arrange

//            //setup a fake connector to return a new Guid string for each call to GetValue
//            var fakeConnector = new Mock<IValueConnector>();
//            fakeConnector.Setup(x => x.GetValue(It.IsAny<Property>(), It.IsAny<ICollection<ArtifactDependency>>()))
//                .Returns(() => Guid.NewGuid().ToString());

//            var valueConnectors = new ValueConnectorCollection(new Dictionary<string, IValueConnector>
//            {
//                [Constants.PropertyEditors.TinyMCEAlias] = fakeConnector.Object,
//                [Constants.PropertyEditors.TextboxAlias] = fakeConnector.Object,
//                [Constants.PropertyEditors.MultipleTextstringAlias] = fakeConnector.Object
//            });

//            var mockPropertyTypes = new List<PropertyType>
//            {
//                new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Nvarchar, "text"),
//                new PropertyType(Constants.PropertyEditors.MultipleTextstringAlias, DataTypeDatabaseType.Nvarchar, "multiText"),
//                new PropertyType(Constants.PropertyEditors.TinyMCEAlias, DataTypeDatabaseType.Ntext, "rTE")
//            };

//            var mockContentType = Mock.Of<IContentType>(ct =>
//                ct.Alias == "nC1"
//                && ct.Name == "nC1"
//                && ct.Key == new Guid("2933d3c1-3c25-4292-92de-a1ec11ded0db")
//                && ct.CompositionPropertyTypes == mockPropertyTypes);

//            var mockContentTypeService = new Mock<IContentTypeService>();
//            mockContentTypeService.Setup(x => x.GetContentType(It.IsAny<string>()))
//                .Returns(() => mockContentType);

//            var nestedContentConnector = new NestedContentConnector(mockContentTypeService.Object, new Lazy<ValueConnectorCollection>(() => valueConnectors));

//            var propType = new PropertyType(Mock.Of<IDataTypeDefinition>(), "prop1");
//            var prop = new Property(propType, @"[
//            {""name"":""Content"",""ncContentTypeAlias"":""nC1"",""text"":""Hello"",""multiText"":""world"",""rTE"":""<p>asdfasdfasdfasdf</p>\n<p>asdf</p>\n<p><img style=\""width: 213px; height: 213px;\"" src=\""/media/1050/profile_pic_cg_2015.jpg?width=213&amp;height=213\"" alt=\""\"" rel=\""1087\"" data-id=\""1087\"" /></p>\n<p>asdf</p>""},
//            {""name"":""Content"",""ncContentTypeAlias"":""nC1"",""text"":""This is "",""multiText"":""pretty cool"",""rTE"":""""}
//            ]");

//            //Act

//            var result = nestedContentConnector.GetValue(prop, new List<ArtifactDependency>());

//            //Assert

//            Assert.IsTrue(result.DetectIsJson());
//            var resultModel = JsonConvert.DeserializeObject<NestedContentConnector.NestedContentValue[]>(result);
//            Assert.AreEqual(2, resultModel.Length);
//            var resultGuids = resultModel.SelectMany(x => x.PropertyValues.Values).ToArray();
//            //make sure the values returned are totally distinct - shows that a call to the GetValue
//            // was made for each cell
//            Assert.AreEqual(resultGuids.Length, resultGuids.Distinct().Count());
//            foreach (var cellGuid in resultGuids)
//            {
//                Guid g;
//                Assert.IsTrue(Guid.TryParse((string)cellGuid, out g));
//            }
//        }

//        [Test]
//        public void Set_Values_From_All_Sub_Property_Types()
//        {
//            //Arrange

//            //setup a fake connector to set a new Guid string for each call to SetValue
//            var fakeConnector = new Mock<IValueConnector>();
//            fakeConnector.Setup(x => x.SetValue(It.IsAny<IContentBase>(), It.IsAny<string>(), It.IsAny<string>()))
//                .Callback((IContentBase content, string alias, string val) =>
//                {
//                    content.SetValue(alias, Guid.NewGuid().ToString());
//                });

//            var valueConnectors = new ValueConnectorCollection(new Dictionary<string, IValueConnector>
//            {
//                [Constants.PropertyEditors.TinyMCEAlias] = fakeConnector.Object,
//                [Constants.PropertyEditors.TextboxAlias] = fakeConnector.Object,
//                [Constants.PropertyEditors.MultipleTextstringAlias] = fakeConnector.Object
//            });

//            var mockPropertyTypes = new List<PropertyType>
//            {
//                new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Nvarchar, "text"),
//                new PropertyType(Constants.PropertyEditors.MultipleTextstringAlias, DataTypeDatabaseType.Nvarchar, "multiText"),
//                new PropertyType(Constants.PropertyEditors.TinyMCEAlias, DataTypeDatabaseType.Ntext, "rTE")
//            };

//            var mockContentType = Mock.Of<IContentType>(ct =>
//                ct.Alias == "nC1"
//                && ct.Name == "nC1"
//                && ct.CompositionPropertyTypes == mockPropertyTypes);

//            var mockContentTypeService = new Mock<IContentTypeService>();
//            mockContentTypeService.Setup(x => x.GetContentType(It.IsAny<string>()))
//                .Returns(() => mockContentType);

//            var nestedContentConnector = new NestedContentConnector(mockContentTypeService.Object, new Lazy<ValueConnectorCollection>(() => valueConnectors));

//            var resultPropertyValue = string.Empty;
//            var mockContent = new Mock<IContent>();
//            mockContent.Setup(x => x.SetValue(It.IsAny<string>(), It.IsAny<object>()))
//                .Callback((string propAlias, object val) =>
//                {
//                    resultPropertyValue = val.ToString();
//                });

//            var strVal = @"[
//                {""name"":""Content"",""ncContentTypeAlias"":""nC1"",""text"":""Hello"",""multiText"":""world"",""rTE"":""<p>asdfasdfasdfasdf</p>\n<p>asdf</p>\n<p><img style=\""width: 213px; height: 213px;\"" src=\""/media/1050/profile_pic_cg_2015.jpg?width=213&amp;height=213\"" alt=\""\"" rel=\""1087\"" data-id=\""1087\"" /></p>\n<p>asdf</p>""},
//                {""name"":""Content"",""ncContentTypeAlias"":""nC1"",""text"":""This is "",""multiText"":""pretty cool"",""rTE"":""""}
//                ]";

//            //Act

//            nestedContentConnector.SetValue(
//                mockContent.Object,
//                "myNestedContentProperty",
//                strVal);

//            //Assert

//            Assert.IsFalse(resultPropertyValue.IsNullOrWhiteSpace());
//            var resultModel = JsonConvert.DeserializeObject<NestedContentConnector.NestedContentValue[]>(resultPropertyValue);
//            Assert.AreEqual(2, resultModel.Length);
//            var resultGuids = resultModel.SelectMany(x => x.PropertyValues.Values).ToArray();
//            //make sure the values returned are totally distinct - shows that a call to the SetValue
//            // was made for each cell
//            Assert.AreEqual(resultGuids.Length, resultGuids.Distinct().Count());
//            foreach (var cellGuid in resultGuids)
//            {
//                Guid g;
//                Assert.IsTrue(Guid.TryParse((string)cellGuid, out g));
//            }
//        }
//    }
//}