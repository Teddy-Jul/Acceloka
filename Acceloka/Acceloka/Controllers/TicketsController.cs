using Acceloka.Features.AvalaibleTickets.Queries;
using Acceloka.Features.BookTicket.Commands;
using Acceloka.Features.BookedTickets.Queries;
using Acceloka.Features.EditBookedTicket.Commands;
using Acceloka.Features.RevokeTicket.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Acceloka.Controllers
{
    [Route("api/v1")]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TicketsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("get-available-ticket")]
        public async Task<IActionResult> GetAvailableTicket(
            [FromQuery] string? categoryName,
            [FromQuery] string? ticketCode,
            [FromQuery] string? ticketName,
            [FromQuery] decimal? price,
            [FromQuery] DateTimeOffset? minEventDate,
            [FromQuery] DateTimeOffset? maxEventDate,
            [FromQuery] string orderBy = "TicketCode",
            [FromQuery] string orderState = "ascending",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new GetAvailableTicketsQuery
            {
                CategoryName = categoryName,
                TicketCode = ticketCode,
                TicketName = ticketName,
                Price = price,
                MinEventDate = minEventDate,
                MaxEventDate = maxEventDate,
                OrderBy = orderBy,
                OrderState = orderState,
                Page = page,
                PageSize = pageSize
            };

            var data = await _mediator.Send(query);
            return Ok(data);
        }

        [HttpPost("book-ticket")]
        public async Task<IActionResult> BookTicket([FromBody] BookTicketCommand command)
        {
            var result = await _mediator.Send(command);
            return Created("",result);
        }

        [HttpGet("get-all-booked-tickets")]
        public async Task<IActionResult> GetAllBookedTickets()
        {
            var query = new GetAllBookedTicketsQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("get-booked-ticket/{bookedTicketId}")]
        public async Task<IActionResult> GetBookedTicket([FromRoute] int bookedTicketId)
        {
            var query = new GetBookedTicketDetailQuery { BookedTicketId = bookedTicketId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpDelete("revoke-ticket/{bookedTicketId}/{ticketCode}/{quantity}")]
        public async Task<IActionResult> RevokeTicket(
            [FromRoute] int bookedTicketId,
            [FromRoute] string ticketCode,
            [FromRoute] int quantity)
        {
            var command = new RevokeTicketCommand
            {
                BookedTicketId = bookedTicketId,
                TicketCode = ticketCode,
                Quantity = quantity
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("edit-booked-ticket/{bookedTicketId}")]
        public async Task<IActionResult> EditBookedTicket(
            [FromRoute] int bookedTicketId,
            [FromBody] EditBookedTicketCommand command)
        {
            command.BookedTicketId = bookedTicketId;
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}