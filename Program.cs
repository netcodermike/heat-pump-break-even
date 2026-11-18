using System.Text.Json;

try
{
    if (args.Length != 1) throw new ArgumentException("Pass one JSON input file");
    using var data = JsonDocument.Parse(File.ReadAllText(args[0]));
    Console.WriteLine(JsonSerializer.Serialize(Analyze(data.RootElement)));
    return 0;
}
catch (Exception error) when (error is IOException or JsonException or FormatException
                               or ArgumentException or InvalidOperationException
                               or KeyNotFoundException or OverflowException)
{
    Console.Error.WriteLine(error.Message);
    return 2;
}

static object Analyze(JsonElement input)
{
    decimal Positive(string key)
    {
        decimal value = input.GetProperty(key).GetDecimal();
        if (value <= 0) throw new ArgumentException($"{key} must be positive");
        return value;
    }
    decimal delivered = Positive("deliveredKwh");
    decimal electricity = Positive("electricPricePerKwh"), cop = Positive("heatPumpCop");
    decimal fuel = Positive("fuelPricePerKwh"), efficiency = Positive("fuelEfficiency");
    if (efficiency > 1) throw new ArgumentException("Fuel efficiency must be a fraction at most one");
    decimal electricalInput = delivered / cop, fuelInput = delivered / efficiency;
    decimal electricCost = electricalInput * electricity, fuelCost = fuelInput * fuel;
    return new { heatPumpCost = Math.Round(electricCost, 2), fuelCost = Math.Round(fuelCost, 2),
        breakEvenCop = Math.Round(electricity * efficiency / fuel, 4),
        heatPumpInputKwh = Math.Round(electricalInput, 2), fuelInputKwh = Math.Round(fuelInput, 2) };
}
