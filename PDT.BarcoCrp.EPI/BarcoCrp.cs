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
using Newtonsoft.Json;

namespace PDT.BarcoCrp.EPI
{
	/// <summary>
	/// 
	/// </summary>
	public class PdtBarcoCrp : EssentialsBridgeableDevice, ICommunicationMonitor, IBridgeAdvanced
	{
		public IBasicCommunication Communication { get; private set; }
		public CommunicationGather PortGather { get; private set; }
		public StatusMonitorBase CommunicationMonitor { get; private set; }
		private int PollState = 0;
		private uint DebugLevel = 2;
		public StringWithFeedback CurrentPerspective;
		public Dictionary<int, StringWithFeedback> CurrentRoutesFeedbacks;
		public int NumberOfTiles = 10;
		string _ID;
		BarcoCrpConfigObject _Config;

		/// <summary>
		/// Constructor for IBasicCommunication with id passed from the device properties in the config file
		/// </summary>
	   public PdtBarcoCrp(string key, string name, IBasicCommunication comm, BarcoCrpConfigObject config)
			: base(key, name)
		{
		   _Config = config;
           Communication = comm;
			
			if (NumberOfTiles != 0)
			{
				NumberOfTiles = _Config.NumberOfTiles;
			}
			CurrentRoutesFeedbacks = new Dictionary<int, StringWithFeedback>();
			for (var x = 1; x <= NumberOfTiles; x++)
			{
				CurrentRoutesFeedbacks.Add(x, new StringWithFeedback()); 
			}
			foreach (var item in CurrentRoutesFeedbacks)
			{
				Debug.Console(2, this, "CurrentRoutesFeedbacks : {0}", item.Key);
			}
			CurrentPerspective = new StringWithFeedback();
			Init();
		}


		void Init()
		{
			PortGather = new CommunicationGather(Communication, '>');
			PortGather.LineReceived += this.Port_LineReceived;
			CommunicationMonitor = new GenericCommunicationMonitor(this, Communication, 30000, 120000, 300000, Poll);
			var socket = Communication as ISocketStatus;

			if (socket != null)
			{
				// This instance uses IP control
				socket.ConnectionChange += socket_ConnectionChange;

			}
		}

		~PdtBarcoCrp()
		{
			PortGather = null;
		}

		public override bool CustomActivate()
		{
			Communication.Connect();
			CommunicationMonitor.StatusChange += new EventHandler<MonitorStatusChangeEventArgs>(CommunicationMonitor_StatusChange);
			CommunicationMonitor.Start();
			return true;
		}

		void CommunicationMonitor_StatusChange(object sender, MonitorStatusChangeEventArgs e)
		{
			Debug.Console(2, this, "Communication monitor state: {0}", CommunicationMonitor.Status);
			
		}

		void socket_ConnectionChange(object sender, GenericSocketStatusChageEventArgs e)
		{
			Debug.Console(2, this, "Socket Status Change: {0}", e.Client.ClientStatus.ToString());

			if (e.Client.IsConnected)
			{
				GetCurrentPerspective();
				if (!string.IsNullOrEmpty(_Config.DefaultPerspective))
				{
					LoadPerspective(_Config.DefaultPerspective);
				}
				else
				{

					GetCurrentRoutes();
				}
			}

			if (!e.Client.IsConnected)
			{
			}
			else
			{

			}
		}
		public void SendCommand(string command)
		{

				string cmd = string.Format("<I:{0}||K:CMS||O:{1}>\n", _Config.HostId, command);
				Communication.SendText(cmd);
				Debug.Console(DebugLevel, this, "Sent: '{0}'", cmd);
			
		}
		public void SendCommand(string command, string[] args)
		{
			if(args.Length == 1)
			{
				string cmd = string.Format("<I:{0}||K:CMS||O:{1}||A1:{2}||>\n", _Config.HostId, command, args[0]);
				Communication.SendText(cmd);
				Debug.Console(DebugLevel, this, "Sent: '{0}'", cmd);
			}
			else if(args.Length == 2)
			{
				string cmd = string.Format("<I:{0}||K:CMS||O:{1}||A1:{2}||A2:{3}||>\n", _Config.HostId, command, args[0], args[1]);
				Communication.SendText(cmd);
				Debug.Console(DebugLevel, this, "Sent: '{0}'", cmd);
			}
			else if (args.Length == 3)
			{
				string cmd = string.Format("<I:{0}||K:CMS||O:{1}||A1:{2}||A2:{3}||A3:{4}||>\n", _Config.HostId, command, args[0], args[1], args[2]);
				Communication.SendText(cmd);
				Debug.Console(DebugLevel, this, "Sent: '{0}'", cmd);
			}
		}
		public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
		{

			BarcoCrpJoinMap joinMap = new BarcoCrpJoinMap(joinStart);

			var JoinMapSerialized = JoinMapHelper.GetJoinMapForDevice(joinMapKey);

			if (!string.IsNullOrEmpty(JoinMapSerialized))
				joinMap = JsonConvert.DeserializeObject<BarcoCrpJoinMap>(JoinMapSerialized);

			//joinMap.OffsetJoinNumbers(joinStart);

			trilist.OnlineStatusChange += (o,a) => {
				if (a.DeviceOnLine)
				{
					var tempJoinNum = joinMap.LoadSource.JoinNumber;
					foreach (var item in CurrentRoutesFeedbacks)
					{
						Debug.Console(0, "*** Linking to Barco: {0} {1}", item.Key, item.Value.Value);
						trilist.StringInput[(ushort)(item.Key + tempJoinNum)].StringValue = item.Value.Value;
					}
				}
			};

			Debug.Console(1, "*** Linking to Trilist '{0}'", trilist.ID.ToString("X"));
			Debug.Console(0, "*** Linking to Barco: {0}", this.Name);

			// trilist.StringInput[joinMap.Joins[].StringValue = device.Name;	
			trilist.SetSigTrueAction(joinMap.Poll.JoinNumber, () => GetCurrentRoutes());

			trilist.SetStringSigAction(joinMap.LoadPerspective.JoinNumber, (s) => LoadPerspective(s));
			CurrentPerspective.Feedback.LinkInputSig(trilist.StringInput[joinMap.LoadPerspective.JoinNumber]);

			var outputNum = 1;
			foreach (var item in CurrentRoutesFeedbacks)
			{
				var tempOutputNum = outputNum;
				item.Value.Feedback.LinkInputSig(trilist.StringInput[(ushort)(joinMap.LoadSource.JoinNumber + tempOutputNum)]);
				outputNum++;
				Debug.Console(0, "*** Linking to Feedback: {0}", tempOutputNum);
			}

			for (var x = 1; x < 10; x++)
			{
				var tempx = x;
				trilist.SetStringSigAction((uint)(joinMap.LoadSource.JoinNumber + tempx), (s) => LoadSource(s, tempx));
			}

			var commMonitor = this as ICommunicationMonitor;
			if (commMonitor != null)
			{
				commMonitor.CommunicationMonitor.IsOnlineFeedback.LinkInputSig(trilist.BooleanInput[joinMap.IsOnline.JoinNumber]);
			}
		}

