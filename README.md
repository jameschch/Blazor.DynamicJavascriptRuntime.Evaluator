# Blazor.DynamicJavascriptRuntime.Evaluator

Wouldn't it be really useful if you could run a line or two of Javascript in your Blazor C# app?
Wouldn't it be handy if you could execute arbitrary Javascript at runtime without strings of script?
Wouldn't it be a big plus if the Javascript code was declared as a dynamic object expression and could be exposed to unit tests?

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

In order to satisfy the C# parser, by convention an underscore ("_") stands in for a space character in Javascript:

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

Don't forget to put the script include in your index.html:

```html
<script src="_content/BlazorDynamicJavascriptRuntime.js"></script>
```

The execution of Javascript is performed with the eval() function. You have been warned (and should probably know better).