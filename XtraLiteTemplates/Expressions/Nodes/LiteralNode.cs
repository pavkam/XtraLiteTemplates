//  Author:
//    Alexandru Ciobanu alex+git@ciobanu.org
//
//  Copyright (c) 2015-2018, Alexandru Ciobanu (alex+git@ciobanu.org)
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

namespace XtraLiteTemplates.Expressions.Nodes
{
    using System.Diagnostics;
    using System.Globalization;
    using System.Text;
    using LinqExpression = System.Linq.Expressions.Expression;

    internal class LiteralNode : LeafNode
    {
        public LiteralNode(ExpressionNode parent, object literal)
            : base(parent)
        {
            Literal = literal;
        }

        public object Literal { get; }

        public override string ToString(ExpressionFormatStyle style)
        {
            if (Literal is string)
            {
                var stringLiteral = (string)Literal;

                var result = new StringBuilder(stringLiteral.Length * 2);
                result.Append('\"');
                var enumerator = StringInfo.GetTextElementEnumerator(stringLiteral);
                while (enumerator.MoveNext())
                {
                    var segment = (string)enumerator.Current;

                    if (segment.Length == 1)
                    {
                        var c = segment[0];
                        switch (c)
                        {
                            case '\'':
                                result.Append("\\'");
                                break;
                            case '"':
                                result.Append("\\\"");
                                break;
                            case '\\':
                                result.Append("\\\\");
                                break;
                            case '\0':
                                result.Append("\\0");
                                break;
                            case '\a':
                                result.Append("\\a");
                                break;
                            case '\b':
                                result.Append("\\b");
                                break;
                            case '\f':
                                result.Append("\\f");
                                break;
                            case '\n':
                                result.Append("\\n");
                                break;
                            case '\r':
                                result.Append("\\r");
                                break;
                            case '\t':
                                result.Append("\\t");
                                break;
                            case '\v':
                                result.Append("\\v");
                                break;
                            default:
                                if (char.IsControl(c))
                                {
                                    result.Append($"\\u{(int)segment[0]:x4}");
                                }
                                else
                                {
                                    result.Append(c);
                                }
                                break;
                        }
                    }
                    else
                    {
                        result.Append(segment);
                    }
                }
                result.Append('\"');
                return result.ToString();
            }

            return Literal.ToString();
        }

        protected override bool TryReduce(IExpressionEvaluationContext reduceContext, out object value)
        {
            Debug.Assert(reduceContext != null, "reduceContext cannot be null.");

            value = Literal;
            return true;
        }

        protected override LinqExpression BuildLinqExpression()
        {
            return LinqExpression.Constant(Literal, typeof(object));
        }
    }
}