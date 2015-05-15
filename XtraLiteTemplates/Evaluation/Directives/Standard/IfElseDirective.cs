//
//  Author:
//    Alexandru Ciobanu alex@ciobanu.org
//
//  Copyright (c) 2015, Alexandru Ciobanu (alex@ciobanu.org)
//
//  All rights reserved.
//
//  Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//
//     * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in
//       the documentation and/or other materials provided with the distribution.
//     * Neither the name of the [ORGANIZATION] nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
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
//

namespace XtraLiteTemplates.Evaluation.Directives.Standard
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using XtraLiteTemplates.Parsing;
    using XtraLiteTemplates.Expressions.Operators.Standard;

    public sealed class IfElseDirective : StandardDirective
    {
        public IfElseDirective(String startTagMarkup, String midTagMarkup, String endTagMarkup, IPrimitiveTypeConverter typeConverter)
            : base(typeConverter, Tag.Parse(startTagMarkup), Tag.Parse(midTagMarkup), Tag.Parse(endTagMarkup))
        {
        }

        public IfElseDirective(IPrimitiveTypeConverter typeConverter)
            : this("IF $ THEN", "ELSE", "END IF", typeConverter)
        {
        }

        protected internal override FlowDecision Execute(Int32 tagIndex, Object[] components, ref Object state,
            IVariableContext context, out String text)
        {
            Debug.Assert(tagIndex >= 0 && tagIndex <= 2);
            Debug.Assert(components != null);
            Debug.Assert(context != null);

            text = null;
            if (tagIndex == 0)
            {
                Debug.Assert(components.Length == 3);

                var conditionIsTrue = TypeConverter.ConvertToBoolean(components[1]) == true;
                state = conditionIsTrue;

                return conditionIsTrue ? FlowDecision.Evaluate : FlowDecision.Skip;
            }
            else if (tagIndex == 1)
            {
                Debug.Assert(components.Length == 1);
                Debug.Assert(state is Boolean);
                var conditionWasTrue = (Boolean)state;

                if (!conditionWasTrue)
                    return FlowDecision.Evaluate;
            }
            
            return FlowDecision.Terminate;
        }
    }
}

