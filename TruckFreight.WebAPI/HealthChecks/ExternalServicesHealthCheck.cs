using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TruckFreight.WebAPI.HealthChecks
{
    public class ExternalServicesHealthCheck : IHealthCheck
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public ExternalServicesHealthCheck(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var results = new Dictionary<string, bool>();

            // Check Zarinpal
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(10);
                var response = await client.GetAsync("https://api.zarinpal.com/pg/v4/payment/request.json", cancellationToken);
                results["Zarinpal"] = response.IsSuccessStatusCode;
            }
            catch
            {
                results["Zarinpal"] = false;
            }

            // Check Neshan Maps
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(10);
                var neshanApiKey = _configuration["Neshan:ApiKey"];
                if (!string.IsNullOrEmpty(neshanApiKey))
                {
                    client.DefaultRequestHeaders.Add("Api-Key", neshanApiKey);
                    var response = await client.GetAsync("https://api.neshan.org/v1/search", cancellationToken);
                    results["Neshan"] = response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Unauthorized;
                }
                else
                {
                    results["Neshan"] = false;
                }
            }
            catch
            {
                results["Neshan"] = false;
            }

            var healthyServices = results.Count(r => r.Value);
            var totalServices = results.Count;

            if (healthyServices == totalServices)
            {
                return HealthCheckResult.Healthy($"All external services are healthy ({healthyServices}/{totalServices})");
            }
            else if (healthyServices > 0)
            {
                return HealthCheckResult.Degraded($"Some external services are unhealthy ({healthyServices}/{totalServices})", 
                    data: results.ToDictionary(r => r.Key, r => (object)r.Value));
            }
            else
            {
                return HealthCheckResult.Unhealthy("All external services are unhealthy", 
                    data: results.ToDictionary(r => r.Key, r => (object)r.Value));
            }
        }
    }
}
Enhanced Admin Views
html<!-- TruckFreight.WebAdmin/Views/Shared/_AdminLayout.cshtml -->
<!DOCTYPE html>
<html lang="fa" dir="rtl">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] - پنل مدیریت حمل و نقل</title>
    
    <!-- Favicon -->
    <link rel="icon" type="image/x-icon" href="~/favicon.ico">
    
    <!-- Bootstrap RTL 5.3 -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.rtl.min.css" rel="stylesheet">
    
    <!-- Font Awesome 6.4 -->
    <link href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" rel="stylesheet">
    
    <!-- DataTables RTL -->
    <link href="https://cdn.datatables.net/1.13.4/css/dataTables.bootstrap5.min.css" rel="stylesheet">
    <link href="https://cdn.datatables.net/responsive/2.4.1/css/responsive.bootstrap5.min.css" rel="stylesheet">
    
    <!-- Chart.js -->
    <link href="https://cdn.jsdelivr.net/npm/chart.js@3.9.1/dist/chart.min.css" rel="stylesheet">
    
    <!-- Custom Admin CSS -->
    <link rel="stylesheet" href="~/css/admin-style.css" asp-append-version="true" />
    
    <!-- Persian Fonts -->
    <link href="https://fonts.googleapis.com/css2?family=Vazirmatn:wght@100;200;300;400;500;600;700;800;900&display=swap" rel="stylesheet">
    
    <!-- Additional CSS from sections -->
    @await RenderSectionAsync("Styles", required: false)
