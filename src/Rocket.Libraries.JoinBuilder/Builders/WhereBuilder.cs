namespace Rocket.Libraries.Qurious.Builders
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class WhereBuilder : BuilderBase
    {
        private List<WhereDescription> _wheres = new List<WhereDescription>();

        public WhereBuilder(QBuilder qBuilder)
            : base(qBuilder)
        {
        }

        public WhereBuilder Where<TTable>(string field, string condition)
        {
            _wheres.Add(new WhereDescription
            {
                Clause = $"{QBuilder.TableNameAliaser.GetTableAlias(QBuilder.TableNameResolver(typeof(TTable)))}.{field} {condition}"
            });
            return this;
        }

        public WhereBuilder WhereIn<TTable, TValueType>(string field, List<TValueType> values)
        {
            var criteria = GetWhereInSectionArguments(values);

            if (string.IsNullOrEmpty(criteria))
            {
                return this;
            }
            else
            {
                return Where<TTable>(field, $" in {criteria}");
            }
        }

        public WhereBuilder WhereExplicitly(string criteria)
        {
            _wheres.Add(new WhereDescription
            {
                Clause = criteria
            });
            return this;
        }

        internal string Build()
        {
            var where = string.Empty;
            foreach (var whereDescription in _wheres)
            {
                if (!string.IsNullOrEmpty(where))
                {
                    where += "and ";
                }
                where += $"{whereDescription.Clause}{Environment.NewLine}";
            }

            _wheres = new List<WhereDescription>();
            if (string.IsNullOrEmpty(where))
            {
                return string.Empty;
            }
            else
            {
                return $"Where {where}";
            }
        }

        private string GetWhereInSectionArguments<TValueType>(List<TValueType> values)
        {
            if (values == null)
            {
                throw new Exception("Cannot build a where clause from an null list of values");
            }
            if (values.Count == 0)
            {
                return string.Empty;
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