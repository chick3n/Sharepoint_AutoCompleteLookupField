using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;

namespace FlyingHippo.AutoComplete.CustomPicker
{
    public class CustomPickerDialog : PickerDialog
    {
        private CustomPickerContract contract;
        public List<string> NamedColumns;

        public CustomPickerDialog()
            : base(new CustomQueryControl(), new TableResultControl(), new CustomPickerEditor())
        {
            NamedColumns = new List<string>();
        }

        protected override void OnLoad(EventArgs e)
        {
            contract = new CustomPickerContract(this.CustomProperty);

            ArrayList columnDisplayName = ((TableResultControl)base.ResultControl).ColumnDisplayNames;
            ArrayList columnNames = ((TableResultControl)base.ResultControl).ColumnNames;
            ArrayList columnWidths = ((TableResultControl)base.ResultControl).ColumnWidths;

            CustomPickerHelper helper = new CustomPickerHelper();

            List<string> columns = helper.SplitGuidColumns(contract.SearchColumns);
            using (SPWeb web = SPContext.Current.Site.OpenWeb())
            {
                SPList searchList = web.TryGetList(contract.LookupGuid);

                if (columns != null && searchList != null)
                {
                    columnDisplayName.Clear();
                    columnNames.Clear();
                    columnWidths.Clear();
                    foreach (var column in columns)
                    {
                        string fieldName = searchList.TryGetFieldName(column);
                        if (!String.IsNullOrEmpty(fieldName))
                        {
                            columnDisplayName.Add(fieldName);
                            columnNames.Add(fieldName);
                            columnWidths.Add(Unit.Percentage(100 / columns.Count).ToString());
                            NamedColumns.Add(fieldName);
                        }
                    }
                }
            }

            base.OnLoad(e);
        }
    }
}
