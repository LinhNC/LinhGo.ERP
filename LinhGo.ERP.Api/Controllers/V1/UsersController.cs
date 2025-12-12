using Asp.Versioning;
using LinhGo.ERP.Api.Models;
using LinhGo.ERP.Application.Abstractions.Services;
using LinhGo.ERP.Application.Common.Constants;
using LinhGo.ERP.Application.DTOs.Users;
using LinhGo.SharedKernel.Querier;
using LinhGo.SharedKernel.Result;
using Microsoft.AspNetCore.Mvc;

namespace LinhGo.ERP.Api.Controllers.V1;

/// <summary>
/// User management endpoints
/// Provides CRUD operations and search functionality for users
/// </summary>
[ApiVersion(GeneralConstants.ApiV1Version)]
[Route("api/v{version:apiVersion}/users")]
public class UsersController(IUserService userService) : BaseApiController
{
    /// <summary>
    /// Search and filter users with advanced query capabilities
    /// </summary>
    /// <remarks>
    /// Search users with flexible filtering, sorting, and pagination.
    /// 
    /// Examples:
    /// - GET /api/v1/users?filter[isActive]=true&amp;sort=-createdAt&amp;page=1&amp;pageSize=20
    /// - GET /api/v1/users?filter[email][contains]=@example.com&amp;sort=username
    /// - GET /api/v1/users?filter[emailConfirmed]=true&amp;filter[isActive]=true&amp;sort=-lastLoginAt
    /// </remarks>
    [QuerierFieldsAttribute(
        entityName: "Users",
        filterFields: [
            "email", "username", "firstName", "lastName", 
            "isActive", "emailConfirmed", "phoneNumber",
            "createdAt", "updatedAt", "lastLoginAt"
        ],
        sortFields: [
            "email", "username", "firstName", "lastName",
            "isActive", "createdAt", "updatedAt", "lastLoginAt"
        ]
    )]
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Search([FromQuery] QuerierParams queries, CancellationToken ctx)
    {
        var result = await userService.QueryAsync(queries, ctx);
        return ToResponse(result);
    }

    /// <summary>
    /// Get all users
    /// </summary>
    /// <remarks>
    /// Returns all users in the system (including inactive).
    /// For active users only, use GET /api/v1/users/active
    /// </remarks>
    [HttpGet("all")]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll()
    {
        var result = await userService.GetAllAsync();
        return ToResponse(result);
    }

    /// <summary>
    /// Get active users only
    /// </summary>
    /// <remarks>
    /// Returns only users where IsActive = true and IsDeleted = false
    /// </remarks>
    [HttpGet("active")]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetActiveUsers()
    {
        var result = await userService.GetActiveUsersAsync();
        return ToResponse(result);
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    /// <param name="id">User unique identifier</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await userService.GetByIdAsync(id);
        return ToResponse(result);
    }

    /// <summary>
    /// Get user by email address
    /// </summary>
    /// <param name="email">User email address</param>
    /// <remarks>
    /// Email search is case-insensitive
    /// </remarks>
    [HttpGet("email/{email}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByEmail(string email)
    {
        var result = await userService.GetByEmailAsync(email);
        return ToResponse(result);
    }

    /// <summary>
    /// Get user by username
    /// </summary>
    /// <param name="username">Username</param>
    /// <remarks>
    /// Username search is case-insensitive
    /// </remarks>
    [HttpGet("username/{username}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByUsername(string username)
    {
        var result = await userService.GetByUsernameAsync(username);
        return ToResponse(result);
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    /// <param name="dto">User creation data</param>
    /// <remarks>
    /// Email and username must be unique.
    /// Password will be hashed before storage.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        var result = await userService.CreateAsync(dto);
        return ToCreatedResponse(result, nameof(GetById), new { id = result.IsError ? Guid.Empty : result.Value.Id });
    }

    /// <summary>
    /// Update an existing user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="dto">User update data (must include Version for optimistic concurrency)</param>
    /// <remarks>
    /// Requires Version field for optimistic concurrency control.
    /// Returns 409 Conflict if Version doesn't match (concurrent update detected).
    /// </remarks>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto)
    {
        var result = await userService.UpdateAsync(id, dto);
        return ToResponse(result);
    }

    /// <summary>
    /// Delete a user (soft delete)
    /// </summary>
    /// <param name="id">User ID</param>
    /// <remarks>
    /// Performs soft delete by setting IsDeleted = true.
    /// User data is retained for audit purposes.
    /// </remarks>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await userService.DeleteAsync(id);
        return ToNoContentResponse(result);
    }
}

