using ProtoBuf;

namespace ExcelTool
{
    [ProtoContract]
    public class PBDataSrc<V>
    {
        [ProtoMember(1)]
        public V[] array;
    }
}
