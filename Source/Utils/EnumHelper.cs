using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Ulee.Utils
{
    public static class EnumHelper
    {
        public static NameValue<T>[] GetNameValues<T>()
        {
            var result = new List<NameValue<T>>();

            var fis = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static);

            foreach (var fi in fis)
            {
                var das = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

                var value = (T)fi.GetValue(null);
                var name = "";
                if (das == null || das.Length == 0)
                    name = value.ToString();
                else
                    name = das[0].Description;

                result.Add(new NameValue<T>(name, value));
            }

            return result.ToArray();
        }

        public static string[] GetNames<T>()
        {
            var result = new List<string>();

            var fis = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static);

            foreach (var fi in fis)
            {
                var das = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

                var value = (T)fi.GetValue(null);
                var name = "";
                if (das == null || das.Length == 0)
                    name = value.ToString();
                else
                    name = das[0].Description;

                result.Add(name);
            }

            return result.ToArray();
        }

        public static IDictionary<TKey, string> GetNameValueMap<TKey>(Type type, string propertyName)
        {
            var result = new Dictionary<TKey, string>();

            var fis = type.GetFields(BindingFlags.Public | BindingFlags.Static);

            foreach (var fi in fis)
            {
                var das = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

                var ea = fi.GetCustomAttribute<EnumAttribute>();
                if (ea != null && ea.IgnorePropertyNames?.Contains(propertyName) == true)
                    continue;

                var value = (TKey)fi.GetValue(null);
                var name = "";
                if (das == null || das.Length == 0)
                    name = value.ToString();
                else
                    name = das[0].Description;

                result[value] = name;
            }

            return result;
        }

        public static string ToDescription(this Enum @this)
        {
            var type = @this.GetType();

            var att = type.GetField(@this.ToString(), BindingFlags.Public | BindingFlags.Static).GetCustomAttribute<DescriptionAttribute>();
            if (att == null)
                return @this.ToString();

            return att.Description;
        }
    }

    public class NameValue<T>
    {
        private string name;

        public string Name
        {
            get
            {
                return name ?? "";
            }

            set
            {
                name = value;
            }
        }
        public T Value { get; }

        public NameValue(string name, T value)
        {
            this.Name = name;
            this.Value = value;
        }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            NameValue<T> nameValue = obj as NameValue<T>;

            if (nameValue == null) return false;

            return Name.Equals(nameValue.Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }

    public class EnumAttribute : Attribute
    {
        public string[] IgnorePropertyNames { get; set; }
    }
}
