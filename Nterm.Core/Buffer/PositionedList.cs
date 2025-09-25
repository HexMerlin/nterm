using System.Collections;

namespace Nterm.Core.Buffer;

/// <summary>
/// A position-keyed, generic list optimized for predecessor lookups (nearest previous position),
/// append-most workloads, and ordered iteration by position. Positions are integers in [0, MaxPosition],
/// and entries are unique by position.
/// </summary>
/// <typeparam name="T">Stored value type.</typeparam>
public sealed class PositionedList<T> : IEnumerable<(int position, T value)>
{
    private readonly List<int> positions = [];
    private readonly List<T> values = [];

    /// <summary>
    /// Creates a positioned list with a maximum valid position (exclusive upper bound).
    /// </summary>
    /// <param name="maxPosition">Exclusive upper bound for positions. Must be >= 0.</param>
    public PositionedList(int maxPosition)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxPosition);
        MaxPosition = maxPosition;
    }

    /// <summary>
    /// Number of stored entries (distinct positions with values).
    /// </summary>
    public int Count => positions.Count;

    /// <summary>
    /// Exclusive upper bound for valid positions. Valid positions are 0..MaxPosition-1.
    /// </summary>
    public int MaxPosition { get; private set; }

    /// <summary>
    /// Returns the value active at the specified position. If there is no entry at or before
    /// the position, returns default(T).
    /// </summary>
    public T this[int position]
    {
        get
        {
            if ((uint)position >= (uint)MaxPosition)
                throw new ArgumentOutOfRangeException(nameof(position));
            int idx = FindIndexOfPredecessor(position);
            return idx >= 0 ? values[idx] : default!;
        }
    }

    /// <summary>
    /// Gets the last (highest-position) item if any.
    /// </summary>
    public (int position, T value)? Last =>
        positions.Count > 0 ? (positions[^1], values[^1]) : null;

    /// <summary>
    /// Adds or replaces the value at the specified position. If an entry at the exact position exists,
    /// its value is replaced; otherwise a new entry is inserted keeping positions ordered.
    /// </summary>
    public void AddOrReplace(int position, T value)
    {
        if ((uint)position > (uint)MaxPosition)
            throw new ArgumentOutOfRangeException(nameof(position));

        int idx = positions.BinarySearch(position);
        if (idx >= 0)
        {
            values[idx] = value;
            return;
        }

        int insertIndex = ~idx; // index of first element larger than position
        positions.Insert(insertIndex, position);
        values.Insert(insertIndex, value);
    }

    /// <summary>
    /// Adds or replaces using a merge strategy with the value active at or before <paramref name="position"/>.
    /// If no predecessor exists, the left operand for <paramref name="merge"/> will be default(T).
    /// The merged value is then inserted/replaced at <paramref name="position"/>.
    /// </summary>
    public void AddOrReplace(int position, T value, Func<T, T, T> merge)
    {
        if ((uint)position > (uint)MaxPosition)
            throw new ArgumentOutOfRangeException(nameof(position));

        int predIndex = FindIndexOfPredecessor(position);
        T? baseValue = predIndex >= 0 ? values[predIndex] : default!;
        T? merged = merge(baseValue, value);
        AddOrReplace(position, merged);
    }

    /// <summary>
    /// Attempts to get the exact stored value at the specified position.
    /// </summary>
    public bool TryGetExact(int position, out T value)
    {
        int idx = positions.BinarySearch(position);
        if (idx >= 0)
        {
            value = values[idx];
            return true;
        }
        value = default!;
        return false;
    }

    /// <summary>
    /// Attempts to get the value active at or before the specified position.
    /// </summary>
    public bool TryGetAtOrBefore(int position, out T value)
    {
        int idx = FindIndexOfPredecessor(position);
        if (idx >= 0)
        {
            value = values[idx];
            return true;
        }
        value = default!;
        return false;
    }

    public IEnumerator<(int position, T value)> GetEnumerator()
    {
        for (int i = 0; i < positions.Count; i++)
        {
            yield return (positions[i], values[i]);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Finds the index of the greatest stored position <= target; returns -1 if none.
    /// </summary>
    private int FindIndexOfPredecessor(int target)
    {
        if (positions.Count == 0)
            return -1;
        int idx = positions.BinarySearch(target);
        if (idx >= 0)
            return idx; // exact match
        idx = ~idx - 1; // previous element
        return idx >= 0 ? idx : -1;
    }

    /// <summary>
    /// Resizes the maximum valid position. Shrinking removes entries that no longer fit.
    /// </summary>
    public void Resize(int newMaxPosition)
    {
        if (newMaxPosition < 0)
            throw new ArgumentOutOfRangeException(nameof(newMaxPosition));
        if (newMaxPosition == MaxPosition)
            return;

        if (newMaxPosition < MaxPosition)
        {
            // remove items with position >= newMaxPosition
            int idx = positions.BinarySearch(newMaxPosition);
            if (idx < 0)
                idx = ~idx; // first index with position > newMaxPosition - 1
            if (idx < positions.Count)
            {
                positions.RemoveRange(idx, positions.Count - idx);
                values.RemoveRange(idx, values.Count - idx);
            }
        }

        MaxPosition = newMaxPosition;
    }

    /// <summary>
    /// Appends another positioned list at the end of this list, offsetting all positions of
    /// <paramref name="other"/> by this list's current MaxPosition. The resulting MaxPosition
    /// becomes the sum of both lists' MaxPosition values.
    /// </summary>
    public void Append(PositionedList<T> other)
    {
        if (other is null)
            throw new ArgumentNullException(nameof(other));
        int offset = MaxPosition;
        foreach ((int pos, T val) in other)
        {
            AddOrReplace(offset + pos, val);
        }
        MaxPosition = checked(MaxPosition + other.MaxPosition);
    }

    /// <summary>
    /// Iterates over contiguous ranges [start, end) covering [0, MaxPosition), where each range
    /// carries the value active at its start.
    /// </summary>
    public IEnumerable<(int start, int end, T value)> GetRanges()
    {
        if (MaxPosition == 0)
            yield break;

        int start = 0;
        T? current = default(T)!;
        for (int i = 0; i < positions.Count; i++)
        {
            int end = positions[i];
            if (end > start)
            {
                yield return (start, end, current);
            }
            current = values[i];
            start = end;
        }

        if (start < MaxPosition)
        {
            yield return (start, MaxPosition, current);
        }
    }

    public bool Equals(PositionedList<T>? other)
    {
        if (ReferenceEquals(this, other))
            return true;
        if (other is null)
            return false;
        if (MaxPosition != other.MaxPosition)
            return false;
        if (positions.Count != other.positions.Count)
            return false;

        EqualityComparer<T> comparer = EqualityComparer<T>.Default;
        for (int i = 0; i < positions.Count; i++)
        {
            if (positions[i] != other.positions[i])
                return false;
            if (!comparer.Equals(values[i], other.values[i]))
                return false;
        }
        return true;
    }

    public override bool Equals(object? obj) => obj is PositionedList<T> pl && Equals(pl);

    public override int GetHashCode()
    {
        HashCode hc = new HashCode();
        hc.Add(MaxPosition);
        for (int i = 0; i < positions.Count; i++)
        {
            hc.Add(positions[i]);
            hc.Add(values[i]);
        }
        return hc.ToHashCode();
    }

    public static bool operator ==(PositionedList<T>? left, PositionedList<T>? right) =>
        ReferenceEquals(left, right) || (left is not null && left.Equals(right));

    public static bool operator !=(PositionedList<T>? left, PositionedList<T>? right) =>
        !(left == right);
}
