using System.Globalization;

namespace Pos.ConsoleUi;

public static class Input
{
    public static string ReadLine(string prompt)
    {
        Console.Write(prompt + ": ");
        var line = Console.ReadLine();
        if (line == null)
            Environment.Exit(0);
        return line.Trim();
    }

    public static string ReadRequired(string prompt)
    {
        while (true)
        {
            var value = ReadLine(prompt);
            if (value != "")
                return value;
            Console.WriteLine("Поле не может быть пустым.");
        }
    }

    public static decimal ReadDecimal(string prompt)
    {
        while (true)
        {
            var text = ReadLine(prompt).Replace(',', '.');
            if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
                return value;
            Console.WriteLine("Введите число, например 7 или 7.5");
        }
    }

    public static T? Choose<T>(string title, IReadOnlyList<T> items, Func<T, string> toText) where T : class
    {
        if (items.Count == 0)
        {
            Console.WriteLine("Список пуст.");
            return null;
        }

        Console.WriteLine(title);
        for (var i = 0; i < items.Count; i++)
            Console.WriteLine($"  {i + 1}. {toText(items[i])}");

        while (true)
        {
            var text = ReadLine("Номер (0 - отмена)");
            if (int.TryParse(text, out var n) && n >= 0 && n <= items.Count)
                return n == 0 ? null : items[n - 1];
            Console.WriteLine("Нет такого номера.");
        }
    }
}
