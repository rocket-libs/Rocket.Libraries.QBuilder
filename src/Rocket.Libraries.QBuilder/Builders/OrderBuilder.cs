namespace Rocket.Libraries.Qurious.Builders
{
    using System;

    public class OrderBuilder : BuilderBase
    {
        private string _field;

        private string _table;

        private string _mode;

        private bool _qualifyWithTableName = true;

        public OrderBuilder(QBuilder qBuilder)
            : base(qBuilder)
        {
        }

        public OrderBuilder DoNotQualifyWithTableName()
        {
            _qualifyWithTableName = false;
            return this;
        }

        public QBuilder OrderByDescending<TTable>(string field)
        {
            _field = field;
            _table = QBuilder.TableNameAliaser.GetTableAlias<TTable>();
            _mode = "Desc";
            return QBuilder;
        }

        public QBuilder OrderBy<TTable>(string field)
        {
            _field = field;
            _table = QBuilder.TableNameAliaser.GetTableAlias<TTable>();
            _mode = "Asc";
            return QBuilder;
        }

        public string Build()
        {
            if (string.IsNullOrEmpty(_field))
            {
                return string.Empty;
            }
            else
            {
                var fieldName = _qualifyWithTableName ? $"{_table}.{_field}" : _field;
                return $"{Environment.NewLine}Order By {fieldName} {_mode}";
            }
        }
    }
}