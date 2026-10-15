using System;

namespace OnlineStore
{
    public abstract class DiscountPolicy
    {
        public abstract Money CalculateDiscount(Order order);
    }

    public class NoDiscount : DiscountPolicy
    {
        public override Money CalculateDiscount(Order order) => new Money(0);
    }

    public class PercentageDiscount : DiscountPolicy
    {
        public decimal Percentage { get; }

        public PercentageDiscount(decimal percentage)
        {
            if (percentage < 0 || percentage > 100)
                throw new ArgumentOutOfRangeException(nameof(percentage), "Процент скидки должен быть в диапазоне от 0 до 100.");

            Percentage = percentage;
        }

        public override Money CalculateDiscount(Order order)
        {
            Money total = order.CalculateTotalGoodsPrice();
            return total * (Percentage / 100m);
        }
    }

    public class FixedDiscount : DiscountPolicy
    {
        public Money DiscountAmount { get; }

        public FixedDiscount(Money discountAmount)
        {
            DiscountAmount = discountAmount;
        }

        public override Money CalculateDiscount(Order order)
        {
            Money total = order.CalculateTotalGoodsPrice();
            if (DiscountAmount.Amount > total.Amount)
                throw new InvalidOperationException($"Фиксированная скидка ({DiscountAmount}) не может превышать стоимость товаров в заказе ({total}).");

            return DiscountAmount;
        }
    }
}