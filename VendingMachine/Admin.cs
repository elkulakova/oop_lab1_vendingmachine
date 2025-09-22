using System.Diagnostics.CodeAnalysis;
using System.Reflection.PortableExecutable;

public class Admin
{
    public Admin(string password, VendingMachine machine)
    {
        if (password != "admin_password")
        {
            Console.WriteLine("\nyou cannot log in as an admin untill you enter the valid pasword");
            machine.MachineStart();
        }
        else
        {
            Console.WriteLine("\nyou are authorized as admin!");
        }
    }

    public static void AddNewPositions(VendingMachine machine)
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

            var parts = input_prod.Split(", ");

            if (parts.Length != 4)
            {
                Console.WriteLine("\nenter product data in a correct way");
                i--;
                continue;
            }

            string name = parts[1];

            if (!int.TryParse(parts[0], out int id) || !int.TryParse(parts[2], out int price) || price < 0 || !int.TryParse(parts[3], out int quantity) || quantity <= 0)
            {
                Console.WriteLine("\nenter product data in a correct way");
                i--;
                continue;
            }

            if (!machine.IdSet.Contains(id))
            {
                Product product_exemp = new() { Id = id, Name = name, Price = price, Quantity = quantity };
                machine.AvailableProducts.Add(product_exemp);
                machine.IdSet.Add(id);
            }
            else // since id in the set
            {
                Product existingProduct = machine.AvailableProducts.First(product => product.Id == id); // since the product is already in the AvailableProducts, id in IdSet
                Console.WriteLine($"\nid {id} already exists: {existingProduct.AdminInfoOutput()}. change the parameters and try again.");
                i--; // do we need it?? think no, since admin wanted to add this product also and it is included in the total amount of products to add
                continue;
            }
        }
    }

    public static void RefillProducts(VendingMachine machine)
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

                var parts = prod_data.Split(", ");
                if (parts.Length != 2)
                {
                    Console.WriteLine("\nwrong format. use: ProductId, ProductAmount. try again:");
                    i--;
                    continue;
                }

                if (!int.TryParse(parts[0], out int id) || !int.TryParse(parts[1], out int quantity) || quantity <= 0)
                {
                    Console.WriteLine("\nwrong format. id must exist and amount must be positive. try again");
                    i--;
                    continue;
                }

                Product? foundProduct = machine.AvailableProducts.FirstOrDefault(product => product.Id == id);

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

    public static void RefillAddProducts(VendingMachine machine) // ststic is suggested by VSCode
    {
        if (machine.AvailableProducts.Count == 0)
        {
            Console.WriteLine("\nthere are no products added yet.");
            AddNewPositions(machine);
        }
        else
        {
            Console.WriteLine("\nhere are the list of available products:\n");
            foreach (Product product in machine.AvailableProducts.Cast<Product>())
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
                    Console.WriteLine("\nrefilling products...."); // gap closure, here i will run an appropriate function. надо здесь сделать доступ к списку продуктов в автомате и их количеству тоже
                    RefillProducts(machine);
                    ChooseTask(machine);
                    return;

                case "2. add new product":
                case "add new product":
                case "add product":
                case "add new":
                case "add":
                case "2":
                    Console.WriteLine("\nadding new products...."); // gap closure
                    AddNewPositions(machine);
                    ChooseTask(machine);
                    return;

                case "3. exit to main menu":
                case "exit to main menu":
                case "exit":
                case "3":
                    Console.WriteLine("\nexiting to main menu...."); // gap closure
                    ChooseTask(machine);
                    return;

                default:
                    Console.WriteLine("\ninvalid option. please choose 1 or 2.");
                    RefillAddProducts(machine);
                    return;
            }
        }
    }

    public static void CollectMoney(VendingMachine machine)
    {
        var cashDesk = machine.CashDesk;
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

    public static void ChooseTask(VendingMachine machine)
    {
        Console.WriteLine("\nchoose the option you want to do:\n1. refill products;\n2. collect the recieved money;\n3. change role;\n4. exit program.");
        string? option = Console.ReadLine();

        while (string.IsNullOrEmpty(option) || string.IsNullOrWhiteSpace(option))
        {
            Console.WriteLine("\nchoose the task to do, input cannot be empty.");
            option = Console.ReadLine();
        }

        switch (option.ToLower().Trim())
        {
            case "1. refill products":
            case "refill products":
            case "refill":
            case "1":
                Console.WriteLine("\nrefilling products...."); // gap closure, here i will run an appropriate function. надо здесь сделать доступ к списку продуктов в автомате и их количеству тоже
                RefillAddProducts(machine);
                ChooseTask(machine);
                return;

            case "2. collect the recieved money":
            case "collect the recieved money":
            case "collect money":
            case "collect":
            case "2":
                Console.WriteLine("\ncollecting money...."); // gap closure
                CollectMoney(machine);
                ChooseTask(machine);
                return;

            case "3. change role":
            case "change role":
            case "change":
            case "3":
                Console.WriteLine("\nchanging role to user...."); // gap closure
                User user = new();
                machine.UserScenario(user);
                return;

            case "4. exit program":
            case "exit program":
            case "exit":
            case "4":
                Console.WriteLine("\nexiting program...."); // gap closure
                VendingMachine.ShutDown();
                return;
        }
    }
}