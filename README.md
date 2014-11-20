Overview
==========================================================================

The edge.js documentation states: Note: Most Node.js modules are written in JavaScript and will execute in Edge as-is. However, some Node.js external modules are native binary modules, rebuilt by NPM on module installation to suit your local execution environment. Native binary modules will not run in Edge unless they are rebuilt to link against the NodeJS dll that Edge uses.

This package rebuilds native node depedencies at runtime so they be used with edge.js. The node modules must be in the edge directory.

Example
==========================================================================
This example write the HTML from the google home page to the console.

```csharp
using System;
using System.Threading.Tasks;
using Edge.NativeModuleSupport;

public static class Program
{
    public static void Main()
    {
        const string javascript = @"
            return function(data, callback) {
                var zombie = require("zombie");
                var browser = zombie.create();
                browser.visit(""http://www.google.com"", function() {
                    callback(null, browser.html());
                });
            };";
        Func<object, Task<object>> func = EdgeWithNativeModules.Func(javascript, "zombie");
        func.Wait();
        Console.WriteLine(func.Result);
    }
}
```
