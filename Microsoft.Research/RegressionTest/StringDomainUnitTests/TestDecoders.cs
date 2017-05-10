using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.CodeDom.Compiler;
using System.Collections;
using Microsoft.Research.AbstractDomains.Expressions;
using Microsoft.Research.DataStructures;
using Microsoft.Research.CodeAnalysis;

namespace StringDomainUnitTests
{
    /// <summary>
    /// Partial implementation of IExpressionDecoder for testing.
    /// </summary>
    class TestExpressionDecoder : IExpressionDecoder<TestVariable, BoxedExpression>
    {
        public object Constant(BoxedExpression exp)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<BoxedExpression> Disjunctions(BoxedExpression exp)
        {
            throw new NotImplementedException();
        }

        public bool IsBinaryExpression(BoxedExpression exp)
        {
            throw new NotImplementedException();
        }

        public bool IsConstant(BoxedExpression exp)
        {
            throw new NotImplementedException();
        }

        public bool IsConstantInt(BoxedExpression exp, out int val)
        {
            if (exp.IsConstant)
            {
                object value = exp.Constant;
                if (value is int)
                {
                    val = (int)value;
                    return true;
                }
            }

            val = 0;
            return false;
        }

        public bool IsFrameworkVariable(TestVariable v)
        {
            throw new NotImplementedException();
        }

        public bool IsNaN(BoxedExpression exp)
        {
            throw new NotImplementedException();
        }

        public bool IsNull(BoxedExpression exp)
        {
            throw new NotImplementedException();
        }

        public bool IsSizeOf(BoxedExpression exp)
        {
            throw new NotImplementedException();
        }

        public bool IsSlackOrFrameworkVariable(TestVariable v)
        {
            throw new NotImplementedException();
        }

        public bool IsSlackVariable(TestVariable v)
        {
            throw new NotImplementedException();
        }

        public bool IsUnaryExpression(BoxedExpression exp)
        {
            throw new NotImplementedException();
        }

        public bool IsVariable(BoxedExpression exp)
        {
            throw new NotImplementedException();
        }

        public bool IsWritableBytes(BoxedExpression exp)
        {
            throw new NotImplementedException();
        }

        public BoxedExpression LeftExpressionFor(BoxedExpression exp)
        {
            throw new NotImplementedException();
        }

        public string NameOf(TestVariable exp)
        {
            return exp.Name;
        }

        public ExpressionOperator OperatorFor(BoxedExpression exp)
        {
            if (exp.IsConstant)
                return ExpressionOperator.Constant;
            else if (exp.IsVariable)
                return ExpressionOperator.Variable;
            else
                return ExpressionOperator.Unknown;
        }

        public BoxedExpression RightExpressionFor(BoxedExpression exp)
        {
            throw new NotImplementedException();
        }

        public BoxedExpression Stripped(BoxedExpression exp)
        {
            throw new NotImplementedException();
        }

        public bool TryGetAssociatedExpression(BoxedExpression exp, AssociatedInfo infoKind, out BoxedExpression info)
        {
            throw new NotImplementedException();
        }

        public bool TryGetAssociatedExpression(APC pc, BoxedExpression exp, AssociatedInfo infoKind, out BoxedExpression info)
        {
            throw new NotImplementedException();
        }

        public bool TrySizeOf(BoxedExpression type, out int value)
        {
            throw new NotImplementedException();
        }

        public bool TryValueOf<T>(BoxedExpression exp, ExpressionType aiType, out T value)
        {
            if (exp.IsConstant && TypeOf(exp) == aiType)
            {
                value = (T)exp.Constant;
                return true;
            }
            else
            {
                value = default(T);
                return false;
            }
        }

        public ExpressionType TypeOf(BoxedExpression exp)
        {
            if (exp.IsConstant)
            {
                Type valueType = exp.Constant.GetType();
                if (valueType == typeof(string))
                    return ExpressionType.String;
                else if (valueType == typeof(int))
                    return ExpressionType.Int32;
                else if (valueType == typeof(bool))
                    return ExpressionType.Bool;


            }
            else if (exp.IsVariable)
            {
                TestVariable var = UnderlyingVariable(exp);

                return var.expressionType;

            }
            return ExpressionType.Unknown;
        }

