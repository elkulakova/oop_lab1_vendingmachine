public class Coin
{
    private int _faceValue;

    public int FaceValue
    {
        get { return _faceValue; }
        set
        {
            List<int> faces = [1, 2, 5, 10]; // or new List<int>() {1, 2, 5, 10}, but VSCode suggested simplification
            if (!faces.Contains(value))
                throw new ArgumentException("coin face value doesn't exist!");
            _faceValue = value;
        }
    }

    public Coin(int faceValue)
    {
        _faceValue = faceValue;
    }
}