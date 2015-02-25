using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtraLiteTemplates.Parsing
{
    internal sealed class StringForwardCursor
    {
        public String Value { get; private set; }

        public Int32 CurrentCharacterPosition
        {
            get
            {
                return m_nextCharacterIndex;
            }
        }

        public Int32 CurrentCharacterPositionInLine
        {
            get
            {
                return m_nextCharacterIndexInLine;
            }
        }

        public Int32 CurrentCharacterLine { get; private set; }


        public Int32 TotalCharacters
        {
            get
            {
                return Value.Length;
            }
        }

        public Int32 RemainingCharacters
        {
            get
            {
                return Value.Length - m_nextCharacterIndex;
            }
        }

        public Boolean EndOfString
        {
            get
            {
                return m_nextCharacterIndex > Value.Length;
            }
        }

        private Int32 m_nextCharacterIndex;
        private Int32 m_nextCharacterIndexInLine;
        private Boolean m_previousCharacterWasLineFeed;

        public StringForwardCursor(String value)
        {
            Debug.Assert(value != null);

            Value = value;
            m_nextCharacterIndex = 0;
            m_previousCharacterWasLineFeed = false;
            CurrentCharacterLine = 0;
            CurrentCharacterPositionInLine = 0;
        }

        public Boolean TryReadNext(out Char character)
        {
            character = (Char)0;
            if (EndOfString)
                return false;
            else
            {
                if (m_previousCharacterWasLineFeed || m_nextCharacterIndex == 0)
                {
                    m_previousCharacterWasLineFeed = false;

                    m_nextCharacterIndexInLine = 0;
                    CurrentCharacterLine++;
                }

                /* Read the next character from the "stream". */
                character = Value[m_nextCharacterIndex];
                if (character == '\n')
                    m_previousCharacterWasLineFeed = true;

                m_nextCharacterIndex++;
                m_nextCharacterIndexInLine++;

                return true;
            }
        }

        public Char ReadNext()
        {
            Char result;
            if (!TryReadNext(out result))
                throw new InvalidOperationException("Cannot read any more characters from the input string as the end has been reached.");

            return result;
        }
    }
}
