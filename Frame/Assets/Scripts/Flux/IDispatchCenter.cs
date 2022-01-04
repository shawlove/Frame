using System.Threading.Tasks;

namespace GameFrame.Flux
{
    public delegate Task Middleware(IAction action);

    public interface IDispatchCenter
    {
        void AddMiddleware(Middleware middleware);

        Task Dispatch(IAction action);

        Middleware DefaultMiddleware { get; }
    }
}