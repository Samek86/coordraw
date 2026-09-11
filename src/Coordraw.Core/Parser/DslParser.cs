using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Coordraw.Core.Models;

namespace Coordraw.Core.Parser
{
    public class DslParser
    {
        private string[] _lines;
        private int _currentLine;
        private List<ParseError> _errors;
        private Stack<GroupNode> _groupStack;
        private Dictionary<string, DiagramElement> _nodeMap;

        public ParseResult Parse(string source)
        {
            _lines = source.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                          .Select(l => l.Trim())
                          .Where(l => !string.IsNullOrWhiteSpace(l) && !l.StartsWith("#"))
                          .ToArray();
            
            _currentLine = 0;
            _errors = new List<ParseError>();
            _groupStack = new Stack<GroupNode>();
            _nodeMap = new Dictionary<string, DiagramElement>();

            DiagramNode diagram = null;

            while (_currentLine < _lines.Length)
            {
                var line = _lines[_currentLine];
                var lineNum = _currentLine + 1;

                try
                {
                    if (line.StartsWith("diagram "))
                    {
                        diagram = ParseDiagram(line, lineNum);
                    }
                    else if (line.StartsWith("box "))
                    {
                        var box = ParseBox(line, lineNum);
                        if (box != null)
                        {
                            _nodeMap[box.Id] = box;
                            if (_groupStack.Count > 0)
                                _groupStack.Peek().Children.Add(box);
                            else if (diagram != null)
                                diagram.Children.Add(box);
                        }
                    }
                    else if (line.StartsWith("group "))
                    {
                        var group = ParseGroup(line, lineNum);
                        if (group != null)
                        {
                            _nodeMap[group.Id] = group;
                            if (diagram != null)
                                diagram.Children.Add(group);
                            _groupStack.Push(group);
                        }
                    }
                    else if (line.StartsWith("icon "))
                    {
                        var icon = ParseIcon(line, lineNum);
                        if (icon != null)
                        {
                            _nodeMap[icon.Id] = icon;
                            if (_groupStack.Count > 0)
                                _groupStack.Peek().Children.Add(icon);
                            else if (diagram != null)
                                diagram.Children.Add(icon);
                        }
                    }
                    else if (line.StartsWith("edge "))
                    {
                        var edge = ParseEdge(line, lineNum);
                        if (edge != null && diagram != null)
                            diagram.Children.Add(edge);
                    }
                    else if (line == "end")
                    {
                        if (_groupStack.Count > 0)
                            _groupStack.Pop();
                    }
                    else
                    {
                        _errors.Add(new ParseError { Line = lineNum, Message = $"Unknown command: {line}" });
                    }
                }
                catch (Exception ex)
                {
                    _errors.Add(new ParseError { Line = lineNum, Message = ex.Message });
                }

                _currentLine++;
            }

            if (diagram == null)
            {
                diagram = new DiagramNode
                {
                    Id = "root",
                    Type = "diagram",
                    Title = "Untitled"
                };
                _errors.Add(new ParseError { Line = 0, Message = "No diagram declaration found" });
            }

            return new ParseResult { Diagram = diagram, Errors = _errors };
        }

        private DiagramNode ParseDiagram(string line, int lineNum)
        {
            var match = Regex.Match(line, @"diagram\s+""([^""]+)""");
            if (!match.Success)
                throw new Exception("Invalid diagram syntax. Expected: diagram \"title\"");

            return new DiagramNode
            {
                Id = "root",
                Type = "diagram",
                Title = match.Groups[1].Value
            };
        }

        private BoxNode ParseBox(string line, int lineNum)
        {
            var attrs = ParseAttributes(line);
            var id = ExtractId(line);

            if (string.IsNullOrEmpty(id))
            {
                _errors.Add(new ParseError { Line = lineNum, Message = "Box must have an id" });
                return null;
            }

            if (!attrs.ContainsKey("label"))
            {
                _errors.Add(new ParseError { Line = lineNum, Message = "Box must have a label" });
                return null;
            }

            return new BoxNode
            {
                Id = id,
                Label = attrs["label"],
                Position = new Position
                {
                    X = attrs.ContainsKey("x") ? double.Parse(attrs["x"]) : 0,
                    Y = attrs.ContainsKey("y") ? double.Parse(attrs["y"]) : 0
                },
                Size = new Size
                {
                    Width = attrs.ContainsKey("w") ? double.Parse(attrs["w"]) : 100,
                    Height = attrs.ContainsKey("h") ? double.Parse(attrs["h"]) : 50
                },
                Color = attrs.ContainsKey("color") ? attrs["color"] : null
            };
        }

