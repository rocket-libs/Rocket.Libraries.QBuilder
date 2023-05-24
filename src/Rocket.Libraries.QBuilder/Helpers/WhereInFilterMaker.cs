using System;
using System.Collections.Generic;
using System.Text;

namespace Rocket.Libraries.Qurious.Helpers
{
    internal static class WhereInFilterMaker
    {
        public static string GetWhereInSectionArguments<TValueType>(IEnumerable<TValueType> values)
        {
            if (values == null)
            {
                throw new Exception("Cannot build a where clause from an null list of values");
            }

            var valuesToList = new List<TValueType>(values);

            if (valuesToList.Count == 0)
            {
                throw new Exception("No list of values provided. Exception is being thrown, as this condition is ambiguous, and going ahead would likely produce an unpredictable result.");
            }

            using (var uniqueValueResolver = new UniqueValueResolver<TValueType>())
            {
                values = uniqueValueResolver.GetUnique(valuesToList);
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