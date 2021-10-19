using Program.Probes;

namespace TestProgram
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var userClass = new UserClass();
            userClass.RunAsync();
        }
    }
}
