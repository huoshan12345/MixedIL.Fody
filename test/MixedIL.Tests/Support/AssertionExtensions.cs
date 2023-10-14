﻿using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Sdk;
using System.Diagnostics.CodeAnalysis;

namespace MixedIL.Tests.Support
{
    internal static class AssertionExtensions
    {
        public static T ShouldNotBeNull<T>(this T? actual) where T : class
        {
            Assert.NotNull(actual);
            return actual ?? throw NotNullException.ForNullValue();
        }

        public static void ShouldNotContain<T>(this IEnumerable<T> items, Func<T, bool> predicate)
            => Assert.DoesNotContain(items, item => predicate(item));

        public static void ShouldNotContain(this string str, string expectedSubstring)
            => Assert.DoesNotContain(expectedSubstring, str);

        public static void ShouldContain(this string str, string expectedSubstring)
            => Assert.Contains(expectedSubstring, str);
    }
}
