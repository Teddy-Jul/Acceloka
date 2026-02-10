using Acceloka.Model;
using Acceloka.Services;
using Microsoft.AspNetCore.Mvc;

namespace Acceloka.Controllers
{
    [Route("api/v1")]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly TicketServices _service;

        public TicketsController(TicketServices service)
        {
            _service = service;
        }

        [HttpGet("get-available-ticket")]
        public async Task<IActionResult> GetAvailableTicket(
            [FromQuery] string? namaKategori,
            [FromQuery] string? kodeTicket,
            [FromQuery] string? namaTicket,
            [FromQuery] decimal? harga,
            [FromQuery] DateTimeOffset? tanggalEventMinimal,
            [FromQuery] DateTimeOffset? tanggalEventMaksimal,
            [FromQuery] string orderBy = "KodeTicket",
            [FromQuery] string orderState = "ascending",
            [FromQuery] int page = 1,       // default page 1
            [FromQuery] int pageSize = 10)  // default max show in 1 page = 10
        {
            var data = await _service.GetAvailableTickets(
                namaKategori,
                kodeTicket,
                namaTicket,
                harga,
                tanggalEventMinimal, 
                tanggalEventMaksimal,
                orderBy, 
                orderState,
                page, 
                pageSize);

            return Ok(data);
        }   
        
        [HttpPost("book-ticket")]

        public async Task<IActionResult> BookTicket([FromBody] BookTicketRequest request)
        {
            var result = await _service.BookTicket(request);
            return Ok(result);
        }
    }
}
