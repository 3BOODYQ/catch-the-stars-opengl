namespace CatchStars;

internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length > 1 || (args.Length == 1 && args[0] != "--smoke-test"))
        {
            Console.Error.WriteLine("Usage: CatchStars [--smoke-test]");
            return 2;
        }
        try
        {
            using var game = new CatchWindow(args.Length == 1);
            game.Run();
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine($"Catch the Stars could not run: {error.Message}");
            Console.Error.WriteLine("See docs/build-it-yourself.md for graphics requirements and troubleshooting.");
            return 1;
        }
    }
}
