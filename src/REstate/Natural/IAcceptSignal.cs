namespace REstate.Natural
{
    public interface IAcceptSignal<TSignal>
        : INaturalAction<TSignal>
        , INaturalPrecondition<TSignal>
    {
    }
}