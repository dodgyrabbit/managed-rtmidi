using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Commons.Music.Midi;
using System.Text.Json;
using Dodgyrabbit.Google.Cloud.PubSub.V1;

namespace midi_filter
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await new Application("", "cloud-piano", "notes").Run();
        }
    }
}