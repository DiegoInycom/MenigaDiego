using System;
using System.Globalization;

namespace Ibercaja.Aggregation.Eurobits
{
    public static class EurobitsDateTimeExtensions
    {
        private const string _eurobitsDateTimeFormat = "dd/MM/yyyy";

        public static DateTime ToEurobitsDateTimeFormat(this string date)
        {
            return DateTime.ParseExact(date, _eurobitsDateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None);
        }

        public static DateTime? ToNullableEurobitsDateTimeFormat(this string date)
        {
            DateTime dateTime;
            if (DateTime.TryParseExact(date, _eurobitsDateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
            {
                return dateTime;
            }

            return null;
        }

        public static string ToEurobitsDateTimeFormat(this DateTime date)
        {
            return date.ToString(_eurobitsDateTimeFormat);
        }

        public static string ToEurobitsDateTimeFormat(this DateTime? date)
        {
            return date?.ToString(_eurobitsDateTimeFormat);
        }
    }
}

