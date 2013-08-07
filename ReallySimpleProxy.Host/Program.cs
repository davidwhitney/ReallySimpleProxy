namespace ReallySimpleProxy.Host
{
    class Program
    {
        static void Main(string[] args)
        {
            new ReallySimpleProxyHost().SelfHost(args, "ReallySimpleProxy.Host", "localhost", 12345);
        }
    }
}
