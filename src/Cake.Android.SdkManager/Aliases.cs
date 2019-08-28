using System;
using System.Linq;
using System.Collections.Generic;
using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.IO;

namespace Cake.AndroidSdkManager
{
	/// <summary>
	/// Android SDK related aliases.
	/// </summary>
	[CakeAliasCategory("Android SDK Manager")]
	public static class AndroidSdkManagerAliases
	{
		/// <summary>
		/// Installs all available updates in the Android SDK.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="settings">The settings.</param>
		[CakeMethodAlias]
		public static void AndroidSdkManagerUpdateAll(this ICakeContext context, AndroidSdkManagerToolSettings settings = null)
		{
			var runner = new AndroidSdkManagerTool(context);
			runner.UpdateAll(settings);
		}

		/// <summary>
		/// Installs the specified Android SDK packages.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="packages">The packages to install.</param>
		/// <param name="settings">The settings.</param>
		[CakeMethodAlias]
		public static void AndroidSdkManagerInstall(this ICakeContext context, IEnumerable<string> packages, AndroidSdkManagerToolSettings settings = null)
		{
			var runner = new AndroidSdkManagerTool(context);
			runner.InstallOrUninstall(true, packages, settings);
		}

		/// <summary>
		/// Uninstalls the specified Android SDK packages.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="packages">The packages to install.</param>
		/// <param name="settings">The settings.</param>
		[CakeMethodAlias]
		public static void AndroidSdkManagerUninstall(this ICakeContext context, IEnumerable<string> packages, AndroidSdkManagerToolSettings settings = null)
		{
			var runner = new AndroidSdkManagerTool(context);
			runner.InstallOrUninstall(false, packages, settings);
		}

		/// <summary>
		/// Gets a list of the Installed, Available and Updates for the Android SDK Manager.
		/// </summary>
		/// <returns>Installed, Available, and Available Updates for the Android SDK.</returns>
		/// <param name="context">The context.</param>
		/// <param name="settings">The settings.</param>
		[CakeMethodAlias]
		public static AndroidSdkManagerList AndroidSdkManagerList(this ICakeContext context, AndroidSdkManagerToolSettings settings = null)
		{
			var runner = new AndroidSdkManagerTool(context);
			return runner.List(settings);
		}

		/// <summary>
		/// Downloads the Android SDK Manager.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="destinationDirectory">Destination directory, or ./tools/androidsdk if none is specified.</param>
		/// <param name="specificVersion">Specific version, or latest available if none is specified.</param>
		[CakeMethodAlias]
		public static void AndroidSdkManagerDownload(this ICakeContext context, DirectoryPath destinationDirectory = null, Version specificVersion = null)
		{
			AndroidSdkDownloader.DownloadSdk(context, destinationDirectory, specificVersion);
		}

		/// <summary>
		/// Accepts all licenses which are not yet accepted.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="settings">The settings.</param>
		[CakeMethodAlias]
		public static void AcceptLicenses(this ICakeContext context, AndroidSdkManagerToolSettings settings = null)
		{
			var runner = new AndroidSdkManagerTool(context);
			runner.AcceptLicenses(settings);
		}
    }
}
