namespace EmaStrategy.Core;

public sealed class EmaSet
{
    private readonly int[] _periods;
    private readonly Dictionary<int, decimal?> _emaByPeriod;
    private readonly Dictionary<int, decimal> _kByPeriod;

    public int MaxPeriod => _periods[^1];

    public EmaSet(IEnumerable<int> periods)
    {
        _periods = periods
            .Distinct()
            .Where(p => p > 0)
            .OrderBy(p => p)
            .ToArray();

        if (_periods.Length == 0)
            throw new ArgumentException("At least one EMA period is required.", nameof(periods));

        _emaByPeriod = new Dictionary<int, decimal?>(_periods.Length);
        _kByPeriod = new Dictionary<int, decimal>(_periods.Length);

        foreach (var p in _periods)
        {
            _emaByPeriod[p] = null;
            var k = (decimal)2 / (p + 1);
            _kByPeriod[p] = k;
        }
    }

    public void Update(decimal price)
    {
        foreach (var p in _periods)
        {
            var current = _emaByPeriod[p];
            var k = _kByPeriod[p];
            if (current is null)
            {
                _emaByPeriod[p] = price;
                continue;
            }

            // EMA_t = price_t * k + EMA_{t-1} * (1-k)
            var next = price * k + current.Value * (1 - k);
            _emaByPeriod[p] = next;
        }
    }

    public IReadOnlyDictionary<int, decimal> GetValues()
    {
        var result = new Dictionary<int, decimal>(_periods.Length);
        foreach (var p in _periods)
        {
            var v = _emaByPeriod[p];
            if (v is not null)
                result[p] = v.Value;
        }
        return result;
    }

    public int[] Periods => _periods;
}