        private GroupNode ParseGroup(string line, int lineNum)
        {
            var attrs = ParseAttributes(line);
            var id = ExtractId(line);

            if (string.IsNullOrEmpty(id))
            {
                _errors.Add(new ParseError { Line = lineNum, Message = "Group must have an id" });
                return null;
            }

            if (!attrs.ContainsKey("title"))
            {
                _errors.Add(new ParseError { Line = lineNum, Message = "Group must have a title" });
                return null;
            }

            return new GroupNode
            {
                Id = id,
                Title = attrs["title"],
                Position = new Position
                {
                    X = attrs.ContainsKey("x") ? double.Parse(attrs["x"]) : 0,
                    Y = attrs.ContainsKey("y") ? double.Parse(attrs["y"]) : 0
                },
                Size = new Size
                {
                    Width = attrs.ContainsKey("w") ? double.Parse(attrs["w"]) : 200,
                    Height = attrs.ContainsKey("h") ? double.Parse(attrs["h"]) : 150
                },
                Color = attrs.ContainsKey("color") ? attrs["color"] : null
            };
        }

        private IconNode ParseIcon(string line, int lineNum)
        {
            var attrs = ParseAttributes(line);
            var id = ExtractId(line);

            if (string.IsNullOrEmpty(id))
            {
                _errors.Add(new ParseError { Line = lineNum, Message = "Icon must have an id" });
                return null;
            }

            if (!attrs.ContainsKey("icon") || !attrs.ContainsKey("label"))
            {
                _errors.Add(new ParseError { Line = lineNum, Message = "Icon must have icon type and label" });
                return null;
            }

            return new IconNode
            {
                Id = id,
                Icon = attrs["icon"],
                Label = attrs["label"],
                Position = new Position
                {
                    X = attrs.ContainsKey("x") ? double.Parse(attrs["x"]) : 0,
                    Y = attrs.ContainsKey("y") ? double.Parse(attrs["y"]) : 0
                },
                Color = attrs.ContainsKey("color") ? attrs["color"] : null
            };
        }

        private EdgeNode ParseEdge(string line, int lineNum)
        {
            var match = Regex.Match(line, @"edge\s+(\w+)\s+->\s+(\w+)");
            if (!match.Success)
            {
                _errors.Add(new ParseError { Line = lineNum, Message = "Invalid edge syntax" });
                return null;
            }

            var from = match.Groups[1].Value;
            var to = match.Groups[2].Value;
            var attrs = ParseAttributes(line);

            if (!_nodeMap.ContainsKey(from))
            {
                _errors.Add(new ParseError { Line = lineNum, Message = $"Edge from unknown node: {from}" });
                return null;
            }

            if (!_nodeMap.ContainsKey(to))
            {
                _errors.Add(new ParseError { Line = lineNum, Message = $"Edge to unknown node: {to}" });
                return null;
            }

            return new EdgeNode
            {
                Id = $"{from}-{to}",
                From = from,
                To = to,
                Label = attrs.ContainsKey("label") ? attrs["label"] : null,
                Style = attrs.ContainsKey("style") ? attrs["style"] : "solid"
            };
        }

        private string ExtractId(string line)
        {
            var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length >= 2 ? parts[1] : null;
        }

        private Dictionary<string, string> ParseAttributes(string line)
        {
            var attrs = new Dictionary<string, string>();
            var regex = new Regex(@"(\w+)=""([^""]*)""|(\w+)=(\S+)");
            var matches = regex.Matches(line);

            foreach (Match match in matches)
            {
                var key = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[3].Value;
                var value = match.Groups[2].Success ? match.Groups[2].Value : match.Groups[4].Value;
                attrs[key] = value;
            }

            return attrs;
        }
    }
}
