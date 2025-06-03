using TruckFreight.Domain.Enums;

namespace TruckFreight.Domain.Entities
{
    public class SystemConfiguration : BaseEntity
    {
        public string Key { get; private set; }
        public string Value { get; private set; }
        public string Description { get; private set; }
        public ConfigurationType Type { get; private set; }
        public bool IsActive { get; private set; }
        public string DataType { get; private set; }  // string, int, bool, decimal, json
        public string DefaultValue { get; private set; }
        public string ValidationRules { get; private set; }

        protected SystemConfiguration() { }

        public SystemConfiguration(string key, string value, string description, 
                                 ConfigurationType type, string dataType = "string")
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            Value = value;
            Description = description;
            Type = type;
            DataType = dataType ?? "string";
            IsActive = true;
        }

        public void UpdateValue(string value)
        {
            Value = value;
        }

        public void UpdateDescription(string description)
        {
            Description = description;
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void SetValidationRules(string rules)
        {
            ValidationRules = rules;
        }

        public T GetTypedValue<T>()
        {
            if (string.IsNullOrEmpty(Value))
                return default(T);

            try
            {
                return (T)Convert.ChangeType(Value, typeof(T));
            }
            catch
            {
                return default(T);
            }
        }
    }
}
