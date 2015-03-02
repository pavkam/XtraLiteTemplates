using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XtraLiteTemplates.Definition;
using XtraLiteTemplates.Evaluation;
using XtraLiteTemplates.Parsing;
using XtraLiteTemplates.Parsing.ObjectModel;
using XtraLiteTemplates.Utils;

namespace XtraLiteTemplates
{
    public class XtraLiteTemplate
    {
        public String Template { get; private set; }
        public ParserProperties ParserProperties { get; private set; }
        public IDirectiveSelectionStrategy DirectiveSelector { get; private set; }
        public Boolean Strict { get; private set; }
        public TemplateDocument CompiledTemplate { get; private set; }

        private void CompileTemplate()
        {
            var parser = new TemplateParser(ParserProperties, DirectiveSelector, Strict, Template);
            CompiledTemplate = parser.Parse();
        }

        public XtraLiteTemplate(ParserProperties parserProperties, String template)
        {
            ValidationHelper.AssertArgumentIsNotNull("parserProperties", parserProperties);
            ValidationHelper.AssertArgumentIsNotNull("template", template);

            Template = template;
            ParserProperties = parserProperties;

            CompileTemplate();
        }
        
        public void Evaluate(TextWriter writer, IEvaluationContext evaluationContext)
        {
            ValidationHelper.AssertArgumentIsNotNull("writer", writer);
            ValidationHelper.AssertArgumentIsNotNull("evaluationContext", evaluationContext);

            CompiledTemplate.Evaluate(writer, evaluationContext);
        }

        public void Evaluate(TextWriter writer, IEqualityComparer<String> comparer, params Variable[] variables)
        {
            ValidationHelper.AssertArgumentIsNotNull("writer", writer);
            ValidationHelper.AssertArgumentIsNotNull("comparer", comparer);

            /* Evaluate ... */
            var evaluationContext = new EvaluationContext(comparer);

            /* Attachg the variable to the context. */
            foreach (var v in variables)
                evaluationContext.RegisterVariable(v.Name, v.Value);

            Evaluate(writer, evaluationContext);
        }

        public String Evaluate(Boolean caseSensitive, params Variable[] variables)
        {
            IEqualityComparer<String> comparer = caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;

            using (var writer = new StringWriter())
            {
                Evaluate(writer, comparer, variables);
                return writer.ToString();
            }   
        }
    }
}
