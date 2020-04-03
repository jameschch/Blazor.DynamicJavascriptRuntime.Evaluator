# Blazor.DynamicJavascriptRuntime.Evaluator

Wouldn't it be really useful if you could run a line or two of Javascript in your Blazor C# app?
Wouldn't it be handy if you could execute arbitrary Javascript at runtime without strings of script?
Wouldn't it be a big plus if the Javascript code was declared as a dynamic object expression and could be exposed to unit tests?
Wouldn't it be nice if you could consume Javascript library API's without creating interop wrappers?

Calling Javascript dynamically from C# couldn't be easier:

```csharp
using (dynamic context = new EvalContext(JSRuntimeInstance))
{
	(context as EvalContext).Expression = () => context.window.location = "www.be-eval.com";
	//window.location = "www.be-eval.com";
}
```

When it comes to executing code with arguments, the C# parser will support this:

```csharp
using (dynamic context = new EvalContext(JSRuntimeInstance))
{
	dynamic arg = new EvalContext(JSRuntimeInstance);
	dynamic arg2 = new EvalContext(JSRuntimeInstance);
	(context as EvalContext).Expression = () => context.Chart.config.data.datasets[arg.i].data.push(arg2.config.data.datasets[arg.i].data);
	//Chart.config.data.datasets[i].data.push(config.data.datasets[i].data);
}
```

Need to call some script that returns a value? No problem:

```csharp
dynamic context = new EvalContext(runtime.Object);
(context as EvalContext).Expression = () => context.document.cookie;           
var cookie = await (context as EvalContext).InvokeAsync<string>();
//document.cookie
```

In order to satisfy the C# parser, by default an underscore ("_") stands in for a space character in Javascript (but this is configurable):

```csharp
using (dynamic context = new EvalContext(JSRuntimeInstance))
{
	dynamic arg = new EvalContext(JSRuntimeInstance);
	(context as EvalContext).Expression = () => context.var_instance = arg.new_object();
	//var instance = new object();
}
```

The dynamic expression is eagerly evaluated. This means decimal arithmetic will not be mangled by Javascript:

```csharp
using (dynamic context = new EvalContext(JSRuntimeInstance))
{
	(context as EvalContext).Expression = () => context.sum = 0.1M + 0.2M * 0.5M / 0.5M;
	//sum = 0.3;
}
```

Maybe you feel like a bit of JQuery?

```csharp
using (dynamic context = new EvalContext(JsRuntime))
{
    (context as EvalContext).Expression = () => context.jQuery("body").css("overflow-y", "hidden");
	//jQuery("body").css("overflow-y", "hidden")
}
```

How about passing complex types as arguments? We've got you covered for anonymous types:

```csharp
using (dynamic context = new EvalContext(JsRuntime))
{
    var arg = new { Property = "Value", Field = 123, Child = new { Member = new DateTime(2001, 1, 1) } };
    (context as EvalContext).Expression = () => context.myScript.set(arg);
	//myScript.set({"property":"Value","field":123,"child":{"member":"2001-01-01T00:00:00"}})
}
```

Passing user-defined types takes more effort, but not too much:

```csharp
var settings = new EvalContextSettings();
settings.SerializableTypes.Add(typeof(Specified));
using (dynamic context = new EvalContext(JsRuntime, settings))
{
    var arg = new Specified { Member = "abc" };
    (context as EvalContext).Expression = () => context.myScript.setSpecified(arg);
	//myScript.setSpecified({"member":"abc"})
}
```

The execution of Javascript is performed with the eval() function, so it's imperative to sanitize user input that's passed into the Javascript runtime. You have been warned.

## Setup

First, install from nuget:

```
Install-Package DynamicJavascriptRuntime.Blazor.Evaluator -Version 1.1.0
```

[https://www.nuget.org/packages/DynamicJavascriptRuntime.Blazor.Evaluator/](https://www.nuget.org/packages/DynamicJavascriptRuntime.Blazor.Evaluator/)

You then need to create a script include in your index.htm:

```html
    <script src="_content/DynamicJavascriptRuntime.Blazor.Evaluator/BlazorDynamicJavascriptRuntime.js"></script>
```