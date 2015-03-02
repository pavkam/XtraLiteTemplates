using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XtraLiteTemplates.Directives;
using XtraLiteTemplates.Evaluation;
using XtraLiteTemplates.Parsing;
using XtraLiteTemplates.Tom;
using XtraLiteTemplates.Utils;

namespace XtraLiteTemplates
{
    public class XtraLiteTemplate
    {
        public String Template { get; private set; }
        public ParserProperties ParserProperties { get; private set; }
        public DocumentTomNode CompiledTemplate { get; private set; }

        private void CompileTemplate()
        {
            var parser = new TemplateParser(ParserProperties, Template);
            var arbiter = new Arbiter(false);
            var builder = new TomDocumentBuilder(arbiter);

            parser.Parse(builder);
            CompiledTemplate = builder.GetDocument();
        }

        public XtraLiteTemplate(ParserProperties parserProperties, String template)
        {
            ValidationHelper.AssertArgumentIsNotNull("parserProperties", parserProperties);
            ValidationHelper.AssertArgumentIsNotNull("template", template);

            Template = template;
            ParserProperties = parserProperties;

            CompileTemplate();
        }

        public void Evaluate(TextWriter writer, params Variable[] variables)
        {
            ValidationHelper.AssertArgumentIsNotNull("writer", writer);

            /* Evaluate ... */
            var evaluationContext = new EvaluationContext(CompiledTemplate);

            /* Attachg the variable to the context. */
            foreach (var v in variables)
                evaluationContext.AssignVariable(v.Name, v.Value);

            evaluationContext.EvaluateChildren(writer);
        }

        public String Evaluate(params Variable[] variables)
        {
            using (var writer = new StringWriter())
            {
                Evaluate(writer, variables);
                return writer.ToString();
            }   
        }
    }
}
