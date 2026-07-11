using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Capturing;
using FlaUI.UIA3;
using Microsoft.Win32;
using RS.SetupApp.Core;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace RS.SetupApp.AutomationTests;

[TestClass]
public sealed class SetupAppUiLifecycleTests
{
    private static readonly TimeSpan UiTimeout = TimeSpan.FromSeconds(30);

    [TestMethod]
    [Timeout(600_000)]
    public void GeneratedBundle_ShouldExerciseTheDisposableUiLifecycle()
    {
        Fixture? fixture = null;
        bool succeeded = false;
        try
        {
            fixture = Fixture.Create();
            using UIA3Automation automation = new();

            InstallThroughWizard(fixture, automation);
            RepairThroughProgressUi(fixture, automation);
            CancelV2UpdateAndVerifyRollback(fixture, automation);
            CompleteV2Update(fixture, automation);
            RecoverInterruptedJournalThroughRetryUi(fixture, automation);
            ConfirmAndCompleteUninstall(fixture, automation);

            fixture.AssertRemoved();
            succeeded = true;
        }
        finally
        {
            fixture?.Dispose(keepArtifacts: !succeeded || KeepArtifacts);
        }
    }

    private static void InstallThroughWizard(Fixture fixture, UIA3Automation automation)
    {
        using Application app = Application.Launch(fixture.V1SetupPath);
        Window window = WaitForWindow(app, automation);
        string welcomeScreenshot = Capture(window, fixture, "01-welcome");
        if (IsBlackScreenshot(welcomeScreenshot))
        {
            Assert.Inconclusive("SetupAppFlaUIInteractiveDesktopRequired: UIA is attached to a black/non-interactive desktop. Screenshot and fixture logs were retained.");
        }

        Click(window, "WelcomeContinue");
        window = WaitForWindow(app, automation);
        WaitForElement(window, "AcceptLicense").AsCheckBox().Toggle();
        Capture(window, fixture, "02-license");
        Click(window, "LicenseContinue");
        window = WaitForWindow(app, automation);
        WaitForElement(window, "InstallDirectory");
        Capture(window, fixture, "03-options");
        Click(window, "InstallOptionsReview");
        window = WaitForWindow(app, automation);
        WaitForElement(window, "ReviewInstall");
        Capture(window, fixture, "04-review");
        Click(window, "ReviewInstall");
        window = WaitForWindow(app, automation);
        WaitForElement(window, "ProgressTimeline");
        Capture(window, fixture, "05-progress");
        WaitForElement(window, "CompletionFinish");
        Capture(window, fixture, "06-completion");
        Click(window, "CompletionFinish");
        WaitForExit(app);

        Assert.IsTrue(File.Exists(Path.Combine(fixture.InstallRoot, "TestPayloadApp.exe")));
        Assert.IsTrue(File.Exists(Path.Combine(fixture.StateRoot, "installed-state.json")));
        Assert.IsTrue(File.Exists(Path.Combine(fixture.InstallRoot, SetupRuntimeDefaults.OwnershipMarkerFileName)));
    }

    private static void RepairThroughProgressUi(Fixture fixture, UIA3Automation automation)
    {
        using Application maintenanceApp = Application.Launch(fixture.V1SetupPath);
        Window maintenanceWindow = WaitForWindow(maintenanceApp, automation);
        WaitForElement(maintenanceWindow, "MaintenanceRepair");
        Capture(maintenanceWindow, fixture, "07-maintenance");
        maintenanceApp.Close();
        WaitForExit(maintenanceApp);

        using Application repairApp = LaunchWorker(fixture.V1SetupPath, "repair", fixture);
        Window repairWindow = WaitForWindow(repairApp, automation);
        WaitForElement(repairWindow, "ProgressTimeline");
        Capture(repairWindow, fixture, "08-repair-progress");
        WaitForElement(repairWindow, "CompletionFinish");
        Click(repairWindow, "CompletionFinish");
        WaitForExit(repairApp);
    }

