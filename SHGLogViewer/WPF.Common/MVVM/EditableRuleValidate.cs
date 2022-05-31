using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Reflection;

namespace WPF.Common
{
    public abstract class EditableRuleValidate<T> : RuleValidator, ICustomEditableObject where T : class
    {
        private T CachedState = null;
        private bool _ignoreBeginEdit;
        private bool _ignoreEndCancelEdit;

        #region ctor

        public EditableRuleValidate()
        {
            IsEditable = false;
        }

        #endregion

        #region Public Properties


        #endregion


        #region IEditableObject Members
        // Created because controls like the grid can fire these on a cell
        // or row basis and screws up our ObjectIsDirty and functionality
        // in our implementation.
        //These methods should be called from
        // our code instead of the default methods.
        /// <summary>
        /// Use this instead of BeginEdit to set a model in Edit mode
        /// </summary>
        public void BeginEditManual()
        {
            try
            {
                this.CachedState = (T)this.MemberwiseClone();
                IsEditable = true;
                _ignoreBeginEdit = true;


            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("EditableRuleValidate.BeginEditManual() failed on Type {0}.", typeof(T)), ex);
            }

        }


        public void BeginEdit()
        {
            try
            {

                // RadGridView initiates BeginEdit when a user tries to edit a cell.
                // We edit more than one cell so we don't need to duplicate our cache here.
                if (!IsEditable && !_ignoreBeginEdit)
                {
                    _ignoreBeginEdit = true;

                    BeginEditManual();
                }

            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("EditableRuleValidate.BeginEdit() failed on Type {0}.", typeof(T)), ex);
            }

        }

        /// <summary>
        /// Controls such as a gridView will call begin and endEdit when a user tries
        /// to edit each cell.  Adding these endpoints so we can subscribe to pre events
        /// and tell our implementation of IEditable to ignore these calls.
        /// This is important for a grid with multiple records and you wish to change 
        /// values in multiple cells of multiple records and sent a bulk update
        /// and not just update each cell
        /// </summary>
        public void IgnoreNextBeginEdit(bool val)
        {
            _ignoreBeginEdit = val;
        }

        public void IgnoreNextEndCancelEdit(bool val)
        {
            _ignoreEndCancelEdit = val;
        }

        public void CancelEdit()
        {
            try
            {
                if (CachedState != null && !_ignoreEndCancelEdit)
                {
                    _ignoreBeginEdit = false;

                    CancelEditManual();

                }

            }
            catch (Exception ex)
            {
                throw new Exception("EditRuleValidate Exception", ex);

            }
            finally
            {

            }
        }


        // Created because controls like the grid can fire these on a cell
        // or row basis and screws up our ObjectIsDirty and functionality
        // in our implementation.
        //These methods should be called from
        // our code instead of the default methods.
        /// <summary>
        /// Use this instead of CancelEdit to take our model out of Edit Mode
        /// </summary>
        public void CancelEditManual()
        {
            try
            {
                PropertyInfo[] _objectProperties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

                // Set IsEditable = false so the changes below won't effect the DirtyProperties collection
                IsEditable = false;

                // We only want to change Properties with public settors.
                foreach (PropertyInfo propInfo in _objectProperties.Where(w => w.CanWrite && w.GetSetMethod() != null))
                {

                    //if (propInfo.CanWrite)
                    //{
                    object currentValue = propInfo.GetValue(this, null);
                    object originalValue = propInfo.GetValue(CachedState, null);
                    propInfo.SetValue(this, originalValue, null);

                    // We must fire the INPC on a value change in order for bound properties to update
                    if (currentValue != originalValue)
                        base.RaisePropertyChanged(propInfo.Name);
                    //}
                }

                // Remove all properties which were deemed as dirty.
                base.DirtyProperties.Clear();
                base.ObjectIsDirty = false;

            }
            catch (Exception ex)
            {
                throw new Exception("CancelEditManual Exception", ex);

            }
            finally
            {

            }
        }

        public void EndEdit()
        {
            if (!_ignoreEndCancelEdit)
            {
                _ignoreBeginEdit = false;

                EndEditManual();
            }
        }


        // Created because controls like the grid can fire these on a cell
        // or row basis and screws up our ObjectIsDirty and functionality
        // in our implementation.
        //These methods should be called from
        // our code instead of the default methods.
        /// <summary>
        /// Use this instead of EndEditEdit to take our model out of Edit Mode
        /// </summary>
        public void EndEditManual()
        {
            CachedState = null;
            IsEditable = false;

            base.DirtyProperties.Clear();
            base.ObjectIsDirty = false;
        }

        #endregion

    }

}
