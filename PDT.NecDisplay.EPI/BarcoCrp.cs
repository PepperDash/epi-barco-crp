using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Routing;
using Feedback = PepperDash.Essentials.Core.Feedback;
using Newtonsoft.Json.Linq;

namespace PDT.BarcoCrp.EPI
{
	/// <summary>
	/// 
	/// </summary>
	public class PdtBarcoCrp : TwoWayDisplayBase, ICommunicationMonitor, IBridgeAdvanced
	{
		public IBasicCommunication Communication { get; private set; }
		public CommunicationGather PortGather { get; private set; }
		public StatusMonitorBase CommunicationMonitor { get; private set; }
		private int PollState = 0; 
		#region Command constants

        public const string HeaderCmd = "\x01\x30";
        public const string InputGetCmd = "\x30\x43\x30\x36\x02\x30\x30\x36\x30\x03"; 
        public const string Hdmi1Cmd = "\x30\x45\x30\x41\x02\x31\x31\x30\x36\x30\x30\x31\x31\x03"; 
        public const string Hdmi2Cmd = "\x30\x45\x30\x41\x02\x31\x31\x30\x36\x30\x30\x31\x32\x03"; 
        public const string Hdmi3Cmd = "\x30\x45\x30\x41\x02\x31\x31\x30\x36\x30\x30\x38\x32x03"; 
        public const string Hdmi4Cmd = "\x30\x45\x30\x41\x02\x31\x31\x30\x36\x30\x30\x38\x33\x03"; 
        public const string Dp1Cmd = "\x30\x45\x30\x41\x02\x30\x30\x36\x30\x30\x30\x30\x46\x03"; 
        public const string Dp2Cmd = "\x30\x45\x30\x41\x02\x30\x30\x36\x30\x30\x30\x31\x30\x03"; 
        public const string Dvi1Cmd = "\x30\x45\x30\x41\x02\x30\x30\x36\x30\x30\x30\x30\x33\x03"; 
        public const string Video1Cmd = "\x30\x45\x30\x41\x02\x30\x30\x36\x30\x30\x30\x30\x35\x03"; 
        public const string VgaCmd = "\x30\x45\x30\x41\x02\x30\x30\x36\x30\x30\x30\x30\x31\x03"; 
        public const string RgbCmd = "\x30\x45\x30\x41\x02\x30\x30\x36\x30\x30\x30\x30\x32\x03"; 

        public const string PowerOnCmd = "\x30\x41\x30\x43\x02\x43\x32\x30\x33\x44\x36\x30\x30\x30\x31\x03"; 
        public const string PowerOffCmd = "\x30\x41\x30\x43\x02\x43\x32\x30\x33\x44\x36\x30\x30\x30\x34\x03"; 
        public const string PowerToggleIrCmd = "\x30\x41\x30\x43\x02\x43\x32\x31\x30\x30\x30\x30\x33\x30\x33\x03"; 
        public const string PowerPoll = "\x30\x41\x30\x36\x02\x30\x31\x64\x36\x03"; 

        public const string MuteOffCmd = "\x30\x45\x30\x41\x02\x30\x30\x38\x44\x30\x30\x30\x30\x03"; 
        public const string MuteOnCmd = "\x30\x45\x30\x41\x02\x30\x30\x38\x44\x30\x30\x30\x31\x03"; 
        public const string MuteToggleIrCmd = "\x30\x41\x30\x43\x02\x43\x32\x31\x30\x30\x30\x31\x42\x30\x33\x03"; 
        public const string MuteGetCmd = "\x30\x43\x30\x36\x02\x30\x30\x38\x44\x03";

        public const string PictureMuteOnCmd = "\x30\x45\x30\x41\x02\x31\x30\x42\x36\x30\x30\x30\x31\x03";
        public const string PictureMuteOffCmd = "\x30\x45\x30\x41\x02\x31\x30\x42\x36\x30\x30\x30\x32\x03";

        public const string MatrixModeOnCmd = "\x30\x45\x30\x41\x02\x30\x32\x44\x33\x30\x30\x30\x32\x03";
        public const string MatrixModeOffCmd = "\x30\x45\x30\x41\x02\x30\x32\x44\x33\x30\x30\x30\x31\x03";

