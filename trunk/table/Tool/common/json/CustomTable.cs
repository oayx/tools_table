/*
 * 
 *  自定义的结构在这里申请
 * 
 */

namespace tab
{
    public class sKeyValue
    {
        public string k;
        
        public string v;
    } 
    public class sActorSlot
    {
        public byte slot;
        
        public uint char_idx;
    } 
    public class sBuffInfo
    {
        public uint idx;
        
        public int prob;
	
        public int bout_limit;
    } 
    public class sAttackMove
    {
        public float speed;
        
        public string pre_pose;
	
        public string end_pose;
    } 
}
