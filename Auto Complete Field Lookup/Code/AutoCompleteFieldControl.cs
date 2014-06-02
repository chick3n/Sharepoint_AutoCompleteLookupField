using Microsoft.SharePoint.WebControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;

using FlyingHippo.AutoComplete.CustomPicker;
using System.Collections;
using Microsoft.SharePoint;

namespace FlyingHippo.AutoComplete.Fields
{
    public class AutoCompleteFieldControl : BaseFieldControl
    {
        protected Label EmailPrefix;
        protected Label EmailValueForDisplay;
        private CustomPickerEditor pickerEditor;
        private AutoCompleteFieldType parentField;

        public AutoCompleteFieldControl(AutoCompleteFieldType parent)
        {
            this.parentField = parent;
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            if (this.ControlMode != SPControlMode.Display)
            {
                pickerEditor = (CustomPickerEditor)this.TemplateContainer.FindControl("CustomPicker");

                if (pickerEditor == null)
                    pickerEditor = new CustomPickerEditor();

                pickerEditor.ID = "CustomPicker";
                pickerEditor.ValidatorEnabled = true;
                pickerEditor.SearchListGuid = parentField.SearchListGuid;
                pickerEditor.SearchColumnsGuid = parentField.SearchColumnGuids;
                pickerEditor.SearchDisplayName = parentField.DisplayColumn;
                pickerEditor.SearchKeyName = "ID";
                pickerEditor.MultiSelect = parentField.AllowMultiple;

                pickerEditor.CustomProperty = new CustomPickerContract
                {
                    AllowMultiple = parentField.AllowMultiple,
                    DisplayColumn = parentField.DisplayColumn,
                    KeyColumn = "ID",
                    LookupGuid = parentField.SearchListGuid,
                    SearchColumns = parentField.SearchColumnGuids
                }.ToString();

                //if (pickerEditor.MultiSelect)
                //    pickerEditor.MaximumEntities = 10;

                if (Value != null && !string.IsNullOrEmpty(Value.ToString()) && SPControlMode.Edit == this.ControlMode)
                {
                    ArrayList entities = new ArrayList();
                    PickerEntity entity = new PickerEntity();
                    entity.Key = (string)this.ItemFieldValue;
                    entities.Add(entity);
                    pickerEditor.UpdateEntities(entities);
                }

                this.Controls.Add(pickerEditor);
            }
        }

        public override void Validate()
        {
            base.Validate();

            if (ControlMode == SPControlMode.Display || !IsValid)
                return;

            if (Field.Required && pickerEditor.ResolvedEntities.Count > 0)
            {
                this.ErrorMessage = Field.Title + " must have a value.";
                IsValid = false;
                return;
            }

            IsValid = true;
        }

        public override void UpdateFieldValueInItem()
        {
            ItemFieldValue = Value;
        }

        public override object Value
        {
            get
            {
                this.EnsureChildControls();

                if (this.pickerEditor != null)
                {
                    if (this.pickerEditor.ResolvedEntities.Count > 1)
                    {
                        var lookupCollection = new SPFieldLookupValueCollection();

                        foreach (PickerEntity entity in pickerEditor.ResolvedEntities)
                        {
                            int entityId = 0;
                            if (int.TryParse(entity.EntityData[entity.Key] as string, out entityId))
                            {
                                var lookupItem = new SPFieldLookupValue(entityId, entity.DisplayText);
                                lookupCollection.Add(lookupItem);
                            }
                        }

                        pickerEditor.IsValid = true;
                        return lookupCollection;
                    }
                    else if(this.pickerEditor.ResolvedEntities.Count == 1)
                    {
                        PickerEntity entity = pickerEditor.ResolvedEntities[0] as PickerEntity;
                        int entityId = 0;
                        if (int.TryParse(entity.EntityData[entity.Key] as string, out entityId))
                        {
                            var lookupItem = new SPFieldLookupValue(entityId, entity.DisplayText);
                            pickerEditor.IsValid = true;
                            return lookupItem;
                        }
                    }
                }

                pickerEditor.IsValid = false;
                return null;
            }
            set
            {
                this.EnsureChildControls();

                if (value == null)
                    return;

                if (value is SPFieldLookupValueCollection)
                {
                    ArrayList entities = new ArrayList();
                    foreach (SPFieldLookupValue val in value as SPFieldLookupValueCollection)
                    {
                        PickerEntity entity = new PickerEntity
                        {
                            Key = val.LookupValue,
                            IsResolved = true,
                            DisplayText = val.LookupValue,
                            Description = val.LookupValue
                        };
                        entity.EntityData.Add(val.LookupValue, val.LookupId);
                        entities.Add(entity);
                    }
                    pickerEditor.UpdateEntities(entities);
                }

            }
        }

    }
}