		void trilist_OnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
		{
			if (args.DeviceOnLine)
			{
				foreach (var item in CurrentRoutesFeedbacks)
				{
					item.Value.Feedback.FireUpdate();
				}
			}
		}

		public void LoadPerspective(string preset)
		{
			string[] args = { preset, _Config.DisplayID  };
			CurrentPerspective.Value = preset;
			SendCommand("LoadPerspective", args);
			GetCurrentRoutes();
		}

		public void LoadSource(string source, int tile)
		{
			string[] args = { CurrentPerspective.Value, source, string.Format("{0}", tile) };
			SendCommand("LoadSourceOnPerspective", args);
			GetCurrentRoutes();
		}

		public void GetCurrentRoutes()
		{
			string[] args = { CurrentPerspective.Value };
			SendCommand("getDispletList", args); 
		}

		public void GetCurrentPerspective()
		{
			string[] args = { _Config.DisplayID };
			SendCommand("GetSharedPerspectiveList", args);
		}
		public FeedbackCollection<Feedback> Feedbacks
		{
			get
			{
				var list = Feedbacks;
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
					GetCurrentRoutes();
					break;
				case 1:
					GetCurrentPerspective();
					break;
				default:
					PollState = 0;
					return;
			}
			PollState++; 
			
		}
		

		void Port_LineReceived(object dev, GenericCommMethodReceiveTextArgs args)
		{
			try
			{
				Debug.Console(DebugLevel, this, "Received: '{0}'", args.Text);

				if (args.Text.Contains("REgetDispletList"))
				{
					var responseWhole = args.Text.Split('|', '|')[6];
					var responseSplit = responseWhole.Split(',');
					Debug.Console(DebugLevel, this, "responseWhole {0}", responseWhole);
					int x = 2;
					for (x = 2; x < responseSplit.Length; x = x + 6)
					{
						Debug.Console(DebugLevel, this, "responseSplit {0} {1}", x, responseSplit[x+1]);
						
						var item =  int.Parse(responseSplit[x + 1]);
						Debug.Console(DebugLevel, this, "Item {0}: '{1}'", item, responseSplit[x]);
						if (item != 0)
						{
							CurrentRoutesFeedbacks[item].Value = responseSplit[x];
						}
					}

				}
				else if (args.Text.Contains(":REGetSharedPerspectiveList"))
				{
					var responseWhole = args.Text.Split('|', '|')[6];
					responseWhole = responseWhole.Remove(0, 3);
					Debug.Console(DebugLevel, this, "REGetSharedPerspectiveList {0}'", responseWhole);
					CurrentPerspective.Value = responseWhole;
				}
			}
			catch (Exception e)
			{
				Debug.Console(0, this, "Error Processing Port_LineReceived: {0}", e);
			}
		}

		void Send(string s)
		{
			Debug.Console(DebugLevel, this, "Send: '{0}'", ComTextHelper.GetEscapedText(s));
			Communication.SendText(s);
		}


	}

	public class PdtBarcoCrpFactory : EssentialsPluginDeviceFactory<PdtBarcoCrp>
	{
		public PdtBarcoCrpFactory()
		{
			MinimumEssentialsFrameworkVersion = "1.9.7";
			TypeNames = new List<string>() { "barcocrp", "barcocms", "BarcoCms" };
		}

		public override EssentialsDevice BuildDevice(DeviceConfig dc)
		{
			Debug.Console(1, "Factory Attempting to create new PdtBarcoCrpFactory Device");
			var comm = CommFactory.CreateCommForDevice(dc);
			var config = dc.Properties.ToObject<BarcoCrpConfigObject>();
			if (comm != null)
                try
                {
					
                    var newMe = new PdtBarcoCrp(dc.Key, dc.Name, comm, config );
                    return newMe;
                }
                catch
                {
                    var newMe = new PdtBarcoCrp(dc.Key, dc.Name, comm, config);
                    return newMe;
                }
			else
				return null;
		}
	}

}	