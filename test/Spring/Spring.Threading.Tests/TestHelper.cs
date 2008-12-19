using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using System.Text.RegularExpressions;

namespace Spring
{
    public static class TestHelper
    {
        /// <summary>
        /// Runs the static method.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <param name="strMethod">The STR method.</param>
        /// <param name="aobjParams">The aobj params.</param>
        /// <returns></returns>
        public static object RunStaticMethod(Type t, string strMethod,
            object[] aobjParams)
        {
            BindingFlags eFlags =
                BindingFlags.Static |
                BindingFlags.Public |
                BindingFlags.NonPublic;

            try
            {
                return RunMethod(t, strMethod, null, aobjParams, eFlags);
            }
            catch (TargetInvocationException ex)
            {
                Exception inner = ex.InnerException;
                throw inner ?? ex;
            }

        } //end of method</PRE>

        /// <summary>
        /// Runs the instance method.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <param name="strMethod">The STR method.</param>
        /// <param name="objInstance">The obj instance.</param>
        /// <param name="aobjParams">The aobj params.</param>
        /// <returns></returns>
        public static object RunInstanceMethod(Type t, string strMethod,
            object objInstance, params object[] aobjParams)
        {
            BindingFlags eFlags =
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic;
            try
            {
                return RunMethod(t, strMethod, objInstance, aobjParams, eFlags);
            }
            catch (TargetInvocationException ex)
            {
                Exception inner = ex.InnerException;
                throw inner ?? ex;
            }
        } //end of method

        public static object GetInstanceProperty(Type t, string propertyName, object instance)
        {
            return RunInstanceMethod(t, "get_" + propertyName, instance);
        }

        public static void SetInstanceProperty(Type t, string propertyName,
            object instance, object value)
        {
            RunInstanceMethod(t, "set_" + propertyName, instance, value);
        }

        /// <summary>
        /// Run the non-public instance method.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="methodName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static object RunInstanceMethod(object instance, string methodName,
            params object[] parameters)
        {
            return RunInstanceMethod(instance.GetType(), methodName, instance, parameters);
        }

        public static object GetInstanceProperty(object instance, string propertyName)
        {
            return GetInstanceProperty(instance.GetType(), propertyName, instance);
        }

        public static void SetInstanceProperty(object instance, string propertyName, object value)
        {
            SetInstanceProperty(instance.GetType(), propertyName, instance, value);
        }

        /// <summary>
        /// Gets the private field.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <param name="fldName">Name of the FLD.</param>
        /// <returns></returns>
        public static FieldInfo GetPrivateField(Type t, string fldName)
        {
            BindingFlags eFlags =
                BindingFlags.Instance |
                BindingFlags.GetField |
                BindingFlags.IgnoreCase |
                BindingFlags.NonPublic;

            FieldInfo fi = t.GetField(fldName, eFlags);

            if (fi == null)
            {
                throw new ArgumentException("There is no field '" +
                 fldName + "' for type '" + t + "'.");
            }

            return fi;
        }

        /// <summary>
        /// Runs the method.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <param name="strMethod">The STR method.</param>
        /// <param name="objInstance">The obj instance.</param>
        /// <param name="aobjParams">The aobj params.</param>
        /// <param name="eFlags">The e flags.</param>
        /// <returns></returns>
        private static object RunMethod(Type t, string strMethod,
            object objInstance, object[] aobjParams, BindingFlags eFlags)
        {
            MethodInfo m = t.GetMethod(strMethod, eFlags);
            if (m == null)
            {
                throw new ArgumentException("There is no method '" +
                 strMethod + "' for type '" + t + "'.");
            }

            return m.Invoke(objInstance, aobjParams);

        } //end of method</PRE>

