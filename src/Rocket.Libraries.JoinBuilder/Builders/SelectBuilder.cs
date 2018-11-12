using Rocket.Libraries.Validation.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rocket.Libraries.JoinBuilder.Builders
{
    public class SelectBuilder
    {
        private JoinBuilder JoinBuilder { get; }
        private List<SelectDescription> _selects = new List<SelectDescription>();
        private bool _distinctRows = false;
        private long? _top;

        internal SelectBuilder(JoinBuilder joinBuilder)
        {
            JoinBuilder = joinBuilder;
        }

        public SelectBuilder SelectTop(long count)
        {
            _top = count;
            return this;
        }

        public JoinBuilder Then()
        {
            return JoinBuilder;
        }

        public SelectBuilder Select<TTable>(string field)
        {
            return Select<TTable>(field, string.Empty);
        }

        public SelectBuilder Select<TTable>(string field, string fieldAlias)
        {
            _selects.Add(new SelectDescription
            {
                Field = field,
                Table = JoinBuilder.TableNameResolver(typeof(TTable)),
                FieldAlias = fieldAlias
            });
            return this;
        }

        public SelectBuilder SelectDistinctRows()
        {
            _distinctRows = true;
            return this;
        }

        public string Build()
        {
            new DataValidator().EvaluateImmediate(() => _selects.Count == 0, "There are no fields queued for selection. Nothing to return");

            var selects = "Select ";
            if(_top.HasValue)
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
                selects += $"{Environment.NewLine}{JoinBuilder.TableNameAliaser.GetTableAlias(selectDescription.Table)}.{selectDescription.Field}";
                var hasAlias = !string.IsNullOrEmpty(selectDescription.FieldAlias);
                if (hasAlias)
                {
                    selects += $" as {selectDescription.FieldAlias}";
                }

                selects += ",";
            }

            selects = selects.Substring(0, selects.Length - 1) + $" From {JoinBuilder.Joins.First().RightTable} " + JoinBuilder.TableNameAliaser.GetTableAlias(JoinBuilder.Joins.First().RightTable);
            _selects = new List<SelectDescription>();
            return selects + Environment.NewLine;
        }

        private bool TableNotKnown(string table)
        {
            var occurenceCount = JoinBuilder.Joins.Count(a => a.LeftTable.Equals(table, StringComparison.CurrentCultureIgnoreCase)
            || a.RightTable.Equals(table, StringComparison.CurrentCultureIgnoreCase));
            return occurenceCount == 0;
        }
    }
}
