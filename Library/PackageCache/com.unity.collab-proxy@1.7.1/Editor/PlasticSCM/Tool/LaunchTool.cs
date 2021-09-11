using System;
using System.Diagnostics;
using System.IO;

using Codice.Client.BaseCommands.EventTracking;
using Codice.CM.Common;
using Codice.LogWrapper;
using Codice.Utils;
using PlasticGui;
using Unity.PlasticSCM.Editor.Views;

namespace Unity.PlasticSCM.Editor.Tool
{
    internal static class LaunchTool
    {
        internal static void OpenGUIForMode(WorkspaceInfo wkInfo, bool isGluonMode)
        {
            if (ShowDownloadPlasticExeWindow(isGluonMode))
                return;

            mLog.DebugFormat(
                "Opening GUI on wkPath '{0}'.",
                wkInfo.ClientPath);

            TrackFeatureUseEvent.For(
                Plastic.API.GetRepositorySpec(wkInfo),
                isGluonMode ?
                    TrackFeatureUseEvent.Features.LaunchGluonTool :
                    TrackFeatureUseEvent.Features.LaunchPlasticTool);

            if (isGluonMode)
            {
                Process gluonProcess = ExecuteGUI(
                    PlasticInstallPath.GetGluonExePath(),
                    string.Format(
                        ToolConstants.Gluon.GUI_WK_EXPLORER_ARG,
                        wkInfo.ClientPath),
                    ToolConstants.Gluon.GUI_COMMAND_FILE_ARG,
                    ToolConstants.Gluon.GUI_COMMAND_FILE,
                    mGluonProcessId);

                if (gluonProcess != null)
                    mGluonProcessId = gluonProcess.Id;

                return;
            }

            if (PlatformIdentifier.IsMac())
            {
                Process plasticProcess = ExecuteGUI(
                    PlasticInstallPath.GetPlasticExePath(),
                    string.Format(
                        ToolConstants.Plastic.GUI_MACOS_WK_EXPLORER_ARG,
                        wkInfo.ClientPath),
                    ToolConstants.Plastic.GUI_MACOS_COMMAND_FILE_ARG,
                    ToolConstants.Plastic.GUI_MACOS_COMMAND_FILE,
                    mPlasticProcessId);

                if (plasticProcess != null)
                    mPlasticProcessId = plasticProcess.Id;

                return;
            }

            ExecuteProcess(
                PlasticInstallPath.GetPlasticExePath(),
                string.Format(
                    ToolConstants.Plastic.GUI_WINDOWS_WK_ARG,
                    wkInfo.ClientPath));
        }

        internal static void OpenBranchExplorer(WorkspaceInfo wkInfo, bool isGluonMode)
        {
            if (ShowDownloadPlasticExeWindow(isGluonMode))
                return;

            mLog.DebugFormat(
                "Opening Branch Explorer on wkPath '{0}'.",
                wkInfo.ClientPath);

            TrackFeatureUseEvent.For(
                Plastic.API.GetRepositorySpec(wkInfo),
                TrackFeatureUseEvent.Features.LaunchBranchExplorer);

            if (PlatformIdentifier.IsMac())
            {
                Process plasticProcess = ExecuteGUI(
                    PlasticInstallPath.GetPlasticExePath(),
                    string.Format(
                        ToolConstants.Plastic.GUI_MACOS_BREX_ARG,
                        wkInfo.ClientPath),
                    ToolConstants.Plastic.GUI_MACOS_COMMAND_FILE_ARG,
                    ToolConstants.Plastic.GUI_MACOS_COMMAND_FILE,
                    mPlasticProcessId);

                if (plasticProcess != null)
                    mPlasticProcessId = plasticProcess.Id;

                return;
            }

            Process brexProcess = ExecuteWindowsGUI(
                PlasticInstallPath.GetPlasticExePath(),
                string.Format(
                    ToolConstants.Plastic.GUI_WINDOWS_BREX_ARG,
                    wkInfo.ClientPath),
                mBrexProcessId);

            if (brexProcess != null)
                mBrexProcessId = brexProcess.Id;
        }

