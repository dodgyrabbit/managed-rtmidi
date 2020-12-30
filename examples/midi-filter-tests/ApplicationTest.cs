using System;
using midi_filter;
using Xunit;

namespace midi_filter_tests
{
    public class ApplicationTest
    {
        [Fact]
        public void FilePathTest()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                var app = new Application("", "projectId", "topicId", null);
            });
        }
    }
}
