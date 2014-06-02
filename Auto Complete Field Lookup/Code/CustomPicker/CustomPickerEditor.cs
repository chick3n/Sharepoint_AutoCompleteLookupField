using AVC_Site_Pages;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FlyingHippo.AutoComplete.CustomPicker
{
    public class CustomPickerEditor : EntityEditorWithPicker
    {
        public event EventHandler EntityValidated;
        private CustomPickerContract contract;

        public string SearchListGuid { get; set; }
        public string SearchColumnsGuid { get; set; }
        public string SearchDisplayName { get; set; }
        public string SearchKeyName { get; set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            PickerDialogType = typeof(CustomPickerDialog);
            ValidatorEnabled = true;
            AllowTypeIn = true;
            //MultiSelect = false;
        }

        public virtual void OnEntityValidated()
        {
            if (EntityValidated != null)
                EntityValidated(this, EventArgs.Empty);
        }

        public override PickerEntity ValidateEntity(PickerEntity needsValidation)
        {
            if (needsValidation.IsResolved)
                return needsValidation;

            LoadContract();

            if (!String.IsNullOrEmpty(SearchListGuid) && !String.IsNullOrEmpty(SearchColumnsGuid)
                 && !String.IsNullOrEmpty(SearchDisplayName) && !String.IsNullOrEmpty(SearchKeyName))
            {
                using (SPWeb web = SPContext.Current.Web)
                {
                    SPList searchList = web.TryGetList(SearchListGuid);
                    List<string> searchColumns = new List<string>();
                    CustomPickerHelper helper = new CustomPickerHelper();

                    if (searchList != null)
                    {
                        helper.SplitGuidColumns(SearchColumnsGuid).ForEach((i) =>
                        {
                            string column = searchList.TryGetFieldName(i);
                            if (!string.IsNullOrEmpty(column))
                                searchColumns.Add(column);
                        });

                        SPQuery query = new SPQuery();
                        query.Query = helper.BuildWhereQuery(searchColumns, needsValidation.Key, "Eq", "Or");
                        query.ViewFields = helper.BuildViewQuery(searchColumns, SearchKeyName);
                        query.RowLimit = 1;

                        var results = searchList.GetItems(query);

                        if (results != null && results.Count >= 1)
                        {
                            SPItem result = results[0];

                            string keyValue = result.TryGetItemValue(SearchKeyName);
                            string displayValue = result.TryGetItemValue(SearchDisplayName);

                            if (/*!String.IsNullOrEmpty(keyValue) && */!String.IsNullOrEmpty(displayValue))
                            {
                                needsValidation.IsResolved = true;
                                needsValidation.Key = needsValidation.Key;
                                needsValidation.DisplayText = displayValue;

                                needsValidation.EntityData.Clear();
                                needsValidation.EntityData.Add(needsValidation.Key, keyValue);
                            }
                        }
                    }
                    else
                    {
                        this.ErrorLabel.Text = "List " + SearchListGuid + " doesn't exists.";
                    }
                }
            }

            return needsValidation;
        }

        private void LoadContract()
        {
            contract = new CustomPickerContract(this.CustomProperty);

            SearchColumnsGuid = contract.SearchColumns;
            SearchDisplayName = contract.DisplayColumn;
            SearchKeyName = contract.KeyColumn;
            SearchListGuid = contract.LookupGuid;
        }

        protected override PickerEntity[] ResolveErrorBySearch(string unresolvedText)
        {
            LoadContract();

            if (!String.IsNullOrEmpty(SearchListGuid) && !String.IsNullOrEmpty(SearchColumnsGuid)
                 && !String.IsNullOrEmpty(SearchDisplayName) && !String.IsNullOrEmpty(SearchKeyName))
            {
                using (SPWeb web = SPContext.Current.Web)
                {
                    SPList searchList = web.TryGetList(SearchListGuid);
                    List<string> searchColumns = new List<string>();
                    CustomPickerHelper helper = new CustomPickerHelper();

                    if (searchList != null)
                    {
                        helper.SplitGuidColumns(SearchColumnsGuid).ForEach((i) =>
                        {
                            string column = searchList.TryGetFieldName(i);
                            if (!string.IsNullOrEmpty(column))
                                searchColumns.Add(column);
                        });

                        SPQuery query = new SPQuery();
                        query.Query = helper.BuildWhereQuery(searchColumns, unresolvedText, "BeginsWith", "Or");
                        query.ViewFields = helper.BuildViewQuery(searchColumns, SearchKeyName);
                        query.RowLimit = 5;

                        var results = searchList.GetItems(query);

                        if (results != null)
                        {
                            List<PickerEntity> entities = new List<PickerEntity>();
                            foreach (SPItem result in results)
                            {
                                PickerEntity entity = new PickerEntity();
                                entity.IsResolved = true;

                                string keyValue = result.TryGetItemValue(SearchKeyName);
                                string displayValue = result.TryGetItemValue(SearchDisplayName);

                                if (!String.IsNullOrEmpty(keyValue) && !String.IsNullOrEmpty(displayValue))
                                {
                                    entity.Key = displayValue;
                                    entity.DisplayText = displayValue;
                                    entity.EntityData.Add(displayValue, keyValue);
                                    entities.Add(entity);
                                }
                            }

                            return entities.ToArray();
                        }
                    }
                    else
                    {
                        this.ErrorLabel.Text = "List " + SearchListGuid + " doesn't exists.";
                    }
                }
            }

            return base.ResolveErrorBySearch(unresolvedText);
        }
    }
}
