namespace Rocket.Libraries.Qurious.Helpers
{
    using Rocket.Libraries.QBuilder;
    using Rocket.Libraries.Validation.Services;
    using System.Globalization;

    internal class ConditionMaker
    {

        public string GetCondition(string field, FilterOperator op, object value, BuiltQuery builtQuery = null)
        {
            new DataValidator().EvaluateImmediate(value == null, $"No value was provided for filter");
            var sqlOperator = GetSqlOperator(op);
            var conditionTemplate = GetConditionTemplate(op);
            var condition = string.Empty;
            if (builtQuery == null)
            {
                condition = $" {sqlOperator} {string.Format(CultureInfo.InvariantCulture, conditionTemplate, value)}";
            }
            else
            {
                var parameterName = GetParameterName(field, builtQuery);
                condition = $" {sqlOperator} {string.Format(CultureInfo.InvariantCulture, conditionTemplate, parameterName)}";
                builtQuery.Parameters.Add(parameterName, value);
            }
            return condition;
        }

        public static string GetParameterName(string field, BuiltQuery builtQuery)
        {
            var parameterName = $"@{field}0";
            var parameterNameExists = builtQuery.Parameters.ContainsKey(parameterName);
            if (parameterNameExists)
            {
                var parameterNameIndex = 1;
                while (parameterNameExists)
                {
                    parameterName = $"@{field}{parameterNameIndex}";
                    parameterNameExists = builtQuery.Parameters.ContainsKey(parameterName);
                    parameterNameIndex++;
                }
            }

            return parameterName;
        }



        private string GetConditionTemplate(FilterOperator op)
        {
            switch (op)
            {
                default:
                    new DataValidator().EvaluateImmediate(true, $"Unknown operator '{op}'. Cannot build filter");
                    return string.Empty;

                case FilterOperator.LessThan:
                    return "'{0}'";

                case FilterOperator.LessThanOrEqualTo:
                    return "'{0}'";

                case FilterOperator.EqualTo:
                    return "'{0}'";

                case FilterOperator.GreaterThanOrEqualTo:
                    return "'{0}'";

                case FilterOperator.GreaterThan:
                    return "'{0}'";

                case FilterOperator.NotEqualTo:
                    return "'{0}'";

                case FilterOperator.StartsWith:
                    return "'%{0}'";

                case FilterOperator.Contains:
                    return "'%{0}%'";

                case FilterOperator.EndsWith:
                    return "'{0}%'";
            }
        }

        private string GetSqlOperator(FilterOperator op)
        {
            switch (op)
            {
                default:
                    new DataValidator().EvaluateImmediate(true, $"Unknown operator '{op}'. Cannot build filter");
                    return string.Empty;

                case FilterOperator.LessThan:
                    return "<";

                case FilterOperator.LessThanOrEqualTo:
                    return "<=";

                case FilterOperator.EqualTo:
                    return "=";

                case FilterOperator.GreaterThanOrEqualTo:
                    return ">=";

                case FilterOperator.GreaterThan:
                    return ">";

                case FilterOperator.NotEqualTo:
                    return "<>";

                case FilterOperator.StartsWith:
                case FilterOperator.Contains:
                case FilterOperator.EndsWith:
                    return "Like";
            }
        }
    }
}