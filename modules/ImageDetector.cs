using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Collections.Generic;
using OpenCvSharp;
using automacro.config;
using automacro.utils;
using automacro.models;

namespace automacro.modules
{
    public class ImageDetector : IDisposable
    {
        private readonly AppConfig _config;
        private Mat _popupTemplate;
        private Mat _uiTemplate;
        private bool _templatesLoaded = false;
        private automacro.gui.Controls.ConsolePanel _consolePanel;

        public ImageDetector(AppConfig config, automacro.gui.Controls.ConsolePanel consolePanel = null)
        {
            _consolePanel = consolePanel;
            Log($"🎯 ImageDetector constructor called at {DateTime.Now:HH:mm:ss}");
            Log($"🎯 Resources directory from config: {config.Paths.ResourcesDir}");
            Log($"🎯 Template file: {config.Paths.TemplateFile}");
            Log($"🎯 UI check file: {config.Paths.UiCheckFile}");
            
            _config = config;
            LoadTemplates();
        }

        private void LoadTemplates()
        {
            try
            {
                var asm = typeof(ImageDetector).Assembly;
                string popupRes = "automacro.resources.popup.png";
                string uiRes = "automacro.resources.ui_check.png";

using (var popupStream = asm.GetManifestResourceStream(popupRes))
{
    if (_popupTemplate != null)
    {
        _popupTemplate.Dispose();
        _popupTemplate = null;
    }
    if (popupStream != null)
    {
        _popupTemplate = Mat.FromStream(popupStream, ImreadModes.Color);
        Log($"✅ Loaded popup template from embedded resource ({_popupTemplate.Width}x{_popupTemplate.Height})");
    }
    else
    {
        Log($"❌ Popup template not found in embedded resources");
        _popupTemplate = new Mat();
    }
}

using (var uiStream = asm.GetManifestResourceStream(uiRes))
{
    if (_uiTemplate != null)
    {
        _uiTemplate.Dispose();
        _uiTemplate = null;
    }
    if (uiStream != null)
    {
        _uiTemplate = Mat.FromStream(uiStream, ImreadModes.Color);
        Log($"✅ Loaded UI template from embedded resource ({_uiTemplate.Width}x{_uiTemplate.Height})");
    }
    else
    {
        Log($"❌ UI template not found in embedded resources");
        _uiTemplate = new Mat();
    }
}

                _templatesLoaded = _popupTemplate != null && !_popupTemplate.Empty() && 
                                   _uiTemplate != null && !_uiTemplate.Empty();
                
                Log($"🔍 Templates loaded: {_templatesLoaded}");
            }
catch (Exception ex)
{
    Log($"❌ Error loading templates: {ex.Message}");
    if (_popupTemplate != null)
    {
        _popupTemplate.Dispose();
        _popupTemplate = null;
    }
    if (_uiTemplate != null)
    {
        _uiTemplate.Dispose();
        _uiTemplate = null;
    }
    _popupTemplate = new Mat();
    _uiTemplate = new Mat();
    _templatesLoaded = false;
}
        }

public DetectionResult CheckGameState(IntPtr gameWindow)
        {
            Log("TEST: Entered CheckGameState");
            Log($"[{DateTime.Now:HH:mm:ss}] 🎯 CheckGameState called");
            Log($"[{DateTime.Now:HH:mm:ss}] 🎯 gameWindow: {gameWindow}");
            Log($"[{DateTime.Now:HH:mm:ss}] 🎯 _templatesLoaded: {_templatesLoaded}");
            Log($"[DEBUG] _config is null: {_config == null}");
            Log($"[DEBUG] _config.SearchRegions is null: {_config?.SearchRegions == null}");
            Log($"[DEBUG] _config.SearchRegions.Popup is null: {_config?.SearchRegions?.Popup == null}");
            Log($"[DEBUG] _config.SearchRegions.Ui is null: {_config?.SearchRegions?.Ui == null}");
            Log($"[DEBUG] _popupTemplate is null: {_popupTemplate == null}");
            Log($"[DEBUG] _uiTemplate is null: {_uiTemplate == null}");

            // Fallback initialization to prevent null reference
            if (_config.SearchRegions == null)
            {
                Log("[DEBUG] Initializing _config.SearchRegions to default");
                _config.SearchRegions = new automacro.models.SearchRegionsConfig
                {
                    Popup = new System.Collections.Generic.List<int> { 0, 0, 1920, 1080 },
                    Ui = new System.Collections.Generic.List<int> { 0, 0, 1920, 1080 }
                };
            }
            if (_config.SearchRegions.Popup == null)
            {
                Log("[DEBUG] Initializing _config.SearchRegions.Popup to default");
                _config.SearchRegions.Popup = new System.Collections.Generic.List<int> { 0, 0, 1920, 1080 };
            }
            if (_config.SearchRegions.Ui == null)
            {
                Log("[DEBUG] Initializing _config.SearchRegions.Ui to default");
                _config.SearchRegions.Ui = new System.Collections.Generic.List<int> { 0, 0, 1920, 1080 };
            }

            // FIXED: Safe check for Mat objects

            // Simple clear check
            if (_popupTemplate == null)
            {
                Log($"[{DateTime.Now:HH:mm:ss}] 🎯 _popupTemplate is NULL");
            }
            else
            {
                Log($"[{DateTime.Now:HH:mm:ss}] 🎯 _popupTemplate exists, Empty={_popupTemplate.Empty}");
            }

            if (_uiTemplate == null)
            {
                Log($"[{DateTime.Now:HH:mm:ss}] 🎯 _uiTemplate is NULL");
            }
            else
            {
                Log($"[{DateTime.Now:HH:mm:ss}] 🎯 _uiTemplate exists, Empty={_uiTemplate.Empty}");
            }

            if (!_templatesLoaded || gameWindow == IntPtr.Zero)
            {
                Log($"[{DateTime.Now:HH:mm:ss}] ⚠️ Cannot check game state: templatesLoaded={_templatesLoaded}, gameWindow={gameWindow}");
                return new DetectionResult { Success = false };
            }

            // Extra logging for screenshot and Mat state
            if (_popupTemplate == null || _popupTemplate.Empty())
                Log("DEBUG: _popupTemplate is null or empty");
            if (_uiTemplate == null || _uiTemplate.Empty())
                Log("DEBUG: _uiTemplate is null or empty");

            try
            {
                // Convert config regions to Rectangles
                var popupRegion = ListToRectangle(_config.SearchRegions.Popup);
                var uiRegion = ListToRectangle(_config.SearchRegions.Ui);

                Log($"[DEBUG] popupRegion is null: {popupRegion == null}");
                Log($"[DEBUG] uiRegion is null: {uiRegion == null}");
                Log($"🔍 Scanning regions (OpenCV):");
                Log($"   Popup region: {popupRegion}");
                Log($"   UI region: {uiRegion}");

                Log("TEST: About to call WindowUtils.CaptureRegion for popup and UI regions");
                if (gameWindow == IntPtr.Zero)
                    Log("[DEBUG] gameWindow is zero");

                // Capture both regions
                Bitmap popupScreenshot = null;
                Bitmap uiScreenshot = null;
                try
                {
                    popupScreenshot = WindowUtils.CaptureRegion(gameWindow, popupRegion, _consolePanel);
                    Log($"TEST: Called WindowUtils.CaptureRegion for popup, result null: {popupScreenshot == null}");
                }
                catch (Exception ex)
                {
                    Log($"❌ Exception in WindowUtils.CaptureRegion (popup): {ex.Message}");
                }
                try
                {
                    uiScreenshot = WindowUtils.CaptureRegion(gameWindow, uiRegion, _consolePanel);
                    Log($"TEST: Called WindowUtils.CaptureRegion for UI, result null: {uiScreenshot == null}");
                }
                catch (Exception ex)
                {
                    Log($"❌ Exception in WindowUtils.CaptureRegion (UI): {ex.Message}");
                }

if (popupScreenshot == null || uiScreenshot == null)
{
    Log("❌ Could not capture screenshots");
    return new DetectionResult { Success = false };
}
else
{
    Log($"✅ Captured popup screenshot: {popupScreenshot.Width}x{popupScreenshot.Height}");
    Log($"✅ Captured UI screenshot: {uiScreenshot.Width}x{uiScreenshot.Height}");
}

using var popupMatRaw = BitmapToMat(popupScreenshot);
using var uiMatRaw = BitmapToMat(uiScreenshot);

Mat popupMat = popupMatRaw;
Mat uiMat = uiMatRaw;

// Convert CV_8UC4 to CV_8UC3 if needed
if (popupMatRaw.Type() == MatType.CV_8UC4)
{
    popupMat = new Mat();
    Cv2.CvtColor(popupMatRaw, popupMat, ColorConversionCodes.BGRA2BGR);
}
if (uiMatRaw.Type() == MatType.CV_8UC4)
{
    uiMat = new Mat();
    Cv2.CvtColor(uiMatRaw, uiMat, ColorConversionCodes.BGRA2BGR);
}

Log($"✅ Converted popup Bitmap to Mat: {popupMat.Width}x{popupMat.Height}, Empty={popupMat.Empty()}, Type={popupMat.Type()}");
Log($"✅ Converted UI Bitmap to Mat: {uiMat.Width}x{uiMat.Height}, Empty={uiMat.Empty()}, Type={uiMat.Type()}");

bool popupDetected = false;
bool uiDetected = false;

if (_popupTemplate != null && !_popupTemplate.Empty())
    popupDetected = FindTemplateOpenCV(popupMat, _popupTemplate, 0.8f, "Popup");

if (_uiTemplate != null && !_uiTemplate.Empty())
    uiDetected = FindTemplateOpenCV(uiMat, _uiTemplate, 0.7f, "UI");

// Dispose original screenshots
popupScreenshot.Dispose();
uiScreenshot.Dispose();

                Log($"🔍 OpenCV Template Matching Results:");
                Log($"   Popup Found: {popupDetected}");
                Log($"   UI Found: {uiDetected}");

                // Clean up Bitmaps
                popupScreenshot.Dispose();
                uiScreenshot.Dispose();

                return new DetectionResult 
                {
                    Success = true,
                    PopupDetected = popupDetected,
                    UiDetected = uiDetected,
                    GameRunning = true
                };
            }
            catch (Exception ex)
            {
                Log($"❌ Error in CheckGameState: {ex.Message}");
                Log(ex.StackTrace);
                return new DetectionResult { Success = false };
            }
        }