        public TestVariable UnderlyingVariable(BoxedExpression exp)
        {
            return (TestVariable)exp.UnderlyingVariable;
        }

        public Set<TestVariable> VariablesIn(BoxedExpression exp)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Partial implementation of IDecodeMetaData for testing.
    /// </summary>
    class TestMdDecoder : IDecodeMetaData<int, int, int, int, int, int, Type, int, int>
    {
        public bool IsPlatformInitialized
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string SharedContractClassAssembly
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public Type System_Array
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Type System_Boolean
        {
            get
            {
                return typeof(bool);
            }
        }

        public Type System_Char
        {
            get
            {
                return typeof(char);
            }
        }

        public Type System_Decimal
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Type System_Double
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Type System_DynamicallyTypedReference
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Type System_Int16
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Type System_Int32
        {
            get
            {
                return typeof(int);
            }
        }

        public Type System_Int64
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Type System_Int8
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Type System_IntPtr
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Type System_Object
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Type System_RuntimeArgumentHandle
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Type System_RuntimeFieldHandle
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Type System_RuntimeMethodHandle
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Type System_RuntimeTypeHandle
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Type System_Single
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Type System_String
        {
            get
            {
                return typeof(string);
            }
        }

        public Type System_Type
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Type System_UInt16
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Type System_UInt32
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Type System_UInt64
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Type System_UInt8
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Type System_UIntPtr
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Type System_Void
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Result AccessMethodBody<Data, Result>(int method, IMethodCodeConsumer<int, int, int, int, Type, Data, Result> consumer, Data data)
        {
            throw new NotImplementedException();
        }

        public IIndexable<Type> ActualTypeArguments(int method)
        {
            throw new NotImplementedException();
        }

        public int ArgumentIndex(int p)
        {
            throw new NotImplementedException();
        }

        public int ArgumentStackIndex(int p)
        {
            throw new NotImplementedException();
        }

        public Type ArrayType(Type type, int rank)
        {
            throw new NotImplementedException();
        }

        public Guid AssemblyGuid(int assembly)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<int> AssemblyReferences(int assembly)
        {
            throw new NotImplementedException();
        }

        public int AttributeConstructor(int attribute)
        {
            throw new NotImplementedException();
        }

        public Type AttributeType(int attribute)
        {
            throw new NotImplementedException();
        }

        public Type BaseClass(Type type)
        {
            throw new NotImplementedException();
        }

        public int ConstructorsCount(Type type)
        {
            throw new NotImplementedException();
        }

        public int DeclaringAssembly(int method)
        {
            throw new NotImplementedException();
        }

        public string DeclaringMemberCanonicalName(int method)
        {
            throw new NotImplementedException();
        }

        public int DeclaringMethod(int p)
        {
            throw new NotImplementedException();
        }

        public Guid DeclaringModule(Type type)
        {
            throw new NotImplementedException();
        }

        public string DeclaringModuleName(Type type)
        {
            throw new NotImplementedException();
        }

        public Type DeclaringType(int method)
        {
            throw new NotImplementedException();
        }

        public bool DerivesFrom(Type sub, Type super)
        {
            throw new NotImplementedException();
        }

        public bool DerivesFromIgnoringTypeArguments(Type sub, Type super)
        {
            throw new NotImplementedException();
        }

        public string DocumentationId(Type type)
        {
            throw new NotImplementedException();
        }

        public string DocumentationId(int field)
        {
            throw new NotImplementedException();
        }

        public Type ElementType(Type type)
        {
            throw new NotImplementedException();
        }

        public bool Equal(Type type1, Type type2)
        {
            return type1 == type2;
        }

        public bool Equal(int m1, int m2)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<int> Events(Type type)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<int> Fields(Type type)
        {
            throw new NotImplementedException();
        }

        public Type FieldType(int field)
        {
            throw new NotImplementedException();
        }

