#r "nuget: System.Threading.Channels,4.5.0"

using System.Net.Http;
using System.Threading;
using System.Threading.Channels;

var workerCount = 200;

// Create a channel with no buffering.
var channel = Channel.CreateBounded<string>(new BoundedChannelOptions(1){ SingleWriter = true });

var client = new HttpClient();

// Create workers
var workers = Enumerable
    .Range(1, workerCount)
    .Select(async id =>
    {
        // await until there is data. If WaitToReadAsync returns false the channel is closed.
        while (await channel.Reader.WaitToReadAsync().ConfigureAwait(false))
        {
            // Attempt to get a message
            if (!channel.Reader.TryRead(out var message))
            {
                // We were woken up for data, but failed to get a message.
                // You almost had it..
                continue;
            }

            // Random async processing work...
            using(var request = await client.GetAsync($@"https://postman-echo.com/get?message={message}").ConfigureAwait(false))
            {
                request.EnsureSuccessStatusCode();
                Console.WriteLine($"{id}: {message}");
            }
        }
        Console.WriteLine($"{id}: Closed");
    }).ToArray();

Console.WriteLine("Starting work");

// Queue work items
foreach(var i in Enumerable.Range(1, 1000))
{
    await channel.Writer.WriteAsync($"TestMessage{i}").ConfigureAwait(false);
}

Console.WriteLine("Work Submitted");

// Let the channel know we're closing down.
channel.Writer.Complete();
Console.WriteLine("Channel closing");

// Wait for all workers to finish.
await Task.WhenAll(workers).ConfigureAwait(false);

Console.WriteLine("Work Completed");
client.Dispose();