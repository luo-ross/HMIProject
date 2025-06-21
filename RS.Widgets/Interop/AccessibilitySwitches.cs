using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace RS.Widgets.Interop
{
    public static class AccessibilitySwitches
    {
        private const int EventId = 1023;

        private const string EventSource = ".NET Runtime";

        private static int s_DefaultsSet = 0;

        private static int s_SwitchesVerified = 0;

        private static string[] AccessibilityPropertyNames = new string[5] { "UseNetFx47CompatibleAccessibilityFeatures", "UseNetFx471CompatibleAccessibilityFeatures", "UseNetFx472CompatibleAccessibilityFeatures", "UseNetFx48CompatibleAccessibilityFeatures", "UseNetFx48aCompatibleAccessibilityFeatures" };

        public const string UseLegacyAccessibilityFeaturesSwitchName = "Switch.UseLegacyAccessibilityFeatures";


        public const string UseLegacyAccessibilityFeatures2SwitchName = "Switch.UseLegacyAccessibilityFeatures.2";


        public const string UseLegacyAccessibilityFeatures3SwitchName = "Switch.UseLegacyAccessibilityFeatures.3";


        public const string UseLegacyToolTipDisplaySwitchName = "Switch.UseLegacyToolTipDisplay";


        public const string ItemsControlDoesNotSupportAutomationSwitchName = "Switch.System.Windows.Controls.ItemsControlDoesNotSupportAutomation";


        public const string UseLegacyAccessibilityFeatures4SwitchName = "Switch.UseLegacyAccessibilityFeatures.4";


        public const string UseLegacyAccessibilityFeatures5SwitchName = "Switch.UseLegacyAccessibilityFeatures.5";


        public const string OptOutOfGridColumnResizeUsingKeyboardSwitchName = "Switch.System.Windows.Controls.OptOutOfGridColumnResizeUsingKeyboard";


        public static bool UseNetFx47CompatibleAccessibilityFeatures
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return System.AppContext.TryGetSwitch("Switch.UseLegacyAccessibilityFeatures", out bool _useLegacyAccessibilityFeatures);
            }
        }

        public static bool UseNetFx471CompatibleAccessibilityFeatures
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return System.AppContext.TryGetSwitch("Switch.UseLegacyAccessibilityFeatures.2", out bool _useLegacyAccessibilityFeatures2);
            }
        }

        public static bool UseNetFx472CompatibleAccessibilityFeatures
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return System.AppContext.TryGetSwitch("Switch.UseLegacyAccessibilityFeatures.3", out bool _useLegacyAccessibilityFeatures3);
            }
        }

        public static bool UseLegacyToolTipDisplay
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return System.AppContext.TryGetSwitch("Switch.UseLegacyToolTipDisplay", out bool _UseLegacyToolTipDisplay);
            }
        }

        public static bool ItemsControlDoesNotSupportAutomation
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return System.AppContext.TryGetSwitch("Switch.System.Windows.Controls.ItemsControlDoesNotSupportAutomation", out bool _ItemsControlDoesNotSupportAutomation);
            }
        }

        public static bool UseNetFx48CompatibleAccessibilityFeatures
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return System.AppContext.TryGetSwitch("Switch.UseLegacyAccessibilityFeatures.4", out bool _useLegacyAccessibilityFeatures4);
            }
        }

        public static bool UseNetFx48aCompatibleAccessibilityFeatures
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return System.AppContext.TryGetSwitch("Switch.UseLegacyAccessibilityFeatures.5", out bool _useLegacyAccessibilityFeatures5);
            }
        }

        public static bool OptOutOfGridColumnResizeUsingKeyboard
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return System.AppContext.TryGetSwitch("Switch.System.Windows.Controls.OptOutOfGridColumnResizeUsingKeyboard", out bool _OptOutOfGridColumnResizeUsingKeyboard);
            }
        }

        public static void SetSwitchDefaults(int targetFrameworkVersion)
        {
            if (Interlocked.CompareExchange(ref s_DefaultsSet, 1, 0) == 0)
            {
                if (targetFrameworkVersion <= 40700)
                {
                    System.AppContext.SetSwitch("Switch.UseLegacyAccessibilityFeatures",  true);
                }

                if (targetFrameworkVersion <= 40701)
                {
                    System.AppContext.SetSwitch("Switch.UseLegacyAccessibilityFeatures.2",  true);
                }

                if (targetFrameworkVersion <= 40702)
                {
                    System.AppContext.SetSwitch("Switch.UseLegacyAccessibilityFeatures.3", true);
                    System.AppContext.SetSwitch("Switch.UseLegacyToolTipDisplay",  true);
                    System.AppContext.SetSwitch("Switch.System.Windows.Controls.ItemsControlDoesNotSupportAutomation", true);
                }

                if (targetFrameworkVersion <= 40800)
                {
                    System.AppContext.SetSwitch("Switch.UseLegacyAccessibilityFeatures.4", true);
                }

                if (targetFrameworkVersion <= 40800)
                {
                    System.AppContext.SetSwitch("Switch.UseLegacyAccessibilityFeatures.5",  true);
                    System.AppContext.SetSwitch("Switch.System.Windows.Controls.OptOutOfGridColumnResizeUsingKeyboard",  true);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void VerifySwitches(Dispatcher dispatcher)
        {
            if (Interlocked.CompareExchange(ref s_SwitchesVerified, 1, 0) != 0)
            {
                return;
            }

            bool? flag = null;
            bool flag2 = false;
            Type typeFromHandle = typeof(AccessibilitySwitches);
            string[] accessibilityPropertyNames = AccessibilityPropertyNames;
            foreach (string name in accessibilityPropertyNames)
            {
                PropertyInfo property = typeFromHandle.GetProperty(name, BindingFlags.Static | BindingFlags.Public);
                bool flag3 = (bool)property.GetValue(null);
                if (flag2 = !flag3 && flag == true)
                {
                    break;
                }

                flag = flag3;
            }

            if (flag2)
            {
                DispatchOnError(dispatcher, "CombinationOfAccessibilitySwitchesNotSupported");
            }

            VerifyDependencies(dispatcher);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void VerifyDependencies(Dispatcher dispatcher)
        {
            if (!UseLegacyToolTipDisplay && UseNetFx472CompatibleAccessibilityFeatures)
            {
                DispatchOnError(dispatcher, string.Format("AccessibilitySwitchDependencyNotSatisfied", "Switch.UseLegacyToolTipDisplay", "Switch.UseLegacyAccessibilityFeatures.3", 3));
            }

            if (!ItemsControlDoesNotSupportAutomation && UseNetFx472CompatibleAccessibilityFeatures)
            {
                DispatchOnError(dispatcher, string.Format("AccessibilitySwitchDependencyNotSatisfied", "Switch.System.Windows.Controls.ItemsControlDoesNotSupportAutomation", "Switch.UseLegacyAccessibilityFeatures.3", 3));
            }

            if (!OptOutOfGridColumnResizeUsingKeyboard && UseNetFx48CompatibleAccessibilityFeatures)
            {
                DispatchOnError(dispatcher, string.Format("AccessibilitySwitchDependencyNotSatisfied", "Switch.System.Windows.Controls.OptOutOfGridColumnResizeUsingKeyboard", "Switch.UseLegacyAccessibilityFeatures.5", 5));
            }
        }

        private static void DispatchOnError(Dispatcher dispatcher, string message)
        {
            dispatcher.BeginInvoke(DispatcherPriority.Loaded, (Action)delegate
            {
                WriteEventAndThrow(message);
            });
        }

        [SecuritySafeCritical]
        private static void WriteEventAndThrow(string message)
        {
            NotSupportedException ex = new NotSupportedException(message);
            if (EventLog.SourceExists(".NET Runtime"))
            {
                EventLog.WriteEntry(".NET Runtime", Process.GetCurrentProcess().ProcessName + Environment.NewLine + ex.ToString(), EventLogEntryType.Error, 1023);
            }

            throw ex;
        }
    }
}