        private bool FindTemplateOpenCV(Mat source, Mat template, double threshold, string name)
        {
            if (source.Empty() || template.Empty())
            {
                Log($"❌ {name}: Source or template is empty");
                return false;
            }

            if (source.Width < template.Width || source.Height < template.Height)
            {
                Log($"❌ {name}: Source is smaller than template");
                return false;
            }

            // Ensure Mat types match
            if (source.Type() != template.Type())
            {
                Log($"❌ {name}: Source type ({source.Type()}) does not match template type ({template.Type()})");
                return false;
            }

            using var result = new Mat();

            // Use normalized cross correlation for better accuracy
            Cv2.MatchTemplate(source, template, result, TemplateMatchModes.CCoeffNormed);

            Cv2.MinMaxLoc(result, out double minVal, out double maxVal, out OpenCvSharp.Point minLoc, out OpenCvSharp.Point maxLoc);

            Log($"   {name} - Best match: {maxVal:F3} (threshold: {threshold:F3})");

            if (maxVal >= threshold)
            {
                Log($"   ✅ {name} found at position ({maxLoc.X}, {maxLoc.Y})");
                return true;
            }
            else
            {
                Log($"   ❌ {name} not found. Match score: {maxVal:F3} (threshold: {threshold:F3})");
            }

            return false;
        }

