using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestaurantReservationSystem.Models;
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


        static class SessionChatState
        {
            private const string Key = "AI_CHAT_STATE";
            public static ChatState Load(ISession s)
                => string.IsNullOrEmpty(s.GetString(Key))
                   ? new ChatState()
                   : System.Text.Json.JsonSerializer.Deserialize<ChatState>(s.GetString(Key)!)!;
            public static void Save(ISession s, ChatState st)
                => s.SetString(Key, System.Text.Json.JsonSerializer.Serialize(st));
            public static void Clear(ISession s) => s.Remove(Key);

            public static void Merge(ChatState st, dynamic intent)
            {
                st.Action ??= intent.Action;
                st.Date ??= intent.Date;
                st.Time ??= intent.Time;
                st.PartySize ??= intent.PartySize;
                st.Name ??= intent.Name;
                st.Phone ??= intent.Phone;
            }
        }

        [HttpPost("handle")]
        public async Task<IActionResult> Handle([FromBody] AiRequest req, CancellationToken ct)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(req.Message))
                    return BadRequest(new AiResponse(false, "Empty message."));

                // 1) Parse whatever the user just said
                var intent = await _ai.ParseAsync(req.Message, ct);

                // 2) Load & merge with previous state
                var st = SessionChatState.Load(HttpContext.Session);
                SessionChatState.Merge(st, intent);

                // 3) Ask only for missing bits (context-aware)
                var missing = new List<string>();
                if (string.IsNullOrWhiteSpace(st.Date)) missing.Add("date (YYYY-MM-DD)");
                if (string.IsNullOrWhiteSpace(st.Time)) missing.Add("time (HH:mm, e.g., 18:00)");
                if (st.PartySize is null) missing.Add("party size");

                if (missing.Count > 0)
                {
                    SessionChatState.Save(HttpContext.Session, st);
                    return Ok(new AiResponse(false, $"I still need: {string.Join(", ", missing)}."));
                }

                // Build DateTime safely
                if (!DateTime.TryParse($"{st.Date} {st.Time}", out var when))
                {
                    SessionChatState.Save(HttpContext.Session, st);
                    return Ok(new AiResponse(false, "I couldn’t read the date/time. Use YYYY-MM-DD and HH:mm."));
                }

                // Default action: if none set, check availability
                st.Action ??= "CheckAvailability";

                if (st.Action == "CheckAvailability")
                {
                    var slots = await _res.CheckAvailabilityAsync(when, st.PartySize!.Value, ct);

                    // Prefer slots near the requested time
                    var shortlist = slots
                        .OrderBy(s => Math.Abs((s - when).TotalMinutes))
                        .Take(8)
                        .Select(s => s.ToString("yyyy-MM-dd HH:mm"))
                        .ToList();

                    SessionChatState.Save(HttpContext.Session, st);
                    var msg = shortlist.Any()
                        ? "Here are times near your request:"
                        : "No availability for that time. Here are other options today:";
                    var payload = shortlist.Any() ? shortlist : slots.Select(s => s.ToString("yyyy-MM-dd HH:mm")).Take(12).ToList();

                    return Ok(new AiResponse(true, msg, new { slots = payload }));
                }

                if (st.Action == "CreateReservation")
                {
                    if (string.IsNullOrWhiteSpace(st.Name) || string.IsNullOrWhiteSpace(st.Phone))
                    {
                        SessionChatState.Save(HttpContext.Session, st);
                        return Ok(new AiResponse(false, "Please include your name and phone to confirm."));
                    }

                    var parts = st.Name.Split(' ', 2);
                    var dto = new CreateReservationDto
                    {
                        DateTime = when,
                        PartySize = st.PartySize!.Value,
                        FirstName = parts[0],
                        LastName = parts.Length > 1 ? parts[1] : "",
                        Phone = st.Phone
                    };

                    var result = await _res.CreateReservationAsync(dto, ct);
                    if (!result.Success)
                    {
                        SessionChatState.Save(HttpContext.Session, st);
                        return Ok(new AiResponse(false, result.Error ?? "Could not create reservation."));
                    }

                    // Success: clear the state so a new conversation starts fresh
                    SessionChatState.Clear(HttpContext.Session);
                    return Ok(new AiResponse(true, "Reservation confirmed.", new { confirmation = result.ConfirmationNumber, id = result.ReservationId }));
                }

                SessionChatState.Save(HttpContext.Session, st);
                return Ok(new AiResponse(false, "Tell me to check availability or create a reservation."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new AiResponse(false, $"Server error: {ex.Message}"));
            }
        }

    }
}
