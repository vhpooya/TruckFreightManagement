using TruckFreight.Application.Features.Administration.Queries.GetSystemHealth;

namespace TruckFreight.WebAdmin.Models.System
{
    public class SystemHealthViewModel : BaseViewModel
    {
        public SystemHealthDto HealthData { get; set; } = new SystemHealthDto();
    }

    public class SystemLogsViewModel : BaseViewModel
    {
        public List<SystemLogDto> Logs { get; set; } = new List<SystemLogDto>();
    }
}
Additional Admin Views
html<!-- TruckFreight.WebAdmin/Views/Users/Index.cshtml -->
@model TruckFreight.WebAdmin.Models.Users.UserListViewModel
@{
    ViewData["Title"] = Model.PageTitle;
}

<div class="row">
    <div class="col-12">
        <h1 class="page-title">@Model.PageTitle</h1>
    </div>
</div>

<!-- Search Form -->
<div class="search-form">
    <form method="get">
        <div class="row">
            <div class="col-md-4">
                <input type="text" name="search" value="@Model.SearchTerm" class="form-control" placeholder="جستجو در نام، کد ملی، شماره تلفن...">
            </div>
            <div class="col-md-3">
                <select name="role" class="form-select">
                    <option value="">همه نقش‌ها</option>
                    <option value="Driver" selected="@(Model.SelectedRole == "Driver")">راننده</option>
                    <option value="CargoOwner" selected="@(Model.SelectedRole == "CargoOwner")">صاحب بار</option>
                    <option value="Admin" selected="@(Model.SelectedRole == "Admin")">ادمین</option>
                </select>
            </div>
            <div class="col-md-3">
                <select name="status" class="form-select">
                    <option value="">همه وضعیت‌ها</option>
                    <option value="PendingVerification" selected="@(Model.SelectedStatus == "PendingVerification")">در انتظار تایید</option>
                    <option value="Active" selected="@(Model.SelectedStatus == "Active")">فعال</option>
                    <option value="Suspended" selected="@(Model.SelectedStatus == "Suspended")">معلق</option>
                    <option value="Blocked" selected="@(Model.SelectedStatus == "Blocked")">مسدود</option>
                </select>
            </div>
            <div class="col-md-2">
                <button type="submit" class="btn btn-primary w-100">
                    <i class="fas fa-search"></i> جستجو
                </button>
            </div>
        </div>
    </form>
</div>

