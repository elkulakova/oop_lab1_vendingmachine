using System.Diagnostics.Metrics;
using System.Globalization;
using System.Numerics;

public class VendingMachine
{
    public List<Product> AvailableProducts = []; // filled by Admin

    public HashSet<int> IdSet = []; // filled by Admin

    public Dictionary<int, int> CashDesk = Coin.Faces.ToDictionary(k => k, k => 0); // filled automatically

    public VendingMachine()
    {
    }

    public void Payment(int id, int price, int amount, Dictionary<int, int> user_purse)
    {
        int purchase_sum = price * amount;

        int user_summa = 0;

        foreach (var entry in user_purse)
        {
            user_summa += entry.Key * entry.Value;
        }

        if (user_summa >= purchase_sum)
        {
            Console.WriteLine($"\nyou need to pay {purchase_sum} RUB.\nnow you need to enter amount of money you want to pay");

            int topay = 0;
            Dictionary<int, int> purchase_dict = [];
            bool paid = false;

            while (!paid)
            {
                foreach (var entry in user_purse.Where(e => e.Value > 0).ToList())
                {
                    if (topay >= purchase_sum)
                    {
                        paid = true;
                        break;
                    }
                    else
                    {
                        Console.WriteLine($"\nenter the amount of {entry.Key} RUB-coins/banknotes you want to use ({entry.Value} available; positive integer)");
                        string? pieces = Console.ReadLine();

                        int num;
                        while (!int.TryParse(pieces, out num) || num < 0 || num > entry.Value)
                        {
                            Console.WriteLine("\ninvalid value. try again");
                            pieces = Console.ReadLine();

                            while (string.IsNullOrEmpty(pieces) || string.IsNullOrWhiteSpace(pieces))
                            {
                                Console.WriteLine("\ninvalid value. try again");
                                pieces = Console.ReadLine();
                            }
                        }

                        if (num > 0)
                        {
                            topay += entry.Key * num;
                            user_purse[entry.Key] -= num;

                            CashDesk[entry.Key] += num;
                            purchase_dict.Add(entry.Key, num);
                            Console.WriteLine($"\n{topay} deposited");
                        }
                    }
                }
            }

            if (topay > purchase_sum)
            {
                int user_change = topay - purchase_sum;
                string result = GiveCharge(user_change, user_purse, purchase_dict);
                if (result == "change")
                {
                    Product foundProduct = AvailableProducts.First(prod => prod.Id == id); // no default, checked existance of id before payment
                    foundProduct.Quantity -= amount;
                    Console.WriteLine($"\nyou have successfully bought {amount} pieces of {foundProduct.Name} (id {foundProduct.Id}). enjoy your product(s)!");
                }
            }

            if (topay == purchase_sum)
            {
                Console.WriteLine("\nthank you for exact payment");
            }
        }
        else
        {
            Console.WriteLine("sorry, you don't have enough money. goodbye");
        }
    }

    public string GiveCharge(int change, Dictionary<int, int> user_purse, Dictionary<int, int> money_dict)
    {
        Dictionary<int, int> change_dict = [];
        int remaining = change;

        foreach (int face in Coin.Faces.OrderByDescending(f => f))
        {
            if (remaining == 0) break;

            int needed = remaining / face;

            if (needed > 0)
            {
                int available = CashDesk.TryGetValue(face, out int value) ? value : 0;

                if (available >= needed)
                {
                    change_dict[face] = needed;
                    remaining -= needed * face;
                }
                else
                {
                    change_dict[face] = available;
                    remaining -= available * face;
                }
            }
        }
        if (remaining > 0)
        {
            RefundMoney(money_dict, user_purse);
            Console.WriteLine("\nsorry, there is no enough charge in the machine. your money has been refund. goodbye");
            return "refund";
        }
        else
        {
            foreach (var entry in change_dict)
            {
                user_purse[entry.Key] += entry.Value;
                CashDesk[entry.Key] -= entry.Value;
            }
            Console.WriteLine($"your change {change} RUB has been given");
            return "change";
        }
    }

    public static void RefundMoney(Dictionary<int, int> user_money, Dictionary<int, int> user_purse)
    {
        foreach (var entry in user_money)
        {
            user_purse[entry.Key] += entry.Value;
        }
    }

