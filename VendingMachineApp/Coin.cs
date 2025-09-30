namespace VendingMachineApp;
public class Coin
{
    public int Face { get; }
    public int Amount { get; set; }

    public Coin(int face, int amount)
    {
        Face = face;
        Amount = amount;
    }
}