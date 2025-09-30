namespace VendingMachineApp;
public class User(VendingMachine vendingMachine) : Role(vendingMachine) // primary constructor, vendingMachine is already readonly field
{
    public override void ChooseTask()
    {
        Console.WriteLine("\nchoose an option to do:\n1. see available products\n2. buy something\n3. change the role\n4. exit program");
        string? answer = Console.ReadLine();

        while (string.IsNullOrEmpty(answer) || string.IsNullOrWhiteSpace(answer))
        {
            Console.WriteLine("entered answer cannot be empty. try again");
            answer = Console.ReadLine();
        }

        switch (answer.ToLower().Trim())
        {
            case "1. see available products":
            case "see available products":
            case "see products":
            case "available products":
            case "see":
            case "1":
                vendingMachine.StoreWindow(); // no need to check if anythig in stock cause you can see the available products, and after seeing nothing changes
                ChooseTask();
                return;

            case "2. buy something":
            case "buy something":
            case "buy":
            case "2":
                Console.WriteLine("be ready to empty your purse!");
                vendingMachine.Purchase();
                vendingMachine.UserScenario(this); // nned to check if there are any products left after the purchase
                return;

            case "3. change the role":
            case "change role":
            case "change the role":
            case "change":
            case "3":
                Console.WriteLine("trying to change the role to admin...");
                vendingMachine.AdminScenario();
                return;

            case "4. exit program":
            case "exit program":
            case "exit":
            case "4":
                vendingMachine.ShutDown();
                return;

            default:
                Console.WriteLine("wrong input. try again, choose 1, 2, 3 or 4");
                ChooseTask();
                return;
        }
    }
}