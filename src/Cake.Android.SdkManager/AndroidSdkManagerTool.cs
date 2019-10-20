﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Cake.Core;
using Cake.Core.IO;

namespace Cake.AndroidSdkManager
{
	internal class AndroidSdkManagerTool : ToolEx
	{
		const string ANDROID_SDKMANAGER_MINIMUM_VERSION_REQUIRED = "26.1.1";

        public AndroidSdkManagerTool(ICakeContext cakeContext)
            : base(cakeContext)
        {
            _cakeContext = cakeContext;
        }

        private ICakeContext _cakeContext;

		protected override string GetToolName()
		{
			return "sdkmanager";
		}

		protected override IEnumerable<string> GetToolExecutableNames()
		{
			return new List<string> {
				"sdkmanager",
				"sdkmanager.bat"
			};
		}

		protected override IEnumerable<FilePath> GetAlternativeToolPaths(AndroidSdkManagerToolSettings settings)
		{
			var results = new List<FilePath>();

            var ext = _cakeContext.Environment.Platform.Family == PlatformFamily.Windows ? ".bat" : "";
            var androidHome = settings.SdkRoot.MakeAbsolute(_cakeContext.Environment).FullPath;

            if (!System.IO.Directory.Exists(androidHome))
                androidHome = _cakeContext.Environment.GetEnvironmentVariable("ANDROID_HOME");

			if (!string.IsNullOrEmpty(androidHome) && System.IO.Directory.Exists(androidHome))
			{
				var exe = new DirectoryPath(androidHome).Combine("tools").Combine("bin").CombineWithFilePath("sdkmanager" + ext);
				results.Add(exe);
			}

			return results;
		}

		readonly Regex rxListDesc = new Regex("\\s+Description:\\s+(?<desc>.*?)$", RegexOptions.Compiled | RegexOptions.Singleline);
		readonly Regex rxListVers = new Regex("\\s+Version:\\s+(?<ver>.*?)$", RegexOptions.Compiled | RegexOptions.Singleline);
		readonly Regex rxListLoc = new Regex("\\s+Installed Location:\\s+(?<loc>.*?)$", RegexOptions.Compiled | RegexOptions.Singleline);

		public void CheckSdkManagerVersion (ref AndroidSdkManagerToolSettings settings)
		{
			if (settings == null)
				settings = new AndroidSdkManagerToolSettings();

			if (settings.SkipVersionCheck)
				return;

			var builder = new ProcessArgumentBuilder();
			builder.Append("--version");

			var pex = RunProcessEx(settings, builder);

			var exitCode = pex.Complete.Result;

			if (!pex.StandardOutput.Any(o => o.Trim().Equals(ANDROID_SDKMANAGER_MINIMUM_VERSION_REQUIRED, StringComparison.OrdinalIgnoreCase)))
				throw new NotSupportedException("Your sdkmanager is out of date.  Version " + ANDROID_SDKMANAGER_MINIMUM_VERSION_REQUIRED + " or later is required.");
		}

		public AndroidSdkManagerList List(AndroidSdkManagerToolSettings settings)
		{
			var result = new AndroidSdkManagerList();


			CheckSdkManagerVersion(settings);

			//adb devices -l
			var builder = new ProcessArgumentBuilder();

			builder.Append("--list --verbose");

			BuildStandardOptions(settings, builder);

			var p = RunProcess(settings, builder, new ProcessSettings
			{
				RedirectStandardOutput = true,
			});


			//var processField = p.GetType().GetField("_process", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.Instance);

			//var process = (System.Diagnostics.Process)processField.GetValue(p);
			//process.StartInfo.RedirectStandardInput = true;
			//process.StandardInput.WriteLine("y");

			p.WaitForExit();

			int section = 0;

			var path = string.Empty;
			var description = string.Empty;
			var version = string.Empty;
			var location = string.Empty;

			foreach (var line in p.GetStandardOutput())
			{
				if (line.StartsWith("------"))
					continue;

				if (line.ToLowerInvariant().Contains("installed packages:"))
				{
					section = 1;
					continue;
				}
				else if (line.ToLowerInvariant().Contains("available packages:"))
				{
					section = 2;
					continue;
				}
				else if (line.ToLowerInvariant().Contains("available updates:"))
				{
					section = 3;
					continue;
				}

				if (section >= 1 && section <= 2)
				{
					if (string.IsNullOrEmpty(path)) {

						// If we have spaces preceding the line, it's not a new item yet
						if (line.StartsWith(" "))
							continue;

						path = line.Trim();
						continue;
					}

					if (rxListDesc.IsMatch(line)) {
						description = rxListDesc.Match(line)?.Groups?["desc"]?.Value;
						continue;
					}

					if (rxListVers.IsMatch(line)) {
						version = rxListVers.Match(line)?.Groups?["ver"]?.Value;
						continue;
					}

					if (rxListLoc.IsMatch(line)) {
						location = rxListLoc.Match(line)?.Groups?["loc"]?.Value;
						continue;
					}

					// If we got here, we should have a good line of data
					if (section == 1)
					{
						result.InstalledPackages.Add(new InstalledAndroidSdkPackage
						{
							Path = path,
							Version = version,
							Description = description,
							Location = location
						});
					}
					else if (section == 2)
					{
						result.AvailablePackages.Add(new AndroidSdkPackage
						{
							Path = path,
							Version = version,
							Description = description
						});
					}

					path = null;
					description = null;
					version = null;
					location = null;
				}
			}

			return result;
		}


