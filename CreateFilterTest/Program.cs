using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CreateFilterProject.Controllers;
using CreateFilterTests;

namespace CreateFilterTest
{
    class Program
    {
        static void Main(string[] args)
        {
            StringUtilityTest stringTest = new StringUtilityTest();
            stringTest.TestReplaceVariations();

            CreateProjectTest projectTest = new CreateProjectTest();
            projectTest.TestCreateProject();

            Console.WriteLine("Tests successful.");
            Console.ReadLine();
        }
    }
}