</head>
<body class="admin-body">
    <div class="admin-wrapper">
        <!-- Sidebar -->
        <nav id="adminSidebar" class="admin-sidebar">
            <div class="sidebar-header">
                <div class="sidebar-brand">
                    <i class="fas fa-truck-moving"></i>
                    <span>مدیریت حمل و نقل</span>
                </div>
                <button type="button" id="sidebarToggle" class="btn btn-link sidebar-toggle">
                    <i class="fas fa-bars"></i>
                </button>
            </div>

            <div class="sidebar-menu">
                <ul class="nav flex-column">
                    <!-- Dashboard -->
                    <li class="nav-item">
                        <a class="nav-link @(ViewContext.RouteData.Values["Controller"]?.ToString() == "Home" ? "active" : "")" 
                           asp-controller="Home" asp-action="Index">
                            <i class="fas fa-tachometer-alt"></i>
                            <span>داشبورد</span>
                        </a>
                    </li>

                    <!-- User Management -->
                    <li class="nav-item">
                        <a class="nav-link @(ViewContext.RouteData.Values["Controller"]?.ToString() == "Users" ? "active" : "")" 
                           asp-controller="Users" asp-action="Index">
                            <i class="fas fa-users"></i>
                            <span>مدیریت کاربران</span>
                        </a>
                    </li>

                    <!-- Document Management -->
                    <li class="nav-item">
                        <a class="nav-link @(ViewContext.RouteData.Values["Controller"]?.ToString() == "Documents" ? "active" : "")" 
                           asp-controller="Documents" asp-action="Index">
                            <i class="fas fa-file-alt"></i>
                            <span>مدیریت مدارک</span>
                            @{
                                // You would get this from a service
                                var pendingDocsCount = 5; // Example
                            }
                            @if (pendingDocsCount > 0)
                            {
                                <span class="badge bg-warning">@pendingDocsCount</span>
                            }
                        </a>
                    </li>

                    <!-- Trip Management -->
                    <li class="nav-item">
                        <a class="nav-link @(ViewContext.RouteData.Values["Controller"]?.ToString() == "Trips" ? "active" : "")" 
                           asp-controller="Trips" asp-action="Index">
                            <i class="fas fa-route"></i>
                            <span>مدیریت سفرها</span>
                        </a>
                    </li>

                    <!-- Payment Management -->
                    <li class="nav-item">
                        <a class="nav-link @(ViewContext.RouteData.Values["Controller"]?.ToString() == "Payments" ? "active" : "")" 
                           asp-controller="Payments" asp-action="Index">
                            <i class="fas fa-credit-card"></i>
                            <span>مدیریت پرداخت‌ها</span>
                        </a>
                    </li>

                    <!-- Reports -->
                    <li class="nav-item">
                        <a class="nav-link collapsed" href="#reportsSubmenu" data-bs-toggle="collapse" role="button" 
                           aria-expanded="false" aria-controls="reportsSubmenu">
                            <i class="fas fa-chart-line"></i>
                            <span>گزارشات</span>
                            <i class="fas fa-chevron-down ms-auto"></i>
                        </a>
                        <div class="collapse" id="reportsSubmenu">
                            <ul class="nav flex-column submenu">
                                <li class="nav-item">
                                    <a class="nav-link" asp-controller="Reports" asp-action="UserReports">
                                        <i class="fas fa-user-chart"></i> گزارش کاربران
                                    </a>
                                </li>
                                <li class="nav-item">
                                    <a class="nav-link" asp-controller="Reports" asp-action="TripReports">
                                        <i class="fas fa-route"></i> گزارش سفرها
                                    </a>
                                </li>
                                <li class="nav-item">
                                    <a class="nav-link" asp-controller="Reports" asp-action="FinancialReports">
                                        <i class="fas fa-money-bill-wave"></i> گزارش مالی
                                    </a>
                                </li>
                            </ul>
                        </div>
                    </li>

                    <!-- Notifications -->
                    <li class="nav-item">
                        <a class="nav-link @(ViewContext.RouteData.Values["Controller"]?.ToString() == "Notifications" ? "active" : "")" 
                           asp-controller="Notifications" asp-action="Index">
                            <i class="fas fa-bell"></i>
                            <span>مدیریت اعلانات</span>
                        </a>
                    </li>

                    <!-- Settings -->
                    <li class="nav-item">
                        <a class="nav-link collapsed" href="#settingsSubmenu" data-bs-toggle="collapse" role="button" 
                           aria-expanded="false" aria-controls="settingsSubmenu">
                            <i class="fas fa-cog"></i>
                            <span>تنظیمات</span>
                            <i class="fas fa-chevron-down ms-auto"></i>
                        </a>
                        <div class="collapse" id="settingsSubmenu">
                            <ul class="nav flex-column submenu">
                                <li class="nav-item">
                                    <a class="nav-link" asp-controller="Settings" asp-action="Index">
                                        <i class="fas fa-sliders-h"></i> تنظیمات سیستم
                                    </a>
                                </li>
                                <li class="nav-item">
                                    <a class="nav-link" asp-controller="Settings" asp-action="EmailTemplates">
                                        <i class="fas fa-envelope"></i> قالب‌های ایمیل
                                    </a>
                                </li>
                                <li class="nav-item">
                                    <a class="nav-link" asp-controller="Settings" asp-action="SmsTemplates">
                                        <i class="fas fa-sms"></i> قالب‌های پیامک
                                    </a>
                                </li>
                            </ul>
                        </div>
                    </li>

                    <!-- System -->
                    @if (User.IsInRole("SuperAdmin"))
                    {
                        <li class="nav-item">
                            <a class="nav-link @(ViewContext.RouteData.Values["Controller"]?.ToString() == "System" ? "active" : "")" 
                               asp-controller="System" asp-action="Index">
                                <i class="fas fa-server"></i>
                                <span>سلامت سیستم</span>
                            </a>
                        </li>
                    }
                </ul>
            </div>

            <!-- Sidebar Footer -->
            <div class="sidebar-footer">
                <div class="admin-user-info">
                    <div class="user-avatar">
                        <i class="fas fa-user-shield"></i>
                    </div>
                    <div class="user-details">
                        <strong>@User.Identity.Name</strong>
                        <small>@User.FindFirst("role")?.Value</small>
                    </div>
                </div>
            </div>
        </nav>

        <!-- Main Content -->
        <div class="admin-content">
            <!-- Top Navigation -->
            <header class="admin-header">
                <div class="header-left">
                    <button type="button" id="sidebarCollapseBtn" class="btn btn-link">
                        <i class="fas fa-bars"></i>
                    </button>
                    
                    <!-- Breadcrumb -->
                    <nav aria-label="breadcrumb" class="ms-3">
                        <ol class="breadcrumb mb-0">
                            <li class="breadcrumb-item"><a asp-controller="Home" asp-action="Index">خانه</a></li>
                            @if (ViewBag.Breadcrumbs != null)
                            {
                                @foreach (var breadcrumb in ViewBag.Breadcrumbs)
                                {
                                    <li class="breadcrumb-item active">@breadcrumb</li>
                                }
                            }
                        </ol>
                    </nav>
                </div>

                <div class="header-right">
                    <!-- Notifications Dropdown -->
                    <div class="dropdown me-3">
                        <button class="btn btn-link position-relative" type="button" data-bs-toggle="dropdown">
                            <i class="fas fa-bell"></i>
                            <span class="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger">
                                3
                            </span>
                        </button>
                        <ul class="dropdown-menu dropdown-menu-end notification-dropdown">
                            <li class="dropdown-header">اعلانات جدید</li>
                            <li><hr class="dropdown-divider"></li>
                            <li><a class="dropdown-item" href="#">کاربر جدید ثبت نام کرده است</a></li>
                            <li><a class="dropdown-item" href="#">سفر جدید در انتظار تایید</a></li>
                            <li><a class="dropdown-item" href="#">پرداخت جدید انجام شده است</a></li>
                            <li><hr class="dropdown-divider"></li>
                            <li><a class="dropdown-item text-center" href="#">مشاهده همه</a></li>
                        </ul>
                    </div>

                    <!-- User Dropdown -->
                    <div class="dropdown">
                        <button class="btn btn-link dropdown-toggle user-dropdown" type="button" data-bs-toggle="dropdown">
                            <i class="fas fa-user-circle me-1"></i>
                            @User.Identity.Name
                        </button>
                        <ul class="dropdown-menu dropdown-menu-end">
                            <li><a class="dropdown-item" href="#"><i class="fas fa-user me-2"></i> پروفایل</a></li>
                            <li><a class="dropdown-item" href="#"><i class="fas fa-cog me-2"></i> تنظیمات</a></li>
                            <li><hr class="dropdown-divider"></li>
                            <li>
                                <form asp-controller="Account" asp-action="Logout" method="post" class="d-inline w-100">
                                    <button type="submit" class="dropdown-item">
                                        <i class="fas fa-sign-out-alt me-2"></i> خروج
                                    </button>
                                </form>
                            </li>
                        </ul>
                    </div>
                </div>
            </header>

            <!-- Page Content -->
            <main class="admin-main">
                <!-- Alert Messages -->
                @if (TempData["SuccessMessage"] != null)
                {
                    <div class="alert alert-success alert-dismissible fade show" role="alert">
                        <i class="fas fa-check-circle me-2"></i>
                        @TempData["SuccessMessage"]
                        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
                    </div>
                }

                @if (TempData["ErrorMessage"] != null)
                {
                    <div class="alert alert-danger alert-dismissible fade show" role="alert">
                        <i class="fas fa-exclamation-circle me-2"></i>
                        @TempData["ErrorMessage"]
                        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
                    </div>
                }

                @if (TempData["InfoMessage"] != null)
                {
                    <div class="alert alert-info alert-dismissible fade show" role="alert">
                        <i class="fas fa-info-circle me-2"></i>
                        @TempData["InfoMessage"]
                        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
                    </div>
                }

                @if (TempData["WarningMessage"] != null)
                {
                    <div class="alert alert-warning alert-dismissible fade show" role="alert">
                        <i class="fas fa-exclamation-triangle me-2"></i>
                        @TempData["WarningMessage"]
                        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
                    </div>
                }

                <!-- Main Content Area -->
                @RenderBody()
            </main>
        </div>
    </div>

    <!-- Loading Overlay -->
    <div id="loadingOverlay" class="loading-overlay d-none">
        <div class="loading-spinner-container">
            <div class="loading-spinner"></div>
            <div class="loading-text">در حال پردازش...</div>
        </div>
    </div>

    <!-- Scripts -->
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script src="https://cdn.datatables.net/1.13.4/js/jquery.dataTables.min.js"></script>
    <script src="https://cdn.datatables.net/1.13.4/js/dataTables.bootstrap5.min.js"></script>
    <script src="https://cdn.datatables.net/responsive/2.4.1/js/dataTables.responsive.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/chart.js@3.9.1/dist/chart.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    
    <!-- Custom Admin Scripts -->
    <script src="~/js/admin-script.js" asp-append-version="true"></script>
    
    <!-- Anti-forgery token for AJAX -->
    <script>
        window.antiForgeryToken = '@Html.AntiForgeryToken()';
    </script>
    
    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>

csharp/