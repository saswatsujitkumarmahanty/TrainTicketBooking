using Microsoft.AspNetCore.Mvc;
using Application.Dto;
using Domain.Models;
using Infrastructure.Services;
using Application.Interface;

namespace TrainTicketBooking.Controllers;
[ApiController]
[Route("api/[controller]")]
public class AuthController(
    IUserRepository users,
    PasswordHasher hasher,
    JwtTokenService tokens) : ControllerBase
{
    // POST api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            Phone = request.Phone,
            PasswordHash = hasher.Hash(request.Password)
        };
        try
        {
            var id = await users.CreateAsync(user);
            return StatusCode(StatusCodes.Status201Created, new { userId = id, message = "Registration successful. Please log in." });
        }
        catch (BusinessRuleException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }
    // POST api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await users.GetByEmailAsync(request.Email.Trim().ToLowerInvariant());
        if (user is null || !hasher.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { error = "Invalid email or password." });
        var (token, expires) = tokens.CreateToken(user);
        return Ok(new AuthResponse
        {
            Token = token,
            ExpiresAtUtc = expires,
            FullName = user.FullName,
            Email = user.Email
        });
    }
}