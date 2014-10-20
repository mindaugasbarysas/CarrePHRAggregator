using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VULSK.CarrePHRAggregator.DataSpecification;
using VULSK.CarrePHRAggregator.PHRInput;
using System.Reflection;

namespace VULSK.CarrePHRAggregator.PHRInputVivaport
{
	public class PHRPluginVivaport:IPHRInput
	{
		
		private SourceIdentifier _sourceID=new SourceIdentifier() {
				  InternalId=typeof(PHRPluginVivaport).GetType().GUID,
				  SourceName="Vivaport.eu"
		};
		public SourceIdentifier Source { get { return _sourceID; } }

		public PHRData GetData(PatientIdentifier p)
		{
			PHRData retData=new PHRData();

			return retData;
		}
	}
}
