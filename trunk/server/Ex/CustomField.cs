/*
 * 
 *  自定义字段
 * 
 */

using ProtoBuf;

namespace tab
{
    [ProtoContract]
    public class vector2
    {
        [ProtoMember(1)]
        public float x;

        [ProtoMember(2)]
        public float y;
    }

    [ProtoContract]
    public class vector3
    {
        [ProtoMember(1)]
        public float x;

        [ProtoMember(2)]
        public float y;

        [ProtoMember(3)]
        public float z;
    }

    [ProtoContract]
    public class vector4
    {
        [ProtoMember(1)]
        public float x;

        [ProtoMember(2)]
        public float y;

        [ProtoMember(3)]
        public float z;

        [ProtoMember(4)]
        public float w;
    }

    [ProtoContract]
    public class color
    {
        [ProtoMember(1)]
        public float r;

        [ProtoMember(2)]
        public float g;

        [ProtoMember(3)]
        public float b;

        [ProtoMember(4)]
        public float a;
    }

    [ProtoContract]
    public class color32
    {
        [ProtoMember(1)]
        public byte r;

        [ProtoMember(2)]
        public byte g;

        [ProtoMember(3)]
        public byte b;

        [ProtoMember(4)]
        public byte a;
    }
}