using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjectAgaman.Core.DTOs;
using ProjectAgaman.Repositories.PaymentsRepositories;
using ProjectAgaman.Repositories.RecruiterRepositories;
using ProjectAgaman.Repositories.SettingsRepositories;
using Stripe;

namespace ProjectAgaman.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentRepository _paymentRepository;
        public PaymentsController(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> Create(PaymentRequestDto dto)
        {
            var res = await _paymentRepository.CreatePaymentAsync(dto);
            if (!res.IsSuccess) return BadRequest(res.ErrorMessage);
            return Ok(res);
        }

        [Authorize]
        [HttpPost("confirm")]
        public async Task<IActionResult> Confirm(ConfirmPaymentDto dto)
        {
            var res = await _paymentRepository.ConfirmPaymentAsync(dto);
            if (!res.IsSuccess) return BadRequest(res.ErrorMessage);
            // TODO: place order logic
            return Ok(new { success = true });
        }
    }
}