        public Type FormalTypeParameterDefiningType(Type type)
        {
            throw new NotImplementedException();
        }

        public string FullName(Type type)
        {
            throw new NotImplementedException();
        }

        public string FullName(int field)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<int> GetAttributes(int method)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<int> GetAttributes(Type type)
        {
            throw new NotImplementedException();
        }

        public int GetPropertyFromAccessor(int method)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Type> GetTypes(int assembly)
        {
            throw new NotImplementedException();
        }

        public Type HandlerType(int e)
        {
            throw new NotImplementedException();
        }

        public bool HasAdder(int @event, out int adder)
        {
            throw new NotImplementedException();
        }

        public bool HasBaseClass(Type type)
        {
            throw new NotImplementedException();
        }

        public bool HasBody(int method)
        {
            throw new NotImplementedException();
        }

        public bool HasFlagsAttribute(Type type)
        {
            throw new NotImplementedException();
        }

        public bool HasGetter(int property, out int getter)
        {
            throw new NotImplementedException();
        }

        public bool HasRemover(int @event, out int remover)
        {
            throw new NotImplementedException();
        }

        public bool HasSetter(int property, out int setter)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<int> ImplementedMethods(int method)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Type> Interfaces(Type type)
        {
            throw new NotImplementedException();
        }

        public bool IsAbstract(int method)
        {
            throw new NotImplementedException();
        }

        public bool IsAbstract(Type type)
        {
            throw new NotImplementedException();
        }

        public bool IsArray(Type type)
        {
            throw new NotImplementedException();
        }

        public bool IsAsVisibleAs(Type t, int m)
        {
            throw new NotImplementedException();
        }

        public bool IsAsVisibleAs(int m1, int m2)
        {
            throw new NotImplementedException();
        }

        public bool IsAsyncMoveNext(int method)
        {
            throw new NotImplementedException();
        }

        public bool IsAutoPropertyMember(int method)
        {
            throw new NotImplementedException();
        }

        public bool IsAutoPropertySetter(int method, out int backingField)
        {
            throw new NotImplementedException();
        }

        public bool IsClass(Type type)
        {
            throw new NotImplementedException();
        }

        public bool IsCompilerGenerated(Type type)
        {
            throw new NotImplementedException();
        }

        public bool IsCompilerGenerated(int method)
        {
            throw new NotImplementedException();
        }

        public bool IsConstructor(int method)
        {
            throw new NotImplementedException();
        }

        public bool IsConstructorConstrained(Type type)
        {
            throw new NotImplementedException();
        }

        public bool IsDebuggerNonUserCode(Type t)
        {
            throw new NotImplementedException();
        }

        public bool IsDebuggerNonUserCode(int method)
        {
            throw new NotImplementedException();
        }

        public bool IsDelegate(Type type)
        {
            throw new NotImplementedException();
        }

        public bool IsDispose(int method)
        {
            throw new NotImplementedException();
        }

        public bool IsEnum(Type type)
        {
            throw new NotImplementedException();
        }

        public bool IsEventAdder(int method, out int @event)
        {
            throw new NotImplementedException();
        }

        public bool IsEventRemover(int method, out int @event)
        {
            throw new NotImplementedException();
        }

        public bool IsExtern(int method)
        {
            throw new NotImplementedException();
        }

        public bool IsFinalizer(int method)
        {
            throw new NotImplementedException();
        }

        public bool IsFormalTypeParameter(Type type)
        {
            throw new NotImplementedException();
        }

        public bool IsGeneric(int method, out IIndexable<Type> formals)
        {
            throw new NotImplementedException();
        }

        public bool IsGeneric(Type type, out IIndexable<Type> formals, bool normalized)
        {
            throw new NotImplementedException();
        }

        public bool IsImplicitImplementation(int method)
        {
            throw new NotImplementedException();
        }

        public bool IsInterface(Type type)
        {
            throw new NotImplementedException();
        }

        public bool IsInternal(Type type)
        {
            throw new NotImplementedException();
        }

