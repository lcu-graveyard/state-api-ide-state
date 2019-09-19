using System;
using System.Globalization;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using LCU.StateAPI;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using LCU.Personas.Client.Applications;
using LCU.Personas.Client.DevOps;
using LCU.Personas.Client.Security;
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
        public LCUIDEStateHarness(HttpRequest req, ILogger logger, LCUIDEState state)
            : base(req, logger, state)
        {
            appMgr = req.ResolveClient<ApplicationManagerClient>(logger);
        }
        #endregion

        #region API Methods
        public virtual async Task<LCUIDEState> Ensure()
        {
            var activitiesResp = await appMgr.LoadIDEActivities(details.EnterpriseAPIKey);

            state.Activities = activitiesResp.Model;

            var appsResp = await appMgr.ListApplications(details.EnterpriseAPIKey);
            
            state.InfrastructureConfigured = activitiesResp.Status && !activitiesResp.Model.IsNullOrEmpty() && appsResp.Status && !appsResp.Model.IsNullOrEmpty();

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
                var sectionsResp = await appMgr.LoadIDESideBarSections(details.EnterpriseAPIKey, state.CurrentActivity.Lookup);

                state.SideBar.Actions = sectionsResp.Model.SelectMany(section =>
                {
                    var actionsResp = appMgr.LoadIDESideBarActions(details.EnterpriseAPIKey, state.CurrentActivity.Lookup, section).Result;

                    return actionsResp.Model;
                }).ToList();
            }
            else
                state.SideBar = new IDESideBar();


            return state;
        }

        public virtual async Task<LCUIDEState> RemoveEditor(string editorLookup)
        {
            logger.LogInformation("Remove Editor function processed a request.");

            state.Editors = state.Editors.Where(e => e.Lookup != editorLookup).ToList();

            state.CurrentEditor = state.Editors.FirstOrDefault();

            state.SideBar.CurrentAction = state.SideBar.Actions.FirstOrDefault(a => $"{a.Group}|{a.Action}" == state.CurrentEditor?.Lookup);

            return state;
        }

        public virtual async Task<LCUIDEState> SelectEditor(string editorLookup)
        {
            logger.LogInformation("Select Editor function processed a request.");

            state.SideBar.CurrentAction = state.SideBar.Actions.FirstOrDefault(a => $"{a.Group}|{a.Action}" == editorLookup);

            state.CurrentEditor = state.Editors.FirstOrDefault(a => a.Lookup == editorLookup);

            return state;
        }

        public virtual async Task<LCUIDEState> SelectSideBarAction(string group, string action, string section)
        {
            logger.LogInformation("Select Side Bar Action function processed a request.");

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
            logger.LogInformation("Set Activity function processed a request.");

            state.CurrentActivity = state.Activities.FirstOrDefault(a => a.Lookup == activityLookup);

            await LoadSideBar();

            state.SideBar.CurrentAction = state.SideBar.Actions.FirstOrDefault(a => $"{a.Group}|{a.Action}" == state.CurrentEditor?.Lookup);

            return state;
        }

        public virtual async Task<LCUIDEState> ToggleShowPanels(string group, string action)
        {
            logger.LogInformation("Toggle Show Panels function processed a request.");

            state.ShowPanels = !state.ShowPanels;

            return state;
        }
        #endregion

        #region Helpers
        #endregion
    }
}