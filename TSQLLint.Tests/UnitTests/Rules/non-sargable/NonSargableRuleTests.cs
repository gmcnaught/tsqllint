using System;
using System.Collections.Generic;
using NUnit.Framework;
using TSQLLint.Lib.Rules;
using TSQLLint.Lib.Rules.RuleViolations;

namespace TSQLLint.Tests.UnitTests.Rules
{
    public class NonSargableRuleTests
    {
        private static readonly object[] testCases =
        {
            new object[]
            {
                "non-sargable", "non-sargable-no-error",  typeof(NonSargableRule), new List<RuleViolation>()
            },
            new object[]
            {
                "non-sargable", "non-sargable-one-error",  typeof(NonSargableRule), new List<RuleViolation>
                {
                    new RuleViolation("non-sargable", 1, 25)
                }
            },
            new object[]
            {
                "non-sargable", "non-sargable-multi-error",  typeof(NonSargableRule), new List<RuleViolation>
                {
                    new RuleViolation("non-sargable", 1, 37),
                    new RuleViolation("non-sargable", 1, 55),
                    new RuleViolation("non-sargable", 3, 37),
                    new RuleViolation("non-sargable", 5, 31),
                    new RuleViolation("non-sargable", 7, 25),
                    new RuleViolation("non-sargable", 9, 28),
                    new RuleViolation("non-sargable", 11, 25),
                    new RuleViolation("non-sargable", 13, 25),
                    new RuleViolation("non-sargable", 15, 25),
                    new RuleViolation("non-sargable", 17, 25),
                    new RuleViolation("non-sargable", 19, 28),
                    new RuleViolation("non-sargable", 21, 28),
                    new RuleViolation("non-sargable", 29, 12),
                    new RuleViolation("non-sargable", 37, 11),
                    new RuleViolation("non-sargable", 45, 13),
                    new RuleViolation("non-sargable", 54, 13)
                }
            }
        };

        [Test, TestCaseSource(nameof(testCases))]
        public void TestRule(string rule, string testFileName, Type ruleType, List<RuleViolation> expectedRuleViolations)
        {
            RulesTestHelper.RunRulesTest(rule, testFileName, ruleType, expectedRuleViolations);
        }
    }
}
