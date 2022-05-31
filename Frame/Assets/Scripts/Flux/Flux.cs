namespace GameFrame.Flux
{
    public class Flux : Singleton<Flux>
    {
        public IDispatchCenter DispatchCenter { get; private set; }

        public Flux()
        {
            DispatchCenter = new DispatchCenter();

            //add middleware here
            DispatchCenter.AddMiddleware(DispatchCenter.defaultMiddleware);
        }
    }
}