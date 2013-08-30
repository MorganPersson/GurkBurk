using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public enum StringSplitOptions
    {
        RemoveEmptyEntries
    }

    public static class StringExtensions
    {
        public static string[] Split(this String toSplit, char[] separator, StringSplitOptions options)
        {
            return (from s in toSplit.Split(separator) where !String.IsNullOrEmpty(s) select s).ToArray();
        }
    }
}
