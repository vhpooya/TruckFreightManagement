using System.Text.RegularExpressions;

namespace TruckFreight.Domain.ValueObjects
{
    public class PhoneNumber : IEquatable<PhoneNumber>
    {
        private static readonly Regex IranianMobileRegex = new Regex(@"^(\+98|0098|98|0)?9[0-9]{9}$");
        private static readonly Regex IranianLandlineRegex = new Regex(@"^(\+98|0098|98|0)?[1-8][0-9]{7,10}$");

        public string Number { get; private set; }
        public string CountryCode { get; private set; }
        public bool IsMobile { get; private set; }

        protected PhoneNumber() { }

        public PhoneNumber(string number, string countryCode = "+98")
        {
            if (string.IsNullOrWhiteSpace(number))
                throw new ArgumentException("Phone number cannot be empty", nameof(number));

            var cleanNumber = CleanNumber(number);
            
            if (!IsValidIranianNumber(cleanNumber))
                throw new ArgumentException("Invalid Iranian phone number format", nameof(number));

            Number = NormalizeNumber(cleanNumber);
            CountryCode = countryCode;
            IsMobile = IranianMobileRegex.IsMatch(cleanNumber);
        }

        private static string CleanNumber(string number)
        {
            return Regex.Replace(number, @"[\s\-\(\)]", "");
        }

        private static bool IsValidIranianNumber(string number)
        {
            return IranianMobileRegex.IsMatch(number) || IranianLandlineRegex.IsMatch(number);
        }

        private static string NormalizeNumber(string number)
        {
            // Remove country code prefixes and normalize to 09xxxxxxxx format for mobile
            // or 0xxxxxxxx format for landline
            if (number.StartsWith("+98"))
                number = "0" + number.Substring(3);
            else if (number.StartsWith("0098"))
                number = "0" + number.Substring(4);
            else if (number.StartsWith("98"))
                number = "0" + number.Substring(2);
            else if (!number.StartsWith("0"))
                number = "0" + number;

            return number;
        }

        public string GetInternationalFormat()
        {
            return $"{CountryCode}{Number.Substring(1)}";
        }

        public string GetDisplayFormat()
        {
            if (IsMobile && Number.Length == 11)
            {
                return $"{Number.Substring(0, 4)} {Number.Substring(4, 3)} {Number.Substring(7, 4)}";
            }
            return Number;
        }

        public bool Equals(PhoneNumber other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return Number == other.Number && CountryCode == other.CountryCode;
        }

        public override bool Equals(object obj) => Equals(obj as PhoneNumber);

        public override int GetHashCode() => HashCode.Combine(Number, CountryCode);

        public static bool operator ==(PhoneNumber left, PhoneNumber right) => 
            ReferenceEquals(left, right) || (left?.Equals(right) ?? false);

        public static bool operator !=(PhoneNumber left, PhoneNumber right) => !(left == right);

        public override string ToString() => GetDisplayFormat();
    }
}
