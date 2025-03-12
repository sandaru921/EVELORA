using Microsoft.AspNetCore.Mvc;

[Route("api/users")]
[ApiController]
public class UsersController : ControllerBase
{
    [HttpGet]
    public IActionResult GetUsers()
    {
        return Ok(new[] { "User1", "User2" });
    }
}

