using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Utilities;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Domain.ValueConverter
{
    public class EncryptedValueConverter<T> : ValueConverter<T, string>
    {
        public EncryptedValueConverter() : base(
            v => SecurityHelpers.EncryptAes(v != null ? v.ToString() : ""),
            v => ConvertFromString(SecurityHelpers.DecryptAes(v)))
        {
        }

        private static T ConvertFromString(string decrypted)
        {
            if (typeof(T) == typeof(DateTime))
                return (T)(object)DateTime.Parse(decrypted);

            if (typeof(T) == typeof(DateTimeOffset))
                return (T)(object)DateTimeOffset.Parse(decrypted);

            if (typeof(T).IsEnum)
                return (T)Enum.Parse(typeof(T), decrypted);

            return (T)Convert.ChangeType(decrypted, typeof(T));
        }
    }
}
