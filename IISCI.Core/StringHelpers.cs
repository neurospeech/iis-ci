using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IISCI
{
    public static class StringHelpers
    {

        public static string ToNonNullLowerCase(this string text) {
            if (string.IsNullOrWhiteSpace(text))
                return null;
            return text.Trim().ToLower();
        }

    }
}