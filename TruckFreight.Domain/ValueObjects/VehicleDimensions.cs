namespace TruckFreight.Domain.ValueObjects
{
    public class VehicleDimensions : IEquatable<VehicleDimensions>
    {
        public double Length { get; private set; }  // meters
        public double Width { get; private set; }   // meters  
        public double Height { get; private set; }  // meters
        public double Weight { get; private set; }  // tons

        protected VehicleDimensions() { }

        public VehicleDimensions(double length, double width, double height, double weight)
        {
            if (length <= 0) throw new ArgumentException("Length must be positive", nameof(length));
            if (width <= 0) throw new ArgumentException("Width must be positive", nameof(width));
            if (height <= 0) throw new ArgumentException("Height must be positive", nameof(height));
            if (weight <= 0) throw new ArgumentException("Weight must be positive", nameof(weight));

            Length = length;
            Width = width;
            Height = height;
            Weight = weight;
        }

        public double GetVolume() => Length * Width * Height;

        public bool CanCarry(VehicleDimensions cargo)
        {
            return Length >= cargo.Length &&
                   Width >= cargo.Width &&
                   Height >= cargo.Height &&
                   Weight >= cargo.Weight;
        }

        public string GetDisplayDimensions()
        {
            return $"{Length:F1}m × {Width:F1}m × {Height:F1}m ({Weight:F1}t)";
        }

        public bool Equals(VehicleDimensions other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return Math.Abs(Length - other.Length) < 0.01 &&
                   Math.Abs(Width - other.Width) < 0.01 &&
                   Math.Abs(Height - other.Height) < 0.01 &&
                   Math.Abs(Weight - other.Weight) < 0.01;
        }

        public override bool Equals(object obj) => Equals(obj as VehicleDimensions);

        public override int GetHashCode() => HashCode.Combine(Length, Width, Height, Weight);

        public static bool operator ==(VehicleDimensions left, VehicleDimensions right) => 
            ReferenceEquals(left, right) || (left?.Equals(right) ?? false);

        public static bool operator !=(VehicleDimensions left, VehicleDimensions right) => !(left == right);
    }
}
