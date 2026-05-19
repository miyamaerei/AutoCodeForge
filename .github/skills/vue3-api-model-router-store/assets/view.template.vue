<script setup lang="ts">
import { onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { storeToRefs } from 'pinia'
import { use__ModulePascal__Store } from '../store/use__ModulePascal__Store'

const route = useRoute()
const store = use__ModulePascal__Store()
const { items, currentItem, loading, error, hasData } = storeToRefs(store)

onMounted(async () => {
  if (route.name === '__module__.list') {
    await store.fetchList({ page: 1, pageSize: 20 })
  }
  if (route.params.id && typeof route.params.id === 'string') {
    await store.fetchDetail(route.params.id)
  }
})
</script>

<template>
  <section>
    <h1>__ModulePascal__</h1>

    <p v-if="loading">Loading...</p>
    <p v-else-if="error">{{ error }}</p>

    <ul v-else-if="hasData">
      <li v-for="item in items" :key="item.id">
        {{ item.name }}
      </li>
    </ul>

    <p v-else>No data</p>

    <pre v-if="currentItem">{{ currentItem }}</pre>
  </section>
</template>
