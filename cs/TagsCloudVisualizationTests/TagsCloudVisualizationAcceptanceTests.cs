using FluentAssertions;
using System.Drawing;
using TagsCloudVisualization;

namespace TagsCloudVisualizationTests;

[TestFixture]
public class TagsCloudVisualizationAcceptanceTests
{
    private CircularCloudLayouter _layouter;
    private readonly Point _center = new(50, 50);

    [SetUp]
    public void Setup()
    {
        _layouter = new CircularCloudLayouter(_center);
    }

    [TearDown]
    public void SaveTagCloudWhenTestFailed()
    {
        if (TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed)
        {
            var testName = TestContext.CurrentContext.Test.Name;
            var fileName = $"FailedTest_{testName}_{DateTime.Now:HH_mm_ss}.png";
            var dir = Path.Combine(TestContext.CurrentContext.WorkDirectory, "TestImages");
            var path = Path.Combine(dir, fileName);
            Directory.CreateDirectory(dir);
            TagCloudSaver.Save(_layouter, path, 100, 100);
            Console.WriteLine($"Tag cloud visualization saved to file {path}");
        }
    }

    [Test]
    public void PutNextRectangle_ReturnsFirstRectangleInCenter()
    {
        _layouter.PutNextRectangle(new Size(10, 15))
            .Should().BeEquivalentTo(new Rectangle((int)(50 - 10 / 2.0), (int)(50 - 15 / 2.0), 10, 15));
    }

    [Test]
    public void PutNextRectangle_WhenSecondRectangle_ReturnsRectangleNotIntersectsWithFirst()
    {
        var rect1 = _layouter.PutNextRectangle(new Size(10, 15));
        var rect2 = _layouter.PutNextRectangle(new Size(13, 7));
        rect1.IntersectsWith(rect2).Should().BeFalse();
        rect2.IntersectsWith(rect1).Should().BeFalse();
    }

    [Test]
    public void WithRectangle_ReturnsSameLayouter()
    {
        var layouter = _layouter.WithRectangle(new Size(10, 15));
        layouter.Should().BeSameAs(_layouter);
    }

    [Test]
    public void PutNextRectangle_WhenManyRectangles_ReturnsRectanglesNotIntersects()
    {
        var sizes = new[]
        {
            new Size(10, 15),
            new Size(13, 7),
            new Size(5, 5),
            new Size(20, 10),
            new Size(8, 12),
            new Size(1, 1),
            new Size(2, 2)
        };
        var rectangles = sizes.Select(_layouter.PutNextRectangle).ToList();
        for (int i = 0; i < rectangles.Count; i++)
        {
            for (int j = i + 1; j < rectangles.Count; j++)
            {
                rectangles[i].IntersectsWith(rectangles[j]).Should().BeFalse();
                rectangles[j].IntersectsWith(rectangles[i]).Should().BeFalse();
            }
        }
    }

    [Test]
    public void PutNextRectangle_NewRectangleNearbyToCloud()
    {
        var sizes = new[]
        {
            new Size(10, 15),
            new Size(13, 7),
            new Size(5, 5),
            new Size(20, 10),
            new Size(8, 12),
            new Size(1, 1),
            new Size(2, 2)
        };
        var rects = new List<Rectangle>();
        foreach (var size in sizes)
        {
            var rect = _layouter.PutNextRectangle(size);
            if (rects.Count == 0)
            {
                rects.Add(rect);
                continue;
            }
            rects.Any(cloudRect => IsNearbyWithNext(rect, cloudRect)).Should().BeTrue();
            rects.Add(rect);
        }
    }

    private bool IsNearbyWithNext(Rectangle newRect, Rectangle cloudRects)
    {
        var inaccuracy = _layouter.Indent + 2;
        var copyNewRect = newRect;
        copyNewRect.Inflate(inaccuracy, inaccuracy);
        return copyNewRect.IntersectsWith(cloudRects);
    }

    [Test]
    public void PutNextRectangle_ReturnsRectangleWithRequestedSize()
    {
        var sizes = new[]
        {
            new Size(10, 15),
            new Size(13, 7),
            new Size(5, 5),
            new Size(20, 10),
            new Size(8, 12),
            new Size(1, 1),
            new Size(2, 2)
        };
        foreach (var size in sizes)
        {
            var rect = _layouter.PutNextRectangle(size);
            rect.Size.Should().Be(size);
        }
    }

    [Test]
    public void PutNextRectangle_WhenInvalidRectangleSize_ThrowArgumentException()
    {
        Action action = () => _layouter.PutNextRectangle(new Size(-1, 10));
        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void CircularCloudLayouter_WhenInvalidSpiralStep_ThrowArgumentException()
    {
        Action action = () => new CircularCloudLayouter(new Point(0, 0), -1);
        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void CircularCloudLayouter_WhenInvalidRadiusOffset_ThrowArgumentException()
    {
        Action action = () => new CircularCloudLayouter(new Point(0, 0), 0.05, -3.0);
        action.Should().Throw<ArgumentException>();
    }

    [Test]
    public void CircularCloudLayouter_WhenInvalidIndent_ThrowArgumentException()
    {
        Action action = () => new CircularCloudLayouter(new Point(0, 0), 0.05, 3.0, -1);
        action.Should().Throw<ArgumentException>();
    }
}
