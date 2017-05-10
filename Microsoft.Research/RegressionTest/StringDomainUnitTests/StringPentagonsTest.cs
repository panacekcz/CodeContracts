using Microsoft.Research.AbstractDomains.Strings;
using Microsoft.Research.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StringDomainUnitTests
{
    using StringPentagons = StringPentagons<TestVariable, BoxedExpression, PrefixInterval>;

    [TestClass]
    public class StringPentagonsTest
    {
        private TestExpressionDecoder decoder = new TestExpressionDecoder();
        private PrefixInterval.Operations<TestVariable> operations = new PrefixInterval.Operations<TestVariable>();

        private BoxedExpression stringVarExp1 = BoxedExpression.Var(TestVariable.Var1);
        private BoxedExpression stringVarExp2 = BoxedExpression.Var(TestVariable.Var2);
        private BoxedExpression stringVarExp3 = BoxedExpression.Var(TestVariable.Var3);
        private BoxedExpression boolVarExp = BoxedExpression.Var(TestVariable.BoolVar);
        private BoxedExpression boolVarExp2 = BoxedExpression.Var(TestVariable.BoolVar2);

        private TestMdDecoder metadataDecoder = new TestMdDecoder();

        private BoxedExpression stringConstExp;

        public StringPentagonsTest() {
            stringConstExp = BoxedExpression.Const("const", typeof(string), metadataDecoder);
        }

        [TestMethod]
        public void TestPentagonsStartsWithConst()
        {
            StringPentagons pentagons = new StringPentagons(decoder, operations);
            
            BoxedExpression comparisonExp = BoxedExpression.Const((int)StringComparison.Ordinal, typeof(int), metadataDecoder);

            pentagons.Copy(stringVarExp1, stringConstExp);
            pentagons.StartsEndsWith(boolVarExp, stringVarExp1, stringConstExp, comparisonExp, false);

            Assert.AreEqual(ProofOutcome.True, pentagons.EvalBool(TestVariable.BoolVar));
        }

        [TestMethod]
        public void TestPentagonsConcatStartsWithVar()
        {
            StringPentagons pentagons = new StringPentagons(decoder, operations);

            BoxedExpression comparisonExp = BoxedExpression.Const((int)StringComparison.Ordinal, typeof(int), metadataDecoder);

            // Concatenate Var2 and Var3 into Var1 and test that Var1 starts with Var 2
            pentagons.Concat(stringVarExp1, stringVarExp2, stringVarExp3);
            pentagons.StartsEndsWith(boolVarExp, stringVarExp1, stringVarExp2, comparisonExp, false);

            Assert.AreEqual(ProofOutcome.True, pentagons.EvalBool(TestVariable.BoolVar));

            // Test that we don't know if Var1 starts with Var 3
            pentagons.RemoveVariable(TestVariable.BoolVar);
            pentagons.StartsEndsWith(boolVarExp, stringVarExp1, stringVarExp3, comparisonExp, false);

            Assert.AreEqual(ProofOutcome.Top, pentagons.EvalBool(TestVariable.BoolVar));
        }


        [TestMethod]
        public void TestPentagonsStartsWithVar()
        {
            StringPentagons pentagons = new StringPentagons(decoder, operations);

            BoxedExpression comparisonExp = BoxedExpression.Const((int)StringComparison.Ordinal, typeof(int), metadataDecoder);

            pentagons.Copy(stringVarExp1, stringVarExp2);
            pentagons.StartsEndsWith(boolVarExp, stringVarExp1, stringVarExp2, comparisonExp, false);

            Assert.AreEqual(ProofOutcome.True, pentagons.EvalBool(TestVariable.BoolVar));
        }



        [TestMethod]
        public void TestPentagonsAssume()
        {
            StringPentagons pentagons = new StringPentagons(decoder, operations);

            BoxedExpression comparisonExp = BoxedExpression.Const((int)StringComparison.Ordinal, typeof(int), metadataDecoder);

            // Assume the condition
            pentagons.StartsEndsWith(boolVarExp, stringVarExp1, stringVarExp2, comparisonExp, false);
            pentagons.TestTrue(boolVarExp);
            Assert.AreEqual(ProofOutcome.True, pentagons.EvalBool(TestVariable.BoolVar));

            // Assert the same condition
            pentagons.StartsEndsWith(boolVarExp2, stringVarExp1, stringVarExp2, comparisonExp, false);
            Assert.AreEqual(ProofOutcome.True, pentagons.EvalBool(TestVariable.BoolVar2));
        }

        [TestMethod]
        public void TestPentagonsAssumeMutation1()
        {
            StringPentagons pentagons = new StringPentagons(decoder, operations);

            BoxedExpression comparisonExp = BoxedExpression.Const((int)StringComparison.Ordinal, typeof(int), metadataDecoder);

            // Assume the condition
            pentagons.StartsEndsWith(boolVarExp, stringVarExp1, stringVarExp2, comparisonExp, false);
            pentagons.TestTrue(boolVarExp);
            Assert.AreEqual(ProofOutcome.True, pentagons.EvalBool(TestVariable.BoolVar));

            // Mutate one of the variables
            pentagons.Mutate(stringVarExp1);

            // Assume the same condition
            pentagons.StartsEndsWith(boolVarExp2, stringVarExp1, stringVarExp2, comparisonExp, false);
            Assert.AreEqual(ProofOutcome.Top, pentagons.EvalBool(TestVariable.BoolVar2));
        }

        [TestMethod]
        public void TestPentagonsAssumeMutation2()
        {
            StringPentagons pentagons = new StringPentagons(decoder, operations);

            BoxedExpression comparisonExp = BoxedExpression.Const((int)StringComparison.Ordinal, typeof(int), metadataDecoder);

            // Assume the condition
            pentagons.StartsEndsWith(boolVarExp, stringVarExp1, stringVarExp2, comparisonExp, false);
            pentagons.TestTrue(boolVarExp);
            Assert.AreEqual(ProofOutcome.True, pentagons.EvalBool(TestVariable.BoolVar));

            // Mutate one of the variables
            pentagons.Mutate(stringVarExp2);

            // Assume the same condition
            pentagons.StartsEndsWith(boolVarExp2, stringVarExp1, stringVarExp2, comparisonExp, false);
            Assert.AreEqual(ProofOutcome.Top, pentagons.EvalBool(TestVariable.BoolVar2));
        }

        [TestMethod]
        public void TestPentagonsMutationAssume1()
        {
            StringPentagons pentagons = new StringPentagons(decoder, operations);

            BoxedExpression comparisonExp = BoxedExpression.Const((int)StringComparison.Ordinal, typeof(int), metadataDecoder);

            // Assume the condition
            pentagons.StartsEndsWith(boolVarExp, stringVarExp1, stringVarExp2, comparisonExp, false);

            // Mutate one of the variables
            pentagons.Mutate(stringVarExp1);

            pentagons.TestTrue(boolVarExp);
            Assert.AreEqual(ProofOutcome.True, pentagons.EvalBool(TestVariable.BoolVar));

            // Assume the same condition
            pentagons.StartsEndsWith(boolVarExp2, stringVarExp1, stringVarExp2, comparisonExp, false);
            Assert.AreEqual(ProofOutcome.Top, pentagons.EvalBool(TestVariable.BoolVar2));
        }

        [TestMethod]
        public void TestPentagonsMutationAssume2()
        {
            StringPentagons pentagons = new StringPentagons(decoder, operations);

            BoxedExpression comparisonExp = BoxedExpression.Const((int)StringComparison.Ordinal, typeof(int), metadataDecoder);

            // Assume the condition
            pentagons.StartsEndsWith(boolVarExp, stringVarExp1, stringVarExp2, comparisonExp, false);

            // Mutate one of the variables
            pentagons.Mutate(stringVarExp2);

            pentagons.TestTrue(boolVarExp);
            Assert.AreEqual(ProofOutcome.True, pentagons.EvalBool(TestVariable.BoolVar));

            // Assume the same condition
            pentagons.StartsEndsWith(boolVarExp2, stringVarExp1, stringVarExp2, comparisonExp, false);
            Assert.AreEqual(ProofOutcome.Top, pentagons.EvalBool(TestVariable.BoolVar2));
        }
    }
}
