// SingletonDemo
// Pattern: Singleton
// Use case: Place an e-commerce order (same domain model reused across all demos).
//
// Tip: Each demo is a self-contained Console app. Run them individually.
// ------------------------------------------------------------

using OrderPatterns.OrderDomain;
using System;


// Singleton: ensure exactly one instance of a shared service.
// Here we use it for application-level settings used during checkout.
sealed class AppSettings
{
    private static readonly Lazy<AppSettings> _instance = new(() => new AppSettings());
    public static AppSettings Instance => _instance.Value;

    // Example settings
    public string EnvironmentName { get; } = "UAT";
    public decimal DefaultTaxRate { get; } = 0.0825m;

    private AppSettings() { }
}

sealed class FlatRateShipping : IShippingRateCalculator
{
    public string Name => "Flat";
    public decimal CalculateShipping(Order order) => 7.99m;
}

sealed class SimpleTax : ITaxCalculator
{
    public string Name => "SimplePercent";
    public decimal CalculateTax(decimal taxableAmount, Address shipTo)
    {
        // Using Singleton-configured tax rate
        var rate = AppSettings.Instance.DefaultTaxRate;
        return Math.Round(taxableAmount * rate, 2);
    }
}

sealed class ConsolePayment : IPaymentProcessor
{
    public string Name => "ConsolePay";
    public void Charge(string orderId, decimal amount) =>
        Console.WriteLine($"[{Name}] Charged {amount:C} for order {orderId}");
}

sealed class ConsoleNotify : INotificationSender
{
    public string Name => "ConsoleNotify";
    public void SendOrderConfirmation(Customer customer, Receipt receipt) =>
        Console.WriteLine($"[{Name}] Sent receipt to {customer.Email}: Total {receipt.Total:C}");
}

sealed class CheckoutService
{
    private readonly IShippingRateCalculator _shipping;
    private readonly ITaxCalculator _tax;
    private readonly IPaymentProcessor _payment;
    private readonly INotificationSender _notify;

    public CheckoutService(IShippingRateCalculator shipping, ITaxCalculator tax, IPaymentProcessor payment, INotificationSender notify)
    {
        _shipping = shipping; _tax = tax; _payment = payment; _notify = notify;
    }

    public Receipt PlaceOrder(Order order)
    {
        Console.WriteLine($"Env: {AppSettings.Instance.EnvironmentName} (Singleton)");
        var shipping = _shipping.CalculateShipping(order);
        var tax = _tax.CalculateTax(order.Subtotal + shipping, order.ShipTo);
        var receipt = OrderMath.BuildReceipt(order, shipping, tax);

        _payment.Charge(order.OrderId, receipt.Total);
        _notify.SendOrderConfirmation(order.Customer, receipt);
        return receipt;
    }
}

static class Program
{
    static void Main()
    {
        var order = SampleData.CreateOrder();
        var checkout = new CheckoutService(new FlatRateShipping(), new SimpleTax(), new ConsolePayment(), new ConsoleNotify());
        checkout.PlaceOrder(order);
    }
}

static class SampleData
{
    public static Order CreateOrder() =>
        new Order(
            orderId: "ORD-1001",
            customer: new Customer("CUST-1", "Ashar", "ashar@example.com"),
            items: new[]
            {
                new OrderItem("SKU-KEYBOARD", "Keyboard", 1, 49.99m),
                new OrderItem("SKU-MOUSE", "Mouse", 2, 19.99m),
            },
            shipTo: new Address("123 Main St", "Wichita", "KS", "67202")
        );
}
