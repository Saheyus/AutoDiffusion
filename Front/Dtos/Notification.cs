namespace AutoDiffusion.Dtos
{
    public class Notification
    {
        public DateTime DateTime { get; set; }
        public string NotificationType { get; set; }
        public object Data { get; set; }
    }
}
