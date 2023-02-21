namespace Oxide.Pooling
{
    public interface IClaimable<TInstance>
    {
        TInstance Claim();

        void Unclaim(ref TInstance instance);
    }

    public interface IClaimable<TInstance, TParameter> : IClaimable<TInstance>
    {
        TInstance Claim(TParameter parameter);
    }
}
