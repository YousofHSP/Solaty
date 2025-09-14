using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Common.Exceptions;

namespace Common.Utilities
{
    public static class StringExtensions
    {
        public static bool HasValue(this string value, bool ignoreWhiteSpace = true)
        {
            return ignoreWhiteSpace ? !string.IsNullOrWhiteSpace(value) : !string.IsNullOrEmpty(value);
        }

        public static int ToInt(this string value)
        {
            return Convert.ToInt32(value);
        }

        public static decimal ToDecimal(this string value)
        {
            return Convert.ToDecimal(value);
        }

        public static string ToNumeric(this float value)
        {
            return value.ToString("N0"); // "123,456"
        }

        public static string ToNumeric(this decimal value)
        {
            return value.ToString("N0");
        }

        public static string ToCurrency(this float value)
        {
            return value.ToString("C0");
        }

        public static string ToCurrency(this decimal value)
        {
            return value.ToString("C0");
        }

        public static string En2Fa(this string str)
        {
            return str.Replace("0", "۰")
                .Replace("1", "۱")
                .Replace("2", "۲")
                .Replace("3", "۳")
                .Replace("4", "۴")
                .Replace("5", "۵")
                .Replace("6", "۶")
                .Replace("7", "۷")
                .Replace("8", "۸")
                .Replace("9", "۹");
        }

        public static string Fa2En(this string str)
        {
            return str.Replace("۰", "0")
                .Replace("۱", "1")
                .Replace("۲", "2")
                .Replace("۳", "3")
                .Replace("۴", "4")
                .Replace("۵", "5")
                .Replace("۶", "6")
                .Replace("۷", "7")
                .Replace("۸", "8")
                .Replace("۹", "9");
        }

        public static string FixPersianChars(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return str;

            str = str
                .Replace('\u200F', ' ') // حذف Right-To-Left Mark (RTLM) یا جایگزینی با فاصله
                .Replace('\u200C', ' ') // حذف ZWNJ (در صورت نیاز می‌تونی به جای فاصله، "" بذاری)
                .Replace("ي", "ی") // ی عربی به ی فارسی
                .Replace("ك", "ک") // ک عربی به ک فارسی
                .Replace("ة", "ه") // تاء مربوطة به ه
                .Replace("‌", " ") // ZWNJ فارسی (ممکنه از صفحه‌کلید فارسی آمده باشه)
                .Replace("‏", " ") // LRM یا Left-To-Right Mark (در بعضی متون وجود داره)
                .Trim(); // حذف فاصله اضافی ابتدا و انتها
            return Regex.Replace(str, @"\s{2,}", " ");
        }

        public static string CleanString(this string str)
        {
            return str.Trim().FixPersianChars().Fa2En().NullIfEmpty();
        }

        public static string NullIfEmpty(this string str)
        {
            return str?.Length == 0 ? null : str;
        }

        public static string ToShamsi(this DateTime dateTime, bool withTime = true)
        {
            PersianCalendar persianCalendar = new PersianCalendar();
            int year = persianCalendar.GetYear(dateTime);
            int month = persianCalendar.GetMonth(dateTime);
            int day = persianCalendar.GetDayOfMonth(dateTime);
            int hour = dateTime.Hour;
            int minute = dateTime.Minute;

            if (withTime)
                return
                    $"{year}/{month.ToString("00")}/{day.ToString("00")} {hour.ToString("00")}:{minute.ToString("00")}";
            return $"{year}/{month.ToString("00")}/{day.ToString("00")}";
        }

        public static string ToShamsi(this DateTimeOffset dateTime, bool withTime = true)
        {
            PersianCalendar persianCalendar = new PersianCalendar();
            var localDateTime = dateTime.ToLocalTime().DateTime;
            int year = persianCalendar.GetYear(localDateTime);
            int month = persianCalendar.GetMonth(localDateTime);
            int day = persianCalendar.GetDayOfMonth(localDateTime);
            int hour = dateTime.Hour;
            int minute = dateTime.Minute;

            if (withTime)
                return
                    $"{year}/{month.ToString("00")}/{day.ToString("00")} {hour.ToString("00")}:{minute.ToString("00")}";
            return $"{year}/{month.ToString("00")}/{day.ToString("00")}";
        }

        public static DateTimeOffset? ToMiladiN(this string? str)
        {
            if (string.IsNullOrEmpty(str))
                return null;
            return str.ToMiladi();
        }

        public static DateTimeOffset ToMiladi(this string str)
        {
            try
            {
                var dateTimeParts = str.Trim().Split(' ');

                // بخش تاریخ
                var dateParts = dateTimeParts[0].Split('/');
                int year = int.Parse(dateParts[0]);
                int month = int.Parse(dateParts[1]);
                int day = int.Parse(dateParts[2]);

                int hour = 0;
                int minute = 0;

                // اگر ساعت هم وجود داشت
                if (dateTimeParts.Length > 1)
                {
                    var timeParts = dateTimeParts[1].Split(':');
                    hour = int.Parse(timeParts[0]);
                    minute = int.Parse(timeParts[1]);
                }

                var pc = new PersianCalendar();
                return pc.ToDateTime(year, month, day, hour, minute, 0, 0).ToLocalTime();
            }
            catch (Exception e)
            {
                throw new BadRequestException("تاریخ معتبر نیست");
            }
        }

        public static string SanitizeFileName(this string fileName)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }

            fileName = Path.GetFileName(fileName);

            return fileName;
        }
        public static int LevenshteinDistance(this string s, string t)
        {
            var dp = new int[s.Length + 1, t.Length + 1];

            for (int i = 0; i <= s.Length; i++)
                dp[i, 0] = i;

            for (int j = 0; j <= t.Length; j++)
                dp[0, j] = j;

            for (int i = 1; i <= s.Length; i++)
            {
                for (int j = 1; j <= t.Length; j++)
                {
                    int cost = s[i - 1] == t[j - 1] ? 0 : 1;

                    dp[i, j] = Math.Min(
                        Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1),
                        dp[i - 1, j - 1] + cost);
                }
            }

            return dp[s.Length, t.Length];
        }
    }
}