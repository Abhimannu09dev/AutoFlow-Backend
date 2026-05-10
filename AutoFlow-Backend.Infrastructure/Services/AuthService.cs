using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Auth;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Infrastructure.Configuration;
using AutoFlow_Backend.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AutoFlow_Backend.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IOptions<JwtSettings> jwtOptions)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<ApiResponse<AuthResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return ApiResponseFactory.Fail<AuthResponse>("Invalid email or password.");

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
            return ApiResponseFactory.Fail<AuthResponse>("Invalid email or password.");

        return ApiResponseFactory.Ok("Login successful.", await BuildAuthResponseAsync(user));
    }

    private async Task<AuthResponse> BuildAuthResponseAsync(ApplicationUser user)
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
            FullName = user.FullName,
            Roles = roles
        };
    }
}