        public Mat BitmapToMat(Bitmap bitmap)
        {
            if (bitmap == null)
                return new Mat();

            try
            {
                // Fast path for supported formats
                if (bitmap.PixelFormat == PixelFormat.Format24bppRgb ||
                    bitmap.PixelFormat == PixelFormat.Format32bppArgb ||
                    bitmap.PixelFormat == PixelFormat.Format32bppPArgb ||
                    bitmap.PixelFormat == PixelFormat.Format32bppRgb)
                {
                    var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                    var bmpData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);

                    try
                    {
                        int channels = Image.GetPixelFormatSize(bitmap.PixelFormat) / 8;
                        long stride = bmpData.Stride;
                        var matType = channels == 3 ? MatType.CV_8UC3 : MatType.CV_8UC4;

                        var mat = Mat.FromPixelData(bitmap.Height, bitmap.Width, matType, bmpData.Scan0, stride);
                        return mat.Clone(); // Clone to avoid referencing unlocked memory
                    }
                    finally
                    {
                        bitmap.UnlockBits(bmpData);
                    }
                }
                // Fallback for unsupported formats (e.g., 16bpp, 8bpp, etc.)
                using var ms = new MemoryStream();
                bitmap.Save(ms, ImageFormat.Png);
                ms.Position = 0;
                return Mat.FromStream(ms, ImreadModes.Color);
            }
            catch
            {
                return new Mat();
            }
        }


        private Rectangle ListToRectangle(List<int> coords)
        {
            if (coords == null || coords.Count != 4)
            {
                // Removed debug log
                return new Rectangle(0, 0, 1920, 1080); // Default to full screen
            }
            
            int x = coords[0];
            int y = coords[1];
            int width = coords[2] - coords[0];
            int height = coords[3] - coords[1];
            
            return new Rectangle(x, y, width, height);
        }

        public void Dispose()
        {
            try
            {
                if (_popupTemplate != null)
                {
                    _popupTemplate.Dispose();
                }
                
                if (_uiTemplate != null)
                {
                    _uiTemplate.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log($"❌ Error disposing ImageDetector: {ex.Message}");
            }
        }

        private void Log(string message)
        {
        }

        
    }

}

// Fallback SearchRegions class if not present in models
public class FallbackSearchRegions
{
    public System.Collections.Generic.List<int> Popup { get; set; } = new System.Collections.Generic.List<int> { 0, 0, 1920, 1080 };
    public System.Collections.Generic.List<int> Ui { get; set; } = new System.Collections.Generic.List<int> { 0, 0, 1920, 1080 };
}

namespace automacro.modules
{
    public class DetectionResult
    {
        public bool Success { get; set; }
        public bool PopupDetected { get; set; }
        public bool UiDetected { get; set; }
        public bool GameRunning { get; set; }
    }
}
