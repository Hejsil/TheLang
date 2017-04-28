using System.IO;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using TheLang.AST.Bases;
using TheLang.AST.Expressions.Operators;
using TheLang.AST.Expressions.Operators.Binary;
using TheLang.AST.Statments;
using TheLang.Syntax;

namespace TheLang.Tests
{
    [TestFixture]
    public class ParserTest
    {
        [TestCase("t1 as t2 as t3", Associativity.LeftToRight)]

        [TestCase("t1 * t2 * t3", Associativity.LeftToRight)]
        [TestCase("t1 / t2 / t3", Associativity.LeftToRight)]
        [TestCase("t1 % t2 % t3", Associativity.LeftToRight)]

        [TestCase("t1 + t2 + t3", Associativity.LeftToRight)]
        [TestCase("t1 - t2 - t3", Associativity.LeftToRight)]
        
        [TestCase("t1 < t2 < t3", Associativity.LeftToRight)]
        [TestCase("t1 <= t2 <= t3", Associativity.LeftToRight)]
        [TestCase("t1 > t2 > t3", Associativity.LeftToRight)]
        [TestCase("t1 >= t2 >= t3", Associativity.LeftToRight)]

        [TestCase("t1 == t2 == t3", Associativity.LeftToRight)]
        [TestCase("t1 != t2 != t3", Associativity.LeftToRight)]

        [TestCase("t1 and t2 and t3", Associativity.LeftToRight)]
        [TestCase("t1 or t2 or t3", Associativity.LeftToRight)]
        public void Assosiativity(string expression, Associativity associativity)
        {
            var compiler = new Compiler();

            using (var str = new StringReader($"test :: {expression}"))
                Assert.True(compiler.ParseProgram(str));

            var variable = compiler.Tree.Files.First().Declarations.First() as Variable;
            Assert.NotNull(variable);

            var op = variable.Value as BinaryNode;
            Assert.NotNull(op);

            Assert.IsNotInstanceOf<BinaryNode>(associativity == Associativity.LeftToRight ? op.Right : op.Left);
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

            TestThatChildrenUpholdPriority(variable.Value);
        }

        private void TestThatChildrenUpholdPriority(Node node)
        {
            OpInfo info;
            Assert.True(Parser.OperatorInfo.TryGetValue(node.GetType(), out info));

            var binary = node as BinaryNode;
            var unary = node as UnaryNode;
            var dot = node as Dot;

            if (binary != null)
            {
                OpInfo left, right;
                if (Parser.OperatorInfo.TryGetValue(binary.Left.GetType(), out left))
                {
                    Assert.GreaterOrEqual(info.Priority, left.Priority);
                    TestThatChildrenUpholdPriority(binary.Left);
                }
                if (Parser.OperatorInfo.TryGetValue(binary.Right.GetType(), out right))
                {
                    Assert.GreaterOrEqual(info.Priority, right.Priority);
                    TestThatChildrenUpholdPriority(binary.Right);
                }
            }
            else if (unary != null)
            {
                OpInfo child;
                if (Parser.OperatorInfo.TryGetValue(unary.Child.GetType(), out child))
                {
                    Assert.GreaterOrEqual(info.Priority, child.Priority);
                    TestThatChildrenUpholdPriority(unary.Child);
                }
            }
            else if (dot != null)
            {

                OpInfo left;
                if (Parser.OperatorInfo.TryGetValue(dot.Left.GetType(), out left))
                {
                    Assert.GreaterOrEqual(info.Priority, left.Priority);
                    TestThatChildrenUpholdPriority(dot.Left);
                }
            }

        }


        [TestCase("const :: func() Nothing => { }")]
        [TestCase("const :: proc() Nothing => { }")]

        [TestCase("const :: func(a: Int) Nothing => { }")]
        [TestCase("const :: proc(a: Int) Nothing => { }")]

