using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Application.Features.Payments.DTOs;

namespace TruckFreight.Application.Features.Payments.Queries.GetTransactions
{
    public class GetTransactionsQuery : IRequest<PaginatedList<TransactionDto>>
    {
        public string UserId { get; set; }
        public string Status { get; set; }
        public string Gateway { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetTransactionsQueryValidator : AbstractValidator<GetTransactionsQuery>
    {
        public GetTransactionsQueryValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("شناسه کاربر الزامی است");

            RuleFor(x => x.Status)
                .Must(x => string.IsNullOrEmpty(x) || x == "Pending" || x == "Completed" || x == "Failed" || x == "Refunded")
                .WithMessage("وضعیت نامعتبر است");

            RuleFor(x => x.Gateway)
                .Must(x => string.IsNullOrEmpty(x) || x == "Zarinpal" || x == "NextPay" || x == "Mellat")
                .WithMessage("درگاه پرداخت نامعتبر است");

            RuleFor(x => x.FromDate)
                .Must(x => !x.HasValue || x.Value <= DateTime.UtcNow)
                .WithMessage("تاریخ شروع نامعتبر است");

            RuleFor(x => x.ToDate)
                .Must(x => !x.HasValue || x.Value <= DateTime.UtcNow)
                .WithMessage("تاریخ پایان نامعتبر است")
                .Must((query, x) => !x.HasValue || !query.FromDate.HasValue || x.Value >= query.FromDate.Value)
                .WithMessage("تاریخ پایان باید بعد از تاریخ شروع باشد");

            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage("شماره صفحه باید بزرگتر از صفر باشد");

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .WithMessage("تعداد آیتم در صفحه باید بزرگتر از صفر باشد")
                .LessThanOrEqualTo(100)
                .WithMessage("تعداد آیتم در صفحه نمی‌تواند بیشتر از 100 باشد");
        }
    }

    public class GetTransactionsQueryHandler : IRequestHandler<GetTransactionsQuery, PaginatedList<TransactionDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetTransactionsQueryHandler> _logger;

        public GetTransactionsQueryHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<GetTransactionsQueryHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<PaginatedList<TransactionDto>> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogError("User not found");
                    return new PaginatedList<TransactionDto>(new List<TransactionDto>(), 0, request.PageNumber, request.PageSize);
                }

                var query = _context.Payments
                    .Where(x => x.UserId == request.UserId);

                if (!string.IsNullOrEmpty(request.Status))
                {
                    query = query.Where(x => x.Status.ToString() == request.Status);
                }

                if (!string.IsNullOrEmpty(request.Gateway))
                {
                    query = query.Where(x => x.Gateway == request.Gateway);
                }

                if (request.FromDate.HasValue)
                {
                    query = query.Where(x => x.CreatedAt >= request.FromDate.Value);
                }

                if (request.ToDate.HasValue)
                {
                    query = query.Where(x => x.CreatedAt <= request.ToDate.Value);
                }

                var transactions = await query
                    .OrderByDescending(x => x.CreatedAt)
                    .Select(x => new TransactionDto
                    {
                        Id = x.Id,
                        UserId = x.UserId,
                        Amount = x.Amount,
                        Status = x.Status.ToString(),
                        Gateway = x.Gateway,
                        Authority = x.GatewayToken,
                        ReferenceId = x.ReferenceId,
                        Description = x.Description,
                        CreatedAt = x.CreatedAt,
                        UpdatedAt = x.UpdatedAt,
                        PaidAt = x.PaidAt
                    })
                    .ToListAsync(cancellationToken);

                return await PaginatedList<TransactionDto>.CreateAsync(
                    transactions.AsQueryable(),
                    request.PageNumber,
                    request.PageSize);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transactions");
                return new PaginatedList<TransactionDto>(new List<TransactionDto>(), 0, request.PageNumber, request.PageSize);
            }
        }
    }
} 