    private static void CancelV2UpdateAndVerifyRollback(Fixture fixture, UIA3Automation automation)
    {
        byte[] v1Executable = File.ReadAllBytes(Path.Combine(fixture.InstallRoot, "TestPayloadApp.exe"));
        using Application updateApp = LaunchWorker(fixture.V2SetupPath, "update", fixture);
        Window updateWindow = WaitForWindow(updateApp, automation);
        WaitForElement(updateWindow, "ProgressCancel");
        Capture(updateWindow, fixture, "09-update-cancel");
        Click(updateWindow, "ProgressCancel");
        updateWindow = WaitForWindow(updateApp, automation);
        WaitForElement(updateWindow, "MaintenanceRepair");
        Capture(updateWindow, fixture, "10-update-rolled-back");
        CollectionAssert.AreEqual(v1Executable, File.ReadAllBytes(Path.Combine(fixture.InstallRoot, "TestPayloadApp.exe")));
        updateApp.Close();
        WaitForExit(updateApp);
    }

    private static void CompleteV2Update(Fixture fixture, UIA3Automation automation)
    {
        using Application updateApp = LaunchWorker(fixture.V2SetupPath, "update", fixture);
        Window updateWindow = WaitForWindow(updateApp, automation);
        WaitForElement(updateWindow, "ProgressTimeline");
        WaitForElement(updateWindow, "CompletionFinish");
        Capture(updateWindow, fixture, "11-update-complete");
        Click(updateWindow, "CompletionFinish");
        WaitForExit(updateApp);
        Assert.AreEqual("2.0.0", File.ReadAllText(Path.Combine(fixture.InstallRoot, "fixture-version.txt")));
    }

    private static void RecoverInterruptedJournalThroughRetryUi(Fixture fixture, UIA3Automation automation)
    {
        string sentinelPath = Path.Combine(fixture.ArtifactRoot, "interrupted-journal-sentinel.txt");
        File.WriteAllText(sentinelPath, "remove after retry");
        FileStream? lockHandle = new(sentinelPath, FileMode.Open, FileAccess.Read, FileShare.None);
        try
        {
            fixture.WriteInterruptedJournal(sentinelPath);
            using Application recoveryApp = LaunchWorker(fixture.V2SetupPath, "repair", fixture);
            Window recoveryWindow = WaitForWindow(recoveryApp, automation);
        WaitForElement(recoveryWindow, "RecoveryRetry");
        Capture(recoveryWindow, fixture, "12-recovery-retry");
            lockHandle.Dispose();
            lockHandle = null;
            Click(recoveryWindow, "RecoveryRetry");
            recoveryWindow = WaitForWindow(recoveryApp, automation);
            WaitForElement(recoveryWindow, "CompletionFinish");
            Click(recoveryWindow, "CompletionFinish");
            WaitForExit(recoveryApp);
        }
        finally
        {
            lockHandle?.Dispose();
        }

        Assert.IsFalse(File.Exists(sentinelPath));
    }

    private static void ConfirmAndCompleteUninstall(Fixture fixture, UIA3Automation automation)
    {
        using Application maintenanceApp = Application.Launch(fixture.V2SetupPath);
        Window maintenanceWindow = WaitForWindow(maintenanceApp, automation);
        WaitForElement(maintenanceWindow, "MaintenanceUninstall");
        Click(maintenanceWindow, "MaintenanceUninstall");
        maintenanceWindow = WaitForWindow(maintenanceApp, automation);
        WaitForElement(maintenanceWindow, "UninstallConfirm");
        Capture(maintenanceWindow, fixture, "13-uninstall-confirmation");
        maintenanceApp.Close();
        WaitForExit(maintenanceApp);

        using Application uninstallApp = LaunchWorker(fixture.V2SetupPath, "uninstall", fixture);
        Window uninstallWindow = WaitForWindow(uninstallApp, automation);
        WaitForElement(uninstallWindow, "ProgressTimeline");
        WaitForElement(uninstallWindow, "CompletionFinish");
        Capture(uninstallWindow, fixture, "14-uninstall-complete");
        Click(uninstallWindow, "CompletionFinish");
        WaitForExit(uninstallApp);
    }

    private static Application LaunchWorker(string setupPath, string mode, Fixture fixture)
    {
        ProcessStartInfo startInfo = new(setupPath)
        {
            UseShellExecute = false
        };
        startInfo.ArgumentList.Add("--worker");
        startInfo.ArgumentList.Add("--mode");
        startInfo.ArgumentList.Add(mode);
        startInfo.ArgumentList.Add("--scope");
        startInfo.ArgumentList.Add("user");
        startInfo.ArgumentList.Add("--install-dir");
        startInfo.ArgumentList.Add(fixture.InstallRoot);
        startInfo.ArgumentList.Add("--skip-launch");
        startInfo.ArgumentList.Add("--no-shortcuts");
        startInfo.ArgumentList.Add("--no-autostart");
        startInfo.ArgumentList.Add("--purge-data");
        return Application.Attach(Process.Start(startInfo) ?? throw new InvalidOperationException("Setup.exe did not start."));
    }

