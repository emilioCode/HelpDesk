namespace HelpDesk.Core.Constants
{
    public static class Messages
    {
        public const string Saved = "has been saved successfully";
        public const string Updated = "has been updated successfully";
        public const string Created = "created successfully";
        public const string Modified = "modified successfully";
        public const string OptionRequired = "the option is required";
        public const string FieldsRequired = "Field(s) required!";

        public static string AccountExists(string account) => $"the userAccount <b>{account}</b> already exists";

        public static string ServiceOrderTitle(string ticketNumber) => $"HelpDesk - Service Order No.{ticketNumber}";
    }
}
