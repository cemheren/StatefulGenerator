# hackhaton21POC

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