using System;
using System.Globalization;
using System.Text;

namespace OnlineStore
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            Order currentOrder = new Order();
            CheckoutService checkoutService = new CheckoutService();

            while (true)
            {
                Console.WriteLine("\n==================================================");
                Console.WriteLine($" ИНТЕРНЕТ-МАГАЗИН (Позиций в корзине: {currentOrder.Items.Count})");
                Console.WriteLine("==================================================");
                Console.WriteLine("1. Добавить физический товар");
                Console.WriteLine("2. Добавить цифровой товар");
                Console.WriteLine("3. Добавить подписку");
                Console.WriteLine("4. Посмотреть корзину");
                Console.WriteLine("5. Оформить и оплатить заказ");
                Console.WriteLine("6. Очистить корзину");
                Console.WriteLine("7. [Авто-демонстрация по Требованию 8 из ТЗ]");
                Console.WriteLine("0. Выход");
                Console.Write("\nВыберите действие: ");

                string choice = Console.ReadLine()?.Trim();
                Console.WriteLine();

                try
                {
                    switch (choice)
                    {
                        case "1":
                            AddPhysicalProductInteractive(currentOrder);
                            break;
                        case "2":
                            AddDigitalProductInteractive(currentOrder);
                            break;
                        case "3":
                            AddSubscriptionInteractive(currentOrder);
                            break;
                        case "4":
                            PrintOrder(currentOrder);
                            break;
                        case "5":
                            CheckoutInteractive(currentOrder, checkoutService);
                            break;
                        case "6":
                            currentOrder = new Order();
                            PrintSuccess("Корзина очищена.");
                            break;
                        case "7":
                            RunAutoDemo();
                            break;
                        case "0":
                            return;
                        default:
                            PrintError("Неверный пункт меню. Выберите цифру от 0 до 7.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    // Перехватываем любые бизнес-исключения (отрицательные деньги, 0 товаров и т.д.)
                    PrintError($"Ошибка: {ex.Message}");
                }
            }
        }

        // --- ИНТЕРАКТИВНОЕ ДОБАВЛЕНИЕ ТОВАРОВ ---

        static void AddPhysicalProductInteractive(Order order)
        {
            Console.WriteLine("--- Добавление физического товара ---");
            string name = ReadString("Введите название товара: ");
            decimal priceAmount = ReadDecimal("Введите цену (₽): ");
            decimal weight = ReadDecimal("Введите вес (кг, например 0,5): ");

            Product product = new PhysicalProduct(name, new Money(priceAmount), weight);
            AddProductToOrderWithQuantityChoice(order, product);
        }

        static void AddDigitalProductInteractive(Order order)
        {
            Console.WriteLine("--- Добавление цифрового товара ---");
            string name = ReadString("Введите название цифрового товара: ");
            decimal priceAmount = ReadDecimal("Введите цену (₽): ");
            double sizeMb = (double)ReadDecimal("Введите размер файла (МБ): ");

            Product product = new DigitalProduct(name, new Money(priceAmount), sizeMb);
            AddProductToOrderWithQuantityChoice(order, product);
        }

        static void AddSubscriptionInteractive(Order order)
        {
            Console.WriteLine("--- Добавление подписки (от 12 мес. скидка 10%) ---");
            string name = ReadString("Введите название подписки: ");
            decimal monthlyPrice = ReadDecimal("Введите стоимость за 1 месяц (₽): ");
            int months = ReadInt("Введите длительность (в месяцах): ");

            Product product = new SubscriptionProduct(name, new Money(monthlyPrice), months);
            AddProductToOrderWithQuantityChoice(order, product);
        }

        static void AddProductToOrderWithQuantityChoice(Order order, Product product)
        {
            Console.Write("Введите количество (или просто нажмите Enter для 1 шт.): ");
            string input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input))
            {
                // Используем первую перегрузку AddProduct(Product)
                order.AddProduct(product);
                PrintSuccess($"Товар «{product.Name}» (1 шт.) успешно добавлен в заказ!");
            }
            else
            {
                if (!int.TryParse(input, out int quantity))
                    throw new ArgumentException("Количество должно быть целым числом.");

                // Используем вторую перегрузку AddProduct(Product, int)
                order.AddProduct(product, quantity);
                PrintSuccess($"Товар «{product.Name}» ({quantity} шт.) успешно добавлен в заказ!");
            }
        }

        // --- ПРОСМОТР КОРЗИНЫ ---

        static void PrintOrder(Order order)
        {
            if (order.Items.Count == 0)
            {
                Console.WriteLine("Корзина пока пуста.");
                return;
            }

            Console.WriteLine("Состав текущего заказа:");
            foreach (var item in order.Items)
            {
                Console.WriteLine($"- {item.Product.Name} | Кол-во: {item.Quantity} шт. | Цена за ед.: {item.Product.GetUnitPrice()} | Итого: {item.GetTotalPrice()}");
            }
            Console.WriteLine($"Общий вес физических товаров: {order.CalculateTotalWeight()} кг");
            Console.WriteLine($"Предварительная сумма товаров: {order.CalculateTotalGoodsPrice()}");
        }

        // --- ИНТЕРАКТИВНОЕ ОФОРМЛЕНИЕ И ОПЛАТА ---

        static void CheckoutInteractive(Order order, CheckoutService checkoutService)
        {
            if (order.Items.Count == 0)
            {
                // Вызываем сервис, чтобы он сам выбросил положенное по ТЗ исключение о пустом заказе
                checkoutService.CalculateOrder(order, new NoDiscount(), new DigitalDelivery());
                return;
            }

            PrintOrder(order);
            Console.WriteLine();

            // 1. Выбор скидки
            DiscountPolicy discount = ChooseDiscountInteractive();

            // 2. Выбор способа доставки (с повтором, если выбрана неподходящая доставка)
            CheckoutResult result = null;
            DeliveryMethod delivery = null;

            while (result == null)
            {
                try
                {
                    delivery = ChooseDeliveryInteractive();
                    result = checkoutService.CalculateOrder(order, discount, delivery);
                }
                catch (Exception ex)
                {
                    PrintError($"Ошибка при расчёте заказа: {ex.Message}");
                    Console.Write("Попробовать выбрать другой способ доставки/скидки? (1 - Да, 0 - Отмена оформления): ");
                    if (Console.ReadLine()?.Trim() != "1") return;
                    discount = ChooseDiscountInteractive();
                }
            }

            // 3. Вывод чека
            Console.WriteLine("\n--- ИТОГОВЫЙ РАСЧЁТ ЗАКАЗА ---");
            Console.WriteLine($"Стоимость товаров:  {result.GoodsTotal}");
            Console.WriteLine($"Размер скидки:     -{result.DiscountAmount}");
            Console.WriteLine($"Доставка ({delivery.Name}): +{result.DeliveryCost}");
            Console.WriteLine(new string('-', 40));
            Console.WriteLine($"ИТОГО К ОПЛАТЕ:     {result.FinalTotal}\n");

            // 4. Оплата (с повтором, если не хватило наличных)
            bool paid = false;
            while (!paid)
            {
                try
                {
                    IPaymentMethod payment = ChoosePaymentInteractive();
                    payment.Pay(result.FinalTotal);
                    paid = true;
                    PrintSuccess("Заказ успешно оплачен и оформлен!");
                    order = new Order(); // Очищаем корзину после покупки
                }
                catch (Exception ex)
                {
                    PrintError($"Ошибка оплаты: {ex.Message}");
                    Console.Write("Попробовать оплатить еще раз? (1 - Да, 0 - Отмена): ");
                    if (Console.ReadLine()?.Trim() != "1") return;
                }
            }
        }

        static DiscountPolicy ChooseDiscountInteractive()
        {
            while (true)
            {
                try
                {
                    Console.WriteLine("Выберите тип скидки:");
                    Console.WriteLine("1. Без скидки (NoDiscount)");
                    Console.WriteLine("2. Процентная скидка (PercentageDiscount)");
                    Console.WriteLine("3. Фиксированная скидка в рублях (FixedDiscount)");
                    Console.Write("Ваш выбор: ");
                    string choice = Console.ReadLine()?.Trim();

                    switch (choice)
                    {
                        case "1":
                            return new NoDiscount();
                        case "2":
                            decimal percent = ReadDecimal("Введите процент скидки (0 - 100): ");
                            return new PercentageDiscount(percent);
                        case "3":
                            decimal fixedSum = ReadDecimal("Введите сумму фиксированной скидки (₽): ");
                            return new FixedDiscount(new Money(fixedSum));
                        default:
                            PrintError("Выберите пункт 1, 2 или 3.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    PrintError($"Ошибка выбора скидки: {ex.Message}");
                }
            }
        }

        static DeliveryMethod ChooseDeliveryInteractive()
        {
            while (true)
            {
                Console.WriteLine("\nВыберите способ доставки:");
                Console.WriteLine("1. Курьерская доставка (300 ₽ + 80 ₽/кг) — только при наличии физ. товаров");
                Console.WriteLine("2. Самовывоз (Бесплатно) — только при наличии физ. товаров");
                Console.WriteLine("3. Цифровая доставка (Бесплатно) — только для заказов БЕЗ физ. товаров");
                Console.Write("Ваш выбор: ");
                string choice = Console.ReadLine()?.Trim();

                switch (choice)
                {
                    case "1": return new CourierDelivery();
                    case "2": return new PickupDelivery();
                    case "3": return new DigitalDelivery();
                    default:
                        PrintError("Выберите пункт 1, 2 или 3.");
                        break;
                }
            }
        }

        static IPaymentMethod ChoosePaymentInteractive()
        {
            while (true)
            {
                try
                {
                    Console.WriteLine("Выберите способ оплаты:");
                    Console.WriteLine("1. Банковской картой (CardPayment)");
                    Console.WriteLine("2. Наличными (CashPayment)");
                    Console.Write("Ваш выбор: ");
                    string choice = Console.ReadLine()?.Trim();

                    switch (choice)
                    {
                        case "1":
                            string card = ReadString("Введите номер карты (например, 2202 **** 8491): ");
                            return new CardPayment(card);
                        case "2":
                            decimal cash = ReadDecimal("Введите сумму вносимых наличных (₽): ");
                            return new CashPayment(new Money(cash));
                        default:
                            PrintError("Выберите пункт 1 или 2.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    PrintError($"Ошибка: {ex.Message}");
                }
            }
        }

        // --- ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ВВОДА С ПРОВЕРКОЙ ---

        static string ReadString(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine()?.Trim();
                if (!string.IsNullOrWhiteSpace(input))
                    return input;

                PrintError("Поле не может быть пустым. Попробуйте снова.");
            }
        }

        static decimal ReadDecimal(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine()?.Trim().Replace('.', ',');
                if (decimal.TryParse(input, NumberStyles.Any, new CultureInfo("ru-RU"), out decimal value))
                    return value;

                PrintError("Некорректное число. Введите числовое значение (например: 1250 или 0,5).");
            }
        }

        static int ReadInt(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine()?.Trim();
                if (int.TryParse(input, out int value))
                    return value;

                PrintError("Некорректное число. Введите целое число.");
            }
        }

        static void PrintError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        static void PrintSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        // --- АВТОМАТИЧЕСКАЯ ДЕМОНСТРАЦИЯ ПО ТРЕБОВАНИЮ 8 ---

        static void RunAutoDemo()
        {
            Console.WriteLine("=== ОСНОВНОЙ СЦЕНАРИЙ ОФОРМЛЕНИЯ ЗАКАЗА (АВТО-ТЕСТ) ===\n");

            Product gamepad = new PhysicalProduct("Геймпад DualSense", new Money(6500m), 0.4m);
            Product gameKey = new DigitalProduct("Ключ F1 25 (PC)", new Money(4200m), 65000);
            Product sub = new SubscriptionProduct("Подписка EA Play Pro", new Money(1000m), 12);

            Order order = new Order();
            order.AddProduct(gamepad, 2);
            order.AddProduct(gameKey);
            order.AddProduct(sub);

            PrintOrder(order);
            Console.WriteLine();

            CheckoutService checkoutService = new CheckoutService();
            DiscountPolicy discount = new PercentageDiscount(10m);
            DeliveryMethod delivery = new CourierDelivery();

            CheckoutResult result = checkoutService.CalculateOrder(order, discount, delivery);

            Console.WriteLine("Расчёт стоимости заказа:");
            Console.WriteLine($"Стоимость товаров:  {result.GoodsTotal}");
            Console.WriteLine($"Размер скидки (10%): -{result.DiscountAmount}");
            Console.WriteLine($"Доставка ({delivery.Name}): +{result.DeliveryCost}");
            Console.WriteLine(new string('-', 40));
            Console.WriteLine($"ИТОГО К ОПЛАТЕ:     {result.FinalTotal}\n");

            IPaymentMethod cardPayment = new CardPayment("2202 20** **** 8491");
            cardPayment.Pay(result.FinalTotal);

            IPaymentMethod cashPayment = new CashPayment(new Money(30000m));
            cashPayment.Pay(result.FinalTotal);

            Console.WriteLine("\n=== ПРОВЕРКА ОБРАБОТКИ НЕКОРРЕКТНЫХ СИТУАЦИЙ ===\n");

            try
            {
                Console.Write("1. Попытка добавить товар в количестве 0 шт.: ");
                order.AddProduct(gamepad, 0);
            }
            catch (Exception ex) { PrintError($"Перехвачена ошибка -> {ex.Message}"); }

            try
            {
                Console.Write("2. Попытка оформить пустой заказ: ");
                Order emptyOrder = new Order();
                checkoutService.CalculateOrder(emptyOrder, new NoDiscount(), new DigitalDelivery());
            }
            catch (Exception ex) { PrintError($"Перехвачена ошибка -> {ex.Message}"); }

            try
            {
                Console.Write("3. Вызов цифровой доставки для заказа с физическим товаром: ");
                checkoutService.CalculateOrder(order, discount, new DigitalDelivery());
            }
            catch (Exception ex) { PrintError($"Перехвачена ошибка -> {ex.Message}"); }

            try
            {
                Console.Write("4. Попытка оплатить заказ наличными, когда денег не хватает: ");
                IPaymentMethod poorCash = new CashPayment(new Money(1000m));
                poorCash.Pay(result.FinalTotal);
            }
            catch (Exception ex) { PrintError($"Перехвачена ошибка -> {ex.Message}"); }
        }
    }
}