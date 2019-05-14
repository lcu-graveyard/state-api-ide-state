using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using LCU.API.IDEState.Models;
using System.Runtime.Serialization;

namespace LCU.API.IDEState
{
	[Serializable]
	[DataContract]
	public class ToggleShowPanelsRequest
	{
		[DataMember]
		public virtual string Action { get; set; }

		[DataMember]
		public virtual string Group { get; set; }
	}

	public static class ToggleShowPanels
    {
        [FunctionName("ToggleShowPanels")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
		{
			return await req.WithState<SetActivityRequest, LCUIDEState>(log, async (details, reqData, state, stateMgr) =>
			{
				log.LogInformation("Toggle Show Panels function processed a request.");

				state.ShowPanels = !state.ShowPanels;

				return state;
			});
		}
    }
}
