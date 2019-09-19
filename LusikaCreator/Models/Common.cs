using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using System.Xml.Serialization;

namespace TestApp.Models
{
    public static class Common
    {
        public static string Serialize<T>(this T value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            try
            {
                var xmlserializer = new XmlSerializer(typeof (T));
                var stringWriter = new StringWriter();
                using (var writer = XmlWriter.Create(stringWriter))
                {
                    xmlserializer.Serialize(writer, value);
                    return stringWriter.ToString();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred", ex);
            }
        }

        public static T FindParent<T>(dynamic sender) where T : FrameworkElement
        {
            FrameworkElement current = sender;
            T t;
            do
            {
                t = current as T;
                current = (FrameworkElement) VisualTreeHelper.GetParent(current);
            } while (t == null && current != null);
            return t;
        }

        public static IEnumerable<string> GetEnumDescriptions(Type type)
        {
            var names = Enum.GetNames(type);
            return (from name in names
                select type.GetField(name)
                into field
                from DescriptionAttribute fd in field.GetCustomAttributes(typeof (DescriptionAttribute), true)
                select fd.Description).ToList();
        }

        public static string GetEnumDescription(dynamic value)
        {
            var fi = value.GetType().GetField(value.ToString());
            var attributes = (DescriptionAttribute[]) fi.GetCustomAttributes(typeof (DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : value.ToString();
        }

        public static bool IsSameName(string targerName, string comparedName) =>
            targerName != null && comparedName != null &&
            string.Equals(targerName.Trim(), comparedName.Trim(), StringComparison.CurrentCultureIgnoreCase);


        public static string GetAppDataPath => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        public static string GetInstallPath => AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// Возвращает список тегов, содержащихся в строке
        /// </summary>
        public static List<string> GetTextTags(string text)
        {
            // "работающ{#его/ей} в {#Место раб.}" --> ["{#его/ей}", "{#Место раб.}"]
            if (string.IsNullOrEmpty(text)) return new List<string>();

            var regex = new Regex(@"{[#,$,@](.[^}]*)}");
            var tags = regex.Matches(text).Cast<Match>().Select(match => match.Value).ToList();
            return tags.Count == 0 ? new List<string>() : tags;
        }


        //public static List<string> GetTextTags(string text)
        //{
        //    var ret = new List<string>();
        //    if (string.IsNullOrEmpty(text)) return ret;
        //    var from = 0;

        //    int startIndex;
        //    do
        //    {
        //        startIndex = text.IndexOf("{#", from, StringComparison.Ordinal);
        //        if (startIndex != -1)
        //        {
        //            from = startIndex + 2;
        //            var endIndex = text.IndexOf("}", @from, StringComparison.Ordinal);

        //            if (endIndex != -1)
        //            {
        //                ret.Add("{#" + text.Substring(from, endIndex - from) + "}");
        //                from = endIndex + 1;
        //            }
        //        }
        //    } while (startIndex != -1);

        //    return ret;
        //}


        public static bool IsTextHasTags(string text)
            => !string.IsNullOrEmpty(text) && GetTextTags(text).Count > 0;

        public static string Reverse(string s)
        {
            var charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
    }
}