        public bool InstallOrUninstall(
            bool install,
            IEnumerable<string> packages,
            AndroidSdkManagerToolSettings settings
		)
        {
			CheckSdkManagerVersion(settings);

			//adb devices -l
			var builder = new ProcessArgumentBuilder();

			if (!install)
				builder.Append("--uninstall");

			foreach (var pkg in packages)
				builder.AppendQuoted(pkg);

			BuildStandardOptions(settings, builder);

			var pex = RunProcessEx(settings, builder);

			pex.StandardInput.WriteLine("y");

			pex.Complete.Wait();

            foreach (var line in pex.StandardOutput)
            {
                if (line.StartsWith("Info:", StringComparison.InvariantCultureIgnoreCase))
                    this._cakeContext.Log.Write(Core.Diagnostics.Verbosity.Diagnostic,
                        Core.Diagnostics.LogLevel.Information, line);
            }

			return true;
		}

        public bool AcceptLicenses(AndroidSdkManagerToolSettings settings)
        {

			CheckSdkManagerVersion(settings);

			//adb devices -l
			var builder = new ProcessArgumentBuilder();

			builder.Append("--licenses");

			BuildStandardOptions(settings, builder);

			var pex = RunProcessEx(settings, builder);

			while (!pex.Complete.IsCompleted)
			{
				System.Threading.Thread.Sleep(250);
				pex.StandardInput.WriteLine("y");
			}

			pex.Complete.Wait();

			System.Threading.Thread.Sleep(500);

            foreach (var line in pex.StandardOutput)
            {
                if (line.StartsWith("Info:", StringComparison.InvariantCultureIgnoreCase))
                    this._cakeContext.Log.Write(Core.Diagnostics.Verbosity.Diagnostic,
                        Core.Diagnostics.LogLevel.Information, line);
            }

			return true;
		}

        public bool UpdateAll(AndroidSdkManagerToolSettings settings)
        {

			//adb devices -l
			var builder = new ProcessArgumentBuilder();

			builder.Append("update");

			BuildStandardOptions(settings, builder);

			var pex = RunProcessEx(settings, builder);

			pex.StandardInput.WriteLine("y");

			pex.Complete.Wait();

            foreach (var line in pex.StandardOutput)
            {
                if (line.StartsWith("Info:", StringComparison.InvariantCultureIgnoreCase))
                    this._cakeContext.Log.Write(Core.Diagnostics.Verbosity.Diagnostic,
                        Core.Diagnostics.LogLevel.Information, line);
            }

			return true;
		}

		public IEnumerable<string> Help(AndroidSdkManagerToolSettings settings)
		{
			//adb devices -l
            var builder = new ProcessArgumentBuilder();

			var pex = RunProcessEx(settings, builder);

			pex.Complete.Wait();

			foreach (var line in pex.StandardOutput)
				yield return line;
		}

		void BuildStandardOptions(AndroidSdkManagerToolSettings settings, ProcessArgumentBuilder builder)
		{
			builder.Append("--verbose");

			if (settings.Channel != AndroidSdkChannel.Stable)
				builder.Append("--channel=" + (int)settings.Channel);

            if (settings.SdkRoot != null)
                builder.Append("--sdk_root=\"{0}\"", settings.SdkRoot.MakeAbsolute(_cakeContext.Environment));

			if (settings.IncludeObsolete)
				builder.Append("--include_obsolete");

			if (settings.NoHttps)
				builder.Append("--no_https");

			if (settings.ProxyType != AndroidSdkManagerProxyType.None)
			{
				builder.Append("--proxy={0}", settings.ProxyType.ToString().ToLower());

				if (!string.IsNullOrEmpty(settings.ProxyHost))
					builder.Append("--proxy_host=\"{0}\"", settings.ProxyHost);

				if (settings.ProxyPort > 0)
					builder.Append("--proxy_port=\"{0}\"", settings.ProxyPort);
			}
		}
	}
}
