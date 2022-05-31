using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace WPF.Common
{
    /// <summary>
    /// Implementation of our own IEditableObject to make adjustments when necessary
    /// </summary>
    public interface ICustomEditableObject : IEditableObject
    {
        /// <summary>
        /// Controls such as a gridView will call begin and endEdit when a user tries
        /// to edit each cell.  Adding these endpoints so we can subscribe to pre events
        /// and tell our implementation of IEditable to ignore these calls.
        /// This is important for a grid with multiple records and you wish to change 
        /// values in multiple cells of multiple records and sent a bulk update
        /// and not just update each cell
        /// </summary>
        void IgnoreNextBeginEdit(bool val);

        void IgnoreNextEndCancelEdit(bool val);

        // Created because controls like the grid can fire these on a cell
        // or row basis and screws up our ObjectIsDirty and functionality
        // in our implementation.  These methods should be called from
        // our code instead of the default methods.
        void BeginEditManual();

        // Created because controls like the grid can fire these on a cell
        // or row basis and screws up our ObjectIsDirty and functionality
        // in our implementation.
        //These methods should be called from
        // our code instead of the default methods.
        void CancelEditManual();

        // Created because controls like the grid can fire these on a cell
        // or row basis and screws up our ObjectIsDirty and functionality
        // in our implementation.
        //These methods should be called from
        // our code instead of the default methods.
        void EndEditManual();


    }
}
