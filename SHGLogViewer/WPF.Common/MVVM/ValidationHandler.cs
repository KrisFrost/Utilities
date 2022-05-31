using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WPF.Common
{
    public class ValidationHandler
    {
        private Dictionary<string, string> BrokenRules { get; set; }

        public ValidationHandler()
        {
            BrokenRules = new Dictionary<string, string>();
        }

        public string this[string property]
        {
            get
            {
                return this.BrokenRules[property];
            }
        }

        internal int BrokenRuleCount
        {
            get { return BrokenRules.Count; }
        }

        public bool BrokenRuleExists(string property)
        {
            return BrokenRules.ContainsKey(property);
        }

        /// <summary>
        /// Returns any broken rules if they exist
        /// </summary>
        /// <param name="propertyName"></param>
        public IList<string> GetBrokenRules(string propertyName)
        {
            List<string> _errors = new List<string>();

            if (this.BrokenRules.ContainsKey(propertyName))
            {
                _errors.Add(this.BrokenRules[propertyName]);
            }

            return _errors;
        }

        public bool ValidateRule(string property, string message, Func<bool> ruleCheck)
        {
            if (!ruleCheck())
            {
                if (!this.BrokenRuleExists(property))
                    this.BrokenRules.Add(property, message);

                return false;
            }
            else
            {
                RemoveBrokenRule(property);
                return true;
            }
        }

        public void RemoveBrokenRule(string property)
        {
            if (this.BrokenRules.ContainsKey(property))
            {
                this.BrokenRules.Remove(property);
            }
        }
    }

}
