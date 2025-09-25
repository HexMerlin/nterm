using System.Collections;

namespace Nterm.Core.Buffer;

public record struct ValueInterval<T>(int Start, int End, T Value);

/// <summary>
/// A position-keyed, generic list optimized for interval lookups (start and end for associated value),
/// append-most workloads, and ordered iteration by position. Positions are integers
/// in [0, MaxPosition) (exclusive upper bound) and entries are unique by position.
/// </summary>
/// <typeparam name="T">Stored type. Must have a default constructor.</typeparam>
public sealed class ValueIntervalList<T> : IEnumerable<ValueInterval<T>>
    where T : new()
{
    private readonly List<int> positions = [];
    private readonly List<T> values = [];

    /// <summary>
    /// Creates a positioned list with range [0, maxPosition).
    /// </summary>
    public ValueIntervalList(int maxPosition)
        : this(0, maxPosition) { }

    /// <summary>
    /// Creates a positioned list with range [minPosition, maxPosition).
    /// </summary>
    public ValueIntervalList(int minPosition, int maxPosition)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(minPosition, maxPosition);
        MinPosition = minPosition;
        MaxPosition = maxPosition;
    }

    /// <summary>
    /// Number of stored entries (distinct positions with values).
    /// </summary>
    public int Count => positions.Count;

    /// <summary>
    /// Inclusive lower bound for valid positions.
    /// </summary>
    public int MinPosition { get; private set; }

    /// <summary>
    /// Exclusive upper bound for valid positions. Valid positions are MinPosition..MaxPosition-1.
    /// </summary>
    public int MaxPosition { get; private set; }

    /// <summary>
    /// Returns the value active at the specified position. If there is no entry at or before
    /// the position, returns new T.
    /// </summary>
    public T this[int position]
    {
        get
        {
            if ((uint)(position - MinPosition) >= (uint)(MaxPosition - MinPosition))
                throw new ArgumentOutOfRangeException(nameof(position));
            int idx = FindIndexOfPredecessor(position);
            return idx >= 0 ? values[idx] : new T();
        }
    }

    /// <summary>
    /// Gets the last (highest-position) item if any.
    /// </summary>
    public ValueInterval<T> Last =>
        positions.Count > 0
            ? new ValueInterval<T>(positions[^1], MaxPosition, values[^1])
            : new ValueInterval<T>(MinPosition, MaxPosition, new T());

    /// <summary>
    /// Adds or replaces the value at the specified position. If an entry at the exact position exists,
    /// its value is replaced; otherwise a new entry is inserted keeping positions ordered.
    /// </summary>
    public void AddOrReplace(int position, T value)
    {
        if ((uint)(position - MinPosition) >= (uint)(MaxPosition - MinPosition))
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
    /// If no predecessor exists, the left operand for <paramref name="merge"/> will be new T.
    /// The merged value is then inserted/replaced at <paramref name="position"/>.
    /// </summary>
    public void AddOrReplace(int position, T value, Func<T, T, T> merge)
    {
        if ((uint)(position - MinPosition) >= (uint)(MaxPosition - MinPosition))
            throw new ArgumentOutOfRangeException(nameof(position));

        int predIndex = FindIndexOfPredecessor(position);
        T? baseValue = predIndex >= 0 ? values[predIndex] : new T();
        T? merged = merge(baseValue, value);
        AddOrReplace(position, merged);
    }

    /// <summary>
    /// Gets the value active at or before the specified position. Returns a new T if no predecessor exists.
    /// </summary>
    public ValueInterval<T> GetAtOrBefore(int position)
    {
        if (positions.Count == 0)
            return new ValueInterval<T>(MinPosition, MaxPosition, new T());

        int idx = FindIndexOfPredecessor(position);
        if (idx < 0)
        {
            int firstEnd = positions.Count > 0 ? positions[0] : MaxPosition;
            return new ValueInterval<T>(MinPosition, firstEnd, new T());
        }

        int start = positions[idx];
        int end = idx + 1 < positions.Count ? positions[idx + 1] : MaxPosition;
        return new ValueInterval<T>(start, end, values[idx]);
    }

    public IEnumerator<ValueInterval<T>> GetEnumerator()
    {
        for (int i = 0; i < positions.Count; i++)
        {
            int end = i + 1 < positions.Count ? positions[i + 1] : MaxPosition;
            yield return new ValueInterval<T>(positions[i], end, values[i]);
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
        ArgumentOutOfRangeException.ThrowIfLessThan(newMaxPosition, MinPosition);
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
    /// <paramref name="other"/> by (this.MaxPosition - other.MinPosition). The resulting MaxPosition
    /// increases by the length of the other list (other.MaxPosition - other.MinPosition).
    /// </summary>
    public void Append(ValueIntervalList<T> other)
    {
        ArgumentNullException.ThrowIfNull(other);
        int offset = MaxPosition - other.MinPosition;
        foreach ((int start, _, T val) in other)
        {
            AddOrReplace(offset + start, val);
        }
        MaxPosition = checked(MaxPosition + (other.MaxPosition - other.MinPosition));
    }

    /// <summary>
    /// Replaces the interval [start, end) with the specified value. Removes any change points
    /// inside the range, inserts a change at <paramref name="start"/> if needed, and restores
    /// the prior value at <paramref name="end"/> if it differs from <paramref name="value"/>.
    /// </summary>
    public void InsertAndReplace(int start, int end, T value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(start);
        ArgumentOutOfRangeException.ThrowIfNegative(end);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(start, end, nameof(start));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(end, MaxPosition, nameof(end));
        if (start == end)
            return; // no-op

        EqualityComparer<T> cmp = EqualityComparer<T>.Default;

        // Capture values before mutation
        T beforeValue = this[start];
        bool hasAfter = end < MaxPosition;
        T afterValue = hasAfter ? this[end] : new T();

        // Remove all change points in [start, end)
        int left = positions.BinarySearch(start);
        if (left < 0)
            left = ~left;
        int right = positions.BinarySearch(end);
        if (right < 0)
            right = ~right; // first index with position >= end
        int removeCount = right - left;
        if (removeCount > 0)
        {
            positions.RemoveRange(left, removeCount);
            values.RemoveRange(left, removeCount);
        }

        // Ensure start has the new value if it differs from the active value at start
        if (!cmp.Equals(beforeValue, value))
        {
            AddOrReplace(start, value);
        }

        // Restore the value at end if needed
        if (hasAfter && !cmp.Equals(afterValue, value))
        {
            AddOrReplace(end, afterValue);
        }
    }

    /// <summary>
    /// Iterates over contiguous ranges [start, end) covering [MinPosition, MaxPosition), where each range
    /// carries the value active at its start.
    /// </summary>
    public IEnumerable<ValueInterval<T>> GetRanges()
    {
        if (MaxPosition == MinPosition)
            yield break;

        int start = MinPosition;
        T current = new();
        for (int i = 0; i < positions.Count; i++)
        {
            int end = positions[i];
            if (end > start)
            {
                yield return new ValueInterval<T>(start, end, current);
            }
            current = values[i];
            start = end;
        }

        if (start < MaxPosition)
        {
            yield return new ValueInterval<T>(start, MaxPosition, current);
        }
    }

    public bool Equals(ValueIntervalList<T>? other)
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

    public override bool Equals(object? obj) => obj is ValueIntervalList<T> pl && Equals(pl);

    public override int GetHashCode()
    {
        HashCode hc = new();
        hc.Add(MaxPosition);
        for (int i = 0; i < positions.Count; i++)
        {
            hc.Add(positions[i]);
            hc.Add(values[i]);
        }
        return hc.ToHashCode();
    }

    public static bool operator ==(ValueIntervalList<T>? left, ValueIntervalList<T>? right) =>
        ReferenceEquals(left, right) || (left is not null && left.Equals(right));

    public static bool operator !=(ValueIntervalList<T>? left, ValueIntervalList<T>? right) =>
        !(left == right);
}
