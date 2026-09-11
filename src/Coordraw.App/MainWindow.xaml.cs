using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using Coordraw.Core.Models;
using Coordraw.Core.Parser;
using Coordraw.Core.Compiler;

namespace Coordraw.App
{
    public partial class MainWindow : Window
    {
        private readonly DslParser _parser = new DslParser();
        private readonly EraserCompiler _compiler = new EraserCompiler();
        private string _currentDslPath;
        private string _currentJsonPath;

        public MainWindow()
        {
            InitializeComponent();
            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            await WebViewCanvas.EnsureCoreWebView2Async(null);
            
            // Load canvas HTML from embedded resource or file
            var htmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "canvas.html");
            if (File.Exists(htmlPath))
            {
                WebViewCanvas.CoreWebView2.Navigate(htmlPath);
            }
            else
            {
                // Fallback: load from resource
                var html = GetEmbeddedCanvasHtml();
                WebViewCanvas.NavigateToString(html);
            }

            // Setup message bridge
            WebViewCanvas.CoreWebView2.WebMessageReceived += WebView_MessageReceived;
        }

        private void WebView_MessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                var message = JsonConvert.DeserializeObject<dynamic>(e.WebMessageAsJson);
                var action = (string)message.action;

                switch (action)
                {
                    case "save":
                        // Handle diagram save from canvas
                        var diagramJson = JsonConvert.SerializeObject(message.data);
                        UpdateDslFromJson(diagramJson);
                        break;
                    case "ready":
                        StatusText.Text = "Canvas ready";
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error handling message: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenDsl_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Coordraw DSL (*.crd)|*.crd|All Files (*.*)|*.*",
                Title = "Open DSL File"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _currentDslPath = dialog.FileName;
                    var dslContent = File.ReadAllText(_currentDslPath);
                    DslTextBox.Text = dslContent;
                    ApplyDsl();
                    StatusText.Text = $"Loaded: {Path.GetFileName(_currentDslPath)}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error opening file: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OpenJson_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Eraser JSON (*.json)|*.json|All Files (*.*)|*.*",
                Title = "Open JSON File"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _currentJsonPath = dialog.FileName;
                    var jsonContent = File.ReadAllText(_currentJsonPath);
                    LoadJsonToCanvas(jsonContent);
                    StatusText.Text = $"Loaded: {Path.GetFileName(_currentJsonPath)}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error opening file: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveDsl_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Coordraw DSL (*.crd)|*.crd|All Files (*.*)|*.*",
                Title = "Save DSL File",
                FileName = _currentDslPath ?? "diagram.crd"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(dialog.FileName, DslTextBox.Text);
                    _currentDslPath = dialog.FileName;
                    StatusText.Text = $"Saved: {Path.GetFileName(_currentDslPath)}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving file: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveJson_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Eraser JSON (*.json)|*.json|All Files (*.*)|*.*",
                Title = "Save JSON File",
                FileName = _currentJsonPath ?? "diagram.json"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var parseResult = _parser.Parse(DslTextBox.Text);
                    var eraserDiagram = _compiler.Compile(parseResult.Diagram);
                    var json = JsonConvert.SerializeObject(eraserDiagram, Formatting.Indented);
                    File.WriteAllText(dialog.FileName, json);
                    _currentJsonPath = dialog.FileName;
                    StatusText.Text = $"Saved: {Path.GetFileName(_currentJsonPath)}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving file: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RenderPng_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "PNG Image (*.png)|*.png",
                Title = "Render to PNG",
                FileName = "diagram.png"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // Compile to JSON first
                    var parseResult = _parser.Parse(DslTextBox.Text);
                    var eraserDiagram = _compiler.Compile(parseResult.Diagram);
                    var tempJson = Path.GetTempFileName() + ".json";
                    var json = JsonConvert.SerializeObject(eraserDiagram, Formatting.Indented);
                    File.WriteAllText(tempJson, json);

                    // Call eraser-diagrams-cli
                    var nodePath = FindNodeModuleCli();
                    if (string.IsNullOrEmpty(nodePath))
                    {
                        MessageBox.Show("eraser-diagrams-cli not found. Run 'npm install' first.", 
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var fontsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, 
                        "..", "..", "..", "examples", "fonts.json");

                    var args = $"\"{tempJson}\" --output \"{dialog.FileName}\"";
                    if (File.Exists(fontsPath))
                        args += $" --fonts \"{Path.GetFullPath(fontsPath)}\"";

                    var process = Process.Start(new ProcessStartInfo
                    {
                        FileName = nodePath,
                        Arguments = args,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });

                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        StatusText.Text = $"Rendered: {Path.GetFileName(dialog.FileName)}";
                        MessageBox.Show($"Diagram rendered successfully!\n\n{dialog.FileName}", 
                            "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Rendering failed. Check that Chrome/Chromium is installed.", 
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    File.Delete(tempJson);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error rendering: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private string FindNodeModuleCli()
        {
            var paths = new[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", 
                    "node_modules", ".bin", "eraser-diagrams-cli.cmd"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                    "npm", "eraser-diagrams-cli.cmd")
            };

            foreach (var path in paths)
            {
                if (File.Exists(Path.GetFullPath(path)))
                    return Path.GetFullPath(path);
            }

            return null;
        }

        private void ApplyDsl_Click(object sender, RoutedEventArgs e)
        {
            ApplyDsl();
        }

        private void ApplyDsl()
        {
            try
            {
                var parseResult = _parser.Parse(DslTextBox.Text);
                
                if (parseResult.Errors.Count > 0)
                {
                    var errors = string.Join("\n", parseResult.Errors.ConvertAll(
                        e => $"Line {e.Line}: {e.Message}"));
                    MessageBox.Show($"Parse errors:\n\n{errors}", "Parse Errors", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                var eraserDiagram = _compiler.Compile(parseResult.Diagram);
                var json = JsonConvert.SerializeObject(eraserDiagram);
                LoadJsonToCanvas(json);
                
                StatusText.Text = "DSL applied successfully";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying DSL: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadJsonToCanvas(string json)
        {
            if (WebViewCanvas.CoreWebView2 != null)
            {
                var script = $"loadDiagram({json});";
                WebViewCanvas.CoreWebView2.ExecuteScriptAsync(script);
            }
        }

        private void UpdateDslFromJson(string json)
        {
            // TODO: Implement reverse compiler (JSON -> DSL) in future version
            // For v0, this is best-effort or deferred
        }

        private void ToggleSource_Click(object sender, RoutedEventArgs e)
        {
            var visible = ShowSourceMenuItem.IsChecked;
            SourcePanel.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            SourceSplitter.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            SourcePanelColumn.Width = visible ? new GridLength(400) : new GridLength(0);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Coordraw v0.1\n\n" +
                "Free, self-hosted diagram toolkit\n" +
                "DSL → Eraser Diagrams compiler\n\n" +
                "MIT License © 2026\n\n" +
                "https://github.com/Samek86/coordraw",
                "About Coordraw",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private string GetEmbeddedCanvasHtml()
        {
            // Minimal fallback HTML if files not found
            return @"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Canvas</title>
    <style>
        body { margin: 0; padding: 0; overflow: hidden; font-family: sans-serif; }
        #canvas { width: 100vw; height: 100vh; background: #f5f5f5; position: relative; }
        .node { position: absolute; border: 2px solid #3498db; background: white; 
                padding: 10px; border-radius: 4px; cursor: move; }
    </style>
</head>
<body>
    <div id='canvas'>
        <p style='padding: 20px;'>Canvas placeholder. Load DSL to see diagram.</p>
    </div>
    <script>
        function loadDiagram(data) {
            document.getElementById('canvas').innerHTML = '<p>Diagram loaded: ' + 
                data.entities.length + ' entities</p>';
            window.chrome.webview.postMessage({ action: 'ready' });
        }
        window.chrome.webview.postMessage({ action: 'ready' });
    </script>
</body>
</html>";
        }
    }
}
