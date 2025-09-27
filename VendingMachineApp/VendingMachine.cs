namespace VendingMachineApp;
public class VendingMachine
{
    public List<Product> AvailableProducts = []; // filled by Admin

    public HashSet<int> IdSet = []; // filled automatically

    public HashSet<string> NamingSet = []; // filled automatically

    public Dictionary<int, int> CashDesk = Coin.Faces.ToDictionary(k => k, k => 0); // filled automatically

    public VendingMachine()
    {
    }

    public string Payment(int id, int price, int amount)
    {
        int purchase_sum = price * amount;

        int user_summa = 0;
        Dictionary<int, int> purchase_dict = [];

        Console.WriteLine($"\nthe total sum of your purchase is {purchase_sum} RUB.\nplease, pay the amount using coins/banknotes of the following FACES: {string.Join(", ", Coin.Faces.OrderBy(f => f))} RUB.\n\nif you want to cancel the purchase, enter 'CANCEL'");
        Console.WriteLine("\n\nyou can pay the amount in several steps, using coins/banknotes of different faces\nwhen finish, enter 'PAY'\n");
        Console.WriteLine("\nlet's start payment. enter the number of pieces of each face you want to use\nFORMAT EXAMPLE: 10, 5 (i.e. 10 RUB-coins/banknotes x 5 pieces)");

        while (true)
        {
            Console.WriteLine("\nenter the amount of the face you want to use (positive integer)\nif you want to cancel the purchase, enter 'CANCEL'\nif you have finished payment, enter 'PAY'\n\nFORMAT EXAMPLE: 10, 5 (i.e. 10 RUB-coins/banknotes x 5 pieces)\n\n");
            Console.WriteLine($"\n\navailable faces: {string.Join(", ", Coin.Faces.OrderBy(f => f))} RUB");
            string? num = Console.ReadLine();

            if (string.IsNullOrEmpty(num) || string.IsNullOrWhiteSpace(num))
            {
                Console.WriteLine("\ninvalid value. try again");
                continue;
            }

            else if (num.ToLower().Trim() == "pay")
            {
                if (user_summa < purchase_sum)
                {
                    Console.WriteLine($"\nthe amount you have paid – {user_summa} RUB – is less than the purchase sum {purchase_sum} RUB. please, pay the remaining amount");
                    continue;
                }
                else
                {
                    foreach (var entry in purchase_dict)
                    {
                        CashDesk[entry.Key] += entry.Value;
                    }
                    break;
                }
            }
            else if (num.ToLower().Trim() == "cancel" || num.ToLower().Trim() == "c" || num.ToLower().Trim() == "exit" || num.ToLower().Trim() == "quit" || num.ToLower().Trim() == "q" || num.ToLower().Trim() == "e")
            {
                RefundMoney(purchase_dict);
                return "cancelled";
            }
            else
            {
                var parts = num.Split(",");
                if (parts.Length != 2)
                {
                    Console.WriteLine("\nwrong format. try again. FORMAT EXAMPLE: 10, 5.");
                    continue;
                }
                if (!int.TryParse(parts[0].Trim(), out int face) || !int.TryParse(parts[1].Trim(), out int pieces) || pieces < 0 || !Coin.Faces.Contains(face))
                {
                    Console.WriteLine($"\nwrong format. try again. FORMAT EXAMPLE: 10, 5.\navailable faces: {string.Join(", ", Coin.Faces.OrderBy(f => f))} RUB");
                    continue;
                }
                user_summa += int.Parse(parts[0]) * int.Parse(parts[1]);
                Console.WriteLine($"\nthe REMAINING amount to pay is {purchase_sum - user_summa} RUB\nDEPOSITED amount is {user_summa} RUB");
                purchase_dict[face] = purchase_dict.GetValueOrDefault(face, 0) + pieces;
            }
        }
        if (user_summa > purchase_sum)
        {
            int user_change = user_summa - purchase_sum;
            string result = GiveCharge(user_change, purchase_dict);
            if (result == "change")
            {
                Product innerProduct = AvailableProducts.First(prod => prod.Id == id); // no default, checked existance of id before payment
                innerProduct.Quantity -= amount;
                Console.WriteLine($"\nyou have successfully bought {amount} pieces of {innerProduct.Name} (id {innerProduct.Id}). enjoy your product(s)!");
                return "success";
            }
            else
            {
                return "refund";
            }
        }
        else
        {
            Console.WriteLine("\nthank you for exact payment");
            Product foundProduct = AvailableProducts.First(prod => prod.Id == id); // no default, checked existance of id before payment
            foundProduct.Quantity -= amount;
            Console.WriteLine($"\nyou have successfully bought {amount} piece(s) of {foundProduct.Name} (id {foundProduct.Id}). enjoy your product(s)!");
            return "success";
        }
    }

