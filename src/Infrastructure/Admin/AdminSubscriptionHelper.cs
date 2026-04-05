namespace Nkkonsult.Infrastructure.Admin;

internal static class AdminSubscriptionHelper
{
    /// <summary>
    /// Calcule le statut d'abonnement affiché dans l'interface admin.
    /// Logique de présentation — ne pas déplacer dans le domaine.
    /// </summary>
    internal static string GetSubscriptionStatus(bool isActive, DateTime trialEndDate)
    {
        if (!isActive) return "expiré";
        if (trialEndDate > DateTime.UtcNow) return "essai";
        return "actif";  // compte payant (futur — facturation Sprint 3)
    }
}
