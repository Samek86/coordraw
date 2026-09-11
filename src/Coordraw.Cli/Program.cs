using System;
using System.Linq;
using Coordraw.Cli.Commands;

namespace Coordraw.Cli
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    PrintUsage();
                    return 0;
                }

                var command = args[0].ToLower();

                switch (command)
                {
                    case "compile":
                        return CompileCommand.Execute(args.Skip(1).ToArray());
                    
                    case "render":
                        return RenderCommand.Execute(args.Skip(1).ToArray());
                    
                    case "validate":
                        return ValidateCommand.Execute(args.Skip(1).ToArray());
                    
                    case "help":
                    case "--help":
                    case "-h":
                        PrintUsage();
                        return 0;
                    
                    default:
                        Console.Error.WriteLine($"Unknown command: {command}");
                        PrintUsage();
                        return 1;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                return 1;
            }
        }

        static void PrintUsage()
        {
            Console.WriteLine(@"
Coordraw CLI - Diagram DSL Compiler
===================================

Usage:
  coordraw <command> [options]

Commands:
  compile <file.crd> -o <output.json>
      Compile DSL file to Eraser JSON format

  validate <file.crd>
      Validate DSL file syntax and structure

  render <file.crd> -o <output.png> [--fonts <fonts.json>]
      Compile DSL and render to PNG using eraser-diagrams-cli
      Requires Node.js and @eraserlabs/diagrams-cli installed

  help
      Show this help message

Examples:
  coordraw compile examples/demo-japanese.crd -o output.json
  coordraw validate examples/simple.crd
  coordraw render examples/demo-japanese.crd -o output.png --fonts examples/fonts.json
");
        }
    }
}
