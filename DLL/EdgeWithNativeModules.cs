using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using EdgeJs;
using Microsoft.Build.Construction;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;

namespace EdgeJs.NativeModuleSupport
{
    public static class EdgeWithNativeModules
    {
        private static readonly ISet<string> loadedTopLevelModules = new HashSet<string>();
        private static readonly object initializeLock = new object();
        private static string assemblyDirectory;

        /// <summary>
        /// Gets or sets the path to the node executable. Defaults to node executable provided by NCapsulate.Node.
        /// </summary>
        public static string NodeExe { get; set; }

        /// <summary>
        /// Gets or sets the logger used by MSBuild when re-compiling the modules.
        /// </summary>
        public static ILogger Logger { get; set; }

        private static string AssemblyDirectory
        {
            get
            {
                if (assemblyDirectory == null)
                {
                    assemblyDirectory = Environment.GetEnvironmentVariable("EDGE_BASE_DIR");
                    if (string.IsNullOrEmpty(assemblyDirectory))
                    {
                        string codeBase = typeof(Edge).Assembly.CodeBase;
                        UriBuilder uriBuilder = new UriBuilder(codeBase);
                        string path = Uri.UnescapeDataString(uriBuilder.Path);
                        assemblyDirectory = Path.GetDirectoryName(path);
                    }
                }

                return assemblyDirectory;
            }
        }

        /// <summary>
        /// Creates a call to the specified javascript code.
        /// </summary>
        /// <param name="code">The javascript code that will be able to be executed.</param>
        /// <param name="topLevelModules">The top level modules that will have their native dependencies re-compiled.</param>
        /// <returns>A call to the specified javascript code.</returns>
        /// <remarks>
        /// Each module is only re-built once.
        /// </remarks>
        public static Func<object, Task<object>> Func(string code, params string[] topLevelModules)
        {
            lock (initializeLock)
            {
                string msbuildFile = Path.Combine(AssemblyDirectory, @"edge\native-node-module.msbuild");
                foreach (string topLevelModule in topLevelModules.Where(module => loadedTopLevelModules.Add(module)))
                {
                    IDictionary<string, string> properties = new Dictionary<string, string>()
					{
						{ "TopLevelModule", topLevelModule },
						{ "Platform", IntPtr.Size == 4 ? "ia32" : "x64" }
					};
                    if (!string.IsNullOrWhiteSpace(NodeExe))
                    {
                        properties.Add("NodeExe", NodeExe);
                    }

                    BuildRequestData request = new BuildRequestData(msbuildFile, properties, null, new string[0], null);
                    BuildParameters buildParameters = new BuildParameters()
                    {
                        Loggers = new ILogger[] { Logger ?? new TraceLogger() }
                    };
                    BuildManager.DefaultBuildManager.Build(buildParameters, request);
                }
            }

            return Edge.Func(code);
        }

        /// <summary>
        /// Registers any modules that have already been compiled.
        /// </summary>
        /// <param name="topLevelModules">The modules that have already been compiled.</param>
        public static void RegisterPreCompiledModules(params string[] topLevelModules)
        {
            lock (initializeLock)
            {
                foreach (string topLevelModule in topLevelModules)
                {
                    loadedTopLevelModules.Add(topLevelModule);
                }
            }
        }
    }
}