<!-- Users Table -->
<div class="card">
    <div class="card-body">
        @if (Model.Users.Items.Any())
        {
            <div class="table-responsive">
                <table class="table table-hover">
                    <thead>
                        <tr>
                            <th>نام کامل</th>
                            <th>کد ملی</th>
                            <th>شماره تلفن</th>
                            <th>نقش</th>
                            <th>وضعیت</th>
                            <th>تاریخ ثبت</th>
                            <th>عملیات</th>
                        </tr>
                    </thead>
                    <tbody>
                        @foreach (var user in Model.Users.Items)
                        {
                            <tr>
                                <td>
                                    <div class="d-flex align-items-center">
                                        @if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                                        {
                                            <img src="@user.ProfileImageUrl" alt="@user.FullName" class="rounded-circle me-2" width="32" height="32">
                                        }
                                        else
                                        {
                                            <div class="bg-secondary rounded-circle me-2 d-flex align-items-center justify-content-center" style="width: 32px; height: 32px;">
                                                <i class="fas fa-user text-white"></i>
                                            </div>
                                        }
                                        <div>
                                            <strong>@user.FullName</strong>
                                            @if (!string.IsNullOrEmpty(user.Email))
                                            {
                                                <br><small class="text-muted">@user.Email</small>
                                            }
                                        </div>
                                    </div>
                                </td>
                                <td>@user.NationalId</td>
                                <td>@user.PhoneNumber</td>
                                <td>
                                    <span class="badge bg-info">@user.Role</span>
                                </td>
                                <td>
                                    <span class="status-badge status-@user.Status.ToLower()">@user.Status</span>
                                </td>
                                <td>@user.CreatedAt.ToString("yyyy/MM/dd")</td>
                                <td class="table-actions">
                                    <a asp-action="Details" asp-route-id="@user.Id" class="btn btn-sm btn-outline-primary">
                                        <i class="fas fa-eye"></i> مشاهده
                                    </a>
                                    @if (user.Status == "PendingVerification")
                                    {
                                        <button type="button" class="btn btn-sm btn-outline-success" onclick="approveUser('@user.Id')">
                                            <i class="fas fa-check"></i> تایید
                                        </button>
                                        <button type="button" class="btn btn-sm btn-outline-danger" onclick="rejectUser('@user.Id')">
                                            <i class="fas fa-times"></i> رد
                                        </button>
                                    }
                                </td>
                            </tr>
                        }
                    </tbody>
                </table>
            </div>

            <!-- Pagination -->
            @if (Model.Users.TotalPages > 1)
            {
                <nav aria-label="صفحه‌بندی">
                    <ul class="pagination">
                        @if (Model.Users.HasPreviousPage)
                        {
                            <li class="page-item">
                                <a class="page-link" href="?page=@(Model.Users.PageNumber - 1)&search=@Model.SearchTerm&role=@Model.SelectedRole&status=@Model.SelectedStatus">قبلی</a>
                            </li>
                        }

                        @for (int i = Math.Max(1, Model.Users.PageNumber - 2); i <= Math.Min(Model.Users.TotalPages, Model.Users.PageNumber + 2); i++)
                        {
                            <li class="page-item @(i == Model.Users.PageNumber ? "active" : "")">
                                <a class="page-link" href="?page=@i&search=@Model.SearchTerm&role=@Model.SelectedRole&status=@Model.SelectedStatus">@i</a>
                            </li>
                        }

                        @if (Model.Users.HasNextPage)
                        {
                            <li class="page-item">
                                <a class="page-link" href="?page=@(Model.Users.PageNumber + 1)&search=@Model.SearchTerm&role=@Model.SelectedRole&status=@Model.SelectedStatus">بعدی</a>
                            </li>
                        }
                    </ul>
                </nav>
            }
        }
        else
        {
            <div class="empty-state">
                <i class="fas fa-users"></i>
                <h4>کاربری یافت نشد</h4>
                <p>هیچ کاربری با معیارهای جستجوی شما یافت نشد.</p>
            </div>
        }
    </div>
</div>

@section Scripts {
    <script>
        function approveUser(userId) {
            ModalUtils.confirm(
                'تایید کاربر',
                'آیا از تایید این کاربر اطمینان دارید؟',
                function() {
                    const form = document.createElement('form');
                    form.method = 'POST';
                    form.action = `/Users/Approve/${userId}`;
                    
                    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
                    const tokenInput = document.createElement('input');
                    tokenInput.type = 'hidden';
                    tokenInput.name = '__RequestVerificationToken';
                    tokenInput.value = token;
                    form.appendChild(tokenInput);
                    
                    document.body.appendChild(form);
                    form.submit();
                }
            );
        }

        function rejectUser(userId) {
            const reason = prompt('دلیل رد درخواست را وارد کنید:');
            if (reason) {
                const form = document.createElement('form');
                form.method = 'POST';
                form.action = `/Users/Reject/${userId}`;
                
                const reasonInput = document.createElement('input');
                reasonInput.type = 'hidden';
                reasonInput.name = 'reason';
                reasonInput.value = reason;
                form.appendChild(reasonInput);
                
                const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
                const tokenInput = document.createElement('input');
                tokenInput.type = 'hidden';
                tokenInput.name = '__RequestVerificationToken';
                tokenInput.value = token;
                form.appendChild(tokenInput);
                
                document.body.appendChild(form);
                form.submit();
            }
        }
    </script>
}
API Swagger Configuration Enhancement
csharp/