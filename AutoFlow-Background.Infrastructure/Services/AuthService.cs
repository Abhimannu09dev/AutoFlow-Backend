using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Auth;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Background.Infrastructure.Configuration;
using AutoFlow_Backend.Domain.Entities;
using AutoFlow_Background.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AutoFlow_Background.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly JwtSettings _jwtSettings;
    private readonly IAppDbContext _context;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IOptions<JwtSettings> jwtOptions,
        IAppDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtSettings = jwtOptions.Value;
        _context = context;
    }

    public async Task<ApiResponse<AuthResponse>> RegisterAsync(
        RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
            return ApiResponseFactory.Fail<AuthResponse>("Email is already registered.");

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Address = request.Address,
            PhoneNumber = request.Phone,  // inherited from IdentityUser
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            return ApiResponseFactory.Fail<AuthResponse>(string.Join(" ", errors));
        }

        await _userManager.AddToRoleAsync(user, "Customer");
        var customer = new Customer
        {
            FullName = $"{request.FirstName} {request.LastName}",
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            CreatedAt = DateTime.UtcNow,
            ApplicationUserId = user.Id
        };
        await _context.Customers.AddAsync(customer, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponseFactory.Ok("Registration successful.", await BuildAuthResponse(user));
    }

    public async Task<ApiResponse<AuthResponse>> LoginAsync(
        LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return ApiResponseFactory.Fail<AuthResponse>("Invalid email or password.");

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
            return ApiResponseFactory.Fail<AuthResponse>("Invalid email or password.");
        return ApiResponseFactory.Ok("Login successful.", await BuildAuthResponse(user));
    }

    private async Task<AuthResponse> BuildAuthResponse(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.Email ?? string.Empty),
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: creds);

        return new AuthResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Email = user.Email ?? string.Empty,
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = roles
        };
    }
}