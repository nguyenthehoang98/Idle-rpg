// Microsoft
using System;
using System.Collections.Generic;

// Unity
using UnityEngine;
using UnityEditor;

// GUPS - AnitCheat
using GUPS.AntiCheat.Monitor.Android;
using GUPS.AntiCheat.Settings;

// GUPS - AnitCheat - Editor
using GUPS.AntiCheat.Editor.Helper;

namespace GUPS.AntiCheat.Editor.Window
{
    /// <summary>
    /// Hosts the AntiCheat configuration page under Unity's Project Settings (Global / Android / Desktop tabs).
    /// </summary>
    public static class GlobalSettingsWindow
    {
        /// <summary>
        /// Current scroll position of the settings pane.
        /// </summary>
        private static Vector2 scrollPosition;

        /// <summary>
        /// EditorPrefs key used to persist the selected tab between editor sessions.
        /// </summary>
        private const String SelectedTabPrefsKey = "GUPS.AntiCheat.GlobalSettingsWindow.SelectedTab";

        /// <summary>
        /// Tab labels rendered in the top toolbar. Order maps directly to the GUI switch indices.
        /// </summary>
        private static readonly String[] TabLabels = new[] { "Global", "Desktop", "Android", "iOS" };

        /// <summary>
        /// Index of the selected tab; -1 means "not yet loaded from EditorPrefs", otherwise [0, TabLabels.Length - 1].
        /// </summary>
        private static int selectedTab = -1;

        /// <summary>
        /// Builds the <see cref="SettingsProvider"/> shown in Project Settings under "GuardingPearSoftware/AntiCheat".
        /// </summary>
        /// <returns>The configured provider.</returns>
        [SettingsProvider]
        public static SettingsProvider CreateAntiCheatSettingsProvider()
        {
            // Create provider and initialize.
            SettingsProvider var_Provider = new SettingsProvider("Project/GuardingPearSoftware/AntiCheat", SettingsScope.Project);

            // Assign the name of the window.
            var_Provider.label = "AntiCheat";

            // Link to the Dashboard and Asset Store pages.
            var_Provider.titleBarGuiHandler = () =>
            {
                GUIStyle var_AssetStoreButtonStyle = new GUIStyle("button");
                var_AssetStoreButtonStyle.fontSize = 12;
                var_AssetStoreButtonStyle.fontStyle = FontStyle.Bold;

                GUILayout.BeginHorizontal();

                if (GUILayout.Button(new GUIContent("Dashboard", "Direct link to the Dashboard page."), var_AssetStoreButtonStyle, GUILayout.MaxWidth(85), GUILayout.MaxHeight(28)))
                {
                    Application.OpenURL("https://dashboard.guardingpearsoftware.com");
                }                

                if (GUILayout.Button(new GUIContent("Asset Store", "Direct link to the Asset Store page."), var_AssetStoreButtonStyle, GUILayout.MaxWidth(85), GUILayout.MaxHeight(28)))
                {
                    Application.OpenURL("https://assetstore.unity.com/packages/slug/300626");
                }

                GUILayout.EndHorizontal();
            };

            // Populate the search keywords to enable smart search filtering and label highlighting:
            var_Provider.keywords = new HashSet<string>(new[] { "Cheat", "Hack", "AntiCheat", "Protect", "Secure", "Global", "Android", "iOS", "Jailbreak", "Cydia", "Sileo", "Desktop", "Windows", "macOS", "Linux", "Mod Loader", "Debugger", "Virtual Machine" });

            // Register a callback that draws the GUI and handles the interaction with the underlying serialized object.
            var_Provider.guiHandler = GetGui;

            // Return the provider.
            return var_Provider;
        }

