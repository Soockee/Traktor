![Nuget](https://img.shields.io/nuget/v/Traktor)

# C# Client for Traktor
* Implements C# Opentracing API
* Supports netstandard2.0

## Status
This library is under construction

## Usage

### Tracer
The following will give you a tracer that reports Spans to the [TraktorAgent via UDP](https://geocode.igd.fraunhofer.de/sstockha/traktoragent)

```
using Traktor;

var agentAddress = "localhost"
var agentPort = 13337;
var reporterPort = 13338;
Tracer tracer = new Tracer()
tracer.Configure(agentAddress,agentPort,reporterPort);
```

The Tracer holds an active span. Spans can be activated by the Scopemanager. The activated spans will be wrapped up in a scope. 
Scopes are managed by the Scopemanager. 

The Tracer can be configured by the Configure() Method.
the usual case would look like:

```
using Traktor;

var registryAddress = "localhost";
var registryPort = "8080";
var agentAddress = "localhost"
var agentPort = 13337;
var reporterPort = 13338;

Tracer tracer = new Tracer()
tracer.Configure(registryAddress,registryPort,agentAddress,agentPort,reporterPort);
```

The [TraktorRegistry](https://geocode.igd.fraunhofer.de/sstockha/traktorregistry) is used for contextpropagation between processes. Configure() will connect the tracer-instance to the running registry application at the given address:port.
Gracefull shutdown of the websocketclients is not implemented. The application will most likely raise runtime errors, if the registry shuts down after the connection was established.

### Spans, Spanbuilder, Scopes and Scopemanager
Building an unactivated Span:

```
var operationname = "example_function";
var spanBuilder = tracer.BuildSpan(operationname);
```
Building a span will result in a SpanBuilder instance. Building a span is usually done for each operation you want to trace. The Scopemanager is not aware of spanbuilder. 
And no Span is actually instantiated yet. To instantiated the span we need to use the Start() function of the SpanBuilder.

```
var operationname = "example_function";
var spanBuilder = tracer.BuildSpan(operationname);
var span = spanBuilder.Start();
```
The span now contains all neseccary informations for further processing.
* TraceId
* SpanId
* SpanContext
* Starttime

To finish a span, which will result in reporting it to the TraktorAgent, we can call the Finish() method. The Endtime will be set and Reporter.report(Span span) will be called.
This concludes the basic lifecycle of a span. 
```
var operationname = "example_function";
var span = tracer.BuildSpan(operationname).Start();
span.Finish();
```


To manage more then one span we need to use the ScopeManager. The scopemanager is not aware of the span in the previous example. To Manage the span, we need to invoke the tracer.Scopemanager.Activate method. 

```
var operationname = "example_function";
var span = tracer.BuildSpan(operationname).Start();
var scope = tracer.Scopemanager.Activate(span);
```

The scope with its wrapped span are now managed by the scopemanager. We can start and activate new Spans which will inherit the TraceId of the currently active span.
The Scopemanager will managed scopes even if they are created inside a Thread of the Application. 
Warning:  Multithreaded scenarios could still make trouble on the reporting end. this needs further testing
Noteworthy: If no Span is Active, the traceId will be generated, otherwise in some way inherited
```

var firstSpan = tracer.BuildSpan("section").Start();
var firstScope = tracer.Scopemanager.Activate(firstSpan);
// example SpanContext of first Span: 
// traceId: 123 spanId: abc 

var secondSpan = tracer.BuildSpan("another_section").Start();
var secondScope = tracer.Scopemanager.Activate(secondSpan);
// example SpanContext of first Span: 
// traceId: 123 spanId: zyx
```

We definitly can do a smoother usage. Scope implements the IDisposable interface. Scope.Dispose() calls Span's Fininsh() method which allows us to use `using` in combination with the StartActive() method of SpanBuilder. StartActive() instantiates a span and activates it, which results in a scope. As soon as the using block completes, the span will be reported.

```
using(var scope = tracer.BuildSpan("operationName").StartActive())
{
    // do some work
}
```

#### Registry and Contextpropagation
Without Contextpropagation spans can only be managed inside of a System. The TraktorRegistry in combination with ReceiveContext() and SendContext() allow us to propagate Context across systemboundaries. 

**Server**
```
var registryAddress = "127.0.0.1";
var registryPort = "8080";
Tracer tracer = new Tracer();
tracer.Configure(registryAddress,registryPort);

ISpanContext parentctx = await tracer.ReceiveContext();
var scope = tracer.BuildSpan("some-operationname").AsChildOf(parentctx).StartActive();
//do some work, and responde to client
scope.Dispose();

```

**Client**
```
var registryAddress = "127.0.0.1";
var registryPort = "8080";
Tracer tracer = new Tracer();
tracer.Configure(registryAddress,registryPort);

var scope = tracer.BuildSpan("some-operationname").StartActive());
await tracer.SendContext(scope.Span)
// do work and make sure to get response
scope.Dispose();
```

The Context flows through the Registry, which will send it to the every other connected tracer.
Warning: Currently RegistrySockets will receive all Messages from Registry, no routing is implemented. 2+ Tracer will result in difficulties and runtime errors

#### Tags

Tags are currently not supported

### SpansContext

The spancontext does have following fields:
```
private string traceId;
private string spanId;
private string referencetyp;
```

traceId and SpanId are 12 characer long ID. IDs are not checked for uniques. We currently just hope it won't happen.
Used algorithm for further insight:
Solution1 of this post https://stackoverflow.com/a/1344365/7383590 ( checked 24.01.2020)

the referencetyp can be any of OpenTracings References. 
* child_of
* follows_from

Traktor currently builds spans with child_of per default. Further work need to be done to make sure follows_from does work correctly. 
The possibility to add the a ReferenceTyp is given via AddRefernce() method of SpanBuilder.
Quality-of-Life Methods like AsChildOf() can be used aswell.

### Reporter

The Reporterclient is implemented as udpclient. The span, which should be reported, is encoded in UTF8 and simply send over the wire to the TraktorAgent. 
Further investigation of correct behaviour is necessary, and will most likely result in some unknown bugs. :D