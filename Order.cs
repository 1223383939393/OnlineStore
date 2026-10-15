using System;
using System.Collections.Generic;
using System.Linq;

namespace OnlineStore
{
    public class OrderItem
    {
        public Product Product { get; }
        public int Quantity { get; }

        public OrderItem(Product product, int quantity)
        {
            Product = product ?? throw new ArgumentNullException(nameof(product));
            if (quantity < 1)
                throw new ArgumentException("Количество товара не может быть меньше 1.", nameof(quantity));

            Quantity = quantity;
        }

        public Money GetTotalPrice() => Product.GetUnitPrice() * Quantity;
        public decimal GetTotalWeight() => Product.GetDeliveryWeight() * Quantity;
    }

    public class Order
    {
        private readonly List<OrderItem> _items = new List<OrderItem>();
        public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

        // Перегрузка 1: добавляет 1 единицу товара
        public void AddProduct(Product product)
        {
            AddProduct(product, 1);
        }

        // Перегрузка 2: добавляет указанное количество
        public void AddProduct(Product product, int quantity)
        {
            if (quantity < 1)
                throw new ArgumentException("Нельзя добавить товар с количеством меньше 1.", nameof(quantity));

            _items.Add(new OrderItem(product, quantity));
        }

        public Money CalculateTotalGoodsPrice()
        {
            Money total = new Money(0);
            foreach (var item in _items)
            {
                total += item.GetTotalPrice();
            }
            return total;
        }

        public bool HasPhysicalProducts()
        {
            return _items.Any(item => item.Product.RequiresPhysicalDelivery);
        }

        public decimal CalculateTotalWeight()
        {
            return _items.Sum(item => item.GetTotalWeight());
        }
    }
}