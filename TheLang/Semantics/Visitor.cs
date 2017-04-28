﻿using System;
using System.Collections.Generic;
using System.Linq;
using TheLang.AST;
using TheLang.AST.Bases;
using TheLang.AST.Expressions;
using TheLang.AST.Expressions.Literals;
using TheLang.AST.Expressions.Operators;
using TheLang.AST.Expressions.Operators.Binary;
using TheLang.AST.Expressions.Operators.Unary;
using TheLang.AST.Statments;

namespace TheLang.Semantics
{
    public abstract class Visitor
    {
        public bool Visit(dynamic node) => Visit(node);

        protected abstract bool Visit(Assign node);
        protected abstract bool Visit(Declaration node);
        protected abstract bool Visit(Return node);
        protected abstract bool Visit(Variable node);

        protected abstract bool Visit(FloatLiteral node);
        protected abstract bool Visit(Infer node);
        protected abstract bool Visit(IntegerLiteral node);
        protected abstract bool Visit(ProcedureType node);
        protected abstract bool Visit(StringLiteral node);
        protected abstract bool Visit(StructType node);
        protected abstract bool Visit(TupleLiteral node);
        protected abstract bool Visit(TypedProcedureLiteral node);
        protected abstract bool Visit(TypeLiteral node);

        protected abstract bool Visit(Add node);
        protected abstract bool Visit(And node);
        protected abstract bool Visit(As node);
        protected abstract bool Visit(Divide node);
        protected abstract bool Visit(Dot node);
        protected abstract bool Visit(Equal node);
        protected abstract bool Visit(GreaterThan node);
        protected abstract bool Visit(GreaterThanEqual node);
        protected abstract bool Visit(LessThan node);
        protected abstract bool Visit(LessThanEqual node);
        protected abstract bool Visit(Modulo node);
        protected abstract bool Visit(NotEqual node);
        protected abstract bool Visit(Or node);
        protected abstract bool Visit(Sub node);
        protected abstract bool Visit(Times node);

        protected abstract bool Visit(ArrayPostfix node);
        protected abstract bool Visit(Call node);
        protected abstract bool Visit(Dereference node);
        protected abstract bool Visit(Indexing node);
        protected abstract bool Visit(Negative node);
        protected abstract bool Visit(Not node);
        protected abstract bool Visit(Positive node);
        protected abstract bool Visit(Reference node);
        protected abstract bool Visit(UniqueReference node);

        protected abstract bool Visit(Symbol node);

        protected virtual bool Visit(ProgramNode node) => VisitCollection(node.Files);
        protected virtual bool Visit(FileNode node) => VisitCollection(node.Declarations);
        protected virtual bool Visit(CodeBlock node) => VisitCollection(node.Statements);

        protected bool VisitCollection<T>(IEnumerable<T> nodes) where T : Node
        {
            var succes = true;

            foreach (var node in nodes)
            {
                if (!Visit(node))
                    succes = false;
            }

            return succes;
        }
    }
}
