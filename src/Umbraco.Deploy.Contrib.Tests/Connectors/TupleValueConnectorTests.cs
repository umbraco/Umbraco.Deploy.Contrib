using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Deploy;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Deploy.Contrib.Connectors.ValueConnectors;
using Umbraco.Deploy.ValueConnectors;

namespace Umbraco.Deploy.Contrib.Tests.Connectors
{
    [TestFixture]
    public class TupleValueConnectorTests
    {
        [Test]
        public void GetValueTest()
        {
            var dataTypeService = Mock.Of<IDataTypeService>();

            var tupleDataType = new DataTypeDefinition("tupleEditorAlias")
            {
                Id = 1,
                Key = Guid.Parse("3BBFB03B-B6AD-4310-986B-44E9F31A4F1F"),
                DatabaseType = DataTypeDatabaseType.Ntext
            };

            var innerDataType = new DataTypeDefinition("innerEditorAlias")
            {
                Id = 2,
                Key = Guid.Parse("0F6DCC71-2FA7-496B-A858-8D6DDAF37F59"),
                DatabaseType = DataTypeDatabaseType.Integer // for true/false
            };

            var dataTypes = new[] { tupleDataType, innerDataType };

            Mock.Get(dataTypeService)
                .Setup(x => x.GetDataTypeDefinitionById(It.IsAny<Guid>()))
                .Returns<Guid>(id => dataTypes.FirstOrDefault(x => x.Key == id));

            var preValues = new Dictionary<int, PreValueCollection>
            {
                { 1, new PreValueCollection(new Dictionary<string, PreValue> { { "dataTypes", new PreValue( $"[{{\"key\":\"{Guid.Empty}\",\"dtd\":\"{innerDataType.Key}\"}}]") } }) }
            };

            Mock.Get(dataTypeService)
                .Setup(x => x.GetPreValuesCollectionByDataTypeId(It.IsAny<int>()))
                .Returns<int>(id => preValues.TryGetValue(id, out var collection) ? collection : null);

            ValueConnectorCollection connectors = null;
            var defaultConnector = new DefaultValueConnector();
            var tupleConnector = new TupleValueConnector(dataTypeService, new Lazy<ValueConnectorCollection>(() => connectors));
            connectors = new ValueConnectorCollection(new Dictionary<string, IValueConnector>
            {
                { "innerEditorAlias", defaultConnector },
                { "tupleEditorAlias", tupleConnector }
            });

            var input = $"[{{\"key\":\"{Guid.Empty}\",\"dtd\":\"{innerDataType.Key}\",\"value\":0}}]";

            var propertyType = new PropertyType(tupleDataType);
            var property = new Property(propertyType, input);
            var dependencies = new List<ArtifactDependency>();
            var output = tupleConnector.GetValue(property, dependencies);

            Console.WriteLine(output);

            var expected = $"[{{\"key\":\"{Guid.Empty}\",\"dtd\":\"{innerDataType.Key}\",\"value\":\"i0\"}}]";
            Assert.AreEqual(expected, output);
        }

