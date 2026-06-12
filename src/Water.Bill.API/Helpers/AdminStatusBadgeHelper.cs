namespace Water.Bill.API.Helpers;

public static class AdminStatusBadgeHelper
{
    public static string CssClass(string? status)
    {
        var token = Normalize(status);
        return token switch
        {
            "" or "-" or "unknown" or "draft" or "inactive" => "text-bg-secondary",

            "success" or "paid" or "approved" or "completed" or "finalconsumercreated"
                or "issued" or "active" or "applied" or "yes" or "0300" or "y" or "suc000" => "text-bg-success",

            "failed" or "failure" or "rejected" or "cancelled" or "canceled" or "reversed"
                or "error" or "declined" or "no" => "text-bg-danger",

            "pending" or "pendingpayment" or "paymentpending" or "underreview" or "submitted"
                or "feepending" or "notpaid" => "text-bg-warning text-dark",

            "inprogress" or "processing" or "forwarded" or "reviewing" => "text-bg-info text-dark",

            "sentbacktoapplicant" or "correctionrequired" or "actionrequired"
                or "sentbacktoprevious" or "sentback" => "text-bg-orange text-dark",

            _ => "text-bg-secondary"
        };
    }

    public static string Label(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return "Unknown";

        return status.Trim() switch
        {
            "PendingPayment" => "Pending Payment",
            "PaymentPending" => "Payment Pending",
            "UnderReview" => "Under Review",
            "FinalConsumerCreated" => "Final Consumer Created",
            "SentBackToApplicant" => "Sent Back to Applicant",
            "SentBackToPrevious" => "Sent Back to Previous",
            "CorrectionRequired" => "Correction Required",
            "InProgress" => "In Progress",
            "0300" => "Bank Success",
            "Y" => "Success",
            "N" => "Pending",
            _ => status.Trim()
        };
    }

    private static string Normalize(string? status)
        => string.IsNullOrWhiteSpace(status)
            ? string.Empty
            : new string(status.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
}
