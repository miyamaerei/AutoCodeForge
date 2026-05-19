<script setup lang="ts">
import { ref, watch, onMounted, onUnmounted } from 'vue'
import { useOnboarding, type OnboardingStep } from '../composables/useOnboarding'

const emit = defineEmits<{
  (e: 'complete'): void
  (e: 'skip'): void
}>()

const onboarding = useOnboarding()
const targetElement = ref<HTMLElement | null>(null)
const guidePosition = ref({ top: 0, left: 0 })

function getCurrentStep(): OnboardingStep {
  return onboarding.currentStep.value
}

function updatePosition(): void {
  const step = getCurrentStep()

  if (step.targetElement === 'body') {
    guidePosition.value = {
      top: window.innerHeight / 2 - 100,
      left: window.innerWidth / 2 - 200,
    }
    return
  }

  const element = document.querySelector(step.targetElement)
  if (element) {
    const rect = element.getBoundingClientRect()
    targetElement.value = element as HTMLElement

    const placement = step.placement
    const guideWidth = 400
    const guideHeight = 140

    switch (placement) {
      case 'top':
        guidePosition.value = {
          top: rect.top - guideHeight - 16,
          left: Math.min(rect.left + rect.width / 2 - guideWidth / 2, window.innerWidth - guideWidth - 20),
        }
        break
      case 'bottom':
        guidePosition.value = {
          top: rect.bottom + 16,
          left: Math.min(rect.left + rect.width / 2 - guideWidth / 2, window.innerWidth - guideWidth - 20),
        }
        break
      case 'left':
        guidePosition.value = {
          top: Math.min(rect.top + rect.height / 2 - guideHeight / 2, window.innerHeight - guideHeight - 20),
          left: rect.left - guideWidth - 16,
        }
        break
      case 'right':
        guidePosition.value = {
          top: Math.min(rect.top + rect.height / 2 - guideHeight / 2, window.innerHeight - guideHeight - 20),
          left: rect.right + 16,
        }
        break
    }
  } else {
    guidePosition.value = {
      top: window.innerHeight / 2 - 100,
      left: window.innerWidth / 2 - 200,
    }
  }
}

function handleNext(): void {
  if (onboarding.isLastStep.value) {
    onboarding.complete()
    emit('complete')
  } else {
    onboarding.next()
  }
}

function handlePrev(): void {
  onboarding.prev()
}

function handleSkip(): void {
  onboarding.skip()
  emit('skip')
}

function handleOverlayClick(e: MouseEvent): void {
  if ((e.target as HTMLElement).classList.contains('onboarding-overlay')) {
    const step = getCurrentStep()
    if (step.skipable) {
      handleSkip()
    }
  }
}

watch(() => onboarding.currentStepIndex.value, () => {
  updatePosition()
})

onMounted(() => {
  updatePosition()
  window.addEventListener('resize', updatePosition)
})

onUnmounted(() => {
  window.removeEventListener('resize', updatePosition)
})
</script>

<template>
  <Teleport to="body">
    <Transition name="fade">
      <div
        v-if="onboarding.isActive.value"
        class="onboarding-overlay"
        @click="handleOverlayClick"
      >
        <div
          v-if="targetElement && getCurrentStep().targetElement !== 'body'"
          class="onboarding-highlight"
          :style="{
            top: targetElement.getBoundingClientRect().top + 'px',
            left: targetElement.getBoundingClientRect().left + 'px',
            width: targetElement.getBoundingClientRect().width + 'px',
            height: targetElement.getBoundingClientRect().height + 'px',
          }"
        />

        <div
          class="onboarding-guide"
          :class="{ 'onboarding-guide-center': getCurrentStep().targetElement === 'body' }"
          :style="{
            top: guidePosition.top + 'px',
            left: guidePosition.left + 'px',
          }"
        >
          <div class="onboarding-guide-header">
            <span class="onboarding-guide-step">{{ onboarding.currentStepIndex.value + 1 }} / {{ onboarding.totalSteps.value }}</span>
            <button
              v-if="getCurrentStep().skipable && !onboarding.isLastStep.value"
              class="onboarding-guide-skip"
              @click.stop="handleSkip"
            >
              跳过引导
            </button>
          </div>

          <div class="onboarding-guide-body">
            <h3 class="onboarding-guide-title">{{ getCurrentStep().title }}</h3>
            <p class="onboarding-guide-description">{{ getCurrentStep().description }}</p>
          </div>

          <div class="onboarding-guide-progress">
            <div
              class="onboarding-guide-progress-bar"
              :style="{ width: onboarding.progress.value + '%' }"
            />
          </div>

          <div class="onboarding-guide-actions">
            <button
              v-if="!onboarding.isFirstStep.value"
              class="onboarding-guide-btn onboarding-guide-btn-secondary"
              @click.stop="handlePrev"
            >
              上一步
            </button>
            <button
              class="onboarding-guide-btn onboarding-guide-btn-primary"
              @click.stop="handleNext"
            >
              {{ onboarding.isLastStep.value ? '开始体验' : '下一步' }}
            </button>
          </div>

          <div
            v-if="getCurrentStep().targetElement !== 'body'"
            class="onboarding-guide-arrow"
            :class="'onboarding-guide-arrow-' + getCurrentStep().placement"
          />
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped>
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

