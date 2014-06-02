using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;


namespace FlyingHippo.AutoComplete.CustomPicker
{
    public class CustomPickerContract
    {
        public bool AllowMultiple { get; set; }
        public string LookupGuid { get; set; }
        public string SearchColumns { get; set; }
        public string DisplayColumn { get; set; }
        public string KeyColumn { get; set; }

        public CustomPickerContract()
        {
            AllowMultiple = false;
            LookupGuid = String.Empty;
            SearchColumns = String.Empty;
            DisplayColumn = String.Empty;
            KeyColumn = String.Empty;
        }

        public CustomPickerContract(string contract)
        {
            if (String.IsNullOrEmpty(contract))
            {
                AllowMultiple = false;
                LookupGuid = String.Empty;
                SearchColumns = String.Empty;
                DisplayColumn = String.Empty;
                KeyColumn = String.Empty;
                return;
            }

            string[] terms = contract.Split(new char[] { ';' });

            if (terms.Length >= 5)
            {
                bool allowMultiple = false;
                bool.TryParse(terms[0], out allowMultiple);
                AllowMultiple = allowMultiple;

                LookupGuid = terms[1];
                SearchColumns = terms[2];
                DisplayColumn = terms[3];
                KeyColumn = terms[4];
            }
            else
            {
                AllowMultiple = false;
                LookupGuid = String.Empty;
                SearchColumns = String.Empty;
                DisplayColumn = String.Empty;
                KeyColumn = String.Empty;
            }
        }

        public override string ToString()
        {
            return String.Format("{0};{1};{2};{3};{4}",
                AllowMultiple.ToString(),
                LookupGuid,
                SearchColumns,
                DisplayColumn,
                KeyColumn);
        }
    }

    public static class Extensions
    {
        public static string TryGetFieldName(this SPList list, string guid)
        {
            try
            {
                SPField field = list.Fields[new Guid(guid)];
                return field.StaticName;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static string TryGetItemValue(this SPItem item, string staticName)
        {
            try
            {
                var data = item[staticName];
                return data.ToString();
            }
            catch (Exception)
            {
            }

            return null;
        }

        public static SPList TryGetList(this SPWeb web, string guid)
        {
            try
            {
                return web.Lists[new Guid(guid)];
            }
            catch (Exception)
            {
            }

            return null;
        }
    }

    public class CustomPickerHelper
    {
        private List<string> columns;

        public List<string> SplitGuidColumns(string columns)
        {
            if (string.IsNullOrEmpty(columns))
                return new List<string>();

            return columns.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        public string BuildWhereQuery(List<string> columns, string key, string compareTag, string conditionTag)
        {
            List<string> queryWhere = new List<string>();

            if (!string.IsNullOrEmpty(key))
            {
                foreach (string column in columns)
                {
                    queryWhere.Add(
                        String.Format(@"<{2}><FieldRef Name=""{0}"" /><Value Type=""Text"">{1}</Value></{2}>",
                            column, key, compareTag));
                }

                string result = GroupTagBuilder(queryWhere, conditionTag);
                if (!String.IsNullOrEmpty(result))
                {
                    return String.Format("<Where>{0}</Where>", result);
                }
            }

            return string.Empty;
        }

        public string BuildViewQuery(List<string> columns)
        {
            StringBuilder query = new StringBuilder();

            foreach (string column in columns)
            {
                query.AppendFormat(@"<FieldRef Name=""{0}"" />", column);
            }

            return query.ToString();
        }

        public string BuildViewQuery(List<string> columns, params string[] additionalColumns)
        {
            StringBuilder query = new StringBuilder();

            if(additionalColumns.Length > 0)
                columns.AddRange(additionalColumns);

            foreach (string column in columns)
            {
                query.AppendFormat(@"<FieldRef Name=""{0}"" />", column);
            }

            return query.ToString();
        }

        internal string BuildViewQuery(System.Data.DataColumnCollection dataColumnCollection)
        {
            List<string> columns = new List<string>();

            foreach (System.Data.DataColumn column in dataColumnCollection)
            {
                columns.Add(column.ColumnName);
            }

            return BuildViewQuery(columns);
        }

        private string GroupTagBuilder(List<string> rows, string tag)
        {
            if (rows == null)
                return string.Empty;

            if (String.IsNullOrEmpty(tag))
                return string.Empty;

            if (rows.Count == 0)
                return string.Empty;

            bool largeGroup = (rows.Count > 2);
            int originalSize = rows.Count;

            StringBuilder query = new StringBuilder();

            if (largeGroup)
                query.Append(String.Format("<{0}>", tag));

            for (int x = rows.Count - 1; x > 0 && originalSize != 1; x -= 2)
            {
                query.AppendFormat("<{0}>{1}", tag, rows[x]);
                query.AppendFormat("{0}</{1}>", rows[x - 1], tag);
                rows.RemoveAt(x);
                rows.RemoveAt(x - 1);
            }

            if (rows.Count == 1)
            {
                query.Append(rows[0]);
            }

            if (largeGroup)
                query.Append(String.Format("</{0}>", tag));

            return query.ToString();
        }
    }
}
