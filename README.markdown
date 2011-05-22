# EyeDisposable

## What?

If you have used `IDisposable`, you will know the problem. It's very easy to "leak" them by forgetting to call `Dispose()` on them. Yes, most objects' finalizers will eventually call `Dispose()`, but it's a [bad practice to rely on finalizer being run at all](http://blogs.msdn.com/b/oldnewthing/archive/2010/08/09/10047586.aspx). It's cleaner and neater to `Dispose()` of them at an appropriate time.

## Usage

### 1. Instrument your entry assembly and other dependent assemblies.

<pre>
&gt; EyeDisposable.exe C:\Projects\Foo\Foo.exe

Instrumenting type `&lt;Module&gt;`...
Instrumenting type `Foo`...
Instrumenting type `Program`...
- System.Void Foo.Program::Main(System.String[]): 1 newobjs; 0 disposes
Instrumenting entry point...

&gt; EyeDisposable.exe C:\Projects\Foo\Bar.dll
...
</pre>

### 2. Run

<pre>
&gt; cd C:\Projects\Foo
&gt; Foo.exe
</pre>

### 3. Profit

<pre>
&gt; type Foo.exe.DisposeLeaks.log
====
Disposer check
1 leaks detected!
====
Disposable object leaked!
Hash code: 30015890
Type: Foo.Foo
Created at:
&gt; [Foo.Program] Void Main(System.String[])
&gt; [Foo.Program] Void EyeDisposable_NewMain(System.String[])
</pre>

## How does it work?

EyeDisposable uses [Mono.Cecil](http://www.mono-project.com/Cecil) to instrument assemblies. `newobj` and `Dispose()` calls are tallied and checked at the end of the program.
