using System.Reflection;
using order_filtering;

namespace order_filtering_test;

public class Tests
{
    [SetUp]
    public void Setup()
    {
        OrderFilter._cityDistrict = "Ленинский";
        OrderFilter._firstDeliveryDateTime = DateTime.Parse("2024-10-25 11:00:00");
    }

    [TestCase(null)]
    [TestCase("")]
    public void ValidateDistrict_NullOrEmpty(string value)
    {
        Assert.That(OrderFilter.ValidateDistrict(value, out _), Is.False);
    }
    
    [Test]
    public void ValidateDistrict_Default()
    {
        Assert.That(OrderFilter.ValidateDistrict("Центральный", out _), Is.True);
    }

    [TestCase(null)]
    [TestCase("")]
    public void ValidateDate_NullOrEmpty(string value)
    {
        Assert.That(OrderFilter.ValidateDate(value, out _), Is.False);
    }
    
    [TestCase("2024-12-12")]
    [TestCase("2024/12/12 12:30:00")]
    [TestCase("2024.12.12 12:30:00")]
    [TestCase("12/12/2024 12:30:00")]
    [TestCase("12:30:30 2024-12-12")]
    public void ValidateDate_WrongFormat(string value)
    {
        Assert.That(OrderFilter.ValidateDate(value, out _), Is.False);
    }
    
    [TestCase("2024-13-12 12:30:30")]
    [TestCase("2024-12-32 12:30:30")]
    [TestCase("2024-12-12 24:30:30")]
    [TestCase("2024-12-12 12:61:30")]
    [TestCase("2024-12-12 12:30:61")]
    public void ValidateDate_Overflow(string value)
    {
        Assert.That(OrderFilter.ValidateDate(value, out _), Is.False);
    }
    
    [Test]
    public void ValidateDate_Default()
    {
        Assert.That(OrderFilter.ValidateDate("2024-12-12 12:30:00", out _), Is.True);
    }

    [TestCase(null)]
    [TestCase("")]
    public void ValidatePath_NullOrEmpty(string value)
    {
        Assert.That(OrderFilter.ValidatePath(value, out _), Is.False);
    }
    
    [TestCase("./test1.txt")]
    [TestCase("test2.txt")]
    public void ValidatePath_RelativePath(string value)
    {
        Assert.That(OrderFilter.ValidatePath(value, out var file), Is.True);
        file.Close();
        File.Delete(value);
    }
    
    [Test]
    public void ValidatePath_AbsolutePath()
    {
        var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "test3.txt");
        Assert.That(OrderFilter.ValidatePath(path, out var file), Is.True);
        file.Close();
        File.Delete(path);
    }

    [TestCase("Центральный")]
    [TestCase("Индустриальный")]
    [TestCase("Академический")]
    [TestCase("Кировский")]
    public void CheckEntry_WrongDistrict(string district)
    {
        Assert.That(OrderFilter.CheckEntry(district, "2024-10-25 11:00:10"), Is.False);
    }
    
    [TestCase("2024-10-25 10:59:59")]
    [TestCase("2024-10-25 11:30:01")]
    [TestCase("2024-10-26 11:00:10")]
    [TestCase("2024-11-25 11:00:10")]
    [TestCase("2026-10-25 11:00:10")]
    public void CheckEntry_WrongDate(string date)
    {
        Assert.That(OrderFilter.CheckEntry("Ленинский", date), Is.False);
    }
    
    [Test]
    public void CheckEntry_Default()
    {
        Assert.That(OrderFilter.CheckEntry("Ленинский", "2024-10-25 11:00:10"), Is.True);
    }
}