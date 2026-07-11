using RS.SetupApp.Core;

namespace RS.SetupApp.ViewModels;

public sealed class SetupLanguageResources
{
    public string LanguageSwitcherLabel { get; init; } = string.Empty;

    public string LanguageEnglishShort { get; init; } = "EN";

    public string LanguageChineseShort { get; init; } = "中文";

    public string InstallerHeadline { get; init; } = string.Empty;

    public string RuntimeStatusTitle { get; init; } = string.Empty;

    public string UpdateStatusTemplate { get; init; } = "{0}";

    public string WelcomeTitle { get; init; } = string.Empty;

    public string WelcomeIntroText { get; init; } = string.Empty;

    public string WelcomeChecklistTitle { get; init; } = string.Empty;

    public string WelcomeChecklistStepOne { get; init; } = string.Empty;

    public string WelcomeChecklistStepTwo { get; init; } = string.Empty;

    public string WelcomeChecklistStepThree { get; init; } = string.Empty;

    public string ContinueButtonText { get; init; } = string.Empty;

    public string LicenseTitle { get; init; } = string.Empty;

    public string AcceptLicenseText { get; init; } = string.Empty;

    public string BackButtonText { get; init; } = string.Empty;

    public string InstallOptionsTitle { get; init; } = string.Empty;

    public string InstallOptionsSubtitle { get; init; } = string.Empty;

    public string InstallScopeLabel { get; init; } = string.Empty;

    public string InstallScopeCurrentUserTitle { get; init; } = string.Empty;

    public string InstallScopeCurrentUserDescription { get; init; } = string.Empty;

    public string InstallScopeAllUsersTitle { get; init; } = string.Empty;

    public string InstallScopeAllUsersDescription { get; init; } = string.Empty;

    public string InstallDirectoryLabel { get; init; } = string.Empty;

    public string BrowseButtonText { get; init; } = string.Empty;

    public string ResetButtonText { get; init; } = string.Empty;

    public string InstallDirectoryEditableHint { get; init; } = string.Empty;

    public string InstallDirectoryLockedHint { get; init; } = string.Empty;

    public string CreateShortcutsText { get; init; } = string.Empty;

    public string EnableAutoStartText { get; init; } = string.Empty;

    public string InstallButtonText { get; init; } = string.Empty;

    public string ApplyChangesButtonText { get; init; } = string.Empty;

    public string ApplyingSetupTitle { get; init; } = string.Empty;

    public string ApplyingSetupDescription { get; init; } = string.Empty;

    public string CompletedTitle { get; init; } = string.Empty;

    public string LaunchAppButtonText { get; init; } = string.Empty;

    public string FinishButtonText { get; init; } = string.Empty;

    public string MaintenanceTitle { get; init; } = string.Empty;

    public string MaintenanceDescription { get; init; } = string.Empty;

    public string InstalledVersionTemplate { get; init; } = "{0}";

    public string RepairButtonText { get; init; } = string.Empty;

    public string CheckUpdateButtonText { get; init; } = string.Empty;

    public string OpenUpdateButtonText { get; init; } = string.Empty;

    public string InstallOptionsButtonText { get; init; } = string.Empty;

    public string UninstallButtonText { get; init; } = string.Empty;

    public string UpdateTitle { get; init; } = string.Empty;

    public string AvailableVersionTemplate { get; init; } = "{0}";

    public string UpdateNowButtonText { get; init; } = string.Empty;

    public string UninstallTitle { get; init; } = string.Empty;

    public string UninstallDescription { get; init; } = string.Empty;

    public string PurgeDataText { get; init; } = string.Empty;

    public string SelectFolderDialogDescription { get; init; } = string.Empty;

    public string ErrorDialogTitle { get; init; } = string.Empty;

    public string DefaultWelcomeTemplate { get; init; } = string.Empty;

    public string SupportLinkFallbackText { get; init; } = string.Empty;

    public string UpdateLinkFallbackText { get; init; } = string.Empty;

    public string NotInstalledStatusText { get; init; } = string.Empty;

    public string NotCheckedStatusText { get; init; } = string.Empty;

    public string NoUpdatesStatusText { get; init; } = string.Empty;

    public string CheckFailedStatusText { get; init; } = string.Empty;

    public string ReadyStatusText { get; init; } = string.Empty;

    public string NoUpdateAvailableStatusText { get; init; } = string.Empty;

    public string UpdateAvailableStatusTemplate { get; init; } = string.Empty;

    public string RunningOperationTemplate { get; init; } = string.Empty;

    public string InstallVerb { get; init; } = string.Empty;

    public string RepairVerb { get; init; } = string.Empty;

    public string UpdateVerb { get; init; } = string.Empty;

    public string UninstallVerb { get; init; } = string.Empty;

    public string ReviewTitle { get; init; } = "Review";

    public string ReviewDescription { get; init; } = "Review your deployment choices before the safe setup workflow starts.";

    public string ReviewButtonText { get; init; } = "Review";

    public string OpenLogButtonText { get; init; } = "Open log";

    public string LogDetailsLabel { get; init; } = "Log details";

    public string RecoveryTitle { get; init; } = "Recovery needs attention";

    public string RecoveryRetryButtonText { get; init; } = "Retry recovery";

    public string LegacyClaimDescription { get; init; } = "Existing installation detected without an ownership marker.";

    public string LegacyClaimButtonText { get; init; } = "Claim legacy installation";

    public string CloseWindowAccessibleName { get; init; } = "Close setup window";

    public string ProgressTimelineAccessibleName { get; init; } = "Completed setup steps";

    public string RecoveryErrorsAccessibleName { get; init; } = "Recovery errors";

    public string RailWelcomeLabel { get; init; } = "01  Welcome";

    public string RailConfigureLabel { get; init; } = "02  Configure";

    public string RailReviewLabel { get; init; } = "03  Review";

    public string RailApplyLabel { get; init; } = "04  Apply";

    public string RailCompleteLabel { get; init; } = "05  Complete";

    public string FormatUpdateStatus(string value)
    {
        return string.Format(UpdateStatusTemplate, value);
    }

    public string FormatInstalledVersion(string value)
    {
        return string.Format(InstalledVersionTemplate, value);
    }

    public string FormatAvailableVersion(string value)
    {
        return string.Format(AvailableVersionTemplate, value);
    }

    public string FormatRunningOperation(SetupMode mode)
    {
        return string.Format(RunningOperationTemplate, ResolveVerb(mode));
    }

    private string ResolveVerb(SetupMode mode)
    {
        return mode switch
        {
            SetupMode.Repair => RepairVerb,
            SetupMode.Update => UpdateVerb,
            SetupMode.Uninstall => UninstallVerb,
            _ => InstallVerb
        };
    }
}
