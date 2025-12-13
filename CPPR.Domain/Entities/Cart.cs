namespace CPPR.Domain.Entities;

public class Cart
{
    /// <summary>
    /// Список объектов в корзине
    /// key - идентификатор объекта
    /// </summary>
    public Dictionary<int, CartItem> CartItems { get; set; } = new();

    /// <summary>
    /// Добавить объект в корзину
    /// </summary>
    /// <param name="dish">Добавляемый объект</param>
    public virtual void AddToCart(Dish dish)
    {
        if (CartItems.ContainsKey(dish.Id))
        {
            CartItems[dish.Id].Count++;
        }
        else
        {
            CartItems[dish.Id] = new CartItem { Item = dish, Count = 1 };
        }
    }

    /// <summary>
    /// Удалить объект из корзины
    /// </summary>
    /// <param name="id">id удаляемого объекта</param>
    public virtual void RemoveItems(int id)
    {
        CartItems.Remove(id);
    }

    /// <summary>
    /// Очистить корзину
    /// </summary>
    public virtual void ClearAll()
    {
        CartItems.Clear();
    }

    /// <summary>
    /// Количество объектов в корзине
    /// </summary>
    public int Count { get => CartItems.Sum(item => item.Value.Count); }

    /// <summary>
    /// Общее количество калорий
    /// </summary>
    public double TotalCalories 
    { 
        get => CartItems.Sum(item => item.Value.Item.Calories * item.Value.Count); 
    }
}
