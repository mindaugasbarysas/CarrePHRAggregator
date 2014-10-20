using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VULSK.CarrePHRAggregator.PHRInputHealthVault;
using VULSK.CarrePHRAggregator.DataSpecification;
using VULSK.CarrePHRAggregator.RUM;
using VULSK.CarrePHRAggregator.ResourceOutput;
using System.Collections.Generic;

namespace TestCases
{
	[TestClass]
	public class TestCases
	{
		[TestMethod]
		public void TestCreate()
		{
			PatientIdentifier p = new PatientIdentifier() { InternalId = new Guid("1b1a04ae-7921-4b6a-a188-7683b83a78b8") };
			PHRData phrd=new PHRPluginHealthVault().GetData(p);
			Assert.IsTrue(phrd.Data.Count>0);
		}

		[TestMethod]
		public void TestRUM()
		{
			RUM rum=new RUM();
			List<PHRData> PD= rum.GetPatientData(new PatientIdentifier() { InternalId = new Guid("1b1a04ae-7921-4b6a-a188-7683b83a78b8") });

			ResourceOutput ro=new ResourceOutput();
			ro.Output(rum.ToXML(PD));
			
		}
	}
}
