using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Commons.Music.Midi;
using midi_filter;
using Moq;
using Xunit;

namespace midi_filter_tests
{
    public class ApplicationTest
    {
        [Fact]
        public void PublisherClientTest()
        {
            // TODO: Create a mock? version of publisher client
            // TODO: Generate some midi data (can the empty one do this?)
            // TODO: Verify the upload code runs
            // TODO: Should we perhaps consider not using unbounded list - if so - how to deal with overflow? Ignore or write to file? 
            var app = new Application(null, new EmptyMidiAccess());
        }
        
        [Fact]
        public async Task TryOpenMidiTest()
        {
            var application = new Application(null, new EmptyMidiAccess());
            Assert.NotNull(await application.TryOpenMidiAsync("Dummy"));
            Assert.Null(await application.TryOpenMidiAsync("DOESNT EXIST"));
        }
    }
}
