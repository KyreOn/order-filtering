using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace order_filtering;

class Program
{
    private static string _cityDistrict;
    private static DateTime _firstDeliveryDateTime = DateTime.Now;
    private static FileStream _deliveryLog;
    private static FileStream _deliveryOrder;
    
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Введите район доставки:");
            InputDistrict();
            Console.WriteLine("Введите дату первой доставки:");
            InputDate();
            Console.WriteLine("Введите путь к файлу логов:");
            InputLogPath();
            Console.WriteLine("Введите путь к файлу с результатами фильтрации:");
            InputOrderPath();
        }
        else
            SetFieldsFromArgs(args);
        var queryResult = Query();
        PrintResult(queryResult);
        _deliveryOrder.Close();
        _deliveryLog.Close();
    }

    #region DataInput

    static void SetFieldsFromArgs(string[] args)
    {
        if (!ValidateDistrict(args[0], out _cityDistrict) || !ValidateDate(args[1], out _firstDeliveryDateTime) ||
            !ValidatePath(args[2], out _deliveryLog) || !ValidatePath(args[3], out _deliveryOrder))
        {
            Console.WriteLine("Неверные параметры. Проверьте правильность введенных данных, либо запустите программу без параметров");
            Environment.Exit(1);
        }
            
        _cityDistrict = args[0];
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

    static void InputLogPath()
    {
        while (true)
        {
            var logPath = Console.ReadLine();
            if (ValidatePath(logPath, out _deliveryLog))
                break;
            Console.WriteLine("Путь к файлу введен неверно, проверьте правильность введеных данных и повторите попытку:");
        }
    }

    static void InputOrderPath()
    {
        while (true)
        {
            var orderPath = Console.ReadLine();
            if (ValidatePath(orderPath, out _deliveryOrder))
                break;
            Console.WriteLine("Путь к файлу введен неверно, проверьте правильность введеных данных и повторите попытку:");
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
            .Where(x => CheckEntry(x["district"].ToString(), x["deliveryDateTime"].ToString()))
            .Select(x => x);
        return result;
    }

    static bool CheckEntry(string district, string data)
    {
        var diff = DateTime.ParseExact(data, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) - _firstDeliveryDateTime;
        return diff.TotalSeconds is > 0 and <= 18000 && district == _cityDistrict;
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
}