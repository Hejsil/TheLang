using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using TheLang.AST.Bases;
using TheLang.AST.Expressions.Operators;
using TheLang.AST.Statments;

namespace TheLang.Tests
{
    [TestFixture]
    public class ParserTest
    {
        [TestCase("t1.t2.t3", AssociativityKind.RightToLeft)]

        [TestCase("t1 as t2 as t3", AssociativityKind.LeftToRight)]

        [TestCase("t1 * t2 * t3", AssociativityKind.LeftToRight)]
        [TestCase("t1 / t2 / t3", AssociativityKind.LeftToRight)]
        [TestCase("t1 % t2 % t3", AssociativityKind.LeftToRight)]

        [TestCase("t1 + t2 + t3", AssociativityKind.LeftToRight)]
        [TestCase("t1 - t2 - t3", AssociativityKind.LeftToRight)]
        
        [TestCase("t1 < t2 < t3", AssociativityKind.LeftToRight)]
        [TestCase("t1 <= t2 <= t3", AssociativityKind.LeftToRight)]
        [TestCase("t1 > t2 > t3", AssociativityKind.LeftToRight)]
        [TestCase("t1 <= t2 <= t3", AssociativityKind.LeftToRight)]

        [TestCase("t1 == t2 == t3", AssociativityKind.LeftToRight)]
        [TestCase("t1 != t2 != t3", AssociativityKind.LeftToRight)]

        [TestCase("t1 and t2 and t3", AssociativityKind.LeftToRight)]
        [TestCase("t1 or t2 or t3", AssociativityKind.LeftToRight)]

        [TestCase("t1 = t2 = t3", AssociativityKind.RightToLeft)]
        [TestCase("t1 += t2 += t3", AssociativityKind.RightToLeft)]
        [TestCase("t1 -= t2 -= t3", AssociativityKind.RightToLeft)]
        [TestCase("t1 *= t2 *= t3", AssociativityKind.RightToLeft)]
        [TestCase("t1 /= t2 /= t3", AssociativityKind.RightToLeft)]
        [TestCase("t1 %= t2 %= t3", AssociativityKind.RightToLeft)]
        public void Assosiativity(string expression, AssociativityKind associativity)
        {
            var compiler = new Compiler();

            using (var str = new StringReader($"test :: {expression}"))
                Assert.True(compiler.ParseProgram(str));

            var variable = compiler.Tree.Files.First().Declarations.First() as Variable;
            Assert.NotNull(variable);

            var op = variable.Value as BinaryOperator;
            Assert.NotNull(op);

            Assert.IsNotInstanceOf<BinaryOperator>(associativity == AssociativityKind.LeftToRight ? op.Right : op.Left);
        }

        
        [TestCase("t1.t2 as t3")]
        [TestCase("t1 as t2.t3")]

        [TestCase("t1.t2 as t3.t4")]
        [TestCase("t1 as t2.t3.t4")]
        [TestCase("t1.t2.t3 as t4")]

        [TestCase("t1 * t2 as t3.t4")]
        [TestCase("t1 * t2.t3 as t4")]

        [TestCase("t1 as t2.t3 * t4")]
        [TestCase("t1 as t2 * t3.t4")]

        [TestCase("t1 * t2.t3 as t4")]
        [TestCase("t1 * t2 as t3.t4")]
        public void Priority(string expression)
        {
            var compiler = new Compiler();

            using (var str = new StringReader($"test :: {expression}"))
                Assert.True(compiler.ParseProgram(str));

            var variable = compiler.Tree.Files.First().Declarations.First() as Variable;
            Assert.NotNull(variable);

            var op = variable.Value as BinaryOperator;
            Assert.NotNull(op);

            TestThatChildrenUpholdPriority(op);

            void TestThatChildrenUpholdPriority(BinaryOperator binary)
            {
                if (binary.Left is BinaryOperator left)
                {
                    Assert.LessOrEqual(left.Priority, binary.Priority, binary.Kind.ToString());
                    TestThatChildrenUpholdPriority(left);
                }

                if (binary.Right is BinaryOperator right)
                {
                    Assert.LessOrEqual(right.Priority, binary.Priority, binary.Kind.ToString());
                    TestThatChildrenUpholdPriority(right);
                }
            }
        }

        
    }
}
