using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace myBackgroundTask
{
    public sealed class myTask: IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            taskInstance.Canceled += instance_Canceled;

            taskInstance.Progress = 0;
            deferral.Complete();
        }

        void instance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            //throw new NotImplementedException();
        }
    }
}
