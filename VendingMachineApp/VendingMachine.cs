using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices.Marshalling;

namespace VendingMachineApp;
public class VendingMachine
{
    private bool isRunning = true;
    private readonly FoodWarehouse foodWarehouse = new();
    private static readonly SortedSet<int> FacesSet = [1, 2, 5, 10, 50, 100, 200, 500, 1000, 2000, 5000]; // or new List<int>() {1, 2, 5, 10}, but VSCode suggested simplification
    private readonly CashDesk cashDesk = new(); // filled automatically or by Admin, so can be null when initializing the machine

    public VendingMachine()
    {
    }

    // user interaction methods
    private string Payment(int id, int price, int amount)
    {
        int purchase_sum = price * amount;

        int user_summa = 0;
        CashDesk purchase_list = new();

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

            if (string.IsNullOrWhiteSpace(num))
            {
                Console.WriteLine("\ninvalid value. try again");
                continue;
            }

            else if (num.ToLower().Trim() is "pay" or "p" or "finish" or "f" or "done" or "d")
            {
                if (user_summa < purchase_sum)
                {
                    Console.WriteLine($"\nthe amount you have paid ({user_summa} RUB) is less than the purchase sum {purchase_sum} RUB. please, pay the remaining amount");
                    continue;
                }
                else
                {
                    break;
                }
            }
            else if (num.ToLower().Trim() is "cancel" or "c" or "exit" or "quit" or "q" or "e")
            {
                RefundMoney(purchase_list);
                return "cancelled";
            }
            else
            {
                string[] parts = num.Split(',', StringSplitOptions.TrimEntries);
                if (parts.Length != 2)
                {
                    Console.WriteLine("\nwrong format. try again. FORMAT EXAMPLE: 10, 5.");
                    continue;
                }
                if (!int.TryParse(parts[0], out int face) || !int.TryParse(parts[1], out int pieces))
                {
                    Console.WriteLine($"\nwrong format. try again. FORMAT EXAMPLE: 10, 5.\navailable faces: {string.Join(", ", FacesSet.OrderBy(f => f))} RUB");
                    continue;
                }

                try
                {
                    purchase_list.AddCoin(face, pieces);
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"\nerror occurred: {ex.Message}\ntry again.");
                    continue;
                }
                user_summa += face * pieces;
            }
        }
        if (user_summa > purchase_sum)
        {
            int user_change = user_summa - purchase_sum;
            string result = cashDesk.GiveChange(user_change, purchase_list.GetCoinsSet());
            if (result == "change")
            {
                Product innerProduct = foodWarehouse.GetProducts().First(prod => prod.Id == id); // no default, checked existance of id before payment
                innerProduct.Quantity -= amount;
                Console.WriteLine($"\nyou have successfully bought {amount} piece(s) of {innerProduct.Name} (id {innerProduct.Id}). enjoy your product(s)!");
                return "success";
            }
            else
            {
                RefundMoney(purchase_list);
                return "refund";
            }
        }
        else
        {
            foreach (Coin coin in purchase_list.GetCoinsSet())
            {
                cashDesk.AddCoin(coin.Face, coin.Amount);
            }
            Console.WriteLine("\nthank you for exact payment");
            Product foundProduct = foodWarehouse.GetProducts().First(prod => prod.Id == id); // no default, checked existance of id before payment
            foundProduct.Quantity -= amount;
            Console.WriteLine($"\nyou have successfully bought {amount} piece(s) of {foundProduct.Name} (id {foundProduct.Id}). enjoy your product(s)!");
            return "success";
        }
    }

    private static void RefundMoney(CashDesk user_money)
    {
        Console.WriteLine("\n\nhere is the money you have deposited:");
        user_money.View();
        Console.WriteLine("\nyour money HAS BEEN REFUNDED");
    }

    public void Purchase()
    {
        while (true)
        {
            if (!foodWarehouse.HasAvailableProducts())
            {
                Console.WriteLine("\nsorry, there are NO products IN STOCK. you cannot make a purchase now.");
                return;
            }

            foodWarehouse.ViewProducts("user");

            HashSet<int> availableIds = foodWarehouse.GetAvailableProductIds();

            Console.WriteLine($"\navailable products ids: {string.Join(", ", availableIds.OrderBy(id => id))}");
            Console.WriteLine("\nenter product id and the amount you want to buy.\nFORMAT EXAMPLE: 10, 3 (id 10, pieces 3)\nif you want to exit purchasing, enter 'CANCEL'");

            string? desire;
            do
            {
                desire = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(desire))
                {
                    Console.WriteLine("entered value cannot be empty. try again. FORMAT EXAMPLE: 123, 5.");
                }
            } while (string.IsNullOrWhiteSpace(desire));

            if (desire.ToLower().Trim() is "exit" or "quit" or "q" or "e" or "cancel" or "c")
            {
                Console.WriteLine("exiting purchasing....");
                return;
            }
            else
            {
                string[] parts = desire.Split(',', StringSplitOptions.TrimEntries);
                if (parts.Length != 2)
                {
                    Console.WriteLine("wrong format. try again. FORMAT EXAMPLE: 123, 5.");
                    continue;
                }

                if (!int.TryParse(parts[0], out int id) || !int.TryParse(parts[1], out int amount) || amount <= 0)
                {
                    Console.WriteLine("wrong format. try again. FORMAT EXAMPLE: 123, 5.");
                    continue;
                }

                Product? foundProduct = foodWarehouse.GetProducts().FirstOrDefault(prod => prod.Id == id && prod.Quantity >= amount);

                if (foundProduct is null)
                {
                    Console.WriteLine($"wrong id or insufficient quantity. choose id one of the list {string.Join(", ", availableIds.OrderBy(id => id))}. try again.");
                    continue;
                }

                int prod_price = foundProduct.Price;
                Console.WriteLine($"your PURCHASE: {foundProduct.Name} (id {foundProduct.Id}), price {foundProduct.Price} RUB, amount - {amount} piece(s).");
                var res = Payment(id, prod_price, amount);
                if (res is "refund" or "cancelled")
                {
                    Console.WriteLine("let's try again!");
                    continue;
                }
                if (res is "success")
                {
                    return;
                }
            }
        }
    }

    // see smth methods
    public void ViewCashDesk()
    {
        cashDesk.View();
    }

    public void ProductsView()
    {
        foodWarehouse.ViewProducts("admin");
    }

    public void StoreWindow()
    {
        foodWarehouse.ViewProducts("user");
    }

    // admin interaction methods
    public void AddProducts()
    {
        Console.WriteLine("\nhow many products types do you want to add? enter an integer value.");

        int prod_num;
        string? products_temp;
        do
        {
            products_temp = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(products_temp) && int.TryParse(products_temp, out prod_num) && prod_num > 0)
            {
                break;
            }
            Console.WriteLine("\nyou must enter a positive integer value");
        } while (true);

        for (int i = 0; i < prod_num; i++)
        {
            bool success = false;

            while (!success)
            {
                Console.WriteLine("\nenter product id (POSITIVE int), product name (string), product price (POSITIVE int) and product quantity (POSITIVE int), separate with comma.\nFORMAT EXAMPLE: 1, water 'saint spring', 50, 100.");
                string? input_prod = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input_prod))
                {
                    Console.WriteLine("\nenter product data in a correct way");
                    continue;
                }

                string[] parts = input_prod.Split(',', StringSplitOptions.TrimEntries);

                if (parts.Length != 4)
                {
                    Console.WriteLine("\nenter product data in a correct way");
                    continue;
                }

                string name = parts[1];

                if (!int.TryParse(parts[0], out int id) || !int.TryParse(parts[2], out int price) || !int.TryParse(parts[3], out int quantity))
                {
                    Console.WriteLine("\nenter product data in a correct way");
                    continue;
                }

                try
                {
                    foodWarehouse.AddProduct(id, name, price, quantity);
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"\nerror occurred: {ex.Message}\ntry again.");
                    continue;
                }
                success = true;
            }
        }
    }
    public void RefillProducts()
    {
        Console.WriteLine("\nhow many different types of products do you want to refill? enter an integer value.");

        string? str_types;
        int types_num;
        do
        {
            str_types = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(str_types) && int.TryParse(str_types, out types_num) && types_num > 0)
            {
                break;
            }
            Console.WriteLine("\ntypes number must be a positive integer. try again");
        } while (true);

        for (int i = 0; i < types_num; i++)
        {
            bool success = false;

            while (!success)
            {
                Console.WriteLine("\nenter product id and the amount of it you want to refill in the format of {ProductId}, {ProductAmount}\nFORMAT EXAMPLE: 56, 7 (id 56, 7 pieces)");
                string? prod_data = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(prod_data))
                {
                    Console.WriteLine("\nenter product data in a correct way");
                    continue;
                }

                string[] parts = prod_data.Split(',', StringSplitOptions.TrimEntries);
                if (parts.Length != 2)
                {
                    Console.WriteLine("\nwrong format. use: ProductId, ProductAmount.\nFORMAT EXAMPLE: 56, 7 (id 56, 7 pieces). try again:");
                    continue;
                }

                if (!int.TryParse(parts[0], out int id) || !int.TryParse(parts[1], out int quantity))
                {
                    Console.WriteLine("\nwrong format. id must exist and amount must be positive. try again");
                    continue;
                }

                try
                {
                    foodWarehouse.RefillProduct(id, quantity);
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"\nerror occurred: {ex.Message}\ntry again.");
                    continue;
                }
                success = true;
            }
        }
    }
    public void RefillAddProducts()
    {
        if (!foodWarehouse.HasAvailableProducts())
        {
            Console.WriteLine("\nthere are no products added yet.");
            AddProducts();
        }
        else
        {
            while (true)
            {
                Console.WriteLine("\nhere are the list of available products:\n");
                foodWarehouse.ViewProducts("admin");
                Console.WriteLine("\nchoose an option to do:\n1. refill existing products\n2. add new product\n3. exit to main menu");

                string? option;
                do
                {
                    option = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(option))
                    {
                        Console.WriteLine("\nchoose the task to do, input cannot be empty.");
                    }
                } while (string.IsNullOrWhiteSpace(option));

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
                        AddProducts();
                        return;

                    case "3. exit to main menu":
                    case "exit to main menu":
                    case "exit":
                    case "3":
                        Console.WriteLine("\nexiting to main menu....");
                        return;

                    default:
                        Console.WriteLine("\ninvalid option. please choose 1 or 2.");
                        break;
                }
            }
        }
    }

    public void CollectMoney()
    {
        cashDesk.View();
        Console.WriteLine($"\n{cashDesk.TotalAmount} RUB collected");
        cashDesk.Clear();
    }

    public void FillCashDesk()
    {
        while(true)
        {
            Console.WriteLine("\nenter the amount of the face you want to add (positive integer)\nif you filled the cash desk, enter 'DONE'\n\nFORMAT EXAMPLE: 10, 5 (i.e. 10 RUB-coins/banknotes x 5 pieces)\n");
            Console.WriteLine($"\navailable faces: {string.Join(", ", FacesSet.OrderBy(f => f))} RUB");
            string? val = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(val))
            {
                Console.WriteLine("\ninvalid value. try again");
                continue;
            }

            else if (val.ToLower().Trim() is "done" or "finish" or "f" or "d") break;

            string[] parts = val.Split(',', StringSplitOptions.TrimEntries);
            if (parts.Length != 2)
            {
                Console.WriteLine("\nWRONG FORMAT. try again.\nFORMAT EXAMPLE: 10, 5 (10 RUB-coin, 5 pieces).");
                continue;
            }

            if (!int.TryParse(parts[0], out int face) || !int.TryParse(parts[1], out int pieces))
            {
                Console.WriteLine($"\nWRONG FORMAT. try again.\nFORMAT EXAMPLE: 10, 5 (positive integers, valid faces; 10 RUB-coin, 5 pieces).\navailable faces: {string.Join(", ", FacesSet.OrderBy(f => f))} RUB");
                continue;
            }

            try
            {
                cashDesk.AddCoin(face, pieces);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"\nerror occurred: {ex.Message}\ntry again.");
                continue;
            }
        }

        Console.WriteLine("\n\ncash desk has been successfully filled. current content:");
        cashDesk.View();
    }

    // checking
    public bool ChangeRoleCheck()
    {
        if (!foodWarehouse.HasAvailableProducts() || cashDesk.IsEmpty())
        {
            if (!foodWarehouse.HasAvailableProducts() && !cashDesk.IsEmpty())
            {
                Console.WriteLine("\nthere are no products added yet.\nyou need to add products before changing the role.");
                AddProducts();
                return false;
            }
            else if (foodWarehouse.HasAvailableProducts() && cashDesk.IsEmpty())
            {
                Console.WriteLine("\nthe cash desk is empty, there is no money to give change from.\nyou need to fill the cash desk before changing the role.");
                FillCashDesk();
                return false;
            }
            else
            {
                Console.WriteLine("\nno products & money yet. cannot change the role.");
                AddProducts();
                FillCashDesk();
                return false;
            }
        }
        return true;
    }

    public bool CheckDesk()
    {
        if (cashDesk.IsEmpty())
        {
            Console.WriteLine("\nthe cash desk is empty.");
            return true;
        }
        else
        {
            Console.WriteLine($"\nthe cash desk is NOT EMPTY, it contains {cashDesk.TotalAmount} RUB");
            return false;
        }
    }

    // scenarios
    public void UserScenario(User consumer) // only User can be in UserScenario, no IRoles then
    {
        if (!foodWarehouse.HasAvailableProducts())
        {
            while (true)
            {
                Console.WriteLine("\nsorry, there are NO products IN STOCK. you cannot make a purchase now.");
                Console.WriteLine("\nwant to change the role? (yes/no&exit)");

                string? ans = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(ans))
                {
                    Console.WriteLine("entered answer cannot be empty. try again");
                    continue;
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
                        break;
                }
            }
        }
        consumer.ChooseTask();
    }

    public void AdminScenario()
    {
        Console.WriteLine("\nenter admin password below");

        string? password;
        do
        {
            password = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("\nadmin password cannot be empty.");
            }
        } while (string.IsNullOrWhiteSpace(password));

        if (password.ToLower().Trim() is "admin_password")
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
                MachineStart("manual");
                return;
            }
        }
        else
        {
            Console.WriteLine("\nwrong password. returning to main menu....");
            MachineStart("manual");
            return;
        }
    }

    private static bool FilledMachine()
    {
        Random rand = new();
        bool randomBool = rand.Next(0, 2) == 0; // will be true if 0, false if 1
        return randomBool;
    }
    public void MachineStart(string pattern = "random") // pattern can be "random" or "manual"
    {
        isRunning = true;
        if (pattern is "random")
        {
            if (FilledMachine())
            {
                // filling the cash desk with random amount of random coins/banknotes
                cashDesk.Fill();
                // adding some products
                foodWarehouse.FillProducts([new Product(1, "water 'saint spring'", 50, 100), new Product(2, "greek salad", 100, 50), new Product(3, "chiken karri sandwich", 120, 50), new Product(4, "pancackes with maple syrup", 120, 100), new Product(5, "nut&dried fruits mix", 100, 100)]);

                while (true)
                {
                    Console.WriteLine("\nchoose your role: user or admin");

                    string? role;
                    do
                    {
                        role = Console.ReadLine();
                        if (string.IsNullOrWhiteSpace(role))
                        {
                            Console.WriteLine("\nentered role cannot be empty. try again");
                        }
                    } while (string.IsNullOrWhiteSpace(role));

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
                            break;
                    }
                }
            }
            else
            {
                Console.WriteLine("\nthere are no products added yet. you need to login as an admin to add new products.");
                AdminScenario();
            }
        }
        else if (pattern is "manual" && !cashDesk.IsEmpty() && foodWarehouse.HasAvailableProducts())
        {
            Console.WriteLine("\nchoose your role: user or admin");

            string? role;
            do
            {
                role = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(role))
                {
                    Console.WriteLine("\nentered role cannot be empty. try again");
                }
            } while (string.IsNullOrWhiteSpace(role));

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
                    MachineStart("manual");
                    return;
            }
        }
        else
        {
            Console.WriteLine("\nthere are no products or money added yet. you need to login as an admin to add new products and fill the cash desk.");
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
