# XtraLiteTemplates [![Build status](https://ci.appveyor.com/api/projects/status/gapq9gvrneooy1ob/branch/master?svg=true)](https://ci.appveyor.com/project/pavkam/xtralitetemplates/branch/master)
A lightweight templating engine for .NET Framework 4.5+, .NET Core 2.0+ and Mono.

### What it is, and what it is not
XtraLiteTemplates is a fully managed .NET Framework library that offers easy string templating features. XtraLiteTemplates offers support for `custom template constructs` (akin to statements), and support for `expressions` with custom operators. XtraLiteTemplates is not a programming language or a domain-specific language in its own right, its sole purpose is to aid in the creation and manipulation of string templates. Anything else is out of scope of this project.

> **XtraLiteTemplates does not use the C# compiler services.** No intermediate .NET source-code is generated in the process, and no assemblies are compiled as a result.

### Usage Examples
The easiest way to evaluate a template is by using the built-in facade class ```XLTemplate```. It works in conjunction with an instance of ```IDialect``` interface to parse and evaluate a template:

```c#
  var @object = new
  {
      FirstName = "John",
      LastName = "McMann",
      Age = 31,
      Loves = new String [] { "Apples", "Bikes", "Everything Nice" }
  };

  var result = XLTemplate.Evaluate(CodeMonkeyDialect.DefaultIgnoreCase, 
    @"{pre}Hello, {customer.FirstName} {customer.LastName}. You are {customer.Age} years old and you love: {for entity in customer.loves}{entity}, {end}{end}", customer => object);
  
  Console.WriteLine(result);
```
Will display `"Hello, John McMann. You are 31 years old and you love: Apples,Bikes,Everything Nice,"`

```
{preformatted}
Hello {Customer.FirstName},

You have made a purchase on {Order.SourceWebsite.Host} on {Order.DateTime:'g'} in amount of {Order.Currency} {Order.Total:'N'}.

Your items include:
{for each purchased_item in Order.Items}  * {purchased_item.Name} with a value of {Order.Currency} {purchased_item.Amount:'N'}{end}

{if Order.DeliveryType == Postal}Your items will be delivered by post on {Order.DeliveryDate:'D'}{end}

{'* This order is a gift' IF Order.IsGift}

With respect,
Somebody responsible.
{end}
```
The above code block exemplifies a possible email template that uses the `Standard` dialect. One is in no way forced to use it; the library allows creation of custom dialects with custom constructs, custom special characters, etc. Nothing is "reserved" by the engine. There are a few small restrictions resulting from the tokenization process but those will be covered in a later area.

And a more exotic example:

```c#
  var template = "{FOR Member IN GetType().GetMembers()}{Member.Name}{WITH}{PRE}, {END}{END}";
  var result = XLTemplate.Evaluate(CodeMonkeyDialect.DefaultIgnoreCase, template);
  
  /* Results in a string (introspection of the IDialect.Self object:
   * "get_TypeConverter, String, Number, Boolean, ToString, Equals, GetHashCode, GetType, .ctor, TypeConverter" 
   */
```

### Core Concepts & Building Blocks
Put in the simplest of terms, the process of evaluation of a template follows the following pattern:
* Tokenization, in which a string is split into tokens. The `tokenizer` requires a set of control characters that drive the process (such as the *directive open & close* characters or *string literal escape* characters).
* Lexical analysis. The `lexer` tries to make sense of the tokens supplied by the tokenization process using a set of given *tags* and *expression operators*.
* Interpretation. The output of lexical analysis is a stream of *lex* objects. The `interpreter` will try to match these objects with known *directives* and build a very simplistic `AST`. The interpretation process will return an `IEvaluable` interface to the caller. This can be considered as being a *compiled template* -- ready to be evaluated.
* Evaluation can pe performed any number of times on the compiled template. The caller must provide an `IEvaluationContext` object that offers the state, variable access, and other functionality.

#### Expressions
The expression builder in XtraLiteTemplates allows for:
* Any number of operators to be defined.
* Either symbols or words can be used to define an operator, but not both at the same time (e.g. `type-of` is not allowed, while `type_of` is).
* Unary and binary operators are supported.
* Short-circuiting is supported (if implemented by the operators).
* Groups are supported, including the comma character. Groups are evaluated to enumerables.
* Operators operate on `object`.
* The standard set of operators provided in XtraLiteTemplates tries to emulate the behaviour of JavaScript as much as possible.
* Support for **invoking object methods, properties and fields**. Runtime selection of overloaded methods is supported to the best possible extent.
* Can invoke methods and properties on **dynamic objects** using DLR. Conversions and operators are not supported due to conflicting needs.

#### Tags
A tag is a collection of keywords, identifier rules and expressions. For example `IF $ THEN` defines a tag that accepts `IF` as a first keyword, followed by an expression and then by `THEN` keyword. A tag of the following form: `FOREACH ? IN $ DO` will match any phrase that starts with the `FOREACH` keyword, followed by any identifier, then by `IN` keyword, an expression and lastly by the `DO` keyword.

Tags are the core building block of directives.

#### Directives
A directive is a collection of one or more tags. A directive can be seen as the priamary language construct that is evaluated at run-time. An example common directive, is the `IF` statement - composed of the starting tag `IF $ THEN` and an ending tag `END`. Such a directive will be configured to evaluate all content found between its tags if the given expression evaluates to `TRUE`.

Example:
```
Hello {IF Customer.Title THEN}{Customer.Title}, {END}{Customer.FirstName} {Customer.LastName}
```

This template will check whether the `Title` property of a provided `Customer` object is not empty, in which case the said property will be evaluated, followed by the `FirstName` and `LastName` properties of the same object.

The interpreter is smart enough to distinguish between similar directives that have one or more differences in their composing tags. As such the following two directives: `{IF $ THEN}...{END}` and `{IF $ THEN}...{ELSE}...{END}` can coexist and be properly selected by the interpreter.

#### Dialects
A dialect is a special object that supplies all the required properties and behaviours that define a language. These  include:
* All supported directives,
* Expression operators and the flow control symbols (such as group open and close and member access),
* String comparison and culture settings, and finally,
* The behaviour of unparsed text blocks.


## For more details and explanations please visit the [wiki](https://github.com/pavkam/XtraLiteTemplates/wiki/Home)
