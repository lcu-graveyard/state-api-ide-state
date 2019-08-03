using LCU.API.IDEState.Models;
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
            return await req.Manage<RemoveEditorRequest, LCUIDEState, LCUIDEStateHarness>(log, async (mgr, reqData) =>
            {
				return await mgr.RemoveEditor(reqData.EditorLookup);
            });
		}
	}
}
