# LimbusCompany-DG-LAB-mod
适配郊狼3.0的边狱巴士私服模组，使用OTC控制器

### 本项目使用MIT协议开源

## 在此感谢：
OTC控制器github仓库:https://github.com/open-toys-controller/open-DGLAB-controller   

边狱巴士模组模板:https://github.com/LEAGUE-OF-NINE/BasePlugin/   



### 注： 
### 1.此模组仅能在边狱巴士私人服务器使用 
### 2.此安装教程默认你已经成功配置私人服务器环境，本教程不会去另外花篇幅去讲述私服资源的获取与安装 

## 安装教程： 
### 模组部分   

1.使用集成开发环境创建C#类库(.dll)项目.NET Framework项目，然后把本仓库的代码复制进去   

2.引入项目依赖，即私服环境配置中由BepInEx生成的动态链接库，例如game\BepInEx\core，game\BepInEx\interop，game\BepInEx\unity-libs这些目录下的文件   

3.修改源代码的IP地址，换成你手机中OTC控制器显示的IP地址   

4.生成.dll文件，然后将其放置在game\BepInEx\plugins目录下   


### OTC控制器部分   

1.参考OTC控制器github仓库（https://github.com/open-toys-controller/open-DGLAB-controller）  
提供的apk文件，将其下载到手机   

2.手机和电脑连接同一个局域网   

3.点击“娱乐模式”，如果成功则会显示出IP地址   

