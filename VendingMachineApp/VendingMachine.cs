using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices.Marshalling;

namespace VendingMachineApp;
public class VendingMachine
{
    private bool isRunning = true;
    private readonly List<Product> AvailableProducts = []; // filled by Admin
    private readonly HashSet<int> IdSet = []; // filled automatically
    private readonly HashSet<string> NamingSet = []; // filled automatically
    private static readonly HashSet<int> FacesSet = [1, 2, 5, 10, 50, 100, 200, 500, 1000, 2000, 5000]; // or new List<int>() {1, 2, 5, 10}, but VSCode suggested simplification
    private readonly HashSet<Coin> CashDesk = []; // Coin.Faces.ToDictionary(k => k, k => 0); // filled automatically or by Admin, so can be null when initializing the machine

    public VendingMachine()
    {
    }

    // user interaction methods
    public string Payment(int id, int price, int amount)
    {
        int purchase_sum = price * amount;

        int user_summa = 0;
        HashSet<Coin> purchase_list = [];

        Console.WriteLine($"\nthe total sum of your purchase is {purchase_sum} RUB.\nplease, pay the amount using coins/banknotes of the following FACES: {string.Join(", ", FacesSet.OrderBy(f => f))} RUB.\n\nif you want to cancel the purchase, enter 'CANCEL'");
        Console.WriteLine("\n\nyou can pay the amount in several steps, using coins/banknotes of different faces\nwhen finish, enter 'PAY'\n");
        Console.WriteLine("\nlet's start payment. enter the number of pieces of each face you want to use\nFORMAT EXAMPLE: 10, 5 (i.e. 10 RUB-coins/banknotes x 5 pieces)");

        while (true) // don't break until payment approval or cancellation, as in a real vending machine button 'pay'
        {
            if (user_summa <= purchase_sum)
            {
                Console.WriteLine($"\nthe REMAINING amount to pay is {purchase_sum - user_summa} RUB");
            }
            Console.WriteLine($"\nDEPOSITED amount is {user_summa} RUB");
            Console.WriteLine("\nenter the amount of the face you want to use (positive integer)\nif you want to cancel the purchase, enter 'CANCEL'\nif you have finished payment, enter 'PAY'\n\nFORMAT EXAMPLE: 10, 5 (i.e. 10 RUB-coins/banknotes x 5 pieces)\n\n");
            Console.WriteLine($"\n\navailable faces: {string.Join(", ", FacesSet.OrderBy(f => f))} RUB");
            string? num = Console.ReadLine();

            if (string.IsNullOrEmpty(num) || string.IsNullOrWhiteSpace(num))
            {
                Console.WriteLine("\ninvalid value. try again");
                continue;
            }

            else if (num.ToLower().Trim() == "pay" || num.ToLower().Trim() == "p" || num.ToLower().Trim() == "finish" || num.ToLower().Trim() == "f" || num.ToLower().Trim() == "done" || num.ToLower().Trim() == "d")
            {
                if (user_summa < purchase_sum)
                {
                    Console.WriteLine($"\nthe amount you have paid – {user_summa} RUB – is less than the purchase sum {purchase_sum} RUB. please, pay the remaining amount");
                    continue;
                }
                else
                {
                    break;
                }
            }
            else if (num.ToLower().Trim() == "cancel" || num.ToLower().Trim() == "c" || num.ToLower().Trim() == "exit" || num.ToLower().Trim() == "quit" || num.ToLower().Trim() == "q" || num.ToLower().Trim() == "e")
            {
                RefundMoney(purchase_list);
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
                if (!int.TryParse(parts[0].Trim(), out int face) || !int.TryParse(parts[1].Trim(), out int pieces) || pieces < 0 || !FacesSet.Contains(face))
                {
                    Console.WriteLine($"\nwrong format. try again. FORMAT EXAMPLE: 10, 5.\navailable faces: {string.Join(", ", FacesSet.OrderBy(f => f))} RUB");
                    continue;
                }
                user_summa += face * pieces;
                Coin? existingCoin = purchase_list.FirstOrDefault(coin => coin.Face == face);
                if (existingCoin != null)
                {
                    existingCoin.Amount += pieces;
                }
                else
                {
                    purchase_list.Add(new Coin(face, pieces));
                }
            }
        }
        if (user_summa > purchase_sum)
        {
            int user_change = user_summa - purchase_sum;
            string result = GiveChange(user_change, purchase_list);
            if (result == "change")
            {
                Product innerProduct = AvailableProducts.First(prod => prod.Id == id); // no default, checked existance of id before payment
                innerProduct.Quantity -= amount;
                Console.WriteLine($"\nyou have successfully bought {amount} piece(s) of {innerProduct.Name} (id {innerProduct.Id}). enjoy your product(s)!");
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

    public string GiveChange(int change, HashSet<Coin> money_list)
    {
        HashSet<Coin> temp_desk = [.. CashDesk.Select(coin => new Coin(coin.Face, coin.Amount))];

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

        HashSet<Coin> change_list = [];
        int remaining = change;

        foreach (int face in FacesSet.OrderByDescending(f => f))
        {
            if (remaining == 0) break;

            int needed = remaining / face;

            Coin? foundCoin = temp_desk.FirstOrDefault(coin => coin.Face == face && coin.Amount > 0);

            if (needed > 0 && foundCoin != null)
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
            RefundMoney(money_list);
            Console.WriteLine($"\n\nsorry, there is NOT ENOUGH CHANGE in the machine/there is enough change but there is NO POSSIBILITY to give you precisely {change} RUB. your money has been refunded");
            return "refund";
        }
        else
        {
            foreach (var entry in money_list)
            {
                Coin? existingCoin = CashDesk.FirstOrDefault(coin => coin.Face == entry.Face);
                if (existingCoin != null)
                {
                    existingCoin.Amount += entry.Amount;
                }
                else
                {
                    CashDesk.Add(new Coin(entry.Face, entry.Amount));
                }
            }

            Console.WriteLine($"\n\nhere is YOUR CHANGE given as follows:");
            foreach (Coin entry in change_list)
            {
                Console.WriteLine($"{entry.Face}-coin/banknotes x {entry.Amount} pieces");

                Coin foundCoin = CashDesk.First(coin => coin.Face == entry.Face);
                foundCoin.Amount -= entry.Amount;
            }
            Console.WriteLine($"\nyour change {change} RUB has been given");
            return "change";
        }
    }

    public static void RefundMoney(HashSet<Coin> user_money)
    {
        Console.WriteLine("\n\nhere is the money you have deposited:");
        foreach (var entry in user_money)
        {
            Console.WriteLine($"{entry.Face}-coin/banknotes x {entry.Amount} pieces");
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

            HashSet<int> availableIds = [];
            foreach (Product prod in AvailableProducts)
            {
                if (prod.InStock)
                {
                    availableIds.Add(prod.Id);
                }
            }
            Console.WriteLine($"\navailable products ids: {string.Join(", ", availableIds.OrderBy(id => id))}");
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
                    Console.WriteLine($"wrong id. choose one of the list {availableIds.OrderBy(i => i)} or the amount is more than in stock. try again.");
                    continue;
                }

                int prod_price = foundProduct.Price;
                Console.WriteLine($"your PURCHASE: {foundProduct.Name} (id {foundProduct.Id}), price {foundProduct.Price} RUB, amount - {amount} piece(s).");
                var res = Payment(id, prod_price, amount);
                if (res == "refund" || res == "cancelled")
                {
                    Console.WriteLine("let's try again!");
                    continue;
                }
                if (res == "success")
                {
                    return;
                }
            }
        }
    }

    // see smth methods
    public void ViewCashDesk()
    {
        Console.WriteLine("\nhere is the CONTENT of the cash desk:\n");
        foreach (var entry in CashDesk)
        {
            Console.WriteLine($"{entry.Face}-coin/banknotes x {entry.Amount} pieces");
        }
    }

    public void ProductsView()
    {
        Console.WriteLine("\nhere is the list of ALL products (in stock and out of stock):\n");
        if (AvailableProducts.Count == 0)
        {
            Console.WriteLine("there are NO products ADDED YET, nothing to show.");
        }
        foreach (Product prod in AvailableProducts)
        {
            Console.WriteLine($"{prod.AdminInfoOutput()}");
        }
    }

    public void StoreWindow()
    {
        Console.WriteLine("\nhere is the list of available products:\n");

        foreach (Product prod in AvailableProducts)
        {
            if (prod.ConsumerInfoOutput() is not null)
            {
                Console.WriteLine($"{prod.ConsumerInfoOutput()}");
            }
        }
    }

    // admin interaction methods
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
            Console.WriteLine("\nenter product id (POSITIVE int), product name (string), product price (POSITIVE int) and product quantity (POSITIVE int), separate with comma.\nFORMAT EXAMPLE: 1, water 'saint spring', 50, 100.");
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

            if (!int.TryParse(parts[0].Trim(), out int id) || id <= 0 || !int.TryParse(parts[2].Trim(), out int price) || price <= 0 || !int.TryParse(parts[3].Trim(), out int quantity) || quantity <= 0)
            {
                Console.WriteLine("\nenter product data in a correct way");
                i--;
                continue;
            }

            if (!IdSet.Contains(id) && !NamingSet.Contains(name))
            {
                Product product_exemp = new(id, name, price, quantity);
                AvailableProducts.Add(product_exemp);
                IdSet.Add(id);
                NamingSet.Add(name);
                Console.WriteLine($"\nyou added a new product: {product_exemp.AdminInfoOutput()}");
            }
            else if (!IdSet.Contains(id) && NamingSet.Contains(name))
            {
                Console.WriteLine($"\nproduct with name '{name}' already exists. change the parameters and try again.");
                i--;
                continue;
            }
            else if (IdSet.Contains(id) && !NamingSet.Contains(name))
            {
                Console.WriteLine($"\nproduct with id {id} already exists. change the parameters and try again.");
                i--;
                continue;
            }
            else if (IdSet.Contains(id) && NamingSet.Contains(name))
            {
                Product existingProductId = AvailableProducts.First(product => product.Id == id); // since the product is already in the AvailableProducts, id in IdSet
                Product existingProductName = AvailableProducts.First(product => product.Name == name);
                if (existingProductId == existingProductName)
                {
                    Console.WriteLine($"\nproduct {name} (id {id}) already exists: {existingProductId.AdminInfoOutput()}. change the parameters and try again.");
                    i--;
                    continue;
                }
                else
                {
                    Console.WriteLine($"\nTHERE ARE TWO DIFFERENT PRODUCTS\nid {id} already exists: {existingProductId.AdminInfoOutput()}.\nproduct {name} already exists: {existingProductName.AdminInfoOutput()}.\nchange the parameters and try again.");
                    i--;
                    continue;
                }
            }
        }
    }

    public void RefillProducts()
    {
        Console.WriteLine("\nhow many different types of products do you want to refill? enter an integer value.");
        string? str_types = Console.ReadLine();

        int types_num;

        while (!int.TryParse(str_types, out types_num) || types_num <= 0)
        {
            Console.WriteLine("\ntypes number must be a positive integer. try again");
            str_types = Console.ReadLine();
        }

        for (int i = 0; i < types_num; i++)
        {
            Console.WriteLine("\nenter product id and the amount of it you want to refill in the format of {ProductId}, {ProductAmount}\nFORMAT EXAMPLE: 56, 7 (id 56, 7 pieces)");
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
                    Console.WriteLine("\nwrong format. use: ProductId, ProductAmount.\nFORMAT EXAMPLE: 56, 7 (id 56, 7 pieces). try again:");
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

    public void RefillAddProducts()
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
                    return;

                case "2. add new product":
                case "add new product":
                case "add product":
                case "add new":
                case "add":
                case "2":
                    Console.WriteLine("\nadding new products....");
                    AddNewPositions();
                    return;

                case "3. exit to main menu":
                case "exit to main menu":
                case "exit":
                case "3":
                    Console.WriteLine("\nexiting to main menu....");
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
        int summa = 0;

        foreach (var entry in CashDesk)
        {
            summa += entry.Face * entry.Amount;
            Console.WriteLine($"{entry.Face}-coin/banknotes x {entry.Amount} pieces");
        }

        Console.WriteLine($"\n{summa} RUB collected");

        foreach (Coin coin in CashDesk)
        {
            coin.Amount = 0;
        }

        Console.WriteLine("\ncash desk is empty now");
    }

    public void FillCashDesk()
    {

        while(true)
        {
            Console.WriteLine("\nenter the amount of the face you want to add (positive integer)\nif you filled the cash desk, enter 'DONE'\n\nFORMAT EXAMPLE: 10, 5 (i.e. 10 RUB-coins/banknotes x 5 pieces)\n");
            Console.WriteLine($"\navailable faces: {string.Join(", ", FacesSet.OrderBy(f => f))} RUB");
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
                Console.WriteLine("\nWRONG FORMAT. try again.\nFORMAT EXAMPLE: 10, 5 (10 RUB-coin, 5 pieces).");
                continue;
            }

            if (!int.TryParse(parts[0].Trim(), out int face) || !int.TryParse(parts[1].Trim(), out int pieces) || pieces < 0 || !FacesSet.Contains(face))
            {
                Console.WriteLine($"\nWRONG FORMAT. try again.\nFORMAT EXAMPLE: 10, 5 (positive integers, valid faces; 10 RUB-coin, 5 pieces).\navailable faces: {string.Join(", ", FacesSet.OrderBy(f => f))} RUB");
                continue;
            }

            Coin? existingCoin = CashDesk.FirstOrDefault(coin => coin.Face == face);

            if (existingCoin != null)
            {
                existingCoin.Amount += pieces;
            }
            else
            {
                CashDesk.Add(new Coin(face, pieces));
            }
        }

        Console.WriteLine("\n\ncash desk has been successfully filled. current content:");
        foreach (Coin entry in CashDesk)
        {
            Console.WriteLine($"{entry.Face}-coin/banknotes x {entry.Amount} pieces");
        }
    }

    // checking
    public bool ChangeRoleCheck()
    {
        if (AvailableProducts.Count == 0 || CheckDesk())
        {
            if (AvailableProducts.Count == 0 && !CheckDesk())
            {
                Console.WriteLine("\nthere are no products added yet.\nyou need to add products before changing the role.");
                AddNewPositions();
                return false;
            }
            else if (AvailableProducts.Count != 0 && CheckDesk())
            {
                Console.WriteLine("\nthe cash desk is empty, there is no money to give change from.\nyou need to fill the cash desk before changing the role.");
                FillCashDesk();
                return false;
            }
            else
            {
                Console.WriteLine("\nno products & money yet. cannot change the role.");
                AddNewPositions();
                FillCashDesk();
                return false;
            }
        }
        return true;
    }

    public bool CheckDesk()
    {
        return CashDesk.All(a => a.Amount == 0) || CashDesk.Count == 0;
    }

    // scenarios
    public void UserScenario(User consumer) // only User can be in UserScenario, no IRoles then
    {
        if (AvailableProducts.All(prod => !prod.InStock))
        {
            Console.WriteLine("\nsorry, there are NO products IN STOCK. you cannot make a purchase now.");
            Console.WriteLine("\nwant to change the role? (yes/no&exit)");
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
                    Console.WriteLine("\nchanging role to admin....");
                    AdminScenario();
                    return;

                case "no&exit":
                case "no":
                case "n":
                case "2":
                    ShutDown();
                    return;

                default:
                    Console.WriteLine("wrong input. try again");
                    UserScenario(consumer);
                    return;
            }
        }
        consumer.ChooseTask();
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

        if (password.ToLower().Trim() == "admin_password")
        {
            try
            {
                var admin = new Admin(this);

                Console.WriteLine("\nyou are authorized as admin!");
                admin.ChooseTask();
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nerror occurred: {ex.Message}\nreturning to main menu....");
                MachineStart();
                return;
            }
        }
        else
        {
            Console.WriteLine("\nwrong password. returning to main menu....");
            MachineStart();
            return;
        }
    }

    private static bool FilledMachine()
    {
        Random rand = new();
        bool randomBool = rand.Next(0, 2) == 0; // will be true if 0, false if 1
        return randomBool;
    }
    public void MachineStart()
    {
        isRunning = true;

        if (FilledMachine())
        {
            // filling the cash desk with random amount of random coins/banknotes
            Random rand = new();
            foreach (int face in FacesSet)
            {
                int pieces = rand.Next(0, 11); // from 1 to 10 pieces of each face
                CashDesk.Add(new Coin(face, pieces));
            }

            // adding some products
            AvailableProducts.Add(new Product(1, "water 'saint spring'", 50, 100));
            AvailableProducts.Add(new Product(2, "greek salad", 100, 50));
            AvailableProducts.Add(new Product(3, "chiken karri sandwich", 120, 50));
            AvailableProducts.Add(new Product(4, "pancackes with marple syrup", 120, 100));
            AvailableProducts.Add(new Product(5, "nut&dried fruits mix", 100, 100));
            IdSet.UnionWith([1, 2, 3, 4, 5]);
            NamingSet.UnionWith(["water 'saint spring'", "greek salad", "chiken karri sandwich", "pancackes with marple syrup", "nut&dried fruits mix"]);

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
                    User user = new(this);
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
        else
        {
            Console.WriteLine("\nthere are no products added yet. you need to login as an admin to add new products.");
            AdminScenario();
            return;
        }
    }

    public void ShutDown()
    {
        if (!isRunning) // in order not to be able to shut the machine down in the main script without starting it first
            throw new InvalidOperationException("machine is not running. cannot shut down.");

        isRunning = false;
        Console.WriteLine("\nshutting down the vending machine...\ngoodbye!");
        Environment.Exit(0);
    }
}
