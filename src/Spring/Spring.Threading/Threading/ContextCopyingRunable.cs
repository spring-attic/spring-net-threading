using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading;

namespace Spring.Threading
{
    public class ContextCopyingRunable : IRunnable, IContextCopier
    {
        private ContextCarrier _contextCarrier;
        private Action _action;

        private ContextCopyingRunable(IEnumerable<string> names)
        {
            if (names == null) throw new ArgumentNullException("names");
            _contextCarrier = new ContextCarrier(names);
        }

        public ContextCopyingRunable(Action action, IEnumerable<string> names)
            :this(names)
        {
            if (action==null) throw new ArgumentNullException("task");
            _action = action;
        }


        public ContextCopyingRunable(IRunnable runnable, IEnumerable<string> names)
            :this(names)
        {
            if (runnable == null) throw new ArgumentNullException("runnable");
            _action = runnable.Run;
        }

        public void Run()
        {
            _contextCarrier.RestoreContext();
            _action();
        }
    }
}