.onboarding-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.6);
  z-index: 9999;
  cursor: pointer;
}

.onboarding-highlight {
  position: fixed;
  background: rgba(59, 130, 246, 0.2);
  border: 2px solid rgba(59, 130, 246, 0.6);
  border-radius: 8px;
  z-index: 10000;
  box-shadow: 0 0 20px rgba(59, 130, 246, 0.4);
}

.onboarding-guide {
  position: fixed;
  width: 400px;
  background: white;
  border-radius: 12px;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.2);
  z-index: 10001;
  cursor: default;
  overflow: hidden;
}

.onboarding-guide-center {
  top: 50% !important;
  left: 50% !important;
  transform: translate(-50%, -50%);
}

.onboarding-guide-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 20px;
  background: linear-gradient(135deg, #3b82f6 0%, #1d4ed8 100%);
  color: white;
}

.onboarding-guide-step {
  font-size: 14px;
  font-weight: 600;
}

.onboarding-guide-skip {
  background: rgba(255, 255, 255, 0.2);
  border: none;
  color: white;
  padding: 6px 12px;
  border-radius: 6px;
  font-size: 14px;
  cursor: pointer;
  transition: background 0.2s;
}

.onboarding-guide-skip:hover {
  background: rgba(255, 255, 255, 0.3);
}

.onboarding-guide-body {
  padding: 20px;
}

.onboarding-guide-title {
  font-size: 18px;
  font-weight: 600;
  color: #1f2937;
  margin: 0 0 8px 0;
}

.onboarding-guide-description {
  font-size: 14px;
  color: #6b7280;
  margin: 0;
  line-height: 1.6;
}

.onboarding-guide-progress {
  height: 4px;
  background: #e5e7eb;
}

.onboarding-guide-progress-bar {
  height: 100%;
  background: linear-gradient(90deg, #3b82f6 0%, #1d4ed8 100%);
  transition: width 0.3s ease;
}

.onboarding-guide-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  padding: 16px 20px;
  background: #f9fafb;
}

.onboarding-guide-btn {
  padding: 8px 20px;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
  border: none;
}

.onboarding-guide-btn-primary {
  background: linear-gradient(135deg, #3b82f6 0%, #1d4ed8 100%);
  color: white;
}

.onboarding-guide-btn-primary:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(59, 130, 246, 0.4);
}

.onboarding-guide-btn-secondary {
  background: #e5e7eb;
  color: #4b5563;
}

.onboarding-guide-btn-secondary:hover {
  background: #d1d5db;
}

.onboarding-guide-arrow {
  position: absolute;
  width: 12px;
  height: 12px;
  background: white;
  transform: rotate(45deg);
}

.onboarding-guide-arrow-top {
  bottom: -6px;
  left: 50%;
  transform: translateX(-50%) rotate(45deg);
  box-shadow: 4px 4px 8px rgba(0, 0, 0, 0.1);
}

.onboarding-guide-arrow-bottom {
  top: -6px;
  left: 50%;
  transform: translateX(-50%) rotate(45deg);
  box-shadow: -4px -4px 8px rgba(0, 0, 0, 0.1);
}

.onboarding-guide-arrow-left {
  right: -6px;
  top: 50%;
  transform: translateY(-50%) rotate(45deg);
  box-shadow: 4px -4px 8px rgba(0, 0, 0, 0.1);
}

.onboarding-guide-arrow-right {
  left: -6px;
  top: 50%;
  transform: translateY(-50%) rotate(45deg);
  box-shadow: -4px 4px 8px rgba(0, 0, 0, 0.1);
}
</style>
