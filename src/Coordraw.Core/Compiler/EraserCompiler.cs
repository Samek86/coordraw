using System;
using System.Collections.Generic;
using System.Linq;
using Coordraw.Core.Models;

namespace Coordraw.Core.Compiler
{
    public class EraserCompiler
    {
        private static readonly Dictionary<string, string> ColorMap = new Dictionary<string, string>
        {
            { "red", "#ff6b6b" },
            { "orange", "#ffa94d" },
            { "yellow", "#ffd43b" },
            { "green", "#51cf66" },
            { "blue", "#4dabf7" },
            { "purple", "#9775fa" },
            { "pink", "#f06595" },
            { "gray", "#868e96" },
            { "black", "#212529" },
            { "white", "#ffffff" }
        };

        private Dictionary<string, GroupNode> _groupMap;
        private List<string> _validationErrors;

        public EraserDiagram Compile(DiagramNode diagram)
        {
            _groupMap = new Dictionary<string, GroupNode>();
            _validationErrors = new List<string>();

            // Build group map
            foreach (var child in diagram.Children.OfType<GroupNode>())
            {
                _groupMap[child.Id] = child;
            }

            var result = new EraserDiagram();

            foreach (var child in diagram.Children)
            {
                if (child is BoxNode box)
                    result.Entities.Add(CompileBox(box));
                else if (child is GroupNode group)
                {
                    result.Entities.Add(CompileGroup(group));
                    // Compile children of groups
                    foreach (var groupChild in group.Children)
                    {
                        if (groupChild is BoxNode childBox)
                            result.Entities.Add(CompileBox(childBox));
                        else if (groupChild is IconNode childIcon)
                            result.Entities.Add(CompileIcon(childIcon));
                    }
                }
                else if (child is IconNode icon)
                    result.Entities.Add(CompileIcon(icon));
                else if (child is EdgeNode edge)
                    result.Connections.Add(CompileEdge(edge));
            }

            return result;
        }

        public List<string> Validate(DiagramNode diagram)
        {
            var errors = new List<string>();
            var nodeIds = new HashSet<string>();

            void ValidateElement(DiagramElement element)
            {
                if (string.IsNullOrEmpty(element.Id))
                {
                    errors.Add("Element missing ID");
                    return;
                }

                if (nodeIds.Contains(element.Id))
                {
                    errors.Add($"Duplicate ID: {element.Id}");
                }
                nodeIds.Add(element.Id);

                if (element is BoxNode box)
                {
                    if (string.IsNullOrEmpty(box.Label))
                        errors.Add($"Box '{box.Id}' missing label");
                    if (box.Size.Width <= 0)
                        errors.Add($"Box '{box.Id}' has invalid width");
                    if (box.Size.Height <= 0)
                        errors.Add($"Box '{box.Id}' has invalid height");
                }
                else if (element is GroupNode group)
                {
                    if (string.IsNullOrEmpty(group.Title))
                        errors.Add($"Group '{group.Id}' missing title");
                    if (group.Size.Width <= 0)
                        errors.Add($"Group '{group.Id}' has invalid width");
                    if (group.Size.Height <= 0)
                        errors.Add($"Group '{group.Id}' has invalid height");

                    foreach (var child in group.Children)
                        ValidateElement(child);
                }
                else if (element is IconNode icon)
                {
                    if (string.IsNullOrEmpty(icon.Icon))
                        errors.Add($"Icon '{icon.Id}' missing icon type");
                    if (string.IsNullOrEmpty(icon.Label))
                        errors.Add($"Icon '{icon.Id}' missing label");
                }
            }

            foreach (var child in diagram.Children)
            {
                if (child is EdgeNode edge)
                {
                    if (!nodeIds.Contains(edge.From))
                        errors.Add($"Edge references unknown 'from' node: {edge.From}");
                    if (!nodeIds.Contains(edge.To))
                        errors.Add($"Edge references unknown 'to' node: {edge.To}");
                }
                else
                {
                    ValidateElement(child);
                }
            }

            foreach (var child in diagram.Children.OfType<EdgeNode>())
            {
                if (!nodeIds.Contains(child.From))
                    errors.Add($"Edge references unknown 'from' node: {child.From}");
                if (!nodeIds.Contains(child.To))
                    errors.Add($"Edge references unknown 'to' node: {child.To}");
            }

            return errors;
        }

        private EraserShape CompileBox(BoxNode node)
        {
            string containerId = null;
            foreach (var kvp in _groupMap)
            {
                if (kvp.Value.Children.Any(c => c.Id == node.Id))
                {
                    containerId = kvp.Key;
                    break;
                }
            }

            return new EraserShape
            {
                Id = node.Id,
                X = node.Position.X,
                Y = node.Position.Y,
                Width = node.Size.Width,
                Height = node.Size.Height,
                Texts = new List<EraserText>
                {
                    new EraserText { Text = node.Label, Typeface = "clean", FontSize = 14 }
                },
                ContainerId = containerId,
                Color = MapColor(node.Color)
            };
        }

        private EraserGroup CompileGroup(GroupNode node)
        {
            var group = new EraserGroup
            {
                Id = node.Id,
                X = node.Position.X,
                Y = node.Position.Y,
                Width = node.Size.Width,
                Height = node.Size.Height,
                Texts = new List<EraserText>
                {
                    new EraserText { Text = node.Title, Typeface = "clean", FontSize = 16 }
                },
                Color = MapColor(node.Color)
            };

            return group;
        }

        private EraserIcon CompileIcon(IconNode node)
        {
            string containerId = null;
            foreach (var kvp in _groupMap)
            {
                if (kvp.Value.Children.Any(c => c.Id == node.Id))
                {
                    containerId = kvp.Key;
                    break;
                }
            }

            return new EraserIcon
            {
                Id = node.Id,
                Icon = node.Icon,
                X = node.Position.X,
                Y = node.Position.Y,
                Texts = new List<EraserText>
                {
                    new EraserText { Text = node.Label, Typeface = "clean", FontSize = 14 }
                },
                ContainerId = containerId,
                Color = MapColor(node.Color)
            };
        }

        private EraserConnection CompileEdge(EdgeNode node)
        {
            return new EraserConnection
            {
                Id = node.Id,
                From = node.From,
                To = node.To,
                Label = node.Label,
                EndArrowhead = node.Arrow ? "arrow" : "none",
                LineStyle = node.Style ?? "solid"
            };
        }

        private string MapColor(string color)
        {
            if (string.IsNullOrEmpty(color))
                return null;

            return ColorMap.ContainsKey(color.ToLower()) ? ColorMap[color.ToLower()] : null;
        }
    }
}
