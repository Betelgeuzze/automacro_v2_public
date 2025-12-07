// RegionMonitorModule.cs
using System;
using System.Drawing;
using OpenCvSharp;
using automacro.utils;

namespace automacro.modules
{
    public interface IRegionMonitorModule
    {
        void Attach();
        void Detach();
        void StartMonitoring();
        void StopMonitoring();
        void SavePreset(string name);
        void LoadPreset(string name);

        event EventHandler<TemplateMatchedEventArgs> OnTemplateMatched;
        event EventHandler<RegionChangedEventArgs> OnRegionChanged;
    }

    public class TemplateMatchedEventArgs : EventArgs
    {
        public System.Drawing.Point MatchLocation { get; set; }
        public float MatchScore { get; set; }
    }

    public class RegionChangedEventArgs : EventArgs
    {
        public Rectangle Region { get; set; }
    }

public class RegionMonitorModule : IRegionMonitorModule, IDisposable
{
    public event EventHandler<TemplateMatchedEventArgs> OnTemplateMatched;
    public event EventHandler<RegionChangedEventArgs> OnRegionChanged;

        public void Attach() { /* Attach logic */ }
        public void Detach() { /* Detach logic */ }
        public void StartMonitoring()
        {
            // Example: Start a timer to monitor region every 500ms
            _monitorTimer = new System.Windows.Forms.Timer();
            _monitorTimer.Interval = 500;
            _monitorTimer.Tick += (s, e) => MonitorRegion();
            _monitorTimer.Start();
        }

        public void StopMonitoring()
        {
            _monitorTimer?.Stop();
            _monitorTimer?.Dispose();
            _monitorTimer = null;
        }

        public void SavePreset(string name) { /* Save preset logic */ }
        public void LoadPreset(string name) { /* Load preset logic */ }

        // --- Monitoring Logic ---
        private System.Windows.Forms.Timer _monitorTimer;
        private ImageDetector _imageDetector;
        private IntPtr _gameWindow;
        private Rectangle _region;
        private Mat _template;
        private float _threshold = 0.8f;
        private System.Drawing.Point _clickCoordinate;
        private MacroEngine _macroEngine;

public void Configure(ImageDetector imageDetector, IntPtr gameWindow, Rectangle region, Mat template, float threshold, System.Drawing.Point clickCoordinate, MacroEngine macroEngine = null)
{
    _imageDetector = imageDetector;
    _gameWindow = gameWindow;
    _region = region;
    if (_template != null)
    {
        _template.Dispose();
        _template = null;
    }
    _template = template;
    _threshold = threshold;
    _clickCoordinate = clickCoordinate;
    _macroEngine = macroEngine;
}

        private void MonitorRegion()
        {
            // Only run autoclick if MacroEngine is running
            if (_macroEngine != null && !_macroEngine.IsRunning)
            {
                return;
            }

            // Capture region screenshot
            var screenshot = WindowUtils.CaptureRegion(_gameWindow, _region, _imageDetector != null ? _imageDetector.GetType().GetProperty("_consolePanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(_imageDetector) as automacro.gui.Controls.ConsolePanel : null);
            if (screenshot == null)
            {
                return;
            }

            using var matRaw = _imageDetector.BitmapToMat(screenshot);
            Mat mat = matRaw;
            // Convert CV_8UC4 to CV_8UC3 if needed
            if (matRaw.Type() == OpenCvSharp.MatType.CV_8UC4)
            {
                mat = new OpenCvSharp.Mat();
                OpenCvSharp.Cv2.CvtColor(matRaw, mat, OpenCvSharp.ColorConversionCodes.BGRA2BGR);
            }
            double matchScore = 0;
            bool match = false;
            if (_template != null && !_template.Empty())
            {
                matchScore = GetTemplateMatchScore(mat, _template);
                // Removed debug log
                match = matchScore >= _threshold;
            }
            screenshot.Dispose();

            if (match)
            {
                // Removed debug log
                WindowUtils.SendMouseClick(_gameWindow, _clickCoordinate.X, _clickCoordinate.Y);
                OnTemplateMatched?.Invoke(this, new TemplateMatchedEventArgs { MatchLocation = _clickCoordinate, MatchScore = (float)matchScore });
            }
            else
            {
                // Removed debug log
            }
        } // <-- closes MonitorRegion()

        private double GetTemplateMatchScore(Mat source, Mat template)
        {
            if (source.Empty() || template.Empty()) return 0;
            if (source.Width < template.Width || source.Height < template.Height) return 0;
            using var result = new Mat();
            Cv2.MatchTemplate(source, template, result, TemplateMatchModes.CCoeffNormed);
            Cv2.MinMaxLoc(result, out double minVal, out double maxVal, out OpenCvSharp.Point minLoc, out OpenCvSharp.Point maxLoc);
            return maxVal;
        }

// Dispose method for cleaning up resources
public void Dispose()
{
    StopMonitoring();
    if (_template != null)
    {
        _template.Dispose();
        _template = null;
    }
}

        private bool FindTemplateOpenCV(Mat source, Mat template, double threshold, string name)
        {
            if (source.Empty() || template.Empty()) return false;
            if (source.Width < template.Width || source.Height < template.Height) return false;
            using var result = new Mat();
            Cv2.MatchTemplate(source, template, result, TemplateMatchModes.CCoeffNormed);
            Cv2.MinMaxLoc(result, out double minVal, out double maxVal, out OpenCvSharp.Point minLoc, out OpenCvSharp.Point maxLoc);
            return maxVal >= threshold;
        }
    }
}
