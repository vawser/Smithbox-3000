using Silk.NET.SDL;
using Smithbox.Core.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.Interface;

public static class DPI
{
    private const float DefaultDpi = 96f;
    private static float _dpi = DefaultDpi;

    public static EventHandler UIScaleChanged;

    public static float Dpi
    {
        get => _dpi;
        set
        {
            if (Math.Abs(_dpi - value) < 0.0001f) return; // Skip doing anything if no difference

            _dpi = value;
            if (CFG.Current.ScalebyDPI)
                UIScaleChanged?.Invoke(null, EventArgs.Empty);
        }
    }

    public static unsafe void UpdateDpi(Window* window)
    {
        if (SdlProvider.SDL.IsValueCreated && window != null)
        {
            int index = SdlProvider.SDL.Value.GetWindowDisplayIndex(window);
            float ddpi = DefaultDpi;
            float _ = 0f;
            SdlProvider.SDL.Value.GetDisplayDPI(index, ref ddpi, ref _, ref _);

            Dpi = ddpi;
        }
    }

    public static float GetUIScale()
    {
        var scale = CFG.Current.InterfaceDisplayScale;

        if (CFG.Current.ScalebyDPI)
            scale = scale / DefaultDpi * Dpi;

        return scale;
    }
}
