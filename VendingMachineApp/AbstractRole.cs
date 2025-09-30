namespace VendingMachineApp;
public interface IRole
{
    void ChooseTask();
}

public abstract class Role: IRole
{
    protected readonly VendingMachine vendingMachine;

    public Role(VendingMachine vendingMachine) // primary constructor, vendingMachine is already readonly field
    {
        this.vendingMachine = vendingMachine;
    }

    public abstract void ChooseTask();
}