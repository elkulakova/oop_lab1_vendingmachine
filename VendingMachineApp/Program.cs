namespace VendingMachineApp;
class Program
{
    static void Main()
    {
        Console.WriteLine("\nwelcome to the Vending Machine simulator!\n");
        VendingMachine vendingMachine = new();
        vendingMachine.MachineStart();
    }
}