        public const string VolumeGetCmd = "\x30\x43\x30\x36\x02\x30\x30\x36\x32\x03"; 
        public const string VolumeLevelPartialCmd = "\x30\x45\x30\x41\x02\x30\x30\x36\x32"; 
        public const string VolumeUpCmd = "\x30\x45\x30\x41\x02\x31\x30\x41\x44\x30\x30\x30\x31\x03"; 
        public const string VolumeDownCmd = "\x30\x45\x30\x41\x02\x31\x30\x41\x44\x30\x30\x30\x32\x03"; 

        public const string MenuIrCmd = "\x41\x30\x41\x30\x43\x02\x43\x32\x31\x30\x30\x30\x32\x30\x30\x33\x03\x03\x0D";
        public const string UpIrCmd = "\x41\x30\x41\x30\x43\x02\x43\x32\x31\x30\x30\x30\x31\x35\x30\x33\x03\x05\x0D";
        public const string DownIrCmd = "\x41\x30\x41\x30\x43\x02\x43\x32\x31\x30\x30\x30\x31\x34\x30\x33\x03\x04\x0D";
        public const string LeftIrCmd = "\x41\x30\x41\x30\x43\x02\x43\x32\x31\x30\x30\x30\x32\x31\x30\x33\x03\x02\x0D";
        public const string RightIrCmd = "\x41\x30\x41\x30\x43\x02\x43\x32\x31\x30\x30\x30\x32\x32\x30\x33\x03\x01\x0D";
        public const string SelectIrCmd = "\x41\x30\x41\x30\x43\x02\x43\x32\x31\x30\x30\x30\x32\x33\x30\x33\x03\x00\x0D";
        public const string ExitIrCmd = "\x41\x30\x41\x30\x43\x02\x43\x32\x31\x30\x30\x30\x31\x46\x30\x33\x03\x76\x0D";
		#endregion

		bool _PowerIsOn;
		bool _IsWarmingUp;
		bool _IsCoolingDown;
		ushort _VolumeLevel;
		ushort _CurrentInput;
		bool _IsMuted;
       byte _ID;

		bool _VideoIsMuted;
		public bool VideoIsMuted
		{
			get
			{
				return _VideoIsMuted;
			}
			set
			{
				_VideoIsMuted = value;
				VideoIsMutedFeedback.FireUpdate();
			}
		}
		public BoolFeedback VideoIsMutedFeedback;
       public ushort CurrentInput {
           get
           {
               return _CurrentInput; 

           } 
           set
           {
               _CurrentInput = value; 
                CurrentInputFeedback.FireUpdate(); 
           }
       }
       public IntFeedback CurrentInputFeedback;

		protected override Func<bool> PowerIsOnFeedbackFunc { get { return () => _PowerIsOn; } }
		protected override Func<bool> IsCoolingDownFeedbackFunc { get { return () => _IsCoolingDown; } }
		protected override Func<bool> IsWarmingUpFeedbackFunc { get { return () => _IsWarmingUp; } }
		protected override Func<string> CurrentInputFeedbackFunc { get { return () => "Not Implemented"; } }
	

		/// <summary>
		/// Constructor for IBasicCommunication with id passed from the device properties in the config file
		/// </summary>
        public PdtBarcoCrp(string key, string name, IBasicCommunication comm, string id)
			: base(key, name)
		{
           _ID = id == null ? (byte)0x2A : Convert.ToByte(id); 
           Communication = comm;
			Init();
		}

        /// <summary>
        /// Constructor for IBasicCommunication when no id is in the properties of the config file
        /// </summary>
        public PdtBarcoCrp(string key, string name, IBasicCommunication comm)
            : base(key, name)
        {
            _ID = (byte)0x2A; 
            Communication = comm;
            Init();
        }

		/// <summary>
		/// Constructor for TCP
		/// </summary>
        public PdtBarcoCrp(string key, string name, string hostname, int port, string id)
			: base(key, name)
		{
            _ID = id == null ? (byte)0x2A : Convert.ToByte(id);
			Communication = new GenericTcpIpClient(key + "-tcp", hostname, port, 5000);
			Init();
		}

		/// <summary>
		/// Constructor for COM
		/// </summary>
        public PdtBarcoCrp(string key, string name, ComPort port, ComPort.ComPortSpec spec, string id)
			: base(key, name)
		{
            _ID = id == null ? (byte)0x2A : Convert.ToByte(id); // If id is null, set default value of 0x2A (all displays command in NEC), otherwise assign value passed in constructor
			Communication = new ComPortController(key + "-com", port, spec);
			Init();
		}

		void Init()
		{
			PortGather = new CommunicationGather(Communication, '>');
			PortGather.LineReceived += this.Port_LineReceived;
			CommunicationMonitor = new GenericCommunicationMonitor(this, Communication, 30000, 120000, 300000, Poll);                
		}

