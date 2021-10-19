using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneratorDependencies;

namespace Hackathon21Poc.Probes
{
    public partial class MultiplierState : InterleaverState
    { }

    public partial class Multiplier : IGeneratorCapable
    {
        public void StatelessImplementation()
        {
            int x = 0;
        }

        public partial void GeneratedStatefulImplementation(MultiplierState state);
    }
}
