using System.Collections;

namespace Meow.NetLinker
{
    public static class Utility
    {

        public static void Swap<T>(ref T t1, ref T t2)
        {

            var temp = t1;
            t1 = t2;
            t2 = temp;
        }
    }

    public class SwitchQueue<T> where T : class
    {

        private Queue _consumeQueue;
        private Queue _produceQueue;

        public SwitchQueue()
        {
            _consumeQueue = new Queue(16);
            _produceQueue = new Queue(16);
        }

        public SwitchQueue(int capcity)
        {
            _consumeQueue = new Queue(capcity);
            _produceQueue = new Queue(capcity);
        }

        // producer
        public void Push(T obj)
        {
            lock (_produceQueue)
            {
                _produceQueue.Enqueue(obj);
            }
        }

        // consumer.
        public T Pop()
        {

            return (T)_consumeQueue.Dequeue();
        }

        public bool Empty()
        {
            return 0 == _consumeQueue.Count;
        }

        public void Switch()
        {
            lock (_produceQueue)
            {
                Utility.Swap(ref _consumeQueue, ref _produceQueue);
            }
        }

        public void Clear()
        {
            lock (_produceQueue)
            {
                _consumeQueue.Clear();
                _produceQueue.Clear();
            }
        }
    }


}
