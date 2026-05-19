<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { storeToRefs } from 'pinia'
import { use__ModulePascal__Store } from '../store/use__ModulePascal__Store'

const route = useRoute()
const store = use__ModulePascal__Store()
const { loading, error, items, currentItem } = storeToRefs(store)

const hasData = computed(() => {
  if (Array.isArray(items?.value)) {
    return items.value.length > 0
  }
  return Boolean(currentItem.value)
})

onMounted(async () => {
  // Replace with the correct action for this page type.
  if (route.params.id && typeof route.params.id === 'string') {
    await store.fetchDetail(route.params.id)
    return
  }
  await store.fetchList({ page: 1, pageSize: 20 })
})
</script>

<template>
  <main class="page page-__module__-__PageTypePascal__">
    <header>
      <h1>__ModulePascal__ __PageTypePascal__</h1>
    </header>

    <p v-if="loading">Loading...</p>
    <p v-else-if="error">{{ error }}</p>

    <section v-else-if="hasData">
      <slot name="content" />
    </section>

    <p v-else>No data</p>
  </main>
</template>
