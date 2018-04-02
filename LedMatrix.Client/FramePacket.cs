using System;
using System.Linq;

namespace LedMatrix.Client
{
   public class FramePacket
   {
      private byte _Header;
      private byte _MatrixId;
      private byte[] _FrameData;

      public FramePacket(byte header, byte matrixId, byte frameData)
         : this(header, matrixId, Enumerable.Repeat(frameData, 8).ToArray())
      {
      }

      public FramePacket(byte header, byte matrixId, byte[] frameData)
      {
         _Header = header;
         _MatrixId = matrixId;
         _FrameData = frameData;
      }

      public byte Header { get { return _Header; } }
      public byte MatrixId { get { return _MatrixId; } }
      public byte[] FrameData { get { return (byte[])_FrameData.Clone(); } }

      public override string ToString()
      {
         string header = $"{_Header:X2}{_MatrixId:X2}";
         string body = string.Join("", _FrameData.Select(b => $"{b:X2}"));
         return header + body;
      }
   }
}
