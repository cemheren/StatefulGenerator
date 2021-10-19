namespace Hackathon21Poc
{
    using Hackathon21Poc.Probes;

    internal class Program
    {
        private static void Main(string[] args)
        {
            var userClass = new UserClass();
            userClass.RunAsync();
        }
    }
}
