using System.Windows;
using System.Windows.Controls;
using RS.SetupApp.ViewModels;

namespace RS.SetupApp.Views;

public sealed class WizardPageTemplateSelector : DataTemplateSelector
{
    public DataTemplate? WelcomeTemplate { get; set; }
    public DataTemplate? LicenseTemplate { get; set; }
    public DataTemplate? InstallOptionsTemplate { get; set; }
    public DataTemplate? ReviewTemplate { get; set; }
    public DataTemplate? ProgressTemplate { get; set; }
    public DataTemplate? RecoveryTemplate { get; set; }
    public DataTemplate? CompletionTemplate { get; set; }
    public DataTemplate? MaintenanceTemplate { get; set; }
    public DataTemplate? UninstallConfirmationTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object item, DependencyObject container) => item switch
    {
        WizardPageKind.License => LicenseTemplate,
        WizardPageKind.InstallOptions => InstallOptionsTemplate,
        WizardPageKind.Review => ReviewTemplate,
        WizardPageKind.Progress => ProgressTemplate,
        WizardPageKind.Recovery => RecoveryTemplate,
        WizardPageKind.Completion => CompletionTemplate,
        WizardPageKind.Maintenance => MaintenanceTemplate,
        WizardPageKind.UninstallConfirm => UninstallConfirmationTemplate,
        _ => WelcomeTemplate
    };
}
