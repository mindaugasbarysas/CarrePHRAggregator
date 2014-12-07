using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestCases
{
	using Vulsk.CarrePhrAggregator.PhrPlugins;
	using Vulsk.CarrePhrAggregator.DataSpecification;
	using Vulsk.CarrePhrAggregator.Rum;
	using Vulsk.CarrePhrAggregator.ResourceOutput;

	[TestClass]
	public class TestCases
	{
		private const string PGuid = "5c30ee9a-2e63-42c7-b418-ef4fe2f3e565";

		[TestMethod]
		public void TestCreate()
		{
			var p = new PatientIdentifier { InternalId = new Guid(PGuid) };
			var phrd = new PhrPluginHealthVault().GetData(p);
			Assert.IsTrue(phrd.Data.Count > 0);
		}

		[TestMethod]
		public void TestRum()
		{
			var rum = new Rum();
			var pd = rum.GetPatientData(new PatientIdentifier { InternalId = new Guid(PGuid) });

			var output = ResourceOutput.Output(Rum.ToXml(pd));
			output.Save("RumOutputTest.xml");
		}
	}
}