        public bool IsInternal(int field)
        {
            throw new NotImplementedException();
        }

        public bool IsMain(int method)
        {
            throw new NotImplementedException();
        }

        public bool IsManagedPointer(Type type)
        {
            throw new NotImplementedException();
        }

        public bool IsMethodFormalTypeParameter(Type type)
        {
            throw new NotImplementedException();
        }

        public bool IsModified(Type type, out Type modified, out IIndexable<Pair<bool, Type>> modifiers)
        {
            throw new NotImplementedException();
        }

        public bool IsNativeCpp(Type type)
        {
            throw new NotImplementedException();
        }

        public bool IsNativePointerType(Type declaringType)
        {
            throw new NotImplementedException();
        }

        public bool IsNested(Type type, out Type parentType)
        {
            throw new NotImplementedException();
        }

        public bool IsNewSlot(int field)
        {
            throw new NotImplementedException();
        }

        public bool IsOut(int p)
        {
            throw new NotImplementedException();
        }

        public bool IsOverride(int p)
        {
            throw new NotImplementedException();
        }

        public bool IsPinned(int local)
        {
            throw new NotImplementedException();
        }

        public bool IsPrimitive(Type type)
        {
            throw new NotImplementedException();
        }

        public bool IsPrivate(Type type)
        {
            throw new NotImplementedException();
        }

        public bool IsPrivate(int field)
        {
            throw new NotImplementedException();
        }

        public bool IsPropertyGetter(int method)
        {
            throw new NotImplementedException();
        }

        public bool IsPropertySetter(int method)
        {
            throw new NotImplementedException();
        }

        public bool IsProtected(Type type)
        {
            throw new NotImplementedException();
        }

        public bool IsProtected(int field)
        {
            throw new NotImplementedException();
        }

        public bool IsPublic(Type type)
        {
            throw new NotImplementedException();
        }

        public bool IsPublic(int field)
        {
            throw new NotImplementedException();
        }

        public bool IsReadonly(int field)
        {
            throw new NotImplementedException();
        }

        public bool IsReferenceConstrained(Type type)
        {
            throw new NotImplementedException();
        }

        public bool IsReferenceType(Type type)
        {
            throw new NotImplementedException();
        }

        public bool IsSealed(Type type)
        {
            throw new NotImplementedException();
        }

        public bool IsSealed(int p)
        {
            throw new NotImplementedException();
        }

        public bool IsSpecialized(int method)
        {
            throw new NotImplementedException();
        }

        public bool IsSpecialized(Type type, out IIndexable<Type> typeArguments)
        {
            throw new NotImplementedException();
        }

        public bool IsSpecialized(int method, ref IFunctionalMap<Type, Type> specialization)
        {
            throw new NotImplementedException();
        }

        public bool IsSpecialized(int method, out int genericMethod, out IIndexable<Type> methodTypeArguments)
        {
            throw new NotImplementedException();
        }

        public bool IsStatic(Type type)
        {
            throw new NotImplementedException();
        }

        public bool IsStatic(int field)
        {
            throw new NotImplementedException();
        }

        public bool IsStruct(Type type)
        {
            throw new NotImplementedException();
        }

        public bool IsUnmanagedPointer(Type type)
        {
            throw new NotImplementedException();
        }

        public bool IsValueConstrained(Type type)
        {
            throw new NotImplementedException();
        }

        public bool IsVirtual(int method)
        {
            throw new NotImplementedException();
        }

        public bool IsVisibleFrom(Type t, Type tfrom)
        {
            throw new NotImplementedException();
        }

        public bool IsVisibleFrom(int m, Type t)
        {
            throw new NotImplementedException();
        }

        public bool IsVisibleOutsideAssembly(Type type)
        {
            throw new NotImplementedException();
        }

        public bool IsVisibleOutsideAssembly(int property)
        {
            throw new NotImplementedException();
        }

        public bool IsVoid(Type type)
        {
            throw new NotImplementedException();
        }

        public bool IsVoidMethod(int method)
        {
            throw new NotImplementedException();
        }