        /// <summary>
        /// Draws the settings GUI: toolbar, scroll view, and the tab body for the currently selected tab.
        /// </summary>
        /// <param name="_SearchContext">Project Settings search context; used to auto-switch tabs.</param>
        private static void GetGui(String _SearchContext)
        {
            // Get the serialized object for the global settings.
            SerializedObject var_GlobalSettingsObject = GlobalSettings.GetSerializedAsset();

            // Update the serialized object.
            var_GlobalSettingsObject.Update();

            // Lazy-load the persisted tab selection on the first GUI tick.
            if (selectedTab < 0 || selectedTab >= TabLabels.Length)
            {
                selectedTab = Mathf.Clamp(EditorPrefs.GetInt(SelectedTabPrefsKey, 0), 0, TabLabels.Length - 1);
            }

            // If the user typed something in the project settings search box, route them to the matching tab so the
            // hit is actually visible. Plain substring match against the tab label keywords.
            if (!String.IsNullOrEmpty(_SearchContext))
            {
                String var_Lower = _SearchContext.ToLowerInvariant();
                if (var_Lower.Contains("global") || var_Lower.Contains("playerpref") || var_Lower.Contains("player pref") || var_Lower.Contains("preference"))
                {
                    selectedTab = 0;
                }
                else if (var_Lower.Contains("desktop"))
                {
                    selectedTab = 1;
                }
                else if (var_Lower.Contains("android"))
                {
                    selectedTab = 2;
                }
                else if (var_Lower.Contains("ios") || var_Lower.Contains("jailbreak") || var_Lower.Contains("cydia") || var_Lower.Contains("sileo"))
                {
                    selectedTab = 3;
                }
            }

            // Display the gui content.
            EditorGUILayout.LabelField("Centralized configuration for global AntiCheat-Settings. The settings apply at runtime and in the editor.", EditorStyles.wordWrappedLabel);

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            // Toolbar - drawn outside the scroll view so it stays anchored at the top while content scrolls.
            int var_NewTab = GUILayout.Toolbar(selectedTab, TabLabels, GUILayout.Height(24));
            if (var_NewTab != selectedTab)
            {
                selectedTab = var_NewTab;
                EditorPrefs.SetInt(SelectedTabPrefsKey, selectedTab);
                scrollPosition = Vector2.zero;
            }

            EditorGUILayout.Space(5);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUIUtility.labelWidth = 200;

            switch (selectedTab)
            {
                case 0:
                    GetPlayerPrefsGui(var_GlobalSettingsObject);
                    break;

                case 1:
                    // Pro only
                    using (new EditorGUI.DisabledScope(true))
                    {
                        GetDesktopGeneralGui(var_GlobalSettingsObject);
                        GetDesktopModLoaderGui(var_GlobalSettingsObject);
                        GetDesktopDebuggerGui(var_GlobalSettingsObject);
                        GetDesktopVirtualMachineGui(var_GlobalSettingsObject);
                    }
                    break;

                case 2:
                    // Pro only
                    using (new EditorGUI.DisabledScope(true))
                    {
                        GetGeneralGui(var_GlobalSettingsObject);
                        GetAppStoreGui(var_GlobalSettingsObject);
                        GetAppHashGui(var_GlobalSettingsObject);
                        GetAppFingerprintGui(var_GlobalSettingsObject);
                        GetAppLibraryGui(var_GlobalSettingsObject);
                        GetDeviceAppGui(var_GlobalSettingsObject);
                    }
                    break;

                case 3:
                    // Pro only
                    using (new EditorGUI.DisabledScope(true))
                    {
                        GetIOSGeneralGui(var_GlobalSettingsObject);
                        GetIOSJailbreakGui(var_GlobalSettingsObject);
                    }
                    break;
            }

            EditorGUILayout.Space(5);

            EditorGUIUtility.labelWidth = 0;

            EditorGUILayout.EndScrollView();

            // Apply changes.
            if (var_GlobalSettingsObject.ApplyModifiedProperties())
            {
            }

            // Dispose the object at the end, after all changes are applied. Required to prevent memory leaks.
            var_GlobalSettingsObject.Dispose();
        }

        /// <summary>
        /// Draws the PlayerPrefs section (key hashing, value encryption, owner check, integrity).
        /// </summary>
        /// <param name="_GlobalSettingsObject">Serialized global settings.</param>
        private static void GetPlayerPrefsGui(SerializedObject _GlobalSettingsObject)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField(new GUIContent("PlayerPrefs - Settings"), EditorStyles.boldLabel);

            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox("In this section you can configure the security settings for your player preferences (PlayerPrefs)", MessageType.Info);

            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(_GlobalSettingsObject.FindProperty("PlayerPreferences_Hash_Key"), new GUIContent("Hash Key", "Check to hash the player preference key (default or file based). Uncheck to not hash the player preference key. When enabled, the key is stored as a hash name instead of its original name. Recommended: true."));

            EditorGUILayout.PropertyField(_GlobalSettingsObject.FindProperty("PlayerPreferences_Value_Encryption_Key"), new GUIContent("Value Encryption Key", "The key used to encrypt the player preference value (default or file based). If a key is not assigned (empty or null), the value will remain unencrypted. If you change the key, the already written values will not be readable anymore, keep that in mind! Recommended"));

            EditorGUILayout.PropertyField(_GlobalSettingsObject.FindProperty("PlayerPreferences_Allow_Read_Any_Owner"), new GUIContent("Allow Read Any Owner", "Check to permit anybody to read the stored player preference (default or file based). So player can share their player preference. Uncheck to only allow the owner (device based) who created the player preference to access it. By default, the owner is identified using the device's unique identifier from Unity, accessed via UnityEngine.SystemInfo.deviceUniqueIdentifier. This feature is useful for sharing player preferences between different users or restricting access to them. For example if a user copy and paste it between devices. Recommended: Optional."));

            EditorGUILayout.PropertyField(_GlobalSettingsObject.FindProperty("PlayerPreferences_Verify_Integrity"), new GUIContent("Verify Integrity", "Check to enable verification of the integrity of the player preferences (default or file based). Uncheck to not verify integrity. The integrity check relies on a hash that is calculated from the data type, value, and owner, and is stored in a signature beside the data. Recommended: Optional."));
        }

