using Microsoft.Win32;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;

namespace Snapmaw {
  class Program {

    private static readonly Random random = new Random();

    static void Main(string[] args) {
      if (args.Length > 0 && args[0] == "--register") {
        RegisterContextMenuAndURLScheme();
        Console.WriteLine("Context menu and URL scheme registered successfully.");
      } else if (args.Length > 0 && args[0] == "--unregister") {
        UnregisterContextMenuAndURLScheme();
        Console.WriteLine("Context menu and URL scheme unregistered successfully.");
      } else if (args.Length >= 2 && args[0] == "--get") {
        DownloadFile(System.Text.RegularExpressions.Regex.Replace(args[1], @"^snapmaw:", ""));
      } else {
        OpenTarget(args);
      }
    }

    private static string GetRandomString(List<string> list) {
      int index = random.Next(list.Count);
      return list[index];
    }

    private static string GenerateCode() {
      List<string> prefixes = [
        "",
        "Apex-",
        "Bandit-",
        "Corrupted-",
        "Daemon-",
        "Overriden-",
        "Rebel-",
      ];
      List<string> elements = [
        "",
        "Acid-",
        "Adhesive-",
        "Berserk-",
        "Fire-",
        "Frost-",
        "Plasma-",
        "Purgewater-",
        "Shock-",
      ];
      List<string> machines = [
        "Behemoth",
        "Bellowback",
        "Bilegut",
        "Bristleback",
        "Broadhead",
        "Burrower",
        "Charger",
        "Clamberjaw",
        "Clawstrider",
        "Corruptor",
        "Deathbringer",
        "Dreadwing",
        "Fanghorn",
        "Fireclaw",
        "Frostclaw",
        "Glinthawk",
        "Grazer",
        "Grimhorn",
        "Horus",
        "Lancehorn",
        "Leapslasher",
        "Longleg",
        "Plowhorn",
        "Ravager",
        "Redeye-Watcher",
        "Rockbreaker",
        "Sawtooth",
        "Scorcher",
        "Scrapper",
        "Scrounger",
        "Shell-Walker",
        "Shellsnapper",
        "Skydrifter",
        "Slitherfang",
        "Snapmaw",
        "Specter",
        "Specter-Prime",
        "Spikesnout",
        "Stalker",
        "Stingspawn",
        "Stormbird",
        "Strider",
        "Sunwing",
        "Tallneck",
        "Tideripper",
        "Trampler",
        "Tremortusk",
        "Watcher",
        "Waterwing",
        "Widemaw",
      ];

      string element = GetRandomString(elements);
      string machine = GetRandomString(machines);

      // Special cases
      if (machine == "Burrower") {
        if (element == "") {
          element = GetRandomString([ "", "Tracker-" ]);
        } else {
          machine = "Canister-Burrower";
        }
      } else if (machine == "Fireclaw" || machine == "Frostclaw") {
        element = "";
      }

      return $"{GetRandomString(prefixes)}{element}{machine}-Level-{random.Next(99) + 1}";
    }

    private static void RegisterContextMenuAndURLScheme() {
      string? appPath = Environment.ProcessPath;

      if (appPath == null) {
        Console.Error.WriteLine("Error: The application path could not be determined.");
        return;
      }

      // Register context menu for files
      using (var key = Registry.ClassesRoot.CreateSubKey(@"*\shell\CopyShareLinkSnapmaw")) {
        key.SetValue("", "Copy share link");
        key.SetValue("Icon", appPath);
        using (var commandKey = key.CreateSubKey("command")) {
          commandKey.SetValue("", $"\"{appPath}\" %1");
        }
      }

      // Register context menu for folders
      using (var key = Registry.ClassesRoot.CreateSubKey(@"Directory\shell\CopyShareLinkSnapmaw")) {
        key.SetValue("", "Copy share link");
        key.SetValue("Icon", appPath);
        using var commandKey = key.CreateSubKey("command");
        commandKey.SetValue("", $"\"{appPath}\" %1");
      }

      // Register URL scheme
      using (var key = Registry.ClassesRoot.CreateSubKey("snapmaw")) {
        key.SetValue("", "URL:Snapmaw Protocol");
        key.SetValue("URL Protocol", "");

        using (var defaultIcon = key.CreateSubKey("DefaultIcon")) {
          defaultIcon.SetValue("", "\"" + appPath + "\",1");
        }

        using var commandKey = key.CreateSubKey(@"shell\open\command");
        commandKey.SetValue("", "\"" + appPath + "\" --get \"%1\"");
      }
    }

