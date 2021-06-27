using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Tools.TeamMate.TeamFoundation.WebApi.WorkItemTracking
{
    // TODO: Add support for link queries and mode (http://blogs.msdn.com/b/team_foundation/archive/2010/07/02/wiql-syntax-for-link-query.aspx)
    public class WorkItemQueryBuilder
    {
        private IList<string> fieldNames = new List<string>();
        private IList<OrderByInfo> orderBy = new List<OrderByInfo>();

        public IList<string> SelectedFields
        {
            get { return this.fieldNames; }
        }

        public IList<OrderByInfo> OrderBy
        {
            get { return this.orderBy; }
        }

        public ConditionInfo Condition { get; set; }

        public DateTime? AsOf { get; set; }

        public void AddField(string fieldName)
        {
            if (!this.SelectedFields.Contains(fieldName, StringComparer.OrdinalIgnoreCase))
            {
                this.SelectedFields.Add(fieldName);
            }
        }

        public void AddFields(params string[] fieldNames)
        {
            AddFields((IEnumerable<string>)fieldNames);
        }

        public void AddFields(IEnumerable<string> fieldNames)
        {
            foreach (var fieldName in fieldNames)
            {
                AddField(fieldName);
            }
        }

        public void AddOrderBy(string fieldName, bool ascending = true)
        {
            orderBy.Add(new OrderByInfo(fieldName, ascending));
        }

        public string ToWiql()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT ");

            if (fieldNames.Any())
            {
                for (int i = 0; i < fieldNames.Count; i++)
                {
                    if (i > 0)
                        sb.Append(", ");

                    sb.AppendFormat("[{0}]", fieldNames[i]);
                }
            }
            else
            {
                sb.AppendFormat("[{0}]", WorkItemConstants.CoreFields.Id);
            }

            sb.AppendLine();
            sb.Append("FROM WorkItems");

            if (Condition != null)
            {
                string whereClause = Condition.ToString();
                if (!String.IsNullOrEmpty(whereClause))
                {
                    sb.AppendLine();
                    sb.Append("WHERE ");
                    sb.Append(whereClause);
                }
            }

            if (orderBy.Any())
            {
                sb.AppendLine();
                sb.Append("ORDER BY ");
                for (int i = 0; i < orderBy.Count; i++)
                {
                    if (i > 0)
                        sb.Append(", ");

                    sb.Append(orderBy[i]);
                }
            }

            if (AsOf != null)
            {
                sb.AppendFormat(" AsOf '{0}'", AsOf.Value);
            }

            return sb.ToString();
        }

        public override string ToString()
        {
            return ToWiql();
        }
    }

    public class OrderByInfo
    {
        public OrderByInfo(string fieldName, bool ascending = true)
        {
            this.FieldName = fieldName;
            this.Ascending = ascending;
        }

        public string FieldName { get; set; }
        public bool Ascending { get; set; }

        public void ToString(StringBuilder sb)
        {
            sb.AppendFormat("[{0}] {1}", FieldName, (Ascending) ? "Asc" : "Desc");
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            ToString(sb);
            return sb.ToString();
        }
    }

    public static class Operators
    {
        public new const string Equals = "=";
        public const string NotEquals = "<>";
        public const string GreaterThan = ">";
        public const string LesserThan = "<";
        public const string GreaterThanOrEqual = ">=";
        public const string LesserThanOrEqual = "<=";
        public const string Contains = "Contains";
        public const string NotContains = "Does Not Contain";
        public const string In = "In";
        public const string InGroup = "In Group";
        public const string NotInGroup = "Not In Group";
        public const string WasEver = "Was Ever";
        public const string Under = "Under";
        public const string NotUnder = "Not Under";

        // Be very careful, only supported in TFS 2012 quarterly update something? Otherwise fall back to plain Contains
        public const string ContainsWords = "Contains Words";
    }

    public abstract class ConditionInfo
    {
        public virtual ConditionInfo And(ConditionInfo condition)
        {
            return new AndConditionInfo(this, condition);
        }

        public virtual ConditionInfo Or(ConditionInfo condition)
        {
            return new OrConditionInfo(this, condition);
        }

        public ConditionInfo And(string fieldName, string oper, object value, ValueType valueType = ValueType.Value)
        {
            return And(new FieldConditionInfo(fieldName, oper, value, valueType));
        }

        public ConditionInfo Or(string fieldName, string oper, object value, ValueType valueType = ValueType.Value)
        {
            return Or(new FieldConditionInfo(fieldName, oper, value, valueType));
        }

        public ConditionInfo Group()
        {
            if (this is GroupConditionInfo)
                return this;
            else
                return new GroupConditionInfo(this);
        }
    }

    public class Clause : ConditionInfo
    {
        public override string ToString()
        {
            return String.Empty;
        }
    }

    public abstract class LogicalConditionInfo : ConditionInfo
    {
        private string logicalOperator;
        private List<ConditionInfo> conditions = new List<ConditionInfo>();

        protected LogicalConditionInfo(string logicalOperator)
        {
            this.logicalOperator = logicalOperator;
        }

        private IList<ConditionInfo> Conditions
        {
            get { return this.conditions; }
        }

        public void Add(ConditionInfo condition)
        {
            this.Conditions.Add(condition);
        }

        public void Add(IEnumerable<ConditionInfo> conditions)
        {
            this.conditions.AddRange(conditions);
        }

        public override string ToString()
        {
            var clauses = conditions.Select(c => c.ToString()).Where(s => !String.IsNullOrEmpty(s)).ToArray();
            if (clauses.Length == 0)
            {
                return String.Empty;
            }
            else if (clauses.Length == 1)
            {
                return clauses[0];
            }
            else
            {
                return "(" + String.Join(" " + logicalOperator + " ", clauses) + ")";
            }
        }

        public bool Any()
        {
            return Conditions.Any();
        }
    }

    public class AndConditionInfo : LogicalConditionInfo
    {
        public AndConditionInfo(params ConditionInfo[] conditions)
            : base("AND")
        {
            this.Add(conditions);
        }

        public override ConditionInfo And(ConditionInfo condition)
        {
            Add(condition);
            return this;
        }
    }

    public class OrConditionInfo : LogicalConditionInfo
    {
        public OrConditionInfo(params ConditionInfo[] conditions)
            : base("OR")
        {
            this.Add(conditions);
        }

        public override ConditionInfo Or(ConditionInfo condition)
        {
            Add(condition);
            return this;
        }
    }

    public class GroupConditionInfo : ConditionInfo
    {
        public ConditionInfo Condition { get; set; }

        public GroupConditionInfo(ConditionInfo condition)
        {
            this.Condition = condition;
        }

        public override string ToString()
        {
            return "(" + Condition.ToString() + ")";
        }
    }

    // TODO: Needs work to support And/Or/Grouping
    public class FieldConditionInfo : ConditionInfo
    {
        public static FieldConditionInfo CurrentProjectCondition
        {
            get
            {
                return new FieldConditionInfo(WorkItemConstants.CoreFields.TeamProject, Operators.Equals, "@project");
            }
        }

        public FieldConditionInfo(string fieldName, string operatorValue, object value, ValueType valueType = ValueType.Value)
        {
            this.FieldName = fieldName;
            this.Operator = operatorValue;
            this.Value = value;
            this.ValueType = valueType;
        }

        public ValueType ValueType { get; private set; }
        public string FieldName { get; private set; }
        public string Operator { get; private set; }
        public object Value { get; private set; }

        public void ToString(StringBuilder sb)
        {
            sb.AppendFormat("[{0}] {1} {2}", FieldName, Operator, Escape(Value));
        }

        private string Escape(object value)
        {
            if ((value is int) || (value is double))
            {
                return value.ToString();
            }

            if (this.Operator == Operators.In)
            {
                if (value is System.Collections.ICollection)
                {
                    string values = String.Join(", ", ((System.Collections.ICollection)value).OfType<object>().Select(singleValue => EscapeRawValue(singleValue)));
                    return "(" + values + ")";
                }
                else
                {
                    return "(" + EscapeRawValue(value) + ")";
                }
            }

            string stringValue = (value != null) ? value.ToString() : String.Empty;

            if (ValueType == ValueType.Field)
            {
                return String.Format("[{0}]", stringValue);
            }

            if (stringValue.StartsWith("@"))
            {
                // Macro...
                return stringValue;
            }

            // Everything else needs to be escaped...
            return EscapeRawValue(stringValue);
        }

        private static string EscapeRawValue(object stringValue)
        {
            return String.Format("'{0}'", stringValue);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            ToString(sb);
            return sb.ToString();
        }
    }

    public enum ValueType
    {
        Value,
        Field
    }
}
