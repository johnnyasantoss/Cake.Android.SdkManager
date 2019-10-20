﻿using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cake.Core;
using Cake.Core.IO;
using Cake.Core.Tooling;

namespace Cake.AndroidSdkManager
{
    internal abstract class ToolEx : Tool<AndroidSdkManagerToolSettings>
    {
        private readonly ICakeContext _cakeContextContext;

        public ToolEx(ICakeContext cakeContextContext)
            : base(cakeContextContext.FileSystem, cakeContextContext.Environment, cakeContextContext.ProcessRunner, cakeContextContext.Tools)
        {
            _cakeContextContext = cakeContextContext;
        }

        protected ToolExProcess RunProcessEx(AndroidSdkManagerToolSettings settings, ProcessArgumentBuilder arguments)
        {
            // Should we customize the arguments?
            if (settings.ArgumentCustomization != null)
            {
                arguments = settings.ArgumentCustomization(arguments);
            }

            // Get the tool name.
            var toolName = GetToolName();

            // Get the tool path.
            var toolPath = GetToolPath(settings);
            if (toolPath == null || !_cakeContextContext.FileSystem.Exist(toolPath))
            {
                const string message = "{0}: Could not locate executable.";
                throw new CakeException(string.Format(CultureInfo.InvariantCulture, message, toolName));
            }

            // Get the working directory.
            var workingDirectory = GetWorkingDirectory(settings);
            if (workingDirectory == null)
            {
                const string message = "{0}: Could not resolve working directory.";
                throw new CakeException(string.Format(CultureInfo.InvariantCulture, message, toolName));
            }

            // Create the process start info.
            var info = new ProcessStartInfo(toolPath.MakeAbsolute(_cakeContextContext.Environment).FullPath)
            {
                Arguments = arguments.Render(),
                WorkingDirectory = workingDirectory.FullPath,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true
            };

            //// Add environment variables
            //ProcessHelper.SetEnvironmentVariable(info, "CAKE", "True");
            //ProcessHelper.SetEnvironmentVariable(info, "CAKE_VERSION", _cakeContext.Environment.Runtime.CakeVersion.ToString(3));
            //if (settings.EnvironmentVariables != null)
            //{
            //	foreach (var environmentVariable in settings.EnvironmentVariables)
            //	{
            //		ProcessHelper.SetEnvironmentVariable(info, environmentVariable.Key, environmentVariable.Value);
            //	}
            //}

            // Start and return the process.
            var process = Process.Start(info);

            if (process == null)
            {
                return null;
            }

            process.EnableRaisingEvents = true;

            var consoleOutputQueue = SubscribeStandardConsoleOutputQueue(process);
            var consoleErrorQueue = SubscribeStandardConsoleErrorQueue(process);

            var complete = Task.Run(() =>
            {
                process.WaitForExit();
                return process.ExitCode;
            });

            return new ToolExProcess
            {
                Complete = complete,
                StandardOutput = consoleOutputQueue,
                StandardError = consoleErrorQueue,
                StandardInput = process.StandardInput
            };
        }


        protected class ToolExProcess
        {
            public Task<int> Complete { get; set; }
            public ConcurrentQueue<string> StandardOutput { get; set; }
            public ConcurrentQueue<string> StandardError { get; set; }
            public StreamWriter StandardInput { get; set; }
        }


        private new FilePath GetToolPath(AndroidSdkManagerToolSettings settings)
        {
            return GetToolPathUsingToolService(settings);
        }

        private FilePath GetToolPathUsingToolService(AndroidSdkManagerToolSettings settings)
        {
            var ext = _cakeContextContext.Environment.Platform.Family == PlatformFamily.Windows ? ".bat" : "";

            FilePath toolPath = null;

            if (settings.SdkRoot != null && _cakeContextContext.FileSystem.Exist(settings.SdkRoot))
                toolPath = settings.SdkRoot?.Combine("tools")?.Combine("bin")?.CombineWithFilePath("sdkmanager" + ext);

            if (toolPath != null)
                return toolPath.MakeAbsolute(_cakeContextContext.Environment);

            toolPath = settings.ToolPath;
            if (toolPath != null)
                return toolPath.MakeAbsolute(_cakeContextContext.Environment);

            // Look for each possible executable name in various places.
            var toolExeNames = GetToolExecutableNames();
            foreach (var toolExeName in toolExeNames)
            {
                var result = _cakeContextContext.Tools.Resolve(toolExeName);
                if (result != null)
                {
                    return result;
                }
            }

            // Look through all the alternative directories for the tool.
            var alternativePaths = GetAlternativeToolPaths(settings) ?? Enumerable.Empty<FilePath>();
            foreach (var alternativePath in alternativePaths)
            {
                if (_cakeContextContext.FileSystem.Exist(alternativePath))
                {
                    return alternativePath.MakeAbsolute(_cakeContextContext.Environment);
                }
            }

            return null;
        }

        private static ConcurrentQueue<string> SubscribeStandardConsoleErrorQueue(Process process)
        {
            var consoleErrorQueue = new ConcurrentQueue<string>();
            process.ErrorDataReceived += (s, e) =>
            {
                if (e.Data != null)
                {
                    consoleErrorQueue.Enqueue(e.Data);
                }
            };
            process.BeginErrorReadLine();
            return consoleErrorQueue;
        }

        private static ConcurrentQueue<string> SubscribeStandardConsoleOutputQueue(Process process)
        {
            var consoleOutputQueue = new ConcurrentQueue<string>();
            process.OutputDataReceived += (s, e) =>
            {
                if (e.Data != null)
                {
                    consoleOutputQueue.Enqueue(e.Data);
                }
            };
            process.BeginOutputReadLine();
            return consoleOutputQueue;
        }
    }
}