        [TestCase("const :: func(a: Int, b: Int) Nothing => { }")]
        [TestCase("const :: proc(a: Int, b: Int) Nothing => { }")]

        [TestCase("const :: func() func(Int, Int) Int => { }")]
        [TestCase("const :: proc() func(Int, Int) Int => { }")]

        [TestCase("const :: func() proc(Int, Int) Int => { }")]
        [TestCase("const :: proc() proc(Int, Int) Int => { }")]

        [TestCase("const :: func(a: proc(Int, Int) Int, b: proc(Int, Int) Int) proc(Int, Int) Int => { }")]
        [TestCase("const :: proc(a: proc(Int, Int) Int, b: proc(Int, Int) Int) proc(Int, Int) Int => { }")]

        [TestCase("const :: struct { }")]
        [TestCase("const :: struct { x: Int }")]
        [TestCase("const :: struct { x: Int y: Int }")]

        [TestCase("const :: struct { x := 1 }")]
        [TestCase("const :: struct { x := 1 y := 2 }")]
        [TestCase("const :: struct { x := 1.0 y := 2.0 }")]

        [TestCase("const :: struct { x: Int = 1 }")]
        [TestCase("const :: struct { x: Int = 1 y: Int = 2 }")]
        [TestCase("const :: struct { x: Float = 1.0 y: Float = 2.0 }")]

        [TestCase("const :: struct { inner :: struct { } }")]
        [TestCase("const :: struct { inner :: struct { x: Int } }")]
        [TestCase("const :: struct { inner :: struct { x: Int y: Int } }")]

        [TestCase("const :: struct { inner :: struct { x := 1 y := 2 } }")]
        [TestCase("const :: struct { inner :: struct { x := 1.0 y := 2.0 } }")]

        [TestCase("const :: struct { inner :: struct { x: Int = 1 y: Int = 2 } }")]
        [TestCase("const :: struct { inner :: struct { x: Float = 1.0 y: Float = 2.0 } }")]

        [TestCase("const :: struct { inner :: proc() Nothing => { } }")]
        [TestCase("const :: struct { inner :: func() Nothing => { } }")]

        [TestCase("dec: Int")]

        [TestCase("dec: []Int")]
        [TestCase("dec: [][]Int")]

        [TestCase("dec: []@Int")]
        [TestCase("dec: []u@Int")]
        [TestCase("dec: []proc() Int")]
        [TestCase("dec: []func() Int")]

        [TestCase("dec: @Int")]
        [TestCase("dec: @@Int")]
        [TestCase("dec: @[]Int")]
        [TestCase("dec: @u@Int")]
        [TestCase("dec: @proc() Int")]
        [TestCase("dec: @func() Int")]

        [TestCase("dec: u@Int")]
        [TestCase("dec: u@u@Int")]
        [TestCase("dec: u@[]Int")]
        [TestCase("dec: u@@Int")]
        [TestCase("dec: u@proc() Int")]
        [TestCase("dec: u@func() Int")]

        [TestCase("dec: func() Int")]

        [TestCase("dec: func() []Int")]
        [TestCase("dec: func() [][]Int")]

        [TestCase("dec: func() []@Int")]
        [TestCase("dec: func() []u@Int")]
        [TestCase("dec: func() []proc() Int")]
        [TestCase("dec: func() []func() Int")]

        [TestCase("dec: func() @Int")]
        [TestCase("dec: func() @@Int")]
        [TestCase("dec: func() @[]Int")]
        [TestCase("dec: func() @u@Int")]
        [TestCase("dec: func() @proc() Int")]
        [TestCase("dec: func() @func() Int")]

        [TestCase("dec: func() u@Int")]
        [TestCase("dec: func() u@u@Int")]
        [TestCase("dec: func() u@[]Int")]
        [TestCase("dec: func() u@@Int")]
        [TestCase("dec: func() u@proc() Int")]
        [TestCase("dec: func() u@func() Int")]

        [TestCase("dec: func(Int) Int")]

