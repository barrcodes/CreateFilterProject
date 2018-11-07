using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CreateFilterProject.Utilities
{
    public delegate Task<string> StringConflictHandler(string foundString, string option1, string option2);

    public static class StringUtility
    {
        /// <summary>
        /// Using case-insensitive regex matching, determines the variation of the match, and replaces it with the appropriate variation of the desired result
        /// e.g. ReplaceVariations("Hello WORLD", "hello", "goodbye") == "Goodbye WORLD", and ReplaceVariations("Hello WORLD", "world", "friend") == "Hello FRIEND"
        /// </summary>
        /// <param name="string">unadultered string to replace</param>
        /// <param name="caseSensitiveOldValue">the old value to replace, in desired case, e.g. "OldFilter"</param>
        /// <param name="caseSensitiveNewValue">the new value, in desired case, e.g. "NewFilter"</param>
        /// <param name="conflictHandler">an async callback for optionally getting user feedback from the main thread</param>
        /// <returns>a string with all variation replacements applied</returns>
        public static string ReplaceVariations(this string @string, string caseSensitiveOldValue, string caseSensitiveNewValue, StringConflictHandler conflictHandler)
        {
            return Regex.Replace(
                input: @string, 
                pattern: caseSensitiveOldValue, 
                evaluator: (Match match) => 
                {
                    var task = EvaluateVariation(match, caseSensitiveOldValue, caseSensitiveNewValue, conflictHandler);
                    task.Wait();
                    return task.Result;
                }, 
                options: RegexOptions.IgnoreCase);
        }
        
        /// <summary>
        /// Utility method for evaluating the Regex Matches in ReplaceVariations()
        /// </summary>
        private static async Task<string> EvaluateVariation(Match match, string caseSensitiveOldValue, string caseSensitiveNewValue, StringConflictHandler conflictHandler)
        {
            if (match.Value == caseSensitiveOldValue.ToUpper())
            {
                return caseSensitiveNewValue.ToUpper();
            }

            if (match.Value == caseSensitiveOldValue.FirstCharacterToLower())
            {
                if (match.Value == caseSensitiveOldValue.ToLower())
                {
                    // resolve conflict (e.g. should dissolve => foofilter or dissolve => fooFilter)
                    return await conflictHandler(match.Value, caseSensitiveNewValue.ToLower(), caseSensitiveNewValue.FirstCharacterToLower());
                }
                else
                {
                    return caseSensitiveNewValue.FirstCharacterToLower();
                }
            }

            if (match.Value == caseSensitiveOldValue.ToLower())
            {
                return caseSensitiveOldValue.ToLower();
            }

            return caseSensitiveNewValue;
        }

        /// <summary>
        /// Quickly swaps the first character of a string to upper case. This is the fastest alternative w/ only one string allocation.
        /// </summary>
        public static string FirstCharacterToUpper(this string s)
        {
            if (s == null)
                return null;
            else if (s.Length == 0)
                return string.Empty;

            var a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }

        /// <summary>
        /// Quickly swaps the first character of a string to lower case. This is the fastest alternative w/ only one string allocation.
        /// </summary>
        public static string FirstCharacterToLower(this string s)
        {
            if (s == null)
                return null;
            else if (s.Length == 0)
                return string.Empty;

            var a = s.ToCharArray();
            a[0] = char.ToLower(a[0]);
            return new string(a);
        }
    }
}
