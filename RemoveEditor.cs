using LCU.API.IDEState.Models;
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
	public class RemoveEditorRequest
	{
		[DataMember]
		public virtual string EditorLookup { get; set; }
	}

	public static class RemoveEditor
	{
		[FunctionName("RemoveEditor")]
		public static async Task<IActionResult> Run(
			[HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
			ILogger log)
		{
			return await req.WithState<RemoveEditorRequest, LCUIDEState>(log, async (details, reqData, state, stateMgr) =>
			{
				log.LogInformation("Remove Editor function processed a request.");

				state.Editors = state.Editors.Where(e => e.Lookup != reqData.EditorLookup).ToList();

				state.CurrentEditor = state.Editors.FirstOrDefault();

				state.SideBar.CurrentAction = state.SideBar.Actions.FirstOrDefault(a => $"{a.Group}|{a.Action}" == state.CurrentEditor?.Lookup);

				return state;
			});
		}
	}
}