        public bool IsVolatile(int field)
        {
            throw new NotImplementedException();
        }

        public IIndexable<int> Locals(int method)
        {
            throw new NotImplementedException();
        }

        public Type LocalType(int local)
        {
            throw new NotImplementedException();
        }

        public Type ManagedPointer(Type type)
        {
            throw new NotImplementedException();
        }

        public int MethodFormalTypeDefiningMethod(Type type)
        {
            throw new NotImplementedException();
        }

        public int MethodFormalTypeParameterIndex(Type type)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<int> Methods(Type type)
        {
            throw new NotImplementedException();
        }

        public int MethodToken(int method)
        {
            throw new NotImplementedException();
        }

        public int? MoveNextStartState(int method)
        {
            throw new NotImplementedException();
        }

        public string Name(Type type)
        {
            throw new NotImplementedException();
        }

        public string Name(int field)
        {
            throw new NotImplementedException();
        }

        public object NamedArgument(string name, int attribute)
        {
            throw new NotImplementedException();
        }

        public string Namespace(Type type)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Type> NestedTypes(Type type)
        {
            throw new NotImplementedException();
        }

        public IIndexable<Type> NormalizedActualTypeArguments(Type type)
        {
            throw new NotImplementedException();
        }

        public int NormalizedFormalTypeParameterIndex(Type type)
        {
            throw new NotImplementedException();
        }

        public bool NormalizedIsSpecialized(Type type, out IIndexable<Type> typeArguments)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<int> OverriddenAndImplementedMethods(int method)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<int> OverriddenMethods(int method)
        {
            throw new NotImplementedException();
        }

        public IIndexable<int> Parameters(int method)
        {
            throw new NotImplementedException();
        }

        public Type ParameterType(int p)
        {
            throw new NotImplementedException();
        }

        public IIndexable<object> PositionalArguments(int attribute)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<int> Properties(Type type)
        {
            throw new NotImplementedException();
        }

        public Type PropertyType(int property)
        {
            throw new NotImplementedException();
        }

        public int Rank(Type type)
        {
            throw new NotImplementedException();
        }

        public Type ReturnType(int method)
        {
            throw new NotImplementedException();
        }

        public void SetTargetPlatform(string framework, IDictionary assemblyCache, string platform, IEnumerable<string> resolved, IEnumerable<string> libPaths, Action<CompilerError> errorHandler, bool trace)
        {
            throw new NotImplementedException();
        }

        public Type Specialize(Type type, Type[] typeArguments)
        {
            throw new NotImplementedException();
        }

        public int Specialize(int method, Type[] methodTypeArguments)
        {
            throw new NotImplementedException();
        }

        public int This(int method)
        {
            throw new NotImplementedException();
        }

        public bool TryGetImplementingMethod(Type type, int baseMethod, out int implementingMethod)
        {
            throw new NotImplementedException();
        }

        public bool TryGetRootMethod(int method, out int rootMethod)
        {
            throw new NotImplementedException();
        }

        public bool TryGetSystemType(string fullName, out Type type)
        {
            throw new NotImplementedException();
        }

        public bool TryInitialValue(int field, out object value)
        {
            throw new NotImplementedException();
        }

        public bool TryLoadAssembly(string fileName, IDictionary assemblyCache, Action<CompilerError> errorHandler, out int assembly, bool legacyContractMode, List<string> referencedAssemblies, bool extractSourceText)
        {
            throw new NotImplementedException();
        }

        public Type TypeEnum(Type type)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Type> TypeParameterConstraints(Type type)
        {
            throw new NotImplementedException();
        }

        public int TypeSize(Type type)
        {
            throw new NotImplementedException();
        }

        public Type UnmanagedPointer(Type type)
        {
            throw new NotImplementedException();
        }

        public Type Unspecialized(Type type)
        {
            throw new NotImplementedException();
        }

        public int Unspecialized(int method)
        {
            throw new NotImplementedException();
        }

        public Version Version(int assembly)
        {
            throw new NotImplementedException();
        }
    }
}
