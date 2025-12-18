using System;
using System.Collections.Generic;
using System.Linq;

namespace OrderPatterns.OrderDomain;

/// <summary>
/// A tiny domain model we will re-use across ALL pattern demos so the "use case"
/// stays consistent: placing an e-commerce order.
/// </summary>
public sealed class Order
{
    public string OrderId { get; }
    public Customer Customer { get; }
    public IReadOnlyList<OrderItem> Items { get; }
    public Address ShipTo { get; }

    public DateTimeOffset CreatedUtc { get; } = DateTimeOffset.UtcNow;

    public Order(string orderId, Customer customer, IEnumerable<OrderItem> items, Address shipTo)
    {
        OrderId = orderId ?? throw new ArgumentNullException(nameof(orderId));
        Customer = customer ?? throw new ArgumentNullException(nameof(customer));
        Items = items?.ToList() ?? throw new ArgumentNullException(nameof(items));
        ShipTo = shipTo ?? throw new ArgumentNullException(nameof(shipTo));

        if (Items.Count == 0) throw new ArgumentException("Order must contain at least one item.", nameof(items));
    }

    public decimal Subtotal => Items.Sum(i => i.UnitPrice * i.Quantity);

    public override string ToString() =>
        $"{OrderId} for {Customer.Name} | {Items.Count} items | Subtotal {Subtotal:C}";
}

public sealed record Customer(string CustomerId, string Name, string Email);

public sealed record Address(string Line1, string City, string State, string PostalCode, string Country = "US");

public sealed record OrderItem(string Sku, string Name, int Quantity, decimal UnitPrice);

public sealed record Receipt(string OrderId, decimal Subtotal, decimal Shipping, decimal Tax, decimal Total);

/// <summary>Simple event record used by Observer demo.</summary>
public sealed record OrderEvent(string OrderId, string Type, DateTimeOffset Utc, string Message);

public interface IPaymentProcessor
{
    string Name { get; }
    void Charge(string orderId, decimal amount);
}

public interface IShippingRateCalculator
{
    string Name { get; }
    decimal CalculateShipping(Order order);
}

public interface ITaxCalculator
{
    string Name { get; }
    decimal CalculateTax(decimal taxableAmount, Address shipTo);
}

public interface INotificationSender
{
    string Name { get; }
    void SendOrderConfirmation(Customer customer, Receipt receipt);
}

/// <summary>
/// Shared helper used by several demos.
/// </summary>
public static class OrderMath
{
    public static Receipt BuildReceipt(Order order, decimal shipping, decimal tax)
    {
        var subtotal = order.Subtotal;
        var total = subtotal + shipping + tax;
        return new Receipt(order.OrderId, subtotal, shipping, tax, total);
    }
}
