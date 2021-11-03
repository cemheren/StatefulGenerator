# StatefulGenerator

A C# source generator for creating stateful methods with checkpoints. 

This generator aims to let the user split up responsibilities when writing stateful code. The consumer can write non stateful code with arbitrary checkpoints and can 
use the generated code for stateful applications. 

Stateful generation is separately tested so in theory you can write bug free code. And easily unit test your work. 

Development stages: 
+ POC
- Single method fully converted to stateful 
- Handle sub-scopes (ifs/whiles/fors) - We are here. 
- Handle sub methods 

- Maybe I'll enable converting whole programs to stateful 
- Optionally skip states if the result is cacheable. 
    ex: checkpoint 2 always returns 10 when checkpoint 1 is 2. In this case we may skip all the instructions between checkpoint 1 and 2.

Generete this: 

```
        public partial void GeneratedStatefulImplementation(UserClassState state)
        {
                
            if (state.ExecutionState == 0) {
                state.x = 1;
                state.y = 10;

                state.ExecutionState = 1;
                state.CurrentStateStartTime = DateTime.UtcNow;
                return;
            }
                

            if (state.ExecutionState == 1) {

                if (state.x == 1)
                {
                    state.x = Enumerable.Range(state.x, 2).Sum();
                }


                state.ExecutionState = 2;
                state.CurrentStateStartTime = DateTime.UtcNow;
                return;
            }
                

            if (state.ExecutionState == 2) {

                while (state.x == 3)
                {
                    state.x = 4;
                }


                state.ExecutionState = 3;
                state.CurrentStateStartTime = DateTime.UtcNow;
                return;
            }
                

            if (state.ExecutionState == 3) {

                Console.WriteLine(state.x);


                state.ExecutionState = -1;
                state.CurrentStateStartTime = DateTime.UtcNow;
                return;
            }
                

            System.Diagnostics.Debug.WriteLine("test");
        }
```

based from this:

```
        public void StatelessImplementation()
        {
            int x = 1;
            int y = 10;

            Interleaver.Pause();

            if (x == 1)
            {
                x = Enumerable.Range(x, 2).Sum();
            }
            Interleaver.Pause();

            while (x == 3)
            {
                x = 4;
            }
            Interleaver.Pause();

            Console.WriteLine(x);
        }
```
