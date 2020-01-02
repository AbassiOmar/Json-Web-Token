using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Secutity.IssuerJWT.Extensions
{
    public static class DateTimeExtensions
    {
        public static Int64 ToUnixEpochDate(this DateTime dateTime)
        {
            var result = (Int64)Math.Round((dateTime.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);

            return result;
        }
    }
}
