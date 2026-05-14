using System;
using System.Drawing;
using Rhino.PlugIns;
using Rhino.UI;

[assembly: System.Runtime.InteropServices.Guid("b2c3d4e5-f6a7-8901-bcde-f23456789012")]

[assembly: PlugInDescription(DescriptionType.Address, "")]
[assembly: PlugInDescription(DescriptionType.Country, "")]
[assembly: PlugInDescription(DescriptionType.Email, "")]
[assembly: PlugInDescription(DescriptionType.Phone, "")]
[assembly: PlugInDescription(DescriptionType.Fax, "")]
[assembly: PlugInDescription(DescriptionType.Organization, "PenguinClaw")]
[assembly: PlugInDescription(DescriptionType.UpdateUrl, "https://github.com/LCS3002/PenguinClaw-Rhinoceros/releases")]
[assembly: PlugInDescription(DescriptionType.WebSite, "https://github.com/LCS3002/PenguinClaw-Rhinoceros")]

namespace PenguinClaw
{
    // Plugin class - Rhino will use the assembly-level GUID to identify it
    public class PenguinClawPlugin : Rhino.PlugIns.PlugIn
    {
        public PenguinClawPlugin()
        {
            Instance = this;
        }

        public static PenguinClawPlugin Instance { get; private set; }

        protected override LoadReturnCode OnLoad(ref string errorMessage)
        {
            try
            {
                // Register the panel
                Rhino.UI.Panels.RegisterPanel(
                    this,
                    typeof(PenguinClawPanel),
                    "PenguinClaw",
                    PenguinClawPanel.PanelIcon
                );

                // Build command registry on background thread (non-blocking)
                System.Threading.ThreadPool.QueueUserWorkItem(_ => RhinoCommandRegistry.Build());

                return LoadReturnCode.Success;
            }
            catch (Exception ex)
            {
                errorMessage = $"Failed to register panel: {ex.Message}";
                return LoadReturnCode.ErrorShowDialog;
            }
        }

        protected override void OnShutdown()
        {
            try { PenguinClawServer.StopServer(); } catch { }
            base.OnShutdown();
        }
    }
}
