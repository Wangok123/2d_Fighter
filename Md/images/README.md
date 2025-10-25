# 文档图片资源说明

本目录用于存放项目文档中使用的图片资源。

## 目录结构

```
images/
├── architecture/     # 架构图
│   ├── client-architecture.png
│   ├── server-architecture.png
│   ├── overall-architecture.png
│   └── module-diagram.png
├── protocol/         # 协议相关图
│   ├── network-stack.png
│   ├── packet-format.png
│   ├── message-flow.png
│   └── kcp-flow.png
├── ui/              # UI截图
│   ├── main-menu.png
│   ├── battle-scene.png
│   ├── match-ui.png
│   └── settings.png
└── workflow/        # 工作流程图
    ├── login-flow.png
    ├── match-flow.png
    ├── battle-flow.png
    └── build-flow.png
```

## 图片命名规范

- 使用小写字母和连字符
- 使用描述性名称
- 格式：`模块-功能-说明.png`
- 例如：`client-architecture-overview.png`

## 支持的图片格式

- PNG（推荐，适合截图和示意图）
- JPG（适合照片）
- SVG（适合矢量图）
- GIF（适合动画演示）

## 图片尺寸建议

- 架构图：1920x1080 或更大
- UI截图：实际分辨率或缩放到1920x1080
- 流程图：根据内容调整，保持清晰
- 图标：建议256x256或512x512

## 如何添加图片

1. 将图片文件放入对应的子目录
2. 在Markdown文档中引用：
   ```markdown
   ![图片说明](images/architecture/client-architecture.png)
   ```

## 图片优化建议

- 使用PNG格式时，可使用TinyPNG等工具压缩
- 截图时注意去除敏感信息
- 保持图片清晰度和可读性
- 添加适当的标注和说明

## 常用图片工具

- **绘图工具**：
  - Draw.io (https://app.diagrams.net/)
  - Excalidraw (https://excalidraw.com/)
  - PlantUML (https://plantuml.com/)

- **截图工具**：
  - Windows: Snipping Tool / Snip & Sketch
  - Mac: Command+Shift+4
  - 跨平台: ShareX, Greenshot

- **图片编辑**：
  - GIMP (免费)
  - Photoshop
  - Paint.NET

## 参考现有图片

项目根目录的 `Md_Imgs/` 文件夹中已有一些图片资源，可以参考其风格和格式。
