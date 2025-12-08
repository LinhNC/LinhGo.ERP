using Asp.Versioning;
using LinhGo.ERP.Application.Common.Constants;
using Microsoft.AspNetCore.Mvc;

namespace LinhGo.ERP.Api.Controllers.V1;

[ApiVersion(GeneralConstants.ApiV1Version)]
[Route("api/v{version:apiVersion}/test")]
public class TestController
{
    [HttpGet]
    public string Get()
    {
        return "Test successful";
    }

}