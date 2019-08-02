using System;
using System.Globalization;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using LCU.Runtime;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using LCU.Presentation.Personas.Applications;
using LCU.Presentation.Personas.DevOps;
using LCU.Presentation.Personas.Security;
using LCU.API.IDEState.Models;
using LCU.Graphs.Registry.Enterprises.IDE;

namespace LCU.State.API.Forge.Infrastructure.Harness
{
    public class LCUIDEStateHarness : LCUStateHarness<LCUIDEState>
    {
        #region Fields
        protected readonly ApplicationManagerClient appMgr;
        #endregion

        #region Properties

        #endregion

        #region Constructors
        public LCUIDEStateHarness(HttpRequest req, ILogger log, LCUIDEState state)
            : base(req, log, state)
        {
            appMgr = req.ResolveClient<ApplicationManagerClient>(log);
        }
        #endregion

        #region API Methods
        public virtual async Task<LCUIDEState> Ensure()
        {
            var activitiesResp = await appMgr.LoadIDEActivities(details.EnterpriseAPIKey);

            state.Activities = activitiesResp.Model;

            state.InfrastructureConfigured = !state.Activities.IsNullOrEmpty();

            state.RootActivities = new List<IDEActivity>();

            if (state.InfrastructureConfigured)
                state.RootActivities.Add(new IDEActivity()
                {
                    Icon = "settings",
                    Lookup = Environment.GetEnvironmentVariable("FORGE-SETTINGS-PATH") ?? "/forge-settings",
                    Title = "Settings"
                });

            state.RootActivities.Add(new IDEActivity()
            {
                Icon = "cloud",
                Lookup = Environment.GetEnvironmentVariable("FORGE-INFRASTRUCTURE-PATH") ?? "/forge-infra",
                Title = "Infrastructure"
            });

            return await LoadSideBar();
        }

        public virtual async Task<LCUIDEState> LoadSideBar()
        {
            if (state.SideBar == null)
                state.SideBar = new IDESideBar();

            if (state.CurrentActivity != null)
            {
                var actionsResp = await appMgr.LoadIDESideBarActions(details.EnterpriseAPIKey, state.CurrentActivity.Lookup);

                state.SideBar.Actions = actionsResp.Model;
            }
            else
                state.SideBar = new IDESideBar();


            return state;
        }

        public virtual async Task<LCUIDEState> SelectSideBarAction(string group, string action, string section)
        {
            log.LogInformation("Select Side Bar Action function processed a request.");

            state.SideBar.CurrentAction = state.SideBar.Actions.FirstOrDefault(a => a.Group == group && a.Action == action);

            if (state.Editors.IsNullOrEmpty())
                state.Editors = new List<IDEEditor>();

            var actionLookup = $"{group}|{action}";

            if (!state.Editors.Select(e => e.Lookup).Contains(actionLookup))
            {
                var ideEditorResp = await appMgr.LoadIDEEditor(details.EnterpriseAPIKey, group, action, section, details.Host, state.CurrentActivity.Lookup);

                if (ideEditorResp.Status)
                    state.Editors.Add(ideEditorResp.Model);
            }

            state.CurrentEditor = state.Editors.FirstOrDefault(a => a.Lookup == actionLookup);

            return state;
        }

        public virtual async Task<LCUIDEState> SetActivity(string activityLookup)
        {
            log.LogInformation("Set Activity function processed a request.");

            state.CurrentActivity = state.Activities.FirstOrDefault(a => a.Lookup == activityLookup);

            await LoadSideBar();

            state.SideBar.CurrentAction = state.SideBar.Actions.FirstOrDefault(a => $"{a.Group}|{a.Action}" == state.CurrentEditor?.Lookup);

            return state;
        }
        #endregion

        #region Helpers
        #endregion
    }
}