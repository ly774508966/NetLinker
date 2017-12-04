using System;
using System.Collections.Generic;
using System.IO;


namespace Meow.NetLinker
{
    public class MsgData
    {
        public int Index;
        public int Length;
        public object Data;
        public Type DataType;
    }

    // ReSharper disable once PartialTypeWithSinglePart
    public partial class DataPack
    {
        public readonly Dictionary<string, MsgData> DataDict = new Dictionary<string, MsgData>();
        
        private readonly byte[] _bytesArray = new byte[8192];
        private int _headIndex = 1024;
        private int _endIndex;

        private readonly Dictionary<Type, int> _typeByteLength = new Dictionary<Type, int>
        {
            {typeof(bool), 1},
            {typeof(byte), 1},
            {typeof(char), 1},
            {typeof(decimal), 16},
            {typeof(double), 8},
            {typeof(float), 4},
            {typeof(int), 4},
            {typeof(long), 8},
            {typeof(sbyte), 1},
            {typeof(short), 2},
            {typeof(uint), 4},
            {typeof(ulong), 8},
            {typeof(ushort), 2}
        };

        private readonly MemoryStream _stream;
        public readonly BinaryReader Reader;
        public readonly BinaryWriter Writer;

        public long Position
        {
            get { return _stream.Position; }
            set { _stream.Position = value; }
        }

        public DataPack()
        {
            _stream = new MemoryStream(_bytesArray);
            Reader = new BinaryReader(_stream);
            Writer = new BinaryWriter(_stream);
        }

        public void WriteBytes(int index, byte[] bytes)
        {
            WriteBytes(index, bytes, bytes.Length);
        }

        public void WriteBytes(int index, byte[] bytes, int length)
        {
            _stream.Position = index;
            Writer.Write(bytes);
            if (index < _headIndex)
            {
                _headIndex = index;
            }
            if (index + length > _endIndex)
            {
                _endIndex = index + length;
            }
        }
        
        public void PrepareWriteBefore(Type type)
        {
            var length = _typeByteLength[type];
            PrepareWriteBefore(length);
        }
        
        public void PrepareWriteBefore(int len)
        {
            var length = len;
            var index = _headIndex - length;
            _stream.Position = index;
            _headIndex = index;
            if (index + length > _endIndex)
            {
                _endIndex = index + length;
            }
        }

        public void WriteBefore(byte[] bytes)
        {
            var length = bytes.Length;
            var index = _headIndex - length;
            _stream.Position = index;
            Writer.Write(bytes);
            _headIndex = index;
            if (index + bytes.Length > _endIndex)
            {
                _endIndex = index + bytes.Length;
            }
        }

        public void PrepareWriteAfter(Type type)
        {
            var length = _typeByteLength[type];
            PrepareWriteAfter(length);
        }

        public void PrepareWriteAfter(int len)
        {
            var length = len;
            var index = _endIndex;
            _stream.Position = index;
            if (index < _headIndex)
            {
                _headIndex = index;
            }
            _endIndex = index + length;
        }

        public void WriteAfter(byte[] bytes)
        {
            var length = bytes.Length;
            var index = _endIndex;
            _stream.Position = index;
            Writer.Write(bytes);
            if (index < _headIndex)
            {
                _headIndex = index;
            }
            _endIndex = index + length;
        }
        
        /// <summary>
        /// Read all bytes in data pack, but keep stream position not change
        /// </summary>
        /// <returns></returns>
        public byte[] ReadAllBytes()
        {
            var tempPostion = _stream.Position;
            _stream.Position = _headIndex;
            var result = Reader.ReadBytes(_endIndex - _headIndex);
            _stream.Position = tempPostion;
            return result;
        }

        public DataPack Recycle()
        {
            DataDict.Clear();
            Array.Clear(_bytesArray, 0, _bytesArray.Length);
            _stream.Position = 0;
            _headIndex = 1024;
            _endIndex = 0;
            return this;
        }

        public void TryAddDataToDict(string name, int index, int length, object data, Type dataType)
        {
            if (!DataDict.ContainsKey(name))
            {
                DataDict.Add(name, new MsgData());
            }
            if (index != -1)
            {
                DataDict[name].Index = index;
            }
            if (length != -1)
            {
                DataDict[name].Length = length;
            }
            if (data != null)
            {
                DataDict[name].Data = data;
            }
            if (dataType != null)
            {
                DataDict[name].DataType = dataType;
            }
        }
    }
}
