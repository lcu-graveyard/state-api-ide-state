using LCU.Graphs;
using LCU.Graphs.Registry.Enterprises;
using LCU.Graphs.Registry.Enterprises.IDE;
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
			return await req.WithState<SelectSideBarActionRequest, LCUIDEState>(log, async (details, reqData, state, stateMgr) =>
			{
				log.LogInformation("Select Side Bar Action function processed a request.");

				var ideGraph = req.LoadGraph<IDEGraph>(log);

				state.SideBar.CurrentAction = state.SideBar.Actions.FirstOrDefault(a => a.Group == reqData.Group && a.Action == reqData.Action);

				if (state.Editors.IsNullOrEmpty())
					state.Editors = new List<IDEEditor>();

				var actionLookup = $"{reqData.Group}|{reqData.Action}";

				if (!state.Editors.Select(e => e.Lookup).Contains(actionLookup))
				{
					var lcu = await ideGraph.GetLCU(reqData.Group, details.EnterpriseAPIKey, "Default");

					log.LogInformation($"LCU loaded: {lcu.ToJSON()}");

					var solution = await ideGraph.GetLCUSolution(reqData.Group, reqData.Action, details.EnterpriseAPIKey, "Default");

					log.LogInformation($"Solution loaded: {solution.ToJSON()}");

					var secAct = await ideGraph.GetSectionAction(state.CurrentActivity.Lookup, reqData.Section, reqData.Action, reqData.Group, details.EnterpriseAPIKey, "Default");

					log.LogInformation($"Section Action loaded: {secAct.ToJSON()}");

					var dafApp = await getLcuDafApp(req, log, details, lcu);

					log.LogInformation($"DAF App loaded: {dafApp.ToJSON()}");

					var files = await ideGraph.ListLCUFiles(lcu.Lookup, details.Host);

					log.LogInformation($"Files loaded: {files.ToJSON()}");

					state.Editors.Add(new IDEEditor()
					{
						Title = $"{reqData.Section} - {secAct.Name}",
						Lookup = actionLookup,
						Editor = solution.Element,
						Toolkit = $"{dafApp.BaseHref}{files.First()}"
					});
				}

				//switch (actionLookup)
				//{
				//	case "LCU|lcu-applications":
				//		break;

				//	case "LCU|lcu-state":
				//		if (!state.Editors.Select(e => e.Lookup).Contains(actionLookup))
				//			state.Editors.Add(new IDEEditor()
				//			{
				//				Title = "Solutions - LCU State (Overview)",
				//				Lookup = actionLookup,
				//				Editor = "lcu-state-config-manager-element",
				//				Toolkit = "/lcu/state/wc/lcu-state.lcu.js"
				//			});
				//		break;
				//}

				state.CurrentEditor = state.Editors.FirstOrDefault(a => a.Lookup == actionLookup);

				return state;
			});
		}

		private static async Task<DAFViewConfiguration> getLcuDafApp(HttpRequest req, ILogger log, LCUStateDetails details, LowCodeUnitConfig lcu)
		{
			var appGraph = req.LoadGraph<ApplicationGraph>(log);

			var apps = await appGraph.ListApplications(details.EnterpriseAPIKey);

			var lcuApp = apps?.FirstOrDefault(a => a.PathRegex == $"/_lcu/{lcu.Lookup}*");

			if (lcuApp != null)
			{
				var dafApps = await appGraph.GetDAFApplications(details.EnterpriseAPIKey, lcuApp.ID);

				var dafApp = dafApps?.FirstOrDefault(a => a.Metadata["BaseHref"].ToString() == $"/_lcu/{lcu.Lookup}/");

				return dafApp.JSONConvert<DAFViewConfiguration>();
			}

			return null;
		}
	}
}
