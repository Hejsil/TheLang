using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheLang.AST;
using TheLang.AST.Bases;
using TheLang.AST.Expressions;
using TheLang.AST.Expressions.Literals;
using TheLang.AST.Expressions.Operators.Binary;
using TheLang.AST.Expressions.Operators.Unary;
using TheLang.AST.Expressions.Types;
using TheLang.AST.Statments;
using TheLang.Semantics.BackEnds.CTree;
using TheLang.Semantics.BackEnds.CTree.Operators.Binaries;
using TheLang.Semantics.BackEnds.CTree.Operators.Unaries;
using TheLang.Semantics.TypeChecking.Types;

namespace TheLang.Semantics.BackEnds
{
    public class CTreeEmitter : Visitor
    {
        private const string UserVariables = "_user_";
        private const string ArrayName = "_compiler_array_type";
        private const string LambdaName = "_compiler_lambda";
        private const string CreateStringProcedure = "_compiler_create_string";

        public CProgram Result { get; private set; }

        private readonly Dictionary<BaseType, CNode> _types = new Dictionary<BaseType, CNode>();

        private readonly List<CDeclaration> _globalScole = new List<CDeclaration>();
        private readonly List<CInclude> _includes = new List<CInclude>();
        private readonly List<CTypedef> _typedefs = new List<CTypedef>();
        private readonly List<CStruct> _structs = new List<CStruct>();
        private readonly List<CFunction> _functions = new List<CFunction>();

        private CNode _lastNode;

        private long _procedureTypeId;
        private long _lambdaId;


