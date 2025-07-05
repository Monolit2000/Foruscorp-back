namespace Foruscorp.Push.Contruct
{
    public interface IExpoPushService
    {
        Task SendAsync(string pushToken, string title, string body, IDictionary<string, object> data);
    }
}
