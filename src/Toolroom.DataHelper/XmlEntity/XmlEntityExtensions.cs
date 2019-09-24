using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Linq;

namespace Toolroom.DataHelper
{
    public static class XmlEntityExtensions
    {
        private const string RootNodename = "Values";
        private const string ValueNodename = "Value";
        private const string KeyAttributename = "Key";

        public static string GetXmlValuesFromProperties(this IXmlEntity entity, Type xmlMappedAttributeType)
        {
            var properties = entity.GetXmlMappedProperties(xmlMappedAttributeType);
            XDocument document = new XDocument(new XElement(RootNodename));
            foreach (var propertyInfo in properties)
            {
                document.Set(entity, propertyInfo);
            }
            var rootNode = document.Root;
            if (rootNode == null || !rootNode.Elements().Any())
            {
                return null;
            }
            return document.ToString(SaveOptions.DisableFormatting);
        }

        public static void FillPropertiesFromXmlValues(this IXmlEntity entity, Type xmlMappedAttributeType, string xmlValues)
        {
            var document = xmlValues == null ? new XDocument(new XElement(RootNodename)) : XDocument.Parse(xmlValues);

            var properties = entity.GetXmlMappedProperties(xmlMappedAttributeType);
            foreach (var propertyInfo in properties)
            {
                propertyInfo.SetValue(entity, document.Get(propertyInfo));
            }
        }

        private static IEnumerable<PropertyInfo> GetXmlMappedProperties(this IXmlEntity entity, Type xmlMappedAttributeType)
        {
            return entity.GetType().GetRuntimeProperties()
                .Where(prop => prop.IsDefined(xmlMappedAttributeType, false));
        }

        #region Generic
        public static object Get<T, TValue>(this XDocument document, Expression<Func<T, TValue>> propertySelector) 
            => Get(document, propertySelector.Body);

        public static object Get(this XDocument document, Expression propertyExpression)
        {
            switch (propertyExpression)
            {
                case MemberExpression memberExpression when memberExpression.Member.MemberType == MemberTypes.Property:
                    return Get(document, (PropertyInfo) memberExpression.Member);
                case MemberExpression _:
                    throw new Exception($"MemberExpressions of types other than other than {MemberTypes.Property} are not supported.");
                case UnaryExpression unaryExpression when unaryExpression.NodeType == ExpressionType.Convert:
                    return Get(document, unaryExpression.Operand);
                case UnaryExpression _:
                    throw new Exception($"UnaryExpression of types other than other than {ExpressionType.Convert} are not supported.");
                default:
                    throw new Exception($"Expressions of types other than other than {nameof(MemberExpression)}, {nameof(UnaryExpression)} are not supported.");
            }
        }

        public static object Get(this XDocument document, PropertyInfo propertyInfo)
        {
            var propertyName = propertyInfo.Name;
            Type type = propertyInfo.PropertyType;
            try
            {
                if (type == typeof(string))
                    return document.GetString(propertyName);
                if (type == typeof(IEnumerable<string>) || type == typeof(IList<string>))
                    return document.GetStrings(propertyName);
                if (type == typeof(int))
                    return document.GetInt(propertyName) ?? default(int);
                if (type == typeof(int?))
                    return document.GetInt(propertyName);
                if (type == typeof(bool))
                    return document.GetBool(propertyName) ?? default(bool);
                if (type == typeof(bool?))
                    return document.GetBool(propertyName);
                if (type == typeof(DateTime))
                    return document.GetDateTime(propertyName) ?? default(DateTime);
                if (type == typeof(DateTime?))
                    return document.GetDateTime(propertyName);
                if (type == typeof(DateTimeOffset))
                    return document.GetDateTimeOffset(propertyName) ?? default(DateTimeOffset);
                if (type == typeof(DateTimeOffset?))
                    return document.GetDateTimeOffset(propertyName);
                if (type == typeof(IEnumerable<int>) || type == typeof(IList<int>))
                    return document.GetInts(propertyName);
                if (type == typeof(double))
                    return document.GetDouble(propertyName) ?? default(double);
                if (type == typeof(double?))
                    return document.GetDouble(propertyName);
                if (type == typeof(IEnumerable<double>) || type == typeof(IList<double>))
                    return document.GetDoubles(propertyName);
                if (type == typeof(IDictionary<string, double>))
                    return document.GetDictionaryStringDouble(propertyName);
                if (type == typeof(IDictionary<string, string>))
                    return document.GetDictionaryStringString(propertyName);
            }
            catch (InvalidCastException)
            {
                //TODO return default();
            }
            throw new NotSupportedException($"Type {type.FullName} is not supported");
        }

