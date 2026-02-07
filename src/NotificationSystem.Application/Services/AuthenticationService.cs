using FluentResults;
using Microsoft.Extensions.Options;
using NotificationSystem.Application.Common.Errors;
using NotificationSystem.Application.Configuration;
using NotificationSystem.Application.DTOs.Auth;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.Services;

public class AuthenticationService(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IJwtTokenGenerator jwtTokenGenerator,
    IPasswordHasher passwordHasher,
    IOptions<JwtSettings> jwtOptions) : IAuthenticationService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly JwtSettings _jwtOptions = jwtOptions.Value;

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request, string ipAddress, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailWithRolesAsync(request.Email, cancellationToken);

        if (user == null)
            return Result.Fail<LoginResponse>(new UnauthorizedError("Invalid email or password"));

        if (!user.IsActive)
            return Result.Fail<LoginResponse>(new UnauthorizedError("User account is inactive"));

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            return Result.Fail<LoginResponse>(new UnauthorizedError("Invalid email or password"));

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToList();

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user, roles, permissions);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress
        };

        await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);

        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken);

        var response = new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes),
            User = new UserInfo
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Roles = roles,
                Permissions = permissions
            }
        };

        return Result.Ok(response);
    }

    public async Task<Result<LoginResponse>> RefreshTokenAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken = default)
    {
        var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken, cancellationToken);

        if (token == null || !token.IsActive)
            return Result.Fail<LoginResponse>(new UnauthorizedError("Invalid or expired refresh token"));

        var user = await _userRepository.GetByIdWithRolesAsync(token.UserId, cancellationToken);

        if (user == null || !user.IsActive)
            return Result.Fail<LoginResponse>(new UnauthorizedError("User not found or inactive"));

        var newRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();

        token.RevokedAt = DateTime.UtcNow;
        token.RevokedByIp = ipAddress;
        token.ReplacedByToken = newRefreshToken;
        await _refreshTokenRepository.UpdateAsync(token, cancellationToken);

        var newRefreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress
        };

        await _refreshTokenRepository.AddAsync(newRefreshTokenEntity, cancellationToken);

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToList();

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user, roles, permissions);

        var response = new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes),
            User = new UserInfo
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Roles = roles,
                Permissions = permissions
            }
        };

        return Result.Ok(response);
    }

    public async Task<Result> RevokeTokenAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken = default)
    {
        var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken, cancellationToken);

        if (token == null || !token.IsActive)
            return Result.Fail(new UnauthorizedError("Invalid or expired refresh token"));

        token.RevokedAt = DateTime.UtcNow;
        token.RevokedByIp = ipAddress;
        await _refreshTokenRepository.UpdateAsync(token, cancellationToken);

        return Result.Ok();
    }

    public async Task<Result<LoginResponse>> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default)
    {
        if (await _userRepository.EmailExistsAsync(request.Email, cancellationToken))
            return Result.Fail<LoginResponse>(new ConflictError("User", $"email '{request.Email}' already in use"));

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            FullName = request.FullName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        if (request.RoleIds.Count != 0)
        {
            user.UserRoles = [.. request.RoleIds.Select(roleId => new UserRole
            {
                UserId = user.Id,
                RoleId = roleId,
                AssignedAt = DateTime.UtcNow
            })];
        }

        await _userRepository.AddAsync(user, cancellationToken);

        user = await _userRepository.GetByIdWithRolesAsync(user.Id, cancellationToken);

        var roles = user!.UserRoles.Select(ur => ur.Role.Name).ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToList();

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user, roles, permissions);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = "registration"
        };

        await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);

        var response = new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes),
            User = new UserInfo
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Roles = roles,
                Permissions = permissions
            }
        };

        return Result.Ok(response);
    }
}
