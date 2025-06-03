using System;
using System.Collections.Generic;

namespace TruckFreight.Application.Features.Documents.DTOs
{
    public class UploadDocumentDto
    {
        public string Type { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string FileUrl { get; set; }
        public string FileType { get; set; }
        public long FileSize { get; set; }
        public string ReferenceId { get; set; }
        public string ReferenceType { get; set; }
        public DateTime ExpiryDate { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
    }

    public class DocumentDto
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string FileUrl { get; set; }
        public string FileType { get; set; }
        public long FileSize { get; set; }
        public string ReferenceId { get; set; }
        public string ReferenceType { get; set; }
        public string Status { get; set; }
        public string VerificationStatus { get; set; }
        public string VerifiedBy { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public DateTime ExpiryDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
    }

    public class DocumentVerificationDto
    {
        public string Status { get; set; }
        public string Comments { get; set; }
        public Dictionary<string, string> VerificationData { get; set; }
    }

    public class DocumentListDto
    {
        public List<DocumentDto> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class DocumentTypeDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsRequired { get; set; }
        public string ValidationRules { get; set; }
        public int ExpiryDays { get; set; }
        public List<string> AllowedFileTypes { get; set; }
        public long MaxFileSize { get; set; }
    }
} 