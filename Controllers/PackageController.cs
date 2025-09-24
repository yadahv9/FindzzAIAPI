using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjectAgaman.Core.DTOs;
using ProjectAgaman.Repositories.PackageRepositories;
using ProjectAgaman.Repositories.RolesRepositories;
using ProjectAgaman.Repositories.UsersRepositories;
using System.IO.Packaging;

namespace ProjectAgaman.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PackageController : ControllerBase
    {
        public readonly IPackageRepository _packageRepository;
        public readonly IRoleRepository _roleRepository;
        private readonly IConfiguration _configuration;
        public PackageController(IPackageRepository packageRepository, IConfiguration configuration, IRoleRepository roleRepository)
        {
            _packageRepository = packageRepository;
            _roleRepository = roleRepository;
            _configuration = configuration;
        }

        [Authorize]
        [HttpGet("GetAllPackages")]
        public async Task<ActionResult<IEnumerable<PackagesDTO>>> GetPackages()
        {
            try
            {
                var packages = await _packageRepository.GetAllPackages();
                return Ok(packages);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [Authorize]
        [HttpPost("InsertPackage")]
        public async Task<IActionResult> InsertPackage([FromBody] PackagesDTO package)
        {
            if (package == null)
                return BadRequest("Package data cannot be null.");

            var result = await _packageRepository.InsertPackageAsync(package);

            if (result.IsSuccess)
                return Ok(result);

            return StatusCode(500, result); // or return BadRequest(result) based on result content
        }


        [Authorize]
        [HttpPut("UpdatePackage")]
        public async Task<IActionResult> UpdatePackage([FromBody] PackagesDTO package)
        {
            if (package == null)
                return BadRequest("Package data cannot be null.");

            var result = await _packageRepository.UpdatePackageAsync(package);

            if (result.IsSuccess)
                return Ok(result);

            return StatusCode(500, result);
        }
        [Authorize]
        [HttpDelete("DeletePackage/{Id}")]
        public async Task<IActionResult> DeletePackage(int Id)
        {
            if (Id <= 0)
                return BadRequest("Invalid package ID.");

            var result = await _packageRepository.DeletePackageAsync(Id);

            if (result.IsSuccess)
                return Ok(result);

            return StatusCode(500, result);
        }

        [HttpGet("GetPackageById/{Id}")]
        public async Task<ActionResult<PackagesDTO>> GetPackageById(int Id)
        {
            try
            {
                if (Id <= 0)
                    return BadRequest("Invalid package ID.");
                var package = await _packageRepository.GetPackageByIdAsync(Id);
                if (package == null)
                    return NotFound("Package not found.");
                return Ok(package);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
        }
}
