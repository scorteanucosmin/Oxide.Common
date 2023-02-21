using Oxide.CompilerServices.Messages;
using System;

namespace Oxide.CompilerServices
{
    public interface ICodeProvider
    {
        /// <summary>
        /// Language Id
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Language Name
        /// </summary>
        string Name { get; }

        CompilerMessage Run(CompileRequestMessage request);
    }
}
