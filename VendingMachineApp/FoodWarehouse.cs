namespace VendingMachineApp;

public class FoodWarehouse
{
    private List<Product> Products = new();
    private readonly HashSet<int> IdSet = new(); // filled automatically
    private readonly HashSet<string> NamingSet = new(); // filled automatically
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
            Console.WriteLine("\nthere are NO PRODUCTS in the vending machine yet, nothing to show.");
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
            foreach (Product prod in Products)
            {
                Console.WriteLine($"{prod.AdminInfoOutput()}");
            }
        }
    }
    public void AddProduct(int id, string name, int price, int quantity)
    {
        if (id <= 0 || price <= 0 || quantity < 0 || string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("\nwrong input, product id, price and quantity must be positive AND name cannot be empty or whitespace");
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
            throw new ArgumentException($"\nproduct with name '{name}' already exists. change the parameters and try again.");
        }
        else if (IdSet.Contains(id) && !NamingSet.Contains(name))
        {
            throw new ArgumentException($"\nproduct with id {id} already exists. change the parameters and try again.");
        }
        else if (IdSet.Contains(id) && NamingSet.Contains(name))
        {
            Product existingProductId = Products.First(product => product.Id == id); // since the product is already in the AvailableProducts, id in IdSet
            Product existingProductName = Products.First(product => product.Name == name);
            if (existingProductId == existingProductName)
            {
                throw new ArgumentException($"\nproduct {name} (id {id}) already exists: {existingProductId.AdminInfoOutput()}. change the parameters and try again.");
            }
            else
            {
                throw new ArgumentException($"\nTHERE ARE TWO DIFFERENT PRODUCTS\nid {id} already exists: {existingProductId.AdminInfoOutput()}.\nproduct {name} already exists: {existingProductName.AdminInfoOutput()}.\nchange the parameters and try again.");
            }
        }
    }
    public void RefillProduct(int id, int quantity)
    {
        Product? foundProduct = Products.FirstOrDefault(product => product.Id == id);
        if (foundProduct is null || quantity <= 0)
        {
            if (foundProduct is null && quantity > 0)
                throw new ArgumentException($"\nthere is no product with id {id} in the vending machine, cannot refill.");
            else if (foundProduct is not null && quantity <= 0)
                throw new ArgumentException("\nwrong input, quantity to add must be positive, cannot refill with non-positive quantity.");
            else
                throw new ArgumentException($"\nthere is no product with id {id} in the vending machine, cannot refill AND quantity to add must be positive, cannot refill with non-positive quantity.");
        }
        foundProduct.Quantity += quantity;
        Console.WriteLine($"\nyou refilled {foundProduct.Name} (id {foundProduct.Id}) by {quantity} pieces. now the amount is {foundProduct.Quantity}");
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