<script setup lang="ts">import { reactive, ref, onMounted } from 'vue';
import { ElMessage } from 'element-plus';
import { useSystemConfigSkills, type NewSkillForm } from '../composables/useSystemConfigSkills';
const { loading, error, saving, saveError, saveSuccess, lastSavedAt, searchKeyword, skills, filteredSkills, selectedSkill, enabledCount, activeCount, form, hasData, statusTagType, loadSkills, saveSkills, selectSkill, toggleSkillEnabled, updateSkillPriority, addSkill, deleteSkill, duplicateSkill, updateSkill, repositories, repositoriesLoading, loadRepositories, getSkillRepositories, } = useSystemConfigSkills();
const showAddModal = ref(false);
const showDeleteConfirm = ref(false);
const editingMode = ref(false);
const newSkillForm = reactive<NewSkillForm>({
 name: '',
 description: '',
 argumentHint: '',
 tags: '',
 whenToUse: '',
 outputTargets: '',
 repositoryIds: []
});
const handleSave = async () => {
 await saveSkills();
 if (saveSuccess.value) {
 ElMessage.success('技能配置已保存');
 }
 if (saveError.value) {
 ElMessage.error(saveError.value);
 }
};
const handleToggleSkill = (id: string, enabled: boolean) => {
 toggleSkillEnabled(id, enabled);
 ElMessage.success(enabled ? '技能已启用' : '技能已禁用');
};
const handleOpenAddModal = () => {
 newSkillForm.name = '';
 newSkillForm.description = '';
 newSkillForm.argumentHint = '';
 newSkillForm.tags = '';
 newSkillForm.whenToUse = '';
 newSkillForm.outputTargets = '';
 newSkillForm.repositoryIds = [];
 showAddModal.value = true;
};
const handleAddSkill = () => {
 if (!newSkillForm.name.trim()) {
 ElMessage.error('请输入技能名称');
 return;
 }
 if (!newSkillForm.description.trim()) {
 ElMessage.error('请输入技能描述');
 return;
 }
 addSkill(newSkillForm);
 showAddModal.value = false;
 ElMessage.success('技能已添加');
};
const handleDeleteSkill = () => {
 if (!selectedSkill.value)
 return;
 deleteSkill(selectedSkill.value.id);
 showDeleteConfirm.value = false;
 ElMessage.success('技能已删除');
};
const handleDuplicateSkill = () => {
 if (!selectedSkill.value)
 return;
 duplicateSkill(selectedSkill.value.id);
 ElMessage.success('技能已复制');
};
const canDeleteSkill = () => {
 return selectedSkill.value && selectedSkill.value.status !== 'active';
};
const handleToggleRepository = (repoId: string) => {
 if (!selectedSkill.value) return;
 const currentIds = selectedSkill.value.repositoryIds || [];
 const newIds = currentIds.includes(repoId)
 ? currentIds.filter(id => id !== repoId)
 : [...currentIds, repoId];
 updateSkill(selectedSkill.value.id, { repositoryIds: newIds });
};
onMounted(async () => {
 loadSkills();
 await loadRepositories();
});
</script>

