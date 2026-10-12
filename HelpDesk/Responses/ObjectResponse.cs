namespace HelpDesk.Responses
{
    public class ObjectResponse
    {
        public string title { get; set; }
        public string message { get; set; }
        public string icon { get; set; }
        public string code { get; set; }
        public object data { get; set; }

        // Success response that the front end recognizes with code "1".
        public static ObjectResponse Saved(string message, object data)
        {
            return new ObjectResponse
            {
                code = "1",
                title = "Saved",
                icon = "success",
                message = message,
                data = data
            };
        }
    }
}
