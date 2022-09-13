﻿using StableDiffusionGui.Io;
using StableDiffusionGui.Main;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Threading.Tasks;

namespace StableDiffusionGui.Os
{
    class ProcessManager
    {
        static List<SdGuiProcess> _subProcs = new List<SdGuiProcess>();
        public static List<SdGuiProcess> AllSubProcesses { get { return GetStartedSubProcesses(); } }
        public static List<SdGuiProcess> RunningSubProcesses { get { return GetRunningSubProcesses(); } }
        public static List<SdGuiProcess> ExitedSubProcesses { get { return GetExitedSubProcesses(); } }

        public static void RegisterProcess(SdGuiProcess p)
        {
            _subProcs.Add(p);
        }

        public static List<SdGuiProcess> GetRunningSubProcesses()
        {
            List<SdGuiProcess> running = new List<SdGuiProcess>();

            foreach (SdGuiProcess p in new List<SdGuiProcess>(_subProcs))
            {
                try
                {
                    if (!p.Process.HasExited)
                        running.Add(p);
                }
                catch { }
            }

            return running;
        }

        public static List<SdGuiProcess> GetExitedSubProcesses()
        {
            List<SdGuiProcess> running = new List<SdGuiProcess>();

            foreach (SdGuiProcess p in new List<SdGuiProcess>(_subProcs))
            {
                try
                {
                    if (p.Process.HasExited)
                        running.Add(p);
                }
                catch { }
            }

            return running;
        }

        public static List<SdGuiProcess> GetStartedSubProcesses()
        {
            List<SdGuiProcess> running = new List<SdGuiProcess>();

            foreach (SdGuiProcess p in new List<SdGuiProcess>(_subProcs))
            {
                try
                {
                    running.Add(p);
                }
                catch { }
            }

            return running;
        }

        public static void ClearExitedProcesses()
        {
            _subProcs = new List<SdGuiProcess>(_subProcs).Where(x => !x.Process.HasExited).ToList();
        }

        public static void Kill(List<SdGuiProcess> list)
        {
            if (list.Count < 1)
                return;

            Logger.Log($"ProcMan: Killing {list.Count} subprocesses ({string.Join(", ", list.Select(x => x.Process.StartInfo.FileName))})", true);

            foreach (SdGuiProcess np in list)
            {
                Process p = np.Process;

                Logger.Log($"ProcMan: Killing {p.StartInfo.FileName} ({np.Type})...", true);

                try
                {
                    OsUtils.KillProcessTree(p.Id);
                    Logger.Log($"ProcMan: Killed process tree for {p.StartInfo.FileName} {p.StartInfo.Arguments.Trunc(150)}", true);
                }
                catch (Exception e)
                {
                    Logger.Log($"ProcMan: Failed to kill process tree for {p.StartInfo.FileName} {p.StartInfo.Arguments.Trunc(150)}: {e.Message}", true);
                }
            }
        }

        public static void KillAll()
        {
            Kill(RunningSubProcesses);
        }

        public static void KillAi()
        {
            Kill(RunningSubProcesses.Where(x => x.Type == SdGuiProcess.ProcessType.Ai).ToList());
        }

        public static void KillHelpers()
        {
            Kill(RunningSubProcesses.Where(x => x.Type == SdGuiProcess.ProcessType.Helper).ToList());
        }

        public static void FindAndKillOrphans(string filter = "")
        {
            string dataPath = Paths.GetDataPath();
            List<string> procsWithCli = new List<string>();

            ManagementClass mgmtClass = new ManagementClass("Win32_Process");

            foreach (ManagementObject o in mgmtClass.GetInstances())
            {
                string exe = $"{o["ExecutablePath"]}";
                string cli = $"{o["CommandLine"]}";
                int pid = $"{o["ProcessId"]}".GetInt();

                if (string.IsNullOrWhiteSpace(exe) || string.IsNullOrWhiteSpace(cli))
                    continue;

                string procWithCli = $"{exe} {cli}";

                if (procWithCli.Contains(dataPath))
                {
                    if (!string.IsNullOrWhiteSpace(filter) && !procWithCli.Contains(filter))
                        continue;

                    try
                    {
                        Logger.Log($"Killing {procWithCli} (PID {pid})", true);
                        OsUtils.KillProcessTree(pid);
                        Logger.Log($"Killed successfully.", true);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Failed to kill process: {ex.Message}");
                    }
                }
            }
        }
    }
}