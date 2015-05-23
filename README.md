# XtraLiteTemplates [![Build status](https://ci.appveyor.com/api/projects/status/gapq9gvrneooy1ob/branch/master?svg=true)](https://ci.appveyor.com/project/pavkam/xtralitetemplates/branch/master)
A lightweight templating engine for .NET Framework

### What it is and what it is not
XtraLiteTemplates is a fully managed .NET Framework library that offers easy string templating features. XtraLiteTemplates offers support for `custom template constructs` (akin to statements), and support for `expressions` with custom operators. XtraLiteTemplates is not a programming language or a domain-specific language in its own right, its sole purpose is to aid in the creation and manipulation of string templates. Anything else is out of scope of this project.

### Usage Examples
The easiest way to evaluate a template is by using the built-in facade class ```XLTemplate```. It works in conjunction with an instance of ```IDialect``` interface to parse and evaluate a template:

```c#
  var customer = new
  {
      FirstName = "John",
      LastName = "McMann",
      Age = 31,
      Loves = new String [] { "Apples", "Bikes", "Everything Nice" }
  };

  var result = XLTemplate.Evaluate(CodeMonkeyDialect.DefaultIgnoreCase, 
    @"{pre}Hello, {_0.FirstName} {_0.LastName}. You are {_0.Age} years old and you love: {for entity in _0.loves}{entity}, {end}{end}", customer);
  
  Console.WriteLine(result);
```
Will display `"Hello, John McMann. You are 31 years old and you love: Apples,Bikes,Everything Nice,"`

### Core Concepts & Building Blocks
Put in the simplest of terms, the process of evaluation of a template follows the following pattern:
* Tokenization, in which a string is split into tokens. The `tokenizer` requires a set of control characters that drive the process (such as the *directive open & close* characters or *string literal escape* characters).
* Lexical analysis. The `lexer` tries to make sense of the tokens supplied by the tokenization process using a set of given *tags* and *expression operators*.
* Interpretation. The output of lexical analysis is a stream of *lex* objects. The `interpreter` will try to match these objects with known *directives* and build a very simplistic `AST`. The interpretation process will return an `IEvaluable` interface to the caller. This can be considered as being a *compiled template*, ready to be evaluated.
* Evaluation can pe performed any number of times on the interpreted template. The caller must provide an `IEvaluationContext` object that offers the state, variable access, and other functionality.

#### Expressions
The expression builder in XtraLiteTemplates allows for:
* Any number of operators to be defined.
* Either symbols or words can be used to define an operator, but not both at the same time (e.g. "type-of" would be illegal, while "type_of" would be legal).
