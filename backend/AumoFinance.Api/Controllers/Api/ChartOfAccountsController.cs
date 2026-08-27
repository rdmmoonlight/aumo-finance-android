using AumoFinance.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AumoFinance.Api.Controllers.Api;

public record AccountRequest(string Code, string Name, AccountType Type);

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChartOfAccountsController : ControllerBase
{
    // TODO fase berikutnya: ganti in-memory list ini dengan EF Core + DbContext nyata.
    private static readonly List<Account> _accounts = new();
    private static int _nextId = 1;

    [HttpGet]
    public IActionResult List() => Ok(_accounts);

    [HttpPost]
    public IActionResult Create([FromBody] AccountRequest request)
    {
        var account = new Account { Id = _nextId++, Code = request.Code, Name = request.Name, Type = request.Type };
        _accounts.Add(account);
        return Ok(account);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] AccountRequest request)
    {
        var account = _accounts.FirstOrDefault(a => a.Id == id);
        if (account == null) return NotFound();
        account.Code = request.Code;
        account.Name = request.Name;
        account.Type = request.Type;
        return Ok(account);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var account = _accounts.FirstOrDefault(a => a.Id == id);
        if (account == null) return NotFound();
        _accounts.Remove(account);
        return NoContent();
    }
}