        [Test]
        public void SetValueTest()
        {
            var dataTypeService = Mock.Of<IDataTypeService>();

            var tupleDataType = new DataTypeDefinition("tupleEditorAlias")
            {
                Id = 1,
                Key = Guid.Parse("3BBFB03B-B6AD-4310-986B-44E9F31A4F1F"),
                DatabaseType = DataTypeDatabaseType.Ntext
            };

            var innerDataType = new DataTypeDefinition("innerEditorAlias")
            {
                Id = 2,
                Key = Guid.Parse("0F6DCC71-2FA7-496B-A858-8D6DDAF37F59"),
                DatabaseType = DataTypeDatabaseType.Integer // for true/false
            };

            var dataTypes = new[] { tupleDataType, innerDataType };

            Mock.Get(dataTypeService)
                .Setup(x => x.GetDataTypeDefinitionById(It.IsAny<Guid>()))
                .Returns<Guid>(id => dataTypes.FirstOrDefault(x => x.Key == id));

            var preValues = new Dictionary<int, PreValueCollection>
            {
                { 1, new PreValueCollection(new Dictionary<string, PreValue> { { "dataTypes", new PreValue( $"[{{\"key\":\"{Guid.Empty}\",\"dtd\":\"{innerDataType.Key}\"}}]") } }) }
            };

            Mock.Get(dataTypeService)
                .Setup(x => x.GetPreValuesCollectionByDataTypeId(It.IsAny<int>()))
                .Returns<int>(id => preValues.TryGetValue(id, out var collection) ? collection : null);

            ValueConnectorCollection connectors = null;
            var defaultConnector = new DefaultValueConnector();
            var tupleConnector = new TupleValueConnector(dataTypeService, new Lazy<ValueConnectorCollection>(() => connectors));
            connectors = new ValueConnectorCollection(new Dictionary<string, IValueConnector>
            {
                { "innerEditorAlias", defaultConnector },
                { "tupleEditorAlias", tupleConnector }
            });

            var input = $"[{{\"key\":\"{Guid.Empty}\",\"dtd\":\"{innerDataType.Key}\",\"value\":\"i0\"}}]";

            UmbracoConfig.For.SetUmbracoSettings(GenerateMockSettings());

            var tuplePropertyType = new PropertyType(tupleDataType, "tupleProperty");
            var tupleProperty = new Property(tuplePropertyType, null); // value is going to be replaced
            var tupleContent = new Content("mockContent", -1, new ContentType(-1), new PropertyCollection(new List<Property> { tupleProperty }));
            tupleConnector.SetValue(tupleContent, "tupleProperty", input);

            var output = tupleContent.GetValue("tupleProperty");

            Assert.IsInstanceOf<string>(output);

            Console.WriteLine(output);

            var expected = $"[{{\"key\":\"{Guid.Empty}\",\"dtd\":\"{innerDataType.Key}\",\"value\":0}}]";
            Assert.AreEqual(expected, output);
        }

        public static IUmbracoSettingsSection GenerateMockSettings()
        {
            var settings = new Mock<IUmbracoSettingsSection>();

            var content = new Mock<IContentSection>();
            var security = new Mock<ISecuritySection>();
            var requestHandler = new Mock<IRequestHandlerSection>();
            var templates = new Mock<ITemplatesSection>();
            var dev = new Mock<IDeveloperSection>();
            var logging = new Mock<ILoggingSection>();
            var tasks = new Mock<IScheduledTasksSection>();
            var distCall = new Mock<IDistributedCallSection>();
            var repos = new Mock<IRepositoriesSection>();
            var providers = new Mock<IProvidersSection>();
            var routing = new Mock<IWebRoutingSection>();

            settings.Setup(x => x.Content).Returns(content.Object);
            settings.Setup(x => x.Security).Returns(security.Object);
            settings.Setup(x => x.RequestHandler).Returns(requestHandler.Object);
            settings.Setup(x => x.Templates).Returns(templates.Object);
            settings.Setup(x => x.Developer).Returns(dev.Object);
            settings.Setup(x => x.Logging).Returns(logging.Object);
            settings.Setup(x => x.ScheduledTasks).Returns(tasks.Object);
            settings.Setup(x => x.DistributedCall).Returns(distCall.Object);
            settings.Setup(x => x.PackageRepositories).Returns(repos.Object);
            settings.Setup(x => x.Providers).Returns(providers.Object);
            settings.Setup(x => x.WebRouting).Returns(routing.Object);

            //Now configure some defaults - the defaults in the config section classes do NOT pertain to the mocked data!!
            settings.Setup(x => x.Content.ForceSafeAliases).Returns(true);
            //settings.Setup(x => x.Content.ImageAutoFillProperties).Returns(ContentImagingElement.GetDefaultImageAutoFillProperties());
            //settings.Setup(x => x.Content.ImageFileTypes).Returns(ContentImagingElement.GetDefaultImageFileTypes());
            settings.Setup(x => x.RequestHandler.AddTrailingSlash).Returns(true);
            settings.Setup(x => x.RequestHandler.UseDomainPrefixes).Returns(false);
            //settings.Setup(x => x.RequestHandler.CharCollection).Returns(RequestHandlerElement.GetDefaultCharReplacements());
            settings.Setup(x => x.Content.UmbracoLibraryCacheDuration).Returns(1800);
            settings.Setup(x => x.WebRouting.UrlProviderMode).Returns("AutoLegacy");
            settings.Setup(x => x.Templates.DefaultRenderingEngine).Returns(RenderingEngine.Mvc);
            settings.Setup(x => x.Providers.DefaultBackOfficeUserProvider).Returns("UsersMembershipProvider");

            return settings.Object;
        }
    }
}
