using System;
using System.Collections.Generic;
using System.Text;

namespace Rocket.Libraries.JoinBuilder.Builders
{
    public class OrderBuilder
    {
        private JoinBuilder JoinBuilder { get; }

        internal OrderBuilder(JoinBuilder joinBuilder)
        {
            JoinBuilder = joinBuilder;
        }

        private string _descendingField;
        private string _table;

        public JoinBuilder OrderByDescending<TTable>(string field)
        {
            _descendingField = field;
            _table = JoinBuilder.TableNameAliaser.GetTableAlias<TTable>();
            return JoinBuilder;
        }

        public string Build()
        {
            if(string.IsNullOrEmpty(_descendingField))
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