        private static void Set(this XDocument document, IXmlEntity entity, PropertyInfo propertyInfo)
        {
            var propertyName = propertyInfo.Name;
            Type type = propertyInfo.PropertyType;
            try
            {
                if (type == typeof(string))
                    document.SetString((string)propertyInfo.GetValue(entity), propertyName);
                if (type == typeof(int) || type == typeof(int?))
                    document.SetInt((int?)propertyInfo.GetValue(entity), propertyName);
                if (type == typeof(double) || type == typeof(double?))
                    document.SetDouble((double?)propertyInfo.GetValue(entity), propertyName);
                if (type == typeof(bool) || type == typeof(bool?))
                    document.SetBool((bool?)propertyInfo.GetValue(entity), propertyName);
                if (type == typeof(DateTime) || type == typeof(DateTime?))
                    document.SetDateTime((DateTime?)propertyInfo.GetValue(entity), propertyName);
                if (type == typeof(DateTimeOffset) || type == typeof(DateTimeOffset?))
                    document.SetDateTimeOffset((DateTimeOffset?)propertyInfo.GetValue(entity), propertyName);
                if (type == typeof(IEnumerable<string>) || type == typeof(IList<string>))
                    document.SetStrings((IEnumerable<string>)propertyInfo.GetValue(entity), propertyName);
                if (type == typeof(IEnumerable<int>) || type == typeof(IList<int>))
                    document.SetInts((IEnumerable<int>)propertyInfo.GetValue(entity), propertyName);
                if (type == typeof(IEnumerable<double>) || type == typeof(IList<double>))
                    document.SetDoubles((IEnumerable<double>)propertyInfo.GetValue(entity), propertyName);
                if (type == typeof(IDictionary<string, double>))
                    document.SetDictionaryStringDouble((IDictionary<string, double>)propertyInfo.GetValue(entity), propertyName);
                if (type == typeof(IDictionary<string, string>))
                    document.SetDictionaryStringString((IDictionary<string, string>)propertyInfo.GetValue(entity), propertyName);

                return;
            }
            catch (InvalidCastException)
            {
                //TODO
            }
            throw new NotSupportedException($"Type {type.FullName} is not supported");
        }
        #endregion

        #region String
        private static void SetString(this XDocument document, string value, string propertyName)
        {
            if (string.IsNullOrWhiteSpace(value))
                document.RemoveElement(propertyName);
            else
            {
                var propertyElement = document.AddOrGetPropertyElement(propertyName);
                propertyElement.SetValue(value);
            }
        }
        private static string GetString(this XDocument document, string propertyName)
        {
            var element = document.GetElement(propertyName);
            return element?.Value;
        }
        private static void SetStrings(this XDocument document, IEnumerable<string> values, string propertyName)
        {
            document.RemoveElement(propertyName);
            if (values == null)
                return;

            XElement element = null;
            foreach (var value in values)
            {
                if (element == null)
                {
                    element = document.AddOrGetPropertyElement(propertyName);
                }
                element.Add(new XElement(ValueNodename, value));
            }
        }

        private static IEnumerable<string> GetStrings(this XDocument document, string propertyName)
        {
            var elements = document.GetValueElements(propertyName)?.ToArray();
            if (elements == null || !elements.Any())
                return null;
            List<string> ret = new List<string>(elements.Length);
            foreach (var element in elements)
            {
                ret.Add(element.Value);
            }
            return ret;
        }
        #endregion

        #region Boolean
        private static void SetBool(this XDocument document, bool? value, string propertyName)
        {
            if (value.HasValue)
                document.SetString(value.Value ? "1" : "0", propertyName);
            else
                document.RemoveElement(propertyName);
        }
        private static bool? GetBool(this XDocument document, string propertyName)
        {
            var element = document.GetElement(propertyName);
            if (element == null)
                return null;
            if (!int.TryParse(element.Value, out var val))
                return null;
            return val != 0;
        }
        #endregion

        #region DateTime
        private static void SetDateTime(this XDocument document, DateTime? value, string propertyName)
        {
            if (value.HasValue)
            {
                document.SetString(value.Value.ToString("O"), propertyName);
            }
            else
            {
                document.RemoveElement(propertyName);
            }
        }
        private static DateTime? GetDateTime(this XDocument document, string propertyName)
        {
            var element = document.GetElement(propertyName);
            if (element == null)
                return null;
            if (!DateTime.TryParse(element.Value, null, DateTimeStyles.RoundtripKind, out var val))
                return null;
            return val;
        }
        #endregion

        #region DateTimeOffset
        private static void SetDateTimeOffset(this XDocument document, DateTimeOffset? value, string propertyName)
        {
            if (value.HasValue)
            {
                document.SetString(value.Value.ToString("O"), propertyName);
            }
            else
            {
                document.RemoveElement(propertyName);
            }
        }
        private static DateTimeOffset? GetDateTimeOffset(this XDocument document, string propertyName)
        {
            var element = document.GetElement(propertyName);
            if (element == null)
                return null;
            if (!DateTimeOffset.TryParse(element.Value, null, DateTimeStyles.RoundtripKind, out var val))
                return null;
            return val;
        }
        #endregion

