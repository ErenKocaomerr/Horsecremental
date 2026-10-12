namespace Horsecremental.Racing
{
    public interface IIdleSettings
    {
        float TrainingSeconds { get; }
        int TrainingReward { get; }
        int TrainingMetal { get; }
        float CarrotSeconds { get; }
        int FarmCapacity { get; }
        int FeedCost { get; }
        float FeedSpeed { get; }
    }
}
