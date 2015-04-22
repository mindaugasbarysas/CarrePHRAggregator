using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vulsk.CarrePhrAggregator.PhrConfigurators;
using Vulsk.CarrePhrAggregator.DataSpecification;
using System.Linq;

namespace TestCases
{
    [TestClass]
    public class CarreConfiguratorTests
    {
        [TestMethod]
        [TestCategory("Configurators")]
        public void TestConfiguration()
        {
            CarreConfigurator c = new CarreConfigurator();
            Configuration config = c.GetConfiguration();

            Assert.IsTrue(c.Source.InternalId == typeof(CarreConfigurator).GUID);
            Assert.IsTrue(config.Sources.Count > 0, "No sources?");
            Assert.IsTrue(config.DesiredData.Count > 0, "No data is desired?");
            var objectCount = config.DesiredData.Select(dd => dd.OntologicName).Distinct().Count();
            Assert.IsTrue(objectCount > 1, "More than one distinct onthologic name is expected");
            Assert.IsTrue(
                config.DesiredData.Select(dd => dd.Name).Distinct().Count() == objectCount, 
                "Onthologic name and distinct name count differs"
            );
            Assert.IsTrue(
                config.DesiredData.Select(dd => dd.Identifier).Distinct().Count() == objectCount, 
                string.Format(
                    "Onthologic name and distinct identifier count differs {0} != {1}", 
                    config.DesiredData.Select(dd => dd.Identifier).Distinct().Count(), 
                    objectCount
                )
           );
        }
    }
}
