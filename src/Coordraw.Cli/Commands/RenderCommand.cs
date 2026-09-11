using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Coordraw.Core.Parser;
using Coordraw.Core.Compiler;
using Newtonsoft.Json;

namespace Coordraw.Cli.Commands
{
    public static class RenderCommand
    {
        public static int Execute(string[] args)
        {
            if (args.Length < 3)
            {
                Console.Error.WriteLine("Usage: coordraw render <file.crd> -o <output.png> [--fonts <fonts.json>]");
                return 1;
            }

            var inputFile = args[0];
            string outputFile = null;
            string fontsFile = null;

            for (int i = 1; i < args.Length; i++)
            {
                if (args[i] == "-o" && i + 1 < args.Length)
                {
                    outputFile = args[i + 1];
                    i++;
                }
                else if (args[i] == "--fonts" && i + 1 < args.Length)
                {
                    fontsFile = args[i + 1];
                    i++;
                }
            }

            if (string.IsNullOrEmpty(outputFile))
            {
                Console.Error.WriteLine("Output file not specified. Use -o <output.png>");
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
                var diagram = compiler.Compile(parseResult.Diagram);
                
                var tempJsonFile = Path.GetTempFileName() + ".json";
                var json = JsonConvert.SerializeObject(diagram, Formatting.Indented);
                File.WriteAllText(tempJsonFile, json);

                Console.WriteLine($"Compiled to: {tempJsonFile}");
                Console.WriteLine($"Rendering with eraser-diagrams-cli...");

                var cliArgs = $"{tempJsonFile} -o {outputFile}";
                if (!string.IsNullOrEmpty(fontsFile) && File.Exists(fontsFile))
                {
                    cliArgs += $" --fonts {fontsFile}";
                }

                var processInfo = new ProcessStartInfo
                {
                    FileName = "npx",
                    Arguments = $"@eraserlabs/diagrams-cli {cliArgs}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(processInfo))
                {
                    var output = process.StandardOutput.ReadToEnd();
                    var error = process.StandardError.ReadToEnd();
                    
                    process.WaitForExit();

                    if (!string.IsNullOrEmpty(output))
                        Console.WriteLine(output);

                    if (process.ExitCode != 0)
                    {
                        Console.Error.WriteLine("Rendering failed:");
                        if (!string.IsNullOrEmpty(error))
                            Console.Error.WriteLine(error);
                        
                        Console.Error.WriteLine();
                        Console.Error.WriteLine("Make sure Node.js is installed and @eraserlabs/diagrams-cli is available:");
                        Console.Error.WriteLine("  npm install -g @eraserlabs/diagrams-cli");
                        Console.Error.WriteLine("or in the project directory:");
                        Console.Error.WriteLine("  npm install");
                        
                        File.Delete(tempJsonFile);
                        return 1;
                    }
                }

                File.Delete(tempJsonFile);

                if (File.Exists(outputFile))
                {
                    Console.WriteLine($"✓ Rendered successfully: {outputFile}");
                    return 0;
                }
                else
                {
                    Console.Error.WriteLine("Rendering appeared to succeed but output file not found.");
                    return 1;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Render failed: {ex.Message}");
                return 1;
            }
        }
    }
}
