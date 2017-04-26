using System.IO;
using System.Linq;
using NUnit.Framework;
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
        [TestCase("t1 >= t2 >= t3", AssociativityKind.LeftToRight)]

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

        [TestCase("t1.t2 * t3 as t4")]
        [TestCase("t1.t2 as t3 * t4")]
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
        

        [TestCase("f :: func() Nothing => { }")]
        [TestCase("f :: proc() Nothing => { }")]

        [TestCase("f :: func(a: Int) Nothing => { }")]
        [TestCase("f :: proc(a: Int) Nothing => { }")]

        [TestCase("f :: func(a: Int, b: Int) Nothing => { }")]
        [TestCase("f :: proc(a: Int, b: Int) Nothing => { }")]

        [TestCase("f :: func() func(Int, Int) Int => { }")]
        [TestCase("f :: proc() func(Int, Int) Int => { }")]

        [TestCase("f :: func() proc(Int, Int) Int => { }")]
        [TestCase("f :: proc() proc(Int, Int) Int => { }")]

        [TestCase("f :: func(a: proc(Int, Int) Int, b: proc(Int, Int) Int) proc(Int, Int) Int => { }")]
        [TestCase("f :: proc(a: proc(Int, Int) Int, b: proc(Int, Int) Int) proc(Int, Int) Int => { }")]

        [TestCase("dec: Int")]

        [TestCase("dec: []Int")]
        [TestCase("dec: [,]Int")]
        [TestCase("dec: [,,]Int")]
        [TestCase("dec: [][]Int")]
        [TestCase("dec: [,][]Int")]
        [TestCase("dec: [][,]Int")]
        [TestCase("dec: [,][,]Int")]

        [TestCase("dec: []@Int")]
        [TestCase("dec: []u@Int")]
        [TestCase("dec: []proc() Int")]
        [TestCase("dec: []func() Int")]

        [TestCase("dec: @Int")]
        [TestCase("dec: @@Int")]
        [TestCase("dec: @[]Int")]
        [TestCase("dec: @[,]Int")]
        [TestCase("dec: @[,,]Int")]
        [TestCase("dec: @u@Int")]
        [TestCase("dec: @proc() Int")]
        [TestCase("dec: @func() Int")]

        [TestCase("dec: u@Int")]
        [TestCase("dec: u@u@Int")]
        [TestCase("dec: u@[]Int")]
        [TestCase("dec: u@[,]Int")]
        [TestCase("dec: u@[,,]Int")]
        [TestCase("dec: u@@Int")]
        [TestCase("dec: u@proc() Int")]
        [TestCase("dec: u@func() Int")]

        [TestCase("dec: func() Int")]

        [TestCase("dec: func() []Int")]
        [TestCase("dec: func() [,]Int")]
        [TestCase("dec: func() [,,]Int")]
        [TestCase("dec: func() [][]Int")]
        [TestCase("dec: func() [,][]Int")]
        [TestCase("dec: func() [][,]Int")]
        [TestCase("dec: func() [,][,]Int")]

        [TestCase("dec: func() []@Int")]
        [TestCase("dec: func() []u@Int")]
        [TestCase("dec: func() []proc() Int")]
        [TestCase("dec: func() []func() Int")]

        [TestCase("dec: func() @Int")]
        [TestCase("dec: func() @@Int")]
        [TestCase("dec: func() @[]Int")]
        [TestCase("dec: func() @[,]Int")]
        [TestCase("dec: func() @[,,]Int")]
        [TestCase("dec: func() @u@Int")]
        [TestCase("dec: func() @proc() Int")]
        [TestCase("dec: func() @func() Int")]

        [TestCase("dec: func() u@Int")]
        [TestCase("dec: func() u@u@Int")]
        [TestCase("dec: func() u@[]Int")]
        [TestCase("dec: func() u@[,]Int")]
        [TestCase("dec: func() u@[,,]Int")]
        [TestCase("dec: func() u@@Int")]
        [TestCase("dec: func() u@proc() Int")]
        [TestCase("dec: func() u@func() Int")]

        [TestCase("dec: func(Int) Int")]

        [TestCase("dec: func([]Int) Int")]
        [TestCase("dec: func([,]Int) Int")]
        [TestCase("dec: func([,,]Int) Int")]
        [TestCase("dec: func([][]Int) Int")]
        [TestCase("dec: func([,][]Int) Int")]
        [TestCase("dec: func([][,]Int) Int")]
        [TestCase("dec: func([,][,]Int) Int")]

        [TestCase("dec: func([]@Int) Int")]
        [TestCase("dec: func([]u@Int) Int")]
        [TestCase("dec: func([]proc() Int) Int")]
        [TestCase("dec: func([]func() Int) Int")]

        [TestCase("dec: func(@Int) Int")]
        [TestCase("dec: func(@@Int) Int")]
        [TestCase("dec: func(@[]Int) Int")]
        [TestCase("dec: func(@[,]Int) Int")]
        [TestCase("dec: func(@[,,]Int) Int")]
        [TestCase("dec: func(@u@Int) Int")]
        [TestCase("dec: func(@proc() Int) Int")]
        [TestCase("dec: func(@func() Int) Int")]

        [TestCase("dec: func(u@Int) Int")]
        [TestCase("dec: func(u@u@Int) Int")]
        [TestCase("dec: func(u@[]Int) Int")]
        [TestCase("dec: func(u@[,]Int) Int")]
        [TestCase("dec: func(u@[,,]Int) Int")]
        [TestCase("dec: func(u@@Int) Int")]
        [TestCase("dec: func(u@proc() Int) Int")]
        [TestCase("dec: func(u@func() Int) Int")]

        [TestCase("dec: proc() Int")]

        [TestCase("dec: proc() []Int")]
        [TestCase("dec: proc() [,]Int")]
        [TestCase("dec: proc() [,,]Int")]
        [TestCase("dec: proc() [][]Int")]
        [TestCase("dec: proc() [,][]Int")]
        [TestCase("dec: proc() [][,]Int")]
        [TestCase("dec: proc() [,][,]Int")]

        [TestCase("dec: proc() []@Int")]
        [TestCase("dec: proc() []u@Int")]
        [TestCase("dec: proc() []proc() Int")]
        [TestCase("dec: proc() []func() Int")]

        [TestCase("dec: proc() @Int")]
        [TestCase("dec: proc() @@Int")]
        [TestCase("dec: proc() @[]Int")]
        [TestCase("dec: proc() @[,]Int")]
        [TestCase("dec: proc() @[,,]Int")]
        [TestCase("dec: proc() @u@Int")]
        [TestCase("dec: proc() @proc() Int")]
        [TestCase("dec: proc() @func() Int")]

        [TestCase("dec: proc() u@Int")]
        [TestCase("dec: proc() u@u@Int")]
        [TestCase("dec: proc() u@[]Int")]
        [TestCase("dec: proc() u@[,]Int")]
        [TestCase("dec: proc() u@[,,]Int")]
        [TestCase("dec: proc() u@@Int")]
        [TestCase("dec: proc() u@proc() Int")]
        [TestCase("dec: proc() u@func() Int")]

        [TestCase("dec: proc(Int) Int")]

        [TestCase("dec: proc([]Int) Int")]
        [TestCase("dec: proc([,]Int) Int")]
        [TestCase("dec: proc([,,]Int) Int")]
        [TestCase("dec: proc([][]Int) Int")]
        [TestCase("dec: proc([,][]Int) Int")]
        [TestCase("dec: proc([][,]Int) Int")]
        [TestCase("dec: proc([,][,]Int) Int")]

        [TestCase("dec: proc([]@Int) Int")]
        [TestCase("dec: proc([]u@Int) Int")]
        [TestCase("dec: proc([]proc() Int) Int")]
        [TestCase("dec: proc([]func() Int) Int")]

        [TestCase("dec: proc(@Int) Int")]
        [TestCase("dec: proc(@@Int) Int")]
        [TestCase("dec: proc(@[]Int) Int")]
        [TestCase("dec: proc(@[,]Int) Int")]
        [TestCase("dec: proc(@[,,]Int) Int")]
        [TestCase("dec: proc(@u@Int) Int")]
        [TestCase("dec: proc(@proc() Int) Int")]
        [TestCase("dec: proc(@func() Int) Int")]

        [TestCase("dec: proc(u@Int) Int")]
        [TestCase("dec: proc(u@u@Int) Int")]
        [TestCase("dec: proc(u@[]Int) Int")]
        [TestCase("dec: proc(u@[,]Int) Int")]
        [TestCase("dec: proc(u@[,,]Int) Int")]
        [TestCase("dec: proc(u@@Int) Int")]
        [TestCase("dec: proc(u@proc() Int) Int")]
        [TestCase("dec: proc(u@func() Int) Int")]
        public void ShouldParsePrograms(string prog)
        {
            var compiler = new Compiler();

            using (var str = new StringReader(prog))
                Assert.True(compiler.ParseProgram(str));
        }
    }
}
