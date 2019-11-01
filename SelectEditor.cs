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
using LCU.State.API.Forge.Infrastructure.Harness;

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
            return await req.Manage<SelectEditorRequest, LCUIDEState, LCUIDEStateHarness>(log, async (mgr, reqData) =>
            {
				log.LogInformation($"Selecting Editor {reqData.EditorLookup}");

				return await mgr.SelectEditor(reqData.EditorLookup);
            });
		}
    }
}
