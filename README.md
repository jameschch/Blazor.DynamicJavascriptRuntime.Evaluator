# Blazor.DynamicJavascriptRuntime.Evaluator

Wouldn't it be really useful if you could run a line or two of Javascript in your Blazor C# app?
Wouldn't it be handy if you could execute arbitrary Javascript at runtime without strings of script?
Wouldn't it be a big plus if the Javascript code was declared as a dynamic object expression and could be exposed to unit tests?
Wouldn't it be nice if you could consume Javascript library API's without creating interop wrappers?

Calling dynamic Javascript from C# at runtime couldn't be easier:

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

Need to call a function that returns a value? No problem:

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

The dynamic expression is eagerly evaluated. This means floating point arithmetic will not be completely broken (it won't run in Javascript):

```csharp
using (dynamic context = new EvalContext(JSRuntimeInstance))
{
	(context as EvalContext).Expression = () => context.sum = 0.1 + 0.2 * 0.5 / 0.5;
	//sum = 0.3;
	//result in Javascript is 0.30000000000000004
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
    var arg = new { Property = "Value", Field = 123, child = new { Member = new DateTime(2001, 1, 1) } };
    (context as EvalContext).Expression = () => context.JsInterop.set(arg);
	//JsInterop.set({"Property":"Value","Field":123,"child":{"Member":"2001-01-01T00:00:00"}})
}
```

Passing user-defined types takes more effort, but not too much:

```csharp
var settings = new EvalContextSettings();
settings.SerializableTypes.Add(typeof(Specified));
using (dynamic context = new EvalContext(JsRuntime, settings))
{
    var arg = new Specified { Member = "abc" };
    (context as EvalContext).Expression = () => context.JsInterop.setSpecified(arg);
	//JsInterop.setSpecified({"Member":"abc"})
}
```

The execution of Javascript is performed with the eval() function, so it's imperative to sanitize user input that's passed into the Javascript runtime. You have been warned.