        #region Int
        private static void SetInt(this XDocument document, int? value, string propertyName)
        {
            if (value.HasValue && value.Value != default(int))
                document.SetString(value.Value.ToString(CultureInfo.InvariantCulture), propertyName);
            else
                document.RemoveElement(propertyName);
        }
        private static int? GetInt(this XDocument document, string propertyName)
        {
            var element = document.GetElement(propertyName);
            if (element == null)
                return null;
            if (!int.TryParse(element.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var ret))
                return null;
            return ret;
        }

        private static void SetInts(this XDocument document, IEnumerable<int> values, string propertyName)
        {
            document.SetStrings(values?.Select(v => v.ToString(CultureInfo.InvariantCulture)), propertyName);
        }

        private static IEnumerable<int> GetInts(this XDocument document, string propertyName)
        {
            var elements = document.GetValueElements(propertyName)?.ToArray();
            if (elements == null || !elements.Any())
                return null;
            List<int> ret = new List<int>(elements.Length);
            foreach (var element in elements)
            {
                if (!int.TryParse(element.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var val))
                    continue;
                ret.Add(val);
            }
            return ret;
        }
        #endregion

        #region Double
        private static void SetDouble(this XDocument document, double? value, string propertyName)
        {
            const double tolerance = 0.00000001;
            if (value.HasValue && Math.Abs(value.Value - default(double)) > tolerance)
                document.SetString(value.Value.ToString(CultureInfo.InvariantCulture), propertyName);
            else
                document.RemoveElement(propertyName);
        }
        private static double? GetDouble(this XDocument document, string propertyName)
        {
            var element = document.GetElement(propertyName);
            if (element == null)
                return null;
            if (!double.TryParse(element.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var ret))
                return null;
            return ret;
        }

        private static void SetDoubles(this XDocument document, IEnumerable<double> values, string propertyName)
        {
            document.SetStrings(values?.Select(v => v.ToString(CultureInfo.InvariantCulture)), propertyName);
        }

        private static IEnumerable<double> GetDoubles(this XDocument document, string propertyName)
        {
            var elements = document.GetValueElements(propertyName)?.ToArray();
            if (elements == null || !elements.Any())
                return null;
            List<double> ret = new List<double>(elements.Length);
            foreach (var element in elements)
            {
                if (!double.TryParse(element.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var val))
                    continue;
                ret.Add(val);
            }
            return ret;
        }
        #endregion

        #region IDictionary<string, string>
        private static void SetDictionaryStringString(this XDocument document, IDictionary<string, string> values, string propertyName)
        {
            document.RemoveElement(propertyName);
            if (values == null || !values.Any())
                return;

            var element = document.AddOrGetPropertyElement(propertyName);
            foreach (var value in values)
            {
                var e = new XElement(ValueNodename, value.Value);
                e.SetAttributeValue(KeyAttributename, value.Key);
                element.Add(e);
            }
        }
        private static IDictionary<string, string> GetDictionaryStringString(this XDocument document, string propertyName)
        {
            var elements = document.GetValueElements(propertyName)?.ToArray();
            if (elements == null || !elements.Any())
                return null;
            Dictionary<string, string> ret = new Dictionary<string, string>(elements.Length);
            foreach (var element in elements)
            {
                var keyAttr = element.Attribute("Key");
                if (keyAttr != null && !ret.ContainsKey(keyAttr.Value))
                    ret.Add(keyAttr.Value, element.Value);
            }
            return ret;
        }
        #endregion

        #region IDictionary<string, double>
        private static void SetDictionaryStringDouble(this XDocument document, IDictionary<string, double> values, string propertyName)
        {
            if (values != null && values.Any())
                document.SetDictionaryStringString(values.ToDictionary(v => v.Key, v => v.Value.ToString(CultureInfo.InvariantCulture)), propertyName);
            else
                document.RemoveElement(propertyName);
        }
        private static IDictionary<string, double> GetDictionaryStringDouble(this XDocument document, string propertyName)
        {
            var dict = document.GetDictionaryStringString(propertyName);
            if (dict == null || !dict.Any())
                return null;

            var ret = new Dictionary<string, double>(dict.Count);
            foreach (var entry in dict)
            {
                if (!double.TryParse(entry.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                    continue;
                ret.Add(entry.Key, value);
            }
            return ret;
        }
        #endregion

        #region Helpers
        private static XElement GetElement(this XDocument document, string propertyName)
        {
            return document.Root?.Element(propertyName);
        }

        private static IEnumerable<XElement> GetValueElements(this XDocument document, string propertyName)
        {
            var element = document.GetElement(propertyName);
            return element?.Elements(ValueNodename);
        }

        private static XElement AddOrGetPropertyElement(this XDocument document, string property)
        {
            var propertyElement = document.Root?.Element(property);
            if (propertyElement == null)
            {
                propertyElement = new XElement(property);
                document.Root?.Add(propertyElement);
            }
            return propertyElement;
        }

        private static void RemoveElement(this XDocument document, string property)
        {
            var element = document.GetElement(property);
            element?.Remove();
        }
        #endregion
    }
}