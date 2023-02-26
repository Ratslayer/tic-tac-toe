using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
namespace BB
{
    public static class StringUtils
    {
        public const string NO_STRING = "[NO STRING]";
        private const string CapitalWordSplitRegex = @"([A-Z]+[^A-Z]+)";
        public static string[] SplitIntoCapitalizedWords(string str)
        {
            var result = SimpleGroupSplit(str, CapitalWordSplitRegex, RegexOptions.Singleline);
            return result;
        }
        public static string Join<T>(IEnumerable<T> objects, Func<T, string> stringGetter, string separator)
        {
            var strs = objects.ToArray().Convert((i, obj) => stringGetter(obj));
            return Join(strs, separator);
        }
        public static string Join(this string[] strs, string separator = "")
        {
            var result = strs.Length > 0 ? strs[0] : "";
            for (int i = 1; i < strs.Length; i++)
                result += separator + strs[i];
            return result;
        }
        public static string Remove(this string from, string str)
        {
            return from.Replace(str, "");
        }
        public static bool ContainsCaseless(this string str1, string str2)
        {
            return str1.ToLower().Contains(str2.ToLower());
        }
        public static bool ContainsAnyCaseless(this string str1, params string[] strs)
        {
            return str1.ToLower().ContainsAny(strs);
        }
        public static string GetPath(params string[] folders)
        {
            string result;
            if (folders.Length > 0)
            {
                result = folders[0];
                for (int i = 1; i < folders.Length; i++)
                {
                    var folder = folders[i];
                    if (!folder.NoE())
                        result = $"{result}/{folder}";
                }
            }
            else
                result = "";
            return result;
        }
        public static bool SafeEqualsCaseless(this string left, string right)
        {
            bool result = left != null && right != null && left.Trim().ToLower() == right.Trim().ToLower() || left == null && right == null;
            return result;
        }
        public static bool StartsWithCaseless(this string left, string right)
        {
            bool result = left != null && right != null && left.Trim().ToLower().StartsWith(right.Trim().ToLower());
            return result;
        }
        public static bool EndsWithCaseless(this string left, string right)
        {
            bool result = left != null && right != null && left.Trim().ToLower().EndsWith(right.Trim().ToLower());
            return result;
        }
        public static bool EndsWithAny(this string str, params string[] values)
        {
            return values.AnyTrue(str.EndsWith);
        }
        public static bool EndsWithAnyCaseless(this string str, params string[] values)
        {
            return values.AnyTrue(str.EndsWithCaseless);
        }
        public static string Capitalize(this string input)
        {
            return input.First().ToString().ToUpper() + input.Substring(1);
        }
        public static string ReplacePattern(this string input, string pattern, string replacement)
        {
            Regex regex = new Regex(pattern);
            var result = regex.Replace(input, replacement);
            return result;
        }
        public static string GetPattern(this string input, string pattern)
        {
            Regex regex = new Regex(pattern);
            var match = regex.Match(input);
            var result = match.Success ? match.Groups[1].Value : "";
            return result;
        }
        public static string[] SplitCapitalCaseNoDigits(this string str)
        {
            var result = SimpleGroupSplit(str, @"([A-Z][a-z]*)", RegexOptions.None);
            return result;
        }
        public static string[] SimpleGroupSplit(this string input, string pattern, RegexOptions options = RegexOptions.None)
        {
            MatchCollection matchList = Regex.Matches(input, pattern);
            var result = matchList.Cast<Match>().Cast<Match>()
                    // flatten to single list
                    .SelectMany(o =>
                        // linq-ify
                        o.Groups.Cast<Capture>()
                            // don't need the pattern
                            .Skip(1)
                            // select what you wanted
                            .Select(c => c.Value)).ToArray();
            return result;
        }
        public static string[] SplitIntoLines(this string input)
        {
            return input.Split(Environment.NewLine.ToCharArray());
        }
        public static bool IsEmpty(this string value)
        {
            var result = value == "";
            return result;
        }
        public static List<string> ReadLinesFromTextFile(string path)
        {
            using (var stream = new StreamReader(path))
            {
                var lines = new List<string>();
                while (!stream.EndOfStream)
                    lines.Add(stream.ReadLine());
                return lines;
            }
        }
        public static bool Matches(this string value, string pattern)
        {
            var result = new Regex(pattern).IsMatch(value);
            return result;
        }
        public static string RemoveLast(this string value, int numChars)
        {
            var result = value.Substring(0, Math.Max(value.Length - numChars, 0));
            return result;
        }
        public static bool ContainsAny(this string value, params string[] strs)
        {
            return strs.AnyTrue(value.Contains);
        }
    }
}