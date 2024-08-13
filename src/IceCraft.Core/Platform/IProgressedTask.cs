namespace IceCraft.Core.Platform;

public interface IProgressedTask
{
    void SetDefiniteProgress(long progress, long max);
    void SetDefinitePrecentage(double precentage);
    void SetIntermediateProgress();
}
