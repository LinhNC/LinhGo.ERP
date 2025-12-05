# Controller Template with Localization Support

## Template for New Controllers

When creating new controllers, use this template to ensure localization support:

```csharp
using Asp.Versioning;
using LinhGo.ERP.Application.Abstractions.Services;
using LinhGo.ERP.Application.Common.Localization;
using LinhGo.ERP.Application.DTOs.YourEntity;
using Microsoft.AspNetCore.Mvc;

namespace LinhGo.ERP.Api.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class YourEntitiesController : BaseApiController
{
    private readonly IYourEntityService _yourEntityService;

    // ✅ Always inject IErrorMessageLocalizer and pass to base
    public YourEntitiesController(
        IYourEntityService yourEntityService, 
        IErrorMessageLocalizer localizer) 
        : base(localizer)
    {
        _yourEntityService = yourEntityService;
    }

    /// <summary>
    /// Get all entities
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<YourEntityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _yourEntityService.GetAllAsync();
        return ToResponse(result);  // ✅ ToResponse handles localization
    }

    /// <summary>
    /// Get entity by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(YourEntityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _yourEntityService.GetByIdAsync(id);
        return ToResponse(result);  // ✅ ToResponse handles localization
    }

    /// <summary>
    /// Create a new entity
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(YourEntityDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateYourEntityDto dto)
    {
        var result = await _yourEntityService.CreateAsync(dto);
        return ToCreatedResponse(result, nameof(GetById), new { id = result.Value?.Id });
    }

    /// <summary>
    /// Update an existing entity
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(YourEntityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateYourEntityDto dto)
    {
        var result = await _yourEntityService.UpdateAsync(id, dto);
        return ToResponse(result);  // ✅ ToResponse handles localization
    }

    /// <summary>
    /// Delete an entity
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _yourEntityService.DeleteAsync(id);
        return ToNoContentResponse(result);  // ✅ ToNoContentResponse handles localization
    }
}
```

## Key Points

### ✅ DO

1. **Always inject `IErrorMessageLocalizer`**
   ```csharp
   public YourController(IYourService service, IErrorMessageLocalizer localizer) 
       : base(localizer)
   ```

2. **Use `ToResponse()` method**
   ```csharp
   return ToResponse(result);
   ```

3. **Use standardized error codes in services**
   ```csharp
   Error.NotFound("ENTITY_NOTFOUND", $"Entity with ID {id} not found")
   ```

4. **Add error translations**
   - Add to `ErrorMessageLocalizer.InitializeErrorMessages()`
   - Add for both English and Vietnamese (and any other supported languages)

### ❌ DON'T

1. **Don't create error responses manually**
   ```csharp
   // ❌ Bad
   return NotFound(new { message = "Not found" });
   
   // ✅ Good
   return ToResponse(result);
   ```

2. **Don't forget to pass localizer to base**
   ```csharp
   // ❌ Bad
   public YourController(IYourService service)
   {
   }
   
   // ✅ Good
   public YourController(IYourService service, IErrorMessageLocalizer localizer) 
       : base(localizer)
   {
   }
   ```

3. **Don't hardcode error messages**
   ```csharp
   // ❌ Bad
   Error.NotFound("Custom error message here")
   
   // ✅ Good
   Error.NotFound("ENTITY_NOTFOUND", $"Entity with ID {id} not found")
   ```

## Service Error Code Pattern

In your services, follow this pattern:

```csharp
public async Task<Result<EntityDto>> GetByIdAsync(Guid id)
{
    try
    {
        var entity = await _repository.GetByIdAsync(id);
        
        if (entity == null)
        {
            // ✅ Use error code constant
            return Result<EntityDto>.Failure(
                Error.NotFound("ENTITY_NOTFOUND", $"Entity with ID {id} not found")
            );
        }

        var dto = _mapper.Map<EntityDto>(entity);
        return Result<EntityDto>.Success(dto);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving entity {EntityId}", id);
        return Result<EntityDto>.Failure(
            Error.Failure("ENTITY_RETRIEVAL_FAILED", "Failed to retrieve entity")
        );
    }
}
```

## Error Code Naming Convention

Follow this pattern for consistency:

```
{ENTITY}_{ACTION}_{REASON}
```

Examples:
- `COMPANY_NOTFOUND`
- `COMPANY_CREATE_FAILED`
- `USER_EMAIL_DUPLICATE`
- `PRODUCT_INSUFFICIENT_STOCK`
- `ORDER_ITEMS_REQUIRED`

## Testing Your Controller

```bash
# Test in English
curl -H "Accept-Language: en" \
     -H "Content-Type: application/json" \
     http://localhost:5000/api/v1/yourentities/invalid-id

# Test in Vietnamese
curl -H "Accept-Language: vi" \
     -H "Content-Type: application/json" \
     http://localhost:5000/api/v1/yourentities/invalid-id
```

## Response Examples

### English Response
```json
{
  "type": "NotFound",
  "errors": [
    {
      "code": "ENTITY_NOTFOUND",
      "description": "Entity with ID abc123 not found"
    }
  ],
  "correlationId": "123-456-789"
}
```

### Vietnamese Response
```json
{
  "type": "NotFound",
  "errors": [
    {
      "code": "ENTITY_NOTFOUND",
      "description": "Không tìm thấy thực thể với ID abc123"
    }
  ],
  "correlationId": "123-456-789"
}
```

## Checklist for New Controllers

- [ ] Inject `IErrorMessageLocalizer` in constructor
- [ ] Pass localizer to `base(localizer)`
- [ ] Use `ToResponse()` for all action methods
- [ ] Add error codes to `ErrorMessageLocalizer`
- [ ] Add translations for all supported languages
- [ ] Test with different `Accept-Language` headers
- [ ] Document new error codes in LOCALIZATION_GUIDE.md

---

**Follow this template to ensure consistent localization support across all controllers! 🌍**

