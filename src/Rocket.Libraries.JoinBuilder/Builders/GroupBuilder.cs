namespace Rocket.Libraries.Qurious.Builders
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Rocket.Libraries.Qurious.Models;

    public class GroupBuilder : BuilderBase
    {
        public GroupBuilder(QBuilder qBuilder)
            : base(qBuilder)
        { }

        private List<GroupDescription> GroupFields { get; set; } = new List<GroupDescription>();

        public GroupBuilder GroupBy<TTable>(string field)
        {
            GroupFields.Add(new GroupDescription
            {
                FieldName = field,
                TableName = QBuilder.TableNameResolver(typeof(TTable)),
            });
            return this;
        }

        public string Build()
        {
            if (GroupFields.Count == 0)
            {
                return string.Empty;
            }
            else
            {
                var grouping = $"{Environment.NewLine} Group By ";
                foreach (var groupField in GroupFields)
                {
                    grouping += $"{QBuilder.TableNameAliaser.GetTableAlias(groupField.TableName)}.{groupField.FieldName},";
                }

                grouping = grouping.Substring(0, grouping.Length - 1) + $"{Environment.NewLine}";
                return grouping;
            }
        }
    }
}