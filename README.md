# ET_Public

### 介绍

一个结合了ET的引擎,只是一个很简陋的Demo很多功能都没实现, 

抄了很多ET8 9 YiYiNumeric的代码

最终打算做一个能把我OtherPackages库的内容实现的引擎


### 未完成

数值系统-合并表

物理系统-Bson序列化

物理系统-角色控制器,铰链,物理动画布娃娃

物理系统-位置同步

渲染系统-材质

渲染系统-RenderPass功能(Prepare, ShadowMap, PBR, SoftShadow...)

渲染系统-dic

Entity-反序列化出来的Entity初始化

Ui系统-UGUI

动画系统

网络系统

编辑器-字典显示,PhysX显示

分析器

### 注意

注意bat的格式(换行符)是CRLF, 不然一堆乱码

Unity.App不会引用Model等 导致一些包会被裁剪, 你需要给Unity.App也加同样的包

C:\Users\UserName\.nuget\packages\physx.net\5.0.1-alpha1\lib\RuntimeFiles.targets 可以在每个Node里面加<Visible>false</Visible>来隐藏它引用的dll
