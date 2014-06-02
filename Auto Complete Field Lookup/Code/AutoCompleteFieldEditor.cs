using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FlyingHippo.AutoComplete.Fields
{
    public class AutoCompleteFieldEditor : UserControl, IFieldEditor
    {
        private AutoCompleteFieldType parentField;
        private string searchListGuid;
        private string searchListName;
        private List<string> searchColumnGuids;
        private string searchDisplayName;
        private string searchKeyName;

        protected DropDownList ddlSearchList;
        protected ListBox lbSearchColumns;
        protected CheckBox chkAllowMultipleDLF;
        protected DropDownList ddlDisplayColumn;
        //protected DropDownList ddlKeyColumn;

        public bool DisplayAsNewSection
        {
            get { return false; }
        }

        public void InitializeWithField(Microsoft.SharePoint.SPField field)
        {
            parentField = field as AutoCompleteFieldType;

            if (parentField != null)
            {
                searchListGuid = parentField.SearchListGuid;
                searchListName = parentField.SearchListName;
                chkAllowMultipleDLF.Checked = parentField.AllowMultiple;
                searchDisplayName = parentField.DisplayColumn;
                //searchKeyName = parentField.KeyColumn;

                string columnGuids = parentField.SearchColumnGuids;

                if (!string.IsNullOrEmpty(columnGuids))
                {
                    searchColumnGuids = columnGuids.Split(',').ToList();
                }
            }

            ddlSearchList.SelectedIndexChanged += ddlSearchList_SelectedIndexChanged;

            if (!Page.IsPostBack)
            {
                using (SPWeb web = SPContext.Current.Web)
                {
                    SPListCollection lists = web.Lists;

                    foreach (SPList list in lists)
                    {
                        if (list.BaseType != SPBaseType.GenericList)
                            continue;

                        ListItem item = new ListItem();
                        item.Text = list.Title;
                        item.Value = list.ID.ToString();
                        if (!String.IsNullOrEmpty(searchListGuid) && item.Value == searchListGuid)
                            item.Selected = true;

                        ddlSearchList.Items.Add(item);
                    }

                    PopulateFieldList();

                }
            }
        }

        void ddlSearchList_SelectedIndexChanged(object sender, EventArgs e)
        {
            PopulateFieldList();
        }

        private void PopulateFieldList()
        {
            using (SPWeb web = SPContext.Current.Site.OpenWeb())
            {
                if (ddlSearchList.SelectedItem != null)
                {
                    searchListGuid = ddlSearchList.SelectedItem.Value;

                    if (searchListGuid != null)
                    {
                        lbSearchColumns.Items.Clear();
                        ddlDisplayColumn.Items.Clear();
                        //ddlKeyColumn.Items.Clear();

                        SPFieldCollection fields = web.Lists[new Guid(searchListGuid)].Fields;

                        if (fields != null)
                        {
                            foreach (SPField listField in fields)
                            {
                                //if (SPBuiltInFieldId.Contains(listField.Id))
                                //    continue;

                                if (listField.Type == SPFieldType.Lookup)
                                    continue;

                                ListItem item = new ListItem();
                                item.Text = listField.StaticName;
                                item.Value = listField.Id.ToString();
                                if (!Page.IsPostBack && IsColumnSelected(item.Value))
                                    item.Selected = true;

                                lbSearchColumns.Items.Add(item);

                                ListItem displayItem = new ListItem();
                                displayItem.Text = listField.StaticName;
                                displayItem.Value = listField.StaticName;
                                if (!Page.IsPostBack && searchDisplayName != null && searchDisplayName.Equals(displayItem.Value))
                                    displayItem.Selected = true;

                                ddlDisplayColumn.Items.Add(displayItem);

                                //ListItem keyItem = new ListItem();
                                //keyItem.Text = listField.StaticName;
                                //keyItem.Value = listField.StaticName;
                                //if (!Page.IsPostBack && searchKeyName != null && searchKeyName.Equals(keyItem.Value))
                                //    keyItem.Selected = true;

                                //ddlKeyColumn.Items.Add(keyItem);
                            }
                        }
                    }
                }
            }
        }

        private bool IsColumnSelected(string value)
        {
            if (searchColumnGuids == null)
                return false;

            if (searchColumnGuids.Count == 0)
                return false;

            foreach (var item in searchColumnGuids)
            {
                if (item.Equals(value))
                    return true;
            }

            return false;
        }

        public void OnSaveChange(Microsoft.SharePoint.SPField field, bool isNewField)
        {
            AutoCompleteFieldType customFieldType = field as AutoCompleteFieldType;

            if (customFieldType != null)
            {
                customFieldType.IsNew = isNewField;
                customFieldType.SearchListGuid = ddlSearchList.SelectedItem.Value;
                customFieldType.SearchListName = ddlSearchList.SelectedItem.Text;
                customFieldType.AllowMultiple = false;
                customFieldType.DisplayColumn = ddlDisplayColumn.SelectedItem.Value;
                //customFieldType.KeyColumn = ddlKeyColumn.SelectedItem.Value;

                foreach (var postId in Request.Form.AllKeys)
                {
                    if (postId.Contains("chkAllowMultipleDLF"))
                    {
                        if (Request[postId] == "on")
                            customFieldType.AllowMultiple = true;
                        else customFieldType.AllowMultiple = false;
                        break;
                    }
                }

                StringBuilder columnGuids = new StringBuilder();
                foreach (int colId in lbSearchColumns.GetSelectedIndices())
                {
                    ListItem item = lbSearchColumns.Items[colId];
                    columnGuids.Append(item.Value + ",");
                }
                customFieldType.SearchColumnGuids = columnGuids.ToString();

                customFieldType.LookupList = ddlSearchList.SelectedItem.Value;
                customFieldType.LookupField = ddlDisplayColumn.SelectedItem.Value;
                customFieldType.LookupWebId = SPContext.Current.Web.ID;
            }
        }
    }
}
