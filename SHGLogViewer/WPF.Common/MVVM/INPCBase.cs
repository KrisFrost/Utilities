using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace WPF.Common
{
    /// <summary>
    /// Base INotifyPropertyChange class.
    /// </summary>
    public abstract class INPCBase : INotifyPropertyChanged, IParentablePropertyExposer
    {
        #region Private Fields

        private bool _isEditable;
        private IParentablePropertyExposer _parent = null;
        private PropertyChangedEventArgs _parentPropertyChangeArgs = null;
        private Stack<string> _dirtyProperties;

        /// <summary>
        /// Will be dirty if any Property value of the object has changed.
        /// </summary>
        private bool _objectIsDirty;

        #endregion

        #region ctor

        public INPCBase()
        {
            IsEditable = false;
            DirtyProperties = new Stack<string>();
        }

        #endregion

        // Made this public in case there is a need to see what properties are dirty.
        public Stack<string> DirtyProperties
        {
            get { return _dirtyProperties; }
            private set
            {
                if (_dirtyProperties != value)
                {
                    _dirtyProperties = value;
                    RaisePropertyChanged(() => DirtyProperties);
                }
            }
        }

        protected internal void NotifyParentPropertyChanged()
        {
            if (_parent == null || _parentPropertyChangeArgs == null)
                return;

            //notify all delegates listening to DataWrapper<T> parent objects PropertyChanged
            //event
            Delegate[] subscribers = _parent.GetINPCSubscribers();
            if (subscribers != null)
            {
                foreach (PropertyChangedEventHandler d in subscribers)
                {
                    d(_parent, _parentPropertyChangeArgs);
                }
            }
        }

        /// <summary>
        /// Signals if a Property has changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Used to determine if the Dirty functionality will be used in this class.
        /// For properties to be marked as Dirty, IsEditable must be set to true.
        /// See RuleValidator for an example.
        /// </summary>
        public bool IsEditable
        {
            get { return _isEditable; }
            set
            {
                if (_isEditable != value)
                {
                    _isEditable = value;
                    NotifyParentPropertyChanged();
                    RaisePropertyChanged(ReflectionUtility.GetPropertyName(() => IsEditable));
                }
            }
        }

        public virtual bool ObjectIsDirty
        {
            get { return _objectIsDirty; }
            set
            {
                if (_objectIsDirty != value)
                {
                    _objectIsDirty = value;
                    NotifyParentPropertyChanged();
                    RaisePropertyChanged(() => ObjectIsDirty);
                }
            }
        }


        /// <summary>
        /// Use to determine if a Properties value has changed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyExpression">Use a lambda expression to pass in the Property and this method will determine the string name.
        /// Ex. IsDirty(a => a.PropertyName) 
        /// </param>
        /// <returns></returns>
        public virtual bool PropertyIsDirty<T>(Expression<Func<T>> propertyExpression)
        {

            if (propertyExpression.Body.NodeType == ExpressionType.MemberAccess)
            {
                var memberExpr = propertyExpression.Body as MemberExpression;
                string propertyName = memberExpr.Member.Name;
                return PropertyIsDirty(propertyName);
            }
            else
                return false;
        }

        /// <summary>
        /// Use to determine if a Properties value has changed.
        /// </summary>
        /// <param name="propertyName">String name of the property.</param>
        /// <returns></returns>
        public virtual bool PropertyIsDirty(string propertyName)
        {
            return DirtyProperties.Contains(propertyName);
        }

        /// <summary>
        /// Use this version to pass a Property Name in and have it create the string.
        /// Example RaisePropertyChanged(() => Name);
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyExpression"></param>


        public void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            if (propertyExpression.Body.NodeType == ExpressionType.MemberAccess)
            {
                var memberExpr = propertyExpression.Body as MemberExpression;
                string propertyName = memberExpr.Member.Name;
                this.RaisePropertyChanged(propertyName);
            }
        }

        /// <summary>
        /// Use this version of RaisePropertyChange if you wish to pass in a property name by String.
        /// </summary>
        /// <param name="propertyName"></param>
        public void RaisePropertyChanged(string propertyName)
        {

            // We only want to set the ObjectIsDirty flag if IsEditable equal true.
            // A property should only be considered dirty if changed by a user.
            if (IsEditable && propertyName != ReflectionUtility.GetPropertyName(() => IsEditable) && propertyName != ReflectionUtility.GetPropertyName(() => ObjectIsDirty))
            {
                if (!DirtyProperties.Any(a => a == propertyName))
                {
                    DirtyProperties.Push(propertyName);
                }

                ObjectIsDirty = true;
            }

            NotifyParentPropertyChanged();

            // If any object has subscribed to the PropertyChanged.
            if (PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #region IParentablePropertyExposer
        /// <summary>
        /// Returns the list of delegates that are currently subscribed for the
        /// <see cref="System.ComponentModel.INotifyPropertyChanged">INotifyPropertyChanged</see>
        /// PropertyChanged event
        /// </summary>
        public Delegate[] GetINPCSubscribers()
        {
            return PropertyChanged == null ? null : PropertyChanged.GetInvocationList();
        }

        #endregion
    }

}
