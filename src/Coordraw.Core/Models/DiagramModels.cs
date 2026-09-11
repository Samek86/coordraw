using System;
using System.Collections.Generic;

namespace Coordraw.Core.Models
{
    public class DiagramNode
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public List<DiagramElement> Children { get; set; } = new List<DiagramElement>();
    }

    public abstract class DiagramElement
    {
        public string Id { get; set; }
        public string Type { get; set; }
    }

    public class BoxNode : DiagramElement
    {
        public string Label { get; set; }
        public Position Position { get; set; }
        public Size Size { get; set; }
        public string Color { get; set; }

        public BoxNode()
        {
            Type = "box";
        }
    }

    public class GroupNode : DiagramElement
    {
        public string Title { get; set; }
        public Position Position { get; set; }
        public Size Size { get; set; }
        public string Color { get; set; }
        public List<DiagramElement> Children { get; set; } = new List<DiagramElement>();

        public GroupNode()
        {
            Type = "group";
        }
    }

    public class IconNode : DiagramElement
    {
        public string Icon { get; set; }
        public string Label { get; set; }
        public Position Position { get; set; }
        public string Color { get; set; }

        public IconNode()
        {
            Type = "icon";
        }
    }

    public class EdgeNode : DiagramElement
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Label { get; set; }
        public string Style { get; set; }
        public bool Arrow { get; set; } = true;

        public EdgeNode()
        {
            Type = "edge";
        }
    }

    public class Position
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public class Size
    {
        public double Width { get; set; }
        public double Height { get; set; }
    }

    public class ParseResult
    {
        public DiagramNode Diagram { get; set; }
        public List<ParseError> Errors { get; set; } = new List<ParseError>();
    }

    public class ParseError
    {
        public int Line { get; set; }
        public string Message { get; set; }
    }
}
