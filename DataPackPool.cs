using System.Collections.Generic;

namespace Meow.NetLinker
{
    public static class DataPackPool
    {
        private static readonly Queue<DataPack> DataPacks = new Queue<DataPack>();

        public static DataPack GetDataPack()
        {
            var result = DataPacks.Count > 0 ? DataPacks.Dequeue() : new DataPack();
            return result;
        }
        
        public static DataPack GetDataPack(int index, byte[] msgByte)
        {
            var result = DataPacks.Count > 0 ? DataPacks.Dequeue() : new DataPack();
            result.WriteBytes(index, msgByte);
            return result;
        }
        
        public static DataPack GetDataPack(int index, byte[] msgByte, int length)
        {
            var result = DataPacks.Count > 0 ? DataPacks.Dequeue() : new DataPack();
            result.WriteBytes(index, msgByte, length);
            return result;
        }

        public static void Recycle(DataPack dataPack)
        {
            DataPacks.Enqueue(dataPack.Recycle());
        }
    }

}
