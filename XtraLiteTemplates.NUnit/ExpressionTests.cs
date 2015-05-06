using NUnit.Framework;
using System;

namespace XtraLiteTemplates.NUnit
{
    [TestFixture]
    public class ExpressionTests : TestBase
    {
        [Test]
        public void TestCaseContruction_1()
        {
            var expression = Expression.CreateStandardCStyle();

            expression.FeedConstant(1.00);
            expression.FeedSymbol("+");
            expression.FeedConstant(2.00);
            expression.FeedSymbol("*");
            expression.FeedConstant(3.00);
            expression.FeedSymbol("/");
            expression.FeedSymbol("(");
            expression.FeedConstant(4.00);            
            expression.FeedSymbol("+");
            expression.FeedConstant(5.00);
            expression.FeedSymbol(")");
            expression.Close();

            var repr = expression.ToString(Expression.FormattingStyle.Canonical);
            Assert.AreEqual("1 + 2", repr);
        }
    }
}

