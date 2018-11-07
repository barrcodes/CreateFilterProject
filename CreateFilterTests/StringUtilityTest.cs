using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CreateFilterProject.Utilities;
using System.Threading.Tasks;

namespace CreateFilterTests
{
    [TestClass]
    public class StringUtilityTest
    {
        [TestMethod]
        public void TestReplaceVariations()
        {
            TestLowerVariation();
            TestFirstUpperVariation();
            TestAllCapsVariation();
            TestCaseSensitiveVariation();
        }

        private void TestLowerVariation()
        {
            var result1 = StringUtility.ReplaceVariations("hello My WORLD", "Hello", "GoodBye", ResolveOption1);
            var result2 = StringUtility.ReplaceVariations("hello My WORLD", "Hello", "GoodBye", ResolveOption2);
            AuditResult(result1, "goodbye");
            AuditResult(result2, "goodBye");   
        }

        private Task<string> ResolveOption1(string foundString, string option1, string option2)
        {
            return Task.FromResult(option1);
        }

        private Task<string> ResolveOption2(string foundString, string option1, string option2)
        {
            return Task.FromResult(option2);
        }

        private Task<string> NoResolution(string foundString, string option1, string option2)
        {
            throw new ArgumentException($"No resolution expected for input string: {foundString}", nameof(foundString));
        }

        private void TestFirstUpperVariation()
        {
            var result = StringUtility.ReplaceVariations("hello My WORLD", "My", "Our", NoResolution);
            AuditResult(result, "Our");
        }

        

        private void TestAllCapsVariation()
        {
            var result = StringUtility.ReplaceVariations("hello My WORLD", "World", "Place", NoResolution);
            AuditResult(result, "PLACE");
        }

        private void TestCaseSensitiveVariation()
        {
            var result1 = StringUtility.ReplaceVariations("private Dissolve dissolve", "Dissolve", "TestFilter", ResolveOption1);
            var result2 = StringUtility.ReplaceVariations("private Dissolve dissolve", "Dissolve", "TestFilter", ResolveOption2);
            AuditResult(result1, "TestFilter");
            AuditResult(result1, "testfilter");
            AuditResult(result2, "TestFilter");
            AuditResult(result2, "testFilter");
        }

        private void AuditResult(string result, string substring)
        {
            if (!result.Contains(substring))
                throw new Exception($"{result} does not contain {substring}.");

            Console.WriteLine($"SUCCESS => {result}");
        }
    }
}