        /// <summary>
        /// Convenient utility mehtod used as an alternative to the 
        /// <see cref="ExpectedExceptionAttribute"/>. This a method allows
        /// to test multiple exception in one test case.
        /// </summary>
        /// <remarks>
        /// <example>
        /// Example code to assert two similar exceptions.
        /// <code language="c#">
        ///    TestHelper.AssertException&lt;ArgumentNullException>(
        ///        delegate { service.GetDepartmentCodes(null); });
        ///
        ///    TestHelper.AssertException&lt;ArgumentNullException>(
        ///        delegate { service.GetCategoryCodes(null, 0, 0); });
        /// </code>
        /// </example>
        /// </remarks>
        /// <typeparam name="T">type of exception</typeparam>
        /// <param name="call">the code to be executed that throws exception</param>
        public static void AssertException<T>(ThreadStart call)
            where T: Exception
        {
            AssertException<T>(call, MessageMatch.Exact, null);
        }

        /// <summary>
        /// Convenient utility mehtod used as an alternative to the 
        /// <see cref="ExpectedExceptionAttribute"/>. This a method allows
        /// to test multiple exception in one test case.
        /// </summary>
        /// <remarks>
        /// <example>
        /// Example code to assert two similar exceptions.
        /// <code language="c#">
        ///    TestHelper.AssertException&lt;ArgumentNullException>(
        ///        delegate { service.GetDepartmentCodes(null); });
        ///
        ///    TestHelper.AssertException&lt;ArgumentNullException>(
        ///        delegate { service.GetCategoryCodes(null, 0, 0); }
        ///        MessageMatch.Contains, "subjectCode");
        /// </code>
        /// </example>
        /// </remarks>
        /// <typeparam name="T">type of exception</typeparam>
        /// <param name="call">The code to be executed that throws exception</param>
        /// <param name="match">To specify how the message is matched</param>
        /// <param name="message">The message to match. <see langword="null"/>
        /// or empty string will not cause assertion.</param>
        public static void AssertException<T>(ThreadStart call, MessageMatch match, string message)
            where T : Exception
        {
            bool hasException = false;
            try
            {
                call();
            }
            catch (T e)
            {
                hasException = true;
                if (!String.IsNullOrEmpty(message))
                {
                    switch (match)
                    {
                        case MessageMatch.Regex:
                            Regex regex = new Regex(message);
                            if(!regex.IsMatch(e.Message))
                            {
                                Assert.Fail(string.Format("Expected regex {0} does not match actual exception message {1}.", 
                                    message, e.Message));
                            }
                            break;
                        case MessageMatch.Contains:
                            if(!e.Message.Contains(message))
                            {
                                Assert.Fail(string.Format("Expected string '{0}' is not found in actual exception message: '{1}'.", message, e.Message));
                            }
                            break;
                        default:
                            Assert.AreEqual(message, e.Message, "Exception ");
                            break;
                    }
                }
            }

            Assert.IsTrue(hasException, "Expecting " + typeof(T).FullName + ", but no exception was throw.");
        }

        /// <summary>
        /// Assert the <paramref name="actual"/> <see cref="IEnumerator"/> 
        /// yields the elements as <paramref name="expected"/>.
        /// </summary>
        /// <param name="expected">Enumerator of expected elements.</param>
        /// <param name="actual">The actual enumerator to be verified.</param>
        /// <exception cref="AssertionException">
        /// When actual has different, less or more element as expected.
        /// </exception>
        public static void AssertEnumeratorEquals(IEnumerator expected, IEnumerator actual)
        {
            if (expected==null && actual==null) return;

            Assert.IsNotNull(expected, "Expecting a null enumerator, but the actual is not null.");
            Assert.IsNotNull(actual, "Expecting a non-null enumerator, but the actual is null.");

            while (expected.MoveNext())
            {
                Assert.IsTrue(actual.MoveNext(), "actual has too less elements.");
                Assert.AreEqual(expected.Current, actual.Current);
            }
            Assert.IsFalse(actual.MoveNext(), "actual has too many elements.");
        }