        private CNode GetCType(BaseType type)
        {
            if (_types.TryGetValue(type, out var result)) return result;

            switch (type)
            {
                case BooleanType t:
                    result = new CSymbol { Name = "uint8_t" };
                    break;

                case FloatType t:
                    switch (t.Size)
                    {
                        case 32:
                            result = new CSymbol { Name = "float" };
                            break;
                        case 64:
                            result = new CSymbol { Name = "double" };
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    break;

                case IntegerType t:
                    var signedness = t.Signed ? "u" : "";
                    result = new CSymbol { Name = $"{signedness}int{t.Size}_t" };
                    break;

                case PointerType t:
                    result = new CPointer { Child = GetCType(t.RefersTo) };
                    break;

                case ProcedureType t:
                    var name = $"_compiler_procedure_type{_procedureTypeId++}";
                    result = new CSymbol { Name = name };

                    var argumentNames = t.Arguments.Select(a => GetCType(a.Type));
                    var returnName = GetCType(t.Return);
                    var typedefBuilder = new StringBuilder();

                    typedefBuilder.AppendFormat("{0} (*{1})(", returnName, name);

                    var first = argumentNames.FirstOrDefault();
                    foreach (var a in argumentNames)
                    {
                        if (!ReferenceEquals(a, first))
                            typedefBuilder.Append(", ");

                        typedefBuilder.Append(a);
                    }

                    typedefBuilder.AppendFormat(");");
                    _typedefs.Add(new CTypedef { Typedeffing = typedefBuilder.ToString() });
                    break;

                case ArrayType a:
                case StringType t:
                    result = new CSymbol { Name = ArrayName };
                    break;

                //case TypeType t:
                //    break;
                case VoidType t:
                    result = new CSymbol { Name = "void" };
                    break;

                default:
                    throw new NotImplementedException();
            }

            _types.Add(type, result);
            return result;
        }

        protected override bool Visit(ASTProgramNode node)
        {
            _includes.Add(new CInclude { Name = "inttypes", Standard = true });
            _includes.Add(new CInclude { Name = "string", Standard = true });

            // typedef struct { void* data; int64_t count; } ArrayName;
            _structs.Add(new CStruct
            {
                Name = ArrayName,
                Enumerable = new []
                {
                    new CStruct.Field
                    {
                        Type = new CPointer { Child = new CSymbol { Name = "void" } },
                        Name = "data"
                    },
                    new CStruct.Field
                    {
                        Type = new CSymbol { Name = "int64_t" },
                        Name = "count"
                    }
                }
            });

            // ArrayName CreateStringProcedure(char* c) { return (ArrayName){ .data = c, .count = strlen(c) }; };
            _functions.Add(new CFunction
            {
                Return = new CSymbol { Name = ArrayName },
                Name = CreateStringProcedure,
                Arguments = new []
                {
                    new CFunction.Argument
                    {
                        Type = new CPointer { Child = new CSymbol { Name = "char" } },
                        Name = "c"
                    },
                },
                Block = new CBlock
                {
                    Statements = new []
                    {
                        new CReturn
                        {
                            Child = new CStructInitializer
                            {
                                Type = new CSymbol { Name = ArrayName },
                                Fields = new []
                                {
                                    new CStructInitializer.Field { Name = "data", Value = new CSymbol { Name = "c" } },
                                    new CStructInitializer.Field {
                                        Name = "count",
                                        Value = new CCall
                                        {
                                            Callee = new CSymbol { Name = "strlen" },
                                            Arguments = new [] { new CSymbol { Name = "c" } }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            });
            
            if (!base.Visit(node)) return false;

            Result = new CProgram
            {
                Functions = _functions,
                Globals = _globalScole,
                Includes = _includes,
                Structs = _structs,
                Typedefs = _typedefs
            };

            return true;
        }

        protected override bool Visit(ASTFileNode node)
        {
            foreach (var declaration in node.Declarations)
            {
                if (!Visit(declaration)) return false;

                if (_lastNode is CDeclaration d)
                    _globalScole.Add(d);
                if (_lastNode is CTypedef t)
                    _typedefs.Add(t);
                else
                    return false; // This should never happen
            }

            return true;
        }

        protected override bool Visit(ASTCodeBlock node)
        {
            var statments = new List<CNode>();

            foreach (var statement in node.Statements)
            {
                if (!Visit(statement)) return false;
                statments.Add(_lastNode);
            }

            _lastNode = new CBlock { Statements = statments };
            return true;
        }

        protected override bool Visit(ASTDeclaration node)
        {
            // This should be safe, if we did typechecking correct
            var type = (TypeType)node.DeclaredType.TypeInfo;
            _lastNode = new CDeclaration { Name = GetVariableName(node.Name), Type = GetCType(type.Type) };
            return true;
        }

        protected override bool Visit(ASTReturn node)
        {
            if (!Visit(node.Child)) return false;

            _lastNode = new CReturn { Child = _lastNode };
            return true;
        }

        protected override bool Visit(ASTVariable node)
        {
            // This should be safe, if we did typechecking correct
            var type = (TypeType) node.DeclaredType.TypeInfo;

            // TODO: Handle structs and type variables

            if (!Visit(node.Value)) return false;
            _lastNode = new CDeclaration { Name = GetVariableName(node.Name), Type = GetCType(type.Type), Value = _lastNode };
            return true;
        }

        protected override bool Visit(ASTFloatLiteral node)
        {
            _lastNode = new CFloatLiteral { Value = node.Value };
            return true;
        }

        protected override bool Visit(ASTIntegerLiteral node)
        {
            _lastNode = new CIntegerLiteral { Value = node.Value };
            return true;
        }

        protected override bool Visit(ASTStringLiteral node)
        {
            _lastNode = new CCall
            {
                Callee = new CSymbol { Name = CreateStringProcedure },
                Arguments = new []{ new CStringLiteral { Value = node.Value } }
            };
            return true;
        }

        protected override bool Visit(ASTLambda node)
        {
            if (!Visit(node.Block)) return false;

            // This should be safe, if we did typechecking correct
            var type = (ProcedureType)node.TypeInfo;
            var arguments = node.Arguments.Select(arg => new CFunction.Argument { Name = arg.Name, Type = GetCType(arg.TypeInfo) });

            _lastNode = new CFunction
            {
                Name = $"{LambdaName}{_lambdaId++}",
                Arguments = arguments,
                Return = GetCType(type.Return),
                Block = (CBlock) _lastNode // Should be safe
            };

            return true;
        }

        private bool EmitBinary<T>(ASTBinaryNode node) where T : CBinary, new()
        {
            if (!Visit(node.Left)) return false;
            var left = _lastNode;

            if (!Visit(node.Right)) return false;
            var right = _lastNode;

            _lastNode = new T { Left = left, Right = right };
            return true;
        }

        protected override bool Visit(ASTAdd node)              => EmitBinary<CAdd>(node);
        protected override bool Visit(ASTAnd node)              => EmitBinary<CAnd>(node);
        protected override bool Visit(ASTDivide node)           => EmitBinary<CDiv>(node);
        protected override bool Visit(ASTDot node)              => EmitBinary<CDot>(node); // TODO, different for pointers
        protected override bool Visit(ASTEqual node)            => EmitBinary<CEqualEqual>(node);
        protected override bool Visit(ASTGreaterThan node)      => EmitBinary<CGreaterThan>(node);
        protected override bool Visit(ASTGreaterThanEqual node) => EmitBinary<CGreaterThanEqual>(node);
        protected override bool Visit(ASTLessThan node)         => EmitBinary<CLesserThan>(node);
        protected override bool Visit(ASTLessThanEqual node)    => EmitBinary<CLesserThanEqual>(node);
        protected override bool Visit(ASTModulo node)           => EmitBinary<CModulo>(node);
        protected override bool Visit(ASTNotEqual node)         => EmitBinary<CNotEqual>(node);
        protected override bool Visit(ASTOr node)               => EmitBinary<COr>(node);
        protected override bool Visit(ASTSub node)              => EmitBinary<CSub>(node);
        protected override bool Visit(ASTTimes node)            => EmitBinary<CMul>(node);

        
        private bool EmitUnary<T>(ASTUnaryNode node) where T : CUnary, new()
        {
            if (!Visit(node.Child)) return false;
            _lastNode = new T { Child = _lastNode };
            return true;
        }

        protected override bool Visit(ASTDereference node) => EmitUnary<CDereference>(node);
        protected override bool Visit(ASTNegative node)    => EmitUnary<CNegative>(node);
        protected override bool Visit(ASTNot node)         => EmitUnary<CNot>(node);
        protected override bool Visit(ASTPositive node)    => EmitUnary<CPositive>(node);
        protected override bool Visit(ASTReference node)   => EmitUnary<CReference>(node);

        protected override bool Visit(ASTAs node)
        {
            throw new NotImplementedException();
            return true;
        }


        protected override bool Visit(ASTCall node)
        {
            if (!Visit(node.Child)) return false;

            var callee = _lastNode;
            var arguments = new List<CNode>();

            foreach (var arg in node.Arguments)
            {
                if (!Visit(arg)) return false;
                arguments.Add(_lastNode);
            }

            _lastNode = new CCall
            {
                Callee = callee,
                Arguments = arguments
            };

            return true;
        }

        protected override bool Visit(ASTCompilerCall node)
        {
            throw new NotImplementedException();
        }

        protected override bool Visit(ASTIndexing node)
        {
            throw new NotImplementedException();
        }

        protected override bool Visit(ASTSymbol node)
        {
            _lastNode = new CSymbol { Name = GetVariableName(node.Name) };
            return true;
        }

        // TODO: Implement later
        protected override bool Visit(ASTStructInitializer node)
        {
            throw new NotImplementedException();
        }

        protected override bool Visit(ASTEmptyInitializer node)
        {
            throw new NotImplementedException();
        }

        protected override bool Visit(ASTArrayInitializer node)
        {
            throw new NotImplementedException();
        }

        // We should never visit an these nodes, as they should not be nessesary after type checking
        protected override bool Visit(ASTLambda.Argument node)
        {
            throw new NotImplementedException();
        }

        protected override bool Visit(ASTArrayType node)
        {
            throw new NotImplementedException();
        }

        protected override bool Visit(ASTInfer node)
        {
            throw new NotImplementedException();
        }

        protected override bool Visit(ASTProcedureType node)
        {
            throw new NotImplementedException();
        }

        protected override bool Visit(ASTStructType node)
        {
            throw new NotImplementedException();
        }

        private string GetVariableName(string name) => UserVariables + name;
    }
}
