using System;
using System.Globalization;

namespace OnlineStore
{
    public readonly struct Money
    {
        public decimal Amount { get; }

        public Money(decimal amount)
        {
            if (amount < 0)
                throw new ArgumentException("Денежная сумма не может быть отрицательной.", nameof(amount));

            // Округляем до копеек для точности расчетов
            Amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);
        }

        public static Money operator +(Money a, Money b) => new Money(a.Amount + b.Amount);

        public static Money operator -(Money a, Money b)
        {
            if (a.Amount < b.Amount)
                throw new InvalidOperationException($"Нельзя вычесть {b} из {a}: результат будет отрицательным.");

            return new Money(a.Amount - b.Amount);
        }

        public static Money operator *(Money money, int multiplier)
        {
            if (multiplier < 0)
                throw new ArgumentException("Множитель не может быть отрицательным.", nameof(multiplier));

            return new Money(money.Amount * multiplier);
        }

        public static Money operator *(int multiplier, Money money) => money * multiplier;

        public static Money operator *(Money money, decimal multiplier)
        {
            if (multiplier < 0)
                throw new ArgumentException("Множитель не может быть отрицательным.", nameof(multiplier));

            return new Money(money.Amount * multiplier);
        }

        public static Money operator *(decimal multiplier, Money money) => money * multiplier;

        public override string ToString()
        {
            var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = " ";
            nfi.NumberDecimalSeparator = ",";
            return $"{Amount.ToString("N2", nfi)} ₽";
        }
    }
}