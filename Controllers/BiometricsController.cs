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

    // ✅ InitLicense endpoint that tries to obtain the Neurotec licenses
    [HttpGet("InitLicense")]
    public IActionResult InitLicense()
    {
        const string Address = "192.168.1.61";
        const string Port = "5000";
        const string Components = "Biometrics.FingerExtraction,Biometrics.FingerSegmentation";
        const string OptionalComponents = "Biometrics.FingerQualityAssessmentBase";

        try
        {
            // Optional: Handle trial mode if needed (assuming Utilities.GetTrialModeFlag() exists)
            // NLicenseManager.TrialMode = Utilities.GetTrialModeFlag();
            // NLicenseManager.TrialMode = true
            // Obtain required components
            if (!NLicense.ObtainComponents(Address, Port, Components))
            {
                return BadRequest(new
                {
                    status = "failed",
                    message = $"Could not obtain licenses for components: {Components}"
                });
            }

            // Optionally obtain additional components
            NLicense.ObtainComponents(Address, Port, OptionalComponents);

            return Ok(new
            {
                status = "success",
                message = "License initialized successfully."
            });
        }
        catch (IOException ioEx)
        {
            return StatusCode(500, new
            {
                status = "error",
                message = $"Failed to obtain licenses. {ioEx.Message} (Licensing service might not be running.)"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                status = "error",
                message = $"Failed to obtain licenses. Error: {ex.Message}"
            });
        }
    }

}
