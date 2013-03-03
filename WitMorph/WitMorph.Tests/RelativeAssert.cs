using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WitMorph.Tests
{
    public static class RelativeAssert
    {
        public static void IsGreaterThanOrEqual<T>(T expected, T actual, string failureMessage)
        {
            if (Comparer<T>.Default.Compare(actual, expected) >= 0) return;
            throw new AssertFailedException(string.Format("Assert failed. Actual <{0}> was not greater than or equal to Expected <{1}>. {2}", actual, expected, failureMessage));
        }
    }
}