using System;

namespace OnlineStore
{
    public class CheckoutResult
    {
        public Money GoodsTotal { get; }
        public Money DiscountAmount { get; }
        public Money DeliveryCost { get; }
        public Money FinalTotal { get; }

        public CheckoutResult(Money goodsTotal, Money discountAmount, Money deliveryCost, Money finalTotal)
        {
            GoodsTotal = goodsTotal;
            DiscountAmount = discountAmount;
            DeliveryCost = deliveryCost;
            FinalTotal = finalTotal;
        }
    }

    public class CheckoutService
    {
        public CheckoutResult CalculateOrder(Order order, DiscountPolicy discountPolicy, DeliveryMethod deliveryMethod)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));
            if (discountPolicy == null) throw new ArgumentNullException(nameof(discountPolicy));
            if (deliveryMethod == null) throw new ArgumentNullException(nameof(deliveryMethod));

            if (order.Items.Count == 0)
                throw new InvalidOperationException("Невозможно оформить пустой заказ.");

            if (!deliveryMethod.CanDeliver(order))
                throw new InvalidOperationException($"Способ доставки «{deliveryMethod.Name}» не подходит для содержимого данного заказа.");

            Money goodsTotal = order.CalculateTotalGoodsPrice();
            Money discount = discountPolicy.CalculateDiscount(order);
            Money deliveryCost = deliveryMethod.CalculateDeliveryCost(order);

            // Формула: Итого = Стоимость товаров - Скидка + Доставка
            Money finalTotal = (goodsTotal - discount) + deliveryCost;

            return new CheckoutResult(goodsTotal, discount, deliveryCost, finalTotal);
        }
    }
}