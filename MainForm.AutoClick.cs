// MainForm.AutoClick.cs
using System.Drawing;
using OpenCvSharp;
using automacro.modules;
using automacro.utils;

namespace automacro.gui
{
    public partial class MainForm
    {
        private CheckBox chkAutoClick;
        private RegionMonitorModule regionMonitorModule;

        private void ChkAutoClick_CheckedChanged(object sender, EventArgs e)
        {
            if (regionMonitorModule == null)
            {
                IntPtr gameWindow = IntPtr.Zero;
                var gameProcess = _selectedProcessTarget != null && _selectedProcessTarget.ProcessId != 0
                    ? WindowUtils.GetGameProcess(_selectedProcessTarget)
                    : null;
                if (gameProcess != null)
                    gameWindow = gameProcess.MainWindowHandle;

                Rectangle region = _selectedRegion ?? new Rectangle(0, 0, 1920, 1080);

                Mat template = null;
                string templatePath = "resources/template.png";
                if (System.IO.File.Exists(templatePath))
                    template = new Mat(templatePath, ImreadModes.Color);

                float threshold = 0.85f;
                System.Drawing.Point clickCoordinate = new System.Drawing.Point(_config.MouseX, _config.MouseY);

                regionMonitorModule = new RegionMonitorModule();
                regionMonitorModule.Configure(
                    imageDetector,
                    gameWindow,
                    region,
                    template,
                    threshold,
                    clickCoordinate,
                        _coordinator?.MacroEngine
                );
            }
            if (chkAutoClick.Checked)
            {
                regionMonitorModule.Configure(
                    imageDetector,
                    _selectedProcessTarget != null && _selectedProcessTarget.ProcessId != 0
                        ? WindowUtils.GetGameProcess(_selectedProcessTarget)?.MainWindowHandle ?? IntPtr.Zero
                        : IntPtr.Zero,
                    _selectedRegion ?? new Rectangle(0, 0, 1920, 1080),
                    System.IO.File.Exists("resources/template.png")
                        ? new Mat("resources/template.png", ImreadModes.Color)
                        : null,
                    0.85f,
                    new System.Drawing.Point(_config.MouseX, _config.MouseY),
                    _coordinator?.MacroEngine
                );
                regionMonitorModule.StartMonitoring();
            }
            else
            {
                regionMonitorModule.StopMonitoring();
            }
        }
    }
}
