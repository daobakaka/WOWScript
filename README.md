一、基本安装
AddOns  文件时插件，粘贴复制到:***\World of Warcraft\_classic_\Interface  目录下
暂时删掉大脚等插件，避免冲突
Release  文件夹内，执行setup.exe文件进行安装，它可以检测.NET Framework运行库，如果你的电脑已经安装了，则会跳过，如果没有，则会优先安装运行库
安装完成后，执行 HSScript.exe 文件启动脚本


二、界面控制调试
重点：玩家使用插件第一次进入游戏时，请启用插件WOWInfo，另外两个插件WOWscript ，WOWTest请禁用，
界面设置中，务必设置图形为窗口然后手动点成最大化，不要设置成窗口最大化，按键设置取消角色专属，设置为默认，
（先恢复默认设置，重点设置：最大镜头距离设置为1 ，点击移动，与目标互动设置O键，总是智能调节镜头，跟随地形，智能调节，目标锁定，总是调整视角这几项必须设置），括号中的内容重点关注，第一次进入游戏时WOWInfo插件基本可以默认配置，
因为安装环境问题，必要时候你需要进行检查，最好初次使用时检查一遍，确定好了设置之后，就就可以永久禁用WOWInfo了


三、信息采集调试
开启WOWTest插件，禁用两外两个，弹出的控制台程序中，可以进入调试模式，调试数据采集范围，默认不需要，输入0即可，游戏时请确保使用窗口最大化，这样可以采取默认状态，不需要手动调试数据采集范围。
如果发现脚本出现不合理的控制，大概率是数据采集出了问题，这种情况下你需要进入调试模式，并且查看控制台是否打印输出了[LOG]++ 字样为抬头的信息，如果没有，就需要调试一下采集范围，直到输出[LOG]++格式为止，或者直接看file reder： 字样后面的数据
是否和插件显示的数据一致，目前大概率是不需要进行调试的，除非你不采用窗口模式，并且手动调整至最大化。



四、参数配置
此时就可以进入游戏了，开启WOWTest插件，禁用另外两个插件，控制台输入的生命值和魔法值以及选择战场等，都可以配置，生命和魔法参数用于设定玩家坐地吃喝的阈值，默认70%，还可以配置辅助技能及增益BUFF的执行时间间隔，1000毫秒=1秒，默认是30秒和10分钟
脚本有两种模式
1是只打怪 2是打怪加战场 
建议使用第二种，同一个战场建议几小时换一次，防止脚本监测，如果暴雪查封严格，建议使用第一种非战场模式，练级效率会降低一点。
在线时间超过12小时，建议休息5分钟再上线。



五、技能栏
1 主要输出技能（读条技能），可以自行配置技能宏，比如饰品 爆发技能等，
2.3 间隔BUFF技能，默认30秒执行一次
4. BUFF技能 ，10分钟执行一次，最高配置三个，采用宏
9.0 吃喝技能栏
如果不熟悉宏的设置，请参考文档 --参考宏设置，或者自行百度



目前脚本就实现这些功能，只测试过法师、萨满、术士、德鲁伊等远程职业，默认核心技能监测现在有只寒冰箭，闪电箭，暗影箭，愤怒，近战职业扩展技能ID 应该也可以执行（没有测试过）；


玩家有其他技能监测需求，可以使用WOWSCRIPT 插件进行监控（使用前禁用WOWTest和WOWInfo），y读取聊天框输出的技能ID，
并在WowTest.lua文件中，用txt文档打开，做出如下改变：
//------------------------------------------------------------------------
-- 存储特定的读条类攻击技能ID
local castSpells = {
    [27072] = true,  -- 寒冰箭的 spellID，例如
    -- 在此添加其他技能的 spellID
    --[xxxxxx]=true 示例
}
//----------------------------------------------------------------------
这个位置，添加你所需要的技能ID，[xxxxxx]=true

任何的插件变动，都需要手动/reload一下，才能刷新

游戏环境时，只需要开启WOWTest插件就行，另外两个配置和测试插件请禁用，游戏前/reload 一下

测试中出现宠物技能栏干扰聊天框的情况，如果出现这个问题/reload一下

启动脚本时，保证自己是在WOW窗口内，最好点击一下

有更多需求，邮件咨询a373823424@qq.com 

项目已在GTIHUB公开 ，作者：daobakaka ，欢迎订阅，欢迎交流探讨！

enjoy your fun！！！！