<template>
  <section class="settings-page">
    <div class="settings-shell">
      <el-page-header content="Skills" />

      <el-skeleton v-if="loading" :rows="10" animated class="state-block" />
      <el-alert v-else-if="error" :title="error" type="error" show-icon :closable="false" class="state-block" />
      <el-empty v-else-if="!hasData" description="暂无可配置技能" class="state-block" />

      <div v-else class="skill-layout">
        <el-card shadow="hover" class="catalog-card">
          <template #header>
            <div class="card-header">
              <strong>技能目录</strong>
              <div class="card-actions">
                <span class="card-subtitle">当前 {{ skills.length }} 项</span>
                <el-button size="small" type="primary" icon="Plus" @click="handleOpenAddModal">
                  添加技能
                </el-button>
              </div>
            </div>
          </template>

          <el-input v-model="searchKeyword" placeholder="搜索技能名称、标签" clearable />

          <div class="catalog-list">
            <button
              v-for="item in filteredSkills"
              :key="item.id"
              class="catalog-item"
              :class="{ active: selectedSkill?.id === item.id }"
              @click="selectSkill(item.id)"
            >
              <div class="catalog-item-title">
                <span>{{ item.name }}</span>
                <el-tag :type="statusTagType(item.status)" size="small">{{ item.status }}</el-tag>
              </div>
              <p class="catalog-item-desc">{{ item.description }}</p>
              <div class="catalog-item-meta">
                <el-tag v-for="tag in item.tags" :key="tag" size="small" effect="plain">{{ tag }}</el-tag>
              </div>
            </button>
          </div>
        </el-card>

        <el-card shadow="hover" class="detail-card">
          <template #header>
            <div class="card-header">
              <strong>{{ selectedSkill?.name || 'Skill' }}</strong>
              <span class="card-subtitle">{{ selectedSkill?.description || '-' }}</span>
            </div>
          </template>

          <template v-if="selectedSkill">
            <el-form label-width="160px" class="settings-form">
              <el-row :gutter="16">
                <el-col :span="12">
                  <el-form-item label="技能状态">
                    <el-switch
                      :model-value="selectedSkill.enabled"
                      @change="(val: boolean) => handleToggleSkill(selectedSkill!.id, val)"
                      active-text="启用"
                      inactive-text="禁用"
                    />
                  </el-form-item>
                </el-col>
                <el-col :span="12">
                  <el-form-item label="优先级">
                    <el-input-number
                      :model-value="form[selectedSkill.id]?.priority || 0"
                      :min="0"
                      :max="100"
                      @change="(val: number) => updateSkillPriority(selectedSkill!.id, val)"
                    />
                  </el-form-item>
                </el-col>
              </el-row>

              <el-divider content-position="left">使用场景</el-divider>
              <el-alert type="info" :closable="false" class="info-alert">
                <ul class="info-list">
                  <li v-for="(useCase, index) in selectedSkill.whenToUse" :key="index">
                    {{ useCase }}
                  </li>
                </ul>
              </el-alert>

              <el-divider content-position="left">参数提示</el-divider>
              <el-input
                :model-value="selectedSkill.argumentHint"
                type="textarea"
                :rows="2"
                readonly
                class="hint-textarea"
              />

              <el-divider content-position="left">输出目标</el-divider>
              <div class="output-targets">
                <el-tag v-for="(target, index) in selectedSkill.outputTargets" :key="index" class="target-tag">
                  {{ target }}
                </el-tag>
              </div>

              <el-divider content-position="left">关联仓库</el-divider>
              <el-alert type="warning" :closable="false" class="info-alert">
                <p class="helper-text">该技能适用的仓库（可多选）：</p>
                <div class="repo-selector">
                  <el-skeleton v-if="repositoriesLoading" :rows="2" animated />
                  <el-checkbox-group
                    v-else-if="repositories.length > 0"
                    v-model="selectedSkill.repositoryIds"
                    class="repo-checkbox-group"
                  >
                    <el-checkbox
                      v-for="repo in repositories"
                      :key="repo.id"
                      :label="repo.id"
                      class="repo-checkbox"
                    >
                      <div class="repo-item">
                        <strong>{{ repo.name }}</strong>
                        <span class="repo-url">{{ repo.url }}</span>
                      </div>
                    </el-checkbox>
                  </el-checkbox-group>
                  <el-empty v-else description="暂无仓库可关联" :image-size="60" />
                </div>
                <p class="helper-text" style="margin-top: 8px; font-size: 12px;">
                  技能只会在关联的仓库中运行，留空表示适用于所有仓库
                </p>
              </el-alert>

              <div class="skill-actions">
                <el-button size="small" icon="Copy" @click="handleDuplicateSkill">复制技能</el-button>
                <el-button
                  v-if="canDeleteSkill()"
                  size="small"
                  type="danger"
                  icon="Delete"
                  @click="showDeleteConfirm = true"
                >删除技能</el-button>
              </div>
            </el-form>
          </template>
        </el-card>

        <el-card shadow="never" class="status-card">
          <template #header>
            <div class="card-header">
              <strong>状态面板</strong>
              <span class="card-subtitle">统一统计与操作</span>
            </div>
          </template>

          <el-descriptions :column="1" border size="small" class="status-meta">
            <el-descriptions-item label="Total Skills">{{ skills.length }}</el-descriptions-item>
            <el-descriptions-item label="Enabled">{{ enabledCount }}</el-descriptions-item>
            <el-descriptions-item label="Active">{{ activeCount }}</el-descriptions-item>
            <el-descriptions-item label="Beta">{{ skills.filter(s => s.status === 'beta').length }}</el-descriptions-item>
            <el-descriptions-item label="Selected">{{ selectedSkill?.name || '-' }}</el-descriptions-item>
            <el-descriptions-item label="Last Saved At">{{ lastSavedAt || '-' }}</el-descriptions-item>
          </el-descriptions>

          <el-alert
            v-if="saveSuccess"
            title="配置保存成功"
            type="success"
            show-icon
            :closable="false"
            class="save-state"
          />
          <el-alert
            v-if="saveError"
            :title="saveError"
            type="error"
            show-icon
            :closable="false"
            class="save-state"
          />

          <div class="actions sticky-actions">
            <el-button type="success" :loading="saving" @click="handleSave">保存全部配置</el-button>
          </div>
        </el-card>
      </div>
    </div>

    <el-dialog title="添加新技能" v-model="showAddModal" width="600px">
      <el-form label-width="120px" class="add-skill-form">
        <el-form-item label="技能名称" required>
          <el-input v-model="newSkillForm.name" placeholder="输入技能名称" />
        </el-form-item>
        <el-form-item label="技能描述" required>
          <el-input v-model="newSkillForm.description" type="textarea" :rows="2" placeholder="描述技能的用途和功能" />
        </el-form-item>
        <el-form-item label="参数提示">
          <el-input v-model="newSkillForm.argumentHint" type="textarea" :rows="2" placeholder="用户使用时的参数提示" />
        </el-form-item>
        <el-form-item label="标签">
          <el-input v-model="newSkillForm.tags" placeholder="多个标签用逗号分隔" />
        </el-form-item>
        <el-form-item label="使用场景">
          <el-input v-model="newSkillForm.whenToUse" type="textarea" :rows="3" placeholder="每行一个场景" />
        </el-form-item>
        <el-form-item label="输出目标">
          <el-input v-model="newSkillForm.outputTargets" type="textarea" :rows="3" placeholder="每行一个目标路径" />
        </el-form-item>
        <el-form-item label="关联仓库">
          <el-select
            v-model="newSkillForm.repositoryIds"
            multiple
            :loading="repositoriesLoading"
            placeholder="选择仓库"
            style="width: 100%"
          >
            <el-option
              v-for="repo in repositories"
              :key="repo.id"
              :label="repo.name"
              :value="repo.id"
            >
              <span>{{ repo.name }}</span>
              <span style="color: #999; font-size: 12px; margin-left: 10px">{{ repo.url }}</span>
            </el-option>
          </el-select>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showAddModal = false">取消</el-button>
        <el-button type="primary" @click="handleAddSkill">添加</el-button>
      </template>
    </el-dialog>

    <el-dialog title="确认删除" v-model="showDeleteConfirm">
      <p>确定要删除技能 <strong>{{ selectedSkill?.name }}</strong> 吗？此操作无法撤销。</p>
      <template #footer>
        <el-button @click="showDeleteConfirm = false">取消</el-button>
        <el-button type="danger" @click="handleDeleteSkill">删除</el-button>
      </template>
    </el-dialog>
  </section>