        /// <summary>
        /// Assert the <paramref name="actual"/> <see cref="IEnumerable"/> 
        /// contains the elements as <paramref name="expected"/>.
        /// </summary>
        /// <param name="expected">Enumerable that contains the expected elements.</param>
        /// <param name="actual">The actual enumerable to be verified.</param>
        /// <exception cref="AssertionException">
        /// When actual has different, less or more element as expected.
        /// </exception>
        public static void AssertEnumerableEquals(IEnumerable expected, IEnumerable actual)
        {
            if (expected == null && actual == null) return;

            Assert.IsNotNull(expected, "Expecting a null enumerable, but the actual is not null.");
            Assert.IsNotNull(actual, "Expecting a non-null enumerable, but the actual is null.");

            AssertEnumeratorEquals(expected.GetEnumerator(), actual.GetEnumerator());
        }

        #region Helpers

        #region Property setters
        public static void SetProperties(object o, string nvs)
        {
            foreach (string nv in nvs.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] sp = nv.Split(new char[] { '=' });
                // Add some keywords.
                sp[1] = sp[1].Replace("{TODAY}", DateTime.Now.ToShortDateString());
                sp[1] = sp[1].Replace("{NOW}", DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());
                sp[1] = sp[1].Replace("{TIME}", DateTime.Now.ToShortTimeString());
                SetProperty(o, sp[0], sp[1]);
            }
        }

        public static void SetProperty(object o, string name, string value)
        {
            if (o != null)
            {
                PropertyDescriptor pd = TypeDescriptor.GetProperties(o.GetType())[name];
                if (pd != null)
                {
                    object value1 = value;
                    if (!typeof(string).Equals(pd.PropertyType) && pd.Converter != null)
                    {
                        value1 = pd.Converter.ConvertFromString(value);
                    }
                    pd.SetValue(o, value1);
                }
                else
                {
                    throw new Exception(string.Format("Unable to find property by name {0} on type {1}", name, o.GetType().FullName));
                }
            }
        }
        #endregion

        #region Getters
        public static bool VerifyProperties(object o, string nvs)
        {
            bool result = false;
            if (o != null)
            {
                foreach (string nv in nvs.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] sp = nv.Split(new char[] { '=' });
                    // Add some keywords.
                    sp[1] = sp[1].Replace("{TODAY}", DateTime.Now.ToShortDateString());
                    sp[1] = sp[1].Replace("{NOW}", DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());
                    sp[1] = sp[1].Replace("{TIME}", DateTime.Now.ToShortTimeString());

                    PropertyDescriptor pd = TypeDescriptor.GetProperties(o.GetType())[sp[0]];
                    if (pd != null)
                    {
                        object valueOnTheObject = pd.GetValue(o);
                        object valueInString = sp[1];
                        if (!typeof(string).Equals(pd.PropertyType) && pd.Converter != null)
                        {
                            valueInString = pd.Converter.ConvertFromString(sp[1]);
                        }
                        if (sp[1].Trim().ToLower().StartsWith("null") && valueOnTheObject == null)
                        {
                            result = true;
                        }
                        else
                        {
                            result = valueOnTheObject.Equals(valueInString);
                        }
                    }
                    else
                    {
                        throw new Exception(string.Format("Unable to find property by name {0} on type {1}", sp[0], o.GetType().FullName));
                    }
                    if (!result) break;
                }
            }
            else if (nvs.Trim().ToLower().StartsWith("null"))
            {
                result = true;
            }
            return result;
        }

        public static object GetProperty(object o, string name)
        {
            object result = null;
            if (o != null)
            {
                PropertyDescriptor pd = TypeDescriptor.GetProperties(o.GetType())[name];
                if (pd != null)
                {
                    result = pd.GetValue(o);
                }
                else
                {
                    throw new Exception(string.Format("Unable to find property by name {0} on type {1}", name, o.GetType().FullName));
                }
            }
            return result;
        }
        #endregion

        #endregion

    }
}
