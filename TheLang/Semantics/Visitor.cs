using System;
using System.Collections.Generic;
using System.Linq;
using TheLang.AST;
using TheLang.AST.Bases;
using TheLang.AST.Expressions;
using TheLang.AST.Expressions.Literals;
using TheLang.AST.Expressions.Operators;
using TheLang.AST.Expressions.Operators.Binary;
using TheLang.AST.Expressions.Operators.Unary;
using TheLang.AST.Expressions.Types;
using TheLang.AST.Statments;

namespace TheLang.Semantics
{
    public abstract class Visitor
    {
        public bool Visit(dynamic node) => Visit(node);
        
        protected abstract bool Visit(ASTDeclaration node);
        protected abstract bool Visit(ASTReturn node);
        protected abstract bool Visit(ASTVariable node);

        protected abstract bool Visit(ASTFloatLiteral node);
        protected abstract bool Visit(ASTInfer node);
        protected abstract bool Visit(ASTIntegerLiteral node);
        protected abstract bool Visit(ASTProcedureType node);
        protected abstract bool Visit(ASTStringLiteral node);
        protected abstract bool Visit(ASTStructType node);
        protected abstract bool Visit(ASTLambda node);
        protected abstract bool Visit(ASTLambda.Argument node);
        protected abstract bool Visit(ASTStructInitializer node);
        protected abstract bool Visit(ASTEmptyInitializer node);
        protected abstract bool Visit(ASTArrayInitializer node);

        protected abstract bool Visit(ASTAdd node);
        protected abstract bool Visit(ASTAnd node);
        protected abstract bool Visit(ASTAs node);
        protected abstract bool Visit(ASTDivide node);
        protected abstract bool Visit(ASTDot node);
        protected abstract bool Visit(ASTEqual node);
        protected abstract bool Visit(ASTGreaterThan node);
        protected abstract bool Visit(ASTGreaterThanEqual node);
        protected abstract bool Visit(ASTLessThan node);
        protected abstract bool Visit(ASTLessThanEqual node);
        protected abstract bool Visit(ASTModulo node);
        protected abstract bool Visit(ASTNotEqual node);
        protected abstract bool Visit(ASTOr node);
        protected abstract bool Visit(ASTSub node);
        protected abstract bool Visit(ASTTimes node);

        protected abstract bool Visit(ASTArrayType node);
        protected abstract bool Visit(ASTCall node);
        protected abstract bool Visit(ASTCompilerCall node);
        protected abstract bool Visit(ASTDereference node);
        protected abstract bool Visit(ASTIndexing node);
        protected abstract bool Visit(ASTNegative node);
        protected abstract bool Visit(ASTNot node);
        protected abstract bool Visit(ASTPositive node);
        protected abstract bool Visit(ASTReference node);

        protected abstract bool Visit(ASTSymbol node);

        protected virtual bool Visit(ASTProgramNode node) => Visit(node.Files);
        protected virtual bool Visit(ASTFileNode node) => Visit(node.Declarations);
        protected virtual bool Visit(ASTCodeBlock node) => Visit(node.Statements);

        protected bool Visit<T>(params T[] nodes) where T : ASTNode => Visit((IEnumerable<T>) nodes);
        protected bool Visit<T>(IEnumerable<T> nodes) where T : ASTNode => nodes.All(Visit);
    }
}
