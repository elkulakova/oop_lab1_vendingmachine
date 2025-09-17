public class Coin
{
    private int _faceValue;

    public int FaceValue
    {
        get { return _faceValue; }
        set
        {
            if (not(1, 2, 5, 10).Contains(value))
                throw new ArgumentException("coin face value doesn't exist!");
            _faceValue = value;
        }
    }

    public Coin(int faceValue)
    {
        _faceValue = faceValue;
    }
}