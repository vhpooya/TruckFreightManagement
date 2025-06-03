using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using TruckFreight.Domain.Enums;
using TruckFreight.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using TruckFreight.Domain.Entities;
using TruckFreight.Persistence.Configurations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;
using System.Linq.Expressions;
using TruckFreight.Domain.Interfaces;
using TruckFreight.Persistence.Context;
using Microsoft.EntityFrameworkCore.Storage;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Interfaces;
using System.Diagnostics;
using TruckFreight.Application.Common.Models;
using MediatRRetryPGContinueEditcsharpusing MediatR;
using MediatRetryPGContinueEditcsharpusing MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using TruckFreight.Application.Common.Behaviors;
using AutoMapper;
using TruckFreight.Application.Features.Users.Queries.GetUserProfile;
using FluentValidation.Results;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TruckFreight.Application.Features.Users.Commands.RegisterUser;
using TruckFreight.Application.Features.Users.Commands.VerifyPhone;
using TruckFreight.Application.Features.Users.Commands.LoginUser;
using TruckFreight.Application.Features.Users.Commands.UpdateUserProfile;
using TruckFreight.Application.Features.CargoRequests.Commands.CreateCargoRequest;
using TruckFreight.Application.Features.CargoRequests.Commands.PublishCargoRequest;
using TruckFreight.Application.Features.CargoRequests.Queries.GetCargoRequests;
using TruckFreight.Application.Features.CargoRequests.Queries.GetCargoRequestDetails;
using TruckFreight.Application.Features.Trips.Commands.AcceptTrip;
using TruckFreight.Application.Features.Trips.Commands.RejectTrip;
using TruckFreight.Application.Features.Trips.Commands.StartTrip;
using TruckFreight.Application.Features.Trips.Commands.UpdateTripLocation;
using TruckFreight.Application.Features.Trips.Commands.CompleteTrip;
using TruckFreight.Application.Features.Trips.Queries.GetDriverActiveTrip;
using TruckFreight.Application.Features.Trips.Queries.GetTripDetails;
using TruckFreight.Application.Features.Trips.Queries.GetDriverTrips;
using TruckFreight.Application.Features.Drivers.Commands.UpdateDriverLocation;
using TruckFreight.Application.Features.Drivers.Commands.SetDriverAvailability;
using TruckFreight.Application.Features.Drivers.Queries.GetDriverProfile;
using TruckFreight.Application.Features.Drivers.Queries.GetNearbyDrivers;
using TruckFreight.Application.Features.Vehicles.Commands.AddVehicle;
using TruckFreight.Application.Features.Vehicles.Commands.UpdateVehicle;
using TruckFreight.Application.Features.Vehicles.Commands.DeleteVehicle;
using TruckFreight.Application.Features.Vehicles.Queries.GetDriverVehicles;
using TruckFreight.Application.Features.Vehicles.Queries.GetVehicleDetails;
using TruckFreight.Application.Features.Payments.Commands.InitiatePayment;
using TruckFreight.Application.Features.Payments.Commands.VerifyPayment;
using TruckFreight.Application.Features.Payments.Queries.GetUserPayments;
using TruckFreight.Application.Features.Payments.Queries.GetPaymentDetails;
using TruckFreight.Application.Features.Wallets.Commands.AddFunds;
using TruckFreight.Application.Features.Wallets.Commands.WithdrawFunds;
using TruckFreight.Application.Features.Wallets.Queries.GetWalletBalance;
using TruckFreight.Application.Features.Wallets.Queries.GetWalletTransactions;
using TruckFreight.Application.Features.Notifications.Commands.MarkAsRead;
using TruckFreight.Application.Features.Notifications.Commands.MarkAllAsRead;
using TruckFreight.Application.Features.Notifications.Queries.GetUserNotifications;
using TruckFreight.Application.Features.Notifications.Queries.GetUnreadCount;
using TruckFreight.Application.Features.Ratings.Queries.GetUserRatings;
using TruckFreight.Application.Features.Ratings.Queries.GetTripRatings;
using TruckFreight.Application.Features.Weather.Queries.GetRouteWeather;
using TruckFreight.Application.Features.Weather.Queries.GetCurrentWeather;
using TruckFreight.Application.Features.Reports.Queries.GetDriverStatistics;
using TruckFreight.Application.Features.Reports.Queries.GetCargoOwnerStatistics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TruckFreight.Application;
using TruckFreight.Infrastructure;
using TruckFreight.Persistence;
using TruckFreight.WebAPI.Middleware;
using TruckFreight.WebAPI.Services;
using System.Security.Claims;
using System.Net;
using TruckFreight.Application.Common.Exceptions;
using TruckFreight.Application.Features.Administration.Queries.GetSystemOverview;
using TruckFreight.WebAdmin.Models;
using TruckFreight.Application.Features.Users.Queries.SearchUsers;
using TruckFreight.Application.Features.Users.Commands.ApproveUser;
using TruckFreight.Application.Features.Users.Commands.RejectUser;
using TruckFreight.WebAdmin.Models.Users;
using TruckFreight.Application.Features.Trips.Queries.SearchTrips;
using TruckFreight.Application.Features.Trips.Commands.AssignDriverToTrip;
using TruckFreight.WebAdmin.Models.Trips;
using TruckFreight.Application.Features.Payments.Queries.SearchPayments;
using TruckFreight.Application.Features.Payments.Commands.ProcessRefund;
using TruckFreight.WebAdmin.Models.Payments;
using TruckFreight.Application.Features.Administration.Queries.GetSystemConfigurations;
using TruckFreight.Application.Features.Administration.Commands.UpdateSystemConfiguration;
using TruckFreight.WebAdmin.Models.Settings;
using TruckFreight.Application.Features.Reports.Queries.GetSystemReports;
using TruckFreight.WebAdmin.Models.Reports;
using TruckFreight.Application.Features.Users.Queries.GetUserDetails;
using TruckFreight.Application.Features.Trips.Queries.GetTripTracking;
using TruckFreight.Application.Features.Documents.Commands.UploadDocument;
using TruckFreight.Application.Features.Documents.Queries.GetUserDocuments;
using TruckFreight.Application.Features.Search.Queries.GlobalSearch;
using TruckFreight.Application.Features.Location.Queries.GetNearbyServices;
using TruckFreight.Application.Features.Location.Queries.GeocodeAddress;
using TruckFreight.Application.Features.Administration.Commands.SendBroadcastNotification;
using TruckFreight.Application.Features.Administration.Queries.GetSystemLogs;
using TruckFreight.Application.Features.Documents.Queries.GetPendingDocuments;
using TruckFreight.Application.Features.Documents.Commands.ApproveDocument;
using TruckFreight.WebAdmin.Models.Documents;
using TruckFreight.Application.Features.Notifications.Commands.SendNotification;
using TruckFreight.WebAdmin.Models.Notifications;
using TruckFreight.Application.Features.Administration.Queries.GetSystemHealth;
using TruckFreight.Application.Features.Administration.Commands.ClearCache;
using TruckFreight.WebAdmin.Models.System;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Concurrent;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using Microsoft.AspNetCore.Http;

