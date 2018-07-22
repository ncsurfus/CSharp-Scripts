#r "nuget: System.Threading.Tasks.Dataflow,4.5.0"

using System.Threading;
using System.Threading.Tasks.Dataflow;

// Set high max worker count assuming worker is not CPU bound.
var workerCount = 200;
var workCount = 1000;

// Random number generator to create a random delay later on.
var rgen = new Random();

var workerBlock = new ActionBlock<string>(async message =>
{
    // Random async processing work...
    var delay = 0;
    lock (rgen)
    {
        delay = rgen.Next(0, 5);
    }
    await Task.Delay(delay).ConfigureAwait(false);
    Console.WriteLine($"T-{Thread.CurrentThread.ManagedThreadId} D-{delay}: {message}");

}, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = workerCount });

// Queue work items
for(int i = 0; i != workCount; i++)
{
    await workerBlock.SendAsync($"TestMessage{i}").ConfigureAwait(false);
}

// Let the channel know we're closing down.
workerBlock.Complete();

// Wait for all workers to finish.
await workerBlock.Completion.ConfigureAwait(false);