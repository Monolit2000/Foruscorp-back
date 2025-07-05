namespace Foruscorp.Push.Domain.Devices
{
    public record ExpoPushToken
    {
        public string Value { get; init; }

        public ExpoPushToken(string value)
        {
            Value = value;
        }

        public override string ToString() => Value;
    }
}
