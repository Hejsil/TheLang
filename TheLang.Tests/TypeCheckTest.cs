using System.IO;
using System.Linq;
using NUnit.Framework;
using TheLang.Semantics.TypeChecking;

namespace TheLang.Tests
{
    [TestFixture]
    public class TypeCheckTest
    {
        [TestCase("const :: 1", TypeId.Integer)]
        [TestCase("const :: 1.0", TypeId.Float)]
        [TestCase("const : Int : 1", TypeId.Integer)]
        [TestCase("const : UInt : 1", TypeId.UInteger)]
        [TestCase("const : Float : 1.0", TypeId.Float)]

        [TestCase("const :: 1 as Int", TypeId.Integer)]
        [TestCase("const :: 1 as UInt", TypeId.UInteger)]
        [TestCase("const :: 1 as Float", TypeId.Float)]

        [TestCase("const :: 1.0 as Int", TypeId.Integer)]
        [TestCase("const :: 1.0 as UInt", TypeId.UInteger)]
        [TestCase("const :: 1.0 as Float", TypeId.Float)]

        [TestCase("cost :: 1 * 1", TypeId.Integer)]
        [TestCase("cost :: 1.0 * 1.0", TypeId.UInteger)]
        [TestCase("cost : Int : 1 * 1", TypeId.Integer)]
        [TestCase("cost : UInt : 1 * 1", TypeId.UInteger)]
        [TestCase("cost :: 1.0 * 1.0", TypeId.UInteger)]


//        [TestCase("t1 / t2 / t3", TypeId.Integer)]
//        [TestCase("t1 % t2 % t3", TypeId.Integer)]
//
//        [TestCase("t1 + t2 + t3", TypeId.Integer)]
//        [TestCase("t1 - t2 - t3", TypeId.Integer)]
//
//        [TestCase("t1 < t2 < t3", TypeId.Integer)]
//        [TestCase("t1 <= t2 <= t3", TypeId.Integer)]
//        [TestCase("t1 > t2 > t3", TypeId.Integer)]
//        [TestCase("t1 >= t2 >= t3", TypeId.Integer)]
//
//        [TestCase("t1 == t2 == t3", TypeId.Integer)]
//        [TestCase("t1 != t2 != t3", TypeId.Integer)]
//
//        [TestCase("t1 and t2 and t3", TypeId.Integer)]
//        [TestCase("t1 or t2 or t3", TypeId.Integer)]
        public void ShouldPass(string proc, TypeId topType)
        {
            var compiler = new Compiler();

            using (var str = new StringReader(proc))
                Assert.True(compiler.ParseProgram(str));

            Assert.True(compiler.TypeCheck());
            var first = compiler.Tree.Files.First().Declarations.First();

            Assert.AreEqual(first.Type.Id, topType);
        }
    }
}