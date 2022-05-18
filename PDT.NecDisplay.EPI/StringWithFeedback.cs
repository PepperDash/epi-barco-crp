using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using PepperDash.Essentials.Core;

namespace PDT.BarcoCrp.EPI
{
	public class StringWithFeedback
	{
		private string _Value;
		public StringFeedback Feedback;
		public string Value
		{
			get
			{
				return _Value;
			}
			set
			{
				_Value = value;
				Feedback.FireUpdate();
			}
		}
		public StringWithFeedback()
			
		{
			Feedback = new StringFeedback(() => Value);
		}
	}
}