using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PDT.BarcoCrp.EPI
{
	public class BarcoCrpConfigObject
	{
		[JsonProperty("HostId")]
		public string HostId { get; set; }

		[JsonProperty("DisplayID")]
		public string DisplayID { get; set; }

		[JsonProperty("DefaultPerspective")]
		public string DefaultPerspective { get; set; }

		[JsonProperty("NumberOfTiles")]
		public int NumberOfTiles { get; set; }
		
	}
}