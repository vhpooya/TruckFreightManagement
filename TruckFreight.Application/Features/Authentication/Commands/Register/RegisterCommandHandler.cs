using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Models;
using TruckFreight.Application.Features.Authentication.Commands.Register;
using TruckFreight.Application.Features.Authentication.Models;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Domain.Entities;
using TruckFreight.Domain.Enums;
using TruckFreight.Domain.Interfaces;
using TruckFreight.Infrastructure.Services.Sms;

namespace TruckFreight.Application.Features.Authentication.Commands.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Envelope<AuthResponse>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IJwtService _jwtService;
        private readonly ISmsService _smsService;
        private readonly IEmailService _emailService;
        private readonly IDateTime _dateTime;
        private readonly ILogger<RegisterCommandHandler> _logger;
        private readonly IMapper _mapper;

        public RegisterCommandHandler(
            IUnitOfWork uow,
            IPasswordHasher<User> passwordHasher,
            IJwtService jwtService,
            SmsService smsService,
            IEmailService emailService,
            IDateTime dateTime,
            IMapper mapper,
            ILogger<RegisterCommandHandler> logger)
        {
            _uow = uow;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
            _smsService = smsService;
            _emailService = emailService;
            _dateTime = dateTime;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<Envelope<AuthResponse>> Handle(RegisterCommand command, CancellationToken cancellationToken)
        {
            var req = command.Request;

            // Check duplicates
            if (await _uow.Users.ExistsAsync(u => u.PhoneNumber == req.PhoneNumber, cancellationToken))
                return Envelope<AuthResponse>.Failure(new[] { "Phone number already registered." }.ToList());
            if (!string.IsNullOrEmpty(req.Email) && await _uow.Users.ExistsAsync(u => u.Email == req.Email, cancellationToken))
                return Envelope<AuthResponse>.Failure(new[] { "Email already in use." }.ToList());
            if (await _uow.Users.ExistsAsync(u => u.NationalId == req.NationalId, cancellationToken))
                return Envelope<AuthResponse>.Failure(new[] { "National ID already registered." }.ToList());

            // Create user entity
            var user = new User
            {
                FirstName = req.FirstName,
                LastName = req.LastName,
                NationalId = req.NationalId,
                PhoneNumber = req.PhoneNumber,
                Email = req.Email,
                Role = req.Role,
                CreatedAt = _dateTime.Now
            };

            // Password hashing
            user.PasswordHash = _passwordHasher.HashPassword(user, req.Password);

            /