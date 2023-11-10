using System.Net.Http.Headers;
using AutoDiffusion.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace AutoDiffusion.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class CallBacksController : ControllerBase
{
    [HttpPost]

    public async Task<IActionResult> ReceiveNotification([FromBody] Notification notification)
    {
        var jsonContent = JsonContent.Create(notification.Data);
        var strContent = await jsonContent.ReadAsStringAsync();
        await Console.Out.WriteLineAsync(strContent);
        return Ok(strContent);
    }

    [HttpGet]
    public IActionResult Index()
    {
        return Ok();
    }
}