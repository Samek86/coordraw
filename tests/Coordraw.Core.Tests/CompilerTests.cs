using NUnit.Framework;
using Coordraw.Core.Compiler;
using Coordraw.Core.Models;
using Newtonsoft.Json;
using System.Linq;

namespace Coordraw.Core.Tests
{
    [TestFixture]
    public class CompilerTests
    {
        private EraserCompiler _compiler;

        [SetUp]
        public void Setup()
        {
            _compiler = new EraserCompiler();
        }

        [Test]
        public void Compile_SimpleBox_Success()
        {
            var diagram = new DiagramNode
            {
                Id = "root",
                Type = "diagram",
                Title = "Test"
            };

            var box = new BoxNode
            {
                Id = "test",
                Label = "Test Box",
                Position = new Position { X = 50, Y = 100 },
                Size = new Size { Width = 120, Height = 60 },
                Color = "blue"
            };

            diagram.Children.Add(box);

            var result = _compiler.Compile(diagram);

            Assert.AreEqual(1, result.Entities.Count);
            var shape = result.Entities[0] as EraserShape;
            Assert.IsNotNull(shape);
            Assert.AreEqual("test", shape.Id);
            Assert.AreEqual(50, shape.X);
            Assert.AreEqual(100, shape.Y);
            Assert.AreEqual(120, shape.Width);
            Assert.AreEqual(60, shape.Height);
            Assert.AreEqual("#4dabf7", shape.Color);
        }

        [Test]
        public void Compile_Group_Success()
        {
            var diagram = new DiagramNode
            {
                Id = "root",
                Type = "diagram",
                Title = "Test"
            };

            var group = new GroupNode
            {
                Id = "container",
                Title = "Container",
                Position = new Position { X = 100, Y = 100 },
                Size = new Size { Width = 400, Height = 300 },
                Color = "blue"
            };

            diagram.Children.Add(group);

            var result = _compiler.Compile(diagram);

            Assert.AreEqual(1, result.Entities.Count);
            var eraserGroup = result.Entities[0] as EraserGroup;
            Assert.IsNotNull(eraserGroup);
            Assert.AreEqual("container", eraserGroup.Id);
            Assert.AreEqual("group", eraserGroup.Type);
        }

        [Test]
        public void Compile_GroupWithChildren_SetsContainerId()
        {
            var diagram = new DiagramNode
            {
                Id = "root",
                Type = "diagram",
                Title = "Test"
            };

            var group = new GroupNode
            {
                Id = "container",
                Title = "Container",
                Position = new Position { X = 100, Y = 100 },
                Size = new Size { Width = 400, Height = 300 }
            };

            var box = new BoxNode
            {
                Id = "child",
                Label = "Child",
                Position = new Position { X = 120, Y = 120 },
                Size = new Size { Width = 100, Height = 50 }
            };

            group.Children.Add(box);
            diagram.Children.Add(group);

            var result = _compiler.Compile(diagram);

            Assert.AreEqual(2, result.Entities.Count);
            var childShape = result.Entities.OfType<EraserShape>().FirstOrDefault(e => e.Id == "child");
            Assert.IsNotNull(childShape);
            Assert.AreEqual("container", childShape.ContainerId);
        }

        [Test]
        public void Compile_Icon_Success()
        {
            var diagram = new DiagramNode
            {
                Id = "root",
                Type = "diagram",
                Title = "Test"
            };

            var icon = new IconNode
            {
                Id = "db",
                Icon = "database",
                Label = "Database",
                Position = new Position { X = 100, Y = 100 },
                Color = "purple"
            };

            diagram.Children.Add(icon);

            var result = _compiler.Compile(diagram);

            Assert.AreEqual(1, result.Entities.Count);
            var eraserIcon = result.Entities[0] as EraserIcon;
            Assert.IsNotNull(eraserIcon);
            Assert.AreEqual("db", eraserIcon.Id);
            Assert.AreEqual("database", eraserIcon.Icon);
            Assert.AreEqual("#9775fa", eraserIcon.Color);
        }

