namespace Application;

internal static class Program
{
    private static void Main(string[] args)
    {
        var app = new Application("LearnOpenGL", 1280, 720);
        app.Run();
    }
}
