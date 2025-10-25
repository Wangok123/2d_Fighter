# LATFramework

> 龙傲天框架

## 框架思想

框架吸收了多个看过的框架如：GameFramework、ET、Loxodon的设计思想，决定融合其所有形成一套自己的。

![gameframework示意图](../Md_Imgs/gameframework.png)

上图是GameFrame的架构示意图，展示了游戏开发的层次结构，主要分为几个部分：

- 游戏框架（Game Framework）：这是顶层，指的是构建游戏所需的整体结构和工具。这应该是一个脱离Unity也可以运行的一个库，如算法，网络，IO等。
- Unity游戏框架（Unity Game Framework）：这是一个特定于Unity引擎的框架，旨在优化在Unity中开发游戏的过程。

因此，同理自己也以[Core](../Md/Core/概述.md)和`UnityCore`来对应次关系。

## 双端工具库

毕竟是联网游戏，还是有必要弄一套双端通用的工具库

[工具库](./PublicLib/Timer/概述.md)