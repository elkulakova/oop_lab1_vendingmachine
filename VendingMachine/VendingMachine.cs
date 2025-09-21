public class VendingMachine
{
    public List<Product> AvailableProducts = []; // filled be Admin

    public HashSet<int> IdSet = []; // filled be Admin

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
                        Console.WriteLine($"enter the amount of {entry.Key} RUB-coins/banknotes you want to use ({entry.Value} available; positive integer)");
                        string? pieces = Console.ReadLine();

                        if (int.TryParse(pieces, out int num) && num > 0 && num <= entry.Value)
                        {
                            topay += entry.Key * num;
                            user_purse[entry.Key] -= num;
                            Console.WriteLine($"{topay} deposited");
                        }
                        //else
                        //{
                        //    do
                        //    {
                        //        Console.WriteLine($"invalid value. try again");
                        //        _ = Console.ReadLine();
                        //    } while (num > entry.Value);
                        //}
                    }
                }
            }
            Product? foundProduct = AvailableProducts.FirstOrDefault(prod => prod.Id == id);

            foundProduct!.Quantity -= amount;
        }
        else
        {
            Console.WriteLine("sorry, you don't have enough money. goodbye");
        }
    }

    public void GiveCharge()
    {

    }

    public void RefundMoney()
    {

    }

    public void Purchase(Dictionary<int, int> user_purse)
    {
        Console.WriteLine("\nhow many items of different types do you want to buy? enter an integer value");
        string? items_num_str = Console.ReadLine();

        if (int.TryParse(items_num_str, out int items_num))
        {
            switch (items_num)
            {
                case < 0:
                    do
                    {
                        Console.WriteLine("entered value must be a positive integer");
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
                                        int prod_price = foundProduct.Price;
                                        Payment(id, prod_price, amount, user_purse);
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
        User consumer = new User();

        Console.WriteLine("\nhere is the list of available products:");

        foreach (Product prod in AvailableProducts)
        {
            if (prod.ConsumerInfoOutput() != null)
            {
                Console.WriteLine($"\n{prod.ConsumerInfoOutput()}");
            }
        }

        Console.WriteLine("\nwant to buy something? (yes/no)");
        string? answer = Console.ReadLine();

        if (!string.IsNullOrEmpty(answer) && !string.IsNullOrWhiteSpace(answer))
        {
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
                    break;
            }
        }
        else
        {
            throw new ArgumentException("entered answer cannot be empty. try again");
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
                    AdminScenario(); // instead of machine cause we are in VendingMachine
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
}