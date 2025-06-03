namespace TruckFreight.Domain.Enums
{
    public enum UnitType
    {
        // Weight Units
        Kilogram = 1,
        Gram = 2,
        Pound = 3,
        Ton = 4,
        MetricTon = 5,

        // Volume Units
        Liter = 6,
        Milliliter = 7,
        Gallon = 8,
        CubicMeter = 9,
        CubicFoot = 10,

        // Length Units
        Meter = 11,
        Centimeter = 12,
        Millimeter = 13,
        Kilometer = 14,
        Mile = 15,
        Foot = 16,
        Inch = 17,

        // Area Units
        SquareMeter = 18,
        SquareFoot = 19,
        SquareYard = 20,
        Acre = 21,
        Hectare = 22,

        // Time Units
        Second = 23,
        Minute = 24,
        Hour = 25,
        Day = 26,
        Week = 27,
        Month = 28,
        Year = 29,

        // Speed Units
        KilometerPerHour = 30,
        MilePerHour = 31,
        MeterPerSecond = 32,

        // Temperature Units
        Celsius = 33,
        Fahrenheit = 34,
        Kelvin = 35,

        // Pressure Units
        Pascal = 36,
        Bar = 37,
        PSI = 38,
        Atmosphere = 39,

        // Energy Units
        Joule = 40,
        KilowattHour = 41,
        Calorie = 42,
        BTU = 43,

        // Other
        Other = 99
    }
} 