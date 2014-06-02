using AVC_Site_Pages;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;

namespace FlyingHippo.AutoComplete.CustomPicker
{
    class CustomQueryControl : SimpleQueryControl
    {
        private CustomPickerContract contract;

        public CustomQueryControl()
        {
            Load += new EventHandler(CustomQueryControl_Load);

        }

        void CustomQueryControl_Load(object sender, EventArgs e)
        {
            contract = new CustomPickerContract(this.PickerDialog.CustomProperty);

            if (!Page.IsPostBack)
            {
                EnsureChildControls();

                List<string> columns = ((CustomPickerDialog)this.PickerDialog).NamedColumns;

                if (columns == null)
                    return;

                columns.ForEach((i) => mColumnList.Items.Add(i));
            }
        }

        protected override int IssueQuery(string search, string groupName, int pageIndex, int pageSize)
        {
            DataTable results = getFilteredData(search, groupName);

            PickerDialog.Results = results;
            PickerDialog.ResultControl.PageSize = results.Rows.Count;

            return results.Rows.Count;
        }

        public override PickerEntity GetEntity(DataRow dr)
        {
            if (dr == null)
                return null;

            PickerEntity entity = new PickerEntity();
            entity.DisplayText = string.Empty + dr[contract.DisplayColumn];
            entity.Key = string.Empty + dr[contract.DisplayColumn];
            entity.Description = string.Empty + dr[contract.DisplayColumn];
            entity.IsResolved = true;
            entity.EntityData.Add(entity.Key, dr[contract.KeyColumn]);

            return entity;
        }

        public DataTable getFilteredData(string search, string groupName)
        {

            DataTable dt = new DataTable();
            bool displayColumnAdded = false;
            bool keyColumnAdded = false;

            foreach (string column in ((CustomPickerDialog)this.PickerDialog).NamedColumns)
            {
                dt.Columns.Add(column);
                if (column.Equals(contract.DisplayColumn))
                    displayColumnAdded = true;
                if (column.Equals(contract.KeyColumn))
                    keyColumnAdded = true;
            }

            if (!displayColumnAdded)
                dt.Columns.Add(contract.DisplayColumn);
            if (!keyColumnAdded)
                dt.Columns.Add(contract.KeyColumn);

            CustomPickerHelper helper = new CustomPickerHelper();
            SPQuery query = new SPQuery();
            List<string> searchColumns = new List<string>();
            searchColumns.Add(groupName);
            query.Query = helper.BuildWhereQuery(searchColumns, search, "BeginsWith", "Or");
            query.ViewFields = helper.BuildViewQuery(dt.Columns);
            query.RowLimit = 1;

            using (SPWeb web = SPContext.Current.Site.OpenWeb())
            {
                SPList searchList = web.TryGetList(contract.LookupGuid);

                if (searchList != null)
                {
                    var results = searchList.GetItems(query);

                    if (results != null && results.Count >= 1)
                    {
                        foreach (SPItem result in results)
                        {
                            var values = new object[((CustomPickerDialog)this.PickerDialog).NamedColumns.Count];
                            for (int x = 0; x < ((CustomPickerDialog)this.PickerDialog).NamedColumns.Count; x++)
                                values[x] = result.TryGetItemValue(((CustomPickerDialog)this.PickerDialog).NamedColumns[x]);

                            dt.Rows.Add(values);
                        }
                    }
                }
            }

            return dt;
        }
    }
}
