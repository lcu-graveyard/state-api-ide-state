using LCU.Graphs;
using LCU.API.IDEState.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LCU.Graphs.Registry.Enterprises.IDE;
using LCU.State.API.Forge.Infrastructure.Harness;

namespace LCU.API.IDEState
{
    public static class Refresh
    {
        [FunctionName("Refresh")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            return await req.Manage<dynamic, LCUIDEState, LCUIDEStateHarness>(log, async (mgr, reqData) =>
            {
                return await mgr.Ensure();
            });
        }
    }
}
