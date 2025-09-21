using System.Collections;

public class User
{
    public Dictionary<int, int> UserPurse = Coin.Faces.ToDictionary(k => k, k => 0);

    public User()
    {
        Dictionary<int, int> userPurse = [];
        foreach (int face in Coin.Faces)
        {
            Console.WriteLine($"enter the amount of {face} RUB-coins/banknotes (it must be a non-negative integer value)");
            string? coin_amount = Console.ReadLine();

            if (int.TryParse(coin_amount, out int value)) // returns 0 as default
            {
                switch (value)
                {
                    case < 0:
                        do
                        {
                            Console.WriteLine($"enter the amount of {face} RUB-coins/banknotes again (positive integer)");
                            _ = Console.ReadLine();
                        } while (value < 0);
                        return;

                    default:
                        userPurse[face] = value;
                        return;
                }
            }
            else
            {
                Console.WriteLine($"are you sure you have ZERO {face} RUB-coins/banknotes? (yes/no)");
                string? approval = Console.ReadLine();

                if (!string.IsNullOrEmpty(approval) && !string.IsNullOrWhiteSpace(approval))
                {
                    switch (approval.ToLower().Trim())
                    {
                        case "yes":
                        case "y":
                        case "1":
                            userPurse.Add(face, value);
                            return;

                        case "no":
                        case "n":
                        case "2":
                            do
                            {
                                Console.WriteLine($"enter the amount of {face} RUB-coins/banknotes again (positive integer)");
                                _ = Console.ReadLine();
                            } while (approval.ToLower().Trim() == "2" || approval.ToLower().Trim() == "n" || approval.ToLower().Trim() == "no");
                            return;
                    }
                }

                UserPurse = userPurse;
            }
        }
    }
}