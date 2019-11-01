using LCU.API.IDEState.Models;
using LCU.Graphs.Registry.Enterprises.IDE;
using LCU.State.API.Forge.Infrastructure.Harness;
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
            return await req.Manage<SetActivityRequest, LCUIDEState, LCUIDEStateHarness>(log, async (mgr, reqData) =>
            {
				log.LogInformation($"Set Activity {reqData.Activity}");

                return await mgr.SetActivity(reqData.Activity);
            });
		}
	}
}