        [TestCase("dec: func([]Int) Int")]
        [TestCase("dec: func([][]Int) Int")]

        [TestCase("dec: func([]@Int) Int")]
        [TestCase("dec: func([]u@Int) Int")]
        [TestCase("dec: func([]proc() Int) Int")]
        [TestCase("dec: func([]func() Int) Int")]

        [TestCase("dec: func(@Int) Int")]
        [TestCase("dec: func(@@Int) Int")]
        [TestCase("dec: func(@[]Int) Int")]
        [TestCase("dec: func(@u@Int) Int")]
        [TestCase("dec: func(@proc() Int) Int")]
        [TestCase("dec: func(@func() Int) Int")]

        [TestCase("dec: func(u@Int) Int")]
        [TestCase("dec: func(u@u@Int) Int")]
        [TestCase("dec: func(u@[]Int) Int")]
        [TestCase("dec: func(u@@Int) Int")]
        [TestCase("dec: func(u@proc() Int) Int")]
        [TestCase("dec: func(u@func() Int) Int")]

        [TestCase("dec: proc() Int")]

        [TestCase("dec: proc() []Int")]
        [TestCase("dec: proc() [][]Int")]

        [TestCase("dec: proc() []@Int")]
        [TestCase("dec: proc() []u@Int")]
        [TestCase("dec: proc() []proc() Int")]
        [TestCase("dec: proc() []func() Int")]

        [TestCase("dec: proc() @Int")]
        [TestCase("dec: proc() @@Int")]
        [TestCase("dec: proc() @[]Int")]
        [TestCase("dec: proc() @u@Int")]
        [TestCase("dec: proc() @proc() Int")]
        [TestCase("dec: proc() @func() Int")]

        [TestCase("dec: proc() u@Int")]
        [TestCase("dec: proc() u@u@Int")]
        [TestCase("dec: proc() u@[]Int")]
        [TestCase("dec: proc() u@@Int")]
        [TestCase("dec: proc() u@proc() Int")]
        [TestCase("dec: proc() u@func() Int")]

        [TestCase("dec: proc(Int) Int")]

        [TestCase("dec: proc([]Int) Int")]
        [TestCase("dec: proc([][]Int) Int")]

        [TestCase("dec: proc([]@Int) Int")]
        [TestCase("dec: proc([]u@Int) Int")]
        [TestCase("dec: proc([]proc() Int) Int")]
        [TestCase("dec: proc([]func() Int) Int")]

        [TestCase("dec: proc(@Int) Int")]
        [TestCase("dec: proc(@@Int) Int")]
        [TestCase("dec: proc(@[]Int) Int")]
        [TestCase("dec: proc(@u@Int) Int")]
        [TestCase("dec: proc(@proc() Int) Int")]
        [TestCase("dec: proc(@func() Int) Int")]

        [TestCase("dec: proc(u@Int) Int")]
        [TestCase("dec: proc(u@u@Int) Int")]
        [TestCase("dec: proc(u@[]Int) Int")]
        [TestCase("dec: proc(u@@Int) Int")]
        [TestCase("dec: proc(u@proc() Int) Int")]
        [TestCase("dec: proc(u@func() Int) Int")]

        [TestCase("lit := 1")]
        [TestCase("lit := 1.0")]
        [TestCase("lit := .0")]
        [TestCase("lit := .000_000")]
        [TestCase("lit := 1_000")]
        [TestCase("lit := 1.000_000")]
        [TestCase("lit := 1_000.000_000")]
        [TestCase("lit := \"Hello World!\"")]
        [TestCase("lit := Vec{}")]
        [TestCase("lit := Vec{ x = 0 }")]
        [TestCase("lit := Vec{ x = 0, y = 1 }")]
        public void ShouldParsePrograms(string prog)
        {
            var compiler = new Compiler();

            using (var str = new StringReader(prog))
                Assert.True(compiler.ParseProgram(str));
        }
    }
}
