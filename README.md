# DirectMuxer_CS
 DirectMuxer C#

因为我不满意原本DirectMuxer的处理，所以自力更生了。因为完成度还不够高，部分功能还没加上去，界面也算简陋吧。
主要想代替DM最常用的几种合成方式，以及增加单文件夹下正则分组合成功能，还有就是可以调整输出格式，DM的png输出对现在动不动几百几千的大尺寸CG很蛋疼，因为还得转BMP再压缩。

因为懒，还没写说明，不过基本上与DM的使用是一样的，这里说一下正则分组合成好了。
File里面把Regex Mode勾上，然后左下角D按钮选图片文件夹。
然后左下角R按钮前的输入框输入分组用正则表达式，其余Group下面输入图片选择用正则表达式。
举个栗子：
a001.png
a002.png
a00a.png
b001.png
b00a.png
b00b.png

其中a和b是两个不同角色，001和002是衣服不同，00a和00b是表情。
那么分组用正则表达式就是：([a-z]).+png
第一个Group就是:00\d.png
第二个Group就是:00[a-z].png

然后输出结果是：
a001.png + a00a.png
a002.png + a00a.png
b001.png + b00a.png
b001.png + b00b.png
