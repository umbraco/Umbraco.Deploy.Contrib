//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Newtonsoft.Json;
//using NUnit.Framework;
//using Umbraco.Core;
//using Umbraco.Core.Deploy;
//using Umbraco.Core.Models;
//using Umbraco.Core.Services;
//using Umbraco.Deploy.Tests.TestHelpers;
//using Umbraco.Deploy.ValueConnectors;

//namespace Umbraco.Deploy.Contrib.Tests.Connectors
//{
//    public class UrlPickerValueConnectorTests
//    {
//        private ServiceContext _services;

//        [TestFixtureSetUp]
//        public void Setup()
//        {
//            _services = new ServiceContext(
//                contentTypeService: new MockServices.ContentTypeService(),
//                contentService: new MockServices.ContentService(),
//                entityService: new MockServices.EntityService(),
//                mediaService: new MockServices.MediaService(),
//                macroService: new MockServices.MacroService()
//            );

//            var testableEntityService = (ITestableEntityService)_services.EntityService;

//            var contentType = MockedContentTypes.CreateTextpageContentType();
//            _services.ContentTypeService.Save(contentType);

//            var imageMediaType = MockedContentTypes.CreateImageMediaType();
//            _services.ContentTypeService.Save(imageMediaType);

//            var media1 = MockedMedia.CreateCuteKittenMedia(imageMediaType);
//            media1.Id = 1110;
//            media1.Key = Guid.Parse("5f31133c38994e46bad8f53be105f71d");
//            _services.MediaService.Save(media1);
//            testableEntityService.Save(media1);

//            var content = MockedContent.CreateTextpageContent(contentType, "Text page", -1);
//            content.Id = 1120;
//            content.Key = Guid.Parse("bee9b27cafd040d5968ae0974273eddd");
//            content.SetValue("bodyText", MockedValues.HtmlValueWith3Images2Unique);
//            _services.ContentService.Save(content);
//            testableEntityService.Save(content);
//        }

//        [Test]
//        public void GetValue_Returns_Empty_String_For_Null_Or_Empty_Value()
//        {
//            var urlPickerValueConnector = new UrlPickerValueConnector(_services.EntityService);
//            var dependencies = new List<ArtifactDependency>();
//            var propertyType = new PropertyType("Imulus.UrlPicker", DataTypeDatabaseType.Ntext, "alias");
//            var valueForEmptyString = urlPickerValueConnector.GetValue(new Property(propertyType, string.Empty), dependencies);
//            Assert.IsNullOrEmpty(valueForEmptyString);
//            var valueForNull = urlPickerValueConnector.GetValue(new Property(propertyType, null), dependencies);
//            Assert.IsNullOrEmpty(valueForNull);
//        }

//        [Test]
//        public void GetValue_Adds_Content_And_Media_As_Dependencies_And_Converts_To_Guids()
//        {
//            var urlPickerValueConnector = new UrlPickerValueConnector(_services.EntityService);
//            var dependencies = new List<ArtifactDependency>();
//            var property = CreateUrlPickerProperty(CreateUrlPickerPropertyData("content", "This is a title", 1120, 1110));
//            var value = urlPickerValueConnector.GetValue(property, dependencies);
//            var convertedValue = JsonConvert.DeserializeObject<IEnumerable<UrlPickerValueConnector.UrlPickerPropertyData>>(value);
//            var contentGuid = Guid.Parse("bee9b27cafd040d5968ae0974273eddd");
//            var mediaGuid = Guid.Parse("5f31133c38994e46bad8f53be105f71d");
//            var contentDependency = dependencies.SingleOrDefault(x => ((GuidUdi)x.Udi).Guid == contentGuid);
//            var mediaDependency = dependencies.SingleOrDefault(x => ((GuidUdi)x.Udi).Guid == mediaGuid);
//            Assert.NotNull(contentDependency);
//            Assert.NotNull(mediaDependency);
//            var typeData = convertedValue.First().TypeData;
//            Assert.AreEqual(contentGuid.ToString(), typeData.ContentId);
//            Assert.AreEqual(mediaGuid.ToString(), typeData.MediaId);
//        }

//        [Test]
//        public void SetValue_Sets_Empty_Or_Null_Value()
//        {
//            var urlPickerValueConnector = new UrlPickerValueConnector(_services.EntityService);
//            var contentType = MockedContentTypes.CreateUrlPickerPageContentType();
//            var content = new Content("test", -1, contentType);
//            urlPickerValueConnector.SetValue(content, "urlPicker", string.Empty);
//            Assert.IsNullOrEmpty(content.GetValue("urlPicker").ToString());
//            urlPickerValueConnector.SetValue(content, "urlPicker", null);
//            Assert.IsNull(content.GetValue("urlPicker"));
//        }

//        [Test]
//        public void SetValue_Converts_Guids_To_Integers()
//        {
//            var urlPickerValueConnector = new UrlPickerValueConnector(_services.EntityService);
//            var contentType = MockedContentTypes.CreateUrlPickerPageContentType();
//            var content = new Content("test", -1, contentType);
//            var contentGuid = Guid.Parse("bee9b27cafd040d5968ae0974273eddd");
//            var mediaGuid = Guid.Parse("5f31133c38994e46bad8f53be105f71d");
//            urlPickerValueConnector.SetValue(content, "urlPicker", JsonConvert.SerializeObject(new[] { CreateUrlPickerPropertyData("content", "This is a title", contentGuid.ToString(), mediaGuid.ToString()) }));
//            var convertedValue = JsonConvert.DeserializeObject<IEnumerable<UrlPickerValueConnector.UrlPickerPropertyData>>(content.GetValue("urlPicker").ToString()).First();
//            Assert.AreEqual(1120, convertedValue.TypeData.ContentId);
//            Assert.AreEqual(1110, convertedValue.TypeData.MediaId);
//        }

//        private UrlPickerValueConnector.UrlPickerPropertyData CreateUrlPickerPropertyData(string type, string title = null, object contentId = null, object mediaId = null, string url = null)
//        {
//            return new UrlPickerValueConnector.UrlPickerPropertyData()
//            {
//                Type = type,
//                Meta = new UrlPickerValueConnector.Meta()
//                {
//                    Title = title
//                },
//                TypeData = new UrlPickerValueConnector.TypeData()
//                {
//                    ContentId = contentId,
//                    MediaId = mediaId,
//                    Url = url
//                }
//            };
//        }

//        private Property CreateUrlPickerProperty(UrlPickerValueConnector.UrlPickerPropertyData urlPickerPropertyData)
//        {
//            var propertyType = new PropertyType("Imulus.UrlPicker", DataTypeDatabaseType.Ntext, "alias");
//            var property = new Property(propertyType, JsonConvert.SerializeObject(new[] { urlPickerPropertyData }));
//            return property;
//        }
//    }
//}
