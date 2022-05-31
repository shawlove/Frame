using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace GameFrame.Flux
{
    public class DispatchCenter : IDispatchCenter
    {
        private List<Middleware> _middlewares;

        public void AddMiddleware(Middleware middleware)
        {
            _middlewares.Add(middleware);
        }

        public async Task Dispatch(IAction action)
        {
            foreach (Middleware middleware in _middlewares)
            {
                try
                {
                    Task task = middleware(action);
                    if (task != null)
                    {
                        await task;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"middleware error :{e}");
                }
            }
        }

        public Middleware defaultMiddleware => Middleware;

        private Task Middleware(IAction action)
        {
            action.Recycle();
            return null;
        }
    }
}