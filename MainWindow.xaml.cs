using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using WindowsDesktop;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace VirtualDesktopShowcase
{
  partial class MainWindow
  {
    private int COLUMNS = 12;
    private int ROWS = 6;

    private Position POSITION_FULLSCREEN = new Position(0, 0, 11, 5);
    private Position POSITION_LEFT = new Position(0, 0, 5, 5);
    private Position POSITION_RIGHT = new Position(6, 0, 11, 5);

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
      catch (Exception)
      {
        // MessageBox.Show(ex.Message, "Failed to initialize.");
      }

      VirtualDesktop.CurrentChanged += (sender, args) => System.Diagnostics.Debug.WriteLine($"Desktop changed: {args.NewDesktop.Id}");
    }

    private void Run(object sender, RoutedEventArgs e)
    {

      var allWindows = getAllWindows();
      var desktops = VirtualDesktop.GetDesktops();


      var screens = new List<VDSScreen>() {
        new VDSScreen(0, 1, 6, 12, new List<global::App>{
          new global::App("brave", 1, POSITION_FULLSCREEN),
          new global::App("code", 3, POSITION_RIGHT),
          new global::App("chrome", 3, POSITION_LEFT),
          new global::App("foobar2000", 2, POSITION_RIGHT),
          new global::App("spotify", 2, POSITION_LEFT),
          new global::App("thunderbird", 1, POSITION_FULLSCREEN),
        }),
          new VDSScreen(1, 1.5, 6, 12, new List<global::App>{
          new global::App("discord", 3, new Position(1, 0, 11, 2), true),
          new global::App("jamm", 3, new Position(9, 3, 11, 5), true),
          new global::App("telegram", 3, new Position(2, 3, 9, 5), true),
          new global::App("twist", 3, new Position(0, 0, 10, 2), true),
          new global::App("slack", 3, new Position(0, 3, 7, 5), true),
        }),
      };
      // TODO: create desktops if there aren't enough

      foreach (var screen in screens)
      {
        screen.Apps.ForEach(app =>
        {
          var regex = new Regex(app.NameRegexp.ToLower());
          var windows = (allWindows.Where(w => regex.IsMatch(w.ProcessName.ToLower()))).ToList();
          windows.ForEach(window =>
          {
            try
            {
              var desktop = desktops[app.TargetDesktop - 1];
              VirtualDesktopHelper.MoveToDesktop(window.hWnd, desktop);

              System.Drawing.Rectangle workingArea = Screen.AllScreens[screen.Index].WorkingArea;
              int DESKTOP_WIDTH = workingArea.Width;
              int DESKTOP_HEIGHT = workingArea.Height;

              if (app.Position == null) return;

              var WindowXPosition = (int)(workingArea.Left) + (DESKTOP_WIDTH / COLUMNS) * app.Position.X1;
              var WindowYPosition = (int)(workingArea.Top) + (DESKTOP_HEIGHT / ROWS) * app.Position.Y1;
              var windowWidth = (int)(DESKTOP_WIDTH / COLUMNS) * (app.Position.X2 - app.Position.X1 + 1);
              var windowHeight = (int)(DESKTOP_HEIGHT / ROWS) * (app.Position.Y2 - app.Position.Y1 + 1);

              MoveWindow(window.hWnd, WindowXPosition, WindowYPosition, windowWidth, windowHeight, true);

              if (app.Pin)
              {
                VirtualDesktop.PinWindow(window.hWnd);
              }
            }
            catch (System.Exception e)
            {
            // TODO: add debug logging
            if (!e.Message.Contains("Element not found") && !e.Message.Contains("The group or resource"))
              {
                System.Console.WriteLine("====================");
                System.Console.WriteLine(e.Message);
                System.Console.WriteLine("====================");
              }
            }
          });
        });
      }
    }

    private List<WindowRef> getAllWindows()
    {
      return Process.GetProcesses().SelectMany(process =>
      {
        return Util.GetRootWindowsOfProcess(process.Id)
          .Where(hWnd => hWnd != IntPtr.Zero)
          .Select(hWnd =>
        {
          return new WindowRef(process.ProcessName, process.Id, process.MainWindowTitle, hWnd);
        });
      }).ToList();
    }

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int Width, int Height, bool Repaint);
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
  public Position Position { get; set; }
  public bool Pin { get; set; }

  public App(string NameRegexp, int TargetDesktop, Position Position, bool pin = false)
  {
    this.NameRegexp = NameRegexp;
    this.TargetDesktop = TargetDesktop;
    this.Position = Position;
    Pin = pin;
  }

  public App(string NameRegexp, int TargetDesktop) : this(NameRegexp, TargetDesktop, null)
  {
  }
}

class Position
{
  public int X1 { get; set; }
  public int Y1 { get; set; }
  public int X2 { get; set; }
  public int Y2 { get; set; }

  public Position(int x1, int y1, int x2, int y2)
  {
    if (x2 < x1) throw new Exception($"x2 ({x2}) cannot be lesser than x1 ({x1}).");
    if (y2 < y1) throw new Exception($"y2 ({y2}) cannot be lesser than y1 ({y1}).");
    X1 = x1;
    Y1 = y1;
    X2 = x2;
    Y2 = y2;
  }
}

class VDSScreen
{
  public int Index { get; set; }
  public double HiDPIMultiplier { get; set; }
  public List<global::App> Apps { get; set; }

  public VDSScreen(int id, double hiDPIMultiplier, int rows, int columns, List<global::App> apps)
  {
    Index = id;
    HiDPIMultiplier = hiDPIMultiplier;
    Apps = apps;
  }
}