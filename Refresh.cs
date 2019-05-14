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

namespace LCU.API.IDEState
{
    public static class Refresh
    {
        [FunctionName("Refresh")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            return await req.WithState<dynamic, Models.LCUIDEState>(log, async (details, reqData, state, stateMgr) =>
            {
                var ideGraph = req.LoadGraph<IDEGraph>(log);

                var activities = await ideGraph.ListActivities(details.EnterpriseAPIKey, "Default");

                state.Activities = activities.Select(activity => new IDEActivity()
                {
                    Icon = activity.Icon,
                    IconSet = activity.IconSet,
                    Lookup = activity.Lookup,
                    Title = activity.Title
                }).ToList();

                if (state.RootActivities.IsNullOrEmpty())
                {
                    state.RootActivities.Add(new IDEActivity()
                    {
						Icon = "cloud",
						Lookup = "/forge-infra",
						Title = "Infrastructure"
                    });
					
                    state.RootActivities.Add(new IDEActivity()
                    {
						Icon = "settings",
						Lookup = "/forge-settings",
						Title = "Settings"
                    });
                }

                if (state.SideBar == null)
                    state.SideBar = new IDESideBar();

                if (state.CurrentActivity != null)
                {
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
                }

                return state;
            });
        }
    }
}
