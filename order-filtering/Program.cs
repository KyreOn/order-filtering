using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace order_filtering;

class Program
{
    static string _cityDistrict;
    static DateTime _firstDeliveryDateTime;
    static FileStream _deliveryLog;
    static FileStream _deliveryOrder;
    
    static void Main(string[] args)
    {
        Setup();
        Log("Пути к файлам логов и результатов найдены.");
        if (args.Length == 0)
        {
            Log("Получение параметров фильтрации...");
            Console.WriteLine("Введите район доставки:");
            InputDistrict();
            Console.WriteLine("Введите дату первой доставки в формате \"yyyy-MM-dd HH:mm:ss\":");
            InputDate();
        }
        else
            SetFieldsFromArgs(args);
        Log($"Район доставки: {_cityDistrict}, Дата первой доставки: {_firstDeliveryDateTime}.");
        Log("Фильтрация данных...");
        var queryResult = Query();
        Log($"Найдено {queryResult.Count()} подходящих записей.");
        Log("Сохранение записей...");
        PrintResult(queryResult);
        Log("Программа завершена успешно.");
        _deliveryOrder.Close();
        _deliveryLog.Close();
    }
    
    #region Setup

    static void Setup()
    {
        using var settings = File.Open("settings.json", FileMode.OpenOrCreate, FileAccess.ReadWrite);
        var jsonSettings = JsonNode.Parse(settings);
        SetPath(jsonSettings,"deliveryLog", out _deliveryLog);
        SetPath(jsonSettings,"deliveryOrder", out _deliveryOrder);
    }

    static void SetPath(JsonNode settings, string fileName, out FileStream file)
    {
        if (ValidatePath(settings[fileName].ToString(), out file)) return;
        Console.WriteLine($"Путь к файлу {fileName} в файле конфигурации settings.json указан неверно. Хотите установить путь вручную (Y/n)?");
        ManualPathSetup(out file);
    }

    static void ManualPathSetup(out FileStream file)
    {
        while (true)
        {
            var manualSetup = Console.ReadLine();
            switch (manualSetup.ToLower())
            {
                case "y":
                {
                    InputPath(out file);
                    break;
                }
                case "n":
                {
                    file = null;
                    Environment.Exit(1);
                    break;
                }
                default:
                {
                    Console.WriteLine("Введено неверное значение. Введите одно из доступных значений (Y/n):");
                    continue;
                }
            }

            break;
        }
    }
    
    static void InputPath(out FileStream file)
    {
        while (true)
        {
            var path = Console.ReadLine();
            if (ValidatePath(path, out file))
                break;
            Console.WriteLine("Путь к файлу введен неверно, проверьте правильность введеных данных и повторите попытку:");
        }
    }

    static void SetFieldsFromArgs(string[] args)
    {
        Log("Программа запущена с параметрами. Проверка...");
        if (!ValidateDistrict(args[0], out _cityDistrict) || !ValidateDate(args[1], out _firstDeliveryDateTime))
        {
            Console.WriteLine("Неверные параметры. Проверьте правильность введенных данных, либо запустите программу без параметров");
            Environment.Exit(1);
            Log("Один или несколько параметров введены неверно.");
            Log("Программа завершена неудачно.");
        }
    }

    static void InputDistrict()
    {
        while (true)
        {
            var district = Console.ReadLine();
            if (ValidateDistrict(district, out _cityDistrict))
                break;
            Console.WriteLine("Район доставки введен неверно, проверьте правильность введеных данных и повторите попытку:");
        }
    }

    static void InputDate()
    {
        while (true)
        {
            var date = Console.ReadLine();
            if (ValidateDate(date, out _firstDeliveryDateTime))
                break;
            Console.WriteLine("Дата доставки введена неверно, проверьте правильность введеных данных и повторите попытку:");
        }
    }
    
    #endregion
    
    #region Validation

    static bool ValidateDistrict(string district, out string districtName)
    {
        districtName = district;
        return !string.IsNullOrEmpty(district);
    }

    static bool ValidateDate(string date, out DateTime dateTime) => 
        DateTime.TryParseExact(date, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);

    static bool ValidatePath(string path, out FileStream file)
    {
        try
        {
            file = File.Create(path);
            return true;
        }
        catch (Exception)
        {
            file = null;
            return false;
        }
    }
    
    #endregion

    #region Query
    
    static IEnumerable<JsonNode> Query()
    {
        var json = JsonNode.Parse(File.ReadAllText("test-data.json"));
        var result = json.AsArray()
            .Where(x => CheckEntry(x["district"].ToString(), x["deliveryDateTime"].ToString()));
        return result;
    }

    static bool CheckEntry(string district, string data)
    {
        var diff = DateTime.ParseExact(data, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) - _firstDeliveryDateTime;
        return diff.TotalSeconds is > 0 and <= 1800 && district == _cityDistrict;
    }

    static void PrintResult(IEnumerable<JsonNode> result)
    {
        foreach (var entry in result)
        {
            var text = $"Номер заказа: {entry["id"]}, Вес заказа(кг): {entry["weight"]}, Район заказа: {entry["district"]}, Дата доставки: {entry["deliveryDateTime"]}\n";
            var bytes = Encoding.UTF8.GetBytes(text);
            _deliveryOrder.Write(bytes);
        }
    }
    
    #endregion

    static void Log(string message)
    {
        _deliveryLog.Write(Encoding.UTF8.GetBytes(message + "\n"));
    }
}