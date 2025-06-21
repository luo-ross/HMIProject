using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Interop
{
    public static class CoreAppContextSwitches
    {
        public const string DoNotScaleForDpiChangesSwitchName = "Switch.System.Windows.DoNotScaleForDpiChanges";
        public const string DisableStylusAndTouchSupportSwitchName = "Switch.System.Windows.Input.Stylus.DisableStylusAndTouchSupport";
        public const string EnablePointerSupportSwitchName = "Switch.System.Windows.Input.Stylus.EnablePointerSupport";
        public const string OverrideExceptionWithNullReferenceExceptionName = "Switch.System.Windows.Media.ImageSourceConverter.OverrideExceptionWithNullReferenceException";
        private static int _overrideExceptionWithNullReferenceException;
        public const string DisableDiagnosticsSwitchName = "Switch.System.Windows.Diagnostics.DisableDiagnostics";
        public const string AllowChangesDuringVisualTreeChangedSwitchName = "Switch.System.Windows.Diagnostics.AllowChangesDuringVisualTreeChanged";
        public const string DisableImplicitTouchKeyboardInvocationSwitchName = "Switch.System.Windows.Input.Stylus.DisableImplicitTouchKeyboardInvocation";
        public const string ShouldRenderEvenWhenNoDisplayDevicesAreAvailableSwitchName = "Switch.System.Windows.Media.ShouldRenderEvenWhenNoDisplayDevicesAreAvailable";
        public const string ShouldNotRenderInNonInteractiveWindowStationSwitchName = "Switch.System.Windows.Media.ShouldNotRenderInNonInteractiveWindowStation";
        public const string DoNotUsePresentationDpiCapabilityTier2OrGreaterSwitchName = "Switch.System.Windows.DoNotUsePresentationDpiCapabilityTier2OrGreater";
        public const string DoNotUsePresentationDpiCapabilityTier3OrGreaterSwitchName = "Switch.System.Windows.DoNotUsePresentationDpiCapabilityTier3OrGreater";
        public const string AllowExternalProcessToBlockAccessToTemporaryFilesSwitchName = "Switch.System.Windows.AllowExternalProcessToBlockAccessToTemporaryFiles";
        public const string EnableLegacyDangerousClipboardDeserializationModeSwitchName = "Switch.System.Windows.EnableLegacyDangerousClipboardDeserializationMode";
        public const string HostVisualDisconnectsOnWrongThreadSwitchName = "Switch.System.Windows.Media.HostVisual.DisconnectsOnWrongThread";
        public const string OptOutOfMoveToChromedWindowFixSwitchName = "Switch.System.Windows.Interop.MouseInput.OptOutOfMoveToChromedWindowFix";
        public const string DoNotOptOutOfMoveToChromedWindowFixSwitchName = "Switch.System.Windows.Interop.MouseInput.DoNotOptOutOfMoveToChromedWindowFix";
        public const string DisableDirtyRectanglesSwitchName = "Switch.System.Windows.Media.MediaContext.DisableDirtyRectangles";
        public const string EnableDynamicDirtyRectanglesSwitchName = "Switch.System.Windows.Media.MediaContext.EnableDynamicDirtyRectangles";
        public const string OptOutIgnoreWin32SetLastErrorSwitchName = "Switch.System.Windows.Common.OptOutIgnoreWin32SetLastError";

        public static bool DoNotScaleForDpiChanges
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                System.AppContext.TryGetSwitch("Switch.System.Windows.DoNotScaleForDpiChanges", out bool _doNotScaleForDpiChanges);
                return _doNotScaleForDpiChanges;
            }
        }

        public static bool DisableStylusAndTouchSupport
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return System.AppContext.TryGetSwitch("Switch.System.Windows.Input.Stylus.DisableStylusAndTouchSupport", out bool _disableStylusAndTouchSupport);
            }
        }

        public static bool EnablePointerSupport
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return System.AppContext.TryGetSwitch("Switch.System.Windows.Input.Stylus.EnablePointerSupport", out bool _enablePointerSupport);
            }
        }

        public static bool OverrideExceptionWithNullReferenceException
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return System.AppContext.TryGetSwitch("Switch.System.Windows.Media.ImageSourceConverter.OverrideExceptionWithNullReferenceException", out bool _overrideExceptionWithNullReferenceException);
            }
        }

        public static bool DisableDiagnostics
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return System.AppContext.TryGetSwitch("Switch.System.Windows.Diagnostics.DisableDiagnostics", out bool _disableDiagnostics);
            }
        }

        public static bool AllowChangesDuringVisualTreeChanged
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return System.AppContext.TryGetSwitch("Switch.System.Windows.Diagnostics.AllowChangesDuringVisualTreeChanged", out bool _allowChangesDuringVisualTreeChanged);
            }
        }

        public static bool DisableImplicitTouchKeyboardInvocation
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return System.AppContext.TryGetSwitch("Switch.System.Windows.Input.Stylus.DisableImplicitTouchKeyboardInvocation", out bool _disableImplicitTouchKeyboardInvocation);
            }
        }

        public static bool UseNetFx47CompatibleAccessibilityFeatures
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return AccessibilitySwitches.UseNetFx47CompatibleAccessibilityFeatures;
            }
        }

        public static bool UseNetFx471CompatibleAccessibilityFeatures
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return AccessibilitySwitches.UseNetFx471CompatibleAccessibilityFeatures;
            }
        }

        public static bool UseNetFx472CompatibleAccessibilityFeatures
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return AccessibilitySwitches.UseNetFx472CompatibleAccessibilityFeatures;
            }
        }

        public static bool ShouldRenderEvenWhenNoDisplayDevicesAreAvailable
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return System.AppContext.TryGetSwitch("Switch.System.Windows.Media.ShouldRenderEvenWhenNoDisplayDevicesAreAvailable", out bool _shouldRenderEvenWhenNoDisplayDevicesAreAvailable);
            }
        }

        public static bool ShouldNotRenderInNonInteractiveWindowStation => System.AppContext.TryGetSwitch("Switch.System.Windows.Media.ShouldNotRenderInNonInteractiveWindowStation", out bool _shouldNotRenderInNonInteractiveWindowStation);

        public static bool DoNotUsePresentationDpiCapabilityTier2OrGreater
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return System.AppContext.TryGetSwitch("Switch.System.Windows.DoNotUsePresentationDpiCapabilityTier2OrGreater", out bool _doNotUsePresentationDpiCapabilityTier2OrGreater);
            }
        }

        public static bool DoNotUsePresentationDpiCapabilityTier3OrGreater
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return System.AppContext.TryGetSwitch("Switch.System.Windows.DoNotUsePresentationDpiCapabilityTier3OrGreater", out bool _doNotUsePresentationDpiCapabilityTier3OrGreater);
            }
        }

        public static bool AllowExternalProcessToBlockAccessToTemporaryFiles
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return System.AppContext.TryGetSwitch("Switch.System.Windows.AllowExternalProcessToBlockAccessToTemporaryFiles", out bool _allowExternalProcessToBlockAccessToTemporaryFiles);
            }
        }

        public static bool EnableLegacyDangerousClipboardDeserializationMode
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return System.AppContext.TryGetSwitch("Switch.System.Windows.EnableLegacyDangerousClipboardDeserializationMode", out bool _enableLegacyDangerousClipboardDeserializationMode);
            }
        }

        public static bool HostVisualDisconnectsOnWrongThread
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return System.AppContext.TryGetSwitch("Switch.System.Windows.Media.HostVisual.DisconnectsOnWrongThread", out bool _hostVisualDisconnectsOnWrongThread);
            }
        }

        public static bool OptOutOfMoveToChromedWindowFix
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return System.AppContext.TryGetSwitch("Switch.System.Windows.Interop.MouseInput.OptOutOfMoveToChromedWindowFix", out bool _OptOutOfMoveToChromedWindowFix);
            }
        }

        public static bool DoNotOptOutOfMoveToChromedWindowFix
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return System.AppContext.TryGetSwitch("Switch.System.Windows.Interop.MouseInput.DoNotOptOutOfMoveToChromedWindowFix", out bool _DoNotOptOutOfMoveToChromedWindowFix);
            }
        }

        public static bool DisableDirtyRectangles
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (EnableDynamicDirtyRectangles)
                {
                    AppContext.TryGetSwitch("Switch.System.Windows.Media.MediaContext.DisableDirtyRectangles", out var isEnabled);
                    return isEnabled;
                }

                return System.AppContext.TryGetSwitch("Switch.System.Windows.Media.MediaContext.DisableDirtyRectangles", out bool _DisableDirtyRectangles);
            }
        }

        public static bool EnableDynamicDirtyRectangles
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return System.AppContext.TryGetSwitch("Switch.System.Windows.Media.MediaContext.EnableDynamicDirtyRectangles", out bool _EnableDynamicDirtyRectangles);
            }
        }

        public static bool OptOutIgnoreWin32SetLastError
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return System.AppContext.TryGetSwitch("Switch.System.Windows.Common.OptOutIgnoreWin32SetLastError", out bool _optOutIgnoreWin32SetLastError);
            }
        }
    }
}
