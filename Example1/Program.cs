using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackZero.NetStandard;
using TrackZero.NetStandard.Models;

namespace Example1
{
    class Program
    {
        static async Task Main(string[] args)
        {
            TrackZeroClient tx = new TrackZeroClient("8fb844be-317f-490a-98b9-7cfa7acc07f7.v7YcdlQVYl93FvGT0rV4PeakMCQfpo5q");
            Entity et = new Entity("UU", 1);
            et.AddAttribute("BD", DateTime.UtcNow);

            await tx.UpsertEntityAsync(et).ConfigureAwait(false);
            await tx.UpsertEntityAsync(et).ConfigureAwait(false);

            await Task.Delay(int.MaxValue);
        }
    }
}
