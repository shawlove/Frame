namespace TheKiwiCoder
{
    public class Jump : CompositeNode
    {
        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

        protected override State OnUpdate()
        {
            return children[0].Update();
        }
    }
}