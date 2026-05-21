/**
 * useOnboarding Composable 单元测试
 */
import { describe, it, expect, beforeEach, vi } from 'vitest'
import { useOnboarding } from '../useOnboarding'

// Mock localStorage
const localStorageMock = {
  getItem: vi.fn(),
  setItem: vi.fn(),
  removeItem: vi.fn(),
}
vi.stubGlobal('localStorage', localStorageMock)

describe('useOnboarding', () => {
  // 在每个测试前重置状态，因为 useOnboarding 使用模块级别的 ref
  beforeEach(() => {
    const { reset } = useOnboarding()
    reset()
    localStorageMock.getItem.mockReturnValue(null)
  })

  describe('初始状态', () => {
    it('should have correct initial values', () => {
      const {
        isActive,
        currentStepIndex,
        totalSteps,
        hasCompleted,
        progress,
        isFirstStep,
        isLastStep,
      } = useOnboarding()

      expect(isActive.value).toBe(false)
      expect(currentStepIndex.value).toBe(0)
      expect(totalSteps.value).toBe(9)
      expect(hasCompleted.value).toBe(false)
      expect(progress.value).toBeCloseTo(11.11, 1)
      expect(isFirstStep.value).toBe(true)
      expect(isLastStep.value).toBe(false)
    })
  })

  describe('导航功能', () => {
    it('should start onboarding and set isActive to true', () => {
      const { start, isActive, currentStepIndex } = useOnboarding()

      start()

      expect(isActive.value).toBe(true)
      expect(currentStepIndex.value).toBe(0)
    })

    it('should go to next step', () => {
      const { start, next, currentStepIndex } = useOnboarding()

      start()
      next()

      expect(currentStepIndex.value).toBe(1)
    })

    it('should go to previous step', () => {
      const { start, next, prev, currentStepIndex } = useOnboarding()

      start()
      next()
      next()
      prev()

      expect(currentStepIndex.value).toBe(1)
    })

    it('should not go below step 0', () => {
      const { start, prev, currentStepIndex } = useOnboarding()

      start()
      prev()

      expect(currentStepIndex.value).toBe(0)
    })

    it('should complete when next is called on last step', () => {
      const { start, next, isActive, currentStepIndex, totalSteps } = useOnboarding()

      start()
      // Go to last step
      currentStepIndex.value = totalSteps.value - 1
      next()

      expect(isActive.value).toBe(false)
    })
  })

  describe('goToStep', () => {
    it('should go to specific step', () => {
      const { goToStep, currentStepIndex } = useOnboarding()

      goToStep(5)

      expect(currentStepIndex.value).toBe(5)
    })

    it('should not go to step less than 0 - does nothing', () => {
      // goToStep only updates if index is valid, so -1 is ignored and currentStepIndex stays at 0
      const { goToStep, currentStepIndex } = useOnboarding()

      // Initial value is 0, goToStep(-1) should not change it
      goToStep(-1)

      expect(currentStepIndex.value).toBe(0)
    })

    it('should not go beyond total steps - does nothing', () => {
      // goToStep only updates if index is valid, so out of bounds is ignored
      const { goToStep, currentStepIndex, totalSteps } = useOnboarding()

      goToStep(totalSteps.value + 1)

      // Should remain at initial 0 since invalid index is ignored
      expect(currentStepIndex.value).toBe(0)
    })
  })

  describe('skip and complete', () => {
    it('should skip onboarding', () => {
      const { skip, isActive, hasCompleted } = useOnboarding()

      skip()

      expect(isActive.value).toBe(false)
      expect(hasCompleted.value).toBe(true)
      expect(localStorageMock.setItem).toHaveBeenCalledWith(
        'autocodeforge-onboarding-completed',
        'true',
      )
    })

    it('should complete onboarding', () => {
      const { complete, isActive, hasCompleted } = useOnboarding()

      complete()

      expect(isActive.value).toBe(false)
      expect(hasCompleted.value).toBe(true)
      expect(localStorageMock.setItem).toHaveBeenCalledWith(
        'autocodeforge-onboarding-completed',
        'true',
      )
    })
  })

  describe('reset', () => {
    it('should reset onboarding state', () => {
      const { start, next, skip, reset, isActive, currentStepIndex, hasCompleted } = useOnboarding()

      // Start and advance
      start()
      next()
      skip()

      reset()

      expect(isActive.value).toBe(false)
      expect(currentStepIndex.value).toBe(0)
      expect(hasCompleted.value).toBe(false)
      expect(localStorageMock.removeItem).toHaveBeenCalledWith('autocodeforge-onboarding-completed')
    })
  })

  describe('checkCompletionStatus', () => {
    it('should set hasCompleted to true when storage has completion flag', () => {
      localStorageMock.getItem.mockReturnValue('true')

      const { checkCompletionStatus, hasCompleted } = useOnboarding()

      checkCompletionStatus()

      expect(hasCompleted.value).toBe(true)
    })

    it('should set hasCompleted to false when storage has no completion flag', () => {
      localStorageMock.getItem.mockReturnValue(null)

      const { checkCompletionStatus, hasCompleted } = useOnboarding()

      checkCompletionStatus()

      expect(hasCompleted.value).toBe(false)
    })
  })

  describe('shouldShowOnboarding', () => {
    it('should return false when onboarding is completed', () => {
      localStorageMock.getItem.mockReturnValue('true')

      const { shouldShowOnboarding } = useOnboarding()

      expect(shouldShowOnboarding()).toBe(false)
    })

    it('should return true when onboarding is not completed', () => {
      localStorageMock.getItem.mockReturnValue(null)

      const { shouldShowOnboarding } = useOnboarding()

      expect(shouldShowOnboarding()).toBe(true)
    })
  })

  describe('currentStep', () => {
    it('should return current step based on currentStepIndex', () => {
      const { goToStep, currentStep } = useOnboarding()

      goToStep(2)

      expect(currentStep.value.id).toBe('task-center')
      expect(currentStep.value.title).toBe('AI任务中心')
    })

    it('should return first step when index is 0 (initial state)', () => {
      // Test initial state - currentStepIndex should be 0 at start
      const { currentStep, reset } = useOnboarding()

      reset()
      expect(currentStep.value.id).toBe('welcome')
    })
  })

  describe('计算属性', () => {
    it('should calculate progress correctly', () => {
      const { goToStep, progress, totalSteps } = useOnboarding()

      goToStep(4) // 5th step (0-indexed as 4)

      // Progress = (step + 1) / total * 100
      expect(progress.value).toBeCloseTo(((4 + 1) / totalSteps.value) * 100)
    })

    it('should identify first step correctly', () => {
      const { goToStep, isFirstStep } = useOnboarding()

      goToStep(0)
      expect(isFirstStep.value).toBe(true)

      goToStep(1)
      expect(isFirstStep.value).toBe(false)
    })

    it('should identify last step correctly', () => {
      const { goToStep, isLastStep, totalSteps } = useOnboarding()

      goToStep(totalSteps.value - 1)
      expect(isLastStep.value).toBe(true)

      goToStep(0)
      expect(isLastStep.value).toBe(false)
    })
  })

  describe('steps array', () => {
    it('should have 9 steps', () => {
      const { steps } = useOnboarding()

      expect(steps).toHaveLength(9)
    })

    it('should have correct first step', () => {
      const { steps } = useOnboarding()

      expect(steps[0].id).toBe('welcome')
      expect(steps[0].title).toBe('欢迎使用 AutoCodeForge')
      expect(steps[0].skipable).toBe(true)
    })

    it('should have correct last step', () => {
      const { steps } = useOnboarding()

      expect(steps[steps.length - 1].id).toBe('complete')
      expect(steps[steps.length - 1].skipable).toBe(false)
    })
  })
})
