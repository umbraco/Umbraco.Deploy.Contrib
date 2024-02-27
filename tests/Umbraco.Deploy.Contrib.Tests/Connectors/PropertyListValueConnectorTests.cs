using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
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
    public class PropertyListValueConnectorTests
    {
        [Test]
        public void GetValueTest()
        {
            var dataTypeService = Mock.Of<IDataTypeService>();

            var propListDataType = new DataTypeDefinition("propListEditorAlias")
            {
                Id = 1,
                Key = Guid.Parse("74AFF355-537A-4443-9801-C131FE83FF1F"),
                DatabaseType = DataTypeDatabaseType.Ntext
            };

            var innerDataType = new DataTypeDefinition("innerEditorAlias")
            {
                Id = 2,
                Key = Guid.Parse("D21BA417-98AC-4D05-8EF9-0ED3D75A8C0D"),
                DatabaseType = DataTypeDatabaseType.Integer // for true/false
            };

            var dataTypes = new[] { propListDataType, innerDataType };

            Mock.Get(dataTypeService)
                .Setup(x => x.GetDataTypeDefinitionById(It.IsAny<Guid>()))
                .Returns<Guid>(id => dataTypes.FirstOrDefault(x => x.Key == id));

            var preValues = new Dictionary<int, PreValueCollection>
            {
                { 1, new PreValueCollection(new Dictionary<string, PreValue>
                    {
                        { "dataType", new PreValue(innerDataType.Key.ToString()) }
                    })
                }
            };

            Mock.Get(dataTypeService)
                .Setup(x => x.GetPreValuesCollectionByDataTypeId(It.IsAny<int>()))
                .Returns<int>(id => preValues.TryGetValue(id, out var collection) ? collection : null);

            ValueConnectorCollection connectors = null;
            var defaultConnector = new DefaultValueConnector();
            var propListConnector = new PropertyListValueConnector(dataTypeService, new Lazy<ValueConnectorCollection>(() => connectors));
            connectors = new ValueConnectorCollection(new Dictionary<string, IValueConnector>
            {
                { "innerEditorAlias", defaultConnector },
                { "propListEditorAlias", propListConnector }
            });

            var input = $"{{\"dtd\":\"{innerDataType.Key}\",\"values\":[0]}}";

            var propertyType = new PropertyType(propListDataType);
            var property = new Property(propertyType, input);
            var dependencies = new List<ArtifactDependency>();
            var output = propListConnector.GetValue(property, dependencies);

            Console.WriteLine(output);

            var expected = $"{{\"dtd\":\"{innerDataType.Key}\",\"values\":[\"i0\"]}}";
            Assert.AreEqual(expected, output);
        }

        [Test]
        public void SetValueTest()
        {
            var dataTypeService = Mock.Of<IDataTypeService>();

            var propListDataType = new DataTypeDefinition("propListEditorAlias")
            {
                Id = 1,
                Key = Guid.Parse("74AFF355-537A-4443-9801-C131FE83FF1F"),
                DatabaseType = DataTypeDatabaseType.Ntext
            };

            var innerDataType = new DataTypeDefinition("innerEditorAlias")
            {
                Id = 2,
                Key = Guid.Parse("D21BA417-98AC-4D05-8EF9-0ED3D75A8C0D"),
                DatabaseType = DataTypeDatabaseType.Integer // for true/false
            };

            var dataTypes = new[] { propListDataType, innerDataType };

            Mock.Get(dataTypeService)
                .Setup(x => x.GetDataTypeDefinitionById(It.IsAny<Guid>()))
                .Returns<Guid>(id => dataTypes.FirstOrDefault(x => x.Key == id));

            var preValues = new Dictionary<int, PreValueCollection>
            {
                { 1, new PreValueCollection(new Dictionary<string, PreValue>
                    {
                        { "dataType", new PreValue(innerDataType.Key.ToString()) }
                    })
                }
            };

            Mock.Get(dataTypeService)
                .Setup(x => x.GetPreValuesCollectionByDataTypeId(It.IsAny<int>()))
                .Returns<int>(id => preValues.TryGetValue(id, out var collection) ? collection : null);

            ValueConnectorCollection connectors = null;
            var defaultConnector = new DefaultValueConnector();
            var propListConnector = new PropertyListValueConnector(dataTypeService, new Lazy<ValueConnectorCollection>(() => connectors));
            connectors = new ValueConnectorCollection(new Dictionary<string, IValueConnector>
            {
                { "innerEditorAlias", defaultConnector },
                { "propListEditorAlias", propListConnector }
            });

            var input = $"{{\"dtd\":\"{innerDataType.Key}\",\"values\":[\"i0\"]}}";

            UmbracoConfig.For.SetUmbracoSettings(GenerateMockSettings());

            var propListPropertyType = new PropertyType(propListDataType, "propListProperty");
            var propListProperty = new Property(propListPropertyType, null); // value is going to be replaced
            var propListContent = new Content("mockContent", -1, new ContentType(-1), new PropertyCollection(new List<Property> { propListProperty }));
            propListConnector.SetValue(propListContent, "propListProperty", input);

            var output = propListContent.GetValue("propListProperty");

            Assert.IsInstanceOf<string>(output);

            Console.WriteLine(output);

            var expected = $"{{\"dtd\":\"{innerDataType.Key}\",\"values\":[0]}}";
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
