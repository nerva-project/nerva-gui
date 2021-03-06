using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using AngryWasp.Helpers;
using Nerva.Desktop.Config;

#if UNIX
using Mono.Unix.Native;
using Nerva.Desktop.Helpers.Native;
#endif

namespace Nerva.Desktop.Helpers
{
    public static class ProcessManager
    {
        private static string ExeNameToProcessName(string exe) => Path.GetFileNameWithoutExtension(exe);

        public static void Kill(string exe)
        {
            try
            {
                //Logger.LogDebug("PM.KIL", "Exe: " + exe);
                List<Process> processList = GetRunningByName(exe);

                if (processList.Count == 0)
                {
                    Logger.LogDebug("PM.KIL", $"No instances of {exe} to kill");
                    return;
                }

                foreach (Process process in processList)
                {
                    Logger.LogDebug("PM.KIL", $"Killing running instance of {exe} with id {process.Id}");
#if UNIX
                    UnixNative.Kill(process.Id, Signum.SIGABRT);
#else
                    process.Kill();
#endif
                    Logger.LogDebug("PM.KIL", $"Process {process.Id} killed");
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException("PM.KIL", ex, "Could not kill process", false);
            }
        }

        public static bool IsRunning(string exe, out Process process)
        {
            process = null;

            try
            {
                //Logger.LogDebug("PM.IR", "Exe: " + exe);
                List<Process> processList = GetRunningByName(exe);

                //Logger.LogDebug("PM.IR", "Process count: " + processList.Count);
                if (processList.Count == 0)
                {
                    return false;
                }

                process = processList[0];

                if (process == null || process.HasExited)
                {
                    Logger.LogDebug("PM.IR", $"CLI tool {exe} exited unexpectedly. Restarting");
                    process = null;
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException("PM.IR", ex, false);
                return false;
            }
        }

        public static List<Process> GetRunningByName(string exe)
        {             
            List<Process> processList = new  List<Process>();

            try
            {                 
                string processName = ExeNameToProcessName(exe);
                //Logger.LogDebug("PM.GEBN", "Exe: " + exe + " | Process Name: " + processName);

                IList<Process> runningProcesses = Process.GetProcesses();

                foreach(Process process in runningProcesses)
                {
                    try
                    {
                        // macOS seems to be limited to 15 chars for process.ProcessName so use process.MainModule.ModuleName instead:
                        // Found nerva: nerva-wallet-rp | ID: 949 | MWT:  | MMFN: /Users/devmac/.nerva-gui/cli/nerva-wallet-rpc | MMMN: nerva-wallet-rpc

                        //if(process.ProcessName.Contains("nerva"))
                        //{
                        //    Logger.LogDebug("PM.GEBN", "Found nerva: " + process.ProcessName + " | ID: " + process.Id + " | MWT: " + process.MainWindowTitle + " | MMFN: " + process.MainModule.FileName + " | MMMN: " + process.MainModule.ModuleName);
                        //}

                        if(process.ProcessName.Contains(processName.Length > 13 ? processName.Substring(0, 12) : processName))
                        {
                            // We're looking at all processes and some will not have MainModule so we need above check first
                            if(process.MainModule.ModuleName.Contains(processName))
                            {
                                //Logger.LogDebug("PM.GEBN", "Found process: " + process.ProcessName + " | ID: " + process.Id + " | MWT: " + process.MainWindowTitle + " | MMFN: " + process.MainModule.FileName + " | MMMN: " + process.MainModule.ModuleName);
                                processList.Add(process);
                            }
                        }
                    }
                    catch (Exception ex1)
                    {
                        ErrorHandler.HandleException("PM.GRBN", ex1, false);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleException("PM.GRBN", ex, false);
            }
            return processList;
        }

        public static void StartExternalProcess(string exePath, string args)
        {
            Logger.LogDebug("PM.SEP", $"Starting process {ExeNameToProcessName(exePath)} {args}");

            Process proc = Process.Start(new ProcessStartInfo(exePath, args)
            {
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            });
        }

        public static string CycleLogFile(string path)
        {
            string logFile = path + ".log";
            string oldLogFile = logFile + ".old";

            try
            {
                if (File.Exists(oldLogFile))
                {
                    File.Delete(oldLogFile);
                }

                if (File.Exists(logFile))
                {
                    File.Move(logFile, oldLogFile);
                }
            }
            catch (Exception)
            {
                logFile = FileHelper.RenameDuplicateFile(logFile);
                Logger.LogError("PM.CLF", $"Cannot cycle log file. New log will be written to {logFile}");
                return logFile;
            }

            return logFile;
        }

        public static string GenerateCommandLine(string exePath, RpcDetails d)
        {
            string arg = $"--log-file \"{CycleLogFile(exePath)}\"";

            if (Configuration.Instance.Testnet)
            {
                Logger.LogDebug("PM.GCL", "Connecting to testnet");
                arg += " --testnet";
            }

            arg += $" --rpc-bind-port {d.Port}";
            arg += " --log-level " + d.LogLevel;

            return arg;
        }
    }
}