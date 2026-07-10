using System;
using System.IO;
using System.Linq;

namespace SpiceSharpMechanical2D.Samples.FeatureGallery
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length == 0 || args[0].Equals("list", StringComparison.OrdinalIgnoreCase))
            {
                PrintCatalog(Console.Out);
                return 0;
            }

            if (args[0].Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                foreach (DemoDefinition demo in DemoCatalog.All)
                {
                    Console.WriteLine(FormattableString.Invariant($"# {demo.Name}: {demo.Description}"));
                    demo.Run(Console.Out);
                    Console.WriteLine();
                }

                return 0;
            }

            if (args[0].Equals("smoke", StringComparison.OrdinalIgnoreCase))
            {
                foreach (DemoDefinition demo in DemoCatalog.All)
                {
                    demo.Run(TextWriter.Null);
                    Console.WriteLine(FormattableString.Invariant($"PASS {demo.Name}"));
                }

                return 0;
            }

            DemoDefinition selected = DemoCatalog.All.FirstOrDefault(demo =>
                demo.Name.Equals(args[0], StringComparison.OrdinalIgnoreCase));
            if (selected == null)
            {
                Console.Error.WriteLine(FormattableString.Invariant(
                    $"Unknown demo '{args[0]}'. Run with 'list' to see available names."));
                return 1;
            }

            selected.Run(Console.Out);
            return 0;
        }

        private static void PrintCatalog(TextWriter output)
        {
            output.WriteLine("SpiceSharpMechanical2D feature gallery");
            output.WriteLine("Run: dotnet run --project <project> -- <demo-name>");
            output.WriteLine();
            foreach (DemoDefinition demo in DemoCatalog.All)
            {
                output.WriteLine(FormattableString.Invariant(
                    $"  {demo.Name,-24} {demo.Description}"));
            }

            output.WriteLine();
            output.WriteLine("Special commands: list, all, smoke");
        }
    }
}
