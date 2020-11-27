// Copyright (c) James Jackson-South.
// See LICENSE for more details.

using System.Threading;
using System.Threading.Tasks;

// This class should really be internal and would be in Umbraco.
namespace FileSystem.Abstractions
{
    /// <summary>
    /// Provides useful extension methods for <see cref="ValueTask"/> that are only present in .NET &gte; 5.0.
    /// <see cref="https://source.dot.net/#System.Private.CoreLib/ValueTask.cs,ad5707f8dfdb165b"/>
    /// </summary>
    public static class ValueTaskEx
    {
        /// <summary>
        /// Creates a <see cref="ValueTask{TResult}"/> that's completed successfully with the specified result.
        /// </summary>
        /// <typeparam name="TResult">The type of the result returned by the task.</typeparam>
        /// <param name="result">The result to store into the completed task.</param>
        /// <returns>The successfully completed task.</returns>
        public static ValueTask<TResult> FromResult<TResult>(TResult result)
            => new ValueTask<TResult>(result);

        /// <summary>
        /// Creates a <see cref="ValueTask"/> that has completed due to cancellation with
        /// the specified cancellation token.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token with which to complete the task.</param>
        /// <returns>The canceled task.</returns>
        public static ValueTask FromCanceled(CancellationToken cancellationToken)
            => new ValueTask(Task.FromCanceled(cancellationToken));

        /// <summary>
        /// Creates a <see cref="ValueTask{TResult}"/> that has completed due to cancellation with
        /// the specified cancellation token.
        /// </summary>
        /// <typeparam name="TResult">The type of the result returned by the task.</typeparam>
        /// <param name="cancellationToken">The cancellation token with which to complete the task.</param>
        /// <returns>The canceled task.</returns>
        public static ValueTask<TResult> FromCanceled<TResult>(CancellationToken cancellationToken)
            => new ValueTask<TResult>(Task.FromCanceled<TResult>(cancellationToken));
    }
}
