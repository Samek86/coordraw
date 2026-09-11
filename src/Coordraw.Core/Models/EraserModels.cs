using System.Collections.Generic;
using Newtonsoft.Json;

namespace Coordraw.Core.Models
{
    public class EraserDiagram
    {
        [JsonProperty("entities")]
        public List<EraserEntity> Entities { get; set; } = new List<EraserEntity>();

        [JsonProperty("connections")]
        public List<EraserConnection> Connections { get; set; } = new List<EraserConnection>();
    }

    public abstract class EraserEntity
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("x")]
        public double X { get; set; }

        [JsonProperty("y")]
        public double Y { get; set; }

        [JsonProperty("texts")]
        public List<EraserText> Texts { get; set; } = new List<EraserText>();

        [JsonProperty("color", NullValueHandling = NullValueHandling.Ignore)]
        public string Color { get; set; }

        [JsonProperty("styleMode", NullValueHandling = NullValueHandling.Ignore)]
        public string StyleMode { get; set; } = "clean";
    }

    public class EraserShape : EraserEntity
    {
        [JsonProperty("width")]
        public double Width { get; set; }

        [JsonProperty("height")]
        public double Height { get; set; }

        [JsonProperty("containerId", NullValueHandling = NullValueHandling.Ignore)]
        public string ContainerId { get; set; }

        public EraserShape()
        {
            Type = "shape";
        }
    }

    public class EraserGroup : EraserEntity
    {
        [JsonProperty("width")]
        public double Width { get; set; }

        [JsonProperty("height")]
        public double Height { get; set; }

        public EraserGroup()
        {
            Type = "group";
        }
    }

    public class EraserIcon : EraserEntity
    {
        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("containerId", NullValueHandling = NullValueHandling.Ignore)]
        public string ContainerId { get; set; }

        public EraserIcon()
        {
            Type = "icon";
        }
    }

    public class EraserText
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("typeface", NullValueHandling = NullValueHandling.Ignore)]
        public string Typeface { get; set; } = "clean";

        [JsonProperty("fontSize", NullValueHandling = NullValueHandling.Ignore)]
        public int? FontSize { get; set; } = 14;
    }

    public class EraserConnection
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("from")]
        public string From { get; set; }

        [JsonProperty("to")]
        public string To { get; set; }

        [JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
        public string Label { get; set; }

        [JsonProperty("endArrowhead", NullValueHandling = NullValueHandling.Ignore)]
        public string EndArrowhead { get; set; } = "arrow";

        [JsonProperty("lineStyle", NullValueHandling = NullValueHandling.Ignore)]
        public string LineStyle { get; set; } = "solid";

        [JsonProperty("typeface", NullValueHandling = NullValueHandling.Ignore)]
        public string Typeface { get; set; } = "clean";

        [JsonProperty("fontSize", NullValueHandling = NullValueHandling.Ignore)]
        public int? FontSize { get; set; } = 12;
    }
}
