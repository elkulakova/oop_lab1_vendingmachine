namespace VendingMachineApp;
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

    public static void ChooseTask(VendingMachine machine)
    {
        Console.WriteLine("\nchoose the option you want to do:\n1. refill products;\n2. collect the recieved money;\n3. change role;\n4. fill the cash desk;\n5. exit program.");
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
                machine.RefillAddProducts();
                ChooseTask(machine);
                return;

            case "2. collect the recieved money":
            case "collect the recieved money":
            case "collect money":
            case "collect":
            case "2":
                while (machine.CashDesk.Values.All(v => v == 0))
                {
                    Console.WriteLine("\ncash desk is empty, there is no money to collect");
                    ChooseTask(machine);
                    return;
                }
                Console.WriteLine("\ncollecting money....");
                machine.CollectMoney();
                ChooseTask(machine);
                return;

            case "3. change role":
            case "change role":
            case "change":
            case "3":
                while (machine.AvailableProducts.Count == 0 || machine.CashDesk.Values.All(v => v == 0))
                {
                    if (machine.AvailableProducts.Count == 0)
                    {
                        Console.WriteLine("\nthere are no products added yet.\nyou need to add products before changing the role.");
                        machine.AddNewPositions();
                    }
                    else if (machine.CashDesk.Values.All(v => v == 0))
                    {
                        Console.WriteLine("\nthe cash desk is empty, there is no money to give change from.\nyou need to fill the cash desk before changing the role.");
                        machine.FillCashDesk();
                    }
                    else
                    {
                        Console.WriteLine("\nno products & money yet. cannot change the role.");
                        machine.AddNewPositions();
                        machine.FillCashDesk();
                    }
                }
                Console.WriteLine("\nchanging role to user....");
                User user = new();
                Console.WriteLine("\nhello, user! ready to choose?");
                machine.UserScenario(user);
                return;

            case "4. fill the cash desk":
            case "fill the cash desk":
            case "fill cash desk":
            case "fill":
            case "4":
                Console.WriteLine("\nfilling the cash desk....");
                machine.FillCashDesk();
                ChooseTask(machine);
                return;

            case "5. exit program":
            case "exit program":
            case "exit":
            case "5":
                Console.WriteLine("\nexiting program....");
                VendingMachine.ShutDown();
                return;

            default:
                Console.WriteLine("\ninvalid option. please choose 1, 2, 3 or 4.");
                ChooseTask(machine);
                return;
        }
    }
}