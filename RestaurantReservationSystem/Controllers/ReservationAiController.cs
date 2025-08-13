using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestaurantReservationSystem.Repositories;

namespace RestaurantReservationSystem.Controllers
{
    [Route("api/ai")]
    [ApiController]
    public class ReservationAiController : ControllerBase
    {
        private readonly ReservationAiService _ai;
        private readonly IReservationService _res;

        public ReservationAiController(ReservationAiService ai, IReservationService res)
        {
            _ai = ai;
            _res = res;
        }

        public record AiRequest(string Message);
        public record AiResponse(bool ok, string Message, object? data = null);


        [HttpPost("handle")]
        public async Task<IActionResult> Handle([FromBody] AiRequest req, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(req.Message))
            {
                return BadRequest(new AiResponse(false, "Empty message."));
            }

            var intent = await _ai.ParseAsync(req.Message, ct);

            // Need core fields to proceed
            if (intent.Date is null || intent.Time is null || intent.PartySize is null)
            {
                return Ok(new AiResponse(false, "Please include date, time, and party size."));
            }

            //Make a DateTime from Date + Time 
            if(!DateTime.TryParse($"{intent.Date} {intent.Time}", out var when))
            {
                return Ok(new AiResponse(false, "I couldn't read the date/time. Try yyyy-mm-dd and HH:mm."));
            }
           
            if(intent.Action == "CheckAvailability")
            {
                var slots = await _res.CheckAvailabilityAsync(when, intent.PartySize.Value, ct);
                var pretty = slots.Select(s => s.ToString("yyyy-MM-dd HH:mm")).ToList();
                var msg = pretty.Any() ? "Here are some available times:" : "Sorry, no availability for that slot.";
                return Ok(new AiResponse(true, msg, new { slots = pretty }));
            }

            if(intent.Action == "CreateReservation")
            {
                if(string.IsNullOrWhiteSpace(intent.Name) || string.IsNullOrWhiteSpace(intent.Phone))
                {
                    return Ok(new AiResponse(false, "Please include your name and phone to confirm the booking."));
                }

                var dto = new CreateReservationDto
                {
                    DateTime = when,
                    PartySize = intent.PartySize.Value,
                    FirstName = intent.Name.Split(' ').FirstOrDefault() ?? intent.Name,
                    LastName = intent.Name.Contains(' ') ? intent.Name[(intent.Name.IndexOf(' ') + 1)..] : "",
                    Phone = intent.Phone
                };


                var result = await _res.CreateReservationAsync(dto, ct);
                if (!result.Success) return Ok(new AiResponse(false, result.Error ?? "Could not create reservation."));
                return Ok(new AiResponse(true, "Reservation confirmed.", new { confirmation = result.ConfirmationNumber, id = result.ReservationId }));
            }
            return Ok(new AiResponse(false, "Tell me to check availability or create a reservation"));
        }
    }
}
