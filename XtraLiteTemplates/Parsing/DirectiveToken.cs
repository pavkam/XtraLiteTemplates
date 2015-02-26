using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtraLiteTemplates.Parsing
{
    internal sealed class DirectiveToken
    {
        public enum TokenType
        {
            Identifier,
            Numerical,
            String,
            Symbolic,
        }

        public TokenType Type { get; private set; }
        public String Value { get; private set; }


        public DirectiveToken(TokenType type, String value)
        {
            Type = type;
            Value = value;
        }
    }
}
