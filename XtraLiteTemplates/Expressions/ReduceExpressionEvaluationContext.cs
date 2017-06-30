//  Author:
//    Alexandru Ciobanu alex+git@ciobanu.org
//
//  Copyright (c) 2015-2017, Alexandru Ciobanu (alex+git@ciobanu.org)
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

namespace XtraLiteTemplates.Expressions
{
    using System.Diagnostics;
    using System.Threading;
    using JetBrains.Annotations;

    internal sealed class ReduceExpressionEvaluationContext : IExpressionEvaluationContext
    {
        [NotNull]
        public static readonly IExpressionEvaluationContext Instance = new ReduceExpressionEvaluationContext();

        public CancellationToken CancellationToken => CancellationToken.None;

        [ContractAnnotation("=> halt")]
        public void AddStateObject(object state)
        {
            Debug.Fail("Invalid operation.");
        }

        [ContractAnnotation("=> halt")]
        public void RemoveStateObject(object state)
        {
            Debug.Fail("Invalid operation.");
        }

        [ContractAnnotation("=> halt")]
        public bool ContainsStateObject(object state)
        {
            Debug.Fail("Invalid operation.");
            return false;
        }

        [ContractAnnotation("=> halt")]
        public void SetProperty(string property, object value)
        {
            Debug.Fail("Invalid operation.");
        }

        [ContractAnnotation("=> halt")]
        public object GetProperty(string property)
        {
            Debug.Fail("Invalid operation.");
            return false;
        }

        [ContractAnnotation("=> halt")]
        public object GetProperty(object @object, string property)
        {
            Debug.Fail("Invalid operation.");
            return false;
        }

        [ContractAnnotation("=> halt")]
        public object Invoke(string method, object[] arguments)
        {
            Debug.Fail("Invalid operation.");
            return false;
        }

        [ContractAnnotation("=> halt")]
        public object Invoke(object @object, string method, object[] arguments)
        {
            Debug.Fail("Invalid operation.");
            return false;
        }
    }
}
