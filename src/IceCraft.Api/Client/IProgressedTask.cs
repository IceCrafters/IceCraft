namespace IceCraft.Api.Client;

public interface IProgressedTask
{
    void SetDefiniteProgress(long progress, long max);
    void SetDefinitePrecentage(double precentage);
    void SetIntermediateProgress();
    void SetText(string text);
}
