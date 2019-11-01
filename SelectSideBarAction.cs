using LCU.Graphs;
using LCU.Graphs.Registry.Enterprises;
using LCU.API.IDEState.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using LCU.Graphs.Registry.Enterprises.IDE;
using LCU.Graphs.Registry.Enterprises.Apps;
using LCU.State.API.Forge.Infrastructure.Harness;

namespace LCU.API.IDEState
{
	[Serializable]
	[DataContract]
	public class SelectSideBarActionRequest
	{
		[DataMember]
		public virtual string Action { get; set; }

		[DataMember]
		public virtual string Group { get; set; }

		[DataMember]
		public virtual string Section { get; set; }
	}

	public static class SelectSideBarAction
	{
		[FunctionName("SelectSideBarAction")]
		public static async Task<IActionResult> Run(
			[HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
			ILogger log)
		{
            return await req.Manage<SelectSideBarActionRequest, LCUIDEState, LCUIDEStateHarness>(log, async (mgr, reqData) =>
            {
				log.LogInformation($"Selecting SideBar Action: {reqData.Group} {reqData.Action} {reqData.Section}");

                return await mgr.SelectSideBarAction(reqData.Group, reqData.Action, reqData.Section);
            });
		}
	}
}
