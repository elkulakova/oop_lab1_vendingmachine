using System.Collections;

public class User
{
    public Dictionary<int, int> UserPurse { get; set; }

    public User()
    {
        UserPurse = Coin.Faces.ToDictionary(k => k, k => 0);

        foreach (int face in Coin.Faces)
        {
            Console.WriteLine($"enter the amount of {face} RUB-coins/banknotes (it must be a non-negative integer value)");
            string? coin_amount = Console.ReadLine();

            if (!int.TryParse(coin_amount, out int value) || value < 0)
            {
                do
                {
                    Console.WriteLine($"invalid value. try again");
                    coin_amount = Console.ReadLine();
                } while (!int.TryParse(coin_amount, out value) || value < 0);
            }

            UserPurse[face] = value;
        }
    }
}