    private static void UnregisterContextMenuAndURLScheme() {
      // Unregister context menu for files
      Registry.ClassesRoot.DeleteSubKeyTree(@"*\shell\CopyShareLinkSnapmaw", false);

      // Unregister context menu for folders
      Registry.ClassesRoot.DeleteSubKeyTree(@"Directory\shell\CopyShareLinkSnapmaw", false);

      // Unregister URL scheme
      Registry.ClassesRoot.DeleteSubKeyTree("snapmaw", false);
    }

    private static void OpenTarget(string[] args) {
      if (args.Length > 0) {
        Console.WriteLine($"Sending file(s)...");
        var process = new Process {
          StartInfo = new ProcessStartInfo {
            FileName = "croc",
            Arguments = $"send --code \"https://snapmaw.pages.dev/{GenerateCode()}\" {string.Join(" ", args.Select(arg => $"\"{arg}\""))}",
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
          }
        };

        AppDomain.CurrentDomain.ProcessExit += (sender, e) => {
          if (!process.HasExited) {
            process.Kill();
            process.WaitForExit(1000);
          }
        };

        process.ErrorDataReceived += new DataReceivedEventHandler(CrocOutputHandler);
        process.Start();
        process.BeginErrorReadLine();
        process.WaitForExit();
        Console.WriteLine("");
      } else {
        Console.WriteLine("No target specified.");
      }
    }

    private static string currentFileName = "";
    private static void CrocOutputHandler(object sendingProcess, DataReceivedEventArgs e) {
      if (!String.IsNullOrEmpty(e.Data)) {
        foreach (var line in e.Data.Split('\n')) {
          var text = line.Trim();
          if (String.IsNullOrEmpty(text) || text.Contains("(Y/n)")) continue;

          var progressMatch = System.Text.RegularExpressions.Regex.Match(
            text,
            @"([^\s]+)\s+(\d+)%.*\(([\d.]+/[\d.]+\s*[KMG]?B),\s*([\d.]+\s*[KMG]?B/s)\)"
          );
          if (progressMatch.Success) {
            var fileName = progressMatch.Groups[1].Value;
            var percent = progressMatch.Groups[2].Value;
            var progress = progressMatch.Groups[3].Value;
            var speed = progressMatch.Groups[4].Value;

            if (fileName != currentFileName) {
              Console.WriteLine();
              currentFileName = fileName;
            }

            int width = 30;
            int filled = (int)((float.Parse(percent) / 100) * width);
            string progressBar = "[" + new string('=', filled) + new string(' ', width - filled) + "]";

            string status = $"\r{fileName}: {progressBar} {percent}% {progress} ({speed})";
            Console.Write(status.PadRight(Console.BufferWidth - 1));
          } else if (!text.Contains("%")) {
            Console.WriteLine(text);
          }
        }
      }
    }

    private static void DownloadFile(string code) {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);

      string? folderPath = null;
      var thread = new Thread(() => {
        using var folderDialog = new FolderBrowserDialog();
        if (folderDialog.ShowDialog() == DialogResult.OK) {
          folderPath = folderDialog.SelectedPath;
        }
      });
      thread.SetApartmentState(ApartmentState.STA);
      thread.Start();
      thread.Join();

      if (folderPath != null) {
        Console.WriteLine($"Downloading file...");
        var process = new Process {
          StartInfo = new ProcessStartInfo {
            FileName = "croc",
            Arguments = $"--yes --out \"{folderPath}\" \"https://snapmaw.pages.dev/{code}\"",
            RedirectStandardError = true,
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true
          }
        };

        process.ErrorDataReceived += new DataReceivedEventHandler(CrocOutputHandler);
        process.Start();
        process.BeginErrorReadLine();
        process.StandardInput.WriteLine("n");
        process.WaitForExit();
        Console.WriteLine("");
      }
    }

  }
}
