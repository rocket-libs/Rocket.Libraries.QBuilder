namespace Rocket.Libraries.Qurious.Builders
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class OrderBuilder : BuilderBase
    {
        private string _descendingField;
        private string _table;

        public OrderBuilder(QBuilder qBuilder)
            : base(qBuilder)
        {
        }

        public QBuilder OrderByDescending<TTable>(string field)
        {
            _descendingField = field;
            _table = QBuilder.TableNameAliaser.GetTableAlias<TTable>();
            return QBuilder;
        }

        public string Build()
        {
            if (string.IsNullOrEmpty(_descendingField))
            {
                return string.Empty;
            }
            else
            {
                return $"{Environment.NewLine}Order By {_table}.{_descendingField} desc";
            }
        }
    }
}