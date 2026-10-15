using System;

namespace OnlineStore
{
    public abstract class Product
    {
        public string Name { get; }

        protected Product(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Название товара не может быть пустым.", nameof(name));

            Name = name;
        }

        public abstract Money GetUnitPrice();
        public abstract bool RequiresPhysicalDelivery { get; }
        public abstract decimal GetDeliveryWeight();
    }

    public class PhysicalProduct : Product
    {
        public Money Price { get; }
        public decimal WeightKg { get; }

        public PhysicalProduct(string name, Money price, decimal weightKg) : base(name)
        {
            if (weightKg <= 0)
                throw new ArgumentException("Вес физического товара должен быть больше 0.", nameof(weightKg));

            Price = price;
            WeightKg = weightKg;
        }

        public override Money GetUnitPrice() => Price;
        public override bool RequiresPhysicalDelivery => true;
        public override decimal GetDeliveryWeight() => WeightKg;
    }

    public class DigitalProduct : Product
    {
        public Money Price { get; }
        public double FileSizeMb { get; }

        public DigitalProduct(string name, Money price, double fileSizeMb) : base(name)
        {
            if (fileSizeMb <= 0)
                throw new ArgumentException("Размер файла должен быть больше 0.", nameof(fileSizeMb));

            Price = price;
            FileSizeMb = fileSizeMb;
        }

        public override Money GetUnitPrice() => Price;
        public override bool RequiresPhysicalDelivery => false;
        public override decimal GetDeliveryWeight() => 0m;
    }

    public class SubscriptionProduct : Product
    {
        public Money MonthlyPrice { get; }
        public int DurationMonths { get; }

        public SubscriptionProduct(string name, Money monthlyPrice, int durationMonths) : base(name)
        {
            if (durationMonths <= 0)
                throw new ArgumentException("Продолжительность подписки должна быть минимум 1 месяц.", nameof(durationMonths));

            MonthlyPrice = monthlyPrice;
            DurationMonths = durationMonths;
        }

        public override Money GetUnitPrice()
        {
            Money total = MonthlyPrice * DurationMonths;
            // При покупке на 12 месяцев и больше — скидка 10% (остается 90% стоимости)
            if (DurationMonths >= 12)
            {
                return total * 0.90m;
            }
            return total;
        }

        public override bool RequiresPhysicalDelivery => false;
        public override decimal GetDeliveryWeight() => 0m;
    }
}