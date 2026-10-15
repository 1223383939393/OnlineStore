using System;

namespace OnlineStore
{
    public interface IPaymentMethod
    {
        void Pay(Money amount);
    }

    public class CardPayment : IPaymentMethod
    {
        public string CardNumber { get; }

        public CardPayment(string cardNumber)
        {
            CardNumber = string.IsNullOrWhiteSpace(cardNumber) ? "**** 0000" : cardNumber;
        }

        public void Pay(Money amount)
        {
            Console.WriteLine($"[Оплата картой {CardNumber}] Успешно списано: {amount}.");
        }
    }

    public class CashPayment : IPaymentMethod
    {
        public Money ProvidedCash { get; }

        public CashPayment(Money providedCash)
        {
            ProvidedCash = providedCash;
        }

        public void Pay(Money amount)
        {
            if (ProvidedCash.Amount < amount.Amount)
                throw new InvalidOperationException($"Недостаточно наличных для оплаты. К оплате: {amount}, внесено: {ProvidedCash}.");

            Money change = ProvidedCash - amount;
            Console.WriteLine($"[Оплата наличными] Принято: {ProvidedCash}. К оплате: {amount}. Ваша сдача: {change}.");
        }
    }
}