using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Bridges;
using Newtonsoft.Json;

namespace PDT.BarcoCrp.EPI
{
	 public static class PdtBarcoCrpBridge
	{
		public static void LinkToApiExt(this PdtBarcoCrp device, BasicTriList trilist, uint joinStart, string joinMapKey)
		{



				//displayDevice.InputNumberFeedback.LinkInputSig(trilist.UShortInput[joinMap.InputSelect]);


                // Debug.Console(2, device, "Setting Input Select Action on Analog Join {0}", joinMap.InputSelect);
			}





	}

}