        internal static void OpenChangesetDiffs(string fullChangesetSpec, bool isGluonMode)
        {
            if (ShowDownloadPlasticExeWindow(isGluonMode))
                return;

            mLog.DebugFormat(
                "Launching changeset diffs for '{0}'",
                fullChangesetSpec);

            string exePath = (isGluonMode) ?
                PlasticInstallPath.GetGluonExePath() :
                PlasticInstallPath.GetPlasticExePath();

            string changesetDiffArg = (isGluonMode) ?
                ToolConstants.Gluon.GUI_CHANGESET_DIFF_ARG :
                ToolConstants.Plastic.GUI_CHANGESET_DIFF_ARG;

            ExecuteProcess(exePath,
                string.Format(
                    changesetDiffArg, fullChangesetSpec));
        }

        internal static void OpenSelectedChangesetsDiffs(
            string srcFullChangesetSpec,
            string dstFullChangesetSpec,
            bool isGluonMode)
        {
            if (ShowDownloadPlasticExeWindow(isGluonMode))
                return;

            mLog.DebugFormat(
                "Launching selected changesets diffs for '{0}' and '{1}'",
                srcFullChangesetSpec,
                dstFullChangesetSpec);

            string exePath = (isGluonMode) ?
                PlasticInstallPath.GetGluonExePath() :
                PlasticInstallPath.GetPlasticExePath();

            string selectedChangesetsDiffArgs = (isGluonMode) ?
                ToolConstants.Gluon.GUI_SELECTED_CHANGESETS_DIFF_ARGS :
                ToolConstants.Plastic.GUI_SELECTED_CHANGESETS_DIFF_ARGS;

            ExecuteProcess(exePath,
                string.Format(
                    selectedChangesetsDiffArgs,
                    srcFullChangesetSpec,
                    dstFullChangesetSpec));
        }

        internal static void OpenBranchDiffs(string fullBranchSpec, bool isGluonMode)
        {
            if (ShowDownloadPlasticExeWindow(isGluonMode))
                return;

            mLog.DebugFormat(
                "Launching branch diffs for '{0}'",
                fullBranchSpec);

            string exePath = (isGluonMode) ?
                PlasticInstallPath.GetGluonExePath() :
                PlasticInstallPath.GetPlasticExePath();

            string branchDiffArg = (isGluonMode) ?
                ToolConstants.Gluon.GUI_BRANCH_DIFF_ARG :
                ToolConstants.Plastic.GUI_BRANCH_DIFF_ARG;

            ExecuteProcess(exePath,
                string.Format(
                    branchDiffArg, fullBranchSpec));
        }

        internal static void OpenWorkspaceConfiguration(WorkspaceInfo wkInfo, bool isGluonMode)
        {
            if (ShowDownloadPlasticExeWindow(isGluonMode))
                return;

            mLog.DebugFormat(
                "Opening Workspace Configuration on wkPath '{0}'.",
                wkInfo.ClientPath);

            TrackFeatureUseEvent.For(
                Plastic.API.GetRepositorySpec(wkInfo),
                TrackFeatureUseEvent.Features.LaunchPartialConfigure);

            Process gluonProcess = ExecuteGUI(
                PlasticInstallPath.GetGluonExePath(),
                string.Format(
                    ToolConstants.Gluon.GUI_WK_CONFIGURATION_ARG,
                    wkInfo.ClientPath),
                ToolConstants.Gluon.GUI_COMMAND_FILE_ARG,
                ToolConstants.Gluon.GUI_COMMAND_FILE,
                mGluonProcessId);

            if (gluonProcess == null)
                return;

            mGluonProcessId = gluonProcess.Id;
        }

