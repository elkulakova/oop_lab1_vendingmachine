namespace VendingMachineApp;
public class Admin(VendingMachine vendingMachine) // primary constructor, vendingMachine is already readonly field
{
    public void ChooseTask()
    {
        Console.WriteLine("\nchoose the option you want to do:\n1. refill products;\n2. collect the recieved money;\n3. view the content of the cash desk;\n4. fill the cash desk;\n5. change the role;\n6. exit program.");
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
                vendingMachine.RefillAddProducts();
                ChooseTask();
                return;

            case "2. collect the recieved money":
            case "collect the recieved money":
            case "collect money":
            case "collect":
            case "2":
                if (!vendingMachine.CheckDesk())
                {
                    Console.WriteLine("\nthe cash desk is empty, there is no money to collect.");
                    ChooseTask();
                    return;
                }
                Console.WriteLine("\ncollecting money....");
                vendingMachine.CollectMoney();
                ChooseTask();
                return;

            case "3. view the content of the cash desk":
            case "view the content of the cash desk":
            case "view cash desk":
            case "view":
            case "3":
                vendingMachine.ViewCashDesk();
                ChooseTask();
                return;

            case "4. fill the cash desk":
            case "fill the cash desk":
            case "fill cash desk":
            case "fill":
            case "4":
                Console.WriteLine("\nfilling the cash desk....");
                vendingMachine.FillCashDesk();
                ChooseTask();
                return;

            case "5. change role":
            case "change role":
            case "change":
            case "5":
                if (!vendingMachine.ChangeRoleCheck())
                {
                    ChooseTask();
                    return;
                }
                Console.WriteLine("\nchanging role to user....");
                User user = new();
                Console.WriteLine("\nhello, user! ready to choose?");
                vendingMachine.UserScenario(user);
                return;

            case "6. exit program":
            case "exit program":
            case "exit":
            case "6":
                Console.WriteLine("\nexiting program....");
                VendingMachine.ShutDown();
                return;

            default:
                Console.WriteLine("\ninvalid option. please choose 1, 2, 3, 4, 5 or 6.");
                ChooseTask();
                return;
        }
    }
}