using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XtraLiteTemplates.Utils;

namespace XtraLiteTemplates.Parsing
{
    public sealed class DirectiveLiteral
    {
        public DirectiveLiteralType Type { get; private set; }
        public String Literal { get; private set; }

        public DirectiveLiteral(DirectiveLiteralType type, String literal)
        {
            ValidationHelper.AssertArgumentIsNotAnEmptyString("literal", literal);

            Type = type;
            Literal = literal;
        }
    }
}
