﻿namespace Rocket.Libraries.Qurious.Builders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Rocket.Libraries.Qurious.Helpers;
    using Rocket.Libraries.Qurious.Models;
    using Rocket.Libraries.Validation.Services;

    public class WhereBuilder : BuilderBase
    {
        private List<WhereDescription> _wheres = new List<WhereDescription>();
        private FieldNameResolver _fieldNameResolver;
        private string _nextConjuntion = "And";
        private WhereConjuntionBuilder _whereConjunctionBuilder;

        private List<ParenthesesDescription> _parentheses = new List<ParenthesesDescription>();

        private ParenthesesDescription _implicitParentheses = new ParenthesesDescription
        {
            Id = default(Guid)
        };

        private ParenthesesDescription CurrentParentheses
        {
            get
            {
                var explicitParentheses = _parentheses.LastOrDefault(a => a.Closed == false);
                var hasExplicitedParentheses = explicitParentheses != null;
                if(hasExplicitedParentheses)
                {
                    return explicitParentheses;
                }
                else
                {
                    return _implicitParentheses;
                }
            }
        }

        public WhereBuilder(QBuilder qBuilder)
            : base(qBuilder)
        {
            _whereConjunctionBuilder = new WhereConjuntionBuilder(this, qBuilder);
            _fieldNameResolver = new FieldNameResolver();
        }

        public WhereConjuntionBuilder UseConjunction()
        {
            return _whereConjunctionBuilder;
        }

        public WhereBuilder OpenParentheses()
        {
            new DataValidator().EvaluateImmediate(() => CurrentParentheses != null,"Nested parentheses are not yet supported.");
            _parentheses.Add(new ParenthesesDescription{
                Closed = false,
                Id = Guid.NewGuid()
            });
            return this;
        }

        public WhereBuilder CloseParentheses()
        {
            var noOpenParentheses = CurrentParentheses == null || CurrentParentheses.Id == _implicitParentheses.Id;
            new DataValidator().EvaluateImmediate(() => noOpenParentheses,"There is currently no open parentheses. Nothing to close.");
            CurrentParentheses.Closed = true;
            return this;
        }

        public WhereConjuntionBuilder Where<TTable>(FilterDescription<TTable> filterDescription)
        {
            if(filterDescription.FilterSet)
            {
                return Where<TTable>(filterDescription.FieldName,filterDescription.Filter);
            }
            else
            {
                return _whereConjunctionBuilder;
            }
        }

        public WhereConjuntionBuilder Where<TTable>(string field, string condition)
        {
            _wheres.Add(new WhereDescription
            {
                Clause = $"{QBuilder.TableNameAliaser.GetTableAlias(QBuilder.TableNameResolver(typeof(TTable)))}.{field} {condition}",
                Conjunction = _nextConjuntion,
                ParenthesesId = CurrentParentheses.Id,
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

        [Obsolete("Is limiting in that it presumes the 'condition' parameter can be added before evaluation the 'fnIfTrue' function param. In some cases this causes exceptions. Use 'OptionalWhere' instead and return an empty string for instances where the filter is to be left out ")]
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

        /// <summary>
        /// This method only injects a where filter if the <paramref name="fnResolveCondition"/> does not resolve to String.Empty
        /// </summary>
        /// <typeparam name="TTable">The table to filter on</typeparam>
        /// <param name="field">The field to filter on</param>
        /// <param name="fnResolveCondition">A function which when executed returns either a valid SQL filter or String.Empty. If String.Empty is returned, no where filter is injected and conversely, an filter is injected if a valid SQL filter is returned</param>
        /// <returns>Instance of <see cref="WhereConjuntionBuilder"/> to allow chaining of filter calls</returns>
        public WhereConjuntionBuilder OptionalWhere<TTable>(string field, Func<string> fnResolveCondition)
        {
            var condition = fnResolveCondition();
            var conditionExists = string.IsNullOrEmpty(condition) == false;
            if (conditionExists)
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
                ParenthesesId = CurrentParentheses.Id,
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
            var unClosedParenthesesExists = CurrentParentheses != null && CurrentParentheses.Id != _implicitParentheses.Id;
            new DataValidator().EvaluateImmediate(() => unClosedParenthesesExists,$"An unclosed parentheses was found. Please check your query.");
            var currentParenthesesId = _implicitParentheses.Id;

            foreach (var whereDescription in _wheres)
            {
                var parenthesesIdIsDifferent = currentParenthesesId != whereDescription.ParenthesesId;
                var exitingExplicitParentheses = parenthesesIdIsDifferent && currentParenthesesId != _implicitParentheses.Id;
                var enteringImplicitParentheses = whereDescription.ParenthesesId == _implicitParentheses.Id;
                if(exitingExplicitParentheses)
                {
                    where += ")";
                }

                if (!string.IsNullOrEmpty(where))
                {
                    where += $" {whereDescription.Conjunction} ";
                }

                if(parenthesesIdIsDifferent)
                {
                    if(enteringImplicitParentheses == false)
                    {
                        where += " (";
                    }
                    currentParenthesesId = whereDescription.ParenthesesId;
                }

                where += $"{whereDescription.Clause}{Environment.NewLine}";
            }

            where = GetWithFinalParenthesesTerminatedIfRequired(currentParenthesesId,where);

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

        private string GetWithFinalParenthesesTerminatedIfRequired(Guid currentParenthesesId, string where)
        {
            var hasUnterminatedParentheses = currentParenthesesId != _implicitParentheses.Id;
            if(hasUnterminatedParentheses)
            {
                where += ") ";
            }
            return where;
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