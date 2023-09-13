using System.Collections.Generic;
using System.IO;
using ProtoBuf;

namespace tab
{
    public abstract class PBTableRow<K>
    {
        public abstract K ID { get; }
    }

    [ProtoContract]
    public class PBDataSrc<K, V> where V : PBTableRow<K>
    {
        [ProtoMember(1)]
        public V[] array;
    }

    public class PBTable<K, V> where V : PBTableRow<K>
    {
        Dictionary<K, V> table;
        V[] array;

        public PBTable(PBDataSrc<K, V> dataSrc)
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
    }

#if UNITY_2017_1_OR_NEWER
    public class PBLoader
    {
        public PBDataSrc<K, T> Load<K, T>(UnityEngine.TextAsset ta) where T : PBTableRow<K>
        {
            if (ta != null)
            {
                using (MemoryStream ms = new MemoryStream(ta.bytes))
                {
                    return Serializer.Deserialize<PBDataSrc<K, T>>(ms);
                }
            }
            return null;
        }
    }
#endif
}
