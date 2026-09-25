# ParametricCurveInterpolation
# 参数曲线建模与交互
实现课件中4种参数曲线：线性插值、Hermite、Catmull‑Rom、三次CubicBezier。

## 功能
1.鼠标拖拽红色控制点小球，曲线实时更新
2.Inspector下拉选择4种曲线类型
3.Catmull‑Rom增加首尾虚拟控制点，整条曲线平滑穿过全部控制点，C1连续
4.Hermite支持自定义两端切线；CubicBezier为逼近曲线，仅经过首尾控制点

## 使用操作
1.Unity打开场景 Scenes/CurveScene.unity
2.运行游戏，拖拽红色控制点小球
3.选中CurveManager物体，Inspector切换CurveType查看不同曲线效果

### 知识点
- Linear：分段线性插值
- Hermite：单段，端点+切线向量
- Catmull‑Rom：分段三次样条插值（插值曲线）
- CubicBezier三次贝塞尔：逼近曲线，中间点为控制手柄
