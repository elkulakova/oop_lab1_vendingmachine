using System.ComponentModel;
using System.Security;

public class VendingMachine
{
    public List<Product> AvailableProducts = []; // filled be Admin

    public HashSet<int> IdSet = []; // filled be Admin

    public Dictionary<int, int> CashDesk = Coin.Faces.ToDictionary(k => k, k => 0); // filled automatically

    public VendingMachine()
    {
    }

    public void MachineStart()
    {
        Console.WriteLine("\nhello! you've started a Vending Machine");
        Console.WriteLine("\nchoose your role: user or admin");
        string? role = Console.ReadLine();

        if (!string.IsNullOrEmpty(role) && !string.IsNullOrWhiteSpace(role))
        {
            switch (role.ToLower().Trim())
            {
                case "user":
                case "1":
                    Console.WriteLine("\nhello, user! ready to choose?");
                    UserScenario();
                    return;

                case "admin":
                case "2":
                    Console.WriteLine("\nenter admin password below");
                    string? password = Console.ReadLine();

                    try
                    {
                        var admin = new Admin(password);
                    }
                    catch
                    {

                    }
                    finally
                    {

                    }
                    return;

            }
        }
        else
        {
            do
            {
                Console.WriteLine("\nentered role cannot be empty. try again");
                _ = Console.ReadLine();
            } while (string.IsNullOrEmpty(role) || string.IsNullOrWhiteSpace(role));

        }
    }

    public void Payment()
    {

    }

    public void GiveCharge()
    {

    }

    public void RefundMoney()
    {

    }

    public void Purchase()
    {
        Console.WriteLine("\nhow many items of different types do you want to buy? enter an integer value");
        string? items_num_str = Console.ReadLine();

        if (int.TryParse(items_num_str, out int items_num))
        {
            switch (items_num)
            {
                case <= 0:
                    do
                    {
                        Console.WriteLine("entered value must. be a positive integer");
                        _ = Console.ReadLine();
                    } while (items_num <= 0);
                    break;

                default:
                    for (int i = 0; i < items_num; i++)
                    {
                        Console.WriteLine("enter product id and the amount you want to buy. format: {ProductId}, {ProductAmount} (int, int; without curly braces)");
                        string? desire = Console.ReadLine();

                        if (!string.IsNullOrEmpty(desire) && !string.IsNullOrWhiteSpace(desire))
                        {
                            var parts = desire.Split(", ");
                            if (parts.Length == 2)
                            {
                                if (int.TryParse(parts[0], out int id) && int.TryParse(parts[1], out int amount))
                                {
                                    Product? foundProduct = AvailableProducts.FirstOrDefault(prod => prod.Id == id && prod.Quantity >= amount);

                                    if (foundProduct != null)
                                    {
                                        Payment();
                                    }
                                }
                            }
                        }
                    }
                    break;
            }
        }

    }

    public void UserScenario()
    {
        Console.WriteLine("\nhere is the list of available products:");

        foreach (Product prod in AvailableProducts)
        {
            if (prod.ConsumerInfoOutput() != null)
            {
                Console.WriteLine($"\n{prod.ConsumerInfoOutput()}");
            }
        }

        Console.WriteLine("want to buy something? (yes/no)");
        string? answer = Console.ReadLine();

        if (!string.IsNullOrEmpty(answer) && !string.IsNullOrWhiteSpace(answer))
        {
            switch (answer.ToLower().Trim())
            {
                case "yes":
                case "y":
                case "1":
                    Purchase();
                    break;

                case "no":
                case "n":
                case "2":
                    break;
            }
        }
        else
        {
            throw new ArgumentException("entered answer cannot be empty. try again");
        }
    }


}