    private static Window WaitForWindow(Application app, UIA3Automation automation)
    {
        DateTime deadline = DateTime.UtcNow + UiTimeout;
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                Window? window = app.GetMainWindow(automation, TimeSpan.FromMilliseconds(250));
                if (window != null)
                {
                    return window;
                }
            }
            catch
            {
                // The WPF window has not been created yet.
            }

            Thread.Sleep(100);
        }

        throw new TimeoutException("The generated Setup.exe main window did not appear.");
    }

    private static AutomationElement WaitForElement(Window window, string automationId)
    {
        DateTime deadline = DateTime.UtcNow + UiTimeout;
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                AutomationElement? element = window.FindFirstDescendant(condition => condition.ByAutomationId(automationId));
                if (element != null && element.IsAvailable)
                {
                    return element;
                }
            }
            catch (COMException)
            {
                // WPF replaces the page content during its transition; retry against UIA once it settles.
            }

            Thread.Sleep(100);
        }

        throw new TimeoutException($"AutomationId '{automationId}' did not appear.");
    }

    private static void Click(Window window, string automationId)
    {
        WaitForElement(window, automationId).AsButton().Invoke();
        Thread.Sleep(250);
    }

    private static string Capture(Window window, Fixture fixture, string name)
    {
        string path = Path.Combine(fixture.ArtifactRoot, "screenshots", $"{name}.png");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        using var bitmap = window.Capture();
        bitmap.Save(path);
        return path;
    }

    private static bool IsBlackScreenshot(string path)
    {
        using var bitmap = new System.Drawing.Bitmap(path);
        int[] horizontal = [bitmap.Width / 4, bitmap.Width / 2, (bitmap.Width * 3) / 4];
        int[] vertical = [bitmap.Height / 4, bitmap.Height / 2, (bitmap.Height * 3) / 4];
        foreach (int x in horizontal)
        {
            foreach (int y in vertical)
            {
                System.Drawing.Color pixel = bitmap.GetPixel(x, y);
                if (pixel.R != 0 || pixel.G != 0 || pixel.B != 0)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static void WaitForExit(Application app)
    {
        using Process process = Process.GetProcessById(app.ProcessId);
        if (!process.WaitForExit((int)UiTimeout.TotalMilliseconds))
        {
            throw new TimeoutException("Generated Setup.exe did not exit.");
        }
    }

    private static bool KeepArtifacts => string.Equals(
        Environment.GetEnvironmentVariable("RS_SETUPAPP_KEEP_ARTIFACTS"),
        "1",
        StringComparison.Ordinal);

    private sealed class Fixture
    {
        private Fixture(JsonElement document)
        {
            ArtifactRoot = Read(document, "artifactRoot");
            ProductId = Read(document, "productId");
            InstallRoot = Read(document, "installRoot");
            StateRoot = Read(document, "stateRoot");
            RecoveryRoot = Read(document, "recoveryRoot");
            LogRoot = Read(document, "logRoot");
            DataRoot = Read(document, "dataRoot");
            RegistryPath = Read(document, "registryPath");
            V1SetupPath = Read(GetProperty(document, "v1"), "setupPath");
            V2SetupPath = Read(GetProperty(document, "v2"), "setupPath");
        }

        public string ArtifactRoot { get; }
        public string ProductId { get; }
        public string InstallRoot { get; }
        public string StateRoot { get; }
        public string RecoveryRoot { get; }
        public string LogRoot { get; }
        public string DataRoot { get; }
        public string RegistryPath { get; }
        public string V1SetupPath { get; }
        public string V2SetupPath { get; }

        public static Fixture Create()
        {
            string repositoryRoot = FindRepositoryRoot();
            string artifactRoot = Path.Combine(repositoryRoot, "artifacts", "setupapp-test", $"ui-{Guid.NewGuid():N}");
            ProcessStartInfo startInfo = new("powershell")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            startInfo.ArgumentList.Add("-NoProfile");
            startInfo.ArgumentList.Add("-ExecutionPolicy");
            startInfo.ArgumentList.Add("Bypass");
            startInfo.ArgumentList.Add("-File");
            startInfo.ArgumentList.Add(Path.Combine(repositoryRoot, "scripts", "Setup-SetupAppFixture.ps1"));
            startInfo.ArgumentList.Add("-EmitFixtureJson");
            startInfo.ArgumentList.Add("-RepositoryRoot");
            startInfo.ArgumentList.Add(repositoryRoot);
            startInfo.ArgumentList.Add("-ArtifactRoot");
            startInfo.ArgumentList.Add(artifactRoot);

            using Process process = Process.Start(startInfo) ?? throw new InvalidOperationException("Fixture builder did not start.");
            string standardOutput = process.StandardOutput.ReadToEnd();
            string standardError = process.StandardError.ReadToEnd();
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException($"Fixture builder failed: {standardError}");
            }

            string json = standardOutput.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Last();
            using JsonDocument document = JsonDocument.Parse(json);
            return new Fixture(document.RootElement);
        }

        public void WriteInterruptedJournal(string sentinelPath)
        {
            string operationId = Guid.NewGuid().ToString();
            string journalRoot = Path.Combine(RecoveryRoot, operationId.Replace("-", string.Empty, StringComparison.Ordinal));
            Directory.CreateDirectory(journalRoot);
            object journal = new
            {
                operationId,
                productId = ProductId,
                scope = "CurrentUser",
                mode = "Repair",
                installDirectory = InstallRoot,
                recoveryDirectory = journalRoot,
                phase = "Applying",
                startedAtUtc = DateTimeOffset.UtcNow,
                updatedAtUtc = DateTimeOffset.UtcNow,
                completedSteps = Array.Empty<string>(),
                recoveryErrors = Array.Empty<string>(),
                compensations = new[]
                {
                    new
                    {
                        id = Guid.NewGuid(),
                        kind = "DeleteFile",
                        target = sentinelPath,
                        applied = true,
                        reverted = false,
                        metadata = new Dictionary<string, string>()
                    }
                }
            };
            File.WriteAllText(Path.Combine(journalRoot, "transaction.json"), JsonSerializer.Serialize(journal));
        }

        public void AssertRemoved()
        {
            foreach (string path in new[] { InstallRoot, StateRoot, RecoveryRoot, DataRoot })
            {
                Assert.IsFalse(Directory.Exists(path), $"Generated fixture path was not removed: {path}");
            }

            Assert.IsNull(Registry.CurrentUser.OpenSubKey(RegistryPath["HKCU:\\".Length..]));
        }

        public void Dispose(bool keepArtifacts)
        {
            CopyLogs();
            foreach (string path in new[] { InstallRoot, StateRoot, RecoveryRoot, DataRoot, LogRoot })
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, recursive: true);
                }
            }

            Registry.CurrentUser.DeleteSubKeyTree(RegistryPath["HKCU:\\".Length..], throwOnMissingSubKey: false);
            if (!keepArtifacts && Directory.Exists(ArtifactRoot))
            {
                Directory.Delete(ArtifactRoot, recursive: true);
            }
        }

        private void CopyLogs()
        {
            if (!Directory.Exists(LogRoot))
            {
                return;
            }

            string destination = Path.Combine(ArtifactRoot, "logs", "ui");
            Directory.CreateDirectory(destination);
            foreach (string source in Directory.GetFiles(LogRoot, "*", SearchOption.AllDirectories))
            {
                File.Copy(source, Path.Combine(destination, Path.GetFileName(source)), overwrite: true);
            }
        }

        private static string Read(JsonElement element, string property) => GetProperty(element, property).GetString()
            ?? throw new InvalidOperationException($"Fixture JSON property '{property}' was empty.");

        private static JsonElement GetProperty(JsonElement element, string name)
        {
            foreach (JsonProperty property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return property.Value;
                }
            }

            throw new KeyNotFoundException($"Fixture JSON property '{name}' was not present.");
        }

        private static string FindRepositoryRoot()
        {
            DirectoryInfo? directory = new(AppContext.BaseDirectory);
            while (directory != null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "MultiVerseKit.sln")))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }

            throw new DirectoryNotFoundException("Unable to locate the repository root.");
        }
    }
}
