using System;
using ReadyPlayerMe.Core;

[Serializable]
public class UpdatedItems {
    // public UpdatableItem<BadgeResponse> updatedBadges;
    public UpdatableItem<IELEvent> updatedGroups;

    [Serializable]
    public class UpdatableItem<T> : IUpdatableItem<T> where T : IResponse {
        public T[] added, removed;
        public T[] Added => added;
        public T[] Removed => removed;
    }
}

public interface IUpdatableItem<out T> {
    T[] Added { get; }
    T[] Removed { get; }
}