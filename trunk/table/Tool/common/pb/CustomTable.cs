/*
 * 
 *  自定义的结构在这里申请
 * 
 */
using ProtoBuf;

namespace tab
{
    [ProtoContract]
    public class sKeyValue
    {
        [ProtoMember(1)]
        public string k;

        [ProtoMember(2)]
        public string v;
    } 
    [ProtoContract]
    public class sActorSlot
    {
        [ProtoMember(1)]
        public byte slot;

        [ProtoMember(2)]
        public uint char_idx;
    } 
    [ProtoContract]
    public class sBuffInfo
    {
        [ProtoMember(1)]
        public uint idx;

        [ProtoMember(2)]
        public int prob;
	
        [ProtoMember(3)]
        public int bout_limit;
    } 
    [ProtoContract]
    public class sAttackMove
    {
        [ProtoMember(1)]
        public float speed;

        [ProtoMember(2)]
        public string pre_pose;
	
        [ProtoMember(3)]
        public string end_pose;
    } 
}
