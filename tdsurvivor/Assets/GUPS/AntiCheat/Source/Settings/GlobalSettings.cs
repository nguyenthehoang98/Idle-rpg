// System
using System;
using System.Collections.Generic;

// Unity
using UnityEngine;

// GUPS - AntiCheat - Core
using GUPS.AntiCheat.Core.Hash;
using GUPS.AntiCheat.Core.Random;

// GUPS - AntiCheat
using GUPS.AntiCheat.Monitor.Android;

namespace GUPS.AntiCheat.Settings
{
    /// <summary>
    /// Project-wide configuration asset for the anti-cheat system. Loaded as a singleton from
    /// <see cref="RELATIVE_SETTINGS_PATH"/>.
    /// </summary>
    /// <example>
    /// Read the active settings asset at runtime:
    /// <code>
    /// using GUPS.AntiCheat.Settings;
    ///
    /// var settings = GlobalSettings.Instance;
    /// bool allowAnyOwner = settings.PlayerPreferences_Allow_Read_Any_Owner;
    /// </code>
    /// In the editor open <c>Project Settings &gt; GuardingPearSoftware &gt; AntiCheat</c> to edit the asset.
    /// </example>
    public class GlobalSettings : ScriptableObject
    {
        // Instance
        #region Instance

        /// <summary>
        /// Resource path (relative, no extension) used by <see cref="Resources.Load{T}(string)"/>.
        /// </summary>
        public const String RELATIVE_SETTINGS_PATH = "GUPS/AntiCheat/Settings/GlobalSettings";

        /// <summary>
        /// Absolute project path to the settings asset (without <c>.asset</c> extension).
        /// </summary>
        public const String SETTINGS_PATH = "Assets/GUPS/AntiCheat/Resources/" + RELATIVE_SETTINGS_PATH;

        /// <summary>
        /// Cached singleton instance, lazily loaded from <see cref="LoadAsset"/>.
        /// </summary>
        private static GlobalSettings instance;

        /// <summary>
        /// Gets the runtime singleton, loading it from resources on first access.
        /// </summary>
        public static GlobalSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = LoadAsset();
                }

