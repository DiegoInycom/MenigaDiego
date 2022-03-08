using System;
using System.Globalization;

namespace Ibercaja.Aggregation.UserDataConnector
{
    public static class DateTimeExtensions
    {
        public static DateTime ToEurobitsDateTimeFormat(this string date)
        {
            return DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None);
        }

        public static string ToEurobitsDateTimeFormat(this DateTime date)
        {
            return date.ToString("dd/MM/yyyy");
        }

        public static DateTime? ToNullableEurobitsDateTimeFormat(this string date)
        {
            DateTime dateTime;
            if (DateTime.TryParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
            {
                return dateTime;
            }
            return null;
        }
    }
}
