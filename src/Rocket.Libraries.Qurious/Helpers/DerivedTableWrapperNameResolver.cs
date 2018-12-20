namespace Rocket.Libraries.Qurious.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    internal static class DerivedTableWrapperNameResolver
    {
        public static string GetWrapperName(string derivedTableName)
        {
            return derivedTableName + "Wrapper";
        }
    }
}