                return instance;
            }
        }

        /// <summary>
        /// Loads the settings asset from resources without caching it on the singleton.
        /// </summary>
        /// <returns>The loaded asset, or <c>null</c> if it does not exist.</returns>
        public static GlobalSettings LoadAsset()
        {
            return Resources.Load<GlobalSettings>(RELATIVE_SETTINGS_PATH);
        }

        #endregion

        // Primitive
        #region Primitive

        /// <summary>
        /// Gets the shared random provider used to generate random values across the library.
        /// </summary>
        public static IRandomProvider RandomProvider { get; } = new PseudoRandom();

        #endregion

        // Player Preferences
        #region Player Preferences

        /// <summary>
        /// When <c>true</c>, player preferences are verified via a hash signature computed from data type, value and owner.
        /// </summary>
        [SerializeField]
        public bool PlayerPreferences_Verify_Integrity = false;

        /// <summary>
        /// When <c>true</c>, the player preference key is stored as a hash instead of its original name.
        /// </summary>
        [SerializeField]
        public bool PlayerPreferences_Hash_Key = false;

        /// <summary>
        /// Encryption key applied to player preference values. Empty means values are stored unencrypted.
        /// Changing the key invalidates previously written values.
        /// </summary>
        [SerializeField]
        public String PlayerPreferences_Value_Encryption_Key = String.Empty;

        /// <summary>
        /// When <c>true</c>, any user may read stored preferences; when <c>false</c>, only the original owner
        /// (identified by <see cref="UnityEngine.SystemInfo.deviceUniqueIdentifier"/>) can read them.
        /// </summary>
        [SerializeField]
        public bool PlayerPreferences_Allow_Read_Any_Owner = true;

        #endregion

        // Android
        #region Android

        /// <summary>
        /// When <c>true</c>, the Android app is validated (appstore, libraries, applications, signature, ...) also
        /// on development builds. Recommended: <c>false</c>.
        /// </summary>
        [SerializeField]
        public bool Android_Enable_Development = false;

        /// <summary>
        /// When <c>true</c>, any package installation source is accepted; otherwise only sources listed in
        /// <see cref="Android_AllowedAppStores"/> / <see cref="Android_AllowedCustomAppStores"/>.
        /// </summary>
        [SerializeField]
        public bool Android_AllowAllAppStores = true;

        /// <summary>
        /// Allowed well-known package installation sources. Installations from other sources raise a notification.
        /// </summary>
        [SerializeField]
        public List<EAppStore> Android_AllowedAppStores = new List<EAppStore>();

        /// <summary>
        /// Additional allowed installer package names (e.g. <c>com.android.vending</c>) for stores not covered by
        /// <see cref="Android_AllowedAppStores"/>.
        /// </summary>
        [SerializeField]
        public List<String> Android_AllowedCustomAppStores = new List<String>();

        /// <summary>
        /// When <c>true</c>, compare the running app's hash against a remote-hosted reference hash to detect
        /// tampering or repackaging.
        /// </summary>
        [SerializeField]
        public bool Android_VerifyAppHash = false;

        /// <summary>
        /// Hash algorithm used to compute and verify the app hash. Recommended: SHA-256.
        /// </summary>
        [SerializeField]
        public EHashAlgorithm Android_AppHashAlgorithm = EHashAlgorithm.SHA256;

        /// <summary>
        /// HTTP GET endpoint that returns the expected app hash. May contain the <c>{version}</c> placeholder,
        /// which is replaced with <see cref="Application.version"/>.
        /// </summary>
        [SerializeField]
        public String Android_AppHashEndpoint = "";

        /// <summary>
        /// When <c>true</c>, validate the app's signing fingerprint against <see cref="Android_AppFingerprint"/>.
        /// </summary>
        [SerializeField]
        public bool Android_VerifyAppFingerprint = false;

        /// <summary>
        /// Hash algorithm used to compute and validate the app fingerprint. Recommended: SHA-256.
        /// </summary>
        [SerializeField]
        public EHashAlgorithm Android_AppFingerprintAlgorithm = EHashAlgorithm.SHA256;

        /// <summary>
        /// Expected app fingerprint used to verify the app's identity.
        /// </summary>
        [SerializeField]
        public String Android_AppFingerprint = "";

        /// <summary>
        /// When <c>true</c>, enable whitelisting / blacklisting for loaded libraries; otherwise any library is allowed.
        /// </summary>
        [SerializeField]
        public bool Android_UseWhitelistingForLibraries = false;

        /// <summary>
        /// Libraries explicitly allowed inside the application; any library not in the list raises a notification.
        /// </summary>
        [SerializeField]
        public List<String> Android_WhitelistedLibraries = new List<String>();

        /// <summary>
        /// Libraries explicitly disallowed inside the application; any library in the list raises a notification.
        /// </summary>
        [SerializeField]
        public List<String> Android_BlacklistedLibraries = new List<String>();

        /// <summary>
        /// When <c>true</c>, the device is scanned for blacklisted apps (see <see cref="Android_BlacklistedApplications"/>).
        /// </summary>
        [SerializeField]
        public bool Android_UseBlacklistingforApplication = false;

        /// <summary>
        /// Installed Android applications considered hostile; presence raises a notification.
        /// </summary>
        [SerializeField]
        public List<String> Android_BlacklistedApplications = new List<String>();

        #endregion

        // iOS
        #region iOS

        /// <summary>
        /// When <c>true</c>, the iOS jailbreak detector also runs on development builds and inside the Unity editor.
        /// Recommended: <c>false</c>.
        /// </summary>
        [SerializeField]
        public bool IOS_Enable_Development = false;

        /// <summary>
        /// When <c>true</c>, probe the jailbreak URL schemes listed in <see cref="IOS_SuspiciousUrlSchemes"/>
        /// (cydia, sileo, zbra, filza, ...) via <c>UIApplication.canOpenURL</c>. Requires matching
        /// <c>LSApplicationQueriesSchemes</c> entries in <c>Info.plist</c> - the iOS build post-processor injects
        /// them automatically when this flag is on.
        /// </summary>
        [SerializeField]
        public bool IOS_DetectUrlSchemes = true;

        /// <summary>
        /// When <c>true</c>, scan the <see cref="IOS_SuspiciousPaths"/> list (Cydia.app, /private/var/lib/apt,
        /// /usr/sbin/sshd, /Library/MobileSubstrate, the rootless /var/jb prefix, ...) via <c>stat()</c>.
        /// </summary>
        [SerializeField]
        public bool IOS_DetectSuspiciousPaths = true;

        /// <summary>
        /// When <c>true</c>, attempt to create a file outside the app sandbox. Sandboxed iOS apps cannot write
        /// outside their container - a successful write indicates the sandbox is not enforced.
        /// </summary>
        [SerializeField]
        public bool IOS_DetectSandboxViolation = true;

        /// <summary>
        /// When <c>true</c>, probe whether <c>fork()</c> returns a positive child PID. Opt-in (default <c>false</c>)
        /// because static analysis from the App Store review pipeline can flag binaries that reference <c>fork</c>.
        /// </summary>
        [SerializeField]
        public bool IOS_DetectFork = false;

        /// <summary>
        /// When <c>true</c>, inspect the <c>DYLD_INSERT_LIBRARIES</c> and <c>DYLD_FORCE_FLAT_NAMESPACE</c>
        /// environment variables for runtime library injection. Entries matching one of the
        /// <see cref="IOS_DyldAllowedPrefixes"/> are considered benign (Xcode debug support libraries).
        /// </summary>
        [SerializeField]
        public bool IOS_DetectDyldInjection = true;

        /// <summary>
        /// When <c>true</c>, scan the loaded dyld images for the tweak framework substrings listed in
        /// <see cref="IOS_SuspiciousDylibs"/> (MobileSubstrate, CydiaSubstrate, libsubstitute, libhooker,
        /// FridaGadget, ...).
        /// </summary>
        [SerializeField]
        public bool IOS_DetectSuspiciousDylibs = true;

        /// <summary>
        /// URL schemes considered suspicious (blacklist), probed via <c>UIApplication.canOpenURL</c>. The iOS build
        /// post-processor injects these entries into <c>LSApplicationQueriesSchemes</c> so the probe works on iOS 9+.
        /// The list is passed to the native scanner; it contains no hidden built-in entries.
        /// </summary>
        [SerializeField]
        public List<String> IOS_SuspiciousUrlSchemes = new List<String>(new[]
        {
            "cydia",
            "sileo",
            "zbra",
            "filza",
            "undecimus-sileo",
            "activator",
        });

        /// <summary>
        /// Filesystem paths considered suspicious (blacklist), checked via <c>stat()</c>. Covers legacy
        /// /Applications/* jailbreaks, MobileSubstrate, SSH leftovers, apt/cydia state, and the rootless (/var/jb)
        /// layout used by Dopamine / palera1n. The list is passed to the native scanner; it contains no hidden
        /// built-in entries.
        /// </summary>
        [SerializeField]
        public List<String> IOS_SuspiciousPaths = new List<String>(new[]
        {
            "/Applications/Cydia.app",
            "/Applications/Sileo.app",
            "/Applications/Zebra.app",
            "/Applications/FakeCarrier.app",
            "/Applications/Icy.app",
            "/Applications/IntelliScreen.app",
            "/Applications/MxTube.app",
            "/Applications/RockApp.app",
            "/Applications/SBSettings.app",
            "/Applications/WinterBoard.app",
            "/Applications/blackra1n.app",
            "/Library/MobileSubstrate/MobileSubstrate.dylib",
            "/Library/MobileSubstrate/DynamicLibraries",
            "/usr/sbin/sshd",
            "/usr/bin/sshd",
            "/etc/apt",
            "/private/var/lib/apt",
            "/private/var/lib/cydia",
            "/private/var/stash",
            "/private/var/mobile/Library/SBSettings/Themes",
            "/private/var/tmp/cydia.log",
            "/bin/bash",
            "/bin/sh",
            "/usr/libexec/ssh-keysign",
            "/usr/libexec/sftp-server",
            "/var/cache/apt",
            "/var/lib/apt",
            "/var/lib/cydia",
            "/var/log/syslog",
            "/var/mobile/Media/.tweaks",
            "/var/jb",
            "/var/jb/Applications/Cydia.app",
            "/var/jb/Applications/Sileo.app",
            "/var/jb/usr/lib/TweakInject",
        });

        /// <summary>
        /// Dylib name substrings considered suspicious (blacklist). Compared case-insensitively against every loaded
        /// dyld image name. The list is passed to the native scanner; it contains no hidden built-in entries.
        /// </summary>
        [SerializeField]
        public List<String> IOS_SuspiciousDylibs = new List<String>(new[]
        {
            "MobileSubstrate",
            "SubstrateLoader.dylib",
            "CydiaSubstrate",
            "libsubstitute.dylib",
            "libsubstrate.dylib",
            "libhooker.dylib",
            "libellekit.dylib",
            "TweakInject",
            "TweakLoader",
            "RocketBootstrap",
            "frida",
            "FridaGadget",
            "cycript",
            "electra",
        });

        /// <summary>
        /// Trusted path prefixes (whitelist) for entries found in the <c>DYLD_INSERT_LIBRARIES</c> environment
        /// variable. Entries starting with one of these prefixes are considered benign - Xcode injects Apple debug
        /// support libraries (libViewDebuggerSupport, libMainThreadChecker, ...) from these read-only system
        /// locations when running with the debugger attached. Every entry not matching a prefix raises a dyld
        /// injection notification.
        /// </summary>
        [SerializeField]
        public List<String> IOS_DyldAllowedPrefixes = new List<String>(new[]
        {
            "/usr/lib/",
            "/Developer/",
        });

        #endregion

        // Desktop
        #region Desktop

        /// <summary>
        /// When <c>true</c>, desktop tampering detectors (mod loader, debugger, virtual environment) also run on
        /// development builds and inside the Unity editor. Recommended: <c>false</c>.
        /// </summary>
        [SerializeField]
        public bool Desktop_Enable_Development = false;

        #endregion

        // Desktop - Mod Loader
        #region Desktop - Mod Loader

        /// <summary>
        /// When <c>true</c>, scan for well-known mod loaders (BepInEx, MelonLoader, UnityDoorstop, ...) via
        /// <see cref="GUPS.AntiCheat.Detector.Desktop.ModLoaderDetector"/>.
        /// </summary>
        [SerializeField]
        public bool Desktop_DetectModLoader = true;

        /// <summary>
        /// Case-insensitive managed-assembly name prefixes attributed to mod loaders. Remove entries you ship
        /// legitimately (e.g. HarmonyLib).
        /// </summary>
        [SerializeField]
        public List<String> Desktop_ModLoader_AssemblyPrefixes = new List<String>(new[]
        {
            "BepInEx",
            "MelonLoader",
            "HarmonyLib",
            "0Harmony",
            "MonoMod",
            "UniverseLib",
            "UnityExplorer",
            "Doorstop",
            "Il2CppInterop",
        });

        /// <summary>
        /// File or folder names attributed to mod loaders when found next to the game executable or loaded as a
        /// native module. Folders end with '/'.
        /// </summary>
        [SerializeField]
        public List<String> Desktop_ModLoader_FileNames = new List<String>(new[]
        {
            "BepInEx/",
            "MelonLoader/",
            "Mods/",
            "doorstop_config.ini",
            "dobby.dll",
            "MelonLoader.dll",
            "BepInEx.Core.dll",
            "BepInEx.IL2CPP.dll",
            "doorstop.dll",
        });

        /// <summary>
        /// DLL names that are legitimate Windows system libraries inside System32 but indicate Unity Doorstop
        /// hijacking (the bootstrap used by BepInEx / MelonLoader) when loaded from anywhere else, e.g. the game
        /// directory. These are never matched by name alone - the probe verifies the loading path first, so the
        /// System32 originals never raise a notification.
        /// </summary>
        [SerializeField]
        public List<String> Desktop_ModLoader_HijackDlls = new List<String>(new[]
        {
            "winhttp.dll",
            "version.dll",
        });

        #endregion

        // Desktop - Debugger
        #region Desktop - Debugger

        /// <summary>
        /// When <c>true</c>, detect attached user-mode debuggers (Visual Studio, Rider, dnSpy, x64dbg, OllyDbg,
        /// ptrace, ...) via <see cref="GUPS.AntiCheat.Detector.Desktop.DebuggerDetector"/>.
        /// </summary>
        [SerializeField]
        public bool Desktop_DetectUserModeDebugger = true;

        /// <summary>
        /// When <c>true</c>, detect active kernel-mode debuggers (WinDbg kernel mode, Syser, SoftICE). Currently
        /// effective only on Windows.
        /// </summary>
        [SerializeField]
        public bool Desktop_DetectKernelModeDebugger = true;

        /// <summary>
        /// When <c>true</c>, raise a notification when only managed <c>System.Diagnostics.Debugger.IsAttached</c>
        /// fires without a matching native debugger.
        /// </summary>
        [SerializeField]
        public bool Desktop_Debugger_ReportManagedOnly = true;

        #endregion

        // Desktop - Virtual Environment
        #region Desktop - Virtual Environment

        /// <summary>
        /// When <c>true</c>, detect virtual-machine environments (VirtualBox, VMware, Parallels, Hyper-V, ...) via
        /// <see cref="GUPS.AntiCheat.Detector.Desktop.VirtualEnvironmentDetector"/>.
        /// </summary>
        [SerializeField]
        public bool Desktop_DetectVirtualMachine = true;

        /// <summary>
        /// MAC address OUI prefixes (6 hex digits, no separators) attributed to known VM vendors,
        /// e.g. <c>080027</c> for VirtualBox.
        /// </summary>
        [SerializeField]
        public List<String> Desktop_VirtualMachine_MacOuis = new List<String>(new[]
        {
            "080027",
            "0A0027",
            "525400",
            "000569",
            "000C29",
            "001C14",
            "005056",
            "001C42",
        });

        /// <summary>
        /// Substrings indicating a virtual machine when found in BIOS / DMI / hardware model strings.
        /// </summary>
        [SerializeField]
        public List<String> Desktop_VirtualMachine_BiosKeywords = new List<String>(new[]
        {
            "VirtualBox",
            "VBox",
            "innotek",
            "VMware",
            "QEMU",
            "Xen",
            "Parallels",
            "Hyper-V",
            "KVM",
            "Bochs",
            "Virtual Machine",
            "Microsoft Corporation Virtual Machine",
            "Google Compute Engine",
            "OpenStack",
        });

        /// <summary>
        /// Windows registry service keys (relative to HKEY_LOCAL_MACHINE) that only exist when virtual machine guest
        /// additions / guest tools are installed. Presence of any key raises a virtual machine notification. Only
        /// used by the Windows platform probe.
        /// </summary>
        [SerializeField]
        public List<String> Desktop_VirtualMachine_WindowsServiceKeys = new List<String>(new[]
        {
            @"SYSTEM\CurrentControlSet\Services\VBoxGuest",
            @"SYSTEM\CurrentControlSet\Services\VBoxMouse",
            @"SYSTEM\CurrentControlSet\Services\VBoxService",
            @"SYSTEM\CurrentControlSet\Services\VBoxSF",
            @"SYSTEM\CurrentControlSet\Services\vmtools",
            @"SYSTEM\CurrentControlSet\Services\vmmemctl",
            @"SYSTEM\CurrentControlSet\Services\vmhgfs",
            @"SYSTEM\CurrentControlSet\Services\prl_tools",
        });

        #endregion

        // Editor
        #region Editor

