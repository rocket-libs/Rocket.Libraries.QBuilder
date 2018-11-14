namespace Rocket.Libraries.Qurious.Builders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Rocket.Libraries.Validation.Services;

    public class SelectBuilder : BuilderBase
    {
        private List<SelectDescription> _selects = new List<SelectDescription>();
        private bool _distinctRows = false;
        private long? _top;

        internal SelectBuilder(QBuilder qBuilder, string selectAlias)
            : base(qBuilder)
        {
            SelectAlias = selectAlias;
        }

        internal string FirstTableName => _selects.First().Table;

        private string SelectAlias { get; }

        public SelectBuilder SelectTop(long count)
        {
            _top = count;
            return this;
        }

        public SelectBuilder Select<TTable>(string field)
        {
            return Select<TTable>(field, string.Empty);
        }

        public SelectBuilder Select<TTable>(string field, string fieldAlias)
        {
            return SelectAggregated<TTable>(field, fieldAlias, string.Empty);
        }

        public SelectBuilder SelectAggregated<TTable>(string field, string fieldAlias, string aggregateFunction)
        {
            _selects.Add(new SelectDescription
            {
                Field = field,
                Table = QBuilder.TableNameResolver(typeof(TTable)),
                FieldAlias = fieldAlias,
                AggregateFunction = aggregateFunction,
            });
            return this;
        }

        public SelectBuilder SelectDistinctRows()
        {
            _distinctRows = true;
            return this;
        }

        internal string Build()
        {
            new DataValidator().EvaluateImmediate(() => _selects.Count == 0, "There are no fields queued for selection. Nothing to return");

            var selects = "Select ";
            if (_top.HasValue)
            {
                selects += $" Top {_top.Value} ";
            }
            if (_distinctRows)
            {
                selects += $" Distinct {Environment.NewLine}";
            }

            foreach (var selectDescription in _selects)
            {
                new DataValidator().EvaluateImmediate(() => TableNotKnown(selectDescription.Table), $"Table '{selectDescription.Table}' has not been queued as a datasource. Cannot show fields from it");
                var qualifiedField = $"{QBuilder.TableNameAliaser.GetTableAlias(selectDescription.Table)}.{selectDescription.Field}";
                var finalField = GetAggregatedFieldIfRequired(qualifiedField, selectDescription.AggregateFunction);
                selects += $"{Environment.NewLine}{finalField}";
                var hasAlias = !string.IsNullOrEmpty(selectDescription.FieldAlias);
                if (hasAlias)
                {
                    selects += $" as {selectDescription.FieldAlias}";
                }

                selects += ",";
            }

            var firstTableName = QBuilder.FirstTableName;
            selects = selects.Substring(0, selects.Length - 1) + $" From {firstTableName} " + QBuilder.TableNameAliaser.GetTableAlias(firstTableName);
            _selects = new List<SelectDescription>();
            return selects + Environment.NewLine;
        }

        private string GetAggregatedFieldIfRequired(string qualifiedField, string aggregateFunction)
        {
            if (string.IsNullOrEmpty(aggregateFunction))
            {
                return qualifiedField;
            }
            else
            {
                return $"{aggregateFunction}({qualifiedField})";
            }
        }

        private bool TableNotKnown(string table)
        {
            var joiner = QBuilder.UseJoiner();
            if (joiner.JoinsExist == false)
            {
                return false;
            }
            else
            {
                return joiner.TableNotKnown(table);
            }
        }
    }
}