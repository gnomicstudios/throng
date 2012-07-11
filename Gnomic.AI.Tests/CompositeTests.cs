// CompositeTests.cs
//

using System.Linq;

using NSubstitute;
using NUnit.Framework;

namespace Gnomic.AI.Tests
{
    [TestFixture]
    public class CompositeTests
    {
        private class TestComposite : Composite
        {
            public override Result Tick(Search search) {
                return Result.Invalid;
            }
            public override void OnChildSuccess(Search search) { }
            public override void OnChildFailure(Search search) { }
            protected override bool IsValid(IActor actor) { return true; }
        }

        [Test]
        public void TestAddChild()
        {
            var composite = Substitute.For<Composite>();
            var child = Substitute.For<Single>();
            var search = Substitute.For<Search>();

            composite.AddChild(child);

            Assert.AreEqual(1, composite.Children.Count());
            
            //child.Success(search);
            composite.Received().OnChildSuccess(search);
        }

        [Test]
        public void TestRemoveChild()
        {
        }
    }
}