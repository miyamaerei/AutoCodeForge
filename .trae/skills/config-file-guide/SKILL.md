---
name: "config-file-guide"
description: "配置文件添加指南。用于指导开发者了解添加新配置文件时，前端和后端需要修改的内容，以及初始化配置的步骤。"
---

# 配置文件添加指南

本Skill用于指导开发者在AutoCodeForge项目中添加新配置文件时，前端和后端需要进行的修改步骤。

## 一、后端修改步骤

### 1. 配置类型枚举
**文件路径**: `server/src/AutoCodeForge.Core/Enums/ConfigType.cs`

在`ConfigType`枚举中添加新的配置类型：
```csharp
public enum ConfigType
{
    Global = 0,
    User = 1,
    Preferences = 2,
    // ... 现有类型
    YourNewConfigType = 15, // 添加新类型
}
```

### 2. 配置服务扩展
**文件路径**: `server/src/AutoCodeForge.Application/Services/ConfigService.cs`

添加特定配置的获取/保存方法：
```csharp
public async Task<YourConfigDto?> GetYourConfigAsync(CancellationToken cancellationToken = default)
{
    var config = await GetByTypeAndKeyAsync(ConfigType.YourNewConfigType, "your-config-key", cancellationToken);
    return config != null ? JsonHelper.Deserialize<YourConfigDto>(config.ConfigValue) : null;
}

public async Task<YourConfigDto> UpsertYourConfigAsync(YourConfigDto config, CancellationToken cancellationToken = default)
{
    await UpsertAsync(ConfigType.YourNewConfigType, "your-config-key", 
                    JsonHelper.Serialize(config), 
                    isEncrypted: false,
                    description: "你的配置说明",
                    cancellationToken: cancellationToken);
    return config;
}
```

### 3. API端点配置
**文件路径**: `server/src/AutoCodeForge.Api/Endpoints/ConfigEndpoints.cs`

添加新的配置端点：
```csharp
// 获取配置
app.MapGet("/api/v1/config/your-config", async (ConfigService service) =>
{
    var config = await service.GetYourConfigAsync();
    return Results.Ok(config);
});

// 更新配置
app.MapPut("/api/v1/config/your-config", async (YourConfigDto request, ConfigService service) =>
{
    await service.UpsertYourConfigAsync(request);
    return Results.Ok();
});
```

### 4. 数据库迁移
运行EF Core迁移命令：
```bash
dotnet ef migrations add AddYourConfigType
dotnet ef database update
```

---

## 二、前端修改步骤

### 1. 类型定义
**文件路径**: `client/src/modules/system-config/api/config.types.ts`

添加新的配置类型和DTO：
```typescript
export interface YourConfigDto {
    setting1: string;
    setting2: boolean;
    setting3: number;
}

export type ConfigType = 
    | 'Global' 
    | 'User' 
    | 'YourNewConfigType';
```

### 2. API层
**文件路径**: `client/src/modules/system-config/api/config.api.ts`

添加API调用方法：
```typescript
export async function getYourConfig(): Promise<YourConfigDto> {
    const response = await axios.get('/api/v1/config/your-config');
    return response.data;
}

export async function saveYourConfig(config: YourConfigDto): Promise<void> {
    await axios.put('/api/v1/config/your-config', config);
}
```

### 3. Store层
**文件路径**: `client/src/modules/system-config/store/useSystemConfigStore.ts`

添加配置状态管理：
```typescript
const state = defineStore({
    id: 'system-config',
    state: () => ({
        yourConfig: null as YourConfigDto | null,
    }),
    
    actions: {
        async loadYourConfig() {
            this.yourConfig = await getYourConfig();
        },
        
        async saveYourConfig(config: YourConfigDto) {
            await saveYourConfig(config);
            this.yourConfig = config;
        }
    }
});
```

### 4. 配置页面组件
**文件路径**: `client/src/modules/system-config/views/YourConfigView.vue`

创建配置页面：
```vue
<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useSystemConfigStore } from '../store/useSystemConfigStore';
import { ElMessage } from 'element-plus';

const store = useSystemConfigStore();
const form = ref({
    setting1: '',
    setting2: false,
    setting3: 0,
});

onMounted(async () => {
    const config = await store.getYourConfig();
    if (config) {
        Object.assign(form.value, config);
    }
});

const handleSave = async () => {
    await store.saveYourConfig(form.value);
    ElMessage.success('配置保存成功');
};
</script>

<template>
    <el-card>
        <template #header>
            <span>你的配置</span>
        </template>
        
        <el-form :model="form" label-width="160px">
            <el-form-item label="设置1">
                <el-input v-model="form.setting1" />
            </el-form-item>
            <el-form-item label="设置2">
                <el-switch v-model="form.setting2" />
            </el-form-item>
            <el-form-item label="设置3">
                <el-input-number v-model="form.setting3" />
            </el-form-item>
            
            <el-button type="success" @click="handleSave">保存配置</el-button>
        </el-form>
    </el-card>
</template>
```

### 5. 路由配置
**文件路径**: `client/src/modules/system-config/routes.ts`

