using NUnit.Framework;
using Coordraw.Core.Parser;
using Coordraw.Core.Models;
using System.Linq;

namespace Coordraw.Core.Tests
{
    [TestFixture]
    public class ParserTests
    {
        private DslParser _parser;

        [SetUp]
        public void Setup()
        {
            _parser = new DslParser();
        }

        [Test]
        public void Parse_SimpleDiagram_Success()
        {
            var source = @"
diagram ""Test Diagram""

box frontend label=""Frontend"" x=50 y=100 w=120 h=60 color=blue
box backend label=""Backend"" x=250 y=100 w=140 h=60 color=green

edge frontend -> backend label=""API""
";

            var result = _parser.Parse(source);

            Assert.IsNotNull(result.Diagram);
            Assert.AreEqual("Test Diagram", result.Diagram.Title);
            Assert.AreEqual(0, result.Errors.Count);
            Assert.AreEqual(3, result.Diagram.Children.Count);

            var frontend = result.Diagram.Children[0] as BoxNode;
            Assert.IsNotNull(frontend);
            Assert.AreEqual("frontend", frontend.Id);
            Assert.AreEqual("Frontend", frontend.Label);
            Assert.AreEqual(50, frontend.Position.X);
            Assert.AreEqual(100, frontend.Position.Y);
            Assert.AreEqual(120, frontend.Size.Width);
            Assert.AreEqual(60, frontend.Size.Height);
            Assert.AreEqual("blue", frontend.Color);
        }

        [Test]
        public void Parse_JapaneseDiagram_Success()
        {
            var source = @"
diagram ""本番環境アーキテクチャ""

box client label=""クライアント"" x=40 y=140 w=140 h=70 color=green

group prod title=""本番環境"" x=200 y=40 w=520 h=280 color=blue
  icon api icon=server label=""注文API"" x=280 y=130
  icon db icon=postgres label=""Postgres"" x=520 y=130
end

edge client -> api label=""HTTPS""
";

            var result = _parser.Parse(source);

            Assert.AreEqual(0, result.Errors.Count);
            Assert.AreEqual("本番環境アーキテクチャ", result.Diagram.Title);

            var client = result.Diagram.Children[0] as BoxNode;
            Assert.AreEqual("クライアント", client.Label);

            var group = result.Diagram.Children[1] as GroupNode;
            Assert.AreEqual("本番環境", group.Title);
            Assert.AreEqual(2, group.Children.Count);

            var api = group.Children[0] as IconNode;
            Assert.AreEqual("注文API", api.Label);
        }

        [Test]
        public void Parse_GroupWithChildren_Success()
        {
            var source = @"
diagram ""Group Test""

group container title=""Container"" x=100 y=100 w=400 h=300 color=blue
  box item1 label=""Item 1"" x=120 y=120 w=100 h=50
  box item2 label=""Item 2"" x=250 y=120 w=100 h=50
end
";

            var result = _parser.Parse(source);

            Assert.AreEqual(0, result.Errors.Count);
            var group = result.Diagram.Children[0] as GroupNode;
            Assert.IsNotNull(group);
            Assert.AreEqual("container", group.Id);
            Assert.AreEqual(2, group.Children.Count);

            var item1 = group.Children[0] as BoxNode;
            Assert.AreEqual("item1", item1.Id);
        }

        [Test]
        public void Parse_IconNode_Success()
        {
            var source = @"
diagram ""Icon Test""

icon db icon=database label=""Database"" x=100 y=100 color=purple
";

            var result = _parser.Parse(source);

            Assert.AreEqual(0, result.Errors.Count);
            var icon = result.Diagram.Children[0] as IconNode;
            Assert.IsNotNull(icon);
            Assert.AreEqual("db", icon.Id);
            Assert.AreEqual("database", icon.Icon);
            Assert.AreEqual("Database", icon.Label);
            Assert.AreEqual("purple", icon.Color);
        }

        [Test]
        public void Parse_EdgeWithStyles_Success()
        {
            var source = @"
diagram ""Edge Test""

box a label=""A"" x=0 y=0 w=100 h=50
box b label=""B"" x=200 y=0 w=100 h=50

edge a -> b label=""solid"" style=solid
";

            var result = _parser.Parse(source);

            Assert.AreEqual(0, result.Errors.Count);
            var edge = result.Diagram.Children[2] as EdgeNode;
            Assert.IsNotNull(edge);
            Assert.AreEqual("a", edge.From);
            Assert.AreEqual("b", edge.To);
            Assert.AreEqual("solid", edge.Style);
        }

        [Test]
        public void Parse_CommentsIgnored_Success()
        {
            var source = @"
# This is a comment
diagram ""Test""

# Another comment
box test label=""Test"" x=0 y=0 w=100 h=50
";

            var result = _parser.Parse(source);

            Assert.AreEqual(0, result.Errors.Count);
            Assert.AreEqual(1, result.Diagram.Children.Count);
        }

        [Test]
        public void Parse_DefaultDimensions_Success()
        {
            var source = @"
diagram ""Test""

box test label=""Test"" x=10 y=10
";

            var result = _parser.Parse(source);

            Assert.AreEqual(0, result.Errors.Count);
            var box = result.Diagram.Children[0] as BoxNode;
            Assert.AreEqual(100, box.Size.Width);
            Assert.AreEqual(50, box.Size.Height);
        }

        [Test]
        public void Parse_MissingLabel_ReturnsError()
        {
            var source = @"
diagram ""Test""

box test x=10 y=10 w=100 h=50
";

            var result = _parser.Parse(source);

            Assert.Greater(result.Errors.Count, 0);
            Assert.IsTrue(result.Errors.Any(e => e.Message.Contains("label")));
        }

        [Test]
        public void Parse_UnknownEdgeNode_ReturnsError()
        {
            var source = @"
diagram ""Test""

box a label=""A"" x=0 y=0 w=100 h=50

edge a -> unknown label=""test""
";

            var result = _parser.Parse(source);

            Assert.Greater(result.Errors.Count, 0);
            Assert.IsTrue(result.Errors.Any(e => e.Message.Contains("unknown")));
        }

        [Test]
        public void Parse_QuotedLabelsWithSpaces_Success()
        {
            var source = @"
diagram ""Test Diagram""

box test label=""This is a long label"" x=0 y=0 w=200 h=60
";

            var result = _parser.Parse(source);

            Assert.AreEqual(0, result.Errors.Count);
            var box = result.Diagram.Children[0] as BoxNode;
            Assert.AreEqual("This is a long label", box.Label);
        }

        [Test]
        public void Parse_EmptySource_ReturnsDefaultDiagram()
        {
            var result = _parser.Parse("");

            Assert.IsNotNull(result.Diagram);
            Assert.AreEqual("Untitled", result.Diagram.Title);
            Assert.Greater(result.Errors.Count, 0);
        }
    }
}
