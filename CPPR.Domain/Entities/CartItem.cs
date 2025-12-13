namespace CPPR.Domain.Entities;

public class CartItem
{
    public Dish Item { get; set; } = null!;
    public int Count { get; set; }
}
