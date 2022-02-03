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
		public static void LinkToApiExt(this PdtBarcoCrp displayDevice, BasicTriList trilist, uint joinStart, string joinMapKey)
		{

				BarcoCrpControllerJoinMap joinMap = new BarcoCrpControllerJoinMap();

				var JoinMapSerialized = JoinMapHelper.GetJoinMapForDevice(joinMapKey);

				if (!string.IsNullOrEmpty(JoinMapSerialized))
					joinMap = JsonConvert.DeserializeObject<BarcoCrpControllerJoinMap>(JoinMapSerialized);

				joinMap.OffsetJoinNumbers(joinStart);

				Debug.Console(1, "*** Linking to Trilist '{0}'",trilist.ID.ToString("X"));
				Debug.Console(0, "*** Linking to Display: {0}", displayDevice.Name);

                trilist.StringInput[joinMap.Name].StringValue = displayDevice.Name;	
				

				var commMonitor = displayDevice as ICommunicationMonitor;
                if (commMonitor != null)
                {
                    commMonitor.CommunicationMonitor.IsOnlineFeedback.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline]);
                }

				//displayDevice.InputNumberFeedback.LinkInputSig(trilist.UShortInput[joinMap.InputSelect]);
				
                // Two way feedbacks
                var twoWayDisplay = displayDevice as PepperDash.Essentials.Core.TwoWayDisplayBase;
                if (twoWayDisplay != null)
                {
                    trilist.SetBool(joinMap.IsTwoWayDisplay, true);

                    twoWayDisplay.CurrentInputFeedback.OutputChange += new EventHandler<FeedbackEventArgs>(CurrentInputFeedback_OutputChange);
					

                   
                }

				// Power Off
				trilist.SetSigTrueAction(joinMap.PowerOff, () =>
					{
						displayDevice.PowerOff();
					});

				displayDevice.PowerIsOnFeedback.OutputChange += new EventHandler<FeedbackEventArgs>(PowerIsOnFeedback_OutputChange);
				displayDevice.PowerIsOnFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.PowerOff]);

				// PowerOn
				trilist.SetSigTrueAction(joinMap.PowerOn, () =>
					{
						displayDevice.PowerOn();
					});
                
                // Picture Mute
                trilist.SetSigTrueAction(joinMap.PictureMuteOn, () =>
                    {
                        displayDevice.PictureMuteOn();
                    });
               
                trilist.SetSigTrueAction(joinMap.PictureMuteOff, () =>
                {
                    displayDevice.PictureMuteOff();
                });
                
                trilist.SetSigTrueAction(joinMap.PictureMuteToggle, () =>
                {
                    displayDevice.PictureMuteToggle();
                });

				displayDevice.VideoIsMutedFeedback.LinkInputSig(trilist.BooleanInput[joinMap.PictureMuteOn]);
				displayDevice.VideoIsMutedFeedback.LinkComplementInputSig(trilist.BooleanInput[joinMap.PictureMuteOff]);



				
				displayDevice.PowerIsOnFeedback.LinkInputSig(trilist.BooleanInput[joinMap.PowerOn]);

				int count = 1;
				var displayBase = displayDevice as PepperDash.Essentials.Core.DisplayBase;
				foreach (var input in displayDevice.InputPorts)
				{
					//displayDevice.InputKeys.Add(input.Key.ToString());
					//var tempKey = InputKeys.ElementAt(count - 1);
					trilist.SetSigTrueAction((ushort)(joinMap.InputSelectOffset + count), () => { displayDevice.ExecuteSwitch(displayDevice.InputPorts[input.Key.ToString()].Selector); });
					Debug.Console(2, displayDevice, "Setting Input Select Action on Digital Join {0} to Input: {1}", joinMap.InputSelectOffset + count, displayDevice.InputPorts[input.Key.ToString()].Key.ToString());
					trilist.StringInput[(ushort)(joinMap.InputNamesOffset + count)].StringValue = input.Key.ToString();
					count++;
				}
              displayDevice.CurrentInputFeedback.LinkInputSig(trilist.UShortInput[joinMap.InputSelect]);

                Debug.Console(2, displayDevice, "Setting Input Select Action on Analog Join {0}", joinMap.InputSelect);

              trilist.SetUShortSigAction(joinMap.InputSelect, (a) =>
				{
						if (a == 0)
						{
							displayDevice.PowerOff();
						}
						else if (a > 0 && a < displayDevice.InputPorts.Count )
						{
							displayDevice.ExecuteSwitch(displayDevice.InputPorts.ElementAt(a - 1).Selector);
						}
						else if (a == 102)
						{
							displayDevice.PowerToggle();

						}
						Debug.Console(2, displayDevice, "InputChange {0}", a);
							

				});


                var volumeDisplay = displayDevice as IBasicVolumeControls;
                if (volumeDisplay != null)
                {
                    trilist.SetBoolSigAction(joinMap.VolumeUp, (b) => volumeDisplay.VolumeUp(b));
                    trilist.SetBoolSigAction(joinMap.VolumeDown, (b) => volumeDisplay.VolumeDown(b));
                    trilist.SetSigTrueAction(joinMap.VolumeMute, () => volumeDisplay.MuteToggle());

                    var volumeDisplayWithFeedback = volumeDisplay as IBasicVolumeWithFeedback;
                    if(volumeDisplayWithFeedback != null)
                    {
                        volumeDisplayWithFeedback.VolumeLevelFeedback.LinkInputSig(trilist.UShortInput[joinMap.VolumeLevelFB]);
                        volumeDisplayWithFeedback.MuteFeedback.LinkInputSig(trilist.BooleanInput[joinMap.VolumeMute]);
                    }
                }
			}

		static void CurrentInputFeedback_OutputChange(object sender, FeedbackEventArgs e)
		{
               Debug.Console(0, "CurrentInputFeedback_OutputChange {0}", e.StringValue);

            if (e.StringValue.Substring(11, 5).Contains("\x31\x31"))
            {
                //HDMI 1
                //InputSelect(11);
                Debug.Console(0, "CurrentInputFeedback_OutputChange {0}", "HDMI 1");
                ((PdtBarcoCrp)sender).CurrentInput = 1;
            }
            if (e.StringValue.Substring(11, 5).Contains("\x31\x32"))
            {
                //HDMI 2
                //InputSelect(12);
                Debug.Console(0, "CurrentInputFeedback_OutputChange {0}", "HDMI 2");

                ((PdtBarcoCrp)sender).CurrentInput = 2;
            }
            if (e.StringValue.Substring(11, 5).Contains("\x38\x32"))
            {
                //HDMI 3
                //InputSelect(82);
                Debug.Console(0, "CurrentInputFeedback_OutputChange {0}", "HDMI 3");
                ((PdtBarcoCrp)sender).CurrentInput = 3;
            }
            if (e.StringValue.Substring(11, 5).Contains("\x38\x33"))
            {
                //HDMI 4
                //InputSelect(83);
                Debug.Console(0, "CurrentInputFeedback_OutputChange {0}", "HDMI 4");
                ((PdtBarcoCrp)sender).CurrentInput = 4;
            }
            if (e.StringValue.Substring(11, 5).Contains("\x30\x46"))
            {
                //DP 1
                //InputSelect(15);
                Debug.Console(0, "CurrentInputFeedback_OutputChange {0}", "DP 1");
                ((PdtBarcoCrp)sender).CurrentInput = 5;
            }
            if (e.StringValue.Substring(11, 5).Contains("\x31\x30"))
            {
                //DP 2
                //InputSelect(16);
                Debug.Console(0, "CurrentInputFeedback_OutputChange {0}", "DP 2");
                ((PdtBarcoCrp)sender).CurrentInput = 6;
            }
            if (e.StringValue.Substring(13, 5).Contains("\x30\x30\x31\x03"))
            {
                //vga

                Debug.Console(0, "CurrentInputFeedback_OutputChange {0}", "VGA");
                ((PdtBarcoCrp)sender).CurrentInput = 7;
            }
            if (e.StringValue.Substring(11, 5).Contains("\x30\x33"))
            {
                //vga

                Debug.Console(0, "CurrentInputFeedback_OutputChange {0}", "DVI 1");
                ((PdtBarcoCrp)sender).CurrentInput = 8;
            }
		}

		static void PowerIsOnFeedback_OutputChange(object sender, FeedbackEventArgs e)
		{

			// Debug.Console(0, "PowerIsOnFeedback_OutputChange {0}",  e.BoolValue);
			if (!e.BoolValue)
			{


			}
			else
			{

			}
		}




	}
    public class BarcoCrpControllerJoinMap : JoinMapBase
	{
        // Digital
        public uint PowerOff { get; set; }
        public uint PowerOn { get; set; }
        public uint IsTwoWayDisplay { get; set; }
        public uint VolumeUp { get; set; }
        public uint VolumeDown { get; set; }
        public uint VolumeMute { get; set; }
        public uint InputSelectOffset { get; set; }
         public uint ButtonVisibilityOffset { get; set; }
        public uint IsOnline { get; set; }
        public uint PictureMuteOff { get; set; }
        public uint PictureMuteOn { get; set; }
        public uint PictureMuteToggle { get; set; }
        public uint MatrixModeOff { get; set; }
        public uint MatrixModeOn { get; set; }

        // Analog
        public uint InputSelect { get; set; }
        public uint VolumeLevelFB { get; set; }

        // Serial
        public uint Name { get; set; }
        public uint InputNamesOffset { get; set; }


		public BarcoCrpControllerJoinMap()
		{
			// Digital
            IsOnline = 50;
            PowerOff = 1;
            PowerOn = 2;
            IsTwoWayDisplay = 3;
            VolumeUp = 5;
            VolumeDown = 6;
            VolumeMute = 7;

            PictureMuteOff = 21;
            PictureMuteOn = 22;
            PictureMuteToggle = 23;

            MatrixModeOff = 41;
            MatrixModeOn = 42;

            ButtonVisibilityOffset = 40;
            InputSelectOffset = 10;

            // Analog
            InputSelect = 11;
            VolumeLevelFB = 5;

            // Serial
            Name = 1;
            InputNamesOffset = 10;
       }

		public override void OffsetJoinNumbers(uint joinStart)
		{
			var joinOffset = joinStart - 1;

			IsOnline = IsOnline + joinOffset;
			PowerOff = PowerOff + joinOffset;
			PowerOn = PowerOn + joinOffset;
            PictureMuteOff = PictureMuteOff + joinOffset;
            PictureMuteOn = PictureMuteOn + joinOffset;
            PictureMuteToggle = PictureMuteToggle + joinOffset;
            IsTwoWayDisplay = IsTwoWayDisplay + joinOffset;
			ButtonVisibilityOffset = ButtonVisibilityOffset + joinOffset;
			Name = Name + joinOffset;
			InputNamesOffset = InputNamesOffset + joinOffset;
			InputSelectOffset = InputSelectOffset + joinOffset;
            MatrixModeOff = MatrixModeOff + joinOffset;
            MatrixModeOn = MatrixModeOn + joinOffset;

            InputSelect = InputSelect + joinOffset;

            VolumeUp = VolumeUp + joinOffset;
            VolumeDown = VolumeDown + joinOffset;
            VolumeMute = VolumeMute + joinOffset;
            VolumeLevelFB = VolumeLevelFB + joinOffset;
		}
	}
}
