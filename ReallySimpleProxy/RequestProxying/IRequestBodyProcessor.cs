namespace ReallySimpleProxy.RequestProxying
{
    public interface IRequestBodyProcessor
    {
        string ProcessBody(string currentBody);
    }
}