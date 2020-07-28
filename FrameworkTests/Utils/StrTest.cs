using NUnit.Framework;
using Unisave.Utils;

namespace FrameworkTests.Utils
{
    [TestFixture]
    public class StrTest
    {
        [Test]
        public void ItMatchesWildcards()
        {
            // Str.Is(input, pattern)
            
            // exact match
            Assert.True(Str.Is("foo/*", "foo/*"));
            
            // one wildcard
            Assert.True(Str.Is("foo/bar", "foo/*"));
            Assert.True(Str.Is("foo/bar/baz/*", "foo/*"));
            Assert.True(Str.Is("foo/**", "foo/*"));
            Assert.True(Str.Is("foo/", "foo/*"));
            
            Assert.False(Str.Is("foo", "foo/*"));
            Assert.False(Str.Is("/foo/", "foo/*"));
            Assert.False(Str.Is("", "foo/*"));
            
            // two wildcards
            Assert.True(Str.Is("lorem/foo/bar", "*foo/*"));
            Assert.True(Str.Is("*/foo/*", "*foo/*"));
            Assert.True(Str.Is("foo/", "*foo/*"));
            Assert.True(Str.Is("lorem/foo/bar/foo/ipsum", "*foo/*"));
            
            Assert.False(Str.Is("foo", "*foo/*"));
            Assert.False(Str.Is("/foo", "*foo/*"));
            Assert.False(Str.Is("lorem ipsum", "*foo/*"));
            Assert.False(Str.Is("", "*foo/*"));
        }
    }
}