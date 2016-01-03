//  Author:
//    Alexandru Ciobanu alex@ciobanu.org
//
//  Copyright (c) 2015-2016, Alexandru Ciobanu (alex@ciobanu.org)
//
//  All rights reserved.
//
//  Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//
//     * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in
//       the documentation and/or other materials provided with the distribution.
//     * Neither the name of the author nor the names of contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
//  "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
//  LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
//  A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
//  CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
//  EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
//  PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
//  PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
//  LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
//  NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//  SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1634:FileHeaderMustShowCopyright", Justification = "Does not apply.")]

namespace XtraLiteTemplates.Expressions.Nodes
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;
    using XtraLiteTemplates.Expressions.Operators;

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
    internal static class LinqExpressionHelper    
    {
        public static readonly MethodInfo MethodInfoUnaryOperatorEvaluate = typeof(UnaryOperator).GetMethod("Evaluate", new[] { typeof(IExpressionEvaluationContext), typeof(object) });
        public static readonly MethodInfo MethodInfoBinaryOperatorEvaluateLhs = typeof(BinaryOperator).GetMethod("EvaluateLhs", new[] { typeof(IExpressionEvaluationContext), typeof(object), typeof(object).MakeByRefType() });
        public static readonly MethodInfo MethodInfoBinaryOperatorEvaluate = typeof(BinaryOperator).GetMethod("Evaluate", new[] { typeof(IExpressionEvaluationContext), typeof(object), typeof(object) });
        public static readonly MethodInfo MethodInfoExpressionEvaluationContextInvokeObject = typeof(IExpressionEvaluationContext).GetMethod("Invoke", new[] { typeof(object), typeof(string), typeof(object[]) });
        public static readonly MethodInfo MethodInfoExpressionEvaluationContextInvoke = typeof(IExpressionEvaluationContext).GetMethod("Invoke", new[] { typeof(string), typeof(object[]) });
        public static readonly MethodInfo MethodInfoExpressionEvaluationContextGetPropertyObject = typeof(IExpressionEvaluationContext).GetMethod("GetProperty", new[] { typeof(object), typeof(string) });
        public static readonly MethodInfo MethodInfoExpressionEvaluationContextGetProperty = typeof(IExpressionEvaluationContext).GetMethod("GetProperty", new[] { typeof(string) });

        public static readonly ParameterExpression ExpressionParameterContext = Expression.Parameter(typeof(IExpressionEvaluationContext));
        public static readonly MethodCallExpression ExpressionCallThrowIfCancellationRequested =
            Expression.Call(
                Expression.Property(ExpressionParameterContext, typeof(IExpressionEvaluationContext).GetProperty("CancellationToken")),
                typeof(CancellationToken).GetMethod("ThrowIfCancellationRequested"));
    }
}
