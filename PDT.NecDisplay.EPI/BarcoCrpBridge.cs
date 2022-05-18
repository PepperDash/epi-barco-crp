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

				BarcoCrpJoinMap joinMap = new BarcoCrpJoinMap(joinStart);

				var JoinMapSerialized = JoinMapHelper.GetJoinMapForDevice(joinMapKey);

				if (!string.IsNullOrEmpty(JoinMapSerialized))
					joinMap = JsonConvert.DeserializeObject<BarcoCrpJoinMap>(JoinMapSerialized);

				//joinMap.OffsetJoinNumbers(joinStart);

				Debug.Console(1, "*** Linking to Trilist '{0}'",trilist.ID.ToString("X"));
				Debug.Console(0, "*** Linking to Display: {0}", device.Name);

               // trilist.StringInput[joinMap.Joins[].StringValue = device.Name;	
				trilist.SetSigTrueAction(joinMap.Poll.JoinNumber, () => device.GetCurrentRoutes());

				trilist.SetStringSigAction(joinMap.LoadPerspective.JoinNumber, (s) => device.LoadPerspective(s));
				
				var outputNum = 1;
				foreach (var item in device.CurrentRoutesFeedbacks)
				{
					var tempOutputNum = outputNum;
					item.Value.Feedback.LinkInputSig(trilist.StringInput[(ushort)(joinMap.LoadSource.JoinNumber + tempOutputNum)]);
					outputNum++;
					Debug.Console(0, "*** Linking to Feedback: {0}", tempOutputNum);
				}

				for (var x = 1; x < 10; x++)
				{
					var tempx = x;
					trilist.SetStringSigAction((uint)(joinMap.LoadSource.JoinNumber + tempx), (s) => device.LoadSource(s, tempx));
				}
				
				var commMonitor = device as ICommunicationMonitor;
                if (commMonitor != null)
                {
                    commMonitor.CommunicationMonitor.IsOnlineFeedback.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline.JoinNumber]);
                }

				//displayDevice.InputNumberFeedback.LinkInputSig(trilist.UShortInput[joinMap.InputSelect]);


                // Debug.Console(2, device, "Setting Input Select Action on Analog Join {0}", joinMap.InputSelect);
			}




	}

}
