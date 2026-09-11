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

        public EraserDiagram Compile(DiagramNode diagram)
        {
            _groupMap = new Dictionary<string, GroupNode>();

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
                    result.Entities.Add(CompileGroup(group));
                else if (child is IconNode icon)
                    result.Entities.Add(CompileIcon(icon));
                else if (child is EdgeNode edge)
                    result.Connections.Add(CompileEdge(edge));
            }

            return result;
        }

        private EraserShape CompileBox(BoxNode node)
        {
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
                Color = MapColor(node.Color)
            };
        }

        private EraserGroup CompileGroup(GroupNode node)
        {
            return new EraserGroup
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
