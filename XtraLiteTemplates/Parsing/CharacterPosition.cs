using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtraLiteTemplates.Parsing
{
    public sealed class CharacterPosition
    {
        public Int32 Position { get; private set; }
        public Int32 Line { get; private set; }
        public Int32 PositionInLine { get; private set; }

        internal CharacterPosition(Int32 position, Int32 line, Int32 positionInLine)
        {
            Position = position;
            Line = line;
            PositionInLine = positionInLine;
        }
    }
}
