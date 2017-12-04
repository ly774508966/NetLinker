namespace Meow.NetLinker
{
    public delegate void RecvFunc(DataPack dataPack);

    public delegate void OutPutFunc(DataPack dataPack);
    
    public abstract class Layer
    {
        public RecvFunc Recv { get; set; }
        public OutPutFunc OutPut { get; set; }

        public abstract void Send(DataPack dataPack);
        public abstract void Input(DataPack dataPack);

        public abstract void Update();
        public abstract void Dispose();

        public void Link(Layer lowerlayer)
        {
            OutPut += lowerlayer.Send;
            lowerlayer.Recv += Input;
        }
    }
}