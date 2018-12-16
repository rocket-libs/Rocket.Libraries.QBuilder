using System;
using System.Linq.Expressions;
using Rocket.Libraries.Qurious.Helpers;

namespace Rocket.Libraries.Qurious.Builders
{
    public class TableBoundJoinBuilder<TLeftTable,TRightTable> : BuilderBase
    {
        private FieldNameResolver _fieldNameResolver;
        public TableBoundJoinBuilder(QBuilder qBuilder) : base(qBuilder)
        {
            _fieldNameResolver = new FieldNameResolver();
        }

        public QBuilder InnerJoin<TLeftField,TRightField>(Expression<Func<TLeftTable, TLeftField>> leftFieldNameDescriptor, Expression<Func<TRightTable, TRightField>> rightFieldNameDescriptor)
        {
            var leftField = _fieldNameResolver.GetFieldName(leftFieldNameDescriptor);
            var rightField = _fieldNameResolver.GetFieldName(rightFieldNameDescriptor);
            QBuilder.UseJoiner().InnerJoin<TLeftField,TRightTable>(leftField,rightField);
            return QBuilder;
        }
        public QBuilder FullJoin<TLeftField, TRightField>(Expression<Func<TLeftTable, TLeftField>> leftFieldNameDescriptor, Expression<Func<TRightTable, TRightField>> rightFieldNameDescriptor)
        {
            var leftField = _fieldNameResolver.GetFieldName(leftFieldNameDescriptor);
            var rightField = _fieldNameResolver.GetFieldName(rightFieldNameDescriptor);
            QBuilder.UseJoiner().FullJoin<TLeftField,TRightTable>(leftField,rightField);
            return QBuilder;
        }

        public QBuilder LeftJoin<TLeftField, TRightField>(Expression<Func<TLeftTable, TLeftField>> leftFieldNameDescriptor, Expression<Func<TRightTable, TRightField>> rightFieldNameDescriptor)
        {
            var leftField = _fieldNameResolver.GetFieldName(leftFieldNameDescriptor);
            var rightField = _fieldNameResolver.GetFieldName(rightFieldNameDescriptor);
            QBuilder.UseJoiner().LeftJoin<TLeftField,TRightTable>(leftField,rightField);
            return QBuilder;
        }

        public QBuilder RightJoin<TLeftField, TRightField>(Expression<Func<TLeftTable, TLeftField>> leftFieldNameDescriptor, Expression<Func<TRightTable, TRightField>> rightFieldNameDescriptor)
        {
            var leftField = _fieldNameResolver.GetFieldName(leftFieldNameDescriptor);
            var rightField = _fieldNameResolver.GetFieldName(rightFieldNameDescriptor);
            QBuilder.UseJoiner().RightJoin<TLeftField,TRightTable>(leftField,rightField);
            return QBuilder;
        }
    }
}