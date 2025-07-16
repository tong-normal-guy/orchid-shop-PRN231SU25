using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OrchidsShop.BLL.Commons;
using OrchidsShop.BLL.Commons.Results;
using OrchidsShop.BLL.DTOs.Accounts.Requests;
using OrchidsShop.BLL.DTOs.Accounts.Responses;
using OrchidsShop.DAL.Contexts;
using OrchidsShop.DAL.Entities;
using OrchidsShop.DAL.Enums;

namespace OrchidsShop.BLL.Services;

public class AccountService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;

    public AccountService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _configuration = configuration;
    }

    /// <summary>
    /// Authenticates user login credentials and returns JWT token.
    /// </summary>
    /// <param name="request">Login request containing email and password.</param>
    /// <returns>OperationResult with JWT token if successful.</returns>
    public async Task<OperationResult<string>> LoginAsync(CommandAccountRequest request)
    {
                try
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return OperationResult<string>.Failure(StatusCode.BadRequest, 
                    new List<string> { "Email and password are required." });
            }

            var account = await _unitOfWork.Repository<Account>()
                .FirstOrDefaultAsync(x => x.Email == request.Email && x.Password == request.Password);

            if (account == null)
            {
                return OperationResult<string>.Failure(StatusCode.BadRequest, 
                    new List<string> { "Invalid email or password." });
            }

            // Load the role information
            var accountWithRole = _unitOfWork.Repository<Account>()
                .Get(filter: x => x.Id == account.Id, includeProperties: "Role")
                .SingleOrDefault();

            if (accountWithRole == null)
            {
                return OperationResult<string>.Failure(StatusCode.ServerError, 
                    new List<string> { "Failed to load account information." });
            }

            // Generate JWT token
            var token = await GenerateToken(accountWithRole, accountWithRole.Role.Name);
            
            return OperationResult<string>.Success(token, StatusCode.Ok, "Login successful.");
        }
        catch (Exception ex)
        {
            return OperationResult<string>.Failure(StatusCode.ServerError, 
                new List<string> { $"An error occurred during login: {ex.Message}" });
        }
    }

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="request">Registration request containing user information.</param>
    /// <returns>OperationResult indicating success or failure.</returns>
    public async Task<OperationResult<bool>> RegisterAsync(CommandAccountRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return OperationResult<bool>.Failure(StatusCode.BadRequest,
                    new List<string> { "Email and password are required." });
            }

            if (request.Password != request.ConfirmPassword)
            {
                return OperationResult<bool>.Failure(StatusCode.BadRequest,
                    new List<string> { "Password and confirm password do not match." });
            }

            // Check if email already exists
            var existingAccount = await _unitOfWork.Repository<Account>()
                .FirstOrDefaultAsync(x => x.Email == request.Email);

            if (existingAccount != null)
            {
                return OperationResult<bool>.Failure(StatusCode.BadRequest,
                    new List<string> { "Email already exists." });
            }

            // Get customer role
            var customerRole = await _unitOfWork.Repository<Role>()
                .FirstOrDefaultAsync(x => x.Name == EnumAccountRole.CUSTOMER.ToString());

            if (customerRole == null)
            {
                return OperationResult<bool>.Failure(StatusCode.ServerError,
                    new List<string> { "Customer role not found." });
            }

            var newAccount = new Account
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                Password = request.Password, // In production, this should be hashed
                Name = request.Name,
                RoleId = customerRole.Id
            };

            _unitOfWork.Repository<Account>().AddAsync(newAccount, false);
            var result = await _unitOfWork.SaveManualChangesAsync();

            return OperationResult<bool>.Success(result > 0, StatusCode.Created,
                result > 0 ? "Account registered successfully." : "Failed to register account.");
        }
        catch (Exception ex)
        {
            return OperationResult<bool>.Failure(StatusCode.ServerError,
                new List<string> { $"An error occurred during registration: {ex.Message}" });
        }
    }

    /// <summary>
    /// Creates a new account (admin function).
    /// </summary>
    /// <param name="request">Account creation request.</param>
    /// <returns>OperationResult indicating success or failure.</returns>
    public async Task<OperationResult<bool>> CreateAsync(CommandAccountRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return OperationResult<bool>.Failure(StatusCode.BadRequest,
                    new List<string> { "Email and password are required." });
            }

            // Check if email already exists
            var existingAccount = await _unitOfWork.Repository<Account>()
                .FirstOrDefaultAsync(x => x.Email == request.Email);

            if (existingAccount != null)
            {
                return OperationResult<bool>.Failure(StatusCode.BadRequest,
                    new List<string> { "Email already exists." });
            }

                        // Get role by name or default to CUSTOMER
            var roleName = request.Role ?? EnumAccountRole.CUSTOMER.ToString();
            var role = await _unitOfWork.Repository<Role>()
                .FirstOrDefaultAsync(x => x.Name == roleName);

            if (role == null)
            {
                return OperationResult<bool>.Failure(StatusCode.BadRequest, 
                    new List<string> { "Invalid role specified." });
            }

            var newAccount = new Account
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                Password = request.Password, // In production, this should be hashed
                Name = request.Name,
                RoleId = role.Id
            };

            await _unitOfWork.Repository<Account>().AddAsync(newAccount, false);
            var result = await _unitOfWork.SaveManualChangesAsync();

            return OperationResult<bool>.Success(result > 0, StatusCode.Created,
                result > 0 ? "Account created successfully." : "Failed to create account.");
        }
        catch (Exception ex)
        {
            return OperationResult<bool>.Failure(StatusCode.ServerError,
                new List<string> { $"An error occurred during account creation: {ex.Message}" });
        }
    }

    /// <summary>
    /// Retrieves all available roles.
    /// </summary>
    /// <returns>OperationResult with list of roles.</returns>
    public async Task<OperationResult<List<QueryRoleResponse>>> GetAllRolesAsync()
    {
        try
        {
            var query = _unitOfWork.Repository<Role>()
                .Get();

            var roles = query.Select(x => _mapper.Map<QueryRoleResponse>(x)).ToList();

            return OperationResult<List<QueryRoleResponse>>.Success(roles, StatusCode.Ok, "Roles retrieved successfully.");
        }
        catch (Exception ex)
        {
            return OperationResult<List<QueryRoleResponse>>.Failure(StatusCode.ServerError,
                new List<string> { $"An error occurred while retrieving roles: {ex.Message}" });
        }
    }

    /// <summary>
    /// Retrieves account by ID.
    /// </summary>
    /// <param name="id">Account ID.</param>
    /// <returns>OperationResult with account information.</returns>
    public async Task<OperationResult<QueryAccountResponse>> GetByIdAsync(Guid id)
    {
        try
        {
            var account = _unitOfWork.Repository<Account>()
                .Get(filter: x => x.Id == id, includeProperties: "Role")
                .FirstOrDefault();

            if (account == null)
            {
                return OperationResult<QueryAccountResponse>.Failure(StatusCode.NotFound,
                    new List<string> { "Account not found." });
            }

            var response = _mapper.Map<QueryAccountResponse>(account);

            return OperationResult<QueryAccountResponse>.Success(response, StatusCode.Ok, "Account retrieved successfully.");
        }
        catch (Exception ex)
        {
            return OperationResult<QueryAccountResponse>.Failure(StatusCode.ServerError,
                new List<string> { $"An error occurred while retrieving account: {ex.Message}" });
        }
    }

    /// <summary>
    /// Updates account information.
    /// </summary>
    /// <param name="id">Account ID to update.</param>
    /// <param name="request">Account update request.</param>
    /// <returns>OperationResult indicating success or failure.</returns>
    public async Task<OperationResult<bool>> UpdateAsync(Guid id, CommandAccountRequest request)
    {
        try
        {
            var existingAccount = await _unitOfWork.Repository<Account>()
                .FindAsync(id);

            if (existingAccount == null)
            {
                return OperationResult<bool>.Failure(StatusCode.NotFound,
                    new List<string> { "Account not found." });
            }

            // Check if email is being changed and if it already exists
            if (!string.IsNullOrEmpty(request.Email) && request.Email != existingAccount.Email)
            {
                var emailExists = await _unitOfWork.Repository<Account>()
                    .FirstOrDefaultAsync(x => x.Email == request.Email && x.Id != id);

                if (emailExists != null)
                {
                    return OperationResult<bool>.Failure(StatusCode.BadRequest,
                        new List<string> { "Email already exists." });
                }
            }

            // Update role if specified
            if (!string.IsNullOrEmpty(request.Role))
            {
                var role = await _unitOfWork.Repository<Role>()
                    .FirstOrDefaultAsync(x => x.Name == request.Role);

                if (role == null)
                {
                    return OperationResult<bool>.Failure(StatusCode.BadRequest,
                        new List<string> { "Invalid role specified." });
                }

                existingAccount.RoleId = role.Id;
            }

            // Use ReflectionHelper to update properties
            ReflectionHepler.UpdateProperties(request, existingAccount, new List<string> { "Id", "ConfirmPassword", "Role" });

            await _unitOfWork.Repository<Account>().UpdateAsync(existingAccount, false);
            var result = await _unitOfWork.SaveManualChangesAsync();

            return OperationResult<bool>.Success(result > 0, StatusCode.Ok,
                result > 0 ? "Account updated successfully." : "Failed to update account.");
        }
        catch (Exception ex)
        {
            return OperationResult<bool>.Failure(StatusCode.ServerError,
                new List<string> { $"An error occurred during account update: {ex.Message}" });
        }
    }

    public async Task<OperationResult<bool>> CreateRoleAsync(CommandAccountRequest request)
    {
        try
        {
            if (_unitOfWork.Repository<Role>().Get(filter: x => x.Name == request.Role).Any())
            {
                return OperationResult<bool>.Failure(StatusCode.BadRequest,
                    new List<string> { "Role already exists." });
            }

            var id = Guid.NewGuid();
            var role = new Role { Name = request.Role, Id = id };
            await _unitOfWork.Repository<Role>().AddAsync(role, false);
            var result = await _unitOfWork.SaveManualChangesAsync();
            return OperationResult<bool>.Success(result > 0, StatusCode.Created,
                result > 0 ? "Role created successfully." : "Failed to create role.");
        }
        catch (Exception ex)
        {
            return OperationResult<bool>.Failure(StatusCode.ServerError,
                new List<string> { $"An error occurred during role creation: {ex.Message}" });
        }
    }
    
    private async Task<string> GenerateToken(Account user, string role)
    {
        var secret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!));
        var credentials = new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
        var tokenDescript = new JwtSecurityToken(
            // issuer: configuration["Jwt:Issuer"],
            // audience: configuration["Jwt:Audience"],
            /*expires: DateTime.Now.AddHours(1),*/
            signingCredentials: credentials,
            claims: [
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, role)
            ]
        );
        return new JwtSecurityTokenHandler().WriteToken(tokenDescript);
    }
}
