using System;

public interface IDeactivatable<T>
{
    public event Action<T> Deactivated;

    public void Deactivate();
}