        /// <summary>
        /// Draws the Android general settings section.
        /// </summary>
        /// <param name="_GlobalSettingsObject">Serialized global settings.</param>
        private static void GetGeneralGui(SerializedObject _GlobalSettingsObject)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField(new GUIContent("Android - Settings"), EditorStyles.boldLabel);

            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(_GlobalSettingsObject.FindProperty("Android_Enable_Development"), new GUIContent("Verify development builds", "Check to validate (Appstore, Libraries, Applications, Signature, ...) the android app on development builds too. Uncheck to not validate the android app on development builds. Recommended: false."));
        }

        /// <summary>
        /// Draws the Android app store / installation source whitelist section.
        /// </summary>
        /// <param name="_GlobalSettingsObject">Serialized global settings.</param>
        private static void GetAppStoreGui(SerializedObject _GlobalSettingsObject)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField(new GUIContent("Android - App Store - Settings"), EditorStyles.boldLabel);

            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox("In this section you can configure which package installation sources are trusted for your Android app.", MessageType.Info);

            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(_GlobalSettingsObject.FindProperty("Android_AllowAllAppStores"), new GUIContent("Allow all installation sources", "Check to allow all package installation sources for your app. Uncheck to allow only the package installation sources in the list of allowed app stores."));

            if (_GlobalSettingsObject.FindProperty("Android_AllowAllAppStores").boolValue == false)
            {
                // Get the property for the whitelisted app stores.
                var var_AllowedAppStoresProperty = _GlobalSettingsObject.FindProperty("Android_AllowedAppStores");

                // Create a list of the current checked app stores.
                List<EAppStore> var_CheckedAppStores = new List<EAppStore>();

                for (int i = 0; i < var_AllowedAppStoresProperty.arraySize; i++)
                {
                    var var_AppStore = (EAppStore)var_AllowedAppStoresProperty.GetArrayElementAtIndex(i).enumValueIndex;
                    var_CheckedAppStores.Add(var_AppStore);
                }

                // Create a list of the new checked app stores.
                List<EAppStore> var_NewCheckedAppStores = new List<EAppStore>();

                // Draw the label for the whitelisted app stores.
                EditorGUILayout.LabelField(new GUIContent("Allow following sources:", "A list of allowed package installation sources for the application. If the app is installed from a source not in the list, you will get a notification. You can react to those notifications and decide what you want to do from there."));

                // Darken the background color for the app stores.
                EditorGUILayout.BeginVertical(StyleHelper.DarkBackground);

                // AndroidPackageInstaller
                if (EditorGUILayout.Toggle(new GUIContent("Android Package Installer", "Package Installer (com.android.packageinstaller or com.google.android.packageinstaller). The installation of apps outside of stores is done by a system app that is integrated into every Android device. This system app, known as the package installer, is responsible for installing applications that originate from apk files downloaded from various locations."), var_CheckedAppStores.Contains(EAppStore.AndroidPackageInstaller)))
                {
                    var_NewCheckedAppStores.Add(EAppStore.AndroidPackageInstaller);
                }

                // AmazonAppstore
                if (EditorGUILayout.Toggle(new GUIContent("Amazon Appstore", "Amazon's digital application distribution platform (com.amazon.venezia)."), var_CheckedAppStores.Contains(EAppStore.AmazonAppstore)))
                {
                    var_NewCheckedAppStores.Add(EAppStore.AmazonAppstore);
                }

                // Aptoide
                if (EditorGUILayout.Toggle(new GUIContent("Aptoide", "An open-source independent Android app store (cm.aptoide.pt)."), var_CheckedAppStores.Contains(EAppStore.Aptoide)))
                {
                    var_NewCheckedAppStores.Add(EAppStore.Aptoide);
                }

                // CafeBazaar
                if (EditorGUILayout.Toggle(new GUIContent("Cafe Bazaar", "An Iranian Android marketplace (com.farsitel.bazaar)."), var_CheckedAppStores.Contains(EAppStore.CafeBazaar)))
                {
                    var_NewCheckedAppStores.Add(EAppStore.CafeBazaar);
                }

                // FDroid
                if (EditorGUILayout.Toggle(new GUIContent("F-Droid", "An open-source software repository for Android (org.fdroid.fdroid)."), var_CheckedAppStores.Contains(EAppStore.FDroid)))
                {
                    var_NewCheckedAppStores.Add(EAppStore.FDroid);
                }

                // GooglePlayStore
                if (EditorGUILayout.Toggle(new GUIContent("Google Play Store", "Google's official app store (com.android.vending)."), var_CheckedAppStores.Contains(EAppStore.GooglePlayStore)))
                {
                    var_NewCheckedAppStores.Add(EAppStore.GooglePlayStore);
                }

                // HuaweiAppGallery
                if (EditorGUILayout.Toggle(new GUIContent("Huawei AppGallery", "Huawei's official app distribution platform (com.huawei.appmarket)."), var_CheckedAppStores.Contains(EAppStore.HuaweiAppGallery)))
                {
                    var_NewCheckedAppStores.Add(EAppStore.HuaweiAppGallery);
                }

                // Myket
                if (EditorGUILayout.Toggle(new GUIContent("Myket", "A popular Android app store (ir.mservices.market)."), var_CheckedAppStores.Contains(EAppStore.Myket)))
                {
                    var_NewCheckedAppStores.Add(EAppStore.Myket);
                }

                // OppoAppMarket
                if (EditorGUILayout.Toggle(new GUIContent("Oppo App Market", "Oppo's official app store (com.oppo.market)."), var_CheckedAppStores.Contains(EAppStore.OppoAppMarket)))
                {
                    var_NewCheckedAppStores.Add(EAppStore.OppoAppMarket);
                }

                // SamsungGalaxyStore
                if (EditorGUILayout.Toggle(new GUIContent("Samsung Galaxy Store", "Samsung's official app store (com.sec.android.app.samsungapps)."), var_CheckedAppStores.Contains(EAppStore.SamsungGalaxyStore)))
                {
                    var_NewCheckedAppStores.Add(EAppStore.SamsungGalaxyStore);
                }

                // TapTap
                if (EditorGUILayout.Toggle(new GUIContent("TapTap", "A Chinese app store for mobile games (com.taptap)."), var_CheckedAppStores.Contains(EAppStore.TapTap)))
                {
                    var_NewCheckedAppStores.Add(EAppStore.TapTap);
                }

                // VivoAppStore
                if (EditorGUILayout.Toggle(new GUIContent("Vivo App Store", "Vivo's official app distribution platform (com.bbk.appstore)."), var_CheckedAppStores.Contains(EAppStore.VivoAppStore)))
                {
                    var_NewCheckedAppStores.Add(EAppStore.VivoAppStore);
                }

                // XiaomiMiGetApps
                if (EditorGUILayout.Toggle(new GUIContent("Xiaomi Mi GetApps", "Xiaomi's official app store (com.xiaomi.market)."), var_CheckedAppStores.Contains(EAppStore.XiaomiMiGetApps)))
                {
                    var_NewCheckedAppStores.Add(EAppStore.XiaomiMiGetApps);
                }

                // XDALabs
                if (EditorGUILayout.Toggle(new GUIContent("XDA Labs", "A platform for mobile development projects (com.xda.labs.play)."), var_CheckedAppStores.Contains(EAppStore.XDALabs)))
                {
                    var_NewCheckedAppStores.Add(EAppStore.XDALabs);
                }

                // MetaHorizonStore
                if (EditorGUILayout.Toggle(new GUIContent("Meta Horizon Store", "A platform for virtual reality applications and games (com.oculus.*)."), var_CheckedAppStores.Contains(EAppStore.MetaHorizonStore)))
                {
                    var_NewCheckedAppStores.Add(EAppStore.MetaHorizonStore);
                }

                // Unknown
                if (EditorGUILayout.Toggle(new GUIContent("Unknown", "Unknown installation source. If it is neither of the above sources."), var_CheckedAppStores.Contains(EAppStore.Unknown)))
                {
                    var_NewCheckedAppStores.Add(EAppStore.Unknown);
                }

                // Add all or remove all.
                EditorGUILayout.BeginHorizontal();

                // A button to add all app stores.
                if (GUILayout.Button("Add All", EditorStyles.miniButtonLeft, GUILayout.Width(100), GUILayout.Height(20)))
                {
                    // Add all app stores.
                    var_NewCheckedAppStores.Clear();

                    foreach (EAppStore var_Store in Enum.GetValues(typeof(EAppStore)))
                    {
                        var_NewCheckedAppStores.Add(var_Store);
                    }
                }

                // A button to remove all app stores.
                if (GUILayout.Button("Remove All", EditorStyles.miniButtonRight, GUILayout.Width(100), GUILayout.Height(20)))
                {
                    // Clear all app stores.
                    var_NewCheckedAppStores.Clear();
                }

                // End the horizontal group.
                EditorGUILayout.EndHorizontal();

                // End the vertical group.
                EditorGUILayout.EndVertical();

                // Apply the new checked app stores to the property.
                var_AllowedAppStoresProperty.ClearArray();

                for (int i = 0; i < var_NewCheckedAppStores.Count; i++)
                {
                    var_AllowedAppStoresProperty.InsertArrayElementAtIndex(i);
                    var_AllowedAppStoresProperty.GetArrayElementAtIndex(i).enumValueIndex = (int)var_NewCheckedAppStores[i];
                }

                // Display the custom package installation sources.
                EditorGUILayout.PropertyField(_GlobalSettingsObject.FindProperty("Android_AllowedCustomAppStores"), new GUIContent("Allow custom sources", "A list of allowed custom package installation sources for the application, if the store you wish to allow installation from is not in the list of allowed app stores. Enter here the package names. \n\nFor example for GooglePlayStore it is com.android.vending."));
            }
        }

        /// <summary>
        /// Draws the Android app hash verification section.
        /// </summary>
        /// <param name="_GlobalSettingsObject">Serialized global settings.</param>
        private static void GetAppHashGui(SerializedObject _GlobalSettingsObject)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField(new GUIContent("Android - App Hash - Settings"), EditorStyles.boldLabel);

            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox("In this section you can configure validation of the Android app package hash against a trusted remote value.", MessageType.Info);

            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(_GlobalSettingsObject.FindProperty("Android_VerifyAppHash"), new GUIContent("Verify app hash", "Check to verify the hash of the app with a remote source. Uncheck to not verify the app hash. After you have built your app, AntiCheat calculates the hash of the enite app (apk / aab) and displays it in the log. Store this hash somewhere on a server in the web, but accessible to your app. When the app starts, it can download the hash from the server and compares it with the hash of the app. If the hashes do not match, the app is not the original app and you can react."));

            if (_GlobalSettingsObject.FindProperty("Android_VerifyAppHash").boolValue == true)
            {
                EditorGUILayout.PropertyField(_GlobalSettingsObject.FindProperty("Android_AppHashAlgorithm"), new GUIContent("Used hash algorithm", "The algorithm used to generate and validate the app hash. Recommend: SHA-256."));

                EditorGUILayout.PropertyField(_GlobalSettingsObject.FindProperty("Android_AppHashEndpoint"), new GUIContent("Remote hash location", "The server get endpoint to read the app hash from. The server should return the hash of the whole app (apk / aab) as string to verify the app's identity and ensure that it is not tampered with or shipped through an unauthorized source. The path can contain a placeholder '{version}' which will be replaced with the Application.version.\n\nFor example: https://yourserver.com/yourapp/hash/{version} or https://yourserver.com/yourapp/hash?version={version}.\n\nApplication.version returns the current version of the Application. To set the version number in Unity, go to Edit>Project Settings>Player. This is the same as PlayerSettings.bundleVersion."));
            }
        }

        /// <summary>
        /// Draws the Android app fingerprint / signature verification section.
        /// </summary>
        /// <param name="_GlobalSettingsObject">Serialized global settings.</param>
        private static void GetAppFingerprintGui(SerializedObject _GlobalSettingsObject)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField(new GUIContent("Android - App Fingerprint - Settings"), EditorStyles.boldLabel);

            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox("In this section you can configure validation of the Android signing fingerprint to detect repackaged or tampered builds.", MessageType.Info);

            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(_GlobalSettingsObject.FindProperty("Android_VerifyAppFingerprint"), new GUIContent("Verify app fingerprint", "Check to verify the app fingerprint. Uncheck to not check the app fingerprint. The fingerprint or signature of the app is a unique identifier. It is used to verify the app's identity and ensure that it is not tampered with. \n\nYou can get the fingerprint directly from the app or you can use for example the following command on your keystore to get the fingerprint: keytool -list -v -keystore yourapp.keystore -alias youralias."));

            if (_GlobalSettingsObject.FindProperty("Android_VerifyAppFingerprint").boolValue == true)
            {
                EditorGUILayout.PropertyField(_GlobalSettingsObject.FindProperty("Android_AppFingerprintAlgorithm"), new GUIContent("Used hash algorithm", "The algorithm used to generate and validate the app fingerprint. Recommend: SHA-256."));

                EditorGUILayout.PropertyField(_GlobalSettingsObject.FindProperty("Android_AppFingerprint"), new GUIContent("Fingerprint", "The actual app fingerprint used to verify the app's identity and ensure that it is not tampered with or shipped through an unauthorized source. Enter as hex-string."));
            }
        }

        /// <summary>
        /// Draws the Android app library whitelist / blacklist section.
        /// </summary>
        /// <param name="_GlobalSettingsObject">Serialized global settings.</param>
        private static void GetAppLibraryGui(SerializedObject _GlobalSettingsObject)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField(new GUIContent("Android - App Library - Settings"), EditorStyles.boldLabel);

            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox("In this section you can configure Android library validation to detect unexpected or known unwanted native libraries.", MessageType.Info);

            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(_GlobalSettingsObject.FindProperty("Android_UseWhitelistingForLibraries"), new GUIContent("White-/Blacklist libraries", "Check to use whitelisting and blacklisting for libraries. Uncheck to allow all libraries to be used in the app."));

            if (_GlobalSettingsObject.FindProperty("Android_UseWhitelistingForLibraries").boolValue == true)
            {
                EditorGUILayout.PropertyField(_GlobalSettingsObject.FindProperty("Android_WhitelistedLibraries"), new GUIContent("Whitelisted libraries", "A list of whitelisted libraries that are allowed to be used in the application. If the application uses a library that is not in the list, you will get a notification. You can react to those notifications and decide what you want to do from there. A very common modding process is to add libraries to the application, which contain cheats."));

                EditorGUILayout.PropertyField(_GlobalSettingsObject.FindProperty("Android_BlacklistedLibraries"), new GUIContent("Blacklisted libraries", "A list of blacklisted libraries that are not allowed to be used in the application. If the application uses a library that is in the list, you will get a notification. You can react to those notifications and decide what you want to do from there. A very common modding process is to add libraries to the application, which contain cheats."));
            }
        }

        /// <summary>
        /// Draws the Android device-app blacklist section.
        /// </summary>
        /// <param name="_GlobalSettingsObject">Serialized global settings.</param>
        private static void GetDeviceAppGui(SerializedObject _GlobalSettingsObject)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField(new GUIContent("Android - Device App - Settings"), EditorStyles.boldLabel);

            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox("In this section you can configure detection of blacklisted apps installed on the Android device.", MessageType.Info);

            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(_GlobalSettingsObject.FindProperty("Android_UseBlacklistingforApplication"), new GUIContent("Blacklist device apps", "Check to use blacklisting for apps on the device. Uncheck to allow all apps to be used on the device. If the user as an app on their device that is blacklisted, you will get a notification. You can react to those notifications and decide what you want to do from there."));

            if (_GlobalSettingsObject.FindProperty("Android_UseBlacklistingforApplication").boolValue == true)
            {
                EditorGUILayout.PropertyField(_GlobalSettingsObject.FindProperty("Android_BlacklistedApplications"), new GUIContent("Blacklisted apps", "A list of blacklisted applications that are not allowed to be used on the device. If the user as an app on their device that is blacklisted, you will get a notification. You can react to those notifications and decide what you want to do from there."));
            }
        }

        /// <summary>
        /// Draws the general iOS settings section.
        /// </summary>
        /// <param name="_GlobalSettingsObject">Serialized global settings.</param>
        private static void GetIOSGeneralGui(SerializedObject _GlobalSettingsObject)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField(new GUIContent("iOS - Settings"), EditorStyles.boldLabel);

            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(_GlobalSettingsObject.FindProperty("IOS_Enable_Development"), new GUIContent("Verify development builds", "Check to also run the iOS jailbreak detector on development builds and inside the Unity editor. Uncheck to skip it while you develop the game so it does not fire on the simulator / unsigned dev builds. Recommended: false."));
        }

        /// <summary>
        /// Draws the iOS jailbreak detection section.
        /// </summary>
        /// <param name="_GlobalSettingsObject">Serialized global settings.</param>
        private static void GetIOSJailbreakGui(SerializedObject _GlobalSettingsObject)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField(new GUIContent("iOS - Jailbreak - Settings"), EditorStyles.boldLabel);

            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox("In this section you can configure the native iOS jailbreak probes used to detect suspicious URL schemes, filesystem paths, sandbox escapes, injected libraries, and tweak dylibs.", MessageType.Info);

            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(_GlobalSettingsObject.FindProperty("IOS_DetectUrlSchemes"), new GUIContent("Detect URL schemes", "Check to probe the URL schemes in the list below (cydia, sileo, zbra, filza, ...) via canOpenURL. Requires matching entries under LSApplicationQueriesSchemes in Info.plist - the iOS build post-processor injects the list entries automatically when this flag is on. Recommended: true."));

            if (_GlobalSettingsObject.FindProperty("IOS_DetectUrlSchemes").boolValue == true)
            {
                EditorGUILayout.PropertyField(_GlobalSettingsObject.FindProperty("IOS_SuspiciousUrlSchemes"), new GUIContent("Suspicious URL schemes", "Blacklist of URL schemes probed via UIApplication.canOpenURL. A scheme the OS routes to a handler indicates the matching jailbreak app is installed. These entries are injected into LSApplicationQueriesSchemes in Info.plist at build time.\n\nExamples: 'cydia', 'sileo', 'zbra', 'filza'."));
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(_GlobalSettingsObject.FindProperty("IOS_DetectSuspiciousPaths"), new GUIContent("Detect suspicious paths", "Check to scan the path list below (Cydia.app, /private/var/lib/apt, /usr/sbin/sshd, /Library/MobileSubstrate, the rootless /var/jb prefix used by Dopamine / palera1n, ...) via stat(). Recommended: true."));

            if (_GlobalSettingsObject.FindProperty("IOS_DetectSuspiciousPaths").boolValue == true)
            {
                EditorGUILayout.PropertyField(_GlobalSettingsObject.FindProperty("IOS_SuspiciousPaths"), new GUIContent("Suspicious paths", "Blacklist of filesystem paths that should never exist on a stock, sandboxed iOS device. Each existing path raises a notification with the path as evidence.\n\nExamples: '/Applications/Cydia.app', '/var/jb', '/usr/sbin/sshd'."));
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(_GlobalSettingsObject.FindProperty("IOS_DetectSandboxViolation"), new GUIContent("Detect sandbox violation", "Check to attempt a write outside the app sandbox. Sandboxed iOS apps cannot write outside their container - a successful write indicates the sandbox is not enforced. The probe deletes the test file immediately. Recommended: true."));

            // The fork probe is intentionally locked off in the editor for now.
            // App Store review heuristics flag binaries that check for forks, and we want to ship the safe baseline
            // first. The toggle will be re-enabled in a future release once the feature had been gone through more
            // testing and auditing.
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(_GlobalSettingsObject.FindProperty("IOS_DetectFork"), new GUIContent("Detect fork() (coming soon)", "Disabled. The fork() probe is locked off in the managed layer for now and will be added in a future release. Some more tests are required."));
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(_GlobalSettingsObject.FindProperty("IOS_DetectDyldInjection"), new GUIContent("Detect dyld injection", "Check to inspect the DYLD_INSERT_LIBRARIES and DYLD_FORCE_FLAT_NAMESPACE environment variables. iOS strips these for sandboxed apps - a populated value indicates runtime library injection. Entries matching one of the allowed prefixes below are considered benign. Recommended: true."));

            if (_GlobalSettingsObject.FindProperty("IOS_DetectDyldInjection").boolValue == true)
            {
                EditorGUILayout.PropertyField(_GlobalSettingsObject.FindProperty("IOS_DyldAllowedPrefixes"), new GUIContent("Allowed library prefixes", "Whitelist of path prefixes for DYLD_INSERT_LIBRARIES entries that are considered benign. When running from Xcode with the debugger attached, Apple injects debug support libraries (libViewDebuggerSupport, libMainThreadChecker, ...) from the read-only system locations '/usr/lib/' and '/Developer/'. Only Apple can place files there on a non-jailbroken device, so trusting these prefixes is safe and avoids false positives during development. Every injected library not matching a prefix raises a notification."));
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(_GlobalSettingsObject.FindProperty("IOS_DetectSuspiciousDylibs"), new GUIContent("Detect suspicious dylibs", "Check to scan the loaded dyld images for the tweak framework substrings in the list below (MobileSubstrate, CydiaSubstrate, libsubstitute, libhooker, FridaGadget, ...). Recommended: true."));

            if (_GlobalSettingsObject.FindProperty("IOS_DetectSuspiciousDylibs").boolValue == true)
            {
                EditorGUILayout.PropertyField(_GlobalSettingsObject.FindProperty("IOS_SuspiciousDylibs"), new GUIContent("Suspicious dylibs", "Blacklist of dylib name substrings compared case-insensitively against every loaded dyld image. A match indicates an injected tweak / hooking framework.\n\nExamples: 'MobileSubstrate', 'libhooker', 'frida', 'cycript'."));
            }
        }

        /// <summary>
        /// Draws the general desktop settings section (Windows, macOS, Linux).
        /// </summary>
        /// <param name="_GlobalSettingsObject">Serialized global settings.</param>
        private static void GetDesktopGeneralGui(SerializedObject _GlobalSettingsObject)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField(new GUIContent("Desktop - Settings"), EditorStyles.boldLabel);

            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(_GlobalSettingsObject.FindProperty("Desktop_Enable_Development"), new GUIContent("Verify development builds", "Check to also run the desktop tampering detectors on development builds and inside the Unity editor. Uncheck to skip them while you develop the game so they do not fire when you attach a debugger / use the editor. Recommended: false."));
        }

        /// <summary>
        /// Draws the desktop mod-loader detection section (BepInEx, MelonLoader, UnityDoorstop, ...).
        /// </summary>
        /// <param name="_GlobalSettingsObject">Serialized global settings.</param>
        private static void GetDesktopModLoaderGui(SerializedObject _GlobalSettingsObject)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField(new GUIContent("Desktop - Mod Loader - Settings"), EditorStyles.boldLabel);

            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox("In this section you can configure detection of known desktop mod loaders by assembly names and files placed next to the game executable.", MessageType.Info);

            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(_GlobalSettingsObject.FindProperty("Desktop_DetectModLoader"), new GUIContent("Detect mod loaders", "Check to detect known mod loaders. Uncheck to disable mod loader detection entirely (the ModLoaderDetector component will not raise notifications even when attached). Recommended: true."));

            if (_GlobalSettingsObject.FindProperty("Desktop_DetectModLoader").boolValue == true)
            {
                EditorGUILayout.PropertyField(_GlobalSettingsObject.FindProperty("Desktop_ModLoader_AssemblyPrefixes"), new GUIContent("Assembly name prefixes", "Managed assembly name prefixes considered to belong to a mod loader. Compared case-insensitively against the simple name of every assembly loaded into the current AppDomain.\n\nIf you legitimately ship one of these (for example HarmonyLib in your own game tools), remove the entry."));

                EditorGUILayout.PropertyField(_GlobalSettingsObject.FindProperty("Desktop_ModLoader_FileNames"), new GUIContent("File / folder names", "File or folder names considered to belong to a mod loader when found next to the game executable or loaded as a native module. Folders must end with '/'.\n\nExamples: 'BepInEx/', 'MelonLoader/', 'doorstop_config.ini', 'MelonLoader.dll'."));

                EditorGUILayout.PropertyField(_GlobalSettingsObject.FindProperty("Desktop_ModLoader_HijackDlls"), new GUIContent("Hijack DLL names", "DLL names that are legitimate Windows system libraries inside System32 but indicate Unity Doorstop hijacking (BepInEx / MelonLoader bootstrap) when loaded from anywhere else, e.g. the game directory. The loading path is verified before a notification is raised, so the System32 originals never fire.\n\nDefaults: 'winhttp.dll', 'version.dll'. Only used on Windows."));
            }
        }

        /// <summary>
        /// Draws the desktop user-mode / kernel-mode debugger detection section.
        /// </summary>
        /// <param name="_GlobalSettingsObject">Serialized global settings.</param>
        private static void GetDesktopDebuggerGui(SerializedObject _GlobalSettingsObject)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField(new GUIContent("Desktop - Debugger - Settings"), EditorStyles.boldLabel);

            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox("In this section you can configure detection of attached user-mode, managed-only, and kernel-mode debuggers.", MessageType.Info);

            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(_GlobalSettingsObject.FindProperty("Desktop_DetectUserModeDebugger"), new GUIContent("Detect user-mode debugger", "Check to detect attached user-mode debuggers (Visual Studio, Rider, dnSpy, x64dbg, OllyDbg, ptrace on Linux/macOS, ...). Recommended: true."));

            EditorGUILayout.PropertyField(_GlobalSettingsObject.FindProperty("Desktop_DetectKernelModeDebugger"), new GUIContent("Detect kernel-mode debugger", "Check to detect active kernel-mode debuggers (WinDbg in kernel mode, Syser, SoftICE). Currently effective only on Windows. Recommended: true."));

            if (_GlobalSettingsObject.FindProperty("Desktop_DetectUserModeDebugger").boolValue == true)
            {
                EditorGUILayout.PropertyField(_GlobalSettingsObject.FindProperty("Desktop_Debugger_ReportManagedOnly"), new GUIContent("Report managed-only debugger", "Check to also raise a notification when only the managed System.Diagnostics.Debugger.IsAttached flag fires (without a matching native debugger). Disable in development if you frequently attach the IDE to your development build and the development verification flag is on. Recommended: true."));
            }
        }

        /// <summary>
        /// Draws the desktop virtual machine / virtual environment detection section.
        /// </summary>
        /// <param name="_GlobalSettingsObject">Serialized global settings.</param>
        private static void GetDesktopVirtualMachineGui(SerializedObject _GlobalSettingsObject)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField(new GUIContent("Desktop - Virtual Environment - Settings"), EditorStyles.boldLabel);

            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox("In this section you can configure detection of virtual machine or virtualized desktop environments using vendor MAC prefixes and BIOS keywords.", MessageType.Info);

            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(_GlobalSettingsObject.FindProperty("Desktop_DetectVirtualMachine"), new GUIContent("Detect virtual machine", "Check to detect virtual machine environments. Uncheck to disable VM detection entirely. Recommended: optional - many legitimate users run inside Hyper-V or WSL2."));

            if (_GlobalSettingsObject.FindProperty("Desktop_DetectVirtualMachine").boolValue == true)
            {
                EditorGUILayout.PropertyField(_GlobalSettingsObject.FindProperty("Desktop_VirtualMachine_MacOuis"), new GUIContent("MAC OUI prefixes", "MAC address OUI prefixes that belong to known virtual machine vendors. Format: 6 hex digits without separators.\n\nExamples: '080027' for VirtualBox, '000C29' for VMware, '001C42' for Parallels.\n\nThe Hyper-V OUI '00155D' is deliberately not in the defaults: it also appears on legitimate host adapters (Hyper-V host vSwitch, WSL2) and would always fire there."));

                EditorGUILayout.PropertyField(_GlobalSettingsObject.FindProperty("Desktop_VirtualMachine_BiosKeywords"), new GUIContent("BIOS keywords", "Substrings indicating a virtual machine when found in BIOS / DMI / hardware model strings. This is the full keyword list used by the platform probes - there are no additional built-in entries. An empty list disables the keyword check.\n\nExamples: 'VirtualBox', 'VMware', 'QEMU', 'Virtual Machine' (Hyper-V product name)."));

                EditorGUILayout.PropertyField(_GlobalSettingsObject.FindProperty("Desktop_VirtualMachine_WindowsServiceKeys"), new GUIContent("Windows service keys", "Windows registry service keys (relative to HKEY_LOCAL_MACHINE) that only exist when VM guest additions / guest tools are installed. Presence of any key raises a virtual machine notification. Only used on Windows.\n\nExamples: 'SYSTEM\\CurrentControlSet\\Services\\VBoxGuest' (VirtualBox), '...\\vmtools' (VMware), '...\\prl_tools' (Parallels)."));
            }
        }
    }
}