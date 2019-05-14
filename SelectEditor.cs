using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using LCU.API.IDEState.Models;
using System.Collections.Generic;
using System.Linq;

namespace LCU.API.IDEState
{
	[Serializable]
	[DataContract]
	public class SelectEditorRequest
	{
		[DataMember]
		public virtual string EditorLookup { get; set; }
	}

	public static class SelectEditor
    {
        [FunctionName("SelectEditor")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
		{
			return await req.WithState<SelectEditorRequest, LCUIDEState>(log, async (details, reqData, state, stateMgr) =>
			{
				log.LogInformation("Select Editor function processed a request.");

				state.SideBar.CurrentAction = state.SideBar.Actions.FirstOrDefault(a => $"{a.Group}|{a.Action}" == reqData.EditorLookup);

				state.CurrentEditor = state.Editors.FirstOrDefault(a => a.Lookup == reqData.EditorLookup);

				return state;
			});
		}
    }
}
