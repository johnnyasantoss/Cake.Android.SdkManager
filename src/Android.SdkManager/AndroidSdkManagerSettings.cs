﻿using System;
using System.IO;
using Android.SdkManager.Enums;

namespace Android.SdkManager
{
    /// <summary>
    /// Android SDK Manager tool settings.
    /// </summary>
    public class AndroidSdkManagerSettings
    {
        /// <summary>
        /// Gets or sets the Android SDK root path.
        /// </summary>
        /// <remarks>
        /// Default: <code>$HOME/.android</code>
        /// </remarks>
        /// <value>The sdk root.</value>
        public string SdkRoot { get; set; }
            = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".android"
            );

        /// <summary>
        /// Gets or sets the release channel.
        /// </summary>
        /// <value>The channel.</value>
        public AndroidSdkChannel Channel { get; set; } = AndroidSdkChannel.Stable;

        /// <summary>
        /// Gets or sets a value indicating whether or not to include obsoleted packages.
        /// </summary>
        /// <value><c>true</c> if include obsoleted packages; otherwise, <c>false</c>.</value>
        public bool IncludeObsolete { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether HTTPS should be used.
        /// </summary>
        /// <value><c>true</c> if no HTTPS; otherwise, <c>false</c>.</value>
        public bool NoHttps { get; set; } = false;

        /// <summary>
        /// Gets or sets the type of the proxy to be used.
        /// </summary>
        /// <value>The type of the proxy.</value>
        public AndroidSdkManagerProxyType ProxyType { get; set; } = AndroidSdkManagerProxyType.None;

        /// <summary>
        /// Gets or sets the proxy host.
        /// </summary>
        /// <value>The proxy host.</value>
        public string ProxyHost { get; set; }

        /// <summary>
        /// Gets or sets the proxy port.
        /// </summary>
        /// <value>The proxy port.</value>
        public int ProxyPort { get; set; } = -1;

        /// <summary>
        /// Gets or sets a value indicating whether to skip the sdkmanager version check.
        /// By default, the sdkmanager version is checked before each invocation to ensure a new enough version is in use.
        /// </summary>
        /// <value><c>true</c> if skip version check; otherwise, <c>false</c>.</value>
        public bool SkipVersionCheck { get; set; } = false;
    }
}
