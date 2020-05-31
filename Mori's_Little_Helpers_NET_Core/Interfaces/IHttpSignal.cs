namespace Schweigm_NETCore_Helpers.Interfaces
{
    public interface IHttpSignal
    {
        string ID { get; set; }
        string Value { get; set; }
        string ValueDataType { get; set; }
        string ValueUnit { get; set; }
        string Timestamp { get; set; }
    }
}
