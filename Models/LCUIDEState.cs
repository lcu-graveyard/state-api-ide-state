using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using LCU.Graphs.Registry.Enterprises.IDE;

namespace LCU.API.IDEState.Models
{
	[Serializable]
	[DataContract]
	public class LCUIDEState
	{
		[DataMember]
		public virtual List<IDEActivity> Activities { get; set; }

		[DataMember]
		public virtual IDEActivity CurrentActivity { get; set; }

		[DataMember]
		public virtual IDEEditor CurrentEditor { get; set; }

		[DataMember]
		public virtual IDEPanel CurrentPanel { get; set; }

		[DataMember]
		public virtual List<IDEEditor> Editors { get; set; }

		[DataMember]
		public virtual bool Loading { get; set; }

		[DataMember]
		public virtual List<IDEPanel> Panels { get; set; }

		[DataMember]
		public virtual List<IDEActivity> RootActivities { get; set; }

		[DataMember]
		public virtual bool ShowPanels { get; set; }

		[DataMember]
		public virtual IDESideBar SideBar { get; set; }

		[DataMember]
		public virtual List<string> StatusChanges { get; set; }
	}
}
