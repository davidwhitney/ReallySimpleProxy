namespace ReallySimpleProxy.Host
{
    class Program
    {
        static void Main(string[] args)
        {
            var proxy = new ReallySimpleProxyHost(args, "ReallySimpleProxy.Host");
            proxy.Host("localhost", 12345);
        }
    }
}