public class CompleteTripCommandHandler : IRequestHandler<CompleteTripCommand, 
BaseResponse>
 {
 private readonly IUnitOfWork _unitOfWork;
 private readonly ICurrentUserService _currentUserService;
 private readonly ILogger<CompleteTripCommandHandler> _logger;
 private readonly IPushNotificationService _pushNotificationService;
 public CompleteTripCommandHandler(
 IUnitOfWork unitOfWork,
 ICurrentUserService currentUserService,
 ILogger<CompleteTripCommandHandler> logger,
 IPushNotificationService pushNotificationService)
 {
 _unitOfWork = unitOfWork;
 _currentUserService = currentUserService;
 _logger = logger;
 _pushNotificationService = pushNotificationService;
 }
 public async Task<BaseResponse> Handle(CompleteTripCommand request, CancellationToken 
cancellationToken)
 {
 try
 {
 var userId = _currentUserService.UserId;
 if (!userId.HasValue)
 {
;(401 :statusCode ,"کاربر احراز هویت نشده است")Failure.BaseResponse return 
 }
 var driver = await _unitOfWork.Drivers.GetByUserIdAsync(userId.Value, cancellationToken);
 if (driver == null)
 {
;(403 :statusCode ,"شما مجاز به تکمیل سفر نیستید")Failure.BaseResponse return 
 }
 var trip = await _unitOfWork.Trips.GetByIdAsync(request.TripId, 
 t => t.CargoRequest.CargoOwner.User,
 t => t.Driver.User);
 if (trip == null)
 {
;("سفر یافت نشد")NotFound.BaseResponse return 
 }
 if (trip.DriverId != driver.Id)
 {
;(403 :statusCode ,"این سفر به شما تعلق ندارد")Failure.BaseResponse return 
 }
 if (trip.Status != TripStatus.Delivered)
 {
;("سفر باید ابتدا تحویل داده شود")Failure.BaseResponse return 
 }
 await _unitOfWork.BeginTransactionAsync(cancellationToken);
 // Complete the trip
 Money actualPrice = null;
 if (request.ActualAmount.HasValue)
 {
 actualPrice = new Money(request.ActualAmount.Value, trip.AgreedPrice.Currency);
 }
 trip.Complete(actualPrice);
 
 if (!string.IsNullOrEmpty(request.Notes))
 {
 trip.AddNotes(request.Notes);
 }
 if (!string.IsNullOrEmpty(request.ElectronicWaybillNumber))
 {
 trip.SetElectronicWaybill(request.ElectronicWaybillNumber, 
request.IsSystemGeneratedWaybill);
 }
 _unitOfWork.Trips.Update(trip);
 // Update driver stats
 driver.CompleteTrip();
 driver.SetAvailability(true);
 _unitOfWork.Drivers.Update(driver);
 // Update cargo owner stats
 trip.CargoRequest.CargoOwner.CompleteOrder();
 _unitOfWork.CargoOwners.Update(trip.CargoRequest.CargoOwner);
 // Create payment record
 var commissionRate = await 
_unitOfWork.SystemConfigurations.GetValueAsync<decimal>("Commission.DefaultPercentage", 5m, 
cancellationToken);
 var finalAmount = actualPrice ?? trip.AgreedPrice;
 var commissionAmount = finalAmount.Multiply(commissionRate / 100);
 var payment = new Payment(
 trip.Id,
 trip.CargoRequest.CargoOwner.UserId,
 trip.Driver.UserId,
 finalAmount,
 commissionAmount,
 PaymentMethod.Wallet,
;("{TripNumber.trip {پرداخت بابت سفر"$ 
 await _unitOfWork.Payments.AddAsync(payment, cancellationToken);
 await _unitOfWork.SaveChangesAsync(cancellationToken);
 await _unitOfWork.CommitTransactionAsync(cancellationToken);
 // Send notifications
 await _pushNotificationService.SendTripStatusUpdateAsync(
 trip.CargoRequest.CargoOwner.UserId, 
 trip.Id, 
;("سفر با موفقیت تکمیل شد" 
 await _pushNotificationService.SendPaymentNotificationAsync(
 trip.CargoRequest.CargoOwner.UserId,
 payment.Id,
;("پرداخت منتظر تایید" 
 _logger.LogInformation("Trip completed successfully: {TripId}", trip.Id);
;("سفر با موفقیت تکمیل شد")Success.BaseResponse return 
 }
 catch (Exception ex)
 {
 await _unitOfWork.RollbackTransactionAsync(cancellationToken);
 _logger.LogError(ex, "Error occurred while completing trip");
;("خطا در تکمیل سفر")Failure.BaseResponse return 
 }
 }
 }
}
// TruckFreight.Application/Features/Trips/Queries/GetDriverActiveTrip/GetDriverActiveTripQuery.cs
using MediatR;
using TruckFreight.Application.Common.Models;
namespace TruckFreight.Application.Features.Trips.Queries.GetDriverActiveTrip
{
 public class GetDriverActiveTripQuery : IRequest<BaseResponse<ActiveTripDto>>
 {
 }
 public class ActiveTripDto
 {
 public Guid Id { get; set; }
 public string TripNumber { get; set; }
 public string Status { get; set; }
 public DateTime? AcceptedAt { get; set; }
 public DateTime? StartedAt { get; set; }
 public DateTime? EstimatedDeliveryTime { get; set; }
 public decimal AgreedAmount { get; set; }
 public string Currency { get; set; }
 // Cargo Request Info
 public CargoRequestInfo CargoRequest { get; set; }
 // Current Location
 public LocationInfo CurrentLocation { get; set; }
 // Progress
 public TripProgressInfo Progress { get; set; }
 }
 public class CargoRequestInfo
 {
 public Guid Id { get; set; }
 public string Title { get; set; }
 public string CargoType { get; set; }
 public AddressInfo OriginAddress { get; set; }
 public AddressInfo DestinationAddress { get; set; }
 public CargoOwnerInfo CargoOwner { get; set; }
 public string ContactPersonName { get; set; }
 public string ContactPersonPhone { get; set; }
 public string SpecialInstructions { get; set; }
 }
 public class AddressInfo
 {
 public string FullAddress { get; set; }
 public string City { get; set; }
 public string Province { get; set; }
 public double Latitude { get; set; }
 public double Longitude { get; set; }
 }
 public class CargoOwnerInfo
 {
 public string CompanyName { get; set; }
 public string FullName { get; set; }
 public double Rating { get; set; }
 }
 public class LocationInfo
 {
 public double Latitude { get; set; }
 public double Longitude { get; set; }
 public DateTime Timestamp { get; set; }
 public double? Speed { get; set; }
 public double? Heading { get; set; }
 }
 public class TripProgressInfo
 {
 public double TotalDistanceKm { get; set; }
 public double RemainingDistanceKm { get; set; }
 public int ProgressPercentage { get; set; }
 public TimeSpan? ElapsedTime { get; set; }
 public TimeSpan? EstimatedRemainingTime { get; set; }
 }
}
// 
TruckFreight.Application/Features/Trips/Queries/GetDriverActiveTrip/GetDriverActiveTripQueryHandler
.cs
using MediatR;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Domain.Interfaces;
namespace TruckFreight.Application.Features.Trips.Queries.GetDriverActiveTrip
{
 public class GetDriverActiveTripQueryHandler : IRequestHandler<GetDriverActiveTripQuery, 
BaseResponse<ActiveTripDto>>
 {
 private readonly IUnitOfWork _unitOfWork;
 private readonly ICurrentUserService _currentUserService;
 private readonly IMapService _mapService;
 public GetDriverActiveTripQueryHandler(
 IUnitOfWork unitOfWork,
 ICurrentUserService currentUserService,
 IMapService mapService)
 {
 _unitOfWork = unitOfWork;
 _currentUserService = currentUserService;
 _mapService = mapService;
 }
 public async Task<BaseResponse<ActiveTripDto>> Handle(GetDriverActiveTripQuery request, 
CancellationToken cancellationToken)
 {
 var userId = _currentUserService.UserId;
 if (!userId.HasValue)
 {
;(401 :statusCode ,"کاربر احراز هویت نشده است")Failure.>ActiveTripDto<BaseResponse return 
 }
 var driver = await _unitOfWork.Drivers.GetByUserIdAsync(userId.Value, cancellationToken);
 if (driver == null)
 {
 return BaseResponse<ActiveTripDto>.Failure("نشد یافت راننده", statusCode: 404);
 }
 var activeTrip = await _unitOfWork.Trips.GetDriverActiveTrip(driver.Id, cancellationToken);
 if (activeTrip == null)
 {
;("سفر فعالی یافت نشد")NotFound.>ActiveTripDto<BaseResponse return 
 }
 // Calculate progress
 var totalDistance = activeTrip.CargoRequest.GetDistanceKm();
 var remainingDistance = 0.0;
 var progressPercentage = 0;
 if (driver.CurrentLocation != null && activeTrip.Status >= Domain.Enums.TripStatus.InTransit)
 {
 var currentLocation = driver.CurrentLocation;
 var destinationLocation = new Domain.ValueObjects.GeoLocation(
 activeTrip.CargoRequest.DestinationAddress.Latitude,
 activeTrip.CargoRequest.DestinationAddress.Longitude);
 
 remainingDistance = currentLocation.CalculateDistanceTo(destinationLocation);
 progressPercentage = (int)Math.Max(0, Math.Min(100, ((totalDistance - remainingDistance) / 
totalDistance) * 100));
 }
 var response = new ActiveTripDto
 {
 Id = activeTrip.Id,
 TripNumber = activeTrip.TripNumber,
 Status = activeTrip.Status.ToString(),
 AcceptedAt = activeTrip.AcceptedAt,
 StartedAt = activeTrip.StartedAt,
 AgreedAmount = activeTrip.AgreedPrice.Amount,
 Currency = activeTrip.AgreedPrice.Currency,
 CargoRequest = new CargoRequestInfo
 {
 Id = activeTrip.CargoRequest.Id,
 Title = activeTrip.CargoRequest.Title,
 CargoType = activeTrip.CargoRequest.CargoType.ToString(),
 OriginAddress = new AddressInfo
 {
 FullAddress = activeTrip.CargoRequest.OriginAddress.GetFullAddress(),
 City = activeTrip.CargoRequest.OriginAddress.City,
 Province = activeTrip.CargoRequest.OriginAddress.Province,
 Latitude = activeTrip.CargoRequest.OriginAddress.Latitude,
 Longitude = activeTrip.CargoRequest.OriginAddress.Longitude
 },
 DestinationAddress = new AddressInfo
 {
 FullAddress = activeTrip.CargoRequest.DestinationAddress.GetFullAddress(),
 City = activeTrip.CargoRequest.DestinationAddress.City,
 Province = activeTrip.CargoRequest.DestinationAddress.Province,
 Latitude = activeTrip.CargoRequest.DestinationAddress.Latitude,
 Longitude = activeTrip.CargoRequest.DestinationAddress.Longitude
 },
 CargoOwner = new CargoOwnerInfo
 {
 CompanyName = activeTrip.CargoRequest.CargoOwner.CompanyName,
 FullName = activeTrip.CargoRequest.CargoOwner.User.GetFullName(),
 Rating = activeTrip.CargoRequest.CargoOwner.Rating
 },
 ContactPersonName = activeTrip.CargoRequest.ContactPersonName,
 ContactPersonPhone = activeTrip.CargoRequest.ContactPersonPhone?.GetDisplayFormat(),
 SpecialInstructions = activeTrip.CargoRequest.SpecialInstructions
 },
 Progress = new TripProgressInfo
 {
 TotalDistanceKm = totalDistance,
 RemainingDistanceKm = remainingDistance,
 ProgressPercentage = progressPercentage,
 ElapsedTime = activeTrip.GetTotalDuration()
 }
 };
 if (driver.CurrentLocation != null)
 {
 response.CurrentLocation = new LocationInfo
 {
 Latitude = driver.CurrentLocation.Latitude,
 Longitude = driver.CurrentLocation.Longitude,
 Timestamp = driver.CurrentLocation.Timestamp,
 Speed = driver.CurrentLocation.Speed,
 Heading = driver.CurrentLocation.Heading
 };
 }
 return BaseResponse<ActiveTripDto>.Success(response);
 }
 }
}
Payment and Wallet Management Features
csharp// 
TruckFreight.Application/Features/Payments/Commands/InitiatePayment/InitiatePaymentCommand.cs
using MediatR;
using TruckFreight.Application.Common.Models;
namespace TruckFreight.Application.Features.Payments.Commands.InitiatePayment
{
 public class InitiatePaymentCommand : IRequest<BaseResponse<InitiatePaymentResponse>>
 {
 public Guid TripId { get; set; }
 public string CallbackUrl { get; set; }
 public string Description { get; set; }
 }
 public class InitiatePaymentResponse
 {
 public Guid PaymentId { get; set; }
 public string PaymentUrl { get; set; }
 public string Authority { get; set; }
 public string Message { get; set; }
 }
}
// 
TruckFreight.Application/Features/Payments/Commands/InitiatePayment/InitiatePaymentCommandHa
ndler.cs
using MediatR;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Domain.Entities;
using TruckFreight.Domain.Enums;
using TruckFreight.Domain.Interfaces;
namespace TruckFreight.Application.Features.Payments.Commands.InitiatePayment
{
 public class InitiatePaymentCommandHandler : IRequestHandler<InitiatePaymentCommand, 
BaseResponse<InitiatePaymentResponse>>
 {
 private readonly IUnitOfWork _unitOfWork;
 private readonly ICurrentUserService _currentUserService;
 private readonly IPaymentGatewayService _paymentGatewayService;
 private readonly ILogger<InitiatePaymentCommandHandler> _logger;
 public InitiatePaymentCommandHandler(
 IUnitOfWork unitOfWork,
 ICurrentUserService currentUserService,
 IPaymentGatewayService paymentGatewayService,
 ILogger<InitiatePaymentCommandHandler> logger)
 {
 _unitOfWork = unitOfWork;
 _currentUserService = currentUserService;
 _paymentGatewayService = paymentGatewayService;
 _logger = logger;
 }
 public async Task<BaseResponse<InitiatePaymentResponse>> Handle(InitiatePaymentCommand 
request, CancellationToken cancellationToken)
 {
 try
 {
 var userId = _currentUserService.UserId;
 if (!userId.HasValue)
 {
 ,"کاربر احراز هویت نشده است")Failure.>InitiatePaymentResponse<BaseResponse return 
statusCode: 401);
 }
 // Get payment by trip
 var payment = await _unitOfWork.Payments.GetByTripIdAsync(request.TripId, 
cancellationToken);
 if (payment == null)
 {
 return BaseResponse<InitiatePaymentResponse>.NotFound("نشد یافت پرداخت;("
 }
 if (payment.PayerId != userId.Value)
 {
 ,"شما مجاز به پرداخت این حساب نیستید")Failure.>InitiatePaymentResponse<BaseResponse return 
statusCode: 403);
 }
 if (payment.Status != PaymentStatus.Pending)
 {
این پردازش شده است")Failure.>InitiatePaymentResponse<BaseResponse return 
;(" پرداخت قبالً
 }
 await _unitOfWork.BeginTransactionAsync(cancellationToken);
 // Get user email and phone for payment gateway
 var user = await _unitOfWork.Users.GetByIdAsync(userId.Value, cancellationToken);
 
 // Initiate payment with Zarinpal
 var paymentResult = await _paymentGatewayService.InitiatePaymentAsync(
 payment.Amount,
 request.Description ?? $"سفر بابت پرداخت} payment.Trip.TripNumber}",
 request.CallbackUrl,
 user.Email,
 user.PhoneNumber?.Number);
 if (!paymentResult.IsSuccess)
 {
 :خطا در اتصال به درگاه پرداخت"$)Failure.>InitiatePaymentResponse<BaseResponse return 
{paymentResult.ErrorMessage}");
 }
 // Create Zarinpal transaction record
 var zarinpalTransaction = new ZarinpalTransaction(
 payment.Id,
 payment.Amount,
 request.Description ?? $"سفر بابت پرداخت} payment.Trip.TripNumber}",
 user.Email,
 user.PhoneNumber?.Number,
 request.CallbackUrl);
 zarinpalTransaction.SetAuthority(paymentResult.Authority);
 await _unitOfWork.Payments.AddAsync(payment, cancellationToken);
 // Update payment with gateway info
 payment.SetGatewayInfo(paymentResult.Authority);
 _unitOfWork.Payments.Update(payment);
 await _unitOfWork.SaveChangesAsync(cancellationToken);
 await _unitOfWork.CommitTransactionAsync(cancellationToken);
 _logger.LogInformation("Payment initiated successfully: {PaymentId}, Authority: {Authority}", 
 payment.Id, paymentResult.Authority);
 return BaseResponse<InitiatePaymentResponse>.Success(new InitiatePaymentResponse
 {
 PaymentId = payment.Id,
 PaymentUrl = paymentResult.PaymentUrl,
 Authority = paymentResult.Authority,
"پرداخت با موفقیت آغاز شد" = Message 
 });
 }