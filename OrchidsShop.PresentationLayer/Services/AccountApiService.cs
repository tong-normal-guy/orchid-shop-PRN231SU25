using OrchidsShop.PresentationLayer.Constants;
using OrchidsShop.PresentationLayer.Models.Accounts;
using OrchidsShop.PresentationLayer.Models.Commons;

namespace OrchidsShop.PresentationLayer.Services;

/// <summary>
/// Service for handling account-related API operations
/// </summary>
public class AccountApiService
{
    private readonly ApiHelper _apiHelper;
    private const string AccountEndpoint = "accounts";

    public AccountApiService(ApiHelper apiHelper)
    {
        _apiHelper = apiHelper;
    }

    /// <summary>
    /// Đăng nhập người dùng
    /// </summary>
    /// <param name="loginRequest">Thông tin đăng nhập</param>
    /// <returns>Kết quả đăng nhập</returns>
    public async Task<LoginResponse?> LoginAsync(LoginRequestModel loginRequest)
    {
        var url = $"{StringValue.BaseUrl}{AccountEndpoint}/login";
        return await _apiHelper.PostLoginAsync(url, loginRequest);
    }

    /// <summary>
    /// Đăng ký người dùng mới
    /// </summary>
    /// <param name="registerRequest">Thông tin đăng ký</param>
    /// <returns>Kết quả đăng ký</returns>
    public async Task<ApiOperationResponse?> RegisterAsync(RegisterRequestModel registerRequest)
    {
        var url = $"{StringValue.BaseUrl}{AccountEndpoint}/register";
        return await _apiHelper.PostAsync(url, registerRequest);
    }

    /// <summary>
    /// Tạo tài khoản mới (Admin only)
    /// </summary>
    /// <param name="accountRequest">Thông tin tài khoản</param>
    /// <returns>Kết quả tạo tài khoản</returns>
    public async Task<ApiOperationResponse?> CreateAccountAsync(AccountRequestModel accountRequest)
    {
        var url = StringValue.BaseUrl + AccountEndpoint;
        return await _apiHelper.PostAsync(url, accountRequest);
    }

    /// <summary>
    /// Lấy tài khoản theo ID
    /// </summary>
    /// <param name="id">ID của tài khoản</param>
    /// <returns>Thông tin tài khoản</returns>
    public async Task<ApiResponse<List<AccountModel>>?> GetAccountByIdAsync(Guid id)
    {
        var url = $"{StringValue.BaseUrl}{AccountEndpoint}/{id}";
        return await _apiHelper.GetAsync<AccountModel>(url);
    }

    /// <summary>
    /// Lấy danh sách tất cả các vai trò
    /// </summary>
    /// <returns>Danh sách các vai trò</returns>
    public async Task<ApiResponse<List<string>>?> GetRolesAsync()
    {
        var url = $"{StringValue.BaseUrl}{AccountEndpoint}/roles";
        return await _apiHelper.GetAsync<string>(url);
    }

    /// <summary>
    /// Cập nhật tài khoản
    /// </summary>
    /// <param name="id">ID của tài khoản cần cập nhật</param>
    /// <param name="accountRequest">Thông tin tài khoản mới</param>
    /// <returns>Kết quả cập nhật</returns>
    public async Task<ApiOperationResponse?> UpdateAccountAsync(Guid id, AccountRequestModel accountRequest)
    {
        accountRequest.Id = id; // Ensure ID is set
        var url = $"{StringValue.BaseUrl}{AccountEndpoint}/{id}";
        return await _apiHelper.PutAsync(url, accountRequest);
    }

    /// <summary>
    /// Xóa tài khoản
    /// </summary>
    /// <param name="id">ID của tài khoản cần xóa</param>
    /// <returns>Kết quả xóa</returns>
    public async Task<ApiOperationResponse?> DeleteAccountAsync(Guid id)
    {
        var url = $"{StringValue.BaseUrl}{AccountEndpoint}/{id}";
        return await _apiHelper.DeleteAsync(url);
    }

    /// <summary>
    /// Đổi mật khẩu
    /// </summary>
    /// <param name="id">ID của tài khoản</param>
    /// <param name="currentPassword">Mật khẩu hiện tại</param>
    /// <param name="newPassword">Mật khẩu mới</param>
    /// <param name="confirmPassword">Xác nhận mật khẩu mới</param>
    /// <returns>Kết quả đổi mật khẩu</returns>
    public async Task<ApiOperationResponse?> ChangePasswordAsync(Guid id, string currentPassword, string newPassword, string confirmPassword)
    {
        var changePasswordRequest = new AccountRequestModel
        {
            Id = id,
            Password = newPassword,
            ConfirmPassword = confirmPassword
        };

        var url = $"{StringValue.BaseUrl}{AccountEndpoint}/{id}/change-password";
        return await _apiHelper.PutAsync(url, changePasswordRequest);
    }

    /// <summary>
    /// Cập nhật thông tin cá nhân (không bao gồm mật khẩu)
    /// </summary>
    /// <param name="id">ID của tài khoản</param>
    /// <param name="name">Tên mới</param>
    /// <param name="email">Email mới</param>
    /// <returns>Kết quả cập nhật</returns>
    public async Task<ApiOperationResponse?> UpdateProfileAsync(Guid id, string name, string email)
    {
        var updateRequest = new AccountRequestModel
        {
            Id = id,
            Name = name,
            Email = email
        };

        return await UpdateAccountAsync(id, updateRequest);
    }

    /// <summary>
    /// Kiểm tra email đã tồn tại chưa
    /// </summary>
    /// <param name="email">Email cần kiểm tra</param>
    /// <returns>True nếu email đã tồn tại</returns>
    public async Task<bool> IsEmailExistsAsync(string email)
    {
        try
        {
            var url = $"{StringValue.BaseUrl}{AccountEndpoint}/check-email?email={Uri.EscapeDataString(email)}";
            var response = await _apiHelper.GetAsync<bool>(url);
            return response?.Data?.FirstOrDefault() ?? false;
        }
        catch
        {
            return false;
        }
    }
} 