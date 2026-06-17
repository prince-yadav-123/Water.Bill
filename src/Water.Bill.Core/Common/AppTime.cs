namespace Water.Bill.Core.Common;

public static class AppTime
{
    private static readonly string[] IndiaTimeZoneIds =
    {
        "India Standard Time",
        "Asia/Kolkata"
    };

    private static readonly Lazy<TimeZoneInfo> IndiaTimeZone = new(ResolveIndiaTimeZone);

    public static DateTime IndiaNow => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IndiaTimeZone.Value);

    private static TimeZoneInfo ResolveIndiaTimeZone()
    {
        foreach (var timeZoneId in IndiaTimeZoneIds)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        return TimeZoneInfo.Local;
    }
}
