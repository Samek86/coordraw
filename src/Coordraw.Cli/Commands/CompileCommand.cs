using System;
using System.IO;
using System.Linq;
using Coordraw.Core.Parser;
using Coordraw.Core.Compiler;
using Newtonsoft.Json;

namespace Coordraw.Cli.Commands
{
    public static class CompileCommand
    {
        public static int Execute(string[] args)
        {
            if (args.Length < 3)
            {
                Console.Error.WriteLine("Usage: coordraw compile <file.crd> -o <output.json>");
                return 1;
            }

            var inputFile = args[0];
            string outputFile = null;

            for (int i = 1; i < args.Length; i++)
            {
                if (args[i] == "-o" && i + 1 < args.Length)
                {
                    outputFile = args[i + 1];
                    break;
                }
            }

            if (string.IsNullOrEmpty(outputFile))
            {
                Console.Error.WriteLine("Output file not specified. Use -o <output.json>");
                return 1;
            }

            if (!File.Exists(inputFile))
            {
                Console.Error.WriteLine($"Input file not found: {inputFile}");
                return 1;
            }

            try
            {
                var source = File.ReadAllText(inputFile);
                var parser = new DslParser();
                var parseResult = parser.Parse(source);

                if (parseResult.Errors.Any())
                {
                    Console.Error.WriteLine("Parse errors:");
                    foreach (var error in parseResult.Errors)
                    {
                        Console.Error.WriteLine($"  Line {error.Line}: {error.Message}");
                    }
                    return 1;
                }

                var compiler = new EraserCompiler();
                var validationErrors = compiler.Validate(parseResult.Diagram);

                if (validationErrors.Any())
                {
                    Console.Error.WriteLine("Validation errors:");
                    foreach (var error in validationErrors)
                    {
                        Console.Error.WriteLine($"  {error}");
                    }
                    return 1;
                }

                var diagram = compiler.Compile(parseResult.Diagram);
                var json = JsonConvert.SerializeObject(diagram, Formatting.Indented);

                Directory.CreateDirectory(Path.GetDirectoryName(outputFile) ?? ".");
                File.WriteAllText(outputFile, json);

                Console.WriteLine($"✓ Compiled successfully: {outputFile}");
                Console.WriteLine($"  Entities: {diagram.Entities.Count}");
                Console.WriteLine($"  Connections: {diagram.Connections.Count}");

                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Compilation failed: {ex.Message}");
                return 1;
            }
        }
    }
}