        [Test]
        public void Compile_Edge_Success()
        {
            var diagram = new DiagramNode
            {
                Id = "root",
                Type = "diagram",
                Title = "Test"
            };

            var box1 = new BoxNode
            {
                Id = "a",
                Label = "A",
                Position = new Position { X = 0, Y = 0 },
                Size = new Size { Width = 100, Height = 50 }
            };

            var box2 = new BoxNode
            {
                Id = "b",
                Label = "B",
                Position = new Position { X = 200, Y = 0 },
                Size = new Size { Width = 100, Height = 50 }
            };

            var edge = new EdgeNode
            {
                Id = "a-b",
                From = "a",
                To = "b",
                Label = "connects",
                Style = "dashed"
            };

            diagram.Children.Add(box1);
            diagram.Children.Add(box2);
            diagram.Children.Add(edge);

            var result = _compiler.Compile(diagram);

            Assert.AreEqual(2, result.Entities.Count);
            Assert.AreEqual(1, result.Connections.Count);

            var connection = result.Connections[0];
            Assert.AreEqual("a", connection.From);
            Assert.AreEqual("b", connection.To);
            Assert.AreEqual("connects", connection.Label);
            Assert.AreEqual("dashed", connection.LineStyle);
        }

        [Test]
        public void Compile_JapaneseDiagram_Success()
        {
            var diagram = new DiagramNode
            {
                Id = "root",
                Type = "diagram",
                Title = "本番環境"
            };

            var box = new BoxNode
            {
                Id = "client",
                Label = "クライアント",
                Position = new Position { X = 40, Y = 140 },
                Size = new Size { Width = 140, Height = 70 },
                Color = "green"
            };

            diagram.Children.Add(box);

            var result = _compiler.Compile(diagram);

            Assert.AreEqual(1, result.Entities.Count);
            var shape = result.Entities[0] as EraserShape;
            Assert.AreEqual("クライアント", shape.Texts[0].Text);
        }

        [Test]
        public void Compile_ColorMapping_Success()
        {
            var diagram = new DiagramNode { Id = "root", Type = "diagram", Title = "Test" };
            var box = new BoxNode
            {
                Id = "test",
                Label = "Test",
                Position = new Position { X = 0, Y = 0 },
                Size = new Size { Width = 100, Height = 50 },
                Color = "red"
            };

            diagram.Children.Add(box);
            var result = _compiler.Compile(diagram);

            var shape = result.Entities[0] as EraserShape;
            Assert.AreEqual("#ff6b6b", shape.Color);
        }

        [Test]
        public void Compile_ToJson_ValidFormat()
        {
            var diagram = new DiagramNode { Id = "root", Type = "diagram", Title = "Test" };
            var box = new BoxNode
            {
                Id = "test",
                Label = "Test",
                Position = new Position { X = 50, Y = 100 },
                Size = new Size { Width = 120, Height = 60 }
            };

            diagram.Children.Add(box);
            var result = _compiler.Compile(diagram);

            var json = JsonConvert.SerializeObject(result, Formatting.Indented);
            Assert.IsNotNull(json);
            Assert.IsTrue(json.Contains("entities"));
            Assert.IsTrue(json.Contains("connections"));
        }

        [Test]
        public void Validate_ValidDiagram_NoErrors()
        {
            var diagram = new DiagramNode { Id = "root", Type = "diagram", Title = "Test" };
            var box = new BoxNode
            {
                Id = "test",
                Label = "Test",
                Position = new Position { X = 0, Y = 0 },
                Size = new Size { Width = 100, Height = 50 }
            };

            diagram.Children.Add(box);

            var errors = _compiler.Validate(diagram);
            Assert.AreEqual(0, errors.Count);
        }

        [Test]
        public void Validate_DuplicateId_ReturnsError()
        {
            var diagram = new DiagramNode { Id = "root", Type = "diagram", Title = "Test" };
            var box1 = new BoxNode
            {
                Id = "test",
                Label = "Test 1",
                Position = new Position { X = 0, Y = 0 },
                Size = new Size { Width = 100, Height = 50 }
            };
            var box2 = new BoxNode
            {
                Id = "test",
                Label = "Test 2",
                Position = new Position { X = 200, Y = 0 },
                Size = new Size { Width = 100, Height = 50 }
            };

            diagram.Children.Add(box1);
            diagram.Children.Add(box2);

            var errors = _compiler.Validate(diagram);
            Assert.Greater(errors.Count, 0);
            Assert.IsTrue(errors.Any(e => e.Contains("Duplicate")));
        }

        [Test]
        public void Validate_InvalidDimensions_ReturnsError()
        {
            var diagram = new DiagramNode { Id = "root", Type = "diagram", Title = "Test" };
            var box = new BoxNode
            {
                Id = "test",
                Label = "Test",
                Position = new Position { X = 0, Y = 0 },
                Size = new Size { Width = -10, Height = 50 }
            };

            diagram.Children.Add(box);

            var errors = _compiler.Validate(diagram);
            Assert.Greater(errors.Count, 0);
            Assert.IsTrue(errors.Any(e => e.Contains("invalid width")));
        }
    }
}
