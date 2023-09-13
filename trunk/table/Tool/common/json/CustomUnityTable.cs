/*
 * 
 *  类unity的结构在这里声明 
 * 
 */

#if UNITY_2017
using UnityEngine;
#endif

namespace tab
{
    public class vector2
    {
        public float x;
        
        public float y;
    }
    
    public class vector3
    {
        public float x;
        
        public float y;
        
        public float z;

#if UNITY_2017
        public Vector3 Value { get { return new Vector3(x, y, z); } }
#endif
    }
    
    public class vector4
    {
        public float x;
        
        public float y;
        
        public float z;
        
        public float w;

#if UNITY_2017
        public Vector4 Value { get { return new Vector4(x, y, z, w); } }
#endif
    }
    
    public class color
    {
        public float r;
        
        public float g;
        
        public float b;
        
        public float a;

#if UNITY_2017
        public Color Value { get { return new Color(r, g, b, a); } }
#endif
    }
    
    public class color32
    {
        public byte r;
        
        public byte g;
        
        public byte b;
        
        public byte a;

#if UNITY_2017
        public Color32 Value { get { return new Color32(r, g, b, a); } }
#endif
    }
}
