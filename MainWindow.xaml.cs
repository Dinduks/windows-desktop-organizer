using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using WindowsDesktop;

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
      var position = new Position("Spotify", 5);

      var desktops = VirtualDesktop.GetDesktops();

      var windows = getAllWindows();
      var spotifyWindow = (windows.Where(w => w.ProcessName == position.NameRegexp)).First();
      var desktop = desktops[position.TargetDesktop];
      VirtualDesktopHelper.MoveToDesktop(spotifyWindow.hWnd, desktop);
    }

    private WindowRef[] getAllWindows()
    {

      Process[] processlist = Process.GetProcesses();
      return processlist.Select(process =>
      {
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

class Position
{
  public string NameRegexp { get; set; }
  public int TargetDesktop { get; set; }

  public Position(string NameRegexp, int TargetDesktop)
  {
    this.NameRegexp = NameRegexp;
    this.TargetDesktop = TargetDesktop;
  }
}