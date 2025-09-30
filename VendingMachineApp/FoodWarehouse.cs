namespace VendingMachineApp;

public class FoodWarehouse
{
    private List<Product> Products = [];
    private readonly HashSet<int> IdSet = []; // filled automatically
    private readonly HashSet<string> NamingSet = []; // filled automatically
    public FoodWarehouse() // primary constructor, vendingMachine is already readonly field
    {
    }
    public List<Product> GetProducts()
    {
        return Products;
    }
    public void ViewProducts(string role)
    {
        if (Products.Count == 0)
        {
            Console.WriteLine("\nthere are no products in the vending machine yet.");
            return;
        }

        if (role.ToLower().Trim() == "user")
        {
            Console.WriteLine("\nhere is the LIST of available products in the vending machine:\n");
            foreach (Product prod in Products)
            {
                if (prod.UserInfoOutput() is not null)
                {
                    Console.WriteLine($"{prod.UserInfoOutput()}");
                }
            }
        }
        else if (role.ToLower().Trim() == "admin")
        {
            Console.WriteLine("\nhere is the list of ALL products (in stock and out of stock):\n");
            if (Products.Count == 0)
            {
                Console.WriteLine("there are NO products ADDED YET, nothing to show.");
            }
            foreach (Product prod in Products)
            {
                Console.WriteLine($"{prod.AdminInfoOutput()}");
            }
        }
    }
    public void AddProducts()
    {
        Console.WriteLine("\nhow many products types do you want to add? enter an integer value.");
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
                Products.Add(product_exemp);
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
                Product existingProductId = Products.First(product => product.Id == id); // since the product is already in the AvailableProducts, id in IdSet
                Product existingProductName = Products.First(product => product.Name == name);
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

                Product? foundProduct = Products.FirstOrDefault(product => product.Id == id);

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
    public bool HasAvailableProducts()
    {
        return Products.Any(p => p.InStock);
    }
    public HashSet<int> GetAvailableProductIds()
    {
        return Products.Where(p => p.InStock).Select(p => p.Id).ToHashSet();
    }
    public void FillProducts(List<Product> products)
    {
        Products = products;
        IdSet.Clear();
        NamingSet.Clear();
        foreach (var product in products)
        {
            IdSet.Add(product.Id);
            NamingSet.Add(product.Name);
        }
    }
}