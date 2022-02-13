using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;

namespace Tests.Extensions
{
    internal static class AssertExtensions
    {
        internal static void ThrowsInnerException<TInnerException>(this Assert assert, Action action)
        {
            try
            {
                action();
                Assert.Fail("No exception thrown.");
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    Assert.IsInstanceOfType(ex.InnerException, typeof(TInnerException));
                }
                else
                {
                    Assert.Fail("Inner exception not present.");
                }
            }
        }
    }
}
