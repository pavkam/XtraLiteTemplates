using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XtraLiteTemplates.Evaluation;
using XtraLiteTemplates.Utils;

namespace XtraLiteTemplates.Parsing.ObjectModel
{
    public sealed class TextNode : TemplateNode
    {
        public String Text { get; private set; }

        internal TextNode(CompositeNode parent, String text)
            : base(parent)
        {
            Text = text;
        }

        public override Boolean Evaluate(TextWriter writer, Object evaluationContext)
        {
            ValidationHelper.AssertArgumentIsNotNull("writer", writer);

            if (!String.IsNullOrEmpty(Text))
                writer.Write(Text);

            return Text.Length;
        }
    }
}
