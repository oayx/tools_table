using JsonFx.Json;
using System.Collections;
using System.Collections.Generic;

namespace tab
{
    [System.Serializable]
    public abstract class JsonTableRow<K>
    {
        public abstract K ID { get; }
    }
    [System.Serializable]
    public class JsonDataSrc<K, V> where V : JsonTableRow<K>
    {
        public V[] array;
    }
    [System.Serializable]
    public class JsonTable<K, V> : IEnumerable<V>, IEnumerable where V : JsonTableRow<K>
    {
        Dictionary<K, V> table;
        V[] array;

        public JsonTable(JsonDataSrc<K, V> dataSrc)
        {
            array = dataSrc.array;
            table = new Dictionary<K, V>(array.Length);
            for (int i = 0; i < array.Length; i++)
            {
                V row = array[i];
                table.Add(row.ID, row);
            }
        }
        
        public V[] GetArray()
        {
            return array;
        }

        public V GetRow(K id)
        {
            V t;
            table.TryGetValue(id, out t);
            return t;
        }

        public bool Contains(K id)
        {
            return table.ContainsKey(id);
        }

        #region 遍历
        public NodeEnumerator GetEnumerator()
        {
            return new NodeEnumerator(array);
        }
        IEnumerator<V> IEnumerable<V>.GetEnumerator()
        {
            return new NodeEnumerator(array);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new NodeEnumerator(array);
        }
        public struct NodeEnumerator : IEnumerator<V>, IEnumerator
        {
            private V[] list;
            private int index;
            private V current;

            internal NodeEnumerator(V[] list)
            {
                this.list = list;
                index = 0;
                current = default(V);
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (index < list.Length)
                {
                    current = list[index];
                    index++;
                    return true;
                }
                else
                {
                    index = -1;
                    current = default(V);
                    return false;
                }
            }

            public V Current
            {
                get
                {
                    return current;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }

            void IEnumerator.Reset()
            {
                index = 0;
                current = default(V);
            }

            bool IEnumerator.MoveNext()
            {
                throw new System.NotImplementedException();
            }
        }
        #endregion
    }

    public class JsonLoader
    {
        public JsonDataSrc<K, T> Load<K, T>(string ta) where T : JsonTableRow<K>
        {
            if (!string.IsNullOrEmpty(ta))
            {
                return JsonReader.Deserialize<JsonDataSrc<K, T>>(ta);
            }
            return null;
        }
    }
}