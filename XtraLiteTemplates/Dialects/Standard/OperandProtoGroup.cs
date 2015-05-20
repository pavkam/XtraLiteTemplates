using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtraLiteTemplates.Dialects.Standard
{
    internal sealed class OperandProtoGroup : IEnumerable, IEnumerable<Object>
    {
        public Object[] Operands { get; private set; }

        public OperandProtoGroup(Object left, Object right)
        {
            var leftPg = left as OperandProtoGroup;
            var rightPg = right as OperandProtoGroup;

            var leftLength = leftPg != null ? leftPg.Operands.Length : 1;
            var rightLength = rightPg != null ? rightPg.Operands.Length : 1;

            Operands = new Object[leftLength + rightLength];
            if (leftPg != null)
                Array.Copy(leftPg.Operands, Operands, leftLength);
            else
                Operands[0] = left;

            if (rightPg != null)
                Array.Copy(rightPg.Operands, 0, Operands, leftLength, rightLength);
            else
                Operands[leftLength] = right;
        }
        
        public IEnumerator GetEnumerator()
        {
            return Operands.GetEnumerator();
        }

        IEnumerator<Object> IEnumerable<Object>.GetEnumerator()
        {
            foreach (var o in Operands)
            {
                yield return o;
            }
        }
    }
}
