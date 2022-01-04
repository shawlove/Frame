namespace GameFrame.Flux
{
    public class Flux : GlobalManager<Flux>
    {
        public IDispatchCenter DispatchCenter { get; private set; }

        Flux()
        {
            DispatchCenter = new DispatchCenter();

            //add middleware here
            DispatchCenter.AddMiddleware(DispatchCenter.DefaultMiddleware);
        }
    }
}