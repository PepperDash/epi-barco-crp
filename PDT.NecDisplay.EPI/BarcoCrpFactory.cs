using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Essentials.Core.Config;
using System.Collections;
using PepperDash.Essentials.Bridges;
using PepperDash.Essentials.Core;

namespace PDT.BarcoCrp.EPI
{
	public class PdtBarcoCrpEpi
	{
		public static void LoadPlugin()
		{
			PepperDash.Essentials.Core.DeviceFactory.AddFactoryForType("BarcoCrp", PdtBarcoCrpEpi.BuildDevice);
		}

		public static string MinimumEssentialsFrameworkVersion = "1.10.0";

		public static PdtBarcoCrp BuildDevice(DeviceConfig dc)
		{
			var config = JsonConvert.DeserializeObject<BarcoCrpConfigObject>(dc.Properties.ToString());
			var comm = CommFactory.CreateCommForDevice(dc);
            try
            {
               // if there is no id in the config file an exception is thrown
               var newMe = new PdtBarcoCrp(dc.Key, dc.Name, comm, config);
               return newMe;
            }
            catch
            {
                // if there is no id in the config file an exception is thrown.  id will default to (0x2a) the all displays command
                var newMe = new PdtBarcoCrp(dc.Key, dc.Name, comm, config);
                return newMe;
            }
		}
	}
}