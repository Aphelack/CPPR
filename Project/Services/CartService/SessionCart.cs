using CPPR.Domain.Entities;
using Project.Extensions;
using System.Text.Json.Serialization;

namespace Project.Services.CartService;

public class SessionCart : Cart
{
    private readonly ISession _session;
    private const string CartSessionKey = "cart";

    [JsonConstructor]
    public SessionCart() 
    {
        _session = null!;
    }

    public SessionCart(IHttpContextAccessor httpContextAccessor)
    {
        _session = httpContextAccessor.HttpContext?.Session 
            ?? throw new InvalidOperationException("Session is not available");
        LoadCart();
    }

    private void LoadCart()
    {
        var cart = _session.Get<Cart>(CartSessionKey);
        if (cart != null)
        {
            CartItems = cart.CartItems;
        }
    }

    private void SaveCart()
    {
        _session.Set(CartSessionKey, this);
    }

    public override void AddToCart(Dish dish)
    {
        base.AddToCart(dish);
        SaveCart();
    }

    public override void RemoveItems(int id)
    {
        base.RemoveItems(id);
        SaveCart();
    }

    public override void ClearAll()
    {
        base.ClearAll();
        SaveCart();
    }
}
