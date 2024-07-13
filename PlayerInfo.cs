using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WowMove
{
    public class PlayerInfo
    {
        public bool ifDebug;
        public int playerHealth;
        public int playerMana;
        public int skillNum;
        //public float4 debugFrame
        public BattleAreas battleAreas;
        public float[] Screen4 = new float[4];
        public int assistSkillTime;
        public int buffSkillTime;
        public int protectHealth;
        private static PlayerInfo Instance; // 用于保存单例实例
        private static readonly object lockObject = new object(); // 用于线程安全
        
        // 将构造函数设为私有，以防止外部实例化
        private PlayerInfo()
        {
            ifDebug = false; playerHealth = 10000; playerMana = 10000; skillNum = 116; battleAreas = BattleAreas.ZGXG;
            assistSkillTime = 30000;
            buffSkillTime = 600000;
            protectHealth = 3000;
            Screen4 = new float[] { 0.02f, 0.2f, 0.168f, 0.368f };

        }

        // 提供一个公共的静态方法来访问单例实例
        public static PlayerInfo GetInstance()
        {
            // 确保线程安全
            if (Instance == null)
            {
                lock (lockObject)
                {
                    if (Instance == null)
                    {
                        Instance = new PlayerInfo();
                     
                        
                    }
                }
            }
            return Instance;
        }
    }
}