    public string GiveCharge(int change, Dictionary<int, int> money_dict)
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
            RefundMoney(money_dict);
            Console.WriteLine($"\n\nsorry, there is NOT ENOUGH CHANGE in the machine/there is enough change but there is NO POSSIBILITY to give you precisely {change} RUB. your money has been refunded");
            return "refund";
        }
        else
        {
            Console.WriteLine($"\n\nhere is YOUR CHANGE of {change} RUB, given as follows:");
            foreach (var entry in change_dict)
            {
                Console.WriteLine($"{entry.Key}-coin/banknotes x {entry.Value} pieces");
                CashDesk[entry.Key] -= entry.Value;
            }
            Console.WriteLine($"\nyour change {change} RUB has been given");
            return "change";
        }
    }

    public static void RefundMoney(Dictionary<int, int> user_money)
    {
        Console.WriteLine("\n\nhere is the money you have deposited:");
        foreach (var entry in user_money)
        {
            Console.WriteLine($"{entry.Key}-coin/banknotes x {entry.Value} pieces");
        }
        Console.WriteLine("\nyour money HAS BEEN REFUNDED");
    }

    public void Purchase()
    {
        while (true)
        {
            if (AvailableProducts.All(prod => !prod.InStock))
            {
                Console.WriteLine("\nsorry, there are NO products IN STOCK. you cannot make a purchase now.");
                return;
            }

            Console.WriteLine("\nhere is the list of AVAILABLE products:\n");

            foreach (Product prod in AvailableProducts)
            {
                if (prod.ConsumerInfoOutput() is not null)
                {
                    Console.WriteLine($"{prod.ConsumerInfoOutput()}");
                }
            }

            Console.WriteLine($"\navailable product ids: {string.Join(", ", IdSet)}");
            Console.WriteLine("\nenter product id and the amount you want to buy.\nFORMAT EXAMPLE: 10, 3 (id 10, pieces 3)\nif you want to exit purchasing, enter 'CANCEL'");
            string? desire = Console.ReadLine();

            if (string.IsNullOrEmpty(desire) || string.IsNullOrWhiteSpace(desire))
            {
                Console.WriteLine("wrong format. try again. FORMAT EXAMPLE: 123, 5.");
                desire = Console.ReadLine();
            }

            else if (desire.ToLower().Trim() == "exit" || desire.ToLower().Trim() == "quit" || desire.ToLower().Trim() == "q" || desire.ToLower().Trim() == "e" || desire.ToLower().Trim() == "cancel" || desire.ToLower().Trim() == "c")
            {
                Console.WriteLine("exiting purchasing....");
                return;
            }
            else
            {
                var parts = desire.Split(",");
                if (parts.Length != 2)
                {
                    Console.WriteLine("wrong format. try again. FORMAT EXAMPLE: 123, 5.");
                    continue;
                }

                if (!int.TryParse(parts[0].Trim(), out int id) || !int.TryParse(parts[1].Trim(), out int amount) || amount <= 0)
                {
                    Console.WriteLine("wrong format. try again. FORMAT EXAMPLE: 123, 5.");
                    continue;
                }

                Product? foundProduct = AvailableProducts.FirstOrDefault(prod => prod.Id == id && prod.Quantity >= amount);

                if (foundProduct is null)
                {
                    Console.WriteLine($"wrong id. choose one of the list {IdSet}");
                    continue;
                }

                int prod_price = foundProduct.Price;
                Console.WriteLine($"your PURCHASE: {foundProduct.Name} (id {foundProduct.Id}), price {foundProduct.Price} RUB, amount - {amount} piece(s).");
                var res = Payment(id, prod_price, amount);
                if (res == "refund" || res == "no_money" || res == "cancelled")
                {
                    continue;
                }
            }
        }
    }

    public void UserScenario(User consumer)
    {
        Console.WriteLine("\nhere is the list of available products:\n");

        foreach (Product prod in AvailableProducts)
        {
            if (prod.ConsumerInfoOutput() is not null)
            {
                Console.WriteLine($"{prod.ConsumerInfoOutput()}");
            }
        }

        if (AvailableProducts.All(prod => !prod.InStock))
        {
            Console.WriteLine("\nsorry, there are NO products IN STOCK. you cannot make a purchase now.");
            Console.WriteLine("\nwant to change role? (yes/no&exit)");
            string? ans = Console.ReadLine();

            while (string.IsNullOrEmpty(ans) || string.IsNullOrWhiteSpace(ans))
            {
                Console.WriteLine("entered answer cannot be empty. try again");
                ans = Console.ReadLine();
            }
            switch (ans.ToLower().Trim())
            {
                case "yes":
                case "y":
                case "1":
                    Console.WriteLine("\nchanging role to admin...."); // gap closure
                    AdminScenario();
                    return;

                case "no&exit":
                case "no":
                case "n":
                case "2":
                    Console.WriteLine("\ngoodbye!");
                    ShutDown();
                    return;

                default:
                    Console.WriteLine("wrong input. try again");
                    UserScenario(consumer);
                    return;
            }
        }
        
        Console.WriteLine("\nwant to buy something? (yes/no)");
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
                Purchase();
                UserScenario(consumer);
                return;

            case "no":
            case "n":
            case "2":
                Console.WriteLine("\nwant to change role? (yes/no&exit)");
                string? ans = Console.ReadLine();

                while (string.IsNullOrEmpty(ans) || string.IsNullOrWhiteSpace(ans))
                {
                    Console.WriteLine("entered answer cannot be empty. try again");
                    ans = Console.ReadLine();
                }
                switch (ans.ToLower().Trim())
                {
                    case "yes":
                    case "y":
                    case "1":
                        Console.WriteLine("\nchanging role to admin...."); // gap closure
                        AdminScenario();
                        return;

                    case "no&exit":
                    case "no":
                    case "n":
                    case "2":
                        Console.WriteLine("\ngoodbye!");
                        ShutDown();
                        return;

                    default:
                        Console.WriteLine("wrong input. try again");
                        UserScenario(consumer);
                        return;
                }
            default:
                Console.WriteLine("wrong input. try again");
                UserScenario(consumer);
                return;
        }
    }

    public void ViewCashDesk()
    {
        Console.WriteLine("\nhere is the CONTENT of the cash desk:\n");
        foreach (var entry in CashDesk)
        {
            Console.WriteLine($"{entry.Key}-coin/banknotes x {entry.Value} pieces");
        }
    }

    public void AddNewPositions()
    {
        Console.WriteLine("\nhow many product types do you want to add? enter an integer value.");
        string? products_temp = Console.ReadLine();

        int prod_num;

        while (!int.TryParse(products_temp, out prod_num) || prod_num <= 0)
        {
            Console.WriteLine("\nyou must enter a positive integer value");
            products_temp = Console.ReadLine();
        }

        for (int i = 0; i < prod_num; i++)
        {
            Console.WriteLine("\nenter product id (int), product name (string), product price (int) and product quantity (int), separate with comma.\nformat example: 1, water 'saint spring', 50, 100.");
            string? input_prod = Console.ReadLine();

            if (string.IsNullOrEmpty(input_prod) || string.IsNullOrWhiteSpace(input_prod))
            {
                Console.WriteLine("\nenter product data in a correct way");
                i--;
                continue;
            }

            var parts = input_prod.Split(",");

            if (parts.Length != 4)
            {
                Console.WriteLine("\nenter product data in a correct way");
                i--;
                continue;
            }

            string name = parts[1].Trim();

            if (!int.TryParse(parts[0].Trim(), out int id) || !int.TryParse(parts[2].Trim(), out int price) || price < 0 || !int.TryParse(parts[3].Trim(), out int quantity) || quantity <= 0)
            {
                Console.WriteLine("\nenter product data in a correct way");
                i--;
                continue;
            }

            if (!IdSet.Contains(id) && !NamingSet.Contains(name)) // if id and name are new
            {
                Product product_exemp = new(id, name, price, quantity);
                AvailableProducts.Add(product_exemp);
                IdSet.Add(id);
                NamingSet.Add(name);
                Console.WriteLine($"\nyou added a new product: {product_exemp.AdminInfoOutput()}");
            }
            else if (!IdSet.Contains(id) && NamingSet.Contains(name)) // if id is new but name exists
            {
                Console.WriteLine($"\nproduct with name '{name}' already exists. change the parameters and try again.");
                i--;
                continue;
            }
            else if (IdSet.Contains(id) && !NamingSet.Contains(name)) // if id exists but name is new
            {
                Console.WriteLine($"\nproduct with id {id} already exists. change the parameters and try again.");
                i--;
                continue;
            }
            else if (IdSet.Contains(id) && NamingSet.Contains(name)) // if both id and name exist
            {
                Product existingProduct = AvailableProducts.First(product => product.Id == id); // since the product is already in the AvailableProducts, id in IdSet
                Console.WriteLine($"\nid {id} already exists: {existingProduct.AdminInfoOutput()}. change the parameters and try again.");
                i--;
                continue;
            }
        }
    }

    public void RefillProducts()
    {
        Console.WriteLine("\nhow many types of products do you want to refill? enter an integer value.");
        string? str_types = Console.ReadLine();

        int types_num;

        while (!int.TryParse(str_types, out types_num) || types_num <= 0)
        {
            Console.WriteLine("\ntypes number must be a positive integer. try again");
            str_types = Console.ReadLine();
        }

        for (int i = 0; i < types_num; i++)
        {
            Console.WriteLine("\nenter prodict id and the amount of it you want to refill in the format of {ProductId}, {ProductAmount} (without curly braces, just 2 intengers separsted by a comma)");
            string? prod_data = Console.ReadLine();

            bool success = false;

            while (!success)
            {
                if (string.IsNullOrEmpty(prod_data) || string.IsNullOrWhiteSpace(prod_data))
                {
                    Console.WriteLine("\nenter product data in a correct way");
                    i--;
                    continue;
                }

                var parts = prod_data.Split(",");
                if (parts.Length != 2)
                {
                    Console.WriteLine("\nwrong format. use: ProductId, ProductAmount. try again:");
                    i--;
                    continue;
                }

                if (!int.TryParse(parts[0].Trim(), out int id) || !int.TryParse(parts[1].Trim(), out int quantity) || quantity <= 0)
                {
                    Console.WriteLine("\nwrong format. id must exist and amount must be positive. try again");
                    i--;
                    continue;
                }

                Product? foundProduct = AvailableProducts.FirstOrDefault(product => product.Id == id);

                if (foundProduct is null)
                {
                    Console.WriteLine($"\nthere is no product with id {id}. try entering the data again");
                    i--;
                    continue;
                }

                foundProduct.Quantity += quantity;
                Console.WriteLine($"\nyou refilled {foundProduct.Name} (id {foundProduct.Id}) by {quantity} pieces. now the amount is {foundProduct.Quantity}");
                success = true;
            }
        }
    }

    public void RefillAddProducts() // ststic is suggested by VSCode
    {
        if (AvailableProducts.Count == 0)
        {
            Console.WriteLine("\nthere are no products added yet.");
            AddNewPositions();
        }
        else
        {
            Console.WriteLine("\nhere are the list of available products:\n");
            foreach (Product product in AvailableProducts.Cast<Product>())
            {
                string? info = product.AdminInfoOutput();
                if (info is not null)
                    Console.WriteLine(info);
            }
            Console.WriteLine("\nchoose an option to do:\n1. refill existing products\n2. add new product\n3. exit to main menu");

            string? option;
            do
            {
                option = Console.ReadLine();
                if (string.IsNullOrEmpty(option) || string.IsNullOrWhiteSpace(option))
                {
                    Console.WriteLine("\nchoose the task to do, input cannot be empty.");
                }
            } while (string.IsNullOrEmpty(option) || string.IsNullOrWhiteSpace(option));

            switch (option.ToLower().Trim())
            {
                case "1. refill existing products":
                case "refill existing products":
                case "refill products":
                case "refill":
                case "1":
                    Console.WriteLine("\nrefilling products....");
                    RefillProducts();
                    Admin.ChooseTask(this);
                    return;

                case "2. add new product":
                case "add new product":
                case "add product":
                case "add new":
                case "add":
                case "2":
                    Console.WriteLine("\nadding new products....");
                    AddNewPositions();
                    Admin.ChooseTask(this);
                    return;

                case "3. exit to main menu":
                case "exit to main menu":
                case "exit":
                case "3":
                    Console.WriteLine("\nexiting to main menu....");
                    Admin.ChooseTask(this);
                    return;

                default:
                    Console.WriteLine("\ninvalid option. please choose 1 or 2.");
                    RefillAddProducts();
                    return;
            }
        }
    }

    public void CollectMoney()
    {
        var cashDesk = CashDesk;
        int summa = 0;

        foreach (var entry in cashDesk)
        {
            summa += entry.Key * entry.Value;
            Console.WriteLine($"\n{entry.Key}-coin/banknotes x {entry.Value} pieces");
        }

        Console.WriteLine($"\n{summa} RUB collected");

        foreach (int face in Coin.Faces)
        {
            cashDesk[face] = 0;
        }

        Console.WriteLine("\ncash desk is empty now");
    }

    public void ChangeRoleCheck()
    {
        if (AvailableProducts.Count == 0 || CashDesk.Values.All(v => v == 0))
        {
            if (AvailableProducts.Count == 0 && !CashDesk.Values.All(v => v == 0))
            {
                Console.WriteLine("\nthere are no products added yet.\nyou need to add products before changing the role.");
                AddNewPositions();
            }
            else if (AvailableProducts.Count != 0 && CashDesk.Values.All(v => v == 0))
            {
                Console.WriteLine("\nthe cash desk is empty, there is no money to give change from.\nyou need to fill the cash desk before changing the role.");
                FillCashDesk();
            }
            else
            {
                Console.WriteLine("\nno products & money yet. cannot change the role.");
                AddNewPositions();
                FillCashDesk();
            }
        }
    }

    public void AdminScenario()
    {
        Console.WriteLine("\nenter admin password below");
        string? password = Console.ReadLine();

        if (string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(password))
        {
            Console.WriteLine("\nadmin password cannot be empty.");
            return;
        }
        try
        {
            var admin = new Admin(password, this);

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
        if (AvailableProducts.Count == 0)
        {
            Console.WriteLine("\nthere are no products added yet. you need to login as admin to add new products.");
            AdminScenario();
            MachineStart();
            return;
        }
        else
        {
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
                    User user = new();
                    UserScenario(user);
                    return;

                case "admin":
                case "2":
                    AdminScenario();
                    return;

                default:
                    Console.WriteLine("\nwrong role. try again");
                    MachineStart();
                    return;
            }
        }
    }

    public void FillCashDesk()
    {

        while(true)
        {
            Console.WriteLine("\nenter the amount of the face you want to add (positive integer)\nif you filled the cash desk, enter 'DONE'\n\nFORMAT EXAMPLE: 10, 5 (i.e. 10 RUB-coins/banknotes x 5 pieces)\n\n");
            Console.WriteLine($"\navailable faces: {string.Join(", ", Coin.Faces.OrderBy(f => f))} RUB");
            string? val = Console.ReadLine();

            if (string.IsNullOrEmpty(val) || string.IsNullOrWhiteSpace(val))
            {
                Console.WriteLine("\ninvalid value. try again");
                continue;
            }

            else if (val.ToLower().Trim() == "done" || val.ToLower().Trim() == "finish" || val.ToLower().Trim() == "f" || val.ToLower().Trim() == "d") break;

            var parts = val.Split(",");
            if (parts.Length != 2)
            {
                Console.WriteLine("\nWRONG FORMAT. try again. FORMAT EXAMPLE: 10, 5 (10 RUB-coin, 5 pieces).");
                continue;
            }

            if (!int.TryParse(parts[0].Trim(), out int face) || !int.TryParse(parts[1].Trim(), out int pieces) || pieces < 0 || !Coin.Faces.Contains(face))
            {
                Console.WriteLine($"\nWRONG FORMAT. try again. FORMAT EXAMPLE: 10, 5 (positive integers, valid faces; 10 RUB-coin, 5 pieces).\navailable faces: {string.Join(", ", Coin.Faces.OrderBy(f => f))} RUB");
                continue;
            }
            CashDesk[face] += pieces;
        }

        Console.WriteLine("\n\ncash desk has been successfully filled. current content:");
        foreach (var entry in CashDesk)
        {
            Console.WriteLine($"{entry.Key}-coin/banknotes x {entry.Value} pieces");
        }
    }
}
