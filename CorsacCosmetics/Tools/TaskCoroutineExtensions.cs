using System;
using System.Collections;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace CorsacCosmetics.Tools;

public static class TaskCoroutineExtensions
{
    public static IEnumerator AsIEnumerator(this Task task)
    {
        while (!task.IsCompleted)
        {
            yield return null;
        }

        if (task is { IsFaulted: true, Exception: not null })
        {
            ExceptionDispatchInfo.Capture(task.Exception.InnerException ?? task.Exception).Throw();
        }
    }

    public static IEnumerator AsIEnumerator<T>(this Task<T> task, Action<T> onComplete)
    {
        yield return task.AsIEnumerator();
        onComplete.Invoke(task.Result);
    }
}