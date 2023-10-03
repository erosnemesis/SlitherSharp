namespace SlitherSharp;

public class Program
{
    private static readonly Slither _slither = new Slither();

    public static async Task Main(string[] args)
    {
        await _slither.StartGame();
    }

    
}