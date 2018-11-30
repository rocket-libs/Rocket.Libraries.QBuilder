namespace Rocket.Libraries.Qurious.Builders
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class WhereBuilder : BuilderBase
    {
        private List<WhereDescription> _wheres = new List<WhereDescription>();
        private string _nextConjuntion = "And";
        private WhereConjuntionBuilder _whereConjunctionBuilder;

        public WhereBuilder(QBuilder qBuilder)
            : base(qBuilder)
        {
            _whereConjunctionBuilder = new WhereConjuntionBuilder(this, qBuilder);
        }

        public WhereConjuntionBuilder Where<TTable>(string field, string condition)
        {
            _wheres.Add(new WhereDescription
            {
                Clause = $"{QBuilder.TableNameAliaser.GetTableAlias(QBuilder.TableNameResolver(typeof(TTable)))}.{field} {condition}",
                Conjunction = _nextConjuntion,
            });
            return _whereConjunctionBuilder;
        }

        public WhereConjuntionBuilder WhereIn<TTable, TValueType>(string field, List<TValueType> values)
        {
            var criteria = GetWhereInSectionArguments(values);

            if (string.IsNullOrEmpty(criteria))
            {
                return _whereConjunctionBuilder;
            }
            else
            {
                return Where<TTable>(field, $" in {criteria}");
            }
        }

        public WhereConjuntionBuilder ConditionalWhere<TTable>(string field, string condition, Func<bool> fnIfTrue)
        {
            if (fnIfTrue())
            {
                return Where<TTable>(field, condition);
            }
            else
            {
                return _whereConjunctionBuilder;
            }
        }

        public WhereConjuntionBuilder WhereExplicitly(string criteria)
        {
            _wheres.Add(new WhereDescription
            {
                Clause = criteria,
                Conjunction = _nextConjuntion,
            });
            return _whereConjunctionBuilder;
        }

        internal void SetNextConjunction(string conjunction)
        {
            _nextConjuntion = conjunction;
        }

        internal string Build()
        {
            var where = string.Empty;
            foreach (var whereDescription in _wheres)
            {
                if (!string.IsNullOrEmpty(where))
                {
                    where += $" {whereDescription.Conjunction} ";
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