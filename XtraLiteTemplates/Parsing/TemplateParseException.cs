using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtraLiteTemplates.Parsing
{
    public sealed class TemplateParseException : Exception
    {
        public Char Character { get; private set; }

        public Int32 Position { get; private set; }
        public Int32 PositionInLine { get; private set; }
        public Int32 Line { get; private set; }

        internal TemplateParseException(Char character, Int32 position, Int32 line, Int32 positionInLine, String errorMessage) : base(errorMessage)
        {
            Character = character;
            Position = position;
            Line = line;
            PositionInLine = positionInLine;
        }
    }
}