#if UNITY_EDITOR

        /// <summary>
        /// Creates the settings asset on disk (and any missing parent directories).
        /// </summary>
        /// <returns>The newly created asset.</returns>
        public static GlobalSettings CreateAsset()
        {
            if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(SETTINGS_PATH)))
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(SETTINGS_PATH));
            }

            GlobalSettings var_Asset = CreateInstance<GlobalSettings>();
            UnityEditor.AssetDatabase.CreateAsset(var_Asset, SETTINGS_PATH + ".asset");
            UnityEditor.AssetDatabase.SaveAssets();
            return var_Asset;
        }

        /// <summary>
        /// Loads the settings asset, creating it on disk if it does not yet exist.
        /// </summary>
        /// <returns>The loaded or newly created asset.</returns>
        public static GlobalSettings LoadOrCreateAsset()
        {
            GlobalSettings var_Asset = LoadAsset();
            if (var_Asset == null)
            {
                var_Asset = CreateAsset();
            }
            return var_Asset;
        }

        /// <summary>
        /// Returns the settings asset wrapped in a <see cref="UnityEditor.SerializedObject"/> for editor UIs.
        /// </summary>
        /// <returns>A serialized-object view of the asset.</returns>
        public static UnityEditor.SerializedObject GetSerializedAsset()
        {
            return new UnityEditor.SerializedObject(LoadOrCreateAsset());
        }

        /// <summary>
        /// Unloads the cached singleton from memory without writing changes to disk.
        /// </summary>
        public static void Unload()
        {
            if (GlobalSettings.instance != null)
            {
                Resources.UnloadAsset(GlobalSettings.instance);

                GlobalSettings.instance = null;
            }
        }

#endif

        #endregion
    }
}
