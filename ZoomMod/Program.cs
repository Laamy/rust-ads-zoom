namespace ZoomMod;

using System;
using System.Reflection;

class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        LogNote("Zoom client activated");
        Console.WriteLine("Type 'help' to list commands");

        AppEngine.Run();

        while (true)
        {
            Console.Write("> ");
            var input = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(input))
                continue;

            var parts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var command = parts[0].ToLower();
            var value = parts.Length > 1 ? parts[1] : null;

            if (command == "exit") break;

            switch (command)
            {
                case "help":
                    ShowHelp();
                    break;

                default:
                    if (!TryUpdateConfig(command, value))
                        Console.WriteLine($"Unknown command: {command}");
                    break;
            }
        }
    }

    static void ShowHelp()
    {
        Console.WriteLine("Available commands:");

        var configFields = typeof(Config)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.IsDefined(typeof(AliasAttribute), false));    

        foreach (var field in configFields)
        {
            var aliases = field.GetCustomAttribute<AliasAttribute>().Aliases;
            Console.Write($"  {field.Name.ToLower()} <value, {field.FieldType.Name}>: ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{field.GetValue(null)}");
            Console.ResetColor();
            Console.WriteLine($" (aliases: {string.Join(", ", aliases)})");
        }

        Console.WriteLine("  help - Show this help text");
        Console.WriteLine("  exit - Exit the program");
    }

    static bool TryUpdateConfig(string fieldName, string valueStr)
    {
        var configFields = typeof(Config)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.IsDefined(typeof(AliasAttribute), false));

        foreach (var field in configFields)
        {
            var aliases = field.GetCustomAttribute<AliasAttribute>().Aliases;
            if (aliases.Contains(fieldName))
            {
                try
                {
                    var fieldType = field.FieldType;
                    object newValue;
                    if (fieldType == typeof(float))
                    {
                        newValue = float.Parse(valueStr);
                    }
                    else if (fieldType == typeof(int))
                    {
                        newValue = int.Parse(valueStr);
                    }
                    else if (fieldType == typeof(bool))
                    {
                        newValue = bool.Parse(valueStr);
                    }
                    else
                    {
                        Console.WriteLine($"Unsupported type: {fieldType.Name}");
                        return false;
                    }
                    field.SetValue(null, newValue);
                    LogNote($"Updated {field.Name} to {newValue}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating {field.Name}: {ex.Message}");
                }
                return true;
            }
        }

        Console.WriteLine($"Unknown field: {fieldName}");

        return true;
    }

    private static void LogNote(string note)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {note}");
    }
}
