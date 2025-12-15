namespace GrapheneTrace.Models
{
    /// <summary>
    /// ErrorViewModel - used for displaying error pages
    /// </summary>
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
