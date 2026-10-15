using System;

namespace OnlineStore
{
    public abstract class DeliveryMethod
    {
        public abstract string Name { get; }
        public abstract bool CanDeliver(Order order);
        public abstract Money CalculateDeliveryCost(Order order);
    }

    public class CourierDelivery : DeliveryMethod
    {
        public override string Name => "Курьерская доставка";

        public override bool CanDeliver(Order order) => order.HasPhysicalProducts();

        public override Money CalculateDeliveryCost(Order order)
        {
            if (!CanDeliver(order))
                throw new InvalidOperationException("Курьерская доставка доступна только для заказов с физическими товарами.");

            decimal weight = order.CalculateTotalWeight();
            // 300 ₽ база + 80 ₽ за каждый килограмм
            return new Money(300m) + (new Money(80m) * weight);
        }
    }

    public class PickupDelivery : DeliveryMethod
    {
        public override string Name => "Самовывоз";

        public override bool CanDeliver(Order order) => order.HasPhysicalProducts();

        public override Money CalculateDeliveryCost(Order order)
        {
            if (!CanDeliver(order))
                throw new InvalidOperationException("Самовывоз доступен только для заказов с физическими товарами.");

            return new Money(0m);
        }
    }

    public class DigitalDelivery : DeliveryMethod
    {
        public override string Name => "Цифровая доставка (на e-mail)";

        public override bool CanDeliver(Order order) => !order.HasPhysicalProducts();

        public override Money CalculateDeliveryCost(Order order)
        {
            if (!CanDeliver(order))
                throw new InvalidOperationException("Цифровая доставка подходит только для заказов без физических товаров.");

            return new Money(0m);
        }
    }
}