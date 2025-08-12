namespace RestaurantReservationSystem.Services.OpenAI
{
    public class OpenAiOptions
    {
        public string ApiKey { get; set; } = "";
        public string Model { get; set; } = "gpt-4o-mini";
    }
}
