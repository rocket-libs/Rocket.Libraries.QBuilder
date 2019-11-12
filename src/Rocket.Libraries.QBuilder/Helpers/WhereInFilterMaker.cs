using System;
using System.Collections.Generic;
using System.Text;

namespace Rocket.Libraries.Qurious.Helpers
{
    internal static class WhereInFilterMaker
    {
        public static string GetWhereInSectionArguments<TValueType>(List<TValueType> values)
        {
            if (values == null)
            {
                throw new Exception("Cannot build a where clause from an null list of values");
            }

            if (values.Count == 0)
            {
                throw new Exception("No list of values provided. Exception is being thrown, as this condition is ambiguous, and going ahead would likely produce an unpredictable result.");
            }

            var args = string.Empty;
            foreach (var value in values)
            {
                args += $",'{value}'";
            }

            args = $"({args.Substring(1)})";
            return args;
        }
    }
}