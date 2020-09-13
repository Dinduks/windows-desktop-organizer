using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

// Source: https://stackoverflow.com/a/28282339/604041
namespace VirtualDesktopShowcase
{
  static class Util
  {
    private delegate bool Win32Callback(IntPtr hwnd, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.Dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumChildWindows(IntPtr parentHandle, Win32Callback callback, IntPtr lParam);


    public static IEnumerable<IntPtr> GetRootWindowsOfProcess(int pid)
    {
      IEnumerable<IntPtr> rootWindows = GetChildWindows(IntPtr.Zero);
      var dsProcRootWindows = new List<IntPtr>();
      foreach (IntPtr hWnd in rootWindows)
      {
        uint lpdwProcessId;
        GetWindowThreadProcessId(hWnd, out lpdwProcessId);
        if (lpdwProcessId == pid)
          dsProcRootWindows.Add(hWnd);
      }
      return dsProcRootWindows;
    }

    private static IEnumerable<IntPtr> GetChildWindows(IntPtr parent)
    {
      var result = new List<IntPtr>();
      GCHandle listHandle = GCHandle.Alloc(result);
      try
      {
        var childProc = new Win32Callback(EnumWindow);
        EnumChildWindows(parent, childProc, GCHandle.ToIntPtr(listHandle));
      }
      finally
      {
        if (listHandle.IsAllocated)
          listHandle.Free();
      }
      return result;
    }

    private static bool EnumWindow(IntPtr handle, IntPtr pointer)
    {
      GCHandle gch = GCHandle.FromIntPtr(pointer);
      var list = gch.Target as List<IntPtr>;
      if (list == null)
      {
        throw new InvalidCastException("GCHandle Target could not be cast as List<IntPtr>");
      }
      list.Add(handle);
      //  You can modify this to check to see if you want to cancel the operation, then return a null here
      return true;
    }
  }
}