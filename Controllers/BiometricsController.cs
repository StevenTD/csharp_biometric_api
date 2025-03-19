using Microsoft.AspNetCore.Mvc;
using Neurotec.Licensing;

namespace csharp_crud_api.Controllers;

[ApiController]
[Route("[controller]")]
public class BiometricsController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<BiometricsController> _logger;

    public BiometricsController(ILogger<BiometricsController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetBiometrics")]
    public IEnumerable<Biometrics> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new Biometrics
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }

    [HttpGet("InitLicense")]
    public IActionResult InitLicense()
    {
        const string Address = "/local"; // License server IP
        const string Port = "5000";           // License server Port
        const string Components = "Biometrics.FingerExtraction,Biometrics.FingerSegmentation";
        const string OptionalComponents = "Biometrics.FingerQualityAssessmentBase";

        try
        {
            // Attempt to obtain the required components
            foreach (var component in Components.Split(','))
            {
                if (!NLicense.ObtainComponents(Address, Port, component))
                {
                    // If any component fails, return an error message
                    return StatusCode(500, new
                    {
                        status = "failed",
                        message = $"Could not obtain license for component: {component}"
                    });
                }
            }

            // Attempt to obtain optional components
            foreach (var optionalComponent in OptionalComponents.Split(','))
            {
                if (!NLicense.ObtainComponents(Address, Port, optionalComponent))
                {
                    // Log or handle optional components failure (this won't stop success)
                    Console.WriteLine($"Optional component '{optionalComponent}' not obtained.");
                }
            }

            // If all components are successfully obtained
            return Ok(new
            {
                status = "success",
                message = "Licenses initialized successfully."
            });
        }
        catch (IOException ioEx)
        {
            // Specific catch for I/O exceptions (e.g., licensing service not running)
            return StatusCode(500, new
            {
                status = "error",
                message = $"Failed to obtain licenses. {ioEx.Message} (Licensing service might not be running.)"
            });
        }
        catch (Exception ex)
        {
            // Catch all other exceptions
            return StatusCode(500, new
            {
                status = "error",
                message = $"Failed to obtain licenses. Error: {ex.Message}"
            });
        }
    }

}
