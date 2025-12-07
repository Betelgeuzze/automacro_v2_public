// utils/ProcessSelector.cs
using System.Windows.Forms;
using Automacro.Models;
using Automacro.Gui.Controls;

namespace Automacro.Utils
{
public static class ProcessSelector
{
    /// <summary>
    /// Shows the process browser dialog and returns the selected ProcessTarget, or null if cancelled.
    /// </summary>
    public static ProcessTarget SelectProcess(IWin32Window owner = null)
    {
        using (var dlg = new ProcessBrowserForm())
        {
            var result = owner == null ? dlg.ShowDialog() : dlg.ShowDialog(owner);
            if (result == DialogResult.OK)
                return dlg.SelectedTarget;
            return null;
    }
}
}
}
