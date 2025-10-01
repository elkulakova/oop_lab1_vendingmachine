namespace VendingMachineApp;
public class Admin(VendingMachine vendingMachine) : Role(vendingMachine) // primary constructor, vendingMachine is already readonly field
{
    public override void ChooseTask()
    {
        while (true)
        {
            Console.WriteLine("\nchoose the option you want to do:\n1. see products list\n2. refill products;\n3. collect the recieved money;\n4. view the content of the cash desk;\n5. fill the cash desk;\n6. change the role;\n7. exit program.");
            string? option;
            do
            {
                option = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(option))
                    Console.WriteLine("entered answer cannot be empty. try again");
            } while (string.IsNullOrWhiteSpace(option));

            switch (option.ToLower().Trim())
            {
                case "1. see products list":
                case "see products":
                case "products list":
                case "see":
                case "1":
                    vendingMachine.ProductsView();
                    break;

                case "2. refill products":
                case "refill products":
                case "refill":
                case "2":
                    vendingMachine.RefillAddProducts();
                    break;

                case "3. collect the recieved money":
                case "collect the recieved money":
                case "collect money":
                case "collect":
                case "3":
                    if (vendingMachine.CheckDesk())
                    {
                        Console.WriteLine("\nthe cash desk is empty, there is no money to collect.");
                        break;
                    }
                    Console.WriteLine("\ncollecting money....");
                    vendingMachine.CollectMoney();
                    break;

                case "4. view the content of the cash desk":
                case "view the content of the cash desk":
                case "view cash desk":
                case "view":
                case "4":
                    vendingMachine.ViewCashDesk();
                    break;

                case "5. fill the cash desk":
                case "fill the cash desk":
                case "fill cash desk":
                case "fill":
                case "5":
                    Console.WriteLine("\nfilling the cash desk....");
                    vendingMachine.FillCashDesk();
                    break;

                case "6. change role":
                case "change role":
                case "change":
                case "6":
                    if (!vendingMachine.ChangeRoleCheck())
                    {
                        break;
                    }
                    Console.WriteLine("\nchanging role to user....");
                    User user = new(vendingMachine);
                    Console.WriteLine("\nhello, user! ready to choose?");
                    vendingMachine.UserScenario(user);
                    return;

                case "7. exit program":
                case "exit program":
                case "exit":
                case "7":
                    vendingMachine.ShutDown();
                    return;

                default:
                    Console.WriteLine("\ninvalid option. please choose 1, 2, 3, 4, 5, 6 or 7.");
                    break;
            }
        }
    }
}