添加路由：
```typescript
const routes: RouteRecordRaw[] = [
    {
        path: '/settings/your-config',
        name: 'YourConfig',
        component: () => import('../views/YourConfigView.vue'),
        meta: { requiresAuth: true },
    },
];
```

### 6. 侧边栏菜单
在菜单配置中添加新项：
```typescript
{
    id: 'your-config',
    name: '你的配置',
    icon: 'Settings',
    path: '/settings/your-config',
},
```

---

## 三、初始化配置

### 默认配置初始化
**文件路径**: `server/src/AutoCodeForge.Application/Services/ConfigInitializationService.cs`

在初始化服务中添加默认配置：
```csharp
public async Task InitializeDefaultsAsync(CancellationToken cancellationToken = default)
{
    var existing = await _configRepository.GetByTypeAndKeyAsync(
        ConfigType.YourNewConfigType, 
        "your-config-key", 
        cancellationToken);
    
    if (existing == null)
    {
        var defaultConfig = new YourConfigDto
        {
            setting1 = "default value",
            setting2 = true,
            setting3 = 100,
        };
        
        await _configService.UpsertAsync(
            ConfigType.YourNewConfigType,
            "your-config-key",
            JsonHelper.Serialize(defaultConfig),
            isEncrypted: false,
            description: "你的配置说明",
            cancellationToken: cancellationToken);
    }
}
```

### 启动时调用
在`Program.cs`中：
```csharp
var initializationService = app.Services.GetRequiredService<ConfigInitializationService>();
await initializationService.InitializeDefaultsAsync();
```

---

## 四、配置应用方式

### 方式一：API调用（运行时加载）
```csharp
public class YourService
{
    private readonly ConfigService _configService;
    
    public YourService(ConfigService configService)
    {
        _configService = configService;
    }
    
    public async Task DoSomethingAsync()
    {
        var config = await _configService.GetYourConfigAsync();
        if (config?.setting2 == true)
        {
            // 执行操作
        }
    }
}
```

### 方式二：配置绑定（启动时加载）
```csharp
// appsettings.json
{
    "YourConfig": {
        "setting1": "value",
        "setting2": true
    }
}

// Program.cs
builder.Services.Configure<YourConfigDto>(builder.Configuration.GetSection("YourConfig"));

// 使用
public class YourService
{
    private readonly YourConfigDto _config;
    
    public YourService(IOptions<YourConfigDto> config)
    {
        _config = config.Value;
    }
}
```

---

## 五、检查清单

| 序号 | 检查项 | 状态 |
|------|--------|------|
| 1 | 后端：添加 ConfigType 枚举值 | ▢ |
| 2 | 后端：添加 ConfigService 方法 | ▢ |
| 3 | 后端：添加 API 端点 | ▢ |
| 4 | 后端：添加初始化默认配置 | ▢ |
| 5 | 后端：运行数据库迁移 | ▢ |
| 6 | 前端：添加类型定义 | ▢ |
| 7 | 前端：添加 API 方法 | ▢ |
| 8 | 前端：添加 Store 状态和方法 | ▢ |
| 9 | 前端：创建配置页面组件 | ▢ |
| 10 | 前端：配置路由和菜单 | ▢ |
| 11 | 测试：验证配置读写功能 | ▢ |

---

## 六、参考示例

### LLM模型配置示例
**场景**: 添加GitHub Copilot CLI配置

**修改内容**:
1. `LLMProvider`枚举添加`GitHubCopilot`值
2. `LLMModelConfigEntity`添加`CliExecutable`、`Organization`、`AuthMode`、`PatEnvVar`字段
3. `LlmGateway`实现`CallGitHubCopilotCliAsync`方法调用CLI命令
4. 前端`SystemConfigModelsView.vue`添加CLI配置面板

---

## 七、注意事项

1. **配置加密**: 敏感配置（如API Key）应设置`isEncrypted: true`
2. **权限控制**: 全局配置仅管理员可操作，用户配置需关联NTID
3. **历史记录**: 系统自动记录配置变更历史，支持追溯和回滚
4. **缓存策略**: 高频访问的配置建议添加缓存机制
5. **验证逻辑**: 添加配置验证规则，确保数据完整性

---

## 八、触发场景

本Skill适用于以下场景：
- 询问"如何添加新配置"
- 询问"配置文件修改步骤"
- 询问"初始化配置需要做什么"
- 需要了解配置系统架构

---

## 关键文件路径汇总

### 后端核心文件
| 文件 | 说明 |
|------|------|
| `Core/Enums/ConfigType.cs` | 配置类型枚举 |
| `Application/Services/ConfigService.cs` | 配置服务 |
| `Api/Endpoints/ConfigEndpoints.cs` | API端点 |
| `Application/Services/ConfigInitializationService.cs` | 初始化服务 |

### 前端核心文件
| 文件 | 说明 |
|------|------|
| `api/config.types.ts` | 类型定义 |
| `api/config.api.ts` | API层 |
| `store/useSystemConfigStore.ts` | 状态管理 |
| `views/*.vue` | 配置页面组件 |
| `routes.ts` | 路由配置 |