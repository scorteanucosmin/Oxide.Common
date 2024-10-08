using System.Diagnostics;

namespace Oxide.IO.TransportMethods
{
    public class ProcessTransportProtocol : StreamTransportProtocol
    {
        #region Default Settings

        public new const int DEFAULT_MAXBUFFERSIZE = 4096;

        public const bool DEFAULT_KILLONDISPOSE = false;

        #endregion

        #region Properties

        protected Process Proc { get; }

        protected bool KillOnDispose { get; }

        protected override bool CanWrite => Proc.StartInfo.RedirectStandardInput;

        protected override bool CanRead => Proc.StartInfo.RedirectStandardOutput;

        protected override object ReadLock => Proc.StandardOutput.BaseStream;

        protected override object WriteLock => Proc.StandardInput.BaseStream;

        #endregion

        public ProcessTransportProtocol(Process process, int maxBufferSize = DEFAULT_MAXBUFFERSIZE, bool killOnDispose = DEFAULT_KILLONDISPOSE) : base(process.StandardInput.BaseStream, maxBufferSize)
        {
            Proc = process;
            Proc.Exited += (s, e) => Dispose();
            Proc.Disposed += (s, e) => Dispose();
            Proc.EnableRaisingEvents = true;
            KillOnDispose = killOnDispose;
        }

        protected override int OnRead(byte[] buffer, int index, int count)
        {
            return Proc.StandardOutput.BaseStream.Read(buffer, index, count);
        }

        protected override void OnWrite(byte[] buffer, int index, int count)
        {
            Proc.StandardInput.BaseStream.Write(buffer, index, count);
        }

        protected override void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                Proc.EnableRaisingEvents = false;
                if (KillOnDispose && !Proc.HasExited && !Proc.WaitForExit(3000))
                {
                    Proc.Kill();
                }

                Proc.Dispose();
            }
        }
    }
}