        internal static void OpenMerge(string wkPath, bool isGluonMode)
        {
            if (ShowDownloadPlasticExeWindow(isGluonMode))
                return;

            mLog.DebugFormat(
                "Opening Merge on wkPath '{0}'.",
                wkPath);

            if (PlatformIdentifier.IsMac())
            {
                Process plasticProcess = ExecuteGUI(
                    PlasticInstallPath.GetPlasticExePath(),
                    string.Format(ToolConstants.Plastic.GUI_MACOS_MERGE_ARG, wkPath),
                    ToolConstants.Plastic.GUI_MACOS_COMMAND_FILE_ARG,
                    ToolConstants.Plastic.GUI_MACOS_COMMAND_FILE,
                    mPlasticProcessId);

                if (plasticProcess != null)
                    mPlasticProcessId = plasticProcess.Id;

                return;
            }

            ExecuteProcess(
                PlasticInstallPath.GetPlasticExePath(),
                string.Format(ToolConstants.Plastic.GUI_WINDOWS_MERGE_ARG, wkPath));
        }

        internal static bool ShowDownloadPlasticExeWindow(bool isGluonMode)
        {
            if (IsExeAvailable.ForMode(isGluonMode))
                return false;

            DownloadPlasticExeWindow.ShowWindow(isGluonMode);
            return true;
        }

        static Process ExecuteGUI(
            string program,
            string args,
            string commandFileArg,
            string commandFileName,
            int processId)
        {
            string commandFile = Path.Combine(
                Path.GetTempPath(), commandFileName);

            Process process = GetGUIProcess(program, processId);

            if (process == null)
            {
                mLog.DebugFormat("Executing {0} (new process).", program);

                return ExecuteProcess(
                    program, args + string.Format(commandFileArg, commandFile));
            }

            mLog.DebugFormat("Executing {0} (reuse process pid:{1}).", program, processId);

            using (StreamWriter writer = new StreamWriter(new FileStream(
                commandFile, FileMode.Append, FileAccess.Write, FileShare.Read)))
            {
                writer.WriteLine(args);
            }

            return process;
        }

        static Process ExecuteWindowsGUI(
            string program,
            string args,
            int processId)
        {
            Process process = GetGUIProcess(program, processId);

            if (process == null)
            {
                mLog.DebugFormat("Executing {0} (new process).", program);

                return ExecuteProcess(program, args);
            }

            mLog.DebugFormat("Not executing {0} (existing process pid:{1}).", program, processId);

            BringWindowToFront.ForWindowsProcess(process.Id);

            return process;
        }

        static Process ExecuteProcess(string program, string args)
        {
            mLog.DebugFormat("Execute process: '{0} {1}'", program, args);

            Process process = BuildProcess(program, args);

            try
            {
                process.Start();

                return process;
            }
            catch (Exception ex)
            {
                mLog.ErrorFormat("Couldn't execute the program {0}: {1}",
                    program, ex.Message);
                mLog.DebugFormat("Stack trace: {0}",
                    ex.StackTrace);

                return null;
            }
        }

        static Process BuildProcess(string program, string args)
        {
            Process result = new Process();
            result.StartInfo.FileName = program;
            result.StartInfo.Arguments = args;
            result.StartInfo.CreateNoWindow = false;
            return result;
        }

        static Process GetGUIProcess(string program, int processId)
        {
            if (processId == -1)
                return null;

            mLog.DebugFormat("Checking {0} process [pid:{1}].", program, processId);

            try
            {
                Process process = Process.GetProcessById(processId);

                if (process == null)
                    return null;

                return process.HasExited ? null : process;
            }
            catch
            {
                // process is not running
                return null;
            }
        }

        static int mPlasticProcessId = -1;
        static int mGluonProcessId = -1;
        static int mBrexProcessId = -1;

        static readonly ILog mLog = LogManager.GetLogger("LaunchTool");
    }
}