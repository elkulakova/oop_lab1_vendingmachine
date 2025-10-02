namespace VendingMachineApp;
public class CashDesk
{
    //private List<Coin> _coins;
    private readonly HashSet<Coin> CoinsSet = new(); // to save coins of different faces only once and then increase their amount
    private static readonly HashSet<int> FacesSet = new() { 1, 2, 5, 10, 50, 100, 200, 500, 1000, 2000, 5000 }; // or new List<int>() {1, 2, 5, 10}, but VSCode suggested simplification
    public CashDesk() // primary constructor, vendingMachine is already readonly field
    {
    }
    public HashSet<Coin> GetCoinsSet()
    {
        return CoinsSet;
    }
    public void View()
    {
        if (!IsEmpty())
        {
            Console.WriteLine("\nhere is the CONTENT of the cash desk:\n");
            foreach (Coin entry in CoinsSet)
            {
                Console.WriteLine($"{entry.Face}-coin/banknotes x {entry.Amount} pieces");
            }
        }
        else
        {
            Console.WriteLine("\nthe cash desk is EMPTY, nothing to show.");
        }
    }
    public int TotalAmount
    {
        get
        {
            int total = 0;
            foreach (Coin coin in CoinsSet)
            {
                total += coin.Face * coin.Amount;
            }
            return total;
        }
    }
    private static void CorrectnessCheck(int face, int amount)
    {
        if (amount <= 0 || !FacesSet.Contains(face))
        {
            if (amount > 0 && !FacesSet.Contains(face))
                throw new ArgumentException("\nwrong input, coin face is not accepted by the vending machine.");
            else if (amount <= 0 && FacesSet.Contains(face))
                throw new ArgumentException("\nwrong input, amount of coins to add cannot be equal or less than 0, try again.");
            else
                throw new ArgumentException("\nwrong input, coin face is not accepted by the vending machine AND amount is non-positive, try again.");
        }
    }
    public void AddCoin(int face, int amount)
    {
        CorrectnessCheck(face, amount);

        Coin? foundCoin = CoinsSet.FirstOrDefault(coin => coin.Face == face);
        if (foundCoin != null)
        {
            foundCoin.Amount += amount;
        }
        else
        {
            CoinsSet.Add(new Coin(face, amount));
        }
    }
    private void RemoveCoin(int face, int amount)
    {
        CorrectnessCheck(face, amount);

        Coin? foundCoin = CoinsSet.FirstOrDefault(coin => coin.Face == face);
        if (foundCoin != null)
        {
            if (foundCoin.Amount < amount)
            {
                Console.WriteLine("there are not enough coins of this face in the cash desk to remove the requested amount!");
                return;
            }
            foundCoin.Amount -= amount;
            if (foundCoin.Amount == 0)
                CoinsSet.Remove(foundCoin);
        }
        else
        {
            Console.WriteLine("\nthere are no coins of this face in the cash desk to remove!");
        }
    }
    public string GiveChange(int change, HashSet<Coin> money_list)
    {
        int remaining = change;
        HashSet<Coin> temp_desk = [.. CoinsSet.Select(coin => new Coin(coin.Face, coin.Amount))];
        foreach (var entry in money_list)
        {
            Coin? existingCoin = temp_desk.FirstOrDefault(coin => coin.Face == entry.Face);
            if (existingCoin != null)
            {
                existingCoin.Amount += entry.Amount;
            }
            else
            {
                temp_desk.Add(new Coin(entry.Face, entry.Amount));
            }
        }

        List<int> sorted_faces = [];
        foreach (Coin coin in temp_desk)
        {
            sorted_faces.Add(coin.Face);
        }
        sorted_faces = sorted_faces.OrderByDescending(face => face).ToList();
        List<Coin> change_list = [];

        foreach (int face in sorted_faces)
        {
            if (remaining <= 0)
                break;

            int needed = remaining / face;
            Coin foundCoin = temp_desk.First(coin => coin.Face == face); // no default, cause face is from the existing coins in the cash desk
            if (needed > 0 && foundCoin.Amount > 0)
            {
                int available = foundCoin.Amount;
                int to_give = Math.Min(available, needed);
                change_list.Add(new Coin(face, to_give)); // to_give is always > 0
                remaining -= to_give * face;
                foundCoin.Amount -= to_give;
            }
        }
        if (remaining > 0)
        {
            Console.WriteLine($"\n\nsorry, there is NOT ENOUGH CHANGE in the machine/there is enough change but there is NO POSSIBILITY to give you precisely {change} RUB. your money has been refunded");
            return "refund";
        }
        else
        {
            // add the received money to the cash desk
            foreach (var coin in money_list)
            {
                AddCoin(coin.Face, coin.Amount);
            }

            Console.WriteLine($"\nhere is your change: {change} RUB, given as:");
            foreach (var coin in change_list)
            {
                Console.WriteLine($"{coin.Face}-coin/banknotes x {coin.Amount} pieces");
                RemoveCoin(coin.Face, coin.Amount); // remove the given change from the cash desk
            }
            return "change";
        }
    }
    public bool IsEmpty()
    {
        return CoinsSet.Count == 0 || TotalAmount == 0; // ~ CoinsSet.All(a => a.Amount == 0) || CoinsSet.Count == 0;
    }
    public void Fill() // fill the cash desk with random num of coins/banknotes of each face
    {
        Random rand = new();
        foreach (int face in FacesSet)
        {
            int num = rand.Next(0, 11);
            if (num == 0)
                continue;
            AddCoin(face, num);
        }
    }
    public void Clear() // remove all the money from the cash desk
    {
        CoinsSet.Clear();
        Console.WriteLine("\ncash desk is empty now");
    }
}