using System;

namespace Weikio.EventFramework.EventSource
{
    // public static class JobOptionsExtensions
    // {
    //     public static bool IsStateless(this JobOptions jobOptions)
    //     {
    //         if (jobOptions == null)
    //         {
    //             throw new ArgumentNullException(nameof(jobOptions));
    //         }
    //
    //         var action = jobOptions.Action;
    //
    //         return !action.Method.ReturnType.IsGenericType;
    //     }
    //
    //     public static bool IsStateful(this JobOptions jobOptions)
    //     {
    //         return jobOptions.IsStateless() == false;
    //     }
    // }
}
