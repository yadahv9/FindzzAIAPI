using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjectAgaman.Core.DTOs;
using ProjectAgaman.Repositories.RolesRepositories;

namespace ProjectAgaman.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly IRoleRepository _roleRepository;
        public RolesController(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }
       
        [HttpGet("GetAllRoles")]
        public async Task<ActionResult<RolesDTO>> GetAllRoles()
        {
            try
            {
                var employees = await _roleRepository.GetAllRoles();
                return Ok(employees);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("AddRoles")]
        public async Task<ActionResult<ResponseDTO>> AddRoles(string roleName)
        {
            if (roleName == null)
            {
                return BadRequest("Role data is null.");
            }
            try
            {

                var response = await _roleRepository.AddRoles(roleName);
                if (!response.IsSuccess)
                {
                    return BadRequest(response.ErrorMessage);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("UpdateRoles")]
        public async Task<ActionResult<ResponseDTO>> UpdateRoles([FromBody] RolesDTO roleDTO)
        {
            if (roleDTO == null)
            {
                return BadRequest("Role data is null.");
            }
            try
            {
                var existingRole = await _roleRepository.GetRoleById(roleDTO.pkroleId);
                if (existingRole == null)
                {
                    return NotFound($"No job found .");
                }
                var response = await _roleRepository.UpdateRoles(roleDTO);
                if (!response.IsSuccess)
                {
                    return BadRequest(response.ErrorMessage);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteRoles/{roleId}")]
        public async Task<ActionResult<ResponseDTO>> DeleteRoles(int roleId)
        {
            if (roleId <= 0)
            {
                return BadRequest("Invalid role ID.");
            }
            try
            {
                var existingRole = await _roleRepository.GetRoleById(roleId);
                if (existingRole == null)
                {
                    return NotFound($"No job found .");
                }
                var response = await _roleRepository.DeleteRoles(roleId);
                if (!response.IsSuccess)
                {
                    return BadRequest(response.ErrorMessage);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteListOfRoles")]
        public async Task<ActionResult<ResponseDTO>> DeleteListOfRoles([FromBody] List<int> roleIds)
        {
            if (roleIds == null || !roleIds.Any())
            {
                return  BadRequest("Role IDs list is null or empty.");
            }
            try
            {
                var response = await _roleRepository.DeleteListOfRoles(roleIds);
                if (!response.IsSuccess)
                {
                    return BadRequest(response.ErrorMessage);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("GetPaginatedRolesAsync")]
        public async Task<ActionResult<PaginationEntityDto<RolesDTO>>> GetPaginatedRolesAsync(int pageNumber, int pageSize,string? roleName)
        {
            try
            {
                var paginatedRoles = await _roleRepository.GetPaginatedRolesAsync(pageNumber, pageSize,roleName);
                return Ok(paginatedRoles);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
    }
}
