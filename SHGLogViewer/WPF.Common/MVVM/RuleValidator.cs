using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace WPF.Common
{
    // Look here for info regarding INotifyDataErrorInfo.
    // http://msdn.microsoft.com/en-us/library/system.componentmodel.inotifydataerrorinfo%28v=vs.95%29.aspx
    public abstract class RuleValidator : INPCBase, INotifyDataErrorInfo, INotifyPropertyChanged
    {
        #region Data
        //private List<Rule> rules = new List<Rule>();
        public ValidationHandler ValidationHandler = new ValidationHandler();
        //readonly Dictionary<string, List<string> _currentErrors;

        /// <summary>
        /// Gets a value indicating whether or not this domain object is valid. 
        /// </summary>
        public virtual bool IsValid
        {
            get
            {
                //return String.IsNullOrEmpty(this.Error);
                return ValidationHandler.BrokenRuleCount == 0;
            }
        }

        #endregion

        #region IDataErrorInfo Members

        #endregion

        #region Debugging Aides



        /// <summary>
        /// Returns whether an exception is thrown, or if a Debug.Fail() is used
        /// when an invalid property name is passed to the VerifyPropertyName method.
        /// The default value is false, but subclasses used by unit tests might 
        /// override this property's getter to return true.
        /// </summary>
        protected virtual bool ThrowOnInvalidPropertyName { get; private set; }

        #endregion // Debugging Aides

        #region INotifyDataErrorInfo

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName)
        {
            return this.ValidationHandler.GetBrokenRules(propertyName);

        }

        public bool HasErrors
        {
            get { return this.ValidationHandler.BrokenRuleCount != 0; }
        }

        #endregion
    }
}