    public void Purchase(Dictionary<int, int> user_purse)
    {
        Console.WriteLine("\nhow many items of different types do you want to buy? enter an integer value");
        string? items_num_str = Console.ReadLine();

        int items_num;

        while (!int.TryParse(items_num_str, out items_num) || items_num <= 0)
        {
            Console.WriteLine("number of different items must be a positive integer. try again.");
            items_num_str = Console.ReadLine();
        }
        for (int i = 0; i < items_num; i++)
        {
            Console.WriteLine("enter product id and the amount you want to buy. format: {ProductId}, {ProductAmount} (int, int; i.e. 123, 5)");
            string? desire = Console.ReadLine();

            if (string.IsNullOrEmpty(desire) || string.IsNullOrWhiteSpace(desire))
            {
                Console.WriteLine("wrong format. try again. format example: 123, 5.");
                i--;
                continue;
            }
            var parts = desire.Split(", ");
            if (parts.Length != 2)
            {
                Console.WriteLine("wrong format. try again. format example: 123, 5.");
                i--;
                continue;
            }

            if (!int.TryParse(parts[0], out int id) || !int.TryParse(parts[1], out int amount) || amount <= 0)
            {
                Console.WriteLine("wrong format. try again. format example: 123, 5.");
                i--;
                continue;
            }

            Product? foundProduct = AvailableProducts.FirstOrDefault(prod => prod.Id == id && prod.Quantity >= amount);

            if (foundProduct is null)
            {
                Console.WriteLine($"wrong id. choose one of the list {IdSet}");
                i--;
                continue;
            }

            int prod_price = foundProduct.Price;
            Console.WriteLine($"your {i + 1}/{items_num} product: {foundProduct.Name} (id {foundProduct.Id}), price {foundProduct.Price} RUB, amount - {amount} pieces.");
            Payment(id, prod_price, amount, user_purse);
        }
    }

    public void UserScenario()
    {
        User consumer = new();

        Console.WriteLine("\nhere is the list of available products:");

        foreach (Product prod in AvailableProducts)
        {
            if (prod.ConsumerInfoOutput() is not null)
            {
                Console.WriteLine($"\n{prod.ConsumerInfoOutput()}");
            }
        }

        Console.WriteLine("\nwant to buy something? (yes/no&exit)");
        string? answer = Console.ReadLine();

        while (string.IsNullOrEmpty(answer) || string.IsNullOrWhiteSpace(answer))
        {
            Console.WriteLine("entered answer cannot be empty. try again");
            answer = Console.ReadLine();
        }

        switch (answer.ToLower().Trim())
        {
            case "yes":
            case "y":
            case "1":
                Purchase(consumer.UserPurse);
                return;

            case "no":
            case "n":
            case "2":
            case "exit":
            case "e":
            case "3":
            case "no&exit":
                Console.WriteLine("\ngoodbye!");
                ShutDown();
                return;
        }
    }

    public void AdminScenario()
    {
        Console.WriteLine("\nenter admin password below");
        string? password = Console.ReadLine();

        if (string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(password))
        {
            Console.WriteLine("admin password cannot be empty.");
            return;
        }
            //throw new ArgumentException("admin password cannot be empty");
        try
        {
            var admin = new Admin(password);

            Admin.ChooseTask(this);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"{ex.Message}");
        }
    }

    public static void ShutDown()
    {
        Environment.Exit(0);
    }

    public void MachineStart()
    {
        Console.WriteLine("\nhello! you've started a Vending Machine");
        Console.WriteLine("\nchoose your role: user or admin");
        string? role = Console.ReadLine();

        while (string.IsNullOrEmpty(role) || string.IsNullOrWhiteSpace(role))
        {
            Console.WriteLine("\nentered role cannot be empty. try again");
            role = Console.ReadLine();
        }

        switch (role.ToLower().Trim())
        {
            case "user":
            case "1":
                Console.WriteLine("\nhello, user! ready to choose?");
                UserScenario();
                return;

            case "admin":
            case "2":
                AdminScenario();
                return;

            default:
                Console.WriteLine("\nwrong role. try again");
                MachineStart();
                return;
                //throw new ArgumentException("wrong role. try again"); --- IGNORE ---
        }
    }
}