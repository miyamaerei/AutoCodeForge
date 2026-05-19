<script setup lang="ts">
import { computed } from 'vue'
import VChart from 'vue-echarts'
import { use } from 'echarts/core'
import { CanvasRenderer } from 'echarts/renderers'
import { LineChart } from 'echarts/charts'
import { GridComponent, LegendComponent, TooltipComponent } from 'echarts/components'

use([CanvasRenderer, LineChart, GridComponent, TooltipComponent, LegendComponent])

const trendOption = computed(() => ({
  tooltip: {
    trigger: 'axis',
  },
  legend: {
    data: ['任务创建数', '成功数'],
    top: 0,
  },
  grid: {
    top: 36,
    left: 24,
    right: 16,
    bottom: 24,
  },
  xAxis: {
    type: 'category',
    boundaryGap: false,
    data: ['09:00', '11:00', '13:00', '15:00', '17:00'],
  },
  yAxis: {
    type: 'value',
  },
  series: [
    {
      name: '任务创建数',
      type: 'line',
      smooth: true,
      data: [4, 6, 5, 7, 4],
    },
    {
      name: '成功数',
      type: 'line',
      smooth: true,
      data: [3, 6, 4, 6, 4],
    },
  ],
}))
</script>

<template>
  <section class="desktop-page">
    <el-page-header content="Dashboard 工作台" />
    <el-row :gutter="12" class="metrics">
      <el-col :span="6"><el-card><el-statistic title="今日任务" :value="23" /></el-card></el-col>
      <el-col :span="6"><el-card><el-statistic title="成功率" :value="92.4" suffix="%" /></el-card></el-col>
      <el-col :span="6"><el-card><el-statistic title="执行中" :value="5" /></el-card></el-col>
      <el-col :span="6"><el-card><el-statistic title="告警提示" :value="2" /></el-card></el-col>
    </el-row>

    <el-row :gutter="16" class="desktop-grid">
      <el-col :span="17">
        <el-card shadow="hover">
          <template #header>
            <strong>最近任务趋势</strong>
          </template>
          <v-chart class="trend-chart" :option="trendOption" autoresize />
        </el-card>
      </el-col>
      <el-col :span="7">
        <el-card shadow="hover">
          <template #header>
            <strong>运行状态</strong>
          </template>
          <el-progress :percentage="92" status="success" />
          <el-divider />
          <el-alert title="AI任务中心整体健康" type="success" :closable="false" show-icon />
        </el-card>
      </el-col>
    </el-row>
  </section>
</template>

<style scoped>
.desktop-page {
  margin-top: 0;
  min-width: 1280px;
}

.metrics {
  margin-top: 0.5rem;
}

.desktop-grid {
  margin-top: 0.5rem;
}

.trend-chart {
  width: 100%;
  height: 280px;
}

@media (max-width: 1365px) {
  .desktop-page {
    overflow-x: auto;
    padding-bottom: 8px;
  }
}
</style>
