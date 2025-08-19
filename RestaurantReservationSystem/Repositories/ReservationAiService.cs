using System.Text.Json;
using System.Text.RegularExpressions;

namespace RestaurantReservationSystem.Repositories
{
    /// <summary>
    /// Uses the chat provider to parse a user's natural-language request
    /// into a structured intent.
    /// </summary>

    public class ReservationAiService
    {
        private readonly IChatProvider _chatProvider;
        private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

        public ReservationAiService(IChatProvider chatProvider)
        {
            _chatProvider = chatProvider;
        }

        public record AiIntent(
            string Action, //"Check Availability" | "Create Reservation"
            string? Date,  //"YYYY-MM-DD
            string? Time,  //"HH:mm"
            int? PartySize,
            string? Name,
            string? Phone,
            string? Response
            );

        private const string SystemPrompt = """
        You are a reservation AI. Extract the user's intent for a restaurant booking.
        - Allowed Action values: "CheckAvailability" or "CreateReservation".
        - Return ONLY strict JSON (no prose, no code fences).
        - If any field is missing, set it to null. Do not invent values.
        - If any information is missing, ask for it in the Response
        - Schema:
          {
            "Action": "CheckAvailability" | "CreateReservation",
            "Date": "YYYY-MM-DD" | null,
            "Time": "HH:mm" | null,
            "PartySize": number | null,
            "Name": string | null,
            "Phone": string | null,
            "Response": string 
          }
        """;

        public async Task<AiIntent> ParseAsync(string userMessage, CancellationToken ct = default)
        {
            var messages = new (string role, string content)[]
            {
                ("System", SystemPrompt),
                ("user", userMessage)
            };

            var raw = await _chatProvider.ChatAsync(messages, ct);


            //in case a model wraps output in fences, strip them safely.
            raw = Regex.Replace(raw, "^```(json)?\\s*|\\s*```$", "", RegexOptions.Multiline);

            var intent = JsonSerializer.Deserialize<AiIntent>(raw, _json)
                            ?? throw new InvalidOperationException("AI returned unparseable intent.");

            return intent;
        }
    }
}
