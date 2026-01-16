using System;

public interface ILoadInterest {
    event Action<string[]> OnInterestLoaded;
    void RegisterInterest( string interest );
    void RemoveInterest( string interest );
}
