using System.Collections;
using Approve.Checker;

internal static class Program
{
    internal static async Task<int> Main(string[] args)
    {
        var app = new App();
        return await app.Run();
    }
}