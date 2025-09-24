using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjectAgaman.Core.DTOs;
using ProjectAgaman.Repositories.OrderInfRepositories;

namespace ProjectAgaman.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderInfoController : ControllerBase
    {
        private readonly IOrderInfoRepository _orderInfoRepository;
        public OrderInfoController(IOrderInfoRepository orderInfoRepository)
        {
            _orderInfoRepository = orderInfoRepository;
        }

        [HttpGet("GetAllOrderInfos")]
        public IActionResult Get()
        {
            try
            {
                var orderInfos = _orderInfoRepository.GetAllOrderInfosAsync().Result;
                return Ok(orderInfos);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetOrderInfoById/{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                var orderInfo = _orderInfoRepository.GetOrderInfoByIdAsync(id).Result;
                if (orderInfo == null)
                {
                    return NotFound($"No order info found with ID {id}.");
                }
                return Ok(orderInfo);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("AddOrderInfo")]
        public async Task<ActionResult<ResponseDTO>> AddOrderInfo([FromBody] OrderInfoDTO orderInfoDto)
        {
            if (orderInfoDto == null)
            {
                return BadRequest("Order info data is null.");
            }
            try
            {
                var response = await _orderInfoRepository.AddOrderInfoAsync(orderInfoDto);
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
    }
}
