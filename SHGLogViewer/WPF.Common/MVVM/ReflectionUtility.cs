using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace WPF.Common
{
    /// <summary>
    /// Reflection Helper methods
    /// </summary>
    public class ReflectionUtility
    {
        /// <summary>
        /// Gives us a way to avoid Magic strings in our INPC properties
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        /// Example 
        /// Trace.WriteLine(ReflectionUtility.GetPropertyName(() => SomeProperty));
        public static string GetPropertyName<T>(Expression<Func<T>> expression)
        {
            MemberExpression body = (MemberExpression)expression.Body;
            return body.Member.Name;
        }

        //Trace.WriteLine(ReflectionUtility.GetPropertyName((Program s) => s.SomeProperty));
        public static string GetPropertyName<T, TReturn>(Expression<Func<T, TReturn>> expression)
        {
            MemberExpression body = (MemberExpression)expression.Body;
            return body.Member.Name;
        }
    }
}
