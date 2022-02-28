//namespace Program.Probes
//{
//    using System;
//    using System.Threading.Tasks;
//    using GeneratorDependencies;
//    using System.Linq;

//    public partial class UserClassState : InterleaverState
//    { }

//    public partial class UserClass : IGeneratorCapable
//    {
//        public void StatelessImplementation()
//        {
//            int x = 1;
//            int y = 10;

//            Interleaver.Pause();

//            if (x == 1)
//            {
//                x = Enumerable.Range(x, 2).Sum();
//            }
//            Interleaver.Pause();

//            while (x == 3)
//            {
//                x = 4;
//            }
//            Interleaver.Pause();

//            Console.WriteLine(x);
//        }

//        public void RunAsync() {
//            var currentState = new UserClassState();
//            while (currentState.ExecutionState != -1)
//            {
//                Console.WriteLine($"Running state {currentState.ExecutionState}");
//                this.GeneratedStatefulImplementation(currentState);
//                Console.WriteLine("Simulating delay between state executions");
//                Task.Delay(TimeSpan.FromSeconds(2)).Wait();
//            }
            
//        }

//        public partial void GeneratedStatefulImplementation(UserClassState state);
//    }
//}
