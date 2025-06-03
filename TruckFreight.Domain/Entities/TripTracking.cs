using System;
using TruckFreight.Domain.Common;

namespace TruckFreight.Domain.Entities
{
    public class TripTracking : BaseEntity
    {
        public Guid TripId { get; private set; }
        public string Location { get; private set; }
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }
        public double? Speed { get; private set; }
        public double? Heading { get; private set; }
        public double? Altitude { get; private set; }
        public double? Accuracy { get; private set; }
        public string DeviceId { get; private set; }
        public string DeviceType { get; private set; }
        public string DeviceModel { get; private set; }
        public string BatteryLevel { get; private set; }
        public bool IsCharging { get; private set; }
        public string NetworkType { get; private set; }
        public string NetworkOperator { get; private set; }
        public int? SignalStrength { get; private set; }
        public string Notes { get; private set; }

        // Navigation Properties
        public virtual Trip Trip { get; private set; }

        protected TripTracking() { }

        public TripTracking(
            Guid tripId,
            string location,
            double latitude,
            double longitude,
            string deviceId,
            string deviceType,
            string deviceModel)
        {
            TripId = tripId;
            Location = location ?? throw new ArgumentNullException(nameof(location));
            Latitude = latitude;
            Longitude = longitude;
            DeviceId = deviceId ?? throw new ArgumentNullException(nameof(deviceId));
            DeviceType = deviceType ?? throw new ArgumentNullException(nameof(deviceType));
            DeviceModel = deviceModel ?? throw new ArgumentNullException(nameof(deviceModel));
        }

        public void UpdateSpeed(double speed)
        {
            Speed = speed;
        }

        public void UpdateHeading(double heading)
        {
            Heading = heading;
        }

        public void UpdateAltitude(double altitude)
        {
            Altitude = altitude;
        }

        public void UpdateAccuracy(double accuracy)
        {
            Accuracy = accuracy;
        }

        public void UpdateBatteryInfo(string batteryLevel, bool isCharging)
        {
            BatteryLevel = batteryLevel;
            IsCharging = isCharging;
        }

        public void UpdateNetworkInfo(string networkType, string networkOperator, int? signalStrength)
        {
            NetworkType = networkType;
            NetworkOperator = networkOperator;
            SignalStrength = signalStrength;
        }

        public void AddNotes(string notes)
        {
            Notes = notes;
        }
    }
}
