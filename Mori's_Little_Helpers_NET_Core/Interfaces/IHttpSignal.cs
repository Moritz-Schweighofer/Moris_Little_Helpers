namespace Schweigm_NETCore_Helpers.Interfaces
{
    public interface IHttpSignal
    {
        string ID { get; }

        string Item { get; set; }
        string Value { get; set; }
        string ValueUnit { get; set; }
        string Timestamp { get; set; }
    }
}
