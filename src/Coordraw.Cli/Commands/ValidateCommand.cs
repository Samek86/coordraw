using System;
using System.IO;
using System.Linq;
using Coordraw.Core.Parser;
using Coordraw.Core.Compiler;

namespace Coordraw.Cli.Commands
{
    public static class ValidateCommand
    {
        public static int Execute(string[] args)
        {
            if (args.Length < 1)
            {
                Console.Error.WriteLine("Usage: coordraw validate <file.crd>");
                return 1;
            }

            var inputFile = args[0];

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

                bool hasErrors = false;

                if (parseResult.Errors.Any())
                {
                    hasErrors = true;
                    Console.Error.WriteLine("Parse errors:");
                    foreach (var error in parseResult.Errors)
                    {
                        Console.Error.WriteLine($"  Line {error.Line}: {error.Message}");
                    }
                }

                var compiler = new EraserCompiler();
                var validationErrors = compiler.Validate(parseResult.Diagram);

                if (validationErrors.Any())
                {
                    hasErrors = true;
                    Console.Error.WriteLine("Validation errors:");
                    foreach (var error in validationErrors)
                    {
                        Console.Error.WriteLine($"  {error}");
                    }
                }

                if (!hasErrors)
                {
                    Console.WriteLine($"✓ {inputFile} is valid");
                    Console.WriteLine($"  Diagram: {parseResult.Diagram.Title}");
                    Console.WriteLine($"  Elements: {parseResult.Diagram.Children.Count}");
                    return 0;
                }

                return 1;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Validation failed: {ex.Message}");
                return 1;
            }
        }
    }
}
