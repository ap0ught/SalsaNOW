namespace SuperSalsaNOW.Windows.Interop;

using System.Runtime.InteropServices;

/// <summary>
/// P/Invoke declarations for Windows API functions
/// ⚠️ WINDOWS-ONLY: Will not compile/run on other platforms
/// </summary>
public static class WindowsAPI
{
    /// <summary>
    /// Find window by class name and/or window title
    /// </summary>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

    /// <summary>
    /// Post a message to a window
    /// </summary>
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    /// <summary>
    /// WM_CLOSE message constant
    /// </summary>
    public const uint WM_CLOSE = 0x0010;

    /// <summary>
    /// Check if process is running with a specific window title
    /// </summary>
    public static bool IsWindowOpen(string windowTitle)
    {
        var hwnd = FindWindow(null, windowTitle);
        return hwnd != IntPtr.Zero;
    }

    /// <summary>
    /// Close window by title
    /// </summary>
    public static bool CloseWindow(string windowTitle)
    {
        var hwnd = FindWindow(null, windowTitle);
        if (hwnd == IntPtr.Zero)
            return false;

        return PostMessage(hwnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
    }
}
