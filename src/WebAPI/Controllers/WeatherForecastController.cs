namespace WebAPI.Controllers;

using System.Reflection;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private const string InformationMessageTemplate = "${Message}";

    private static readonly string[] Summaries =
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching",
    };

    private readonly ILogger<WeatherForecastController> logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        this.logger = logger;
    }

    /// <summary>
    /// Gets the forecast
    /// </summary>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="WeatherForecast"/></returns>
    /// <remarks>Authenticated</remarks>
    [Authorize]
    [HttpGet("")]
    public IEnumerable<WeatherForecast> Get()
    {
        string name = MethodBase.GetCurrentMethod().GetCustomAttribute<HttpGetAttribute>()?.Template ?? throw new InvalidOperationException();

        this.logger.LogInformation(WeatherForecastController.InformationMessageTemplate, name);

        return GetWeatherForecastImplementation();
    }

    /// <summary>
    /// Gets the forecast
    /// </summary>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="WeatherForecast"/></returns>
    /// <remarks>Anonymous</remarks>
    [AllowAnonymous]
    [HttpGet("NoAuth")]
    public IEnumerable<WeatherForecast> GetNoAuth()
    {
        string name = MethodBase.GetCurrentMethod()!.GetCustomAttribute<HttpGetAttribute>()?.Template ?? throw new InvalidOperationException();

        this.logger.LogInformation(WeatherForecastController.InformationMessageTemplate, name);

        return GetWeatherForecastImplementation();
    }

    private static IEnumerable<WeatherForecast> GetWeatherForecastImplementation()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = WeatherForecastController.Summaries[Random.Shared.Next(WeatherForecastController.Summaries.Length)],
            })
            .ToArray();
    }
}