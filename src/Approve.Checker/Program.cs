using System.Collections;

internal static class Program
{
    internal static Task<int> Main(string[] args)
    {

        foreach (DictionaryEntry e in System.Environment.GetEnvironmentVariables())
        {
            Console.WriteLine(e.Key + " = " + e.Value);
        }

        return Task.FromResult(-1);
    }
}