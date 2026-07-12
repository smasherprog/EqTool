using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Reflection;

namespace EQtoolsTests
{
    // MSTest v4 no longer runs .NET Framework tests in a per-assembly AppDomain, so the
    // binding redirects in app.config are never applied. Assemblies compiled against an
    // older version of a dependency (e.g. Autofac -> Microsoft.Bcl.AsyncInterfaces 8.0.0.0)
    // then fail to load the newer copy in the output folder. This resolver applies the same
    // policy the redirects would: serve whatever version sits next to the test assembly.
    [TestClass]
    public class AssemblyBindingFixture
    {
        [AssemblyInitialize]
        public static void Initialize(TestContext _)
        {
            var binFolder = Path.GetDirectoryName(typeof(AssemblyBindingFixture).Assembly.Location);
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                var name = new AssemblyName(args.Name);
                if (name.Name.EndsWith(".resources", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }
                var candidate = Path.Combine(binFolder, name.Name + ".dll");
                return File.Exists(candidate) ? Assembly.LoadFrom(candidate) : null;
            };
        }
    }
}
