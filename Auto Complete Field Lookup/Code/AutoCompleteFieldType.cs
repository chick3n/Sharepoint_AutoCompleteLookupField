using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using System.Security.Permissions;
using Microsoft.SharePoint.Security;
using Microsoft.SharePoint.WebControls;
using System.Xml;
using System.Threading;
using System.Reflection;


namespace FlyingHippo.AutoComplete.Fields
{
    public class AutoCompleteFieldType : SPFieldLookup
    {
        #region Threading IDs
        private const string THREAD_SEARCHLISTGUID = "SearchListGuid";
        private const string THREAD_ALLOWMULTIPLE = "AllowMultiple";
        private const string THREAD_SEARCHCOLUMNGUIDS = "SearchColumnGuids";
        private const string THREAD_SEARCHDISPLAYNAME = "DisplayColumnStaticName";
        //private const string THREAD_SEARCHKEYNAME = "KeyColumnStaticName";
        #endregion

        private string searchListGuid;
        public string SearchListGuid
        {
            get
            {
                //string value = (string)GetThreadDataValue(THREAD_SEARCHLISTGUID);
                string value = (string)GetFieldAttribute(THREAD_SEARCHLISTGUID);
                if (value == null)
                    return string.Empty;
                return value;
            }
            set
            {
                SetFieldAttribute(THREAD_SEARCHLISTGUID, value);
            }
        }

        public string SearchListName { get; set; }
        public bool IsNew { get; set; }

        private bool allowMultiple;
        public bool AllowMultiple
        {
            get
            {
                return (bool)GetFieldAttributeAsBool(THREAD_ALLOWMULTIPLE);
            }
            set
            {
                base.AllowMultipleValues = value;
                SetFieldAttribute(THREAD_ALLOWMULTIPLE, value.ToString());

                if (value)
                    SetFieldAttribute("Type", "AutoCompleteLookupMultiFieldType");
                else SetFieldAttribute("Type", "AutoCompleteLookupFieldType");
            }
        }

        private string searchColumnGuids;
        public string SearchColumnGuids
        {
            get
            {
                string value = (string)GetFieldAttribute(THREAD_SEARCHCOLUMNGUIDS);
                if (value == null)
                    return string.Empty;
                return value;
            }
            set
            {
                SetFieldAttribute(THREAD_SEARCHCOLUMNGUIDS, value);
            }
        }

        private string displayColumn;
        public string DisplayColumn
        {
            get
            {
                string value = (string)GetFieldAttribute(THREAD_SEARCHDISPLAYNAME);
                if (value == null)
                    return string.Empty;
                return value;
            }
            set
            {
                SetFieldAttribute(THREAD_SEARCHDISPLAYNAME, value);
            }
        }

        public AutoCompleteFieldType(SPFieldCollection fields, string fieldName)
            : base(fields, fieldName)
        {
            LoadProperties();
        }

        public AutoCompleteFieldType(SPFieldCollection fields, string typeName, string displayName)
            : base(fields, typeName, displayName)
        {
            LoadProperties();
        }

        public override BaseFieldControl FieldRenderingControl
        {
            [SharePointPermission(SecurityAction.LinkDemand, ObjectModel = true)]
            get
            {
                BaseFieldControl fieldControl = new AutoCompleteFieldControl(this);
                fieldControl.FieldName = InternalName;

                return fieldControl;
            }
        }

        private void LoadProperties()
        {
            //base.LookupList = SearchListGuid;
            //base.LookupField = DisplayColumn;
            //base.LookupWebId = SPContext.Current.Web.ID;
        }

        public override void OnAdded(SPAddFieldOptions op)
        {
            base.OnAdded(op);
            Update();
        }

        public override void Update()
        {
            SetCustomProperty("SearchListGuid", SearchListGuid);
            SetCustomProperty("SearchColumnGuids", SearchColumnGuids);
            SetCustomProperty("AllowMultiple", AllowMultiple);
            SetCustomProperty("DisplayColumnStaticName", DisplayColumn);
            //SetCustomProperty("KeyColumnStaticName", KeyColumn);
            
            base.Update();

            FreeThreadData();
        }

        #region Reflection

        private void SetFieldAttribute(string attribute, string value)
        {
            Type baseType;
            BindingFlags flags;
            MethodInfo mi;

            baseType = typeof(SPFieldText);
            flags = BindingFlags.Instance | BindingFlags.NonPublic;
            mi = baseType.GetMethod("SetFieldAttributeValue", flags);
            mi.Invoke(this, new object[] { attribute, value });
        }

        private string GetFieldAttribute(string attribute)
        {
            Type baseType;
            BindingFlags flags;
            MethodInfo mi;

            baseType = typeof(SPFieldText);
            flags = BindingFlags.Instance | BindingFlags.NonPublic;
            mi = baseType.GetMethod("GetFieldAttributeValue", flags, null, new Type[] { typeof(String) }, null);
            object obj = mi.Invoke(this, new object[] { attribute });

            if (obj == null)
                return string.Empty;
            return obj.ToString();
        }

        private bool GetFieldAttributeAsBool(string attribute)
        {
            Type baseType;
            BindingFlags flags;
            MethodInfo mi;

            baseType = typeof(SPFieldText);
            flags = BindingFlags.Instance | BindingFlags.NonPublic;
            mi = baseType.GetMethod("GetFieldAttributeValue", flags, null, new Type[] { typeof(String) }, null);
            object obj = mi.Invoke(this, new object[] { attribute });

            if (obj == null)
                return false;

            bool result = false;
            Boolean.TryParse(obj.ToString(), out result);
            return result;
        }

        #endregion

        #region Threading

        private string GetThreadDataValue(string propertyName)
        {
            LocalDataStoreSlot slot = Thread.GetNamedDataSlot(propertyName);
            object dataSlot = Thread.GetData(slot);

            if (dataSlot != null)
            {
                return dataSlot.ToString();
            }

            return string.Empty;
        }

        private bool GetThreadDataValueAsBool(string propertyName)
        {
            LocalDataStoreSlot slot = Thread.GetNamedDataSlot(propertyName);
            object dataSlot = Thread.GetData(slot);

            if (dataSlot != null)
            {
                return (bool)dataSlot;
            }

            return false;
        }

        private void SetThreadDataValue(string propertyName, object value)
        {
            Thread.SetData(Thread.GetNamedDataSlot(propertyName), value);
        }

        private void FreeThreadData()
        {
            Thread.FreeNamedDataSlot(THREAD_ALLOWMULTIPLE);
            Thread.FreeNamedDataSlot(THREAD_SEARCHCOLUMNGUIDS);
            Thread.FreeNamedDataSlot(THREAD_SEARCHDISPLAYNAME);
            //Thread.FreeNamedDataSlot(THREAD_SEARCHKEYNAME);
            Thread.FreeNamedDataSlot(THREAD_SEARCHLISTGUID);
        }

        #endregion Threading
    }
}
