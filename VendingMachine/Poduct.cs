public class Product
{
    private int _productPrice;
    private int _productQuantity;

    public required string Name { get; set; }
    public required int Id { get; set; } // uniqueness check – in VendingMachine IdSet

    public required int Price
    {
        get { return _productPrice; }
        set
        {
            if (value <= 0)
                throw new ArgumentException("price of the product cannot be equal or less than 0!");
            _productPrice = value;
        }
    }

    public required int Quantity
    {
        get { return _productQuantity; }
        set
        {
            if (value < 0)
                throw new ArgumentException("quantity of the product cannot be negative!");
            _productQuantity = value;
        }
    }

    public bool InStock
    {
        get { return _productQuantity > 0; }
    }

    public Product()
    {}
    public Product(int id, string name, int price, int quantity)
    {
        Id = id;
        Name = name;
        Price = price;
        Quantity = quantity;
    }

    public string AdminInfoOutput()
    {
        if (InStock)
            return $"{Name} (id {Id}): price {Price} RUB, in stock {Quantity}";
        return $"{Name} (id {Id}): price {Price} RUB - OUT OF STOCK";
    }

    public string? ConsumerInfoOutput()
    {
        if (InStock)
            return $"{Name} (id {Id}): price {Price} RUB, in stock {Quantity}";
        return null;
    }
}