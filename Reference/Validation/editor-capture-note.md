# 编辑器截图工具诊断

强化 / 绝杀专项、开场回归和首战存档回归通过后，调用 Unity MCP `manage_camera` 的组合截图路径时，Unity 2022.3.62f3 Console 出现五条 `PlayerLoop internal function has been called recursively` 错误日志。

堆栈定位：

```text
MCPForUnity.Runtime.Helpers.ScreenshotUtility.CaptureCompositedAfterFrame
  Runtime/Helpers/ScreenshotUtility.cs:197
MCPForUnity.Runtime.Helpers.ScreenshotUtility.CaptureComposited
MCPForUnity.Editor.Tools.ManageScene.CaptureScreenshot
MCPForUnity.Editor.Tools.Cameras.ManageCamera.HandleCommand
```

插件版本锁定为 `aa5fc638d623f56178d50329d9ad8c541a57fe66`。该错误发生在编辑器截图路径；未修改插件依赖或把它归入游戏脚本错误。回归报告仍保留实际通过的路径和验证限制。后续截图可改用 Unity 正常帧结束后的异步 ScreenCapture，避免在工具命令内部同步推进 PlayerLoop。

最终强化预览为 `UnityProject/Captures/reinforcement-verified.png`。
