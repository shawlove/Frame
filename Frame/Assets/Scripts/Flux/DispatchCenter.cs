using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace GameFrame.Flux
{
    public class DispatchCenter : IDispatchCenter
    {
        private List<Middleware> _middlewares;

        private          bool           _waiting;
        private readonly Queue<IAction> _queue = new Queue<IAction>();

        public void AddMiddleware(Middleware middleware)
        {
            _middlewares.Add(middleware);
        }

        public async Task Dispatch(IAction action)
        {
            _queue.Enqueue(action);

            if (_waiting) return;

            _waiting = true;
            while (_queue.Count > 0)
            {
                foreach (Middleware middleware in _middlewares)
                {
                    try
                    {
                        await middleware(action);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"middleware error :{e}");
                    }
                }
            }

            _waiting = false;
        }

        public Middleware DefaultMiddleware => Middleware;

        private Task Middleware(IAction action)
        {
            action.Recycle();
            return Task.CompletedTask;
        }
    }
}