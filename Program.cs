//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="BME">
//     Copyright (c) JitSmart  All rights reserved.
// </copyright>
// <author>Barnabas Szirmay</author>
// <date>2014. 4. 21. 9:55</date>
//-----------------------------------------------------------------------
namespace Triangle
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Application which generates test cases for Boundary Value Analysis
    /// and Equivalence Class Testing for the Triangle problem
    /// </summary>
    public class Program
    {
        /// <summary>
        /// System path for <c>Blackbox.exe</c>, the application being tested
        /// </summary>
        private const string OutputDir = @"C:\Users\Barnabas\Packages\blackbox\";

        /// <summary>
        /// Expected output when either input variable is out of range
        /// </summary>
        private const string OutOfRange = "Out of range";

        /// <summary>
        /// Expected output for isosceles triangle
        /// </summary>
        private const string Isosceles = "Isosceles";

        /// <summary>
        /// Expected output for scalene output
        /// </summary>
        private const string Scalane = "Scalene";

        /// <summary>
        /// Expected output for invalid triangle
        /// </summary>
        private const string NotTriangle = "Not a Triangle";

        /// <summary>
        /// Expected output for equilateral triangle
        /// </summary>
        private const string Equilateral = "Equilateral";
                
        /// <summary>
        /// Generate CSV file for Simple Boundary Value analysis
        /// </summary>
        /// <param name="minVal">Range minimum</param>
        /// <param name="maxVal">Range maximum</param>
        public static void GenerateSimpleBVA(int minVal, int maxVal)
        {
            int normalVal = (maxVal + minVal) / 2;
            var tests = GetWeakTests(new int[] { minVal, minVal + 1, maxVal - 1, maxVal + 1 }, new int[] { normalVal, normalVal, normalVal });
            RunTestCases(tests, "simpleBVA.txt");
        }

        /// <summary>
        /// Generate CSV file for Worst-Case Boundary Value analysis
        /// </summary>
        /// <param name="minVal">Range minimum</param>
        /// <param name="maxVal">Range maximum</param>
        public static void GenerateWorstCaseBVA(int minVal, int maxVal)
        {
            int normalVal = (maxVal + minVal) / 2;
            RunTestCases(GetStrongTests(new int[] { minVal, minVal + 1, normalVal, maxVal - 1, maxVal }), "worstCaseBVA.txt");
        }

        /// <summary>
        /// Generate Weak Normal Equivalence tests
        /// </summary>
        public static void GenerateWeakNormalEquivalence()
        {
            RunTestCases(GetEquivalenceClasses(), "weakNormal.txt");
        }

        /// <summary>
        /// Generate Strong Normal Equivalence tests
        /// </summary>
        public static void GenerateStrongNormalEquivalence()
        {
            // Since there are no valid subintervals of the [1,200] interval
            // the strong tests match the weak tests
            RunTestCases(GetEquivalenceClasses(), "strongNormal.txt");
        }

        /// <summary>
        /// Generate Weak Robust Equivalence tests
        /// </summary>
        public static void GenerateWeakRobustNormalEquivalence()
        {
            var equivalenceClasses = GetEquivalenceClasses();
            var testCases = equivalenceClasses.SelectMany(e => GetWeakTests(new int[] { -1, 400 }, e));
            RunTestCases(testCases, "weakRobust.txt");
        }

        /// <summary>
        /// Generate Strong Robust Equivalence tests
        /// </summary>
        public static void GenerateStrongRobustNormalEquivalence()
        {
            var equivalenceClasses = GetEquivalenceClasses();

            var testCases = equivalenceClasses.SelectMany(
                triangleValue => GetStrongTests(triangleValue.Select(sideValue => new int[] { -1, sideValue, 400 }).ToArray()));

            RunTestCases(testCases, "strongRobust.txt");
        }

        /// <summary>
        /// Application entry point
        /// </summary>
        /// <param name="args">Command line arguments</param>
        public static void Main(string[] args)
        {
            GenerateSimpleBVA(1, 200);
            GenerateWorstCaseBVA(1, 200);
            GenerateWeakNormalEquivalence();
            GenerateWeakRobustNormalEquivalence();
            GenerateStrongNormalEquivalence();
            GenerateStrongRobustNormalEquivalence();
        }

        /// <summary>
        /// Get equivalence classes for the triangle program
        /// </summary>
        /// <returns>The equivalence classes</returns>
        private static IEnumerable<int[]> GetEquivalenceClasses()
        {
            yield return new int[] { 3, 3, 3 }; // equilateral
            yield return new int[] { 4, 4, 3 }; // isosceles
            yield return new int[] { 3, 4, 5 }; // orthogonal 
            yield return new int[] { 3, 4, 6 }; // normal
            yield return new int[] { 1, 1, 5 }; // not a triangle
        }

        /// <summary>
        /// Runs tests with single fault assumption
        /// </summary>
        /// <param name="boundaryValues">Values to be tested with others set to normal</param>
        /// <param name="normalValues">Normal value for the triangle's sides</param>
        /// <returns>All test cases</returns>
        private static IEnumerable<int[]> GetWeakTests(int[] boundaryValues, int[] normalValues)
        {
            var retval = new List<int[]>();
            for (int i = 0; i < 3; i++)
            {
                int[] values = (int[])normalValues.Clone();

                foreach (var changedValue in boundaryValues)
                {
                    values[i] = changedValue;
                    yield return new int[] { values[0], values[1], values[2] };
                }
            }

            yield return normalValues;
        }

        /// <summary>
        /// Runs tests with multiple fault assumption
        /// </summary>
        /// <param name="values">Allowed values</param>
        /// <returns>List of test cases</returns>
        private static IEnumerable<int[]> GetStrongTests(int[] values)
        {
            return GetStrongTests(new int[][] { values, values, values });
        }

        /// <summary>
        /// Runs tests with multiple fault assumption, each with its own values
        /// </summary>
        /// <param name="values">Allowed values for each side</param>
        /// <returns>List of test cases</returns>
        private static IEnumerable<int[]> GetStrongTests(int[][] values)
        {
            foreach (var i in values[0]) 
            {
                foreach (var j in values[1])
                {
                    foreach (var k in values[2])
                    {
                        yield return new int[] { i, j, k };
                    }
                }
            }
        }

        /// <summary>
        /// What output is expected from
        /// </summary>
        /// <param name="a">Side A</param>
        /// <param name="b">Side B</param>
        /// <param name="c">Side C</param>
        /// <returns>Expected output by specification</returns>
        private static string GetTriangleType(int a, int b, int c)
        {
            var sides = (new int[] { a, b, c }).OrderByDescending(i => i);

            // Check range
            if (sides.First() > 200 || sides.Last() < 1)
            {
                return OutOfRange;
            }

            // Check if triangle
            if (sides.First() >= sides.Skip(1).Sum())
            {
                return NotTriangle;
            }

            switch (sides.Distinct().Count())
            {
                case 1: return Equilateral;
                case 2: return Isosceles;
                default: return Scalane;
            }
        }

        /// <summary>
        /// Generate Strong Normal Equivalence tests
        /// </summary>
        /// <param name="testCases">Which test case to run</param>
        /// <param name="file">Output file to write content</param>
        private static void RunTestCases(IEnumerable<int[]> testCases, string file)
        {
            using (var stream = File.CreateText(OutputDir + file))
            {
                foreach (var testCase in testCases)
                {
                    stream.WriteLine(string.Format("{0},{1},{2},\"{3}\"", testCase[0], testCase[1], testCase[2], GetTriangleType(testCase[0], testCase[1], testCase[2])));
                }
            }
        }
    }
}