</template>

<style scoped>
.settings-page {
  min-width: 1280px;
  padding: 20px 16px 40px;
}

.settings-shell {
  width: 100%;
  max-width: 1580px;
  margin: 0 auto;
}

.state-block {
  margin-top: 12px;
}

.skill-layout {
  margin-top: 12px;
  display: grid;
  grid-template-columns: 320px minmax(720px, 1fr) 340px;
  gap: 16px;
  align-items: start;
}

.catalog-card,
.detail-card,
.status-card {
  min-height: 620px;
}

.card-header {
  display: flex;
  align-items: baseline;
  justify-content: space-between;
  gap: 12px;
}

.card-actions {
  display: flex;
  align-items: center;
  gap: 8px;
}

.card-subtitle {
  color: #64748b;
  font-size: 13px;
}

.catalog-list {
  margin-top: 12px;
  display: grid;
  gap: 10px;
  max-height: 700px;
  overflow: auto;
}

.catalog-item {
  text-align: left;
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  background: #fff;
  padding: 10px;
  cursor: pointer;
}

.catalog-item.active {
  border-color: #6366f1;
  box-shadow: 0 0 0 2px rgba(99, 102, 241, 0.12);
}

.catalog-item-title {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 8px;
  font-weight: 600;
}

.catalog-item-desc {
  margin: 8px 0;
  color: #64748b;
  line-height: 1.4;
  font-size: 13px;
}

.catalog-item-meta {
  display: flex;
  gap: 6px;
  flex-wrap: wrap;
}

.settings-form {
  max-width: 100%;
}

.info-alert {
  margin-bottom: 16px;
}

.info-list {
  margin: 0;
  padding-left: 20px;
}

.info-list li {
  margin: 4px 0;
}

.hint-textarea {
  margin-bottom: 16px;
}

.output-targets {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-bottom: 16px;
}

.target-tag {
  max-width: 100%;
  overflow: hidden;
  text-overflow: ellipsis;
}

.repo-selector {
  margin-top: 8px;
}

.repo-checkbox-group {
  display: grid;
  gap: 8px;
}

.repo-checkbox {
  display: block;
  height: auto;
  padding: 8px 12px;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  margin-right: 0;
}

.repo-checkbox:hover {
  background-color: #f8fafc;
}

.repo-item {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.repo-url {
  font-size: 12px;
  color: #64748b;
}

.helper-text {
  margin: 0;
}

.skill-actions {
  display: flex;
  gap: 8px;
  margin-top: 16px;
  padding-top: 16px;
  border-top: 1px solid #e2e8f0;
}

.add-skill-form {
  max-width: 100%;
}

.actions {
  margin-top: 8px;
}

.status-card {
  position: sticky;
  top: 10px;
}

.status-meta {
  margin-top: 8px;
}

.save-state {
  margin-top: 10px;
}

.sticky-actions {
  margin-top: 14px;
}

@media (max-width: 1365px) {
  .settings-page {
    overflow-x: auto;
    padding-bottom: 10px;
  }

  .skill-layout {
    min-width: 1320px;
  }
}
</style>