		~PdtBarcoCrp()
		{
			PortGather = null;
		}

		public override bool CustomActivate()
		{
			Communication.Connect();
			CommunicationMonitor.StatusChange += (o, a) => { Debug.Console(2, this, "Communication monitor state: {0}", CommunicationMonitor.Status); };
			CommunicationMonitor.Start();
			return true;
		}

		public void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
		{
			PdtBarcoCrpBridge.LinkToApiExt(this, trilist, joinStart, joinMapKey);
		}

		public override FeedbackCollection<Feedback> Feedbacks
		{
			get
			{
				var list = base.Feedbacks;
				list.AddRange(new List<Feedback>
				{

				});
				return list;
			}
		}
		public void Poll()
		{
			switch (PollState)
			{
				case 0: 
					
					break;
				
				default:
					PollState = 0;
					return;
			}
			//PollState++; 
			
		}

		void Port_LineReceived(object dev, GenericCommMethodReceiveTextArgs args)
		{
			if (Debug.Level == 2)
				Debug.Console(2, this, "Received: '{0}'", ComTextHelper.GetEscapedText(args.Text));

			if (args.Text == "DO SOMETHING HERE EVENTUALLY")
			{

			}
		}

		void Send(string s)
		{
			Debug.Console(2, this, "Send: '{0}'", ComTextHelper.GetEscapedText(s));
			Communication.SendText(s);
		}


		public override void PowerOn()
		{
			if (!PowerIsOnFeedback.BoolValue && !_IsWarmingUp && !_IsCoolingDown)
			{
				_IsWarmingUp = true;
				IsWarmingUpFeedback.FireUpdate();
				// Fake power-up cycle
				WarmupTimer = new CTimer(o =>
				{
					_IsWarmingUp = false;
					_PowerIsOn = true;
					IsWarmingUpFeedback.FireUpdate();
					PowerIsOnFeedback.FireUpdate();
				}, WarmupTime);
			}
		}

		public override void PowerOff()
		{
			// If a display has unreliable-power off feedback, just override this and
			// remove this check.
				_IsCoolingDown = true;
				_PowerIsOn = false;
				PowerIsOnFeedback.FireUpdate();
				IsCoolingDownFeedback.FireUpdate();
				// Fake cool-down cycle
				CooldownTimer = new CTimer(o =>
				{
					Debug.Console(2, this, "Cooldown timer ending");
					_IsCoolingDown = false;
					IsCoolingDownFeedback.FireUpdate();
				}, CooldownTime);
		}

		public override void PowerToggle()
		{
			if (PowerIsOnFeedback.BoolValue && !IsWarmingUpFeedback.BoolValue)
				PowerOff();
			else if (!PowerIsOnFeedback.BoolValue && !IsCoolingDownFeedback.BoolValue)
				PowerOn();
		}

        public void PictureMuteOn()
        {
			VideoIsMuted = true;
        }

        public void PictureMuteOff()
        {
         	VideoIsMuted = false;
        }

        public void PictureMuteToggle()
        {
			Debug.Console(2, this, "PictureMuteToggle: '{0}'", VideoIsMuted);
			if (!VideoIsMuted)
			{
				PictureMuteOn();
			}
			else
			{
				PictureMuteOff();
			}

        }



		public override void ExecuteSwitch(object selector)
		{
			if (selector is Action)
				(selector as Action).Invoke();
			else
				Debug.Console(1, this, "WARNING: ExecuteSwitch cannot handle type {0}", selector.GetType());
			//Send((string)selector);
		}

	}

	public class NecPSXMDisplayFactory : EssentialsDeviceFactory<PdtBarcoCrp>
	{
		public NecPSXMDisplayFactory()
		{
			TypeNames = new List<string>() { "necmpsx" };
		}

		public override EssentialsDevice BuildDevice(DeviceConfig dc)
		{
			Debug.Console(1, "Factory Attempting to create new Generic Comm Device");
			var comm = CommFactory.CreateCommForDevice(dc);
			if (comm != null)
                try
                {
                    var newMe = new PdtBarcoCrp(dc.Key, dc.Name, comm, dc.Properties["id"].Value<string>());
                    return newMe;
                }
                catch
                {
                    var newMe = new PdtBarcoCrp(dc.Key, dc.Name, comm);
                    return newMe;
                }
			else
				return null;
		}
	}

}