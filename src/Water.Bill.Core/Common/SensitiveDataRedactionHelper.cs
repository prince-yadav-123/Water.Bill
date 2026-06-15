using System.Text.RegularExpressions;

namespace Water.Bill.Core.Common;

public static partial class SensitiveDataRedactionHelper
{
    private const string RedactedValue = "***REDACTED***";

    public static string? Redact(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        var redacted = value;
        redacted = BearerTokenRegex().Replace(redacted, "$1 " + RedactedValue);
        redacted = KeyValueRegex().Replace(redacted, match =>
        {
            var key = match.Groups["key"].Value;
            var separator = match.Groups["separator"].Value;
            return $"{key}{separator}{RedactedValue}";
        });

        return redacted;
    }

    [GeneratedRegex(@"(?i)\b(Bearer)\s+[A-Za-z0-9\-._~+/]+=*", RegexOptions.Compiled)]
    private static partial Regex BearerTokenRegex();

    [GeneratedRegex(@"(?ix)
        \b
        (?<key>
            otp|
            token|
            access_token|
            refresh_token|
            password|
            confirmpassword|
            oldpassword|
            newpassword|
            mobile|
            phone|
            email|
            consumerno|
            consumernumber|
            returnurl|
            secret|
            key|
            apikey|
            auth|
            authorization
        )
        (?<separator>\s*(?:=|:)\s*)
        (?:
            ""[^""]*""|
            '[^']*'|
            [^&\s,;}\]]+
        )", RegexOptions.Compiled)]
    private static partial Regex KeyValueRegex();
}
