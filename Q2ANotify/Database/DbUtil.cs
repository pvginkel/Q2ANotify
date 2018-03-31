using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Q2ANotify.Database
{
    public static class DbUtil
    {
        private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

        public static DateTime ParseDateTime(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return DateTime.ParseExact(value, DateTimeFormat, CultureInfo.InvariantCulture);
        }

        public static DateTime? ParseDateTimeOpt(string value)
        {
            if (value == null)
                return null;

            return DateTime.ParseExact(value, DateTimeFormat, CultureInfo.InvariantCulture);
        }

        public static string PrintDateTime(DateTime value)
        {
            return value.ToString(DateTimeFormat);
        }

        public static string PrintDateTimeOpt(DateTime? value)
        {
            if (!value.HasValue)
                return null;

            return value.Value.ToString(DateTimeFormat);
        }
    }
}
