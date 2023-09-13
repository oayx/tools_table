/*
 * 
 *  自定义字段
 * 
 */
 
using UnityEngine;
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
        
        public Vector2 Value { get { return new Vector2(x, y); } }
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
        
        public Vector3 Value { get { return new Vector3(x, y, z); } }
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
        
        public Vector4 Value { get { return new Vector4(x, y, z, w); } }
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
        
        public Color Value { get { return new Color(r, g, b, a); } }
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
        
        public Color32 Value { get { return new Color32(r, g, b, a); } }
    }
}