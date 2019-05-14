using LCU.API.IDEState.Models;
using LCU.Graphs.Registry.Enterprises.IDE;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace LCU.API.IDEState
{
	[Serializable]
	[DataContract]
	public class SetActivityRequest
	{
		[DataMember]
		public virtual string Activity { get; set; }
	}

	public static class SetActivity
	{
		[FunctionName("SetActivity")]
		public static async Task<IActionResult> Run(
			[HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
			ILogger log)
		{
			return await req.WithState<SetActivityRequest, Models.LCUIDEState>(log, async (details, reqData, state, stateMgr) =>
			{
				log.LogInformation("Set Activity function processed a request.");

				var ideGraph = req.LoadGraph<IDEGraph>(log);

				state.CurrentActivity = state.Activities.FirstOrDefault(a => a.Lookup == reqData.Activity);

				if (state.SideBar == null)
					state.SideBar = new IDESideBar();

				var sections = await ideGraph.ListSideBarSections(state.CurrentActivity.Lookup, details.EnterpriseAPIKey, "Default");

				state.SideBar.Actions = sections.SelectMany(section =>
				{
					var actions = ideGraph.ListSectionActions(state.CurrentActivity.Lookup, section, details.EnterpriseAPIKey, "Default").Result;

					return actions.Select(act => new IDESideBarAction()
					{
						Title = act.Name,
						Section = section,
						Group = act.Group,
						Action = act.Action
					});
				}).ToList();

				state.SideBar.CurrentAction = state.SideBar.Actions.FirstOrDefault(a => $"{a.Group}|{a.Action}" == state.CurrentEditor?.Lookup);

				return state;
			});
		}
	}
}
