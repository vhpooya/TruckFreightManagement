using System;
using System.Collections.Generic;

namespace TruckFreight.Application.Features.Settings.DTOs
{
    public class SystemSettingsDto
    {
        public string Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string DataType { get; set; }
        public bool IsEncrypted { get; set; }
        public bool IsReadOnly { get; set; }
        public string ValidationRules { get; set; }
        public string DefaultValue { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
    }

    public class UpdateSystemSettingsDto
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string DataType { get; set; }
        public bool IsEncrypted { get; set; }
        public bool IsReadOnly { get; set; }
        public string ValidationRules { get; set; }
        public string DefaultValue { get; set; }
    }

    public class SystemSettingsListDto
    {
        public List<SystemSettingsDto> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class SystemSettingsFilterDto
    {
        public string SearchTerm { get; set; }
        public string Category { get; set; }
        public string DataType { get; set; }
        public bool? IsEncrypted { get; set; }
        public bool? IsReadOnly { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; }
        public bool SortDescending { get; set; }
    }

    public class SystemSettingsCategoryDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int SettingsCount { get; set; }
        public List<SystemSettingsDto> Settings { get; set; }
    }

    public class SystemSettingsSummaryDto
    {
        public int TotalSettings { get; set; }
        public int EncryptedSettings { get; set; }
        public int ReadOnlySettings { get; set; }
        public Dictionary<string, int> SettingsByCategory { get; set; }
        public Dictionary<string, int> SettingsByDataType { get; set; }
        public List<SystemSettingsCategoryDto> Categories { get; set; }
    }

    // Predefined setting categories
    public static class SettingCategories
    {
        public const string General = "General";
        public const string Security = "Security";
        public const string Payment = "Payment";
        public const string Notification = "Notification";
        public const string Integration = "Integration";
        public const string Maintenance = "Maintenance";
        public const string Reporting = "Reporting";
        public const string System = "System";
    }

    // Predefined data types
    public static class SettingDataTypes
    {
        public const string String = "String";
        public const string Number = "Number";
        public const string Boolean = "Boolean";
        public const string DateTime = "DateTime";
        public const string Json = "Json";
        public const string Array = "Array";
        public const string Object = "Object";
    }

    // Common validation rules
    public static class SettingValidationRules
    {
        public const string Required = "required";
        public const string Email = "email";
        public const string Url = "url";
        public const string PhoneNumber = "phone";
        public const string MinLength = "minLength:{0}";
        public const string MaxLength = "maxLength:{0}";
        public const string MinValue = "minValue:{0}";
        public const string MaxValue = "maxValue:{0}";
        public const string Pattern = "pattern:{0}";
        public const string Enum = "enum:{0}";
    }
} 