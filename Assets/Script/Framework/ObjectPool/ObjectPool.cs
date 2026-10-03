using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class ObjectPool<T> where T : class
{
    private readonly Queue<T> _pool = new();
    private readonly Func<UniTask<T>> _createFunc;
    private readonly Action<T> _onGet;
    private readonly Action<T> _onReturn;

    public int Count => _pool.Count;

    public ObjectPool(Func<UniTask<T>> createFunc, Action<T> onGet = null, Action<T> onReturn = null)
    {
        _createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
        _onGet = onGet;
        _onReturn = onReturn;
    }

    public async UniTask PrewarmAsync(int count)
    {
        if (count <= 0)
            return;

        for (int i = 0; i < count; i++)
        {
            T item = await _createFunc();
            _onReturn?.Invoke(item);
            _pool.Enqueue(item);
        }
    }

    public async UniTask<T> GetAsync()
    {
        T item;

        if (_pool.Count > 0)
            item = _pool.Dequeue();
        else
            item = await _createFunc();

        _onGet?.Invoke(item);

        return item;
    }

    public void Return(T item)
    {
        if (item == null)
            return;

        _onReturn?.Invoke(item);
        _pool.Enqueue(item);
    }
}