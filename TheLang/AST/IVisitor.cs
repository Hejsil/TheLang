using TheLang.AST.Expressions.Literals;
using TheLang.AST.Expressions.Operators;
using TheLang.AST.Statments;
using TheLang.AST.Types;

namespace TheLang.AST
{
    public interface IVisitor
    {
        bool Visit(Program program);
        bool Visit(Variable variableDeclaration);
        bool Visit(NeedsToBeInferedType needsToBeInferedType);
        bool Visit(IntegerLiteral integerLiteral);
        bool Visit(FloatLiteral floatLiteral);
        bool Visit(ProcedureType functionType);
        bool Visit(ArrayType arrayType);
        bool Visit(TupleType tupleType);
        bool Visit(NamedType namedType);
        bool Visit(PointerType pointerType);
        bool Visit(Declaration declaration);
        bool Visit(BinaryOperator binaryOperator);
        bool Visit(UnaryOperator needsToBeInferedType);
    }
}
