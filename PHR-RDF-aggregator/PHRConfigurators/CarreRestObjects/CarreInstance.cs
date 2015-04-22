using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Vulsk.CarrePhrAggregator.PhrConfigurators.CarreRestObjects
{
    class CarreInstance
    {
        public string predicate { get; set; }
        [JsonProperty("Object")]
        public string obj { get; set; }
        public string subject { get; set; }
    }
}
