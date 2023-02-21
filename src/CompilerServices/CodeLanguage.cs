using System;

namespace Oxide.CompilerServices
{
    [Flags]
    [Serializable]
    public enum CodeLanguage
    {
        /// <summary>
        /// CSharp Language
        /// </summary>
        CSharp = 0x00,

        /// <summary>
        /// Visual Basic Language
        /// </summary>
        VisualBasic = 0x01,

        /// <summary>
        /// Already compiled DLL file hot loadable
        /// </summary>
        Compiled = 0x02
    }
}
