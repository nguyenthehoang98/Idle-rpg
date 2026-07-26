// System
using System;

namespace GUPS.AntiCheat.Monitor.Android
{
    /// <summary>
    /// Maps Android installer package names to <see cref="EAppStore"/> values.
    /// </summary>
    public static class AppStoreHelper
    {
        /// <summary>
        /// Maps an installer package name to its corresponding <see cref="EAppStore"/>.
        /// </summary>
        /// <param name="_Package">The installer package name reported by the OS.</param>
        /// <returns>The matching <see cref="EAppStore"/>, or <see cref="EAppStore.Unknown"/> if unrecognized.</returns>
        public static EAppStore GetStore(String _Package)
        {
            // Most stores publish a single, well-known installer package name.
            switch (_Package)
            {
                // API <= 22: com.android.packageinstaller
                case "com.android.packageinstaller":
                // API >= 23: com.google.android.packageinstaller
                case "com.google.android.packageinstaller":
                    return EAppStore.AndroidPackageInstaller;

                case "com.amazon.venezia":
                    return EAppStore.AmazonAppstore;

                case "cm.aptoide.pt":
                    return EAppStore.Aptoide;

                case "com.farsitel.bazaar":
                    return EAppStore.CafeBazaar;

                case "org.fdroid.fdroid":
                    return EAppStore.FDroid;

                case "com.android.vending":
                    return EAppStore.GooglePlayStore;

                case "com.huawei.appmarket":
                    return EAppStore.HuaweiAppGallery;

                case "ir.mservices.market":
                    return EAppStore.Myket;

                case "com.oppo.market":
                    return EAppStore.OppoAppMarket;

                case "com.sec.android.app.samsungapps":
                    return EAppStore.SamsungGalaxyStore;

                case "com.taptap":
                    return EAppStore.TapTap;

                case "com.bbk.appstore":
                    return EAppStore.VivoAppStore;

                case "com.xiaomi.market":
                    return EAppStore.XiaomiMiGetApps;

                case "com.xda.labs.play":
                    return EAppStore.XDALabs;
            }

            // Meta uses multiple installer packages (e.g. com.oculus.twilight, com.oculus.store).
            if (_Package.StartsWith("com.oculus."))
            {
                return EAppStore.MetaHorizonStore;
            }

            // If the store is not recognized, return Unknown.
            return EAppStore.Unknown;
        }
    }
}
