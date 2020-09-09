using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using WindowsDesktop;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace VirtualDesktopShowcase
{
  partial class MainWindow
  {
    public MainWindow()
    {
      this.InitializeComponent();
      InitializeComObjects();
    }

    private static async void InitializeComObjects()
    {
      try
      {
        await VirtualDesktopProvider.Default.Initialize(TaskScheduler.FromCurrentSynchronizationContext());
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, "Failed to initialize.");
      }

      VirtualDesktop.CurrentChanged += (sender, args) => System.Diagnostics.Debug.WriteLine($"Desktop changed: {args.NewDesktop.Id}");
    }

    private void Run(object sender, RoutedEventArgs e)
    {
      var allWindows = getAllWindows();
      var desktops = VirtualDesktop.GetDesktops();

      var apps = new List<global::App> {
        new global::App("brave", 1),
        new global::App("code", 3),
        new global::App("chrome", 3),
        new global::App("discord", 5),
        new global::App("foobar2000", 2),
        new global::App("jamm", 4),
        new global::App("spotify", 2),
        new global::App("telegram", 4),
        new global::App("thunderbird", 1),
        new global::App("twist", 4),
      };

      // TODO: create desktops if there aren't enough

      apps.ForEach(app =>
      {
        var regex = new Regex(app.NameRegexp.ToLower());
        var windows = (allWindows.Where(w => regex.IsMatch(w.ProcessName.ToLower()))).ToList();
        windows.ForEach(window =>
        {
          var desktop = desktops[app.TargetDesktop - 1];
          VirtualDesktopHelper.MoveToDesktop(window.hWnd, desktop);
        });
      });
    }

    private WindowRef[] getAllWindows()
    {
      Process[] processlist = Process.GetProcesses();
      return processlist.Where(process => process.MainWindowHandle != IntPtr.Zero).Select(process =>
      {
        // System.Console.WriteLine("++");
        // System.Console.WriteLine(process.MainWindowTitle);
        // System.Console.WriteLine(process.ProcessName);
        // System.Console.WriteLine(process.MainWindowHandle);
        // System.Console.WriteLine("++");
        return new WindowRef(process.ProcessName, process.Id, process.MainWindowTitle, process.MainWindowHandle);
      }).ToArray();
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();
  }
}

class WindowRef
{
  public string ProcessName { get; }
  public int Id { get; }
  public string MainWindowTitle { get; }
  public IntPtr hWnd { get; }

  public WindowRef(string ProcessName, int Is, string MainWindowTitle, IntPtr hWnd)
  {
    this.ProcessName = ProcessName;
    this.Id = Id;
    this.MainWindowTitle = MainWindowTitle;
    this.hWnd = hWnd;
  }
}

class App
{
  public string NameRegexp { get; set; }
  public int TargetDesktop { get; set; }

  public App(string NameRegexp, int TargetDesktop)
  {
    this.NameRegexp = NameRegexp;
    this.TargetDesktop = TargetDesktop;
  }
}