public class Coin
{
    private int _faceValue;
    private int _pieces;

    public static readonly HashSet<int> Faces = [1, 2, 5, 10, 50, 100, 200, 500, 1000, 2000, 5000, 10000]; // or new List<int>() {1, 2, 5, 10}, but VSCode suggested simplification



    public Coin(int faceValue, int pieces)
    {
        if (!Faces.Contains(faceValue))
            throw new ArgumentException("coin face value doesn't exist!");

        _faceValue = faceValue;

        if (pieces < 0)
            throw new ArgumentException("coins amount cannot be negative!");

        _pieces = pieces;
    }
}