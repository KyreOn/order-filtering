using System.Globalization;
using System.Text.Json.Nodes;

namespace order_filtering;

class Program
{
    private static string _cityDistrict;
    private static DateTime _firstDeliveryDateTime = DateTime.Now;
    private static string _deliveryLog;
    private static string _deliveryOrder;
    
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
    }

    #region DataInput

    static void SetFieldsFromArgs(string[] args)
    {
        if (!ValidateDistrict(args[0]) || !ValidateDate(args[1]) ||
            !ValidatePath(args[2]) || !ValidatePath(args[3]))
        {
            Console.WriteLine("Неверные параметры. Проверьте правильность введенных данных, либо запустите программу без параметров");
            Environment.Exit(1);
        }
            
        _cityDistrict = args[0];
        _firstDeliveryDateTime = DateTime.ParseExact(args[1], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        _deliveryLog = args[2];
        _deliveryOrder = args[3];
    }

    static void InputDistrict()
    {
        while (true)
        {
            var district = Console.ReadLine();
            if (ValidateDistrict(district))
                _cityDistrict = district;
            else
            {
                Console.WriteLine("Район доставки введен неверно, проверьте правильность введеных данных и повторите попытку:");
                continue;
            }

            break;
        }
    }

    static void InputDate()
    {
        while (true)
        {
            var date = Console.ReadLine();
            if (ValidateDate(date))
                _firstDeliveryDateTime = DateTime.ParseExact(date, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            else
            {
                Console.WriteLine("Дата доставки введена неверно, проверьте правильность введеных данных и повторите попытку:");
                continue;
            }

            break;
        }
    }

    static void InputLogPath()
    {
        while (true)
        {
            var logPath = Console.ReadLine();
            if (ValidatePath(logPath))
                _deliveryLog = logPath;
            else
            {
                Console.WriteLine("Путь к файлу введен неверно, проверьте правильность введеных данных и повторите попытку:");
                continue;
            }

            break;
        }
    }

    static void InputOrderPath()
    {
        while (true)
        {
            var orderPath = Console.ReadLine();
            if (ValidatePath(orderPath))
                _deliveryOrder = orderPath;
            else
            {
                Console.WriteLine("Путь к файлу введен неверно, проверьте правильность введеных данных и повторите попытку:");
                continue;
            }

            break;
        }
    }

    #endregion
    
    #region Validation

    static bool ValidateDistrict(string district) => !string.IsNullOrEmpty(district);

    static bool ValidateDate(string date) => 
        DateTime.TryParseExact(date, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);

    static bool ValidatePath(string path) => Path.Exists(